using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public static class SoundWrapper {
    /// <summary>
    /// Whether or not we have sounds loaded.
    /// This wrapper is necessary in case a compilation is done without sounds. We do not distribute sounds in our repository.
    /// </summary>
    private static bool _canPlaySounds;

    private static bool _checkedCanPlaySounds;

    public static SoundEffectInstance PlaySound(Vector2 position, string soundPath, float volumeScale = 1f, float pitchOffset = 0.0f)
      => PlaySound((int) position.X, (int) position.Y,
        SoundLoader.GetSoundSlot(SoundType.Custom, "OriMod/Sounds/Custom/NewSFX/" + soundPath),
        volumeScale, pitchOffset);
    
    public static SoundEffectInstance PlaySound(int x = -1, int y = -1, int style = 1, float volumeScale = 1f, float pitchOffset = 0.0f) {
      if (_checkedCanPlaySounds)
        return !_canPlaySounds ? null : Main.PlaySound((int)SoundType.Custom, x, y, style, volumeScale, pitchOffset);
      
      // Check if we can play sounds
      if (OriMod.instance is null) return null;
      _checkedCanPlaySounds = true;
      _canPlaySounds = OriMod.instance.SoundExists("Sounds/Custom/NewSFX/Ori/Dash/seinDashA");

      return !_canPlaySounds ? null : Main.PlaySound((int)SoundType.Custom, x, y, style, volumeScale, pitchOffset);
    }
  }
}