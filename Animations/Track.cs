using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Animation Track created on startup, contains <see cref="Frame"/> data.
  /// </summary>
  public class Track {
    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction"/>, and <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="asRange">Whether to use <paramref name="frames"/> as-is (false) or populate the range between frames (true).</param>
    /// <param name="frames">Assigns to <see cref="frames"/>. Used instead as a range if <paramref name="asRange"/> is <c>true</c>.</param>
    public Track(LoopMode loopMode, Direction direction, bool asRange, params Frame[] frames) {
      loop = loopMode;
      this.direction = direction;

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
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction.Forward"/>, and the given <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="asRange">Whether to use <paramref name="frames"/> as-is (false) or populate the range between frames (true).</param>
    /// <param name="frames">Assigns to <see cref="frames"/>. Used instead as a range if <paramref name="asRange"/> is <c>true</c>.</param>
    public Track(LoopMode loopMode, bool asRange, params Frame[] frames) : this(loopMode, Direction.Forward, asRange, frames) { }

    /// <summary>
    /// Creates a track using <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, and with the given <see cref="Frame"/> array. The array may instead be used as range parameters, if desired.
    /// </summary>
    /// <param name="asRange">Whether to use <paramref name="frames"/> as-is (false) or populate the range between frames (true).</param>
    /// <param name="frames">Assigns to <see cref="frames"/>. Used instead as a range if <paramref name="asRange"/> is <c>true</c>.</param>
    public Track(bool asRange, params Frame[] frames) : this(LoopMode.Always, Direction.Forward, asRange, frames) { }

    /// <summary>
    /// Creates a track that consists of a single <see cref="Frame"/>.
    /// </summary>
    /// <param name="frame">Assigns to <see cref="frames"/> as a single <see cref="Frame"/>.</param>
    public Track(Frame frame) {
      frames = new[] { frame };
    }


    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly Frame[] frames;

    /// <inheritdoc cref="LoopMode"/>
    public readonly LoopMode loop = LoopMode.Always;

    /// <inheritdoc cref="Direction"/>
    public readonly Direction direction = Direction.Forward;

    /// <summary>
    /// Optional spritesheet that may be used instead of <see cref="AnimationSource.texture"/>
    /// </summary>
    public ReferencedTexture2D Texture { get; private set; }

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
