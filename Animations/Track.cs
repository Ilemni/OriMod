using System;
using System.Collections.Generic;

namespace OriMod.Animations {
  internal class Track : IDisposable {
    internal Header Header { get; private set; }
    internal Frame[] Frames { get; private set; }
    internal int Duration { get; }

    internal Track(Header header, params Frame[] frames) {
      Header = header;

      if (header.Init != InitType.Range || frames.Length < 2) {
        Frames = frames;
      }
      else {
        var newFrames = new List<Frame>();
        for (int i = 0; i < frames.Length - 1; i++) {
          Frame startFrame = frames[i];
          Frame endFrame = frames[i + 1];
          for (int y = startFrame.Y; y < endFrame.Y; y++) {
            newFrames.Add(new Frame(startFrame.X, y, startFrame.Duration));
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

    public void Dispose() {
      Header.Dispose();
      Header = null;
      Frames = null;
    }
  }
}
