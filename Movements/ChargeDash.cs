using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Movements {
  public class ChargeDash : Ability {
    internal ChargeDash(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { Npc = 255; }
    
    private static readonly float[] Speeds = new float[] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
    };
    private const int Duration = 14;
    
    private int CurrTime = 0;
    public byte Npc { get; internal set; }
    internal int CurrDirection = 1;
    
    protected override void UpdateStarting() {
      float tempDist = 720f;
      int tempNPC = -1;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC localNpc = Main.npc[n];
        if (!localNpc.active) continue;
        float dist = (player.position - localNpc.position).Length();
        if (dist < tempDist) {
          tempDist = dist;
          tempNPC = localNpc.whoAmI;
        }
      }
      if (tempNPC != -1) {
        Npc = (byte)tempNPC;
        CurrDirection = player.direction = player.position.X - Main.npc[Npc].position.X < 0 ? 1 : -1;
      }
      else {
        CurrDirection = player.direction;
      }
      OPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + OriPlayer.RandomChar(3), .5f);
      // oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashChargeStart" + OriPlayer.RandomChar(2), .5f);
    }
    protected override void UpdateUsing() {
      float speed = Speeds[CurrTime];
      player.gravity = 0;
      if (Npc < Main.maxNPCs && Main.npc[Npc].active) {
        player.maxFallSpeed = speed;
        Vector2 dir = (Main.npc[Npc].position - player.position);
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed * 0.8f;
        if (CurrTime < Duration && (player.position - Main.npc[Npc].position).Length() < speed) {
          State = States.Inactive;
          CurrTime = Duration;
          player.position = Main.npc[Npc].position;
          player.position.Y -= 32f;
          Npc = 255;
          player.velocity *= speed < 50 ? 0.5f : 0.25f;
        }
      }
      else {
        player.velocity.X = speed * CurrDirection * 0.8f;
        player.velocity.Y = OPlayer.isGrounded ? -0.1f : 0.15f * CurrTime;
      }
      player.runSlowdown = 26f;
    }

    internal override void Tick() {
      if (!Refreshed && !OriMod.ChargeKey.Current) {
        Refreshed = true;
      }
      if (InUse) {
        Handler.dash.State = States.Inactive;
        Handler.dash.Refreshed = false;
        CurrTime++;
        if (CurrTime > Duration || OPlayer.onWall || OPlayer.bashActive || PlayerInput.Triggers.JustPressed.Jump) {
          State = States.Inactive;
          if ((Npc == 255 || CurrTime > 4) && Math.Abs(player.velocity.Y) < Math.Abs(player.velocity.X)) {
            Vector2 newVel = Npc == 255 && !Handler.airJump.InUse ? new Vector2(CurrDirection, 0) : player.velocity;
            newVel.Normalize();
            newVel *= Speeds[Speeds.Length - 1];
            player.velocity = newVel;
          }
          Npc = 255;
        }
      }
      else {
        CanUse = Refreshed && !OPlayer.onWall && !Handler.stomp.InUse && !OPlayer.bashActive /*TODO: Replace with IsInUse */;
        if (CanUse && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
          State = States.Active;
          CurrTime = 0;
          CanUse = false;
          Refreshed = false;
          UpdateStarting();
        }
      }
    }
  }
}