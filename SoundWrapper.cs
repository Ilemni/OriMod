using Microsoft.Xna.Framework;
using Terraria.Audio;
using ReLogic.Utilities;

namespace OriMod {
  public static class SoundWrapper {
    /// <summary>
    /// Whether or not we have sounds loaded.
    /// This wrapper is necessary in case a compilation is done without sounds. We do not distribute sounds in our repository.
    /// </summary>
    private static bool _canPlaySounds;

    private static bool _checkedCanPlaySounds;

    public static SlotId PlaySound(Vector2 position, string soundPath, out SoundStyle style, float volumeScale = 1f, float pitchOffset = 0.0f)
      => PlaySound((int) position.X, (int) position.Y, soundPath, out style, volumeScale, pitchOffset);

    public static SlotId PlaySound(int x, int y, string soundPath, out SoundStyle style, float volumeScale = 1f, float pitchOffset = 0.0f) {
      style = new("OriMod/Sounds/Custom/NewSFX/" + soundPath) {
        Pitch = pitchOffset,
        Volume = volumeScale,
      };
      Vector2 pos = new(x, y);
      if (_checkedCanPlaySounds)
        return !_canPlaySounds ? SlotId.Invalid : SoundEngine.PlaySound(in style, pos);

      // Check if we can play sounds
      if (OriMod.instance is null) return SlotId.Invalid;
      _checkedCanPlaySounds = true;
      _canPlaySounds = OriMod.instance.HasAsset("Sounds/Custom/NewSFX/Ori/Dash/seinDashA");

      return !_canPlaySounds ? SlotId.Invalid : SoundEngine.PlaySound(in style, pos);
    }
  }
}