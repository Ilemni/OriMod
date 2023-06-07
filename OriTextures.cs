using AnimLib;

namespace OriMod; 

/// <summary>
/// Stores references to <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> that are used in code.
/// </summary>
internal class OriTextures : SingleInstance<OriTextures> {
  private OriTextures() { }

  internal readonly ReferencedTexture2D playerPrimary = new ReferencedTexture2D("Animations/PlayerAnim");
  internal readonly ReferencedTexture2D playerSecondary = new ReferencedTexture2D("Animations/PlayerAnimSecondary");
  internal readonly ReferencedTexture2D transform = new ReferencedTexture2D("Animations/TransformAnim");
  internal readonly ReferencedTexture2D trail = new ReferencedTexture2D("PlayerEffects/PlayerTrail");
  //[Obsolete] internal readonly ReferencedTexture2D soulLink; // = new ReferencedTexture2D("PlayerEffects/RevSoulLinkSpritesheet");
  internal readonly ReferencedTexture2D burrowTimer = new ReferencedTexture2D("PlayerEffects/BurrowTimer");
  internal readonly ReferencedTexture2D bashArrow = new ReferencedTexture2D("Animations/BashAnim");
  internal readonly ReferencedTexture2D feather = new ReferencedTexture2D("Animations/GlideAnim");
  internal readonly ReferencedTexture2D sein = new ReferencedTexture2D("Projectiles/Minions/Sein_Glow");
}
