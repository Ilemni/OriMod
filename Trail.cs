using Microsoft.Xna.Framework;

namespace OriMod {
  internal class Trail {
    internal Vector2 Position = Vector2.Zero;
    private Vector2 _frame = Vector2.Zero;
    internal Vector2 Frame {
      get {
        return _frame;
      }
      set {
        value.X = (int)value.X;
        value.Y = (int)value.Y;
        _frame = value;
      }
    }
    internal int FrameX => (int)Frame.X;
    internal int FrameY => (int)Frame.Y;
    internal float Alpha = 1;
    internal float Rotation = 0;
    internal int Direction = 1;
  }
}