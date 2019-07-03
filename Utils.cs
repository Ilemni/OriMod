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
    public static bool IsAnyBossAlive {
      get {
        for(int i = 0; i < Main.maxNPCs; i++) {
          if (Main.npc[i].active && Main.npc[i].boss) return true;
        }
        return false;
      }
    }
  }
}