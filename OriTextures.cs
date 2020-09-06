namespace OriMod {
  /// <summary>
  /// Stores references to <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> that are used in code.
  /// </summary>
  internal class OriTextures : SingleInstance<OriTextures> {
    private OriTextures() { }

    internal readonly ReferencedTexture2D PlayerPrimary = new ReferencedTexture2D("Animations/PlayerAnim");
    internal readonly ReferencedTexture2D PlayerSecondary = new ReferencedTexture2D("PlayerEffects/OriPlayerSecondary");
    internal readonly ReferencedTexture2D Transform = new ReferencedTexture2D("Animations/TransformAnim");
    internal readonly ReferencedTexture2D Trail = new ReferencedTexture2D("PlayerEffects/OriGlow");
    [System.Obsolete] internal readonly ReferencedTexture2D SoulLink = new ReferencedTexture2D("PlayerEffects/RevSoulLinkSpritesheet");
    internal readonly ReferencedTexture2D BurrowTimer = new ReferencedTexture2D("PlayerEffects/BurrowTimer");
    internal readonly ReferencedTexture2D BashArrow = new ReferencedTexture2D("Animations/BashAnim");
    internal readonly ReferencedTexture2D Feather = new ReferencedTexture2D("Animations/GlideAnim");
    internal readonly ReferencedTexture2D Sein = new ReferencedTexture2D("Projectiles/Minions/Sein_Glow");
  }
}