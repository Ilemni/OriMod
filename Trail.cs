using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Utilities;
using Terraria;
using Terraria.DataStructures;

namespace OriMod {
  /// <summary>
  /// For drawing a trail behind the player.
  /// </summary>
  public class Trail {
    /// <summary>
    /// Creates a <see cref="Trail"/> that belongs to <paramref name="oPlayer"/>.
    /// </summary>
    /// <param name="oPlayer"><see cref="OriPlayer"/> this trail will belong to.</param>
    internal Trail(OriPlayer oPlayer) => this.oPlayer = oPlayer;

    private readonly OriPlayer oPlayer;
    private Vector2 Position;
    private PointByte Tile;
    private float Alpha = 1;
    private float StartAlpha = 1;
    private SpriteEffects effect;
    
    private static Vector2 Origin => new Vector2(OriPlayer.SpriteWidth / 2, OriPlayer.SpriteHeight / 2 + 6);

    /// <summary>
    /// Resets various attributes to be based on the player's current attributes.
    /// </summary>
    public void Reset() {
      var player = oPlayer.player;
      Position = player.Center;
      Tile = oPlayer.animationTile;
      
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
    public void Tick() {
      Alpha -= StartAlpha / 26;
      if (Alpha < 0) {
        Alpha = 0;
      }
    }

    /// <summary>
    /// Gets the Trail <see cref="DrawData"/> for this <see cref="OriPlayer"/>.
    /// </summary>
    public DrawData GetDrawData() {
      var pos = Position - Main.screenPosition;
      var frame = OriPlayer.TileToPixel(Tile);
      var rect = new Rectangle(frame.X, frame.Y, OriPlayer.SpriteWidth, OriPlayer.SpriteHeight);
      var color = oPlayer.SpriteColorPrimary * (Alpha * 10);

      return new DrawData(OriTextures.Instance.Trail, pos, rect, color, 0, Origin, 1, effect, 0);
    }
  }
}
