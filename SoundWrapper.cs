using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria;
using Terraria.ModLoader;

namespace OriMod;

public static class SoundWrapper {
  /// <summary>
  /// Whether we have sounds loaded.
  /// This wrapper is necessary in case a mod build is done without sounds. We do not distribute sounds in our repository.
  /// </summary>
  private static bool _canPlaySounds;

  private static bool _checkedCanPlaySounds;

  public static void Play(Player player, string soundPath, float volumeScale = 1f, float pitchOffset = 0.0f) =>
    Play(player.Center, soundPath, volumeScale, pitchOffset);

  public static void Play(Vector2 pos, string soundPath, float volumeScale = 1f, float pitchOffset = 0.0f) {
    Play(pos, new SoundStyle("OriMod/Sounds/" + soundPath) {
      Pitch = pitchOffset,
      Volume = volumeScale,
    });
  }

  public static void Play(Vector2 pos, SoundStyle style) {
    if (_checkedCanPlaySounds) {
      if (_canPlaySounds) {
        SoundEngine.PlaySound(in style, pos);
      }

      return;
    }

    // Check if we can play sounds
    _canPlaySounds = ModContent.HasAsset(style.SoundPath);
    _checkedCanPlaySounds = true;

    if (_canPlaySounds) {
      SoundEngine.PlaySound(in style, pos);
    }
  }

  public static void PlayLocal(Player player, string path, float volume = 1, float pitch = 0) {
    if (player.whoAmI == Main.myPlayer) {
      Play(player.Center, path, volume, pitch);
    }
  }
}
