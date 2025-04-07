using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AnimLib;
using AnimLib.States;
using JetBrains.Annotations;
using OriMod.Abilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.Commands;

/// <summary>
/// ModPlayer used in conjunction with <see cref="AbilityUIDemoCommand"/>
/// to demonstrate the progression of <see cref="OriAbility"/>s.
/// <para/> This is mainly used to show the progression in the <see cref="Menus.AbilityMenu"/>
/// </summary>
[UsedImplicitly]
public sealed class AbilityUIDemoPlayer : ModPlayer {
  public override bool IsLoadingEnabled(Mod mod) => Menus.AbilityMenu.LoadingEnabled;

  private const int TimeBetweenAbilityChanges = 159;
  private const int WaitAtLastChange = 540;

  [field: AllowNull, MaybeNull]
  private OriCharacter Ori => field ??= Player.GetCharacter<OriCharacter>();

  private int CurrentMaxWaitTime =>
    _demoIndex >= _abilityProgression.Length - 1 ? WaitAtLastChange : TimeBetweenAbilityChanges;

  public bool IsDemoing;
  private int _timeToNextChange;
  private int _demoIndex;

  private static (int id, int level, string text)[] _abilityProgression = null!; // SetStaticDefaults
  private readonly Dictionary<int, int> _savedLevels = new();

  public void StartDemo() {
    if (IsDemoing) {
      return;
    }

    IsDemoing = true;

    _demoIndex = -1;
    _timeToNextChange = TimeBetweenAbilityChanges;

    foreach (AbilityState ability in Ori.AbilityStates) {
      _savedLevels[ability.Index] = ability.Level;
      ability.Level = 0;
    }
  }

  public void EndDemo() {
    if (!IsDemoing) {
      return;
    }

    IsDemoing = false;

    foreach ((int id, int savedLevel) in _savedLevels) {
      Player.GetState<OriAbility>(id).Level = savedLevel;
    }
  }

  public void ToggleDemo() {
    if (IsDemoing) {
      EndDemo();
    }
    else {
      StartDemo();
    }
  }

  public override void SetStaticDefaults() {
    _abilityProgression = [
      ProgressionB<WallJump>("Acquired with Gold Bars"),
      ProgressionA<AirJump>(1, "Acquired with Gold Bars"),

      ProgressionB<Climb>("Some time after beating Eater of Worlds or Brain of Cthulhu"),

      ProgressionA<Burrow>(1, "After reaching Hell"),
      ProgressionA<AirJump>(2, "After reaching Hell"),

      ProgressionB<Glide>("Reached Hardmode. Glide is acquired with Souls of Night"),

      ProgressionA<Bash>(1, "Gained with Cobalt or Palladium"),
      ProgressionA<Dash>(1, "Also gained with Cobalt or Palladium"),

      ProgressionA<AirJump>(3, "Gained with Mithril or Orichalcum"),

      ProgressionA<Burrow>(2, "Gained with Adamantite or Titanium"),
      ProgressionA<Stomp>(1, "Gained with Adamantite or Titanium"),

      ProgressionA<AirJump>(4, "Defeated at least one Mechanical Boss"),
      ProgressionA<Dash>(2, "More Hallowed bars"),
      ProgressionB<ChargeJump>("More Hallowed bars"),

      ProgressionA<Burrow>(3, "Defeated Plantera"),
      ProgressionB<WallChargeJump>("Defeated Plantera"),

      ProgressionB<ChargeDash>("Made with Shroomite Bars"),
      ProgressionA<Stomp>(2, "Also made with Shroomite Bars"),

      ProgressionA<Bash>(2, "Defeated Golem"),

      ProgressionA<Launch>(1, "Beginning to explore the Lunar Events"),
      ProgressionA<Stomp>(3, "Also created with Lunar Fragments"),
      ProgressionA<Bash>(3, "Also created with Lunar Fragments"),

      ProgressionA<Launch>(2, "Moon Lord defeated"),
    ];
    return;

    // For abilities that are leveled at least twice
    static (int, int, string) ProgressionA<T>(int level, string s) where T : OriAbility {
      T ability = ModContent.GetInstance<T>();
      return ProgressionCore(ability, level, s);
    }

    // For abilities that don't use a level
    static (int, int, string) ProgressionB<T>(string s) where T : OriAbility {
      T ability = ModContent.GetInstance<T>();
      int level = ability.MaxLevel;
      return ProgressionCore(ability, level, s);
    }

    static (int, int, string) ProgressionCore(OriAbility ability, int level, string s) {
      string levelText = level switch {
        0 => throw new ArgumentException("Should not have a level 0"),
        1 when ability.MaxLevel == 1 => "Unlocked",
        _ when level < ability.MaxLevel => $"Level {level}",
        _ => "Maxed"
      };
      string str = $"[Ori Ability Demo] {ability.Name} {levelText} : {s}";
      return (ability.Index, level, str);
    }
  }

  public override void Unload() {
    _abilityProgression = null!;
  }

  public override void ResetEffects() {
    if (!IsDemoing) {
      return;
    }

    _timeToNextChange--;
    if (_timeToNextChange > 0) {
      return;
    }

    _demoIndex++;
    _timeToNextChange = CurrentMaxWaitTime;
    if (_demoIndex >= _abilityProgression.Length) {
      Main.NewText("[Ori Ability Demo]: Demo complete.");
      EndDemo();
      return;
    }

    for (int i = 0; i <= _demoIndex; i++) {
      // Ensure all abilities are at the current demo level
      (int id, int level, _) = _abilityProgression[i];
      Player.GetState<OriAbility>(id).Level = level;
    }

    Main.NewText(_abilityProgression[_demoIndex].text);
  }

  public override void PreSavePlayer() {
    if (!IsDemoing) {
      return;
    }

    Main.NewText("[Ori Ability Demo]: Saving in progress, stopping demo.");
    EndDemo();
  }

  public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
    if (!IsDemoing) {
      return;
    }

    Main.NewText("[Ori Ability Demo]: Death occurred, stopping demo.");
    EndDemo();
  }
}
