using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Animation Track created on startup, contains <see cref="Frame"/> data.
  /// </summary>
  public class Track {
    /// <summary>
    /// Creates a track with the given Header and Frames.
    /// <para>If <paramref name="header"/> uses <see cref="InitType.Range"/>, <paramref name="frames"/> will be filled automatically.</para>
    /// </summary>
    /// <param name="header">Assigns to <see cref="header"/>.</param>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param>
    public Track(Header header, params Frame[] frames) {
      this.header = header;

      if (header.init != InitType.Range || frames.Length < 2) {
        this.frames = frames;
      }
      else {
        var newFrames = new List<Frame>();
        for (int i = 0; i < frames.Length - 1; i++) {
          Frame startFrame = frames[i];
          Frame endFrame = frames[i + 1];
          for (int y = startFrame.Tile.Y; y < endFrame.Tile.Y; y++) {
            newFrames.Add(new Frame(startFrame.Tile.X, y, startFrame.Duration));
          }
        }
        newFrames.Add(frames[frames.Length - 1]);
        this.frames = newFrames.ToArray();
      }

      foreach (Frame f in frames) {
        if (f.Duration == -1) {
          duration = -1;
          break;
        }
        duration += f.Duration;
      }
    }

    /// <summary>
    /// Header used for this track.
    /// </summary>
    public readonly Header header;

    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] frames;
    
    /// <summary>
    /// Total duration of the track, equal to total duration of <see cref="frames"/>. May be -1, which is no duration.
    /// </summary>
    public readonly short duration;
  }
}
