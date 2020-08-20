using OriMod.Utilities;

namespace OriMod.Animations {
  public readonly struct Frame {
    public Frame(int x, int y, int duration = -1) : this((byte)x, (byte)y, (short)duration) { }
    public Frame(byte x, byte y, short duration = -1) {
      Tile = new PointByte(x, y);
      Duration = duration;
    }

    public readonly PointByte Tile;

    public readonly short Duration;

    public override string ToString() => $"Tile [{Tile}] Duration {Duration}";
  }
}
