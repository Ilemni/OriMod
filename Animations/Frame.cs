using OriMod.Utilities;

namespace OriMod.Animations {
  /// <summary>
  /// Single frame of animation. Stores sprite position on the sprite sheet, and duration of the frame.
  /// </summary>
  public readonly struct Frame {
    public Frame(int x, int y, int duration = -1) : this((byte)x, (byte)y, (short)duration) { }
    public Frame(byte x, byte y, short duration = -1) {
      Tile = new PointByte(x, y);
      Duration = duration;
    }
    
    /// <summary>
    /// Position of the tile, in sprite-space.
    /// </summary>
    public readonly PointByte Tile;

    /// <summary>
    /// Duration of the tile. -1 if the animation should stay on this frame.
    /// </summary>
    public readonly short Duration;

    public override string ToString() => $"Tile [{Tile}] Duration {Duration}";
  }
}
