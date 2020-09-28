using System;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities {
  public static class ColorExtensions {
    public static Color Brightest(this Color color) {
      var vect = color.ToVector3();
      float highest = Math.Max(Math.Max(vect.X, vect.Y), vect.Z);
      if (highest == 0) {
        return Color.White;
      }
      if (highest == 1) {
        return color;
      }
      vect *= (1f / highest);

      return new Color(vect) {
        A = color.A
      };
    }
  }
}