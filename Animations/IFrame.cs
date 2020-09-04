using OriMod.Utilities;

namespace OriMod.Animations {
  /// <summary>
  /// Single frame of animation. Stores sprite position on the sprite sheet, and duration of the frame.
  /// </summary>
  public interface IFrame {
    /// <summary>
    /// Position of the tile, in sprite-space.
    /// </summary>
    PointByte tile { get; }

    /// <summary>
    /// Duration of the tile. <see langword="-1"/> if the animation should stay on this frame.
    /// </summary>
    short duration { get; }
  }
}
