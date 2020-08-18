using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OriMod.Animations {
  internal class AnimationSource {
    internal Dictionary<string, Track> Tracks { get; private set; }
    internal Point TileSize { get; }
    public readonly ReferencedTexture2D texture;
    internal readonly string texturePath;
    internal string[] TrackNames { get; private set; }
    internal Track this[string name] => Tracks[name];

    internal AnimationSource(string texture, int x, int y, Dictionary<string, Track> tracks) {
      texturePath = texture;
      Tracks = tracks;
      TrackNames = tracks.Keys.ToArray();
      TileSize = new Point(x, y);
      Texture = new CachedTexture2D(texturePath);
    }
  }
}
