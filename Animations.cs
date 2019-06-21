using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OriMod {
  internal enum InitType {
    None = 0,
    Range = 1,
    Select = 2,
  }
  internal enum LoopMode {
    None = 0,
    Always = 1,
    Once = 2,
  }
  internal enum PlaybackMode {
      None = 0,
      Normal = 1,
      PingPong = 2,
      Reverse = 3,
      Random = 4,
  }
  internal class Header {
    internal string SpritePath;
    internal const string DefaultSpritePath = "PlayerEffects/OriPlayer";
    internal InitType Init;
    internal LoopMode Loop;
    internal PlaybackMode Playback;
    internal Header(string spritePath=DefaultSpritePath, InitType init=InitType.None, LoopMode loop=LoopMode.None, PlaybackMode playback=PlaybackMode.None) {
      SpritePath = spritePath;
      Init = init;
      Loop = loop;
      Playback = playback;
    }
    internal void OverwriteSome(Header other) {
      if (other.Init != 0) Init = other.Init;
      if (other.Loop != 0) Loop = other.Loop;
      if (other.Playback != 0) Playback = other.Playback;
    }
    internal Header CopySome(Header other) {
      return new Header(
        other.SpritePath,
        other.Init != 0 ? other.Init : Init,
        other.Loop != 0 ? other.Loop : Loop,
        other.Playback != 0 ? other.Playback : Playback
      );
    }
    internal static Header Default => new Header(DefaultSpritePath, InitType.Range, LoopMode.Always, PlaybackMode.Normal);
    internal static Header None => new Header(DefaultSpritePath, InitType.None, LoopMode.None, PlaybackMode.None);
    public override string ToString()
      => $"Init: {Init} | Loop: {Loop} | Playback: {Playback}" + (SpritePath != DefaultSpritePath ? $" | SpritePath: {SpritePath}" : "");
  }
  internal class Frame {
    internal byte X;
    internal byte Y;
    internal Point Tile => new Point(X, Y);
    internal int Duration;
    internal Frame(int x, int y, int duration=-1) {
      X = (byte)x;
      Y = (byte)y;
      Duration = duration;
    }
    internal Frame (byte x, byte y, int duration=-1) {
      X = x;
      Y = y;
      Duration = duration;
    }
    internal byte this[int idx] {
      get {
        if (idx == 0) return X;
        if (idx == 1) return Y;
        throw new System.IndexOutOfRangeException();
      }
    }
    public override string ToString() => $"Tile [{X}, {Y}] Duration {Duration}";
  }
  internal class Track {
    internal string SpritePath { get; }
    internal Header Header { get; }
    internal Frame[] Frames { get; }
    internal Track(Header header, params Frame[] frames) {
      Header = header;
      Frame[] newFrames = (header.Init == InitType.Range && frames.Length > 1) ? InitRange(frames) : frames;
      Frames = newFrames;
    }
    private Frame[] InitRange(Frame[] frames) {
      List<Frame> newFrames = new List<Frame>();
      for (int i = 0; i < frames.Length - 1; i++) {
        Frame startFrame = frames[i];
        Frame endFrame = frames[i + 1];
        for (int y = startFrame.Y; y < endFrame.Y; y++) {
          newFrames.Add(new Frame(startFrame.X, y, startFrame.Duration));
        }
      }
      newFrames.Add(frames[frames.Length - 1]);
      return newFrames.ToArray();
    }
    internal Frame this[int idx] => Frames[idx];
  }
  internal static partial class AnimationHandler {
    private static Frame f(int frameX, int frameY, int duration=-1) { return new Frame(frameX, frameY, duration); }
    private static Header h(string s=Header.DefaultSpritePath, InitType i=InitType.Range, LoopMode l=LoopMode.Always, PlaybackMode p=PlaybackMode.Normal)
      => new Header(spritePath:s, init:i, loop:l, playback:p);
    
    internal static Dictionary<string, Track> Tracks = new Dictionary<string, Track> {
      {"Default", new Track(h(),
        f(0, 0)
      )},
      {"FallNoAnim", new Track(h(),
        f(3, 13)
      )},
      {"Running", new Track(h(),
        f(2, 0, 4), f(2, 10, 4)
      )},
      {"Idle", new Track(h(),
        f(0, 1, 9), f(0, 8, 9)
      )},
      {"Dash", new Track(h(i:InitType.Select, l:LoopMode.Once),
        f(2, 12, 36), f(2, 13, 12)
      )},
      {"Bash", new Track(h(i:InitType.Select, l:LoopMode.Once),
        f(2, 14, 40), f(2, 13)
      )},
      {"CrouchStart", new Track(h(),
        f(1, 8)
      )},
      {"Crouch", new Track(h(),
        f(1, 9)
      )},
      {"WallJump", new Track(h(),
        f(5, 15, 12)
      )},
      {"WallChargeJumpCharge", new Track(h(),
        f(6, 0), f(6, 4)
      )},
      {"AirJump", new Track(h(),
        f(3, 0)
      )},
      {"ChargeJump", new Track(h(l:LoopMode.Once, p:PlaybackMode.PingPong),
        f(3, 5, 4), f(3, 8, 4)
      )},
      {"Falling", new Track(h(),
        f(3, 9, 4), f(3, 12, 4)
      )},
      {"ClimbIdle", new Track(h(),
        f(5, 0)
      )},
      {"Climb", new Track(h(),
        f(5, 1, 4), f(5, 8, 4)
      )},
      {"WallSlide", new Track(h(),
        f(5, 9, 5), f(5, 12, 5)
      )},
      {"IntoJumpBall", new Track(h(i:InitType.Select, l:LoopMode.Once),
        f(3, 3, 6), f(3, 4, 4)
      )},
      {"IdleAgainst", new Track(h(),
        f(0, 9, 7), f(0, 14, 7)
      )},
      {"Jump", new Track(h(i:InitType.Select, p:PlaybackMode.Reverse),
        f(3, 1), f(3, 2, 14)
      )},
      {"GlideStart", new Track(h(l:LoopMode.Once),
        f(4, 0, 5), f(4, 2, 5)
      )},
      {"GlideIdle", new Track(h(),
        f(4, 3)
      )},
      {"Glide", new Track(h(),
        f(4, 4, 5), f(4, 9, 5)
      )},
      {"LookUpStart", new Track(h(),
        f(1, 0)
      )},
      {"LookUp", new Track(h(),
        f(1, 1, 8), f(1, 7, 8)
      )},
      {"TransformEnd", new Track(h(i:InitType.Select),
        f(15, 8, 6), f(15, 9, 50), f(15, 10, 6), f(15, 11, 60),
        f(15, 12, 10), f(15, 13, 40), f(15, 14, 3), f(15, 15, 60)
      )},
      {"Burrow", new Track(h(i:InitType.Range),
        f(7, 0, 3), f(7, 7, 3)
      )},
    };
    
    private static string[] Names = Tracks.Keys.ToArray();
  }
}