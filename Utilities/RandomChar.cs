using System;
using Terraria;

namespace OriMod.Utilities;

/// <summary>
/// Class to get randomized <see cref="char"/>s between A and Z. Chars are capitalized.
/// </summary>
internal struct RandomChar {
  public RandomChar() {
  }

  private const byte RandMaxValue = 25;
  private const char A = 'A';

  /// <summary>
  /// Value that represents the last used character of <see cref="NextNoRepeat"/>
  /// </summary>
  private byte _value = byte.MaxValue;

  /// <summary>
  /// Gets a random <see cref="char"/> between A and alphabet[<paramref name="length"/>].
  /// </summary>
  /// <param name="length">Range of letters from "A" that may be returned. Must be at least 1.</param>
  /// <returns>A <see cref="char"/> between A and alphabet[<paramref name="length"/>].</returns>
  /// <exception cref="ArgumentOutOfRangeException">Value must be between 1 and <see cref="RandMaxValue"/>.</exception>
  public static char Next(int length) {
    ArgumentOutOfRangeException.ThrowIfNegative(length);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(length, RandMaxValue);

    return length switch {
      0 => A,
      _ => (char)(A + Main.rand.Next(length))
    };
  }

  /// <summary>
  /// Gets a random <see cref="char"/> between A and alphabet[<paramref name="length"/>], without repeating the previous result of this method.
  /// </summary>
  /// <param name="length">Range of letters from "A" that may be returned. Must be at least 1.</param>
  /// <returns>A <see cref="char"/> between A and alphabet[<paramref name="length"/>], different from the previous result.</returns>
  public char NextNoRepeat(int length) {
    ArgumentOutOfRangeException.ThrowIfNegative(length);
    ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(length, RandMaxValue);

    if (length == 0) {
      _value = 0;
      return A;
    }

    if (_value == byte.MaxValue) {
      _value = (byte)Main.rand.Next(length);
      return (char)(A + _value);
    }

    byte old = _value;
    _value = (byte)Main.rand.Next(length - 1);
    if (_value >= old) {
      _value++;
    }

    return (char)(A + _value);
  }
}
