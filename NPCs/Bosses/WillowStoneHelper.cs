using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.NPCs.Bosses {
  internal static class WillowStoneHelper {
    public static NPC Head(NPC segment) => Main.npc[(int)segment.ai[0]];

    public static Vector2 GetWorldPosition(NPC head, int idx) {
      var vect = GetRelativePosition(head, idx);

      var rotation = head.ai[3];
      var origin = head.Center;

      var pos = vect.ToLocalPosition();

      return origin + (pos.RotatedBy(rotation, origin));
    }
    public static Vector2 GetWorldPosition(WillowStoneSegment segment) {
      var head = Head(segment.npc);
      return GetWorldPosition(head, (int)segment.npc.ai[1]);
    }

    internal static Vector2 Test_Position(int idx) {
      var segmentsPerRing = WillowStoneHead.MaxSegmentCount / 2;
      bool inner = idx < segmentsPerRing; // Willow has two rings. Inner is 0-11 (12), Outer is 12-31 (20)
      if (!inner) idx -= segmentsPerRing;
      float arcLength = (360f / segmentsPerRing) / 360;
      float percent = (idx * arcLength * 1.5f) % 1; // Percent is the "idle" rotation about the head. 0-1
      //Main.NewText($"{idx} ({(inner ? "inner" : "outer")}) = {percent}");

      var vect = Position_Laser(percent, inner, 1);
      //Main.NewText($"idx: {idx} percent:{percent} factor:{factor}");
      var pos = vect.ToLocalPosition();
      return Main.MouseWorld + pos;
    }

    /// <summary>
    /// Values are (x: rotation in <strong>degrees</strong>, y: distance from origin)
    /// </summary>
    /// <returns></returns>
    public static PolarCoordinate GetRelativePosition(NPC head, int idx) {
      var state = (WillowState)head.ai[0];
      var extra = (WillowStateExtra)head.ai[1];

      bool inner = idx < 24; // Willow has two rings. Inner is 0-11 (12), Outer is 12-31 (20)
      float percent = (idx * (15 / 360f) + (7.5f / 360f)) % 1;

      switch (state) {
        case WillowState.None:
        case WillowState.Starting:
        case WillowState.Regenerate:
        case WillowState.Idle:
        case WillowState.Explode:
          return Position_Idle(percent, inner);
        case WillowState.Shooting: 
        case WillowState.ShootingSeeking:
          return Position_Laser(percent, inner, 1);
        case WillowState.Laser2:
          return Position_Laser(percent, inner, 2);
        case WillowState.Laser3:
          return Position_Laser(percent, inner, 3);
        case WillowState.Laser4:
          return Position_Laser(percent, inner, 4);
        default:
          return Position_Idle(percent, inner);
      }
    }

    /// <summary>
    /// Values are (x: rotation in <strong>degrees</strong>, y: distance from origin)
    /// </summary>
    public static PolarCoordinate Position_Idle(float percent, bool inner) {
      PolarCoordinate result = default;
      result.rotation = (percent * 360f) % 360f;
      //Main.NewText(result.X);
      if (inner) {
        result.distance = 120;
      }
      else {
        result.distance = 160;
      }
      return result;
    }

    // Percent is the rotation of one of the rocks around the core. values 0-1, where 0.5 is halfway around
    // Inner, just means the rock is in the inner ring, since Willow Stone has 2 rings
    // LaserCount, how many holes we need to make for the laser(s) that will be made
    public static PolarCoordinate Position_Laser(float percent, bool inner, int laserCount) {
      // Result.x is the degree representation of percent. If percent is 0.25, result.x is 90 degrees
      // Result.y is distance from the willow stone; we don't need to use it here
      var result = Position_Idle(percent, inner);
      // Size of an arc when split in 2, or 3, or whatever laserCount is
      float arcSize = 360f / laserCount;
      // Which number of arc this is in
      int idx = (int)Math.Floor((double)percent * laserCount);
      // Distance between 0 and the start of the arc this is in
      float offset = idx * arcSize;
      // Degrees relative to the start of the arc this is in
      float angle = result.rotation % arcSize;
      // The gap we want for the laser to shoot through, in degrees
      float gap = 30;
      // Compress is how much we want to squeeze points together. This is based on how much of a gap we want. 0-1
      float compress = 1 - gap / arcSize;
      // Degrees to offset by within the arc. 20% is used to center it on the arc.
      float innerOffset = (1 - compress) * 0.5f;

      // Maffs
      result.rotation = offset + (angle * compress + innerOffset) + gap / 2;

      return result;
    }

    // 0.25 * (300 - 200) + 200
    public static void Crunch(ref float value, float min, float max) {
      float percent = value / 360;
      float range = max - min;
      value = percent * range + min;
      value %= 360;
    }

    public static void Compress(ref float value, float percent, float offset) {
      value *= percent;
      value += offset;
    }
  }

  /// <summary>
  /// Struct for representing the distance and rotation of a vector.
  /// </summary>
  public struct PolarCoordinate {
    /// <summary>
    /// Local rotation, in degrees
    /// </summary>
    public float rotation;
    /// <summary>
    /// Local distance
    /// </summary>
    public float distance;

    /// <summary>
    /// Returns a <see cref="Vector2"/> with an X and Y value based on this distance and rotation.
    /// </summary>
    /// <returns></returns>
    public Vector2 ToLocalPosition() => (Vector2.UnitY * distance).RotatedBy(MathHelper.ToRadians(rotation));

    /// <summary>
    /// Returns a <see cref="PolarCoordinate"/> based on the given <see cref="Vector2"/>'s X and Y value.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static PolarCoordinate FromLocalPosition(Vector2 position) {
      return new PolarCoordinate {
        rotation = MathHelper.ToDegrees((float)Math.Atan2(position.Y, position.X)),
        distance = position.Length()
      };
    }

    /// <summary>
    /// Linear interpolation between two <see cref="PolarCoordinate"/>s.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="by"></param>
    /// <returns></returns>
    public static PolarCoordinate Lerp(PolarCoordinate start, PolarCoordinate end, float by) {
      return new PolarCoordinate {
        rotation = MathHelper.Lerp(start.rotation, end.rotation, by),
        distance = MathHelper.Lerp(start.distance, end.distance, by)
      };
    }

    /// <summary>
    /// Linear interpolation between two <see cref="Vector2"/>s, with respect to local distance.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="by"></param>
    /// <returns></returns>
    public static Vector2 Lerp(Vector2 start, Vector2 end, float by) {
      return Lerp(FromLocalPosition(start), FromLocalPosition(end), by).ToLocalPosition();
    }


  }
}
