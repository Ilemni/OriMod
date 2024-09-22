using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Utilities;

public static class EntityExtensions {
  /// <summary>
  /// Gets the closest <see cref="Entity"/>. Returns true if any are in range, the closest <see cref="Entity"/> passed out as <paramref name="entity"/>.
  /// </summary>
  /// <param name="me">Y'know...</param>
  /// <param name="entities">Array of entities, such as <see cref="Main.player"/>, <see cref="Main.npc"/>, <see cref="Main.projectile"/>. These are the candidates for <paramref name="entity"/>.</param>
  /// <param name="distance">Maximum distance to be considered in-range, -or- 0 or negative for infinite range.
  /// <para>If this method returns <see langword="true"/>, this value is the distance to <paramref name="entity"/>.</para>
  /// <para>If this method returns <see langword="false"/>, this value is not modified.</para>
  /// </param>
  /// <param name="entity">Closest <see cref="Entity"/> in-range, -or- <see langword="null"/> if no entities are in range.</param>
  /// <param name="condition">Extra condition to filter out entities. If <paramref name="condition"/> returns false, the entity is skipped.</param>
  /// <typeparam name="T">Type of Entity (i.e. <see cref="Player"/>, <see cref="NPC"/>, <see cref="Projectile"/>).</typeparam>
  /// <returns><see langword="true"/> if there is an <see cref="Entity"/> closer than <paramref name="distance"/> (<paramref name="entity"/> is not <see langword="null"/>), otherwise <see langword="false"/>.</returns>
  internal static bool GetClosesEntity<T>(this Entity me, ActiveEntityIterator<T> entities, ref float distance,
    [NotNullWhen(true)] out T? entity, Func<T, bool>? condition = null) where T : Entity {
    // Setup method
    float distanceSquared = distance <= 0
      ? float.MaxValue // Infinite range detect
      : distance * distance; // Squared for DistanceSquared

    // Search for closest entity
    entity = null;
    foreach (T e in entities) {
      if (ReferenceEquals(e, me) || condition?.Invoke(e) == false) {
        continue;
      }

      float entityDistanceSquared = OriUtils.DistanceBetweenTwoEntitiesSquared(me, e);
      if (entityDistanceSquared >= distanceSquared) {
        continue;
      }

      distanceSquared = entityDistanceSquared;
      entity = e;
    }

    if (entity is null) {
      return false;
    }

    // Entity found
    distance = (float)Math.Sqrt(distance);
    return true;
  }
}
