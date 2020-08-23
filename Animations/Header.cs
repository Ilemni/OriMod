namespace OriMod.Animations {
  /// <summary>
  /// Contains info for a <see cref="Track"/>.
  /// </summary>
  public class Header {
    public Header(InitType init = InitType.None, LoopMode loop = LoopMode.None, PlaybackMode playback = PlaybackMode.None, string transferTo = null, ReferencedTexture2D rtx = null) {
      this.init = init;
      this.loop = loop;
      this.playback = playback;
      this.transferTo = transferTo;
      texture = rtx;
    }

    /// <summary>
    /// <see cref="InitType"/>
    /// </summary>
    public InitType init;

    /// <summary>
    /// <see cref=" LoopMode"/>
    /// </summary>
    public LoopMode loop;

    /// <summary>
    /// <see cref=" PlaybackMode"/>
    /// </summary>
    public PlaybackMode playback;

    /// <summary>
    /// Animation track to transfer to, if <see cref="LoopMode.Transfer"/> is used.
    /// </summary>
    public readonly string transferTo;

    /// <summary>
    /// Spritesheet that may be used instead of <see cref="AnimationSource.texture"/>
    /// </summary>
    public readonly ReferencedTexture2D texture = null;

    /// <summary>
    /// Copies the <see cref="init"/>, <see cref="loop"/>, and <see cref="playback"/> of <paramref name="other"/>.
    /// </summary>
    /// <param name="other"><see cref="Header"/> to copy.</param>
    internal Header CopySome(Header other) {
      return new Header(
        other.init != 0 ? other.init : init,
        other.loop != 0 ? other.loop : loop,
        other.playback != 0 ? other.playback : playback
      );
    }

    /// <summary>
    /// Initialize with <see cref="InitType.Range"/>, <see cref="LoopMode.Always"/>, <see cref="PlaybackMode.None"/>.
    /// </summary>
    public static Header Default => new Header(InitType.Range, LoopMode.Always, PlaybackMode.None);

    /// <summary>
    /// Initialize with <see cref="InitType.None"/>, <see cref="LoopMode.None"/>, <see cref="PlaybackMode.None"/>.
    /// </summary>
    public static Header None => new Header(InitType.None, LoopMode.None, PlaybackMode.None);
    
    public override string ToString() => $"init={init}, loop={loop}, playback={playback}";
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
  /// Used to determine how a track behaves after its last frame is played.
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
