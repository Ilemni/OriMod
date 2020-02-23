using Microsoft.Xna.Framework;

namespace OriMod.Animations {
  internal class Frame {
    internal readonly byte X;
    internal readonly byte Y;
    internal Point Tile => new Point(X, Y);
    internal int Duration;

    internal Frame(int x, int y, int duration = -1) : this((byte)x, (byte)y, duration) { }
    internal Frame(byte x, byte y, int duration = -1) {
      X = x;
      Y = y;
      Duration = duration;
    }

    public override string ToString() => $"Tile [{X}, {Y}] Duration {Duration}";
  }
}
