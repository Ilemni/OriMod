using AnimLib;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Stores references to <see cref="Microsoft.Xna.Framework.Graphics.Texture2D"/> that are used in code.
/// </summary>
internal class OriTextures : SingleInstance<OriTextures> {
  private OriTextures() { }

  internal readonly Asset<Texture2D> playerPrimary = ModContent.Request<Texture2D>("OriMod/Animations/PlayerAnim");
  internal readonly Asset<Texture2D> transform = ModContent.Request<Texture2D>("OriMod/Animations/TransformAnim");
  internal readonly Asset<Texture2D> trail = ModContent.Request<Texture2D>("OriMod/PlayerEffects/PlayerTrail");
  internal readonly Asset<Texture2D> burrowTimer = ModContent.Request<Texture2D>("OriMod/PlayerEffects/BurrowTimer");
  //internal readonly Asset<Texture2D> bashArrow = ModContent.Request<Texture2D>("OriMod/Animations/BashAnim");
  //internal readonly Asset<Texture2D> feather = ModContent.Request<Texture2D>("OriMod/Animations/GlideAnim");
  internal readonly Asset<Texture2D> sein = ModContent.Request<Texture2D>("OriMod/Projectiles/Minions/Sein_Glow");
  internal readonly Asset<Texture2D> playerSecondary = ModContent.Request<Texture2D>("OriMod/Animations/PlayerAnimSecondary");
}
