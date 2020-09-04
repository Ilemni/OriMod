using Microsoft.Xna.Framework.Graphics;
using OriMod.Utilities;

namespace OriMod.Animations {
  /// <inheritdoc cref="IFrame"/>
  public readonly struct Frame : IFrame {
    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, and frame duration to play. These values will be cast to smaller data types.
    /// </summary>
    /// <param name="x">X position of the tile. This will be cast to a byte.</param>
    /// <param name="y">Y position of the tile. This will be cast to a byte.</param>
    /// <param name="duration">Duration of the frame. This will be cast to a short.</param>
    public Frame(int x, int y, int duration = -1) : this((byte)x, (byte)y, (short)duration) { }

    /// <summary>
    /// Creates a <see cref="Frame"/> with the given X and Y position, and frame duration to play.
    /// </summary>
    /// <param name="x">X position of the tile.</param>
    /// <param name="y">Y position of the tile.</param>
    /// <param name="duration">Duration of the frame.</param>
    public Frame(byte x, byte y, short duration = -1) {
      tile = new PointByte(x, y);
      this.duration = duration;
    }
    
    public PointByte tile { get; }

    public short duration { get; }

    /// <summary>
    /// For a <see cref="Track"/>, adds another <see cref="Texture2D"/> to the track, and switches to that texture when this track is played.
    /// </summary>
    /// <remarks>
    /// This should only ever be used if a single <strong><see cref="Track"/></strong> needs to use more than one spritesheet.
    /// If all of one <see cref="Track"/> can fit on a 2048x2048 spritesheet, use <see cref="Track.WithTexture(Texture2D)"/> instead.
    /// </remarks>
    public SwitchTextureFrame WithNextSpritesheet(Texture2D texture) {
      if (texture is null) {
        throw new System.ArgumentNullException(nameof(texture));
      }

      return new SwitchTextureFrame(tile.X, tile.Y, duration, texture);
    }

    public override string ToString() => $"x:{tile.X}, y:{tile.Y}, duration:{duration}";
  }
}
