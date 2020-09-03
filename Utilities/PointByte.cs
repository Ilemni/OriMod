using System;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities {
  /// <summary>
  /// Uses <see cref="byte"/>s to represent a point.
  /// </summary>
  public struct PointByte : IEquatable<PointByte> {
    /// <summary>
    /// Creates a new instance of <see cref="PointByte"/> with the given X and Y value.
    /// </summary>
    /// <param name="x">X value.</param>
    /// <param name="y">Y value.</param>
    public PointByte(byte x, byte y) {
      X = x;
      Y = y;
    }

    /// <summary>
    /// X value.
    /// </summary>
    public byte X;
    /// <summary>
    /// Y value.
    /// </summary>
    public byte Y;

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is PointByte point && Equals(point);
    /// <summary>
    /// Returns a value indicating whether this instance and a specified <see cref="PointByte"/> object represent the same value.
    /// </summary>
    /// <param name="other">A <see cref="PointByte"/> to compare to this instance.</param>
    /// <returns></returns>
    public bool Equals(PointByte other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override int GetHashCode() {
      int hash = 17;
      hash += X.GetHashCode() * 34;
      hash <<= 3;
      hash += 17 + Y.GetHashCode() * 34;
      return hash;
    }

    public override string ToString() => $"{X}, {Y}";

    public static bool operator ==(PointByte left, PointByte right) => left.Equals(right);
    public static bool operator !=(PointByte left, PointByte right) => !(left == right);

    public static implicit operator Point(PointByte point) => new Point(point.X, point.Y);
    public static explicit operator PointByte(Point point) => new PointByte((byte)point.X, (byte)point.Y);
  }
}
