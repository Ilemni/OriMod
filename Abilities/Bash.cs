using Microsoft.Xna.Framework;
using OriMod.NPCs;
using OriMod.Projectiles;
using OriMod.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for pushing the player and enemies in opposite directions. Iconic ability of the Ori franchise.
  /// </summary>
  public sealed class Bash : Ability {
    static Bash() => OriMod.OnUnload += Unload;
    internal Bash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Bash;

    internal override bool UpdateCondition => true;
    internal override bool CanUse => base.CanUse && Inactive && !Manager.stomp.InUse && !Manager.chargeJump.InUse;
    protected override int Cooldown => (int)(Config.BashCooldown * 30);
    protected override Color RefreshColor => Color.LightYellow;

    public static List<short> CannotBashNPC => _cannotBashNPC ?? (_cannotBashNPC = new List<short> {
      NPCID.BlazingWheel, NPCID.SpikeBall
    });
    public static List<short> CannotBashProj => _cannotBashProj ?? (_cannotBashProj = new List<short> {
      ProjectileID.FlamethrowerTrap, ProjectileID.FlamesTrap, ProjectileID.GeyserTrap, ProjectileID.SpearTrap,
      ProjectileID.GemHookAmethyst, ProjectileID.GemHookDiamond, ProjectileID.GemHookEmerald,
      ProjectileID.GemHookRuby, ProjectileID.GemHookSapphire, ProjectileID.GemHookTopaz,
      ProjectileID.Hook, ProjectileID.AntiGravityHook, ProjectileID.BatHook, ProjectileID.CandyCaneHook,
      ProjectileID.DualHookBlue, ProjectileID.DualHookRed, ProjectileID.FishHook, ProjectileID.IlluminantHook,
      ProjectileID.LunarHookNebula, ProjectileID.LunarHookSolar, ProjectileID.LunarHookStardust, ProjectileID.LunarHookVortex,
      ProjectileID.SlimeHook, ProjectileID.StaticHook, ProjectileID.TendonHook, ProjectileID.ThornHook, ProjectileID.TrackHook,
      ProjectileID.WoodHook, ProjectileID.WormHook,
    });

    private static List<short> _cannotBashNPC;
    private static List<short> _cannotBashProj;

    private static int BashDamage => 15;
    private static float BashPlayerStrength => Config.BashStrength;
    private static float BashNpcStrength => BashPlayerStrength * 0.8f;
    private static float BashRange => 48f;
    private static int MinBashDuration => 30;
    private static int MaxBashDuration => 85;

    private Vector2 playerStartPos;
    private Vector2 targetStartPos;

    /// <summary>
    /// <see cref="OriNPC"/> or <see cref="OriProjectile"/> that is being targeted.
    /// </summary>
    public IBashable BashTarget { get; private set; }

    /// <summary>
    /// Entity that this player is Bashing.
    /// </summary>
    public Entity BashEntity { get; private set; }

    private readonly RandomChar rand = new RandomChar();

    protected override void ReadPacket(System.IO.BinaryReader r) {
      if (InUse) {
        if (Starting) {
          targetStartPos = r.ReadVector2();
          playerStartPos = r.ReadVector2();
        }
        var isNPC = r.ReadBoolean();
        var id = r.ReadUInt16();
        SetTarget(isNPC, id);
      }
    }

    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      if (InUse) {
        if (Starting) {
          packet.WriteVector2(targetStartPos);
          packet.WriteVector2(playerStartPos);
        }
        packet.Write(BashEntity is NPC);
        packet.Write(BashEntity?.whoAmI ?? ushort.MaxValue);
      }
    }

    /// <summary>
    /// Filter to determine if this <see cref="NPC"/> can be bashed. Returns true if the NPC should be bashed.
    /// <para>Excludes friendly NPCs, bosses, specific NPCs, and NPCs that are already being Bashed.</para>
    /// </summary>
    /// <param name="npc"></param>
    /// <returns>True if the NPC should be bashed.</returns>
    private bool BashNpcFilter(NPC npc) =>
      !npc.friendly && !npc.boss && npc.aiStyle != 37 && !CannotBashNPC.Contains((short)npc.type) && !npc.GetGlobalNPC<OriNPC>().IsBashed;

    /// <summary>
    /// Filter to determine if this <see cref="Projectile"/> can be bashed. Returns true if the projectile should be bashed.
    /// <para>Excludes friendly if disallowed, 0 damage projectiles, minions, sentries, traps, grapples, and projectiles that are already being Bashed.</para>
    /// </summary>
    /// <param name="proj"></param>
    /// <returns>True if the projectile should be bashed.</returns>
    private bool BashProjFilter(Projectile proj) =>
      (Config.BashOnProjectilesFriendly || !proj.friendly) && proj.damage != 0 && !proj.minion && !proj.sentry && !proj.trap && !CannotBashProj.Contains((short)proj.type) && !proj.GetGlobalProjectile<OriProjectile>().IsBashed;

    private void SetTarget(bool isNPC, ushort id) {
      if (id == ushort.MaxValue) {
        SetTarget(null);
      }
      else if (isNPC) {
        SetTarget(Main.npc[id]);
      }
      else {
        SetTarget(Main.projectile[id]);
      }
    }

    /// <summary>
    /// Sets the target to be bashed to <paramref name="entity"/>. Set to null to not bash anything.
    /// </summary>
    /// <param name="entity"><see cref="Entity"/> to target for bashing.</param>
    private void SetTarget(Entity entity) {
      BashEntity = entity;
      BashTarget =
        entity is NPC npc ? npc.GetGlobalNPC<OriNPC>() as IBashable :
        entity is Projectile projectile ? projectile.GetGlobalProjectile<OriProjectile>() : null;
    }

    /// <summary>
    /// Attempt to start Bash. This will search for an <see cref="NPC"/> or <see cref="Projectile"/> to bash,
    /// </summary>
    /// <returns></returns>
    private bool Start() {
      SetTarget(null);
      float currDist = BashRange;

      // Check for Bashing NPCs
      bool isBashingNpc = player.GetClosestEntity(Main.npc, ref currDist, out NPC npc, condition: BashNpcFilter);
      if (isBashingNpc) {
        if (npc.aiStyle == 6) {
          // Worm: Must bash head of worm-like rather than body
          // Otherwise only part of the npc will be suspended
          npc = Main.npc[(int)npc.ai[3]];
        }
        SetTarget(npc);
      }
      else {
        // If no NPCs to Bash, check for Projectiles
        if (!Config.BashOnProjectiles) {
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
      BashTarget.BashPlayer = oPlayer;

      playerStartPos = player.Center;
      targetStartPos = BashEntity.Center;
      oPlayer.PlayNewSound("Ori/Bash/seinBashStartA", 0.7f);
      return true;
    }

    private void End() {
      player.pulley = false;
      float bashAngle = player.AngleFrom(Main.MouseWorld);
      oPlayer.PlayNewSound("Ori/Bash/seinBashEnd" + rand.NextNoRepeat(3), 0.7f);
      oPlayer.UnrestrictedMovement = true;

      var bashVector = new Vector2((float)(0 - Math.Cos(bashAngle)), (float)(0 - Math.Sin(bashAngle)));
      Vector2 playerBashVector = bashVector * BashPlayerStrength;
      Vector2 npcBashVector = -bashVector * BashNpcStrength;
      player.velocity = playerBashVector;
      player.position += playerBashVector * 3;
      BashEntity.velocity = npcBashVector;
      player.position += npcBashVector * 5;
      if (oPlayer.IsGrounded) {
        player.position.Y -= 1f;
      }

      player.immuneTime = 5;

      BashTarget.IsBashed = false;
      /*if (TargetIsNpc) {
        int damage = BashDamage + (int)OriWorld.GlobalUpgrade * 9;
        player.ApplyDamageToNPC(Main.npc[TargetID], damage, 0, 1, false);
      }*/

      PutOnCooldown();
    }

    protected override void UpdateUsing() {
      if (!Ending) {
        if (BashEntity != null) {
          BashEntity.Center = targetStartPos;
        }

        player.velocity = Vector2.Zero;
        player.gravity = 0;
      }
      if (BashEntity is NPC npc) {
        npc.netUpdate2 = true;
      }
      else if (BashEntity is Projectile projectile) {
        projectile.netUpdate2 = true;
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
      player.immune = true;
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

    internal override void Tick() {
      if (CanUse && OriMod.BashKey.JustPressed) {
        bool didBash = Start();
        if (didBash) {
          SetState(State.Starting);
        }
        else {
          oPlayer.PlayNewSound("Ori/Bash/bashNoTargetB", 0.4f);
        }
        return;
      }
      else if (InUse) {
        if (Starting) {
          if (CurrentTime > MinBashDuration) {
            SetState(State.Active, preserveCurrentTime: true);
          }
          return;
        }
        if (Active) {
          if (CurrentTime == MinBashDuration + 2) {
            oPlayer.PlayNewSound("Ori/Bash/seinBashLoopA", 0.7f);
          }
          if (CurrentTime > MaxBashDuration || IsLocal && !OriMod.BashKey.Current || BashEntity is null || !BashEntity.active) {
            End();
            SetState(State.Inactive);
          }
          return;
        }
      }
      else {
        TickCooldown();
      }
    }

    private static void Unload() {
      _cannotBashNPC = null;
      _cannotBashProj = null;
    }
  }
}
