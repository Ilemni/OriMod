using System;

namespace OriMod {
  internal class OriTextures : IDisposable {
    private OriTextures() { }

    internal static OriTextures Instance => _instance ?? (_instance = new OriTextures());
    private static OriTextures _instance;

    internal readonly CachedTexture2D PlayerPrimary = new CachedTexture2D("PlayerEffects/OriPlayer");
    internal readonly CachedTexture2D PlayerSecondary = new CachedTexture2D("PlayerEffects/OriPlayerSecondary");
    internal readonly CachedTexture2D Trail = new CachedTexture2D("PlayerEffects/OriGlow");
    internal readonly CachedTexture2D SoulLink = new CachedTexture2D("PlayerEffects/RevSoulLinkSpritesheet");
    internal readonly CachedTexture2D BurrowTimer = new CachedTexture2D("PlayerEffects/BurrowTimer");
    internal readonly CachedTexture2D Sein = new CachedTexture2D("Projectiles/Minions/Sein_Glow");

    public void Dispose() {
    }
  }
}