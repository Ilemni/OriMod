namespace OriMod {
  internal class OriTextures : SingleInstance<OriTextures> {
    private OriTextures() { }

    internal readonly CachedTexture2D PlayerPrimary = new CachedTexture2D("PlayerEffects/OriPlayer");
    internal readonly CachedTexture2D PlayerSecondary = new CachedTexture2D("PlayerEffects/OriPlayerSecondary");
    internal readonly CachedTexture2D Transform = new CachedTexture2D("PlayerEffects/Transform");
    internal readonly CachedTexture2D Trail = new CachedTexture2D("PlayerEffects/OriGlow");
    internal readonly CachedTexture2D SoulLink = new CachedTexture2D("PlayerEffects/RevSoulLinkSpritesheet");
    internal readonly CachedTexture2D BurrowTimer = new CachedTexture2D("PlayerEffects/BurrowTimer");
    internal readonly CachedTexture2D Sein = new CachedTexture2D("Projectiles/Minions/Sein_Glow");
  }
}