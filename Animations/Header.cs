using Microsoft.Xna.Framework.Graphics;
using System;

namespace OriMod.Animations {
  internal class Header : IDisposable {
    internal InitType Init;
    internal LoopMode Loop;
    internal PlaybackMode Playback;
    internal Texture2D Texture => !_tex?.IsDisposed ?? false ? _tex : TexturePath != null ? _tex = OriMod.Instance.GetTexture(TexturePath) : null;
    private Texture2D _tex;
    private string TexturePath;
    internal string TransferTo { get; private set; }

    internal Header(InitType init = InitType.None, LoopMode loop = LoopMode.None, PlaybackMode playback = PlaybackMode.None, string transferTo = null, string overrideTexturePath = null) {
      Init = init;
      Loop = loop;
      Playback = playback;
      TexturePath = overrideTexturePath;
    }

    internal Header CopySome(Header other) {
      return new Header(
        other.Init != 0 ? other.Init : Init,
        other.Loop != 0 ? other.Loop : Loop,
        other.Playback != 0 ? other.Playback : Playback
      );
    }

    internal static Header Default => new Header(InitType.Range, LoopMode.Always, PlaybackMode.Normal);
    internal static Header None => new Header(InitType.None, LoopMode.None, PlaybackMode.None);
    public override string ToString()
      => $"Init: {Init} | Loop: {Loop} | Playback: {Playback}" + (Texture != null ? $" | Texture Path: \"{Texture.Name}\"" : "");

    public void Dispose() {
      if (_tex != null) {
        _tex.Dispose();
        _tex = null;
      }
      TexturePath = null;
      TransferTo = null;
    }
  }

  internal enum InitType {
    None = 0,
    Range = 1,
    Select = 2,
  }
  internal enum LoopMode {
    None = 0,
    Always = 1,
    Once = 2,
    Transfer = 3,
  }
  internal enum PlaybackMode {
    None = 0,
    Normal = 1,
    PingPong = 2,
    Reverse = 3,
    Random = 4,
  }
}
