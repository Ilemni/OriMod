using AnimLib;

namespace OriMod;

/// <summary>
/// Stores references to <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> that are used in code.
/// </summary>
internal class OriTextures : SingleInstance<OriTextures> {
  private OriTextures() { }

  internal readonly ReferencedTexture2D playerPrimary = new("Animations/PlayerAnim");
  internal readonly ReferencedTexture2D playerSecondary = new("Animations/PlayerAnimSecondary");
  internal readonly ReferencedTexture2D transform = new("Animations/TransformAnim");
  internal readonly ReferencedTexture2D trail = new("PlayerEffects/PlayerTrail");
  internal readonly ReferencedTexture2D burrowTimer = new("PlayerEffects/BurrowTimer");
  internal readonly ReferencedTexture2D bashArrow = new("Animations/BashAnim");
  internal readonly ReferencedTexture2D feather = new("Animations/GlideAnim");
  internal readonly ReferencedTexture2D sein = new("Projectiles/Minions/Sein_Glow");
}
