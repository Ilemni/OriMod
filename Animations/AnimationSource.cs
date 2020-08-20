using OriMod.Utilities;
using System.Collections.Generic;

namespace OriMod.Animations {
  public class AnimationSource {
    public AnimationSource(string texture, byte x, byte y, Dictionary<string, Track> tracks) {
      this.tracks = tracks;
      this.texture = new ReferencedTexture2D(texture);
      spriteSize = new PointByte(x, y);
    }

    public readonly Dictionary<string, Track> tracks;
    public readonly ReferencedTexture2D texture;
    public readonly PointByte spriteSize;

    public Track this[string name] => tracks[name];
  }
}
