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

    private static double lastBossCheck = 0;

    /// <summary>
    /// Whether or not any boss is alive.
    /// </summary>
    internal static bool isAnyBossAlive = false;

    /// <summary>
    /// Checks if any active <see cref="NPC"/>s are bosses.
    /// </summary>
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

    /// <summary>
    /// Checks if there are any bosses alive.
    /// </summary>
    /// <param name="check">Force a search through all NPCs for a boss.</param>
    /// <returns>True if any bosses are active in the world.</returns>
    internal static bool IsAnyBossAlive(bool check = false) {
      if (check || Main.time > lastBossCheck + 20) {
        lastBossCheck = Main.time;
        isAnyBossAlive = CheckAnyBossAlive();
      }
      return isAnyBossAlive;
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
        distanceSquaredCheck = (e1, e2) => e1.DistanceShortSquared(e2);
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
    /// Gets the closest point on this entity towards <paramref name="other"/>.
    /// </summary>
    /// <param name="me">Entity to get the point from.</param>
    /// <param name="other">Entity that the point will be closest to.</param>
    /// <returns></returns>
    internal static Vector2 ClosestSideTo(this Entity me, Entity other) {
      var x = (me.Left.X > other.Left.X ? me.Left.X : me.Right.X) < other.Right.X ? me.Right.X : me.Center.X;
      var y = (me.Top.Y > other.Top.Y ? me.Top.Y : me.Bottom.Y) < other.Bottom.Y ? me.Bottom.Y : me.Center.Y;
      return new Vector2(x, y);
    }

    /// <summary>
    /// Gets the closest point on this entity towards <paramref name="vect"/>.
    /// </summary>
    /// <param name="me">Entity to get the point from.</param>
    /// <param name="other">Point that the calculated point will be closest to.</param>
    /// <returns></returns>
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