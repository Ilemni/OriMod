namespace OriMod {
  /// <summary>
  /// Stores references to <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> that are used in code.
  /// </summary>
  internal class OriTextures : SingleInstance<OriTextures> {
    private OriTextures() { }

    internal readonly ReferencedTexture2D PlayerPrimary = new ReferencedTexture2D("PlayerEffects/OriPlayer");
    internal readonly ReferencedTexture2D PlayerSecondary = new ReferencedTexture2D("PlayerEffects/OriPlayerSecondary");
    internal readonly ReferencedTexture2D Transform = new ReferencedTexture2D("PlayerEffects/Transform");
    internal readonly ReferencedTexture2D Trail = new ReferencedTexture2D("PlayerEffects/OriGlow");
    internal readonly ReferencedTexture2D SoulLink = new ReferencedTexture2D("PlayerEffects/RevSoulLinkSpritesheet");
    internal readonly ReferencedTexture2D BurrowTimer = new ReferencedTexture2D("PlayerEffects/BurrowTimer");
    internal readonly ReferencedTexture2D Sein = new ReferencedTexture2D("Projectiles/Minions/Sein_Glow");
  }
}