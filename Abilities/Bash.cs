using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.NPCs;
using OriMod.Projectiles;
using OriMod.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for pushing the player and enemies in opposite directions. Iconic ability of the Ori franchise.
/// </summary>
public sealed class Bash(Player player) : OriAbility(player) {
  private static float NetAngleTolerance => 0.3f;
  private static float NetAngleLerpValue => 0.2f;

  public override int MaxLevel => 3;

  private ref BashStats Stats => ref IStats<BashStats>.Get(Level);

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

  /// <summary>
  /// The <see cref="NPC"/> or <see cref="Projectile"/> that is being bashed. May be segment of a worm.
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

  /// <summary>
  /// Query if the specified entity is being bashed by this.
  /// </summary>
  /// <param name="entity"></param>
  /// <returns></returns>
  public bool IsBashing(Entity entity) {
    return IsActive && _bashEntity is not null && ReferenceEquals(entity, _bashEntity);
  }

  /// <summary>
  /// Entity that this player is Bashing. Setting this also sets <see cref="_bashTarget"/>.
  /// </summary>
  [MemberNotNull(nameof(_bashEntity), nameof(_bashTarget))]
  private void SetBashEntity(Entity value) {
    if (value is not (NPC or Projectile)) {
      throw new ArgumentException("Value must be of type NPC, Projectile, or null");
    }

    _bashEntity = value;
    UpdateBashTarget();
    System.Diagnostics.Debug.Assert(_bashTarget is not null);
  }

