using Terraria;

namespace OriMod.Utilities {
  class RandomChar {
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
      if (length < 2 || length > RandMaxValue) {
        throw new System.ArgumentOutOfRangeException(nameof(length), "Value must be between 2 and " + RandMaxValue);
      }

      int rand;
      if (nextExclude < 0 || nextExclude >= length) {
        rand = Main.rand.Next(length);
        nextExclude = rand;
        return alphabet[rand];
      }

      rand = Main.rand.Next(length - 1);
      if (rand >= nextExclude) {
        rand += 1;
      }
      nextExclude = rand;
      return alphabet[rand];
    }

    private int nextExclude = int.MaxValue; // Start as max value to avoid excludes on first use

    private static int RandMaxValue => 25;
    
    private static char[] alphabet => _a ?? (_a = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
    private static char[] _a;

    public static void Unload() => _a = null;
  }
}
