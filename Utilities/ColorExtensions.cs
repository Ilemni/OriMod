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
    return highest switch {
      0 => Color.White with { A = color.A }, // Color is black, return white
      1 => color, // One has max value, keep color
      _ => new Color(vector * 1f / highest) { A = color.A }
    };
  }
}
