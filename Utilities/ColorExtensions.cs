using System;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities; 

public static class ColorExtensions {
  /// <summary>
  /// Returns a brightened color where at least one value is the max value. If the current color is black, the resulting color is white. 
  /// </summary>
  /// <param name="color"></param>
  /// <returns></returns>
  public static Color Brightened(this Color color) {
    Vector3 vector = color.ToVector3();
    float highest = Math.Max(Math.Max(vector.X, vector.Y), vector.Z);
    switch (highest) {
      case 0:
        return Color.White;
      case 1:
        return color;
      default:
        vector *= 1f / highest;

        return new Color(vector) {
          A = color.A
        };
    }
  }
}
