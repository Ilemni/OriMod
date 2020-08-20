using Terraria;

namespace OriMod.Utilities {
  internal class RandomChar {
    /// <summary>
    /// Gets a random char between A and Z
    /// </summary>
    /// <returns></returns>
    public static char Next() => Next(RandMaxValue);

    /// <summary>
    /// Gets a random char between A and alphabet[length]
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static char Next(int length) {
      if (length < 1 || length > RandMaxValue) {
        throw new System.ArgumentOutOfRangeException(nameof(length), "Value must be between 1 and " + RandMaxValue);
      }

      return alphabet[Main.rand.Next(length)];
    }

    /// <summary>
    /// Gets a random char between A and Z, without repeating the previous result of this method
    /// </summary>
    /// <returns></returns>
    public char NextNoRepeat() => NextNoRepeat(RandMaxValue);

    /// <summary>
    /// Gets a random char between A and alphabet[length], without repeating the previous result of this method
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public char NextNoRepeat(int length) {
      if (length < 1 || length > RandMaxValue) {
        throw new System.ArgumentOutOfRangeException(nameof(length), "Value must be between 1 and " + RandMaxValue);
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

    private byte nextExclude = byte.MaxValue; // Start as max value to avoid excludes on first use

    private const byte RandMaxValue = 25;
    
    private static char[] alphabet => _a ?? (_a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
    private static char[] _a;

    public static void Unload() => _a = null;
  }
}
