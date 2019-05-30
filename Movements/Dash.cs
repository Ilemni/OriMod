using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;

namespace OriMod.Movements {
  public class Dash : Movement {
    public Dash(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }
    private static readonly float[] speeds = new float[] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    };
    private const int duration = 24;
    public int currTime = 0;
    public int currDirection = 1;

    public override void Active() {
      if (currTime == 0) {
        currDirection = player.direction;
        oPlayer.PlayNewSound("Ori/Dash/seinDash" + OriPlayer.RandomChar(3), 0.2f);
        player.pulley = false;
      }
      player.velocity.X = speeds[currTime] * currDirection * 0.65f;
      player.velocity.Y = 0.25f * currTime;
      if (currTime > 20) player.runSlowdown = 26f;
    }
    public override void Tick() {
      if (!refreshed && (oPlayer.isGrounded || oPlayer.onWall || oPlayer.bashActive /* TODO: Replace with bash.inUse */)) {
        refreshed = true;
      }
      if (inUse) {
        currTime++;
        if (currTime > duration || oPlayer.onWall || oPlayer.bashActive) {
          state = State.Inactive;
        }
      }
      else {
        canUse = refreshed && !oPlayer.onWall && !Handler.stomp.inUse && !oPlayer.bashActive /* TODO: Replace with bash.inUse */;
        if (canUse && OriMod.DashKey.JustPressed) {
          state = State.Active;
          currTime = 0;
          refreshed = false;
          canUse = false;
        }
      }
    }
  }
}