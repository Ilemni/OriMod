using OriMod.Utilities;
using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Contains all animation data for a single animation set. Created at startup, used for all players.
  /// </summary>
  public class AnimationSource {
    public AnimationSource(string texture, byte x, byte y, Dictionary<string, Track> tracks) {
      this.tracks = tracks;
      this.texture = new ReferencedTexture2D(texture);
      spriteSize = new PointByte(x, y);
    }

    /// <summary>
    /// All <see cref="Track"/>s in the animation set.
    /// </summary>
    public readonly Dictionary<string, Track> tracks;
    
    /// <summary>
    /// Spritesheet used for the animations.
    /// </summary>
    public readonly ReferencedTexture2D texture;
    
    /// <summary>
    /// Size of the sprites in the animations
    /// </summary>
    public readonly PointByte spriteSize;
    
    /// <summary>
    /// Shortcut for accessing <see cref="tracks">
    /// </summary>
    public Track this[string name] => tracks[name];
  }
}
