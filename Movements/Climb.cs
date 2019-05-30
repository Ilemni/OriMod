using Terraria.GameInput;

namespace OriMod.Movements {
  public class Climb : Movement {
    public Climb(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }

    public override void Active() {
      player.gravity = 0;
      player.runAcceleration = 0;
      player.maxRunSpeed = 0;
      if (PlayerInput.Triggers.Current.Up) {
        player.velocity.Y += player.velocity.Y < -2 ? 1 : -1;
      }
      else if (PlayerInput.Triggers.Current.Down) {
        player.velocity.Y += player.velocity.Y < 4 ? 1 : -1;
      }
      if (
        (player.velocity.Y > 1 && !PlayerInput.Triggers.Current.Down) ||
        (player.velocity.Y < 1 && !PlayerInput.Triggers.Current.Up)) {
        player.velocity.Y /= 3;
      }
      if (
        player.velocity.Y > -1 && !PlayerInput.Triggers.Current.Down &&
        player.velocity.Y < 1 && !PlayerInput.Triggers.Current.Up
      ) {
        player.velocity.Y = 0;
      }
    }

    public override void Tick() {
      canUse = oPlayer.onWall;
      if (inUse) {
        if (!oPlayer.onWall) {
          state = State.Inactive;
        }
      }
      else {
        if (canUse && OriMod.ClimbKey.Current) {
          state = State.Active;
        }
      }
    }
  }
}