namespace OriMod.Animations {
  public class Header {
    public Header(InitType init = InitType.None, LoopMode loop = LoopMode.None, PlaybackMode playback = PlaybackMode.None, string transferTo = null, ReferencedTexture2D rtx = null) {
      Init = init;
      Loop = loop;
      Playback = playback;
      TransferTo = transferTo;
      Texture = rtx;
    }

    public InitType Init;
    public LoopMode Loop;
    public PlaybackMode Playback;
    public readonly string TransferTo;
    public readonly ReferencedTexture2D Texture = null;

    internal Header CopySome(Header other) {
      return new Header(
        other.Init != 0 ? other.Init : Init,
        other.Loop != 0 ? other.Loop : Loop,
        other.Playback != 0 ? other.Playback : Playback
      );
    }

    public static Header Default => new Header(InitType.Range, LoopMode.Always, PlaybackMode.None);
    public static Header None => new Header(InitType.None, LoopMode.None, PlaybackMode.None);
    public override string ToString()
      => $"Init: {Init} | Loop: {Loop} | Playback: {Playback}";
  }

  public enum InitType : byte {
    None = 0,
    Range = 1,
  }
  public enum LoopMode : byte {
    None = 0,
    Always = 1,
    Transfer = 2,
  }
  public enum PlaybackMode : byte {
    None = 0,
    PingPong = 1,
    Reverse = 2,
  }
}
