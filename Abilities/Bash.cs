using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.NPCs;
using OriMod.Projectiles;
using OriMod.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for pushing the player and enemies in opposite directions. Iconic ability of the Ori franchise.
/// </summary>
public sealed class Bash : OriAbility, ILevelable {
  public override int Id => AbilityId.Bash;
  public override int Level => ((ILevelable)this).Level;
  public override bool Unlocked => Level > 0;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 3;

  public override bool CanUse => base.CanUse && Inactive && !Player.mount.Active &&
    !Abilities.Burrow && !Abilities.ChargeDash && !Abilities.ChargeJump && !Abilities.Climb &&
    !Abilities.Launch && !Abilities.Stomp && !Abilities.WallChargeJump;

  public override void OnRefreshed() => Abilities.RefreshParticles(Color.LightYellow);

  private static List<short> CannotBashNpc => _cannotBashNpc ??= Unloadable.New( new List<short> {
    NPCID.BlazingWheel, NPCID.SpikeBall, NPCID.DD2EterniaCrystal, NPCID.DD2LanePortal,
    NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerStardust, NPCID.LunarTowerVortex,
    NPCID.CultistTablet, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsHead, NPCID.EaterofWorldsTail
  }, () => _cannotBashNpc = null);

  private static List<short> CannotBashProj => _cannotBashProj ??= Unloadable.New(new List<short> {
    ProjectileID.FlamethrowerTrap, ProjectileID.FlamesTrap, ProjectileID.GeyserTrap, ProjectileID.SpearTrap,
    ProjectileID.GemHookAmethyst, ProjectileID.GemHookDiamond, ProjectileID.GemHookEmerald,
    ProjectileID.GemHookRuby, ProjectileID.GemHookSapphire, ProjectileID.GemHookTopaz,
    ProjectileID.Hook, ProjectileID.AntiGravityHook, ProjectileID.BatHook, ProjectileID.CandyCaneHook,
    ProjectileID.DualHookBlue, ProjectileID.DualHookRed, ProjectileID.FishHook, ProjectileID.IlluminantHook,
    ProjectileID.LunarHookNebula, ProjectileID.LunarHookSolar, ProjectileID.LunarHookStardust, ProjectileID.LunarHookVortex,
    ProjectileID.SlimeHook, ProjectileID.StaticHook, ProjectileID.TendonHook, ProjectileID.ThornHook, ProjectileID.TrackHook,
    ProjectileID.WoodHook, ProjectileID.WormHook,
  }, () => _cannotBashNpc = null);

  private static List<short> _cannotBashNpc;
  private static List<short> _cannotBashProj;

