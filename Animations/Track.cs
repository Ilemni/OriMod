using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

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
    public static Track Range(IFrame start, IFrame end) => Range(LoopMode.Always, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and using <see cref="Direction.Forward"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(LoopMode loopMode, IFrame start, IFrame end) => Range(loopMode, Direction.Forward, start, end);

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/> and <see cref="Direction"/>, with a <see cref="Frame"/> array ranging from <paramref name="start"/> to <paramref name="end"/>.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="start">First <see cref="Frame"/> of the track.</param>
    /// <param name="end">Last <see cref="Frame"/> of the track.</param>
    /// <returns>A new <see cref="Track"/> with the frames ranging from <paramref name="start"/> to <paramref name="end"/>.</returns>
    public static Track Range(LoopMode loopMode, Direction direction, IFrame start, IFrame end) {
      // Fill range of frames
      // I.e. if given [(0,1), (0,4)], we make [(0,1), (0,2), (0,3), (0,4)]
      var frames = new List<IFrame>();
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
    public static Track Single(Frame frame) => new Track(new IFrame[] { frame });

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction"/>, and <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="direction"><see cref="Direction"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param>
    public Track(LoopMode loopMode, Direction direction, IFrame[] frames) {
      loop = loopMode;
      this.direction = direction;
      this.frames = frames;
      foreach (var frame in frames) {
        if (frame is SwitchTextureFrame) {
          multiTexture = true;
          break;
        }
      }
    }

    /// <summary>
    /// Creates a track with the given <see cref="LoopMode"/>, <see cref="Direction.Forward"/>, and the given <see cref="Frame"/> array. This may be used as range parameters instead, if desired.
    /// </summary>
    /// <param name="loopMode"><see cref="LoopMode"/> of the track.</param>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param>
    public Track(LoopMode loopMode, IFrame[] frames) : this(loopMode, Direction.Forward, frames) { }

    /// <summary>
    /// Creates a track using <see cref="LoopMode.Always"/> and <see cref="Direction.Forward"/>, and with the given <see cref="Frame"/> array.
    /// </summary>
    /// <param name="frames">Assigns to <see cref="frames"/>.</param> 
    public Track(IFrame[] frames) : this(LoopMode.Always, Direction.Forward, frames) { }

    
    /// <summary>
    /// All frames used for this track.
    /// </summary>
    public readonly IFrame[] frames;

    /// <inheritdoc cref="LoopMode"/>
    public readonly LoopMode loop = LoopMode.Always;

    /// <inheritdoc cref="Direction"/>
    public readonly Direction direction = Direction.Forward;

    /// <summary>
    /// Assigned to <see langword="true"/> if any <see cref="IFrame"/> in <see cref="frames"/> is a <see cref="SwitchTextureFrame"/> (from <see cref="Frame.WithNextSpritesheet(Texture2D)"/>).
    /// </summary>
    public readonly bool multiTexture;

    /// <summary>
    /// Optional spritesheet that may be used instead of <see cref="AnimationSource.texture"/>.
    /// <para>If any frame after or including the current frame (at <paramref name="frameIdx"/>) is a <see cref="SwitchTextureFrame"/>, that <see cref="Texture2D"/> will be returned instead.</para>
    /// </summary>
    /// <param name="frameIdx">Index of the <see cref="IFrame"/> currently being played.</param>
    /// <returns>A valid <see cref="Texture2D"/> if <see cref="AnimationSource.texture"/> should be overridden, else <see langword="null"/>.</returns>
    public Texture2D GetTexture(int frameIdx) {
      if (frameIdx < 0 || frameIdx >= frames.Length) {
        throw new System.ArgumentOutOfRangeException(nameof(frameIdx), $"Expected value between 0 and {frames.Length - 1}, got {frameIdx}");
      }
      if (multiTexture) {
        while (frameIdx >= 0) {
          if (frames[frameIdx] is SwitchTextureFrame stf) {
            return stf.texture;
          }
        }
      }
      return trackTexture;
    }

    /// <summary>
    /// Texture of the track itself. If any <see cref="SwitchTextureFrame"/>s are in use, use <see cref="GetTexture(int)"/> instead.
    /// </summary>
    public Texture2D trackTexture { get; private set; }
    
    /// <summary>
    /// Assign a spritesheet that will be used instead of <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Track WithTexture(Texture2D texture) {
      this.trackTexture = texture;
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
