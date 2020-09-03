namespace OriMod.Animations {
  /// <summary>
  /// Contains info for a <see cref="Track"/>.
  /// </summary>
  public class Header {
    public Header(LoopMode loop = LoopMode.None, Direction direction = Direction.Forward, string transferTo = null, ReferencedTexture2D rtx = null) {
      this.loop = loop;
      this.direction = direction;
      this.transferTo = transferTo;
      texture = rtx;
    }

    /// <summary>
    /// <inheritdoc cref="LoopMode"/>
    /// </summary>
    public readonly LoopMode loop;

    /// <summary>
    /// <inheritdoc cref="Direction"/>
    /// </summary>
    public readonly Direction direction;

    /// <summary>
    /// Animation track to transfer to, if <see cref="LoopMode.Transfer"/> is used.
    /// </summary>
    public readonly string transferTo;

    /// <summary>
    /// Spritesheet that may be used instead of <see cref="AnimationSource.texture"/>
    /// </summary>
    public readonly ReferencedTexture2D texture = null;

    /// <summary>
    /// Initialize with <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>.
    /// </summary>
    public static Header Default => new Header(LoopMode.Always, Direction.Forward);

    /// <summary>
    /// Initialize with <see cref="LoopMode.None"/> and <see cref="Direction.Forward"/>.
    /// </summary>
    public static Header None => new Header(LoopMode.None, Direction.Forward);
    
    public override string ToString() => $"loop:{loop}, direction:{direction}";
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
  /// Used to determine the direction that frames in a track are played.
  /// </summary>
  public enum Direction : byte {
    /// <summary>
    /// Frames are played forward.
    /// </summary>
    Forward = 0,
    /// <summary>
    /// Frames alternate between playing forward and backwards when reaching their last frames.
    /// </summary>
    PingPong = 1,
    /// <summary>
    /// Frames are played backwards.
    /// </summary>
    Reverse = 2,
  }
}
