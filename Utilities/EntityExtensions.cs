using System;
using Terraria;

namespace OriMod.Utilities {
  public static class EntityExtensions {
    /// <summary>
    /// Gets the closest <see cref="Entity"/>. Returns true if any are in range, the closest <see cref="Entity"/> passed out as <paramref name="entity"/>.
    /// </summary>
    /// <param name="me">Y'know...</param>
    /// <param name="arr">Array of entities, such as <see cref="Main.player"/>, <see cref="Main.npc"/>, <see cref="Main.projectile"/>. These are the candidates for <paramref name="entity"/>.</param>
    /// <param name="distance">Maximum distance to be considered in-range, -or- 0 or negative for infinite range.
    /// <para>If this method returns <see langword="true"/>, this value is the distance to <paramref name="entity"/>.</para>
    /// <para>If this method returns <see langword="false"/>, this value is not modified.</para>
    /// </param>
    /// <param name="entity">Closest <see cref="Entity"/> in-range, -or- <see langword="null"/> if no entities are in range.</param>
    /// <param name="distanceSquaredCheck">How the closest distance is checked. Defaults to `<see cref="OriUtils.DistanceBetweenTwoEntitiesSquared(Entity, Entity)"/>`, getting closest distance between two entities.</param>
    /// <param name="condition">Extra condition to filter out entities. If <paramref name="condition"/> returns false, the entity is skipped.</param>
    /// <typeparam name="T">Type of Entity (i.e. <see cref="Player"/>, <see cref="NPC"/>, <see cref="Projectile"/>).</typeparam>
    /// <returns><see langword="true"/> if there is an <see cref="Entity"/> closer than <paramref name="distance"/> (<paramref name="entity"/> is not <see langword="null"/>), otherwise <see langword="false"/>.</returns>
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
        distanceSquaredCheck = (e1, e2) => OriUtils.DistanceBetweenTwoEntitiesSquared(e1, e2);
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


  }
}