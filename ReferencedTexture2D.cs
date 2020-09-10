using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// This class exists simply because I'd rather have direct reference to textures.
  /// instead of getting them from a string dictionary using <see cref="ModContent.GetTexture(string)"/>,
  /// multiple times per frame depending on number of textures.
  /// </summary>
  /// <remarks>
  /// Turns out that even though the description of <see cref="Mod.GetTexture(string)"/> says it's shorthand
  /// for <see cref="ModContent.GetTexture(string)"/>, that is false and the reverse is true
  /// (<see cref="ModContent.GetTexture(string)"/> calls <see cref="Mod.GetTexture(string)"/>)
  /// </remarks>
  public class ReferencedTexture2D {
    /// <summary>
    /// Creates a new instance of <see cref="ReferencedTexture2D"/> with the given texture path.
    /// </summary>
    /// <param name="texturePath">Path of the texture to gete. Either relative to <see cref="OriMod"/> or Modloader.</param>
    /// <exception cref="System.InvalidOperationException">Texture classes cannot be constructed on a server.</exception>
    /// <exception cref="System.ArgumentException"><paramref name="texturePath"/> is empty, or is not a valid texture path.</exception>
    public ReferencedTexture2D(string texturePath) {
      if (Main.netMode == NetmodeID.Server) {
        throw new System.InvalidOperationException("Texture classes cannot be constructed on a server.");
      }
      if (string.IsNullOrWhiteSpace(texturePath)) {
        throw new System.ArgumentException($"{nameof(texturePath)} cannot be empty.", nameof(texturePath));
      }

      if (OriMod.Instance.TextureExists(texturePath)) {
        texture = OriMod.Instance.GetTexture(texturePath);
      }
      else if (ModContent.TextureExists(texturePath)) {
        texture = ModContent.GetTexture(texturePath);
      }
      else {
        throw new System.ArgumentException($"{texturePath} is not a valid texture path.", nameof(texturePath));
      }
    }

    /// <summary>
    /// Texture that this instance represents.
    /// </summary>
    public readonly Texture2D texture;

    public static implicit operator Texture2D(ReferencedTexture2D ct) => ct.texture;
    public static explicit operator ReferencedTexture2D(Texture2D tx) => new ReferencedTexture2D(tx.Name);
  }
}
