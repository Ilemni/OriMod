using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Animation Track created on startup, contains <see cref="Frame"/> data.
  /// </summary>
  public class Track {
    /// <summary>
    /// Creates a track with <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(Frame start, Frame end) => Range(LoopMode.Always, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and using <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(LoopMode loopMode, Frame start, Frame end) => Range(loopMode, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and <see cref="Direction"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(LoopMode loopMode, Direction direction, Frame start, Frame end) {
      // Fill range of frames
      // I.e. if given [(0,1), (0,4)], we make [(0,1), (0,2), (0,3), (0,4)]
      var frames = new List<Frame>();
      for (int y = start.tile.Y; y < end.tile.Y; y++) {
        frames.Add(new Frame(start.tile.X, y, start.duration));
      }
      frames.Add(end);
      return new Track(loopMode, direction, frames.ToArray());
    }

    /// <summary>
    /// Creates a track that consists of a single <see cref="Frame"/>.
    /// </summary>
    /// <param name="frame">Assigns to <see cref="frames"/> as a single <see cref="Frame"/>.</param>
    public static Track Single(Frame frame) => new Track(new[] { frame });

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction"/>, and <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param>
    public Track(LoopMode loopMode, Direction direction, Frame[] frames) {
      loop = loopMode;
      this.direction = direction;
      this.frames = frames;
    }

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction.Forward"/>, and the given <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param>
    public Track(LoopMode loopMode, Frame[] frames) : this(loopMode, Direction.Forward, frames) { }

    /// <summary>
    /// Creates a track using <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, and with the given <see cref="Frame"/> array.
    /// </summary>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param> 
    public Track(Frame[] frames) : this(LoopMode.Always, Direction.Forward, frames) { }

    
    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] frames;

    /// <inheritdoc cref="LoopMode"/>
    public readonly LoopMode loop = LoopMode.Always;

    /// <inheritdoc cref="Direction"/>
    public readonly Direction direction = Direction.Forward;

    /// <summary>
    /// Optional spritesheet that may be used instead of <see cref="AnimationSource.texture"/>.
    /// </summary>
    public ReferencedTexture2D Texture { get; private set; }

    /// <summary>
    /// Assign a spritesheet that will be used instead of <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Track WithTexture(ReferencedTexture2D texture) {
      Texture = texture;
      return this;
    }

    /// <summary>
    /// Animation track to transfer to, if <see cref="LoopMode.Transfer"/> is used.
    /// </summary>
    // Use: /// <param name="transferTo">Track to transfer to. Requires <paramref name="loop"/> to be <see cref="LoopMode.Transfer"/>.</param>
    public readonly string transferTo;
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
