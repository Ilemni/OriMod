using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace OriMod {
  internal class Trail {
    internal Trail(OriPlayer oriPlayer) {
      oPlayer = oriPlayer;
      player = oriPlayer.player;
    }

    private OriPlayer oPlayer { get; }
    private Player player { get; }
    private Vector2 Position = Vector2.Zero;
    private Point Frame = Point.Zero;
    private float Alpha = 1;
    private float StartAlpha = 1;
    private Point Direction = new Point(1, 1);
    private Vector2 Origin { get; } = new Vector2(OriPlayer.SpriteWidth / 2, OriPlayer.SpriteHeight / 2 + 6);
    private Texture2D Texture => _tex ?? (_tex = OriMod.Instance.GetTexture("PlayerEffects/OriGlow"));
    private Texture2D _tex;

    /// <summary> Resets various attributes to be based on the player's current attributes. </summary>
    internal void Reset() {
      Position = player.Center;
      Frame = oPlayer.AnimFrame;
      StartAlpha = player.velocity.Length() * 0.002f;
      if (StartAlpha > 0.08f) {
        StartAlpha = 0.08f;
      }

      Alpha = StartAlpha;
      Direction.X = player.direction;
      Direction.Y = (int)player.gravDir;
    }

    /// <summary> Decreases Alpha by a fixed amount. </summary>
    internal void Tick() {
      Alpha -= StartAlpha / 26;
      if (Alpha < 0) {
        Alpha = 0;
      }
    }

    /// <summary> Gets the Trail DrawData of this OriPlayer. </summary>
    internal DrawData GetDrawData() {
      Vector2 pos = Position - Main.screenPosition;
      var rect = new Rectangle(Frame.X, Frame.Y, OriPlayer.SpriteWidth, OriPlayer.SpriteHeight);

      SpriteEffects effect = SpriteEffects.None;
      if (Direction.X == -1) {
        effect |= SpriteEffects.FlipHorizontally;
      }
      if (Direction.Y == -1) {
        effect |= SpriteEffects.FlipVertically;
      }

      Color color = oPlayer.SpriteColor * (Alpha * 10);
      return new DrawData(Texture, pos, rect, color, 0, Origin, 1, effect, 0);
    }
  }
}
