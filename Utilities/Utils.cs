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
  #region Point Arithmetic
  /// <summary>
  /// Addition between two points.
  /// </summary>
  /// <param name="self">This <see cref="Point"/>.</param>
  /// <param name="point"><see cref="Point"/> to add.</param>
  /// <returns>A <see cref="Point"/> that is the sum of this and <paramref name="point"/>.</returns>
  internal static Point Add(this Point self, Point point) {
    Point p3;
    p3.X = self.X + point.X;
    p3.Y = self.Y + point.Y;
    return p3;
  }
  #endregion

  /// <summary>
  /// Readonly version of Normalize().
  /// </summary>
  internal static Vector2 Normalized(this Vector2 vect) {
    if (vect == Vector2.Zero) {
      return vect;
    }
    vect.Normalize();
    return vect;
  }

  
  /// <summary>
  /// Checks if the given tile is solid.
  /// </summary>
  /// <param name="tile">The <see cref="Tile"/> to check.</param>
  /// <param name="asLiquid">If <see langword="true"/>, bubble blocks are considered solid, and open grates are considered not solid </param>
  public static bool IsSolid(Tile tile, bool asLiquid = false) {
    bool result = false;
    
    bool bubbleSolid = Main.tileSolid[379];
    Liquid.tilesIgnoreWater(asLiquid);
    Main.tileSolid[379] = asLiquid;

    if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) result = true;

    Main.tileSolid[379] = bubbleSolid;
    Liquid.tilesIgnoreWater(false);

    return result;
  }

  /// <summary>
  /// Checks if any active <see cref="NPC"/>s are bosses.
  /// </summary>
  public static bool IsAnyBossAlive()
    => Main.npc.Any(npc => npc.active && (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]));

  #region Distance Checking
  /// <summary>
  /// Distance between the hitboxes of two entities. If the entity hitboxes overlap, this will return <see langword="0"/>.
  /// </summary>
  /// <param name="entity1">First entity.</param>
  /// <param name="entity2">Second entity.</param>
  /// <returns>The squared value between two entities, -or- <see langword="0"/> if they overlap.</returns>
  internal static float DistanceBetweenTwoEntitiesSquared(Entity entity1, Entity entity2) {
    return DistanceBetweenTwoRectsSquared(
      new Rectangle((int)entity1.Left.X, (int)entity1.Top.Y, entity1.width, entity1.height),
      new Rectangle((int)entity2.Left.X, (int)entity2.Top.Y, entity2.width, entity2.height));
  }

  /// <summary>
  /// Distance between two rectangles. If the rectangles overlap, this will return <see langword="0"/>.
  /// </summary>
  /// <param name="rect1">First rectangle.</param>
  /// <param name="rect2">Second rectangle.</param>
  /// <returns>The squared distance between two rectangles, -or- <see langword="0"/> if they overlap.</returns>
  internal static float DistanceBetweenTwoRectsSquared(Rectangle rect1, Rectangle rect2) {
    float xAxis = rect1.Right < rect2.Left ? rect2.Left - rect1.Right : rect2.Right < rect1.Left ? rect1.Left - rect2.Right : 0;
    float yAxis = rect1.Bottom < rect2.Top ? rect2.Top - rect1.Bottom : rect2.Bottom < rect1.Top ? rect1.Top - rect2.Bottom : 0;
    return (float)(Math.Pow(xAxis, 2) + Math.Pow(yAxis, 2));
  }
  #endregion

  internal static Vector2 GetMouseDirection(OriPlayer oPlayer, out float angle, Vector2? direction = null, float maxAngle = (float)Math.PI) {
    Player player = oPlayer.Player;
    Vector2 dir = direction ?? new Vector2(player.direction, player.gravDir);

    Vector2 offset = Main.MouseWorld;
    offset = (offset - player.Center) * dir + player.Center;

    angle = Utils.Clamp(player.AngleTo(offset), -maxAngle, maxAngle);

    Vector2 result = Vector2.UnitX.RotatedBy(angle);
    result *= dir;
    return result;
  }

  /// <summary>
  /// Linear interpoation between two <paramref name="f1"/> and <paramref name="f2"/> by <paramref name="by"/>.
  /// </summary>
  /// <param name="f1">First point.</param>
  /// <param name="f2">Second point.</param>
  /// <param name="by">Weight between the two, between <see langword="0"/> and <see langword="1"/>.</param>
  /// <returns>A value between <paramref name="f1"/> and <paramref name="f2"/>.</returns>
  internal static float Lerp(float f1, float f2, float by) => f1 * (1 - by) + f2 * by;    
}
