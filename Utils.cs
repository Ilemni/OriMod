using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod {
  public static class OriModUtils {
    public static Point Add(this Point point, Point other) {
      Point point3;
      point3.X = point.X + other.X;
      point3.Y = point.Y + other.Y;
      return point3;
    }
    public static Point Add(this Point point, Vector2 other) {
      Point point3;
      point3.X = point.X + (int)other.X;
      point3.Y = point.Y + (int)other.Y;
      return point;
    }
    public static Vector2 Add(this Vector2 vector2, Point other) {
      Vector2 newV;
      newV.X = vector2.X + other.X;
      newV.Y = vector2.Y + other.Y;
      return newV;
    }
    public static Point Multiply(this Point p1, Point p2) {
      Point p3;
      p3.X = p1.X * p2.X;
      p3.Y = p1.Y * p2.Y;
      return p3;
    }
    public static Vector2 Norm(this Vector2 vect) {
      Vector2 v = vect;
      v.Normalize();
      return v;
    }
    public static void UpdateHitbox(this Point[] box, Point[] template, Vector2 center) {
      box.UpdateHitbox(template, center.ToTileCoordinates());
    }
    public static void UpdateHitbox(this Point[] box, Point[] template, Point center) {
      for(int i = 0; i < box.Length; i++) {
        box[i] = template[i].Add(center);
      }
    }
    public static void CheckAnyBossAlive() {
      if (Main.time - 5 < lastBossCheck) return;
      lastBossCheck = Main.time;
      for(int i = 0; i < Main.maxNPCs; i++) {
        if (Main.npc[i].active && Main.npc[i].boss) {
          _isAnyBossAlive = true;
          return;
        }
      }
      _isAnyBossAlive = false;
    }
    private static double lastBossCheck = 0;
    private static bool _isAnyBossAlive = false;
    public static bool IsAnyBossAlive(bool check=false) {
      if (check) CheckAnyBossAlive();
      return _isAnyBossAlive;
    }
    public static bool GetClosestEntity<T>(this Entity me, T[] arr, ref float dist, out int entityId, Func<T, bool> filter=null) where T : Entity {
      entityId = int.MaxValue;
      float oldDist = dist;
      dist = (float)Math.Pow(dist, 2);
      bool inRange = false;
      for (int e = 0; e < arr.Length; e++) {
        T entity = arr[e];
        if (entity == null || !entity.active || (filter != null ? filter(entity) : false)) continue;
        float newDist = me.DistanceShortSquared(entity);
        if (newDist < dist) {
          dist = newDist;
          inRange = true;
          entityId = entity.whoAmI;
        }
      }
      if (inRange) {
        dist = (float)Math.Sqrt(dist);
      }
      else {
        dist = oldDist;
      }
      return inRange;
    }
    public static Vector2 ClosestSideTo(this Entity me, Entity other) =>
      new Vector2(
        me.Left.X > other.Left.X ? me.Left.X : me.Right.X < other.Right.Y ? me.Right.X : me.Center.X,
        me.Top.Y > other.Top.Y ? me.Top.Y : me.Bottom.Y < other.Bottom.Y ? me.Bottom.Y : me.Center.Y
      );
    public static Vector2 ClosestSideTo(this Entity me, Vector2 vect) =>
      new Vector2(
        me.Left.X > vect.X ? me.Left.X : me.Right.X < vect.Y ? me.Right.X : me.Center.X,
        me.Top.Y > vect.Y ? me.Top.Y : me.Bottom.Y < vect.Y ? me.Bottom.Y : me.Center.Y
      );
    public static float DistanceShort(this Entity me, Entity other) =>
      Vector2.Distance(me.ClosestSideTo(other), other.ClosestSideTo(me));
    public static float DistanceShortSquared(this Entity me, Entity other) =>
      Vector2.DistanceSquared(me.ClosestSideTo(other), other.ClosestSideTo(me));
    public static float DistanceShort(this Entity me, Vector2 otherVect) =>
      Vector2.Distance(me.ClosestSideTo(otherVect), otherVect);
    public static float DistanceShortSquared(this Entity me, Vector2 otherVect) =>
      Vector2.DistanceSquared(me.ClosestSideTo(otherVect), otherVect);
  }
}