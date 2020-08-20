namespace OriMod.Animations {
  /// <summary>
  /// Contains info for a track
  /// </summary>
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

    /// <summary>
    /// Copies the <see cref="Init"/>, <see cref="Loop"/>, and <see cref="Playback"/> of <paramref name="other"/>.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    internal Header CopySome(Header other) {
      return new Header(
        other.Init != 0 ? other.Init : Init,
        other.Loop != 0 ? other.Loop : Loop,
        other.Playback != 0 ? other.Playback : Playback
      );
    }

    /// <summary>
    /// Initialize with <see cref="InitType.Range"/>, <see cref="LoopMode.Always"/>, <see cref="PlaybackMode.None"/>.
    /// </summary>
    public static Header Default => new Header(InitType.Range, LoopMode.Always, PlaybackMode.None);
    public static Header None => new Header(InitType.None, LoopMode.None, PlaybackMode.None);
    public override string ToString()
      => $"Init: {Init} | Loop: {Loop} | Playback: {Playback}";
  }

  /// <summary>
  /// Used to determine how to construct <see cref="Frame"/> array on <see cref="Track"/> creation.
  /// </summary>
  public enum InitType : byte {
    /// <summary>
    /// The <see cref="Frame"/> array will be used as-is.
    /// </summary>
    None = 0,
    /// <summary>
    /// Any missing sprites between two specified frames will be automatically inserted.
    /// </summary>
    Range = 1,
  }
  
  /// <summary>
  /// Used to determine how a track behaves after its last frame.
  /// </summary>
  public enum LoopMode : byte {
    /// <summary>
    /// When the last frame ends, the animation remains on the last frame until the track changes.
    /// </summary>
    None = 0,
    /// <summary>
    /// When the last frame ends, the animation loops back to the start of the track.
    /// </summary>
    Always = 1,
    /// <summary>
    /// When the last frame ends, the animation plays the specified track.
    /// </summary>
    Transfer = 2,
  }
  
  /// <summary>
  /// Used to determine overall playback behavior.
  /// </summary>
  public enum PlaybackMode : byte {
    /// <summary>
    /// Playback is normal.
    /// </summary>
    None = 0,
    /// <summary>
    /// This track plays in reverse after ending the last frame.
    /// </summary>
    PingPong = 1,
    /// <summary>
    /// This track is played in reverse.
    /// </summary>
    Reverse = 2,
  }
}
