using Microsoft.Xna.Framework;

namespace OriMod {
  internal class Trail {
    internal Vector2 Position = Vector2.Zero;
    internal Point Frame = Point.Zero;
    internal int X => Frame.X;
    internal int Y => Frame.Y;
    internal float Alpha = 1;
    internal float Rotation = 0;
    internal int Direction = 1;
  }
}