using System.Collections.Generic;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;
using Terraria.Audio;

namespace OriMod;

/// <summary>
/// Contains information for playing sounds,
/// and randomly switches between sounds of different suffixes without repeating.
/// </summary>
public struct SoundInfo(string pathPrefix, byte random, float volume, float pitch = 0f) {
  private static SoundStyle[] GetOrCreateStyles(string pathPrefix, int random, float volume, float pitch) {
    if (AllPaths.TryGetValue(pathPrefix, out var soundStyles)) {
      return soundStyles;
    }

    soundStyles = new SoundStyle[random];
    for (int i = 0; i < random; i++) {
      string soundPath = $"OriMod/Sounds/{pathPrefix}{(char)('A' + i)}";
      soundStyles[i] = new SoundStyle(soundPath) {
        Volume = volume,
        Pitch = pitch
      };
    }

    AllPaths[pathPrefix] = soundStyles;
    return soundStyles;
  }

  public float Pitch {
    init {
      for (int index = 0; index < _paths.Length; index++) {
        _paths[index].Pitch = value;
      }
    }
  }

  private readonly SoundStyle[] _paths = GetOrCreateStyles(pathPrefix, random, volume, pitch);
  private RandomChar _randomChar = new(random);

  public void Play(Player player) => Play(player.Center);

  public void Play(Vector2 vector) {
    if (Main.dedServ) {
      return;
    }

    if (_paths.Length == 0) {
      return;
    }

    SoundStyle soundStyle = _paths[_randomChar.NextNoRepeat()];
    SoundWrapper.Play(vector, soundStyle);
  }

  public void PlayLocal(Player player, float? volume = null, float? pitch = null) {
    if (player.whoAmI == Main.myPlayer) {
      Play(player);
    }
  }

  private static Dictionary<string, SoundStyle[]> AllPaths => _allPaths ??= [];
  private static Dictionary<string, SoundStyle[]>? _allPaths;

  internal static void Unload() => _allPaths = null;
}
