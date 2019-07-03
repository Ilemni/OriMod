using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  public class ChargeDash : Ability {
    internal ChargeDash(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { NpcID = 255; }
    internal override bool DoUpdate => !Handler.dash.DoUpdate && (InUse || (oPlayer.Input(OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) && Handler.cDash.Refreshed));
    internal override bool CanUse => base.CanUse && Refreshed && !InUse && !oPlayer.OnWall && !Handler.stomp.InUse && !Handler.bash.InUse && !player.mount.Active;
    protected override int Cooldown => 180;
    protected override Color RefreshColor => Color.LightBlue;
    
    private int ManaCost => 20;
    private int Duration => 14;
    private static readonly float[] Speeds = new float[] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
    };
    
    public byte NpcID { get; internal set; }
    internal int Direction = 1;
    private int CurrTime = 0;
    
    public Projectile Proj { get; private set; }

    protected override void ReadPacket(System.IO.BinaryReader r) {
      NpcID = r.ReadByte();
    }
    protected override void WritePacket(ModPacket packet) {
      packet.Write((byte)NpcID);
    }

    internal override void PutOnCooldown(bool force=false) {
      Refreshed = false;
      base.PutOnCooldown(force);
    }
    protected override void TickCooldown() {
      if (!Refreshed || CurrCooldown > 0) {
        CurrCooldown--;
        if (CurrCooldown < 0 && !OriMod.ChargeKey.Current) {
          Refreshed = true;
        }
      }
    }
    
    protected override void UpdateStarting() {
      float tempDist = 720f;
      int tempNPC = -1;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC localNpc = Main.npc[n];
        if (!localNpc.active || localNpc.friendly) continue;
        float dist = (player.position - localNpc.position).Length();
        if (dist < tempDist) {
          tempDist = dist;
          tempNPC = localNpc.whoAmI;
        }
      }
      if (tempNPC != -1) {
        NpcID = (byte)tempNPC;
        Direction = player.direction = player.position.X - Main.npc[NpcID].position.X < 0 ? 1 : -1;
      }
      else {
        Direction = (Direction = PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction);
      }
      oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + OriPlayer.RandomChar(3), .5f);
      Proj = Main.projectile[Projectile.NewProjectile(player.Center, new Vector2(0, 0), oPlayer.mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1)];
      Proj.damage = 12 + OriWorld.GlobalSeinUpgrade * 9;
    }
    protected override void UpdateUsing() {
      float speed = Speeds[CurrTime];
      player.gravity = 0;
      if (NpcID < Main.maxNPCs && Main.npc[NpcID].active) {
        player.maxFallSpeed = speed;
        Vector2 dir = (Main.npc[NpcID].position - player.position);
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed * 0.8f;
        if (CurrTime < Duration && (player.position - Main.npc[NpcID].position).Length() < speed) {
          Inactive = true;
          PutOnCooldown();
          CurrTime = Duration;
          player.position = Main.npc[NpcID].position;
          player.position.Y -= 32f;
          NpcID = 255;
          player.velocity *= speed < 50 ? 0.5f : 0.25f;
        }
      }
      else {
        player.velocity.X = speed * Direction * 0.8f;
        player.velocity.Y = (oPlayer.IsGrounded ? -0.1f : 0.15f * (CurrTime + 1)) * player.gravDir;
      }
      
      Proj.width = (int)Utils.Clamp((Math.Abs(player.velocity.X) * 2.5f), 96, 250);
      Proj.height = (int)Utils.Clamp((Math.Abs(player.velocity.Y) * 2.5f), 96, 250);
      Proj.Center = player.Center;
      player.runSlowdown = 26f;
      oPlayer.ImmuneTimer = 12;
    }

    internal override void Tick() {
      if (CanUse && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
        if (player.CheckMana(ManaCost, true, true)) {
          Active = true;
          CurrTime = 0;
          UpdateStarting();
        }
        else if (!Handler.dash.InUse) {
          Handler.dash.StartDash();
        }
        return;
      }
      TickCooldown();
      if (InUse) {
        Handler.dash.Inactive = true;
        Handler.dash.Refreshed = false;
        CurrTime++;
        if (CurrTime > Duration || oPlayer.OnWall || Handler.bash.InUse || PlayerInput.Triggers.JustPressed.Jump) {
          Inactive = true;
          PutOnCooldown();
          if ((NpcID == 255 || CurrTime > 4) && Math.Abs(player.velocity.Y) < Math.Abs(player.velocity.X)) {
            Vector2 newVel = NpcID == 255 && !Handler.airJump.InUse ? new Vector2(Direction, 0) : player.velocity;
            newVel.Normalize();
            newVel *= Speeds[Speeds.Length - 1];
            player.velocity = newVel;
          }
          Proj = null;
          NpcID = 255;
        }
      }
    }
  }
}