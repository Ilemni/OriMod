using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities {
  public class Bash : Ability {
    internal Bash(OriAbilities handler) : base(handler) { }
    public override int id => AbilityID.Bash;
    internal override bool DoUpdate => InUse || oPlayer.Input(OriMod.BashKey.Current);
    internal override bool CanUse => base.CanUse && Inactive && !Handler.stomp.InUse && !Handler.cJump.InUse;
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
    public bool TargetIsProjectile { get; private set; }
    private byte NpcID;
    private byte WormID;
    private ushort ProjID;
    public bool IsBashingWorm => WormID < Main.maxNPCs;
    public NPC BashNpc => NpcID < Main.maxNPCs ? Main.npc[NpcID] : null;
    public NPC WormNpc => WormID < Main.maxNPCs ? Main.npc[WormID] : null;
    public Projectile BashProj => ProjID < Main.projectile.Length ? Main.projectile[ProjID] : null;
    public Entity BashEntity => TargetIsProjectile ? BashProj as Entity : BashNpc as Entity;

    protected override void ReadPacket(System.IO.BinaryReader r) {
      if (InUse) {
        if (Starting) {
          targetStartPos = r.ReadVector2();
          playerStartPos = r.ReadVector2();
        }
        TargetIsProjectile = r.ReadBoolean();
        if (TargetIsProjectile) {
          ProjID = r.ReadUInt16();
        }
        else {
          NpcID = r.ReadByte();
        }
      }
    }
    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      if (InUse) {
        if (Starting) {
          packet.WriteVector2(targetStartPos);
          packet.WriteVector2(playerStartPos);
        }
        packet.Write((bool)TargetIsProjectile);
        if (TargetIsProjectile) {
          packet.Write((ushort)ProjID);
        }
        else {
          packet.Write((byte)NpcID);
        }
      }
    }
    private bool BashNpcFilter(NPC npc) =>
      !npc.friendly && !npc.boss && npc.aiStyle != 37 && !CannotBash.Contains(npc.type);
    private bool BashProjFilter(Projectile proj) =>
      (proj.friendly ? Config.BashOnProjectilesFriendly : true) && proj.damage != 0 && !proj.minion && !proj.sentry && !proj.trap && !CannotBashProj.Contains(proj.type);
    
    private bool BashStart() {
      float currDist = BashRange;
      bool isBashingNpc = player.GetClosestEntity(Main.npc, ref currDist, out int tempNpcID, condition:BashNpcFilter);
      int tempProjID = ushort.MaxValue;
      bool isBashingProj = Config.BashOnProjectiles ? player.GetClosestEntity(Main.projectile, ref currDist, out tempProjID, condition:BashProjFilter) : false;
      if (!isBashingProj && !isBashingNpc) return false;
      if (isBashingNpc) {
        isBashingProj = false;
      }
      TargetIsProjectile = isBashingProj;

      if (isBashingNpc) {
        NpcID = (byte)tempNpcID;
        WormID = BashNpc.aiStyle == 6 ? (byte)BashNpc.ai[3] : (byte)255;
        if (IsBashingWorm) {
          OriNPC oNpc = WormNpc.GetGlobalNPC<OriNPC>();
          oNpc.IsBashed = true;
          oNpc.BashPos = WormNpc.Center;
        }
        else {
          OriNPC oNpc = BashNpc.GetGlobalNPC<OriNPC>();
          oNpc.IsBashed = true;
          oNpc.BashPos = BashNpc.Center;
        }
      }
      else {
        ProjID = (ushort)tempProjID;
        OriProjectile oProj = BashProj.GetGlobalProjectile<OriProjectile>();
        oProj.IsBashed = true;
        oProj.BashPos = BashProj.Center;
      }
      
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
      oPlayer.PlayNewSound("Ori/Bash/seinBashEnd" + OriPlayer.RandomChar(3, ref CurrSoundRand), 0.7f);
      oPlayer.UnrestrictedMovement = true;
      
      Vector2 bashVector = new Vector2((float)(0 - (Math.Cos(bashAngle))), (float)(0 - (Math.Sin(bashAngle))));
      Vector2 playerBashVector = bashVector * BashPlayerStrength;
      Vector2 npcBashVector = -bashVector * BashNpcStrength;
      player.velocity = playerBashVector;
      player.position += playerBashVector * 10;
      BashEntity.velocity = npcBashVector;
      player.position += npcBashVector * 10;

      player.immuneTime = 5;

      if (TargetIsProjectile) {
        OriProjectile bashProj = BashProj.GetGlobalProjectile<OriProjectile>();
        bashProj.IsBashed = false;
      }
      else {
        OriNPC bashNpc = (IsBashingWorm ? WormNpc : BashNpc).GetGlobalNPC<OriNPC>();
        bashNpc.IsBashed = false;
        int damage = BashDamage + OriWorld.GlobalSeinUpgrade * 9;
        player.ApplyDamageToNPC(BashNpc, damage, 0, 1, false);
      }
      if (oPlayer.IsGrounded) {
        player.position.Y -= 1f;
      }
      PutOnCooldown();
    }
    protected override void UpdateUsing() {
      if (!Ending) {
        if (BashEntity != null) BashEntity.Center = targetStartPos;
        player.velocity = Vector2.Zero;
        player.gravity = 0;
      }
      if (TargetIsProjectile) {
        BashProj.netUpdate2 = true;
      }
      else {
        BashNpc.netUpdate2 = true;
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
          Failed = true;
        }
        else {
          Starting = true;
        }
        return;
      }
      else if (InUse) {
        CurrDuration++;
        if (Starting) {
          if (CurrDuration > MinBashDuration) {
            Active = true;
          }
          return;
        }
        if (Active) {
          if (CurrDuration > MaxBashDuration || !OriMod.BashKey.Current || BashEntity == null || !BashEntity.active) {
            Ending = true;
          }
          return;
        }
        if (Ending) {
          Inactive = true;
          return;
        }
      }
      else {
        if (Failed) {
          Inactive = true;
        }
        TickCooldown();
      }
    }
  }
}