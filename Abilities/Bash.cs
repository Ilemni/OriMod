using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.NPCs;
using OriMod.Projectiles;
using OriMod.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using OriMod.Networking;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for pushing the player and enemies in opposite directions. Iconic ability of the Ori franchise.
/// </summary>
public sealed class Bash : OriAbility {
  private static float NetAngleTolerance => 0.3f;
  private static float NetAngleLerpValue => 0.2f;

  public override int MaxLevel => 3;

  public ref readonly BashStats Stats => ref IStats<BashStats>.Get(Level);

  private int _currentBuffer;

  private int _currentStress;
  private int _lastStress;
  private int _stressParticleTimer;

  private float _aimAngle;
  private float _netAngle;

  private SoundInfo _startSound = new("Ori/Bash/seinBashStart", 1, 0.5f);
  private SoundInfo _endSound = new("Ori/Bash/seinBashEnd", 3, 0.5f);

  /// Set false at state activation, true whenever input released, prevents repeated activation from holding down input
  private bool _hasReleasedBash;

  private bool _immuneAfterExit;

  /// <summary>
  /// The <see cref="NPC"/> or <see cref="Projectile"/> that is being bashed. This may be a segment of a worm.
  /// </summary>
  private Entity? _bashEntity;

  /// <summary>
  /// The <see cref="OriNpc"/> or <see cref="OriProjectile"/> global that is being bashed.
  /// If <see cref="_bashEntity"/> is a worm, this will be the worm's head.
  /// </summary>
  private IBashable? _bashTarget;

  private Vector2 _playerStartPos;
  private Vector2 _targetStartPos;

  [MemberNotNullWhen(true, nameof(_bashEntity), nameof(_bashTarget))]
  private bool HasBashEntity => _bashEntity is { active: true };

  public override bool ShowHintInUI => Main.hardMode;

  /// <summary>
  /// Query if the specified entity is being bashed by this.
  /// </summary>
  /// <param name="entity"></param>
  /// <returns></returns>
  public bool IsBashing(Entity entity) {
    return Active && _bashEntity is not null && ReferenceEquals(entity, _bashEntity);
  }

  /// <summary>
  /// Entity that this player is Bashing. Setting this also sets <see cref="_bashTarget"/>.
  /// </summary>
  [MemberNotNull(nameof(_bashEntity), nameof(_bashTarget))]
  private void SetBashEntity(Entity value) {
    if (value is not (NPC or Projectile)) {
      throw new ArgumentException("Value must be of type NPC or Projectile");
    }

    _bashEntity = value;
    UpdateBashTarget();
    System.Diagnostics.Debug.Assert(_bashTarget is not null);
  }

  private void UpdateBashTarget() {
    IBashable? oldBashTarget = _bashTarget;
    IBashable? newBashTarget = GetBashTarget(_bashEntity);

    if (ReferenceEquals(oldBashTarget, newBashTarget)) {
      return;
    }

    _bashTarget?.TryClearBash(this);
    _bashTarget = newBashTarget;
    _bashTarget?.SetBash(this);
  }

  private static IBashable? GetBashTarget(Entity? entity) {
    return entity switch {
      NPC npc => npc.HeadOrSelf().GetGlobalNPC<OriNpc>(),
      Projectile proj => proj.GetGlobalProjectile<OriProjectile>(),
      _ => null
    };
  }

