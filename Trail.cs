using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace OriMod {
  internal class Trail {
    private Vector2 Position = Vector2.Zero;
    private Point Frame = Point.Zero;
    private float Alpha = 1;
    private float StartAlpha = 1;
    private float Rotation = 0;
    private Point Direction = new Point(1, 1);
    private Vector2 Origin = new Vector2(OriPlayer.SpriteWidth / 2, OriPlayer.SpriteHeight / 2 + 6);
    private Texture2D Texture => _tex ?? (_tex = OriMod.Instance.GetTexture("PlayerEffects/OriGlow"));
    private Texture2D _tex;
    internal void Reset(OriPlayer oPlayer) {
      Player player = oPlayer.player;
      Position = player.Center;
      Frame = oPlayer.AnimFrame;
      Direction.X = player.direction;
      Direction.Y = (int)player.gravDir;
      StartAlpha = player.velocity.Length() * 0.002f;
      if (StartAlpha > 0.08f) StartAlpha = 0.08f;
      Alpha = StartAlpha;
      Rotation = oPlayer.AnimRads;
    }
    internal void Tick() {
      Alpha -= StartAlpha / 26;
      if (Alpha < 0) {
        Alpha = 0;
      }
    }
    internal DrawData GetDrawData(OriPlayer oPlayer) {
      Vector2 pos = Position - Main.screenPosition;
      Rectangle rect = new Rectangle(Frame.X, Frame.Y, OriPlayer.SpriteWidth, OriPlayer.SpriteHeight);
      SpriteEffects effect = SpriteEffects.None;
      if (Direction.X == -1) effect = effect | SpriteEffects.FlipHorizontally;
      if (Direction.Y == -1) effect = effect | SpriteEffects.FlipVertically;
      Color color = oPlayer.SpriteColor * (Alpha * 10);
      DrawData data = new DrawData(Texture, pos, rect, color, 0, Origin, 1, effect, 0);
      return data;
    }
  }
}