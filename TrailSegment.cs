using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Animations;
using OriMod.Utilities;
using Terraria;
using Terraria.DataStructures;

namespace OriMod {
  /// <summary>
  /// For drawing a trail behind the player.
  /// </summary>
  public class TrailSegment {
    /// <summary>
    /// Creates a <see cref="TrailSegment"/> that belongs to <paramref name="oPlayer"/>.
    /// </summary>
    /// <param name="oPlayer"><see cref="OriPlayer"/> this trail will belong to.</param>
    internal TrailSegment(OriPlayer oPlayer) => this.oPlayer = oPlayer;

    private readonly OriPlayer oPlayer;
    private Vector2 position;
    private PointByte tile;
    private byte time;
    private float startAlpha = 1;
    private SpriteEffects effect;

    /// <summary>
    /// Resets various attributes to be based on the player's current attributes.
    /// </summary>
    public void Reset() {
      var player = oPlayer.player;
      position = player.Center;
      tile = oPlayer.animations.playerAnim.ActiveFrame.tile;

      startAlpha = player.velocity.LengthSquared() * 0.0008f; // 0.002f
      if (startAlpha > 0.16f) {
        startAlpha = 0.16f;
      }
      time = 26;

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
      if (time > 0) {
        time--;
      }
    }

    /// <summary>
    /// Gets the Trail <see cref="DrawData"/> for this <see cref="OriPlayer"/>.
    /// </summary>
    public DrawData GetDrawData() {
      var pos = position - Main.screenPosition;
      var spriteSize = AnimationTrackData.Instance.PlayerAnim.spriteSize;
      var rect = new Rectangle(tile.X * spriteSize.X, tile.Y & spriteSize.Y, spriteSize.X, spriteSize.Y);
      var alpha = startAlpha * (time / 26f) - 0.1f * (26 - time);
      var color = oPlayer.SpriteColorPrimary * alpha;
      var origin = new Vector2(AnimationTrackData.SpriteWidth / 2, AnimationTrackData.SpriteHeight / 2 + 6);

      return new DrawData(OriTextures.Instance.Trail, pos, rect, color, 0, origin, 1, effect, 0);
    }
  }
}
