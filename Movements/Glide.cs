using Terraria;
using Terraria.GameInput;

namespace OriMod.Movements {
  public class Glide : Movement {
    public Glide(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }

    public const float glideMaxFallSpeed = 2f;
    public const float glideRunSlowdown = 0.125f;
    public const float glideRunAcceleration = 0.2f;
    private const int glideStartDuration = 8;
    private const int glideEndDuration = 10;
    public int glideCurrTime = 0;

    public override void Active() {
      if (PlayerInput.Triggers.JustPressed.Left || PlayerInput.Triggers.JustPressed.Right) {
        oPlayer.PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + OriPlayer.RandomChar(5), 0.45f);
      }
      player.maxFallSpeed = glideMaxFallSpeed;
      player.runSlowdown = glideRunSlowdown;
      player.runAcceleration = glideRunAcceleration;
    }
    public override void Starting() {
      if (glideCurrTime == 0) oPlayer.PlayNewSound("Ori/Glide/seinGlideStart" + OriPlayer.RandomChar(3), 0.8f);
      Active();
    }
    public override void Ending() {
      if (glideCurrTime == 0) oPlayer.PlayNewSound("Ori/Glide/seinGildeEnd" + OriPlayer.RandomChar(3), 0.8f);
      Active();
    }
    public override void Tick() {
      if (!unlocked) return;
      if (Handler.dash.inUse) {
        state = State.Inactive;
        canUse = false;
        glideCurrTime = 0;
        return;
      }
      if (inUse) {
        if (IsState(State.Starting)) {
          glideCurrTime++;
          if (glideCurrTime > glideStartDuration) {
            state = State.Active;
            glideCurrTime = 0;
          }
        }
        else if (IsState(State.Ending)) {
          glideCurrTime++;
          if (glideCurrTime > glideEndDuration) {
            state = State.Inactive;
            canUse = true;
          }
        }
        if (player.velocity.Y < 0 || oPlayer.onWall || oPlayer.isGrounded) {
          state = inUse ? State.Ending : State.Inactive;
          canUse = false;
        }
        
        else if (OriMod.FeatherKey.JustReleased) {
          state = State.Ending;
          glideCurrTime = 0;
        }
      }
      else {
        if (player.velocity.Y > 0 && !oPlayer.onWall && (OriMod.FeatherKey.JustPressed || OriMod.FeatherKey.Current)) {
          state = State.Starting;
          glideCurrTime = 0;
        }
      }
    }
  }
}