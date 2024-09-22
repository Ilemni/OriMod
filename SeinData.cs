using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod;

public readonly record struct SeinTypeInfo(int Buff, int Minion, int SpiritFlame);

public readonly record struct SeinData {
  public SeinData() {
  }

  /// <summary>
  /// Collection of all <see cref="SeinData"/>s in the mod.
  /// </summary>
  private static SeinData[] All { get; set; } = null!; // OriMod.Load() -> SeinData.Load()

  /// <summary>
  /// Collection of all <see cref="Buffs.SeinBuff{T}"/> IDs.
  /// </summary>
  public static SeinTypeInfo[] Ids { get; private set; } = null!; // OriMod.Load() -> SeinData.Load()

  public static ref SeinData Get(int index) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(index, All.Length);

    return ref All[index - 1];
  }

  public static SeinTypeInfo GetSeinTypeInfo(int index) {
    ArgumentOutOfRangeException.ThrowIfLessThan(index, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Ids.Length);

    return Ids[index - 1];
  }

  /// <summary>
  /// Loads all Sein variants. Sein stats are hardcoded into this method.
  /// </summary>
  internal static void Load() {
    var list = new List<SeinData> {
      // Tier 1 (Silver)
      new() {
        Rarity = ItemRarityID.Blue,
        Value = 1000,
        Color = Color.White,

        Damage = 18,
        Targets = 1,
        Bursts = 2,
        ShotsPerTarget = 1,
        ShotsToPrimaryTarget = 1,
        MaxShotsAtOnce = 1,

        TargetMaxDistTiles = 15,
        TargetThroughWallDistTiles = 5,

        RandDegrees = 40,
        ProjectileSpeedStart = 7.5f,
        ProjectileSpeedIncreaseDelay = 8,
        ProjectileSpeedIncreaseRate = 0.5f,

        HomingStrengthStart = 0.08f,
        HomingIncreaseDelay = 12,
        HomingIncreaseRate = 0.05f,

        CooldownMin = 10,
        CooldownShort = 15,
        CooldownLong = 30,

        DustScale = 1.65f,
        LightStrength = 0.4f
      }
    };

    // Tier 2 (Demonite/Crimtane)
    // Increased shots per burst
    // Max damage per burst: 15
    list.Add(list[^1] with {
      Rarity = ItemRarityID.Green,
      Value = 3000,
      Color = new Color(108, 92, 172),

      Damage = 24,
      Targets = 1,
      Bursts = 3,

      ProjectileSpeedStart = 9f,

      HomingIncreaseRate = 0.05f,

      DustScale = 1.8f,
      LightStrength = 1.6f
    });

    // Tier 3 (Hellstone)
    // 2 targets
    // Max damage per burst: 42
    // For some sort of "rage" effect to pair with red theme, lower CD
    list.Add(list[^1] with {
      Rarity = ItemRarityID.Orange,
      Value = 10000,
      Color = new Color(240, 0, 0, 194),

      Damage = 29,
      Targets = 2,
      MaxShotsAtOnce = 2,

      TargetMaxDistTiles = 23,

      RandDegrees = 100,
      ProjectileSpeedStart = 12.5f,
      ProjectileSpeedIncreaseDelay = 10,
      ProjectileSpeedIncreaseRate = 0.7f,

      CooldownMin = 5,
      CooldownShort = 16,
      CooldownLong = 35,

      DustScale = 2f,
      LightStrength = 1.275f
    });

    // Tier 4 (Mithril/Orichalcum)
    // 2 targets, 2 shots to primary, 3 shots max (rather than 4)
    // Max damage per burst: 81
    list.Add(list[^1] with {
      Rarity = ItemRarityID.LightRed,
      Value = 25000,
      Color = new Color(185, 248, 248),

      Damage = 33,
      ShotsToPrimaryTarget = 2,
      MaxShotsAtOnce = 3,

      RandDegrees = 60,
      ProjectileSpeedStart = 13.5f,

      HomingIncreaseDelay = 20,
      HomingIncreaseRate = 0.06f,

      CooldownMin = 11,
      CooldownShort = 25,
      CooldownLong = 40,

      DustScale = 2.2f,
      LightStrength = 1.2f,
    });

    // Tier 5 (Hallow)
    // 3 targets, 2 shots to primary, 4 shots max (rather than 5)
    // Max damage per burst: 132
    list.Add(list[^1] with {
      Rarity = ItemRarityID.Pink,
      Value = 50000,
      Color = new Color(255, 228, 160),

      Damage = 37,
      Targets = 3,
      MaxShotsAtOnce = 4,

      TargetMaxDistTiles = 27.5f,

      HomingIncreaseDelay = 17,

      DustScale = 2.4f,
      LightStrength = 1.4f,
    });

    // Tier 6 (Spectral)
    // 3 targets, 3 shots to primary, 5 shots max.
    // Max damage per burst: 195
    list.Add(list[^1] with {
      Rarity = ItemRarityID.Yellow,
      Value = 100000,
      Color = new Color(0, 180, 174, 210),

      Damage = 42,
      ShotsToPrimaryTarget = 3,
      MaxShotsAtOnce = 5,

      TargetThroughWallDistTiles = 14,

      RandDegrees = 70,
      ProjectileSpeedStart = 15f,
      ProjectileSpeedIncreaseDelay = 14,
      ProjectileSpeedIncreaseRate = 0.85f,

      HomingIncreaseRate = 0.07f,

      CooldownMin = 12,
      CooldownShort = 26,
      CooldownLong = 52,

      DustScale = 2.65f,
      LightStrength = 2.25f,
    });

    // Tier 7 (Lunar)
    // 4 targets, 3 shots to primary, 2 to others, 6 shots max (rather than 9)
    // Max damage per burst: 282
    list.Add(list[^1] with {
      Rarity = ItemRarityID.Cyan,
      Value = 250000,
      Color = new Color(78, 38, 102),

      Damage = 47,
      Targets = 4,
      ShotsPerTarget = 2,
      ShotsToPrimaryTarget = 3,
      MaxShotsAtOnce = 9,

      TargetMaxDistTiles = 32,

      RandDegrees = 120,
      ProjectileSpeedStart = 16f,

      HomingIncreaseRate = 0.025f,

      DustScale = 3f,
      LightStrength = 4.5f,
    });

    // Tier 8 (Lunar Bars)
    // 5 targets, 4 shots to primary, 2 shots to others, 10 shots max (rather than 12)
    // Max damage per burst: 530 (too high?)
    list.Add(list[^1] with {
      Rarity = ItemRarityID.Red,
      Value = 500000,
      Color = new Color(220, 220, 220),

      Damage = 53,
      Bursts = 4,
      Targets = 6,
      ShotsToPrimaryTarget = 4,
      MaxShotsAtOnce = 10,

      TargetMaxDistTiles = 40,
      TargetThroughWallDistTiles = 23,

      RandDegrees = 180,
      ProjectileSpeedStart = 20f,
      ProjectileSpeedIncreaseRate = 1.25f,
      ProjectileSpeedIncreaseDelay = 28,

      HomingStrengthStart = 0.05f,
      HomingIncreaseDelay = 15,

      CooldownMin = 16,
      CooldownShort = 24,
      CooldownLong = 55,

      DustScale = 3.35f,
      LightStrength = 2.5f,
    });

    All = list.ToArray();
    Ids = new SeinTypeInfo[list.Count];

    for (int u = 0; u < list.Count; u++) {
      Ids[u] = new SeinTypeInfo {
        Buff = OriMod.instance.Find<ModBuff>("SeinBuff" + (u + 1)).Type,
        Minion = OriMod.instance.Find<ModProjectile>("Sein" + (u + 1)).Type,
        SpiritFlame = OriMod.instance.Find<ModProjectile>("SpiritFlame" + (u + 1)).Type
      };
    }
  }

  #region Stats

  #region Stats responsible for DPS

  /// <summary>
  /// Damage dealt by Spirit Flame.
  /// </summary>
  public int Damage { get; private init; }

  /// <summary>
  /// Number of NPCs that can be targeted at once.
  /// </summary>
  public int Targets { get; private init; }

  /// <summary>
  /// Maximum times the minion can fire with a delay of <see cref="CooldownMin"/> before having a delay of <see cref="CooldownLong"/>.
  /// </summary>
  public int Bursts { get; private init; }

  /// <summary>
  /// Maximum number of shots that can be fired at each target.
  /// </summary>
  public int ShotsPerTarget { get; private init; }

  /// <summary>
  /// Maximum number of shots that can be fired at the primary target at once.
  /// </summary>
  public int ShotsToPrimaryTarget { get; private init; }

  /// <summary>
  /// Maximum number of shots that can be fired at once.
  /// </summary>
  public int MaxShotsAtOnce { get; private init; }

  /// <summary>
  /// Delay between each shot in <see cref="Bursts"/>.
  /// </summary>
  public int CooldownMin { get; private init; }

  /// <summary>
  /// Shortest time to wait during <see cref="Bursts"/> to reset burst count.
  /// </summary>
  public int CooldownShort { get; private init; }

  /// <summary>
  /// Delay between each series of shots, incurred when shots reaches <see cref="Bursts"/>.
  /// </summary>
  public int CooldownLong { get; private init; }

  #endregion

  /// <summary>
  /// Maximum angle that fired Spirit Flames will be away from the target.
  /// </summary>
  internal int RandDegrees { get; private init; }

  /// <summary>
  /// Value of <see cref="TargetMaxDist"/>, in tiles.
  /// <para />
  /// <inheritdoc cref="TargetMaxDist"/>
  /// </summary>
  public float TargetMaxDistTiles {
    get => TargetMaxDist / 16;
    init => TargetMaxDist = value * 16;
  }

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, if there is line of sight between it and the player.
  /// </summary>
  public float TargetMaxDist { get; private init; }

  /// <summary>
  /// Squared version of <see cref="TargetMaxDist"/>
  /// <para />
  /// <inheritdoc cref="TargetMaxDist"/>
  /// </summary>
  public float TargetMaxDistSquared => MathF.Pow(TargetMaxDist, 2);

  /// <summary>
  /// Value of <see cref="TargetThroughWallDist"/>, in tiles.
  /// <para />
  /// <inheritdoc cref="TargetThroughWallDist"/>
  /// </summary>
  public float TargetThroughWallDistTiles {
    get => TargetThroughWallDist / 16;
    init => TargetThroughWallDist = value * 16;
  }

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, regardless of line of sight.
  /// </summary>
  public float TargetThroughWallDist { get; private init; }

  /// <summary>
  /// Squared version of <see cref="TargetThroughWallDist"/>
  /// <para />
  /// <inheritdoc cref="TargetThroughWallDist"/>
  /// </summary>
  public float TargetThroughWallDistSquared => MathF.Pow(TargetThroughWallDist, 2);

  /// <summary>
  /// The knockback of Spirit Flame.
  /// </summary>
  public float Knockback { get; private init; } = 0f;

  /// <summary>
  /// Starting homing strength of Spirit Flame.
  /// </summary>
  internal float HomingStrengthStart { get; private init; }

  /// <summary>
  /// Rate to increase homing strength every frame after <see cref="HomingIncreaseDelay"/>.
  /// </summary>
  internal float HomingIncreaseRate { get; private init; }

  /// <summary>
  /// Ticks to wait before increasing homing strength by <see cref="HomingIncreaseRate"/>.
  /// </summary>
  internal int HomingIncreaseDelay { get; private init; }

  /// <summary>
  /// Speed of Spirit Flame when it is fired.
  /// </summary>
  internal float ProjectileSpeedStart { get; private init; }

  /// <summary>
  /// Acceleration of Spirit Flame after waiting for <see cref="ProjectileSpeedIncreaseDelay"/>.
  /// </summary>
  internal float ProjectileSpeedIncreaseRate { get; private init; }

  /// <summary>
  /// Time to wait before increasing Spirit Flame speed by <see cref="ProjectileSpeedIncreaseRate"/>.
  /// </summary>
  internal int ProjectileSpeedIncreaseDelay { get; private init; }

  internal const int SeinWidth = 10;
  internal const int SeinHeight = 11;
  internal const int SpiritFlameWidth = 12;
  internal const int SpiritFlameHeight = 12;

  /// <summary>
  /// The size of the dust trail emitted from Spirit Flame.
  /// </summary>
  public float DustScale { get; private init; }

  /// <summary>
  /// Rarity of the Spirit Orb.
  /// </summary>
  internal int Rarity { get; private init; }

  /// <summary>
  /// Buy value of the Spirit Orb.
  /// </summary>
  internal int Value { get; private init; }

  /// <summary>
  /// Color of the Spirit Orb, Sein, Spirit Flame, and emitted lights.
  /// </summary>
  internal Color Color { get; private init; }

  /// <summary>
  /// Strength of the light emitted from Sein and Spirit Flame.
  /// </summary>
  internal float LightStrength { get; private init; }

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
