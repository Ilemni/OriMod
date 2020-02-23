using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OriMod.Animations {
  internal class AnimationSource : IDisposable {
    internal Dictionary<string, Track> Tracks { get; private set; }
    internal Point TileSize { get; }
    internal readonly CachedTexture2D Texture;
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

    public void Dispose() {
      if (Tracks != null) {
        foreach (var track in Tracks.Values) {
          track.Dispose();
        }
        Tracks = null;
      }
      Texture.DisposeTexture();
      TrackNames = null;
    }
  }
}
