using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod; 

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
  /// <param name="texturePath">Path of the texture to get. Either relative to <see cref="OriMod"/> or ModLoader.</param>
  /// <exception cref="System.InvalidOperationException">Texture classes cannot be constructed on a server.</exception>
  /// <exception cref="System.ArgumentException"><paramref name="texturePath"/> is empty, or is not a valid texture path.</exception>
  public ReferencedTexture2D(string texturePath) {
    if (Main.netMode == NetmodeID.Server) {
      throw new InvalidOperationException("Texture classes cannot be constructed on a server.");
    }
    if (string.IsNullOrWhiteSpace(texturePath)) {
      throw new ArgumentException($"{nameof(texturePath)} cannot be empty.", nameof(texturePath));
    }

    if (OriMod.instance.HasAsset(texturePath)) {
      _texture = OriMod.instance.Assets.Request<Texture2D>(texturePath);
    }
    else if (ModContent.HasAsset(texturePath)) {
      _texture = ModContent.Request<Texture2D>(texturePath);
    }
    else {
      throw new ArgumentException($"{texturePath} is not a valid texture path.", nameof(texturePath));
    }
  }
        
  private readonly Asset<Texture2D> _texture;

  /// <summary>
  /// Texture loading is async so you can check if it is loaded, it is not required
  /// </summary>
  public bool ready => _texture.IsLoaded;

  /// <summary>
  /// Texture that this instance represents.
  /// </summary>
  public Texture2D texture { get { 
      if(ready) return _texture.Value;
      _texture.Wait();
      return _texture.Value;
    }
  }

  public static implicit operator Texture2D(ReferencedTexture2D ct) => ct.texture;
  public static explicit operator ReferencedTexture2D(Texture2D tx) => new ReferencedTexture2D(tx.Name);
}