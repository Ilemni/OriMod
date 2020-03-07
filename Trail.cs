using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace OriMod {
  internal class Trail {
    internal Trail(OriPlayer oriPlayer) => oPlayer = oriPlayer;

    private readonly OriPlayer oPlayer;
    private Vector2 Position;
    private Point Frame;
    private float Alpha = 1;
    private float StartAlpha = 1;
    private SpriteEffects effect;
    
    private static Vector2 Origin => new Vector2(OriPlayer.SpriteWidth / 2, OriPlayer.SpriteHeight / 2 + 6);

    /// <summary>
    /// Resets various attributes to be based on the player's current attributes.
    /// </summary>
    internal void Reset() {
      var player = oPlayer.player;
      Position = player.Center;
      Frame = oPlayer.AnimFrame;
      
      StartAlpha = player.velocity.Length() * 0.002f;
      if (StartAlpha > 0.08f) {
        StartAlpha = 0.08f;
      }
      Alpha = StartAlpha;

      effect = SpriteEffects.None;
      if (player.direction == -1) {
        effect |= SpriteEffects.FlipHorizontally;
      }
      if (player.gravDir == -1) {
        effect |= SpriteEffects.FlipVertically;
      }
    }

    /// <summary>
    /// Decreases Alpha by a fixed amount.
    /// </summary>
    internal void Tick() {
      Alpha -= StartAlpha / 26;
      if (Alpha < 0) {
        Alpha = 0;
      }
    }

    /// <summary>
    /// Gets the Trail DrawData of this OriPlayer.
    /// </summary>
    internal DrawData GetDrawData() {
      Vector2 pos = Position - Main.screenPosition;
      var rect = new Rectangle(Frame.X, Frame.Y, OriPlayer.SpriteWidth, OriPlayer.SpriteHeight);

      Color color = oPlayer.SpriteColorPrimary * (Alpha * 10);
      return new DrawData(OriTextures.Instance.Trail, pos, rect, color, 0, Origin, 1, effect, 0);
    }
  }
}
