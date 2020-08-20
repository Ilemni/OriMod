using System;
using Microsoft.Xna.Framework;

namespace OriMod.Utilities {
  public struct PointByte : IEquatable<PointByte> {
    public PointByte(byte x, byte y) {
      X = x;
      Y = y;
    }

    public byte X;
    public byte Y;

    public override bool Equals(object obj) => obj is PointByte point && Equals(point);
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
