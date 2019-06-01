using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;

namespace OriMod.Movements {
  public class Dash : Ability {
    internal Dash(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }
    private static readonly float[] Speeds = new float[] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    };
    private const int Duration = 24;
    private int CurrTime = 0;
    internal int CurrDirection = 1;
    
    internal override bool CanUse {
      get {
        return !InUse && Refreshed && !OPlayer.onWall && !Handler.stomp.InUse && !OPlayer.bashActive /* TODO: Replace with bash.inUse */;
      }
    }
    private void StartDash() {
      CurrDirection = player.direction;
      OPlayer.PlayNewSound("Ori/Dash/seinDash" + OriPlayer.RandomChar(3), 0.2f);
      player.pulley = false;
    }

    protected override void UpdateActive() {
      player.velocity.X = Speeds[CurrTime] * CurrDirection * 0.65f;
      player.velocity.Y = 0.25f * CurrTime;
      if (CurrTime > 20) player.runSlowdown = 26f;
    }
    internal override void Tick() {
      if (!Refreshed && (OPlayer.isGrounded || OPlayer.onWall || OPlayer.bashActive /* TODO: Replace with bash.inUse */)) {
        Refreshed = true;
      }
      if (InUse) {
        CurrTime++;
        if (CurrTime > Duration || OPlayer.onWall || OPlayer.bashActive) {
          State = States.Inactive;
        }
      }
      else {
        if (CanUse && OriMod.DashKey.JustPressed) {
          StartDash();
          State = States.Active;
          CurrTime = 0;
          Refreshed = false;
        }
      }
    }
  }
}