  private void ClearBashEntity() {
    _bashEntity = null;
    _bashTarget?.TryClearBash(this);
    _bashTarget = null;
  }

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility>(),
      GetState<AirJump>(),
      GetState<Climb>(),
      GetState<Crouch>(),
      GetState<Dash>(),
      GetState<Glide>(),
      GetState<LookUp>(),
      GetState<WallJump>()
    ]);
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) {
    if (!Active) {
      return _immuneAfterExit && InactiveTime <= 20;
    }

    if (_lastStress >= Stats.MaxStress / 2) {
      return false;
    }

    return IsLocal && damageSource.TryGetCausingEntity(out Entity? entity) && IsBashing(entity);
  }

  /// <summary>
  /// Always syncs <see cref="_netAngle"/>.
  /// At start, will sync start positions, stress
  /// <see cref="Player"/> position and velocity, and
  /// <see cref="_bashEntity"/> and its velocity and position.
  /// </summary>
  /// <param name="sync"></param>
  protected override void NetSync(NetSyncer sync) {
    Vector2 oldEntityPosition = default;
    Vector2 oldEntityVelocity = default;

    // No need to send this stuff more than once
    if (ActiveTime == 0) {
      sync.Sync(ref _targetStartPos);
      sync.Sync(ref _playerStartPos);
      sync.Sync7BitEncodedInt(ref _currentStress);
      sync.Sync7BitEncodedInt(ref _lastStress);
      sync.SyncPositionAndVelocity(Player);
      sync.SyncEntity(ref _bashEntity);

      if (_bashEntity is not null) {
        oldEntityPosition = _bashEntity.position;
        oldEntityVelocity = _bashEntity.velocity;
        sync.SyncPositionAndVelocity(_bashEntity);
      }

      if (sync.Reading && !Main.dedServ) {
        // dedServ requires validating, later in method
        UpdateBashTarget();
      }
    }

    // _netAngle and _bashAngle are separate values to allow visual lerping when syncing from a MP client
    // This avoids a jittery, snappy look when other clients modify the angle (i.e. move their mouse)
    // We use a deadzone for syncing to avoid potentially spamming packets every tick

    if (sync.Writing) {
      // Send the actual bash angle value
      _netAngle = _aimAngle;
    }

    sync.Sync(ref _netAngle);

    // Done syncing data
    if (sync.Reading && Main.dedServ) {
      if (!Main.dedServ) {
        UpdateBashTarget();
        return;
      }

      // Server needs to know actual value, and doesn't need visual lerping
      _aimAngle = _netAngle;

      // Server needs to make sure bash target is valid for bashing.
      // It's possible to be invalid if 2 or more players attempt to bash the same target at the same time.
      // If entity is somehow null, bash is invalid there too.
      // This check must be at end of sync to ensure all data is read
      IBashable? target = GetBashTarget(_bashEntity);
      if (target is not (null or { IsBashed: true, Bash.Active: true })) {
        UpdateBashTarget();
        return;
      }

      // The player that sent the invalid bash needs its bash rejected.
      ModNetHandler.BashRejection.SendPacket(Player.whoAmI);

      // Restore pos/vel values that were set in packet.
      if (_bashEntity is not null) {
        _bashEntity.position = oldEntityPosition;
        _bashEntity.velocity = oldEntityVelocity;
      }
    }
  }

  protected override bool StartCooldownOnEnter => true;

  protected override void OnEnter(State? fromState) {
    _immuneAfterExit = false;
    RestoreAirJumps();
    NetUpdate = true;
  }

  protected override void OnExit(State? toState) {
    Player.pulley = false;
    _endSound.Play(Player);

    Vector2 bashVector = _aimAngle.ToRotationVector2();
    Vector2 playerBashVector = bashVector * Stats.PlayerStrength;
    Vector2 npcBashVector = -bashVector * Stats.NpcStrength;

    Player.velocity = playerBashVector;

    if (IsGrounded) {
      Player.position.Y -= 1f * Player.gravDir;
    }

    if (_lastStress < Stats.MaxStress / 1.33f) {
      _immuneAfterExit = true;
    }

    if (_bashEntity is NPC { active: true } npc) {
      if (!npc.immortal) {
        // Don't knockback target dummies
        npc.velocity = npcBashVector * npc.knockBackResist;
      }

      if (IsLocal && Level >= 2) {
        Player.ApplyDamageToNPC(npc, Stats.Damage, 0, 1);
      }
    }

    ClearBashEntity();
  }

  protected override bool UpdateInterrupt(State activeState) {
    if (!_hasReleasedBash) {
      return false;
    }

    if (!IsLocal) {
      return false;
    }

    if (Input.Bash.JustPressed && !Input.Charge.Current) {
      _currentBuffer = 0;
      _lastStress = _currentStress;
      AddStress(40);
    }
    else if (Input.Bash.Current) {
      _currentBuffer++;
    }
    else {
      _currentBuffer = 0;
      return false;
    }

    StressDust();
    BashStats stats = Stats;
    if (!Input.Charge.Current && _currentBuffer <= stats.MaxBuffer) {
      if (_currentBuffer == 0) {
        SoundWrapper.PlayLocal(Player, "OriMod/Sounds/Ori/Bash/bashNoTargetB", 0.35f);
      }

      AddStress(3);
      if (TryStart()) {
        _hasReleasedBash = false;
        return true;
      }

      if (_currentBuffer == stats.MaxBuffer) {
        RefreshParticles(Color.LightYellow);
      }

      return false;
    }

    return false;
  }

  /// <summary>
  /// Attempt to start Bash. This will search for an <see cref="NPC"/> or <see cref="Projectile"/> to bash, and set it as target.
  /// </summary>
  /// <returns><see langword="true"/> if an <see cref="Entity"/> to bash was found and set as target, otherwise <see langword="false"/>.</returns>
  private bool TryStart() {
    // Check for Bashing NPCs
    float range = Stats.Range;
    if (Player.GetClosesEntity(Main.ActiveNPCs, ref range, out NPC? npc, condition: BashNpcFilter)) {
      if (npc.aiStyle == NPCAIStyleID.Worm) {
        // ReSharper disable once GrammarMistakeInComment (Terraria AI field is lowercase)
        // Worm: Must bash head of worm-like rather than body (head is stored as ai[3])
        // Otherwise only part of the npc will be suspended
        npc = Main.npc[(int)npc.ai[3]];
      }

      SetBashEntity(npc);
    }
    else {
      // Bash Lv2 or higher required for projectiles
      if (Level < 2) {
        return false;
      }

      if (!Player.GetClosesEntity(Main.ActiveProjectiles, ref range, out Projectile? proj, condition: BashProjFilter)) {
        return false;
      }

      SetBashEntity(proj);
    }

    _bashTarget.SetBash(this);

    _playerStartPos = Player.position;
    _targetStartPos = _bashEntity.position;
    _startSound.PlayLocal(Player);
    return true;

    // TryGet, since explicitly immune Npcs/Projs will not have the bash GlobalNpc/Proj created for them
    static bool BashNpcFilter(NPC npc) =>
      npc.HeadOrSelf().TryGetGlobalNPC(out OriNpc oNpc) && ((IBashable)oNpc).CanBeBashed();

    static bool BashProjFilter(Projectile proj) =>
      proj.TryGetGlobalProjectile(out OriProjectile oProj) && ((IBashable)oProj).CanBeBashed();
  }

  private void StressDust() {
    BashStats stats = Stats;
    if (stats.MaxStress == 0) {
      return;
    }

    _stressParticleTimer++;
    if (_stressParticleTimer <= 8 - _currentStress / stats.MaxStress * 5) {
      return;
    }

    _stressParticleTimer = 0;
    int loopCount = _currentStress / (stats.MaxStress / 4);
    int dustType = ModContent.DustType<AbilityRefreshedDust>();
    Vector2 playerCenter = Player.Center;
    for (int i = 0; i < loopCount; i++) {
      Dust.NewDust(playerCenter, 12, 12, dustType, newColor: Color.LightYellow);
    }
  }

  public override void SetControls() {
    // Allow only quick heal and quick mana
    Player.controlJump = false;
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlHook = false;
    Player.controlInv = false;
    Player.controlMount = false;
    Player.controlSmart = false;
    Player.controlThrow = false;
    Player.controlTorch = false;
    Player.controlUseItem = false;
    Player.controlUseTile = false;
  }

  public override void PostUpdateMiscEffects() {
    if (_bashEntity is not { active: true } ||
        _bashTarget?.Bash is null ||
        _bashTarget.Bash.Player.whoAmI != Player.whoAmI) {
      CancelState();
    }

    ref readonly BashStats stats = ref Stats;
    if (ActiveTime == stats.MinTime + 4) {
      SoundWrapper.PlayLocal(Player, "OriMod/Sounds/Ori/Bash/seinBashLoopA", 0.5f);
    }

    AddStress(1);

    if (!IsLocal) {
      return;
    }

    if (ActiveTime > stats.MaxTime ||
        ActiveTime > stats.MinTime && !Input.Bash.Current ||
        !HasBashEntity) {
      CancelState();
    }
  }

  public override void PostUpdateRunSpeeds() {
    if (HasBashEntity) {
      _bashEntity.position = _targetStartPos;
      if (IsLocal) {
        Entity target = OriMod.ConfigClient.bashMode == "Target" ? _bashEntity : Player;
        _aimAngle = target.AngleTo(Main.MouseWorld);
      }
    }

    Player.position = _playerStartPos;
    Player.velocity = Vector2.Zero;
    Player.gravity = 0;
    Player.buffImmune.AssignValueToKeys(true, [
      BuffID.CursedInferno,
      BuffID.Dazed,
      BuffID.Frozen,
      BuffID.Frostburn,
      BuffID.MoonLeech,
      BuffID.Obstructed,
      BuffID.OnFire,
      BuffID.Poisoned,
      BuffID.ShadowFlame,
      BuffID.Silenced,
      BuffID.Slow,
      BuffID.Stoned,
      BuffID.Suffocation,
      BuffID.Venom,
      BuffID.Weak,
      BuffID.WitheredArmor,
      BuffID.WitheredWeapon,
      BuffID.WindPushed
    ]);

    if (IsLocal) {
      // Determine whether to push a netupdate
      if (Math.Abs(_aimAngle - _netAngle) > NetAngleTolerance) {
        _netAngle = _aimAngle;
        NetUpdate = true;
      }
    }
    else {
      // Non-local, smooth visual angle to net angle
      _aimAngle = _aimAngle.AngleLerp(_netAngle, NetAngleLerpValue);
    }
  }

  public override void PostUpdate() {
    if (!Active) {
      AddStress(-1);
    }

    if (!Input.Bash.Current) {
      _hasReleasedBash = true;
    }
  }

  protected override void OnEndCooldown(bool wasOnCooldown) {
    if (wasOnCooldown) {
      RefreshParticles(Color.LightYellow);
    }
  }

  public override AnimationOptions? GetAnimationOptions() => new("Bash") { LoopCount = 1 };

  private void AddStress(int value) => _currentStress = Math.Clamp(_currentStress + value, 0, Stats.MaxStress);

  internal float GetRotation(ref readonly PlayerDrawSet drawInfo) {
    return !Character.UiInfo.IsDrawingInUI
      ? _aimAngle
      : (drawInfo.Position - Main.screenPosition).AngleTo(Main.MouseScreen);
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelValue("Range", Stats.Range / 16f, format: "F1");
    ui.DrawAppendLabelValue("Damage", Stats.Damage);
    string name = _bashEntity switch {
      null => "null",
      NPC npc => npc.TypeName,
      Projectile proj => proj.Name,
      _ => _bashEntity.GetType().Name
    };
    ui.DrawAppendLabelValue("Target", name);
    ui.DrawAppendLabelProgressBar("Buffer", _currentBuffer, Stats.MaxBuffer);
    ui.DrawAppendLabelProgressBar("Stress", _currentStress, Stats.MaxStress);
    ui.DrawAppendLabelProgressBar("Duration", ActiveTime, [Stats.MinTime, Stats.MaxTime]);
  }

  /// <summary>
  /// Stats which determine Bash effects.
  /// </summary>
  /// <param name="Damage">Damage that Bash will apply when applying knockback</param>
  /// <param name="Range">Max distance a NPC may be from the player to be a candidate for bashing.</param>
  /// <param name="PlayerStrength">Strength of knockback applied to player</param>
  /// <param name="NpcStrength">Strength of knockback applied to NPC</param>
  /// <param name="MinTime">Min time which bash must be in state.</param>
  /// <param name="MaxTime">Max time which bash may be in state.</param>
  /// <param name="MaxBuffer">Max time which Bash may attempt to be entered</param>
  /// <param name="MaxStress"></param>
  public readonly record struct BashStats(
    int Damage,
    float Range,
    float PlayerStrength,
    float NpcStrength,
    int MinTime,
    int MaxTime,
    int MaxBuffer,
    int MaxStress) : IStats<BashStats> {
    public static BashStats[] Values { get; } = [
      new(
        Damage: 0, Range: 56,
        PlayerStrength: 15, NpcStrength: 12,
        MinTime: 20, MaxTime: 85,
        MaxBuffer: 25, MaxStress: 240),
      new(
        Damage: 20, Range: 56,
        PlayerStrength: 15, NpcStrength: 12,
        MinTime: 20, MaxTime: 85,
        MaxBuffer: 25, MaxStress: 240),
      new(Damage: 45, Range: 90,
        PlayerStrength: 20, NpcStrength: 16,
        MinTime: 15, MaxTime: 105,
        MaxBuffer: 60, MaxStress: 360)
    ];
  }
}