  private float BashPlayerStrength {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 15;
        default: return 8 + Level * 4;
      }
    }
  }

  private float BashNpcStrength {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 12;
        default: return 4 + Level * 4;
      }
    }
  }

  private int MinBashDuration {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 20;
        case 3: return 15;
        default: return 10 + Level * 14 / 255;
      }
    }
  }
  

  private int MaxBashDuration {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 85;
        case 3: return 105;
        default: return 70 + Level * 10;
      }
    }
  }

  private int MaxBufferDuration {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 25;
        case 3: return 60;
        default: return Level * 20;
      }
    }
  }
  private int _bufferDuration;

  private int MaxStress {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 240;
        case 3: return 360;
        default: return Level * 120;
      }
    }
  }
  private int _currentStress;
  private int CurrentStress {
    get => _currentStress;
    set => _currentStress = Math.Clamp(value,0,MaxStress);
  }
  private int _lastStress;
  private int _stressParticleTimer;

  private int BashDamage {
    get {
      switch (Level) {
        case 0:
        case 1: return 0;
        case 2: return 20;
        case 3: return 45;
        default: return 20 + Level * 15;
      }
    }
  }

  private float BashRange {
    get {
      switch (Level) {
        case 0: return 0;
        case 1:
        case 2: return 56;
        case 3: return 90;
        default: return 60 + Level * 10;
      }
    }
  }

  private Vector2 _playerStartPos;
  private Vector2 _targetStartPos;
  public float BashAngle { get; private set; }

  /// <summary>
  /// <see cref="OriNpc"/> or <see cref="OriProjectile"/> that this player is Bashing.
  /// </summary>
  public IBashable BashTarget { get; private set; }

  /// <summary>
  /// Entity that this player is Bashing.
  /// </summary>
  public Entity BashEntity { get; private set; }

    private readonly RandomChar _rand = new();

  private bool _sBashed;

  public override void ReadPacket(BinaryReader r) {
    if (!InUse) return;
    if (Starting) {
      _targetStartPos = r.ReadVector2();
      _playerStartPos = r.ReadVector2();
    }
    bool isNpc = r.ReadBoolean();
    ushort id = r.ReadUInt16();
    SetTarget(isNpc, id);
    BashAngle = r.ReadSingle();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
    CurrentStress = r.ReadInt32();
    _lastStress = r.ReadInt32();
    _bufferDuration = r.ReadInt32();
  }

  public override void WritePacket(ModPacket packet) {
    if (!InUse) return;
    if (Starting) {
      packet.WriteVector2(_targetStartPos);
      packet.WriteVector2(_playerStartPos);
    }
    packet.Write(BashEntity is NPC);
    packet.Write((ushort)(BashEntity?.whoAmI ?? ushort.MaxValue));
    packet.Write(BashAngle);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
    packet.Write(CurrentStress);
    packet.Write(_lastStress);
    packet.Write(_bufferDuration);
  }

  /// <summary>
  /// Filter to determine if this <see cref="NPC"/> can be bashed. Returns <see langword="true"/> if the NPC should be bashed.
  /// <para>Excludes friendly NPCs, bosses, specific NPCs, and NPCs that are already being Bashed.</para>
  /// </summary>
  /// <param name="npc"><see cref="NPC"/> to check.</param>
  /// <returns><see langword="true"/> if the NPC should be bashed, otherwise <see langword="false"/>.</returns>
  private static bool BashNpcFilter(NPC npc) =>
    !npc.friendly && !npc.boss && npc.aiStyle != 37 && !CannotBashNpc.Contains((short)npc.type) && !npc.GetGlobalNPC<OriNpc>().IsBashed;

  /// <summary>
  /// Filter to determine if this <see cref="Projectile"/> can be bashed. Returns true if the projectile should be bashed.
  /// <para>Excludes non-hostile, 0 damage projectiles, minions, sentries, traps, grapples, and projectiles that are already being Bashed.</para>
  /// </summary>
  /// <param name="proj">Projectile to check for bashing.</param>
  /// <returns><see langword="true"/> if the projectile should be bashed, otherwise <see langword="false"/>.</returns>
  private static bool BashProjFilter(Projectile proj) =>
    proj.hostile && proj.damage != 0 && !proj.minion && !proj.sentry && !proj.trap && !CannotBashProj.Contains((short)proj.type) && !proj.GetGlobalProjectile<OriProjectile>().IsBashed;

  private void SetTarget(bool isNpc, ushort id) {
    if (id == ushort.MaxValue) {
      SetTarget(null);
    }
    else if (isNpc) {
      SetTarget(Main.npc[id]);
    }
    else {
      SetTarget(Main.projectile[id]);
    }
  }

  /// <summary>
  /// Sets the target to be bashed to <paramref name="entity"/>. Pass in <see langword="null"/> to not bash anything.
  /// </summary>
  /// <param name="entity"><see cref="Entity"/> to target for bashing.</param>
  private void SetTarget(Entity entity) {
    BashEntity = entity;
    BashTarget = entity switch {
      NPC npc => npc.GetGlobalNPC<OriNpc>(),
      Projectile projectile => projectile.GetGlobalProjectile<OriProjectile>(),
      _ => null
    };
  }

  /// <summary>
  /// Attempt to start Bash. This will search for an <see cref="NPC"/> or <see cref="Projectile"/> to bash, and set it as target.
  /// </summary>
  /// <returns><see langword="true"/> if an <see cref="Entity"/> to bash was found and set as target, otherwise <see langword="false"/>.</returns>
  private bool Start() {
    SetTarget(null);
    float currDist = BashRange;

    // Check for Bashing NPCs
    bool isBashingNpc = Player.GetClosestEntity(Main.npc, ref currDist, out NPC npc, condition: BashNpcFilter);
    if (isBashingNpc) {
      if (npc.aiStyle == 6) {
        // Worm: Must bash head of worm-like rather than body (head is stored as ai[3])
        // Otherwise only part of the npc will be suspended
        npc = Main.npc[(int)npc.ai[3]];
      }
      SetTarget(npc);
    }
    else {
      // Bash Lv2 or higher required for projectiles
      if (Level < 2) {
        return false;
      }

      bool isBashingProj = Player.GetClosestEntity(Main.projectile, ref currDist, out Projectile proj, condition: BashProjFilter);
      if (!isBashingProj) {
        return false;
      }

      SetTarget(proj);
    }

    BashTarget.IsBashed = true;
    BashTarget.BashPosition = BashEntity.Center;
    BashTarget.BashPlayer = oPlayer;

    _playerStartPos = Player.Center;
    _targetStartPos = BashEntity.Center;
    PlayLocalSound("Ori/Bash/seinBashStartA", 0.5f);
    return true;
  }

  private void End() {
    Player.pulley = false;
    PlaySound("Ori/Bash/seinBashEnd" + _rand.NextNoRepeat(3), 0.5f);

    bool isNpc = BashEntity is NPC;
    NPC npc = (NPC)(isNpc ? BashEntity : null);

    Vector2 bashVector = new((float)(0 - Math.Cos(BashAngle)), (float)(0 - Math.Sin(BashAngle)));
    Vector2 playerBashVector = -bashVector * BashPlayerStrength;
    Vector2 npcBashVector = bashVector * BashNpcStrength;
    Player.velocity = playerBashVector;
    Player.position += playerBashVector * 3;
    if (!isNpc || !npc.immortal) BashEntity.velocity = npcBashVector; // Don't knockback target dummies
    Player.position += npcBashVector * 5;
    if (IsGrounded) {
      Player.position.Y -= 1f;
    }

    if (_lastStress < MaxStress/1.33) oPlayer.ImmuneTimer = 20;

    BashTarget.IsBashed = false;
    if (IsLocal && Level >= 2 && isNpc) {
      Player.ApplyDamageToNPC(npc, BashDamage, 0, 1);
    }

    StartCooldown();
  }
  public override void UpdateActive() {
    if (IsLocal) NetUpdate = true;
  }

  public override void UpdateUsing() {
    if (!Ending) {
      if (BashEntity is not null) {
        BashEntity.Center = _targetStartPos;
      }

      Player.velocity = Vector2.Zero;
      Player.gravity = 0;
    }
    if (IsLocal) {
      NetUpdate = true;
      switch (BashEntity) {
        case NPC npc:
          npc.netUpdate2 = true;
          break;
        case Projectile projectile:
          projectile.netUpdate2 = true;
          break;
      }

      if (BashEntity != null)
      {
        BashAngle = OriMod.ConfigClient.bashMode == "Target" ? 
          BashEntity.AngleTo(Main.MouseWorld) : Player.AngleTo(Main.MouseWorld);
      }
    }
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
    Player.buffImmune[BuffID.CursedInferno] = true;
    Player.buffImmune[BuffID.Dazed] = true;
    Player.buffImmune[BuffID.Frozen] = true;
    Player.buffImmune[BuffID.Frostburn] = true;
    Player.buffImmune[BuffID.MoonLeech] = true;
    Player.buffImmune[BuffID.Obstructed] = true;
    Player.buffImmune[BuffID.OnFire] = true;
    Player.buffImmune[BuffID.Poisoned] = true;
    Player.buffImmune[BuffID.ShadowFlame] = true;
    Player.buffImmune[BuffID.Silenced] = true;
    Player.buffImmune[BuffID.Slow] = true;
    Player.buffImmune[BuffID.Stoned] = true;
    Player.buffImmune[BuffID.Suffocation] = true;
    Player.buffImmune[BuffID.Venom] = true;
    Player.buffImmune[BuffID.Weak] = true;
    Player.buffImmune[BuffID.WitheredArmor] = true;
    Player.buffImmune[BuffID.WitheredWeapon] = true;
    Player.buffImmune[BuffID.WindPushed] = true;
    if (_lastStress < MaxStress/2) oPlayer.ImmuneTimer = 2;
  }

  public override void PreUpdate() {
    if (Input.Bash.Current) _bufferDuration++;
    if (Input.Bash.JustPressed) {
      _bufferDuration = 0;
      _lastStress = CurrentStress;
      CurrentStress += 40;
    }

    _stressParticleTimer++;
    if (_stressParticleTimer > 8-(CurrentStress/MaxStress*5)) {
      _stressParticleTimer = 0;
      for (int i = 0; i < CurrentStress/(MaxStress/4); i++) {
        Dust.NewDust(Player.Center, 12, 12, ModContent.DustType<AbilityRefreshedDust>(), newColor: Color.LightYellow);
      }
    }

    if (CanUse && Input.Bash.Current && !Input.Charge.Current && _bufferDuration <= MaxBufferDuration) {
      if(_bufferDuration == 0)
        PlayLocalSound("Ori/Bash/bashNoTargetB", 0.35f);
      CurrentStress += 3; 
      bool didBash = Start();
      if (didBash) {
        SetState(AbilityState.Starting);
        RestoreAirJumps();
      }
      else if (_bufferDuration == MaxBufferDuration) {
        Abilities.RefreshParticles(Color.LightYellow);
      }
    }
    else if (InUse) {
      if (Starting) {
        if (StateTime > MinBashDuration) {
          SetState(AbilityState.Active, true);
        }
        return;
      }

      if (!Active) return;
      if (StateTime == MinBashDuration + 4) {
        PlayLocalSound("Ori/Bash/seinBashLoopA", 0.5f);
      }
      oPlayer.Animations?.Update();
      CurrentStress += 1;

      if (!IsLocal) _sBashed = true;
      if ((StateTime <= MaxBashDuration && Input.Bash.Current &&
          BashEntity is not null && BashEntity.active) || !IsLocal) return;
      End();
      SetState(AbilityState.Inactive);
    } else {
      CurrentStress -= 1;
      if (_sBashed && !IsLocal) { 
        End();
        _sBashed = false;
      }
    }
  }
}
