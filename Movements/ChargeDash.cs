using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Movements {
  public class ChargeDash : Movement {
    public ChargeDash(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { npc = 255; }
    
    private static readonly float[] speeds = new float[] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f
    };
    private const int duration = 14;
    
    private int currTime = 0;
    public byte npc = 255;
    public int currDirection = 1;
    public override void Starting() {
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
        npc = (byte)tempNPC;
        currDirection = player.position.X - Main.npc[npc].position.X < 0 ? 1 : -1;
      }
      currDirection = player.direction;
      currTime = 0;
      oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + OriPlayer.RandomChar(3), .5f);
      // oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashChargeStart" + OriPlayer.RandomChar(2), .5f);
    }
    public override void Using() { // TODO: Figure out why Ori drops so fast after Using ends
      float speed = currTime < speeds.Length ? speeds[currTime] : speeds[speeds.Length - 1];
      player.gravity = 0;
      if (npc < Main.maxNPCs && Main.npc[npc].active) {
        player.maxFallSpeed = speed;
        Vector2 dir = (Main.npc[npc].position - player.position);
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed * 0.8f;
        if (currTime < duration && (player.position - Main.npc[npc].position).Length() < speed) {
          state = State.Inactive;
          currTime = duration;
          player.position = Main.npc[npc].position;
          player.position.Y -= 32f;
          npc = 255;
          player.velocity *= speed < 50 ? 0.5f : 0.25f;
        }
      }
      else {
        player.velocity.X = speed * currDirection * 0.8f;
        player.velocity.Y = oPlayer.isGrounded ? -0.1f : 0.15f * currTime;
      }
      player.runSlowdown = 26f;
    }

    public override void Tick() {
      if (!refreshed && !OriMod.ChargeKey.Current) {
        refreshed = true;
      }
      if (inUse) {
        Handler.dash.state = State.Inactive;
        Handler.dash.refreshed = false;
        currTime++;
        if (currTime > duration || oPlayer.onWall || oPlayer.bashActive || PlayerInput.Triggers.JustPressed.Jump) {
          state = State.Inactive;
          if (currTime > 4 || npc == 255) {
            player.velocity.Normalize();
            player.velocity *= speeds[speeds.Length - 1];
          }
          npc = 255;
        }
      }
      else {
        canUse = refreshed && !oPlayer.onWall && !Handler.stomp.inUse && !oPlayer.bashActive /*TODO: Replace with IsInUse */;
        if (canUse && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
          state = State.Active;
          currTime = 0;
          canUse = false;
          refreshed = false;
          Starting();
        }
      }
    }
  }
}