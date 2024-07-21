using AnimLib;
using AnimLib.Animations;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Stores references to Aseprite spritesheets and their TextureAtlases.
/// </summary>
internal class OriTextures : SingleInstance<OriTextures> {
  private OriTextures() {
    PlayerSprites = OriMod.instance.Assets.Request<AnimSpriteSheet>("Animations/PlayerAnim", AssetRequestMode.ImmediateLoad).Value;
    Arrow = OriMod.instance.Assets.Request<AnimSpriteSheet>("Animations/BashAnim", AssetRequestMode.ImmediateLoad).Value;

    PlayerPrimary = PlayerSprites.Atlases["Primary"];
    PlayerSecondary = PlayerSprites.Atlases["Secondary"];
    Transform = PlayerSprites.Atlases["Transform"];
    Feather = PlayerSprites.Atlases["Feather"];
    Trail = PlayerSprites.Atlases["AfterImage"];
  }

  internal readonly AnimSpriteSheet PlayerSprites;
  internal readonly AnimSpriteSheet Arrow;
  internal readonly AnimTextureAtlas PlayerPrimary;
  internal readonly AnimTextureAtlas PlayerSecondary;
  internal readonly AnimTextureAtlas Transform;
  internal readonly AnimTextureAtlas Trail;
  internal readonly AnimTextureAtlas Feather;

  internal readonly Asset<Texture2D> burrowTimer = ModContent.Request<Texture2D>("OriMod/PlayerEffects/BurrowTimer");
  internal readonly Asset<Texture2D> sein = ModContent.Request<Texture2D>("OriMod/Projectiles/Minions/Sein_Glow");
}
