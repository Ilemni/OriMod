using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OriMod.Utilities;

/// <summary>
/// Class for various utility methods used by OriMod.
/// </summary>
public static class OriUtils {
  /// <summary>
  /// Checks if the given tile is solid.
  /// </summary>
  /// <param name="tile">The <see cref="Tile"/> to check.</param>
  /// <param name="asLiquid">If <see langword="true"/>, bubble blocks are considered solid, and open grates are considered not solid </param>
  public static bool IsSolid(Tile tile, bool asLiquid = false) {
    bool bubbleSolid = Main.tileSolid[TileID.Bubble];
    Liquid.tilesIgnoreWater(asLiquid);
    Main.tileSolid[TileID.Bubble] = asLiquid;

    bool result = tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];

    Liquid.tilesIgnoreWater(false);
    Main.tileSolid[TileID.Bubble] = bubbleSolid;

    return result;
  }

  /// <summary>
  /// Checks if any active <see cref="NPC"/>s are bosses.
  /// </summary>
  public static bool IsAnyBossAlive() => Main.npc.Any(IsBoss);

  private static bool IsBoss(NPC npc) => npc.active && (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]);

  #region Distance Checking

  /// <summary>
  /// Distance between the hitboxes of two entities. If the entity hitboxes overlap, this will return <see langword="0"/>.
  /// </summary>
  /// <param name="entity1">First entity.</param>
  /// <param name="entity2">Second entity.</param>
  /// <returns>The squared value between two entities, -or- <see langword="0"/> if they overlap.</returns>
  internal static float DistanceBetweenTwoEntitiesSquared(Entity entity1, Entity entity2) {
    return DistanceBetweenTwoRectsSquared(entity1.Hitbox, entity2.Hitbox);
  }

  /// <summary>
  /// Distance between two rectangles. If the rectangles overlap, this will return <see langword="0"/>.
  /// </summary>
  /// <param name="rect1">First rectangle.</param>
  /// <param name="rect2">Second rectangle.</param>
  /// <returns>The squared distance between two rectangles, -or- <see langword="0"/> if they overlap.</returns>
  private static float DistanceBetweenTwoRectsSquared(Rectangle rect1, Rectangle rect2) {
    float xAxis =
      rect1.Right < rect2.Left ? rect2.Left - rect1.Right :
      rect2.Right < rect1.Left ? rect1.Left - rect2.Right : 0;
    float yAxis =
      rect1.Bottom < rect2.Top ? rect2.Top - rect1.Bottom :
      rect2.Bottom < rect1.Top ? rect1.Top - rect2.Bottom : 0;
    return xAxis * xAxis + yAxis * yAxis;
  }

  #endregion

  internal static Vector2 GetMouseDirection(Player player, out float angle, Vector2? direction = null,
    float maxAngle = MathF.PI) {
    // Normalize direction, or set to player values if null or cannot normalize
    Vector2 defaultDir = Vector2.Normalize(new Vector2(player.direction, player.gravDir));
    Vector2 dir = direction?.SafeNormalize(defaultDir) ?? defaultDir;

    Vector2 offset = (Main.MouseWorld - player.Center) * dir + player.Center;

    angle = Math.Clamp(player.AngleTo(offset), -Math.Abs(maxAngle), Math.Abs(maxAngle));

    return Vector2.UnitX.RotatedBy(angle) * dir;
  }
}
