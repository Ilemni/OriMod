using AnimLib;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace OriMod.Tiles;

/// <summary>
/// Used for draw effects, specifically brightening solid areas when the player uses <see cref="Burrow"/>.
/// </summary>
[UsedImplicitly]
public sealed class BurrowGlobalWall : GlobalWall {
  private static int InnerRange => 4;
  private static int OuterRange => 13;

  public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b) {
    if (Main.LocalPlayer.GetState<Burrow>() is not { Active: true } burrow) {
      return;
    }

    Tile tile = Main.tile[i, j];
    float lightStrength = Burrow.CanBurrow(tile, burrow) ? 0.8f : 0.4f;

    float dist = (Main.LocalPlayer.Center / 16 - new Vector2(i, j)).Length();
    if (dist < InnerRange) {
      r = MathHelper.Clamp(r + lightStrength, r, 1f);
      g = MathHelper.Clamp(g + lightStrength, g, 1f);
      b = MathHelper.Clamp(b + lightStrength, b, 1f);
    }
    else if (dist < OuterRange) {
      float lerp = 1 - (dist - InnerRange) / (OuterRange - InnerRange);
      r = float.Lerp(r, lightStrength, lerp);
      g = float.Lerp(g, lightStrength, lerp);
      b = float.Lerp(b, lightStrength, lerp);
    }
    else {
      r = MathHelper.Clamp(r, 0.02f, r);
      g = MathHelper.Clamp(g, 0.02f, g);
      b = MathHelper.Clamp(b, 0.02f, b);
    }
  }
}

/// <summary>
/// Draws hitbox tiles for <see cref="Burrow"/>,
/// representing burrow entry position in green,
/// and player burrow hitbox in red.
/// </summary>
[UsedImplicitly]
public sealed class BurrowGlobalTile : GlobalTile {
  public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) {
    if (!AnimLibMod.DebugEnabled) {
      return;
    }

    Tile tile = Main.tile[i, j];
    if (!tile.HasUnactuatedTile || !Main.tileSolid[type]) {
      return;
    }

    Burrow burrow = Main.LocalPlayer.GetState<Burrow>();

    Point pos = new(i, j);
    Vector2 screenPos = pos.ToWorldCoordinates() - Main.screenPosition + new Vector2(182, 182);
    Rectangle sourceRect = new(0, 0, 16, 16);
    if (burrow.Active && burrow.InnerHitbox.Contains(pos)) {
      spriteBatch.Draw(TextureAssets.MagicPixel.Value, screenPos, sourceRect, Color.Red);
    }
    if ((burrow.Active || burrow.InactiveTime < 90) && burrow.EnterHitbox.Contains(pos)) {
      spriteBatch.Draw(TextureAssets.MagicPixel.Value, screenPos, sourceRect, Color.Green);
    }
  }
}
