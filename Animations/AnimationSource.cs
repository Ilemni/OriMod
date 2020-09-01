using OriMod.Utilities;
using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. Created at startup, used for all players. This class data is accessed by <see cref="Animation"/>.
  /// </summary>
  public class AnimationSource {
    /// <summary>
    /// Creates a new instance of <see cref="AnimationSource"/> with the given track specifications.
    /// </summary>
    /// <param name="texture">Texture path to the default spritesheet.</param>
    /// <param name="x">Tile width of the spritesheet.</param>
    /// <param name="y">Tile height of the spritesheet.</param>
    /// <param name="tracks">All <see cref="Track"/>s that are part of the spritsheet.</param>
    /// <exception cref="System.InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    public AnimationSource(string texture, byte x, byte y, Dictionary<string, Track> tracks) {
      if (Terraria.Main.netMode == Terraria.ID.NetmodeID.Server) {
        throw new System.InvalidOperationException($"{typeof(AnimationSource)} is not allowed to be constructed on servers.");
      }
      this.tracks = tracks;
      this.texture = new ReferencedTexture2D(texture);
      spriteSize = new PointByte(x, y);
    }

    /// <summary>
    /// All <see cref="Track"/>s in the animation set.
    /// </summary>
    public readonly Dictionary<string, Track> tracks;
    
    /// <summary>
    /// Default spritesheet used for animations.
    /// </summary>
    public readonly ReferencedTexture2D texture;
    
    /// <summary>
    /// Size of individual sprites in the spritesheet.
    /// </summary>
    public readonly PointByte spriteSize;

    /// <summary>
    /// Shortcut for accessing <see cref="tracks">.
    /// </summary>
    public Track this[string name] => tracks[name];
  }
}
