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
    internal Bash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Bash;

    internal override bool UpdateCondition => InUse || oPlayer.Input(OriMod.BashKey.Current);
    internal override bool CanUse => base.CanUse && Inactive && !Manager.stomp.InUse && !Manager.chargeJump.InUse;
    protected override int Cooldown => (int)(Config.BashCooldown * 30);
    protected override Color RefreshColor => Color.LightYellow;

    public static List<int> CannotBash = new List<int> {
      NPCID.BlazingWheel, NPCID.SpikeBall
    };
    public static List<int> CannotBashProj = new List<int> {
      ProjectileID.FlamethrowerTrap, ProjectileID.FlamesTrap, ProjectileID.GeyserTrap, ProjectileID.SpearTrap,
      ProjectileID.GemHookAmethyst, ProjectileID.GemHookDiamond, ProjectileID.GemHookEmerald,
      ProjectileID.GemHookRuby, ProjectileID.GemHookSapphire, ProjectileID.GemHookTopaz,
      ProjectileID.Hook, ProjectileID.AntiGravityHook, ProjectileID.BatHook, ProjectileID.CandyCaneHook,
      ProjectileID.DualHookBlue, ProjectileID.DualHookRed, ProjectileID.FishHook, ProjectileID.IlluminantHook,
      ProjectileID.LunarHookNebula, ProjectileID.LunarHookSolar, ProjectileID.LunarHookStardust, ProjectileID.LunarHookVortex,
      ProjectileID.SlimeHook, ProjectileID.StaticHook, ProjectileID.TendonHook, ProjectileID.ThornHook, ProjectileID.TrackHook,
      ProjectileID.WoodHook, ProjectileID.WormHook,
    };

    private int BashDamage => 15;
    private float BashPlayerStrength => Config.BashStrength;
    private float BashNpcStrength => BashPlayerStrength * 0.8f;
    private float BashRange => 48f;
    private int MinBashDuration => 30;
    private int MaxBashDuration => 85;

    internal int CurrDuration { get; private set; }
    private Vector2 playerStartPos;
    private Vector2 targetStartPos;
    
    /// <summary>
    /// True if the target is an <see cref="NPC"/>, false if it's a <see cref="Projectile"/>.
    /// </summary>
    public bool TargetIsNpc { get; private set; }
    
    /// <summary>
    /// ID of either an <see cref="NPC"/> or <see cref="Projectile"/>.
    /// </summary>
    private ushort TargetID;

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
        TargetIsNpc = r.ReadBoolean();
        TargetID = r.ReadUInt16();
      }
    }

    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      if (InUse) {
        if (Starting) {
          packet.WriteVector2(targetStartPos);
          packet.WriteVector2(playerStartPos);
        }
        packet.Write(TargetIsNpc);
        packet.Write(TargetID);
      }
    }

    /// <summary>
    /// Filter to determine if this <see cref="NPC"/> can be bashed. Returns true if the NPC should be bashed.
    /// <para>Excludes friendly NPCs, bosses, worms, and specific NPCs.</para>
    /// </summary>
    /// <param name="npc"></param>
    /// <returns>True if the NPC should be bashed.</returns>
    private bool BashNpcFilter(NPC npc) =>
      !npc.friendly && !npc.boss && npc.aiStyle != 37 && !CannotBash.Contains(npc.type);

    /// <summary>
    /// Filter to determine if this <see cref="Projectile"/> can be bashed. Returns true if the projectile should be bashed.
    /// <para>Excludes friendly if disallowed, 0 damage projectiles, minions, sentries, traps, and grapples.</para>
    /// </summary>
    /// <param name="proj"></param>
    /// <returns>True if the projectile should be bashed.</returns>
    private bool BashProjFilter(Projectile proj) =>
      (!proj.friendly || Config.BashOnProjectilesFriendly) && proj.damage != 0 && !proj.minion && !proj.sentry && !proj.trap && !CannotBashProj.Contains(proj.type);

    private bool BashStart() {
      float currDist = BashRange;

      BashTarget = null;
      BashEntity = null;

      // Check for Bashing NPCs
      bool isBashingNpc = player.GetClosestEntity(Main.npc, ref currDist, out NPC npc, condition: BashNpcFilter);
      if (isBashingNpc) {
        if (npc.aiStyle == 6) {
          // Bash head of worm-like rather than body
          npc = Main.npc[(int)npc.ai[3]];
        }
        TargetIsNpc = true;
        TargetID = (ushort)npc.whoAmI;
        BashEntity = npc;
        BashTarget = npc.GetGlobalNPC<OriNPC>();
      }
      else {
        // If no NPCs to Bash, check for Projectiles
        if (!Config.BashOnProjectiles) {
          return false;
        }

        currDist = BashRange;
        bool isBashingProj = player.GetClosestEntity(Main.projectile, ref currDist, out Projectile proj, condition: BashProjFilter);
        if (!isBashingProj) {
          return false;
        }
        
        TargetIsNpc = false;
        TargetID = (ushort)proj.whoAmI;
        BashEntity = proj;
        BashTarget = proj.GetGlobalProjectile<OriProjectile>();
      }
      
      BashTarget.IsBashed = true;
      BashTarget.BashPosition = BashEntity.Center;
      BashTarget.BashPlayer = oPlayer;

      playerStartPos = player.Center;
      targetStartPos = BashEntity.Center;
      oPlayer.PlayNewSound("Ori/Bash/seinBashStartA", 0.7f);
      return true;
    }

    protected override void UpdateActive() {
      if (CurrDuration == MinBashDuration + 2) {
        oPlayer.PlayNewSound("Ori/Bash/seinBashLoopA", 0.7f);
      }
    }

    protected override void UpdateEnding() {
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
      if (TargetIsNpc) {
        ((NPC)BashEntity).netUpdate2 = true;
      }
      else {
        ((Projectile)BashEntity).netUpdate2 = true;
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

    protected override void UpdateFailed() {
      oPlayer.PlayNewSound("Ori/Bash/bashNoTargetB", Main.soundVolume);
    }

    internal override void Tick() {
      if (CanUse && OriMod.BashKey.JustPressed) {
        CurrDuration = 0;
        bool didBash = BashStart();
        if (!didBash) {
          SetState(State.Failed);
        }
        else {
          SetState(State.Starting);
        }
        return;
      }
      else if (InUse) {
        CurrDuration++;
        if (Starting) {
          if (CurrDuration > MinBashDuration) {
            SetState(State.Active);
          }
          return;
        }
        if (Active) {
          if (CurrDuration > MaxBashDuration || !OriMod.BashKey.Current || BashEntity is null || !BashEntity.active) {
            SetState(State.Ending);
          }
          return;
        }
        if (Ending) {
          SetState(State.Inactive);
          return;
        }
      }
      else {
        if (Failed) {
          SetState(State.Inactive);
        }
        TickCooldown();
      }
    }
  }
}
