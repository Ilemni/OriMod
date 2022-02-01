using System;
using Terraria;

namespace OriMod.Utilities {
  /// <summary>
  /// Class to get randomized <see cref="char"/>s between A and Z. Chars are capitalized.
  /// </summary>
  internal class RandomChar {
    private byte _nextExclude = byte.MaxValue; // Start as max value to avoid excludes on first use

    private const byte RandMaxValue = 25;

    /// <summary>
    /// Gets a random <see cref="char"/> between A and alphabet[<paramref name="length"/>].
    /// </summary>
    /// <param name="length">Range of letters from "A" that may be returned. Must be at least 1.</param>
    /// <returns>A <see cref="char"/> between A and alphabet[<paramref name="length"/>].</returns>
    /// <exception cref="ArgumentOutOfRangeException">Value must be between 1 and <see cref="RandMaxValue"/>.</exception>
    public static char Next(int length) {
      if (length < 0 || length > RandMaxValue) {
        throw new ArgumentOutOfRangeException(nameof(length), "Value must be between 1 and " + RandMaxValue);
      }

      if (length == 0) return 'A';
      
      return (char) ('A' + Main.rand.Next(length));
    }

    /// <summary>
    /// Gets a random <see cref="char"/> between A and alphabet[<paramref name="length"/>], without repeating the previous result of this method.
    /// </summary>
    /// <param name="length">Range of letters from "A" that may be returned. Must be at least 1.</param>
    /// <returns>A <see cref="char"/> between A and alphabet[<paramref name="length"/>], different from the previous result.</returns>
    public char NextNoRepeat(int length) {
      if (length < 0 || length > RandMaxValue) {
        throw new ArgumentOutOfRangeException(nameof(length), "Value must be between 1 and " + RandMaxValue);
      }

      if (length == 0) {
        _nextExclude = 0;
        return 'A';
      }
      byte rand;
      if (_nextExclude == byte.MaxValue) {
        rand = (byte) Main.rand.Next(length);
      }
      else {
        rand = (byte) Main.rand.Next(length - 1);
        if (rand >= _nextExclude) {
          rand++;
        }
      }

      _nextExclude = rand;
      return (char) ('A' + rand);
    }
  }
}