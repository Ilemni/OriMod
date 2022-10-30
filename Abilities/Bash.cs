using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.NPCs;
using OriMod.Projectiles;
using OriMod.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for pushing the player and enemies in opposite directions. Iconic ability of the Ori franchise.
  /// </summary>
  public sealed class Bash : Ability<OriAbilityManager>, ILevelable {
    static Bash() => OriMod.OnUnload += Unload;
    public override int Id => AbilityId.Bash;
    public override int Level => (this as ILevelable).Level;
    public override bool Unlocked => Level > 0;
    int ILevelable.Level { get; set; }
    int ILevelable.MaxLevel => 3;

    public override bool CanUse => base.CanUse && Inactive && !player.mount.Active &&
      !abilities.burrow && !abilities.chargeDash && !abilities.chargeJump && !abilities.climb && !abilities.dash &&
      !abilities.launch && !abilities.stomp && !abilities.wallChargeJump;

    public override void OnRefreshed() => abilities.RefreshParticles(Color.LightYellow);

    private static List<short> CannotBashNpc => _cannotBashNpc ?? (_cannotBashNpc = new List<short> {
      NPCID.BlazingWheel, NPCID.SpikeBall, NPCID.DD2EterniaCrystal, NPCID.DD2LanePortal
    });

    private static List<short> CannotBashProj => _cannotBashProj ?? (_cannotBashProj = new List<short> {
      ProjectileID.FlamethrowerTrap, ProjectileID.FlamesTrap, ProjectileID.GeyserTrap, ProjectileID.SpearTrap,
      ProjectileID.GemHookAmethyst, ProjectileID.GemHookDiamond, ProjectileID.GemHookEmerald,
      ProjectileID.GemHookRuby, ProjectileID.GemHookSapphire, ProjectileID.GemHookTopaz,
      ProjectileID.Hook, ProjectileID.AntiGravityHook, ProjectileID.BatHook, ProjectileID.CandyCaneHook,
      ProjectileID.DualHookBlue, ProjectileID.DualHookRed, ProjectileID.FishHook, ProjectileID.IlluminantHook,
      ProjectileID.LunarHookNebula, ProjectileID.LunarHookSolar, ProjectileID.LunarHookStardust, ProjectileID.LunarHookVortex,
      ProjectileID.SlimeHook, ProjectileID.StaticHook, ProjectileID.TendonHook, ProjectileID.ThornHook, ProjectileID.TrackHook,
      ProjectileID.WoodHook, ProjectileID.WormHook,
    });

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
          case 2: return 30;
          case 3: return 24;
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

    private readonly RandomChar _rand = new RandomChar();

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
    }

    /// <summary>
    /// Filter to determine if this <see cref="NPC"/> can be bashed. Returns <see langword="true"/> if the NPC should be bashed.
    /// <para>Excludes friendly NPCs, bosses, specific NPCs, and NPCs that are already being Bashed.</para>
    /// </summary>
    /// <param name="npc"><see cref="NPC"/> to check.</param>
    /// <returns><see langword="true"/> if the NPC should be bashed, otherwise <see langword="false"/>.</returns>
    private bool BashNpcFilter(NPC npc) =>
      !npc.friendly && !npc.boss && npc.aiStyle != 37 && !CannotBashNpc.Contains((short)npc.type) && !npc.GetGlobalNPC<OriNpc>().IsBashed;

    /// <summary>
    /// Filter to determine if this <see cref="Projectile"/> can be bashed. Returns true if the projectile should be bashed.
    /// <para>Excludes friendly if disallowed, 0 damage projectiles, minions, sentries, traps, grapples, and projectiles that are already being Bashed.</para>
    /// </summary>
    /// <param name="proj">Projectile to check for bashing.</param>
    /// <returns><see langword="true"/> if the projectile should be bashed, otherwise <see langword="false"/>.</returns>
    private bool BashProjFilter(Projectile proj) =>
      !proj.friendly && proj.damage != 0 && !proj.minion && !proj.sentry && !proj.trap && !CannotBashProj.Contains((short)proj.type) && !proj.GetGlobalProjectile<OriProjectile>().IsBashed;

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
      BashTarget =
        entity is NPC npc ? npc.GetGlobalNPC<OriNpc>() as IBashable :
        entity is Projectile projectile ? projectile.GetGlobalProjectile<OriProjectile>() : null;
    }

    /// <summary>
    /// Attempt to start Bash. This will search for an <see cref="NPC"/> or <see cref="Projectile"/> to bash, and set it as target.
    /// </summary>
    /// <returns><see langword="true"/> if an <see cref="Entity"/> to bash was found and set as target, otherwise <see langword="false"/>.</returns>
    private bool Start() {
      SetTarget(null);
      float currDist = BashRange;

      // Check for Bashing NPCs
      bool isBashingNpc = player.GetClosestEntity(Main.npc, ref currDist, out NPC npc, condition: BashNpcFilter);
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

        bool isBashingProj = player.GetClosestEntity(Main.projectile, ref currDist, out Projectile proj, condition: BashProjFilter);
        if (!isBashingProj) {
          return false;
        }

        SetTarget(proj);
      }

      BashTarget.IsBashed = true;
      BashTarget.BashPosition = BashEntity.Center;
      BashTarget.BashPlayer = abilities.oPlayer;

      _playerStartPos = player.Center;
      _targetStartPos = BashEntity.Center;
      abilities.oPlayer.PlayLocalSound("Ori/Bash/seinBashStartA", 0.5f);
      return true;
    }

    private void End() {
      player.pulley = false;
      abilities.oPlayer.PlaySound("Ori/Bash/seinBashEnd" + _rand.NextNoRepeat(3), 0.5f);
      abilities.oPlayer.UnrestrictedMovement = true;

      Vector2 bashVector = new Vector2((float)(0 - Math.Cos(BashAngle)), (float)(0 - Math.Sin(BashAngle)));
      Vector2 playerBashVector = -bashVector * BashPlayerStrength;
      Vector2 npcBashVector = bashVector * BashNpcStrength;
      player.velocity = playerBashVector;
      player.position += playerBashVector * 3;
      BashEntity.velocity = npcBashVector;
      player.position += npcBashVector * 5;
      if (abilities.oPlayer.IsGrounded) {
        player.position.Y -= 1f;
      }

      abilities.oPlayer.immuneTimer = 5;

      BashTarget.IsBashed = false;
      if (IsLocal && Level >= 2 && BashEntity is NPC npc) {
        player.ApplyDamageToNPC(npc, BashDamage, 0, 1, false);
      }

      StartCooldown();
    }

    public override void UpdateUsing() {
      if (!Ending) {
        if (BashEntity is not null) {
          BashEntity.Center = _targetStartPos;
        }

        player.velocity = Vector2.Zero;
        player.gravity = 0;
      }
      if (IsLocal) {
        netUpdate = true;
        switch (BashEntity) {
          case NPC npc:
            npc.netUpdate2 = true;
            break;
          case Projectile projectile:
            projectile.netUpdate2 = true;
            break;
        }

        if (BashEntity != null) BashAngle = BashEntity.AngleTo(Main.MouseWorld);
      }
      // Allow only quick heal and quick mana
      player.controlJump = false;
      player.controlUp = false;
      player.controlDown = false;
      player.controlLeft = false;
      player.controlRight = false;
      player.controlHook = false;
      player.controlInv = false;
      player.controlMount = false;
      player.controlSmart = false;
      player.controlThrow = false;
      player.controlTorch = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
      abilities.oPlayer.immuneTimer = 2;
      player.buffImmune[BuffID.CursedInferno] = true;
      player.buffImmune[BuffID.Dazed] = true;
      player.buffImmune[BuffID.Frozen] = true;
      player.buffImmune[BuffID.Frostburn] = true;
      player.buffImmune[BuffID.MoonLeech] = true;
      player.buffImmune[BuffID.Obstructed] = true;
      player.buffImmune[BuffID.OnFire] = true;
      player.buffImmune[BuffID.Poisoned] = true;
      player.buffImmune[BuffID.ShadowFlame] = true;
      player.buffImmune[BuffID.Silenced] = true;
      player.buffImmune[BuffID.Slow] = true;
      player.buffImmune[BuffID.Stoned] = true;
      player.buffImmune[BuffID.Suffocation] = true;
      player.buffImmune[BuffID.Venom] = true;
      player.buffImmune[BuffID.Weak] = true;
      player.buffImmune[BuffID.WitheredArmor] = true;
      player.buffImmune[BuffID.WitheredWeapon] = true;
      player.buffImmune[BuffID.WindPushed] = true;
    }

    public override void PreUpdate() {
      if (CanUse && abilities.oPlayer.input.bash.JustPressed) {
        bool didBash = Start();
        if (didBash) {
          SetState(AbilityState.Starting);
        }
        else {
          if (!abilities.launch.CanUse) {
            abilities.oPlayer.PlayLocalSound("Ori/Bash/bashNoTargetB", 0.35f);
          }
        }
      }
      else if (InUse) {
        if (Starting) {
          if (stateTime > MinBashDuration) {
            SetState(AbilityState.Active, preserveCurrentTime: true);
          }
          return;
        }

        if (!Active) return;
        if (stateTime == MinBashDuration + 4) {
          abilities.oPlayer.PlayLocalSound("Ori/Bash/seinBashLoopA", 0.5f);
        }
        abilities.oPlayer.Animations.Update();

        if (stateTime <= MaxBashDuration && abilities.oPlayer.input.bash.Current &&
          BashEntity is not null && BashEntity.active) return;
        End();
        SetState(AbilityState.Inactive);
      }
    }

    private static void Unload() {
      _cannotBashNpc = null;
      _cannotBashProj = null;
    }
  }
}
