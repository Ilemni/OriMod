using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// This class exists simply because I'd rather have direct reference to textures
  /// instead of getting them from a string dictionary using ModContent.GetTexture(),
  /// multiple times per frame depending on number of textures.
  /// Is all of this setup, plus having to manage texture disposal on my own, worth it?
  /// Just to avoid a string dictionary?
  /// Likely not.
  /// </summary>
  /// <remarks>
  /// Turns out that even though the description of mod.GetTexture() says it's shorthand
  /// for ModContent.GetTexture(), turns out that that is false and the reverse is true
  /// </remarks>
  public class CachedTexture2D {
    public CachedTexture2D(string texturePath) {
      if (texturePath == null) {
        throw new System.ArgumentNullException(nameof(texturePath));
      }
      
      if (OriMod.Instance.TextureExists(texturePath)) {
        thisMod = true;
      }
      else if (ModContent.TextureExists(texturePath)) {
        thisMod = false;
      }
      else {
        throw new System.ArgumentException($"{texturePath} is not a valid texture path.", nameof(texturePath));
      }
      
      TexturePath = texturePath;
    }

    public readonly string TexturePath;

    private Texture2D texture;

    /// <summary>
    /// Indicates usage of MyMod.Instance.GetTexture() vs ModContent.GetTexture()
    /// </summary>
    private readonly bool thisMod;

    public static implicit operator Texture2D(CachedTexture2D ct) => ct.GetTexture();
    public static implicit operator CachedTexture2D(Texture2D tx) => new CachedTexture2D(tx.Name);

    public Texture2D GetTexture() {
      if (texture == null || texture.IsDisposed) {
        texture = thisMod ? OriMod.Instance.GetTexture(TexturePath) : ModContent.GetTexture(TexturePath);
      }
      return texture;
    }

    public void DisposeTexture() {
      texture?.Dispose();
      texture = null;
    }
  }
}
