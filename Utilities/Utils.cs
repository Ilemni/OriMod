using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Utilities {
  /// <summary>
  /// Class for various utility methods used by OriMod.
  /// </summary>
  public static class OriUtils {
    #region Point Arithmetic
    /// <summary>
    /// Addition between two points.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="point">Point to add.</param>
    /// <returns></returns>
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
    /// Checks if any active <see cref="NPC"/>s are bosses.
    /// </summary>
    public static bool AnyBossAlive() {
      for (int i = 0, len = Main.npc.Length; i < len; i++) {
        var npc = Main.npc[i];
        if (npc.active && (npc.boss || Terraria.ID.NPCID.Sets.TechnicallyABoss[npc.type])) {
          return true;
        }
      }
      return false;
    }

    #region Distance Checking
    /// <summary>
    /// Gets the closest <see cref="Entity"/>. Returns true if any are in range, the closest <see cref="Entity"/> passed out as <paramref name="entity"/>.
    /// </summary>
    /// <param name="me">Y'know...</param>
    /// <param name="arr">Array of entities, such as <see cref="Main.player"/>, <see cref="Main.npc"/>, <see cref="Main.projectile"/>. These are the candidates for <paramref name="entity"/>.</param>
    /// <param name="distance">Maximum distance to be considered in-range. If passed in as 0 or negative, range is infinite.
    /// <para>If <paramref name="entity"/> is not null, this value is the distance to that entity.</para>
    /// <para>If <paramref name="entity"/> is null, this value is not modified.</para>
    /// </param>
    /// <param name="entity">Closest <see cref="Entity"/> in-range, or null if no entities are in range.</param>
    /// <param name="distanceSquaredCheck">How the closest distance is checked. Defaults to `<see cref="DistanceShortSquared(Entity, Entity)"/>`, getting closest distance between two entities.</param>
    /// <param name="condition">Extra condition to filter out entities. If <paramref name="condition"/> returns false, the entity is skipped.</param>
    /// <typeparam name="T">Type of Entity (i.e. <see cref="Player"/>, <see cref="NPC"/>, <see cref="Projectile"/>).</typeparam>
    /// <returns><c>true</c> if there is an <see cref="Entity"/> closer than <paramref name="distance"/> (<paramref name="entity"/> is not null), else <c>false</c>.</returns>
    internal static bool GetClosestEntity<T>(this Entity me, T[] arr, ref float distance, out T entity, Func<Entity, T, float> distanceSquaredCheck = null, Func<T, bool> condition = null) where T : Entity {
      // Setup method
      float startDistance = distance;
      if (distance <= 0) {
        distance = float.MaxValue; // Infinite range detect
      }
      else {
        distance *= distance; // Squared for DistanceSquared
      }
      if (distanceSquaredCheck is null) {
        distanceSquaredCheck = (e1, e2) => DistanceBetweenTwoEntitiesSquared(e1, e2);
      }

      // Search for closest entity
      int id = -1;
      for (int i = 0, len = arr.Length; i < len; i++) {
        T e = arr[i];
        if (e is null || !e.active || ReferenceEquals(e, me) || condition?.Invoke(e) == false) {
          continue;
        }

        float newDist = distanceSquaredCheck(me, e);
        if (newDist < distance) {
          distance = newDist;
          id = e.whoAmI;
        }
      }

      if (id != -1) {
        // Entity found
        distance = (float)Math.Sqrt(distance);
        entity = arr[id];
        return true;
      }

      // No entity found
      distance = startDistance;
      entity = null;
      return false;
    }

    /// <summary>
    /// Distance between the hitboxes of two entities. If the entity hitboxes overlap, this will return <c>0</c>.
    /// </summary>
    /// <param name="entity1">First entity.</param>
    /// <param name="entity2">Second entity.</param>
    /// <returns>The squared value between two entities, or 0 if they overlap.</returns>
    internal static float DistanceBetweenTwoEntitiesSquared(Entity entity1, Entity entity2) {
      return DistanceBetweenTwoRectsSquared(
        new Rectangle((int)entity1.Left.X, (int)entity1.Top.Y, entity1.width, entity1.height),
        new Rectangle((int)entity2.Left.X, (int)entity2.Top.Y, entity2.width, entity2.height));
    }

    /// <summary>
    /// Distance between two rectangles. If the rectangles overlap, this will return <c>0</c>.
    /// </summary>
    /// <param name="rect1">First rectangle.</param>
    /// <param name="rect2">Second rectangle.</param>
    /// <returns>The squared value between two rectangles, or 0 if they overlap.</returns>
    internal static float DistanceBetweenTwoRectsSquared(Rectangle rect1, Rectangle rect2) {
      float xAxis = rect1.Right < rect2.Left ? rect2.Left - rect1.Right : rect2.Right < rect1.Left ? rect1.Left - rect2.Right : 0;
      float yAxis = rect1.Bottom < rect2.Top ? rect2.Top - rect1.Bottom : rect2.Bottom < rect1.Top ? rect1.Top - rect2.Bottom : 0;
      return (float)(Math.Pow(xAxis, 2) + Math.Pow(yAxis, 2));
    }
    #endregion

    /// <summary>
    /// Linear interpoation between two <paramref name="f1"/> and <paramref name="f2"/> by <paramref name="by"/>.
    /// </summary>
    /// <param name="f1">First point.</param>
    /// <param name="f2">Second point.</param>
    /// <param name="by">Weight between the two, 0-1.</param>
    /// <returns></returns>
    internal static float Lerp(float f1, float f2, float by) => f1 * (1 - by) + f2 * by;

    /// <summary>
    /// Assigns multiple indexes of an array to <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to assign to.</param>
    /// <param name="keys">Indices of the array to assign to.</param>
    internal static void AssignValueToKeys<T>(this T[] arr, T value, params int[] keys) {
      for (int i = 0, len = keys.Length; i < len; i++) {
        arr[keys[i]] = value;
      }
    }
  }
}