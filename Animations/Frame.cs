using OriMod.Utilities;

namespace OriMod.Animations {
  /// <summary>
  /// Single frame of animation. Stores sprite position on the sprite sheet, and duration of the frame.
  /// </summary>
  public readonly struct Frame {
    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, and frame duration to play. These values will be cast to smaller data types.
    /// </summary>
    /// <param name="x">X position of the tile. This will be cast to a byte.</param>
    /// <param name="y">Y position of the tile. This will be cast to a byte.</param>
    /// <param name="duration">Duration of the frame. This will be cast to a short.</param>
    public Frame(int x, int y, int duration = -1) : this((byte)x, (byte)y, (short)duration) { }

    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, and frame duration to play.
    /// </summary>
    /// <param name="x">X position of the tile.</param>
    /// <param name="y">Y position of the tile.</param>
    /// <param name="duration">Duration of the frame.</param>
    public Frame(byte x, byte y, short duration = -1) {
      tile = new PointByte(x, y);
      this.duration = duration;
    }
    
    /// <summary>
    /// Position of the tile, in sprite-space.
    /// </summary>
    public readonly PointByte tile;

    /// <summary>
    /// Duration of the tile. -1 if the animation should stay on this frame.
    /// </summary>
    public readonly short duration;

    public override string ToString() => $"tile:[{tile}], duration:{duration}";
  }
}
