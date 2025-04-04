using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod;

[UsedImplicitly]
public sealed class SeinLoader : ModSystem {
  public static int Count => 8;

  /// <summary>
  /// Collection of all <see cref="SeinData"/>s in the mod.
  /// </summary>
  private static SeinData[] All { get; set; } = null!; // Load()

  public static int[] BuffTypes { get; private set; } = null!; // Load()

  public static int[] MinionTypes { get; private set; } = null!; // Load()

  public static int[] SpiritFlameTypes { get; private set; } = null!; // Load()

  public static ref readonly SeinData Get(int index) {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(index);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(index, All.Length);

    return ref All[index - 1];
  }

  public static int BuffType(int index) {
    AssertInRange(index);
    return BuffTypes[index - 1];
  }

  public static int MinionType(int index) {
    AssertInRange(index);
    return MinionTypes[index - 1];
  }

  public static int SpiritFlameType(int index) {
    AssertInRange(index);
    return SpiritFlameTypes[index - 1];
  }

  private static void AssertInRange(int index) {
    ArgumentOutOfRangeException.ThrowIfLessThan(index, 1);
    ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);
  }

  /// <summary>
  /// Loads all Sein variants. Sein stats are hardcoded into this method.
  /// </summary>
  public override void Load() {
    ReadOnlySpan<(int rarity, int value)> itemInfo = [
      (ItemRarityID.Blue, Item.buyPrice(silver: 10)),
      (ItemRarityID.Green, Item.buyPrice(silver: 30)),
      (ItemRarityID.Orange, Item.buyPrice(gold: 1)),
      (ItemRarityID.LightRed, Item.buyPrice(gold: 2, silver: 50)),
      (ItemRarityID.Pink, Item.buyPrice(gold: 5)),
      (ItemRarityID.Yellow, Item.buyPrice(gold: 10)),
      (ItemRarityID.Cyan, Item.buyPrice(gold: 25)),
      (ItemRarityID.Red, Item.buyPrice(gold: 50))
    ];

    ReadOnlySpan<(float scale, float lightStrength, Color color)> aesthetic = [
      (1.6f, 0.4f, Color.White),
      (1.8f, 1.6f, new Color(108, 92, 172)),
      (2.0f, 1.3f, new Color(240, 0, 0, 194)),
      (2.2f, 1.2f, new Color(185, 248, 248)),
      (2.4f, 1.4f, new Color(255, 228, 160)),
      (2.6f, 2.3f, new Color(0, 180, 174, 210)),
      (3.0f, 4.5f, new Color(78, 38, 102)),
      (3.3f, 2.5f, new Color(220, 220, 220))
    ];

    ReadOnlySpan<(float start, int delay, float rate)> projectileSpeeds = [
      (07.5f, 08, 0.50f),
      (09.0f, 08, 0.50f),
      (12.5f, 10, 0.70f),
      (13.5f, 10, 0.70f),
      (13.5f, 10, 0.70f),
      (15.0f, 14, 0.85f),
      (16.0f, 14, 0.85f),
      (20.0f, 28, 1.25f)
    ];

    ReadOnlySpan<(float start, int delay, float rate, int startOffset)> homingStrengths = [
      (0.08f, 12, 0.050f, 040),
      (0.08f, 12, 0.050f, 040),
      (0.08f, 12, 0.050f, 100),
      (0.08f, 20, 0.060f, 060),
      (0.08f, 17, 0.060f, 060),
      (0.08f, 17, 0.070f, 070),
      (0.08f, 17, 0.025f, 120),
      (0.05f, 15, 0.025f, 180)
    ];

    ReadOnlySpan<(float max, float throughWall)> targetDistances = [
      (15, 05),
      (15, 05),
      (23, 05),
      (23, 05),
      (28, 05),
      (28, 14),
      (32, 14),
      (40, 23)
    ];

    ReadOnlySpan<(int bursts, int cdMin, int cdShort, int cdLong)> cooldowns = [
      (2, 10, 15, 30),
      (3, 10, 15, 30),
      (3, 05, 16, 35),
      (3, 11, 25, 40),
      (3, 11, 25, 40),
      (3, 12, 26, 52),
      (3, 12, 26, 52),
      (4, 16, 24, 55)
    ];

    ReadOnlySpan<(int dmg, int maxShots, int maxTargets, int shotsToPrimary, int shotsToOther)> damageInfo = [
      (18, 01, 1, 1, 1),
      (24, 01, 1, 1, 1),
      (29, 02, 2, 1, 1),
      (33, 03, 2, 2, 1),
      (37, 04, 3, 2, 1),
      (42, 05, 3, 3, 1),
      (47, 09, 4, 3, 2),
      (53, 10, 6, 4, 2)
    ];

    All = new SeinData[Count];
    for (int i = 0; i < Count; i++) {
      All[i] = new SeinData {
        Rarity = itemInfo[i].rarity,
        Value = itemInfo[i].value,

        Damage = damageInfo[i].dmg,
        Targets = damageInfo[i].maxTargets,
        ShotsPerTarget = damageInfo[i].shotsToOther,
        ShotsToPrimaryTarget = damageInfo[i].shotsToPrimary,
        MaxShotsAtOnce = damageInfo[i].maxShots,

        TargetMaxDistTiles = targetDistances[i].max,
        TargetThroughWallDistTiles = targetDistances[i].throughWall,

        ProjectileSpeedStart = projectileSpeeds[i].start,
        ProjectileSpeedIncreaseDelay = projectileSpeeds[i].delay,
        ProjectileSpeedIncreaseRate = projectileSpeeds[i].rate,

        RandDegrees = homingStrengths[i].startOffset,
        HomingStrengthStart = homingStrengths[i].start,
        HomingIncreaseDelay = homingStrengths[i].delay,
        HomingIncreaseRate = homingStrengths[i].rate,

        Bursts = cooldowns[i].bursts,
        CooldownMin = cooldowns[i].cdMin,
        CooldownShort = cooldowns[i].cdShort,
        CooldownLong = cooldowns[i].cdLong,

        Color = aesthetic[i].color,
        DustScale = aesthetic[i].scale,
        LightStrength = aesthetic[i].lightStrength
      };
    }
  }

  public override void OnModLoad() {
    OriMod mod = OriMod.Instance;
    BuffTypes = new int[Count];
    MinionTypes = new int[Count];
    SpiritFlameTypes = new int[Count];
    for (int i = 0; i < Count; i++) {
      BuffTypes[i] = mod.Find<ModBuff>($"SeinBuff{i + 1}").Type;
      MinionTypes[i] = mod.Find<ModProjectile>($"Sein{i + 1}").Type;
      SpiritFlameTypes[i] = mod.Find<ModProjectile>($"SpiritFlame{i + 1}").Type;
    }
  }

  public override void Unload() {
    All = null!;
    BuffTypes = null!;
    MinionTypes = null!;
    SpiritFlameTypes = null!;
  }
}
