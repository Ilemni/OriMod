using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Animation Track created on startup, contains <see cref="Frame"/> data.
  /// </summary>
  public class Track {
    /// <summary>
    /// Creates a track with the given <see cref="Header"/> and <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="asRange">Whether to use <paramref name="frames"/> as-is (false) or populate the range between frames (true).</param>
    /// <param name="frames">Assigns to <see cref="frames"/>. Used instead as a range if <paramref name="asRange"/> is <c>true</c>.</param>
    public Track(Header header, bool asRange, params Frame[] frames) {
      this.header = header ?? throw new System.ArgumentNullException(nameof(header));

      if (!asRange || frames.Length < 2) {
        this.frames = frames;
      }
      else {
        // Fill range of frames
        // I.e. if given [(0,1), (0,4)], we make [(0,1), (0,2), (0,3), (0,4)]
        var newFrames = new List<Frame>();
        for (int i = 0; i < frames.Length - 1; i++) {
          Frame startFrame = frames[i];
          Frame endFrame = frames[i + 1];
          for (int y = startFrame.tile.Y; y < endFrame.tile.Y; y++) {
            newFrames.Add(new Frame(startFrame.tile.X, y, startFrame.duration));
          }
        }
        newFrames.Add(frames[frames.Length - 1]);
        this.frames = newFrames.ToArray();
      }
    }

    /// <summary>
    /// Creates a track using <see cref="Header.Default"/> and with the given <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="header">Assigns to <see cref="header"/>.</param>
    /// <param name="asRange">Whether to use <paramref name="frames"/> as-is (false) or populate the range between frames (true).</param>
    /// <param name="frames">Assigns to <see cref="frames"/>. Used instead as a range if <paramref name="asRange"/> is <c>true</c>.</param>
    public Track(bool asRange, params Frame[] frames) : this(Header.Default, asRange, frames) { }

    /// <summary>
    /// Creates a track that consists of a single <see cref="Frame"/>, and uses the given <see cref="Header"/>.
    /// </summary>
    /// <param name="header">Assigns to <see cref="header"/>.</param>
    /// <param name="frame">Assigns to <see cref="frames"/> as a single <see cref="Frame"/>.</param>
    public Track(Header header, Frame frame) {
      this.header = header ?? throw new System.ArgumentNullException(nameof(header));
      frames = new[] { frame };
    }

    /// <summary>
    /// Creates a track that consists of a single <see cref="Frame"/>, and uses <see cref="Header.Default"/>.
    /// </summary>
    /// <param name="frame">Assigns to <see cref="frames"/> as a single <see cref="Frame"/>.</param>
    public Track(Frame frame) : this(Header.Default, frame) { }

    /// <summary>
    /// Header used for this track.
    /// </summary>
    public readonly Header header;

    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] frames;
  }
}
