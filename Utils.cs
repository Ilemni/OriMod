using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod {
  public static class OriModUtils {
    internal static Point Add(this Point p1, Point p2) {
      Point p3;
      p3.X = p1.X + p2.X;
      p3.Y = p1.Y + p2.Y;
      return p3;
    }
    internal static Point Add(this Point p, Vector2 v) {
      Point p2;
      p2.X = p.X + (int)v.X;
      p2.Y = p.Y + (int)v.Y;
      return p;
    }
    internal static Vector2 Add(this Vector2 v, Point p) {
      Vector2 v2;
      v2.X = v.X + p.X;
      v2.Y = v.Y + p.Y;
      return v2;
    }
    internal static Point Multiply(this Point p1, Point p2) {
      Point p3;
      p3.X = p1.X * p2.X;
      p3.Y = p1.Y * p2.Y;
      return p3;
    }
    
    /// <summary> Returns Normalize() without changing vect </summary>
    internal static Vector2 Norm(this Vector2 vect) {
      if (vect == Vector2.Zero) return vect;
      Vector2 v = vect;
      v.Normalize();
      return v;
    }
    
    internal static void UpdateHitbox(this Point[] box, Point[] template, Vector2 center) =>
      box.UpdateHitbox(template, center.ToTileCoordinates());
    internal static void UpdateHitbox(this Point[] box, Point[] template, Point center) {
      for(int i = 0; i < box.Length; i++) {
        box[i] = template[i].Add(center);
      }
    }
    
    private static double lastBossCheck = 0;
    internal static bool isAnyBossAlive = false;
    
    private static bool CheckAnyBossAlive() {
      for(int i = 0, len = Main.npc.Length; i < len; i++) {
        if (Main.npc[i].active && Main.npc[i].boss) {
          isAnyBossAlive = true;
          return true;
        }
      }
      isAnyBossAlive = false;
      return false;
    }

    internal static bool IsAnyBossAlive(bool check=false) {
      if (check || Main.time > lastBossCheck + 20) {
        lastBossCheck = Main.time;
        CheckAnyBossAlive();
      }
      return isAnyBossAlive;
    }
    
    /// <summary> Get closest entity. Returns true if any are in range. </summary>
    /// <param name="me">Y'know...</param>
    /// <param name="arr">Array of entities, such as `Main.player`, `Main.npc`, `Main.projectile`</param>
    /// <param name="dist">Maximum distance to be considered in-range. If 0 or less, range is infinite</param>
    /// <param name="entityId">whoAmI of the closest entity in-range, or 255 if no entities are in range</param>
    /// <param name="distSQCheck">How the closest distance is checked. Defaults to `DistanceShortSquared()`, getting closest distance between two entities</param>
    /// <param name="condition">Extra condition to filter out entities. If false, the entity is skipped.</param>
    /// <typeparam name="T">Type of Entity (i.e. Player, NPC, Projectile)</typeparam>
    /// <returns>`true` if there is an Entity closer than `dist`, false otherwise</returns>
    internal static bool GetClosestEntity<T>(this Entity me, T[] arr, ref float dist, out int entityId, Func<Entity, T, float> distSQCheck=null, Func<T, bool> condition=null) where T : Entity {
      if (dist <= 0) {
        // Infinite range detect
        dist = float.MaxValue;
      }
      else {
        // Squared for proper DistanceSquared
        dist = dist * dist;
      }
      if (distSQCheck == null) {
        distSQCheck = (e1, e2) => e1.DistanceShortSquared(e2);
      }
      entityId = 255;
      bool inRange = false;
      
      for (int e = 0; e < arr.Length; e++) {
        T entity = arr[e];
        if (entity == null) continue;
        if (!entity.active || condition?.Invoke(entity) == false) continue;
        
        float newDist = distSQCheck(me, entity);
        if (newDist < dist) {
          inRange = true;
          dist = newDist;
          entityId = entity.whoAmI;
        }
      }

      if (inRange) {
        dist = (float)Math.Sqrt(dist);
      }
      return inRange;
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

    internal static float Lerp(float f1, float f2, float by) => f1 * (1 - by) + f2 * by;

    /// <summary> Checks if the held item shot a projectile of this type. </summary>
    internal static bool HeldItemShotThis(this Projectile proj) {
      if (proj.owner == 255) return false;
      var item = Main.player[proj.owner].HeldItem;
      return item.shoot == proj.type;
    }
  }
}