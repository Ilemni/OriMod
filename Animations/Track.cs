using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Animation Track created on startup
  /// </summary>
  public class Track {
    /// <summary>
    /// Creates a track with the given Header and Frames.
    /// <para>If <see cref="Header.Init"/> is <see cref="InitType.Range"/>, <paramref name="frames"/> will be filled automatically.</para>
    /// </summary>
    /// <param name="header"></param>
    /// <param name="frames"></param>
    public Track(Header header, params Frame[] frames) {
      Header = header;

      if (header.Init != InitType.Range || frames.Length < 2) {
        Frames = frames;
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
        Frames = newFrames.ToArray();
      }

      foreach (Frame f in frames) {
        if (f.Duration == -1) {
          Duration = -1;
          break;
        }
        Duration += f.Duration;
      }
    }

    /// <summary>
    /// Header used for this track.
    /// </summary>
    public readonly Header Header;

    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] Frames;
    
    /// <summary>
    /// Total duration of the track, equal to total duration of <see cref="Frames"/>. May be -1, which is no duration.
    /// </summary>
    public readonly short Duration;
  }
}
