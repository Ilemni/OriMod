using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod;

public sealed class SeinData {
  private SeinData() { }

  /// <summary>
  /// Collection of all <see cref="SeinData"/>s in the mod.
  /// </summary>
  public static SeinData[] All { get; private set; }

  /// <summary>
  /// Collection of all sein buffs ids.
  /// </summary>
  public static int[] SeinBuffs { get; private set; }

  /// <summary>
  /// Loads all Sein variants. Sein stats are hardcoded into this method.
  /// </summary>
  /// <remarks>
  /// Okay, no matter how many times I try to refactor this, I don't think it can be organized any better.
  /// <para>1. Memory is always only ever using 8 <see cref="SeinData"/>s, or however many is in <see cref="All"/>. Not too big a deal though.</para>
  /// <para>2. Readability. I feel that this is the best setup for a few reasons.</para>
  /// <para>2a. It's all in one file rather than multiple derived classes for easy comparison.</para>
  /// <para>2b. Only the changes/upgrades are shown, rather than having redundant data. The same could be accomplished with inheritance, but I'd rather not have 8 levels of it.</para>
  /// </remarks>
  internal static void Load() {
    SeinData defaultSein = new();
    var fields = typeof(SeinData).GetFields();

    var list = new List<SeinData>();
    void AddNewSein(SeinData newSein) {
      SeinData lastSein = list.Count == 0 ? new SeinData() : list[^1];

      foreach (FieldInfo field in fields) {
        object defVal = field.GetValue(defaultSein);
        object oldVal = field.GetValue(lastSein);
        object newVal = field.GetValue(newSein);

        // If value is specified in constructor, use it
        // If value is unspecified, use value of previous upgrade
        if (newVal?.ToString() == defVal?.ToString()) {
          newVal = oldVal;
        }
        field.SetValue(newSein, newVal);
      }
      list.Add(newSein);
      //OriMod.Log.Debug(newSein.CalculateStuff(tierName));
    }

    // Tier 1 (Silver)
    AddNewSein(new SeinData() {
      LightStrength = 0.4f
    });

    // Tier 2 (Demonite/Crimtane)
    // Increased shots per burst
    // Max damage per burst: 15
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.Green,
      Value = 3000,
      Color = new Color(108, 92, 172),

      Damage = 24,
      Targets = 1,
      Bursts = 3,
      ProjectileSpeedStart = 9f,
      HomingIncreaseRate = 0.05f,
      DustScale = 1.8f,
      LightStrength = 1.6f,
    });

    // Tier 3 (Hellstone)
    // 2 targets
    // Max damage per burst: 42
    // For some sort of "rage" effect to pair with red theme, lower CD
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.Orange,
      Value = 10000,
      Color = new Color(240, 0, 0, 194),

      Damage = 29,
      Targets = 2,
      MaxShotsAtOnce = 2,
      RandDegrees = 100,
      CooldownMin = 5,
      CooldownShort = 16,
      CooldownLong = 35,
      ProjectileSpeedStart = 12.5f,
      ProjectileSpeedIncreaseRate = 0.7f,
      ProjectileSpeedIncreaseDelay = 10,
      TargetMaxDist = 370f,
      DustScale = 2f,
      LightStrength = 1.275f,
    });

    // Tier 4 (Mythril/Orichalcum)
    // 2 targets, 2 shots to primary, 3 shots max (rather than 4)
    // Max damage per burst: 81
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.LightRed,
      Value = 25000,
      Color = new Color(185, 248, 248),

      Damage = 33,
      ShotsToPrimaryTarget = 2,
      MaxShotsAtOnce = 3,
      RandDegrees = 60,
      CooldownMin = 11,
      CooldownShort = 25,
      CooldownLong = 40,
      ProjectileSpeedStart = 13.5f,
      HomingIncreaseRate = 0.06f,
      HomingIncreaseDelay = 20,
      DustScale = 2.2f,
      LightStrength = 1.2f,
    });

    // Tier 5 (Hallow)
    // 3 targets, 2 shots to primary, 4 shots max (rather than 5)
    // Max damage per burst: 132
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.Pink,
      Value = 50000,
      Color = new Color(255, 228, 160),

      Damage = 37,
      Targets = 3,
      MaxShotsAtOnce = 4,
      HomingIncreaseDelay = 17,
      TargetMaxDist = 440f,
      DustScale = 2.4f,
      LightStrength = 1.4f,
    });

    // Tier 6 (Spectral)
    // 3 targets, 3 shots to primary, 5 shots max
    // Max damage per burst: 195
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.Yellow,
      Value = 100000,
      Color = new Color(0, 180, 174, 210),

      Damage = 42,
      ShotsToPrimaryTarget = 3,
      MaxShotsAtOnce = 5,
      CooldownMin = 12,
      CooldownShort = 26,
      CooldownLong = 52,
      TargetThroughWallDist = 224f,
      HomingIncreaseRate = 0.07f,
      ProjectileSpeedStart = 15f,
      ProjectileSpeedIncreaseRate = 0.85f,
      ProjectileSpeedIncreaseDelay = 14,
      RandDegrees = 70,
      DustScale = 2.65f,
      LightStrength = 2.25f,
    });

    // Tier 7 (Lunar)
    // 4 targets, 3 shots to primary, 2 to others, 6 shots max (rather than 9)
    // Max damage per burst: 282
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.Cyan,
      Value = 250000,
      Color = new Color(78, 38, 102),

      Damage = 47,
      Targets = 4,
      ShotsToPrimaryTarget = 3,
      ShotsPerTarget = 2,
      MaxShotsAtOnce = 9,
      HomingIncreaseRate = 0.025f,
      ProjectileSpeedStart = 16f,
      TargetMaxDist = 510f,
      RandDegrees = 120,
      DustScale = 3f,
      LightStrength = 4.5f,
    });

    // Tier 8 (Lunar Bars)
    // 5 targets, 4 shots to primary, 2 shots to others, 10 shots max (rather than 12)
    // Max damage per burst: 530 (too high?)
    AddNewSein(new SeinData {
      Rarity = ItemRarityID.Red,
      Value = 500000,
      Color = new Color(220, 220, 220),

      Damage = 53,
      Bursts = 4,
      Targets = 6,
      ShotsToPrimaryTarget = 4,
      MaxShotsAtOnce = 10,
      CooldownMin = 16,
      CooldownShort = 24,
      CooldownLong = 55,
      HomingStrengthStart = 0.05f,
      HomingIncreaseDelay = 15,
      ProjectileSpeedStart = 20f,
      ProjectileSpeedIncreaseRate = 1.25f,
      ProjectileSpeedIncreaseDelay = 28,
      RandDegrees = 180,
      TargetMaxDist = 650f,
      TargetThroughWallDist = 370f,
      DustScale = 3.35f,
      LightStrength = 2.5f,
    });

    All = Unloadable.New(list.ToArray(), () => All = null);
    SeinBuffs = Unloadable.New(new int[All.Length], () => SeinBuffs = null);
    for (int u = 0; u < All.Length; u++) {
      SeinBuffs[u] = ModContent.Find<ModBuff>(OriMod.instance.Name, "SeinBuff" + (u + 1)).Type;
    }
  }

  #region Stats
  #region Stats responsible for DPS
  /// <summary>
  /// Damage of Spirit Flame.
  /// </summary>
  public int Damage = 18;

  /// <summary>
  /// Number of NPCs that can be targeted at once.
  /// </summary>
  public int Targets = 1;

  /// <summary>
  /// Maximum times the minion can fire with a delay of <see cref="CooldownMin"/> before having a delay of <see cref="CooldownLong"/>.
  /// </summary>
  public int Bursts = 2;

  /// <summary>
  /// Maximum number of shots that can be fired at each target.
  /// </summary>
  public int ShotsPerTarget = 1;

  /// <summary>
  /// Maximum number of shots that can be fired at the primary target at once.
  /// </summary>
  public int ShotsToPrimaryTarget = 1;

  /// <summary>
  /// Maximum number of shots that can be fired at once.
  /// </summary>
  public int MaxShotsAtOnce = 1;

  /// <summary>
  /// Delay between each shot in <see cref="Bursts"/>.
  /// </summary>
  public int CooldownMin = 10;

  /// <summary>
  /// Shortest time to wait during <see cref="Bursts"/> to reset burst count.
  /// </summary>
  public int CooldownShort = 15;

  /// <summary>
  /// Delay between each series of shots, incurred when shots reaches <see cref="Bursts"/>.
  /// </summary>
  public int CooldownLong = 30;
  #endregion

  /// <summary>
  /// Maximum angle that fired Spirit Flames will be away from the target.
  /// </summary>
  internal int RandDegrees = 40;

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, if there is line of sight between it and the player.
  /// </summary>
  public float TargetMaxDist = 240f;
  public float TargetMaxDistSquared => TargetMaxDist * TargetMaxDist;

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, regardless of line of sight.
  /// </summary>
  public float TargetThroughWallDist = 80f;
  public float TargetThroughWallDistSquared => TargetThroughWallDist * TargetThroughWallDist;

  /// <summary>
  /// The knockback of Spirit Flame.
  /// </summary>
  public float Knockback = 0f;

  /// <summary>
  /// Starting homing strength of Spirit Flame.
  /// </summary>
  internal float HomingStrengthStart = 0.08f;

  /// <summary>
  /// Rate to increase homing strength every frame after <see cref="HomingIncreaseDelay"/>.
  /// </summary>
  internal float HomingIncreaseRate = 0.05f;

  /// <summary>
  /// Ticks to wait before increasing homing strength by <see cref="HomingIncreaseRate"/>.
  /// </summary>
  internal int HomingIncreaseDelay = 12;

  /// <summary>
  /// Speed of Spirit Flame when it is fired.
  /// </summary>
  internal float ProjectileSpeedStart = 7.5f;

  /// <summary>
  /// Acceleration of Spirit Flame after waiting for <see cref="ProjectileSpeedIncreaseDelay"/>.
  /// </summary>
  internal float ProjectileSpeedIncreaseRate = 0.5f;

  /// <summary>
  /// Time to wait before increasing Spirit Flame speed by <see cref="ProjectileSpeedIncreaseRate"/>.
  /// </summary>
  internal int ProjectileSpeedIncreaseDelay = 8;

  internal int SeinWidth = 10;
  internal int SeinHeight = 11;
  internal int SpiritFlameWidth = 12;
  internal int SpiritFlameHeight = 12;

  /// <summary>
  /// The size of the dust trail emitted from Spirit Flame.
  /// </summary>
  public float DustScale = 1.65f;

  /// <summary>
  /// Rarity of the Spirit Orb.
  /// </summary>
  internal int Rarity = 1;

  /// <summary>
  /// Buy value of the Spirit Orb.
  /// </summary>
  internal int Value = 1000;

  /// <summary>
  /// Color of the Spirit Orb, Sein, Spirit Flame, and emitted lights.
  /// </summary>
  internal Color Color = Color.White;

  /// <summary>
  /// Strength of the light emitted from Sein and Spirit Flame.
  /// </summary>
  internal float LightStrength;

  internal string CalculateStuff(string tierName) {
    int minShotsPerBurst = ShotsToPrimaryTarget;
    int minDmgPerBurst = Damage * minShotsPerBurst;
    int minDmgPerAllBursts = minDmgPerBurst * Bursts;

    int maxShotsPerBurst = Math.Min(ShotsToPrimaryTarget + ShotsPerTarget * (Targets - 1), MaxShotsAtOnce);
    int maxDmgPerBurst = Damage * maxShotsPerBurst;
    int maxDmgPerAllBursts = maxDmgPerBurst * Bursts;

    int minDps = minDmgPerAllBursts * 60 / (CooldownMin * Bursts + CooldownLong);
    int maxDps = maxDmgPerAllBursts * 60 / (CooldownMin * Bursts + CooldownLong);

    return minShotsPerBurst == maxShotsPerBurst
      ? $"Sein ({tierName}): DPS:{minDps}, Shots:{minShotsPerBurst} Bursts:{Bursts} DMG per Burst:{minDmgPerBurst}, DMG per all Bursts:{minDmgPerAllBursts}"
      : $"Sein ({tierName}): DPS:{minDps}-{maxDps}, Shots:{minShotsPerBurst}-{maxShotsPerBurst} Bursts:{Bursts} DMG per Burst:{minDmgPerBurst}-{maxDmgPerBurst}, DMG per all Bursts:{minDmgPerAllBursts}-{maxDmgPerAllBursts}";
  }
  #endregion
}
