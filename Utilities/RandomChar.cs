using System;
using Terraria;

namespace OriMod.Utilities;

/// <summary>
/// Class to get non-repeating random values.
/// </summary>
internal struct RandomChar {
  public RandomChar(byte maxValue) {
    ArgumentOutOfRangeException.ThrowIfNegative((int)_maxValue);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((int)_maxValue, RandMaxValue);
    _maxValue = maxValue;
  }

  private const byte RandMaxValue = 25;

  /// <summary>
  /// Value that represents the last used character of <see cref="NextNoRepeat"/>
  /// </summary>
  private byte _value = byte.MaxValue;

  private readonly byte _maxValue;

  public byte NextNoRepeat() {
    if ((int)_maxValue is 0 or 1) {
      _value = 0;
      return _value;
    }

    if (_value == byte.MaxValue) {
      _value = (byte)Main.rand.Next(_maxValue);
      return _value;
    }

    byte old = _value;
    _value = (byte)Main.rand.Next(_maxValue - 1);
    if (_value >= old) {
      _value++;
    }

    return _value;
  }
}
