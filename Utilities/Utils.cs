using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Utilities {
  public static class OriUtils {
    #region Point Arithmetic
    internal static Point Add(this Point p1, Point p2) {
      Point p3;
      p3.X = p1.X + p2.X;
      p3.Y = p1.Y + p2.Y;
      return p3;
    }

    internal static Point Multiply(this Point p1, Point p2) {
      Point p3;
      p3.X = p1.X * p2.X;
      p3.Y = p1.Y * p2.Y;
      return p3;
    }
    #endregion

    /// <summary>
    /// Readonly version of Normalize()
    /// </summary>
    internal static Vector2 Normalized(this Vector2 vect) {
      if (vect == Vector2.Zero) {
        return vect;
      }
      vect.Normalize();
      return vect;
    }

    private static double lastBossCheck = 0;
    internal static bool isAnyBossAlive = false;

    private static bool CheckAnyBossAlive() {
      for (int i = 0, len = Main.npc.Length; i < len; i++) {
        var npc = Main.npc[i];
        if (npc.active && npc.boss) {
          isAnyBossAlive = true;
          return true;
        }
      }
      return false;
    }

    internal static bool IsAnyBossAlive(bool check = false) {
      if (check || Main.time > lastBossCheck + 20) {
        lastBossCheck = Main.time;
        isAnyBossAlive = CheckAnyBossAlive();
      }
      return isAnyBossAlive;
    }

    #region Distance Checking
    /// <summary>
    /// Get closest entity. Returns true if any are in range.
    /// </summary>
    /// <param name="me">Y'know...</param>
    /// <param name="arr">Array of entities, such as `Main.player`, `Main.npc`, `Main.projectile`</param>
    /// <param name="dist">Maximum distance to be considered in-range. If 0 or less, range is infinite</param>
    /// <param name="entity">whoAmI of the closest entity in-range, or 255 if no entities are in range</param>
    /// <param name="distanceSquaredCheck">How the closest distance is checked. Defaults to `DistanceShortSquared()`, getting closest distance between two entities</param>
    /// <param name="condition">Extra condition to filter out entities. If false, the entity is skipped.</param>
    /// <typeparam name="T">Type of Entity (i.e. Player, NPC, Projectile)</typeparam>
    /// <returns>`true` if there is an Entity closer than `dist`, false otherwise</returns>
    internal static bool GetClosestEntity<T>(this Entity me, T[] arr, ref float dist, out T entity, Func<Entity, T, float> distanceSquaredCheck = null, Func<T, bool> condition = null) where T : Entity {
      // Setup method
      if (dist <= 0) {
        dist = float.MaxValue; // Infinite range detect
      }
      else {
        dist *= dist; // Squared for DistanceSquared
      }
      if (distanceSquaredCheck is null) {
        distanceSquaredCheck = (e1, e2) => e1.DistanceShortSquared(e2);
      }

      // Search for closest entity
      int id = -1;
      for (int i = 0, len = arr.Length; i < len; i++) {
        T e = arr[i];
        if (e is null || !e.active || condition?.Invoke(e) == false) {
          continue;
        }

        float newDist = distanceSquaredCheck(me, e);
        if (newDist < dist) {
          dist = newDist;
          id = e.whoAmI;
        }
      }

      dist = (float)Math.Sqrt(dist);
      if (id != -1) {
        // In range
        entity = arr[id];
        return true;
      }

      entity = null;
      return false;
    }

    internal static Vector2 ClosestSideTo(this Entity me, Entity other) =>
      new Vector2(
        (me.Left.X > other.Left.X ? me.Left.X : me.Right.X) < other.Right.X ? me.Right.X : me.Center.X,
        (me.Top.Y > other.Top.Y ? me.Top.Y : me.Bottom.Y) < other.Bottom.Y ? me.Bottom.Y : me.Center.Y
      );
    internal static Vector2 ClosestSideTo(this Entity me, Vector2 vect) =>
      new Vector2(
        (me.Left.X > vect.X ? me.Left.X : me.Right.X) < vect.X ? me.Right.X : me.Center.X,
        (me.Top.Y > vect.Y ? me.Top.Y : me.Bottom.Y) < vect.Y ? me.Bottom.Y : me.Center.Y
      );

    internal static float DistanceShort(this Entity me, Entity other) =>
      Vector2.Distance(me.ClosestSideTo(other), other.ClosestSideTo(me));
    internal static float DistanceShortSquared(this Entity me, Entity other) =>
      Vector2.DistanceSquared(me.ClosestSideTo(other), other.ClosestSideTo(me));
    internal static float DistanceShort(this Entity me, Vector2 otherVect) =>
      Vector2.Distance(me.ClosestSideTo(otherVect), otherVect);
    internal static float DistanceShortSquared(this Entity me, Vector2 otherVect) =>
      Vector2.DistanceSquared(me.ClosestSideTo(otherVect), otherVect);
    #endregion

    /// <summary>
    /// Linear interpoation between two <paramref name="f1"/> and <paramref name="f2"/> by <paramref name="by"/>.
    /// </summary>
    /// <param name="f1">First point</param>
    /// <param name="f2">Second point</param>
    /// <param name="by">Weight between the two, 0-1</param>
    /// <returns></returns>
    internal static float Lerp(float f1, float f2, float by) => f1 * (1 - by) + f2 * by;

    /// <summary>
    /// Assigns multiple indexes of an array to <paramref name="value"/>
    /// </summary>
    /// <param name="value">The value to assign to</param>
    /// <param name="keys">Indices of the array to assign to</param>
    internal static void AssignValueToKeys<T>(this T[] arr, T value, params int[] keys) {
      for (int i = 0, len = keys.Length; i < len; i++) {
        arr[keys[i]] = value;
      }
    }
  }
}