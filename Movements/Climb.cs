using Terraria.GameInput;

namespace OriMod.Movements {
  public class Climb : Ability {
    internal Climb(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }

    protected override void UpdateActive() {
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

    internal override void Tick() {
      CanUse = OPlayer.onWall;
      if (InUse) {
        if (!OPlayer.onWall) {
          State = States.Inactive;
        }
      }
      else {
        if (CanUse && OriMod.ClimbKey.Current) {
          State = States.Active;
        }
      }
    }
  }
}