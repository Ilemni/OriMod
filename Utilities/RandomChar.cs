using System;
using Terraria;

namespace OriMod.Utilities {
  /// <summary>
  /// Class to get randomized <see cref="char"/>s between A and Z. Chars are capitalized.
  /// </summary>
  internal class RandomChar {
    static RandomChar() {
      OriMod.OnUnload += Unload;
    }

    private byte nextExclude = byte.MaxValue; // Start as max value to avoid excludes on first use

    private const byte RandMaxValue = 25;

    private static char[] alphabet => _alphabet ?? (_alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
    private static char[] _alphabet;

    /// <summary>
    /// Gets a random <see cref="char"/> between A and Z.
    /// </summary>
    /// <returns>A <see cref="char"/> between A and Z.</returns>
    public static char Next() => Next(RandMaxValue);

    /// <summary>
    /// Gets a random <see cref="char"/> between A and alphabet[<paramref name="length"/>].
    /// </summary>
    /// <param name="length">Range of letters from "A" that may be returned. Must be at least 1.</param>
    /// <returns>A <see cref="char"/> between A and alphabet[<paramref name="length"/>].</returns>
    /// <exception cref="ArgumentOutOfRangeException">Value must be between 1 and <see cref="RandMaxValue"/>.</exception>
    public static char Next(int length) {
      if (length < 1 || length > RandMaxValue) {
        throw new ArgumentOutOfRangeException(nameof(length), "Value must be between 1 and " + RandMaxValue);
      }

      return alphabet[Main.rand.Next(length)];
    }

    /// <summary>
    /// Gets a random <see cref="char"/> between A and Z, without repeating the previous result of this method.
    /// </summary>
    /// <returns>A <see cref="char"/> between A and Z, different from the previous result.</returns>
    public char NextNoRepeat() => NextNoRepeat(RandMaxValue);

    /// <summary>
    /// Gets a random <see cref="char"/> between A and alphabet[<paramref name="length"/>], without repeating the previous result of this method.
    /// </summary>
    /// <param name="length">Range of letters from "A" that may be returned. Must be at least 1.</param>
    /// <returns>A <see cref="char"/> between A and alphabet[<paramref name="length"/>], different from the previous result.</returns>
    public char NextNoRepeat(int length) {
      if (length < 1 || length > RandMaxValue) {
        throw new ArgumentOutOfRangeException(nameof(length), "Value must be between 1 and " + RandMaxValue);
      }

      byte rand;
      if (nextExclude == byte.MaxValue) {
        rand = (byte)Main.rand.Next(length);
      }
      else {
        rand = (byte)Main.rand.Next(length - 1);
        if (rand >= nextExclude) {
          rand++;
        }
      }
      nextExclude = rand;
      return alphabet[rand];
    }

    private static void Unload() {
      _alphabet = null;
    }
  }
}