  private void UpdateBashTarget() {
    IBashable? oldBashTarget = _bashTarget;
    IBashable? newBashGlobal = GetBashTarget(_bashEntity);

    if (ReferenceEquals(oldBashTarget, newBashGlobal)) {
      return;
    }

    _bashTarget?.TryClearBashPlayer(Player);
    _bashTarget = newBashGlobal;
    _bashTarget?.SetBashPlayer(OriPlayer);
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
    _bashTarget?.TryClearBashPlayer(Player);
    _bashTarget = null;
  }

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();
    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<AirJump>(to: this);
    parent.AddInterruptible<Climb>(to: this);
    parent.AddInterruptible<Crouch>(to: this);
    parent.AddInterruptible<Dash>(to: this);
    parent.AddInterruptible<Glide>(to: this);
    parent.AddInterruptible<LookUp>(to: this);
    parent.AddInterruptible<WallJump>(to: this);
  }

  /// <summary>
  /// Always syncs <see cref="_netAngle"/>.
  /// At start, will sync start positions, stress
  /// <see cref="Player"/> position and velocity, and
  /// <see cref="_bashEntity"/> and its velocity and position.
  /// </summary>
  /// <param name="sync"></param>
  protected override void NetSync(ISync sync) {
    // No need to send this stuff more than once
    if (ActiveTime == 0) {
      sync.Sync(ref _targetStartPos);
      sync.Sync(ref _playerStartPos);
      sync.Sync7BitEncodedInt(ref _currentStress);
      sync.Sync7BitEncodedInt(ref _lastStress);
      sync.SyncPositionAndVelocity(Player);
      sync.SyncEntity(ref _bashEntity);
      if (_bashEntity is not null) {
        sync.SyncPositionAndVelocity(_bashEntity);
      }

      if (sync.Reading) {
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

    if (sync.Reading && Main.dedServ) {
      // Server needs to know actual value, and doesn't need visual lerping
      _aimAngle = _netAngle;
    }
  }

  protected override bool StartCooldownOnEnter => true;

  protected override void OnEnter(State? fromState) {
    RestoreAirJumps();
    NetUpdate = true;
  }

  protected override void OnExit() {
    Player.pulley = false;
    _endSound.Play(Player);

    Vector2 bashVector = new((float)(0 - Math.Cos(_aimAngle)), (float)(0 - Math.Sin(_aimAngle)));
    Vector2 playerBashVector = -bashVector * Stats.PlayerStrength;
    Vector2 npcBashVector = bashVector * Stats.NpcStrength;

    Player.velocity = playerBashVector;

    if (IsGrounded) {
      Player.position.Y -= 1f * Player.gravDir;
    }

    if (_lastStress < Stats.MaxStress / 1.33) {
      OriPlayer.SetImmune(20);
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

  protected override bool OnPreUpdateInterruptible(State activeState) {
    if (!_hasReleasedBash) {
      return false;
    }

    if (!IsLocal) {
      return false;
    }

    if (Input.Bash.JustPressed) {
      _currentBuffer = 0;
      _lastStress = _currentStress;
      AddStress(40);
    }
    else if (Input.Bash.Current) {
      _currentBuffer++;
    }
    else {
      return false;
    }

    StressDust();
    BashStats stats = Stats;
    if (!Input.Charge.Current && _currentBuffer <= stats.MaxBuffer) {
      if (_currentBuffer == 0) {
        SoundWrapper.PlayLocal(Player, "Ori/Bash/bashNoTargetB", 0.35f);
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

    _bashTarget.SetBashPlayer(OriPlayer);

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

  protected override void OnPreUpdate() {
    if (_bashEntity is not { active: true } ||
        _bashTarget?.BashPlayer is null ||
        _bashTarget.BashPlayer.Player.whoAmI != Player.whoAmI) {
      OriMod.Debug(
        $"[{Main.time}] State cancelled by \"Invalid bash entity {_bashEntity?.ToString() ?? "null"} (active:{_bashEntity?.active.ToString() ?? "null"} | {_bashTarget?.BashPlayer?.ToString() ?? "null"})\"");
      CancelState();
    }

    ref readonly BashStats stats = ref Stats;
    if (ActiveTime == stats.MinTime + 4) {
      SoundWrapper.PlayLocal(Player, "Ori/Bash/seinBashLoopA", 0.5f);
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

  protected override void OnUpdate() {
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
    if (_lastStress < Stats.MaxStress / 2) {
      OriPlayer.SetImmune(2);
    }

    if (IsLocal) {
      // Determine whether to push a netupdate
      if (Math.Abs(_aimAngle - _netAngle) > NetAngleTolerance) {
        _netAngle = _aimAngle;
        NetUpdate = true;
      }
    }
    else {
      // Non-local, smooth visual angle to net angle
      _aimAngle = LerpAngleRad(_aimAngle, _netAngle, NetAngleLerpValue);
    }

    return;

    static float LerpAngleRad(float from, float to, float weight) {
      float num1 = (to - from) % MathF.Tau;
      float num2 = 2f * num1 % MathF.Tau - num1;
      return from + num2 * weight;
    }
  }

  protected override void OnPostUpdate() {
    AddStress(-1);
    if (!Input.Bash.Current) {
      _hasReleasedBash = true;
    }
  }

  protected override void OnEndCooldown() {
    RefreshParticles(Color.LightYellow);
  }

  protected override AnimationOptions? GetAnimationOptions() => new("Bash", loopCount: 1);

  private void AddStress(int value) => _currentStress = Math.Clamp(_currentStress + value, 0, Stats.MaxStress);

  internal void GetDrawFields(AnimSpriteSheet sheet, out Vector2 position, out float rotation, out Rectangle rect) {
    Entity target = HasBashEntity ? _bashEntity : Player;
    position = target.Center;
    rotation = _aimAngle;
    rect = sheet.GetRectFromTimer("Bash", "Arrow", ActiveTime);
  }

  protected override void DebugText(DebugUIState ui) {
    base.DebugText(ui);
    string name = _bashEntity switch {
      null => "null",
      NPC npc => npc.TypeName,
      Projectile proj => proj.Name,
      _ => _bashEntity.GetType().Name
    };
    ui.DrawAppendLabelValue("Target", name);
    ui.DrawAppendLabelValue("Stress", _currentStress);
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
  private readonly record struct BashStats(
    int Damage,
    float Range,
    float PlayerStrength,
    float NpcStrength,
    int MinTime,
    int MaxTime,
    int MaxBuffer,
    int MaxStress) : IStats<BashStats> {
    public static ref BashStats[] Values => ref _values;

    private static BashStats[] _values = [
      default,
      new BashStats(
        Damage: 0, Range: 56,
        PlayerStrength: 15, NpcStrength: 12,
        MinTime: 20, MaxTime: 85,
        MaxBuffer: 25, MaxStress: 240),
      new BashStats(
        Damage: 20, Range: 56,
        PlayerStrength: 15, NpcStrength: 12,
        MinTime: 20, MaxTime: 85,
        MaxBuffer: 25, MaxStress: 240),
      new BashStats(Damage: 45, Range: 90,
        PlayerStrength: 20, NpcStrength: 16,
        MinTime: 15, MaxTime: 105,
        MaxBuffer: 60, MaxStress: 360)
    ];

    public static BashStats CreateFromLevel(int level) => new(
      Damage: 20 + level * 15,
      Range: 60 + level * 10,
      PlayerStrength: 8 + level * 4,
      NpcStrength: 4 + level * 4,
      MinTime: 10 + level * 14 / 255,
      MaxTime: 70 + level * 10,
      MaxBuffer: level * 20,
      MaxStress: level * 120
    );
  }
}
