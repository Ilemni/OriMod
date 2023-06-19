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
        if (newVal.ToString() == defVal.ToString()) {
          newVal = oldVal;
        }
        field.SetValue(newSein, newVal);
      }
      list.Add(newSein);
      //OriMod.Log.Debug(newSein.CalculateStuff(tierName));
    }

    // Tier 1 (Silver)
    AddNewSein(new SeinData() {
      lightStrength = 0.4f
    });

    // Tier 2 (Demonite/Crimtane)
    // Increased shots per burst
    // Max damage per burst: 15
    AddNewSein(new SeinData {
      rarity = ItemRarityID.Green,
      value = 3000,
      color = new Color(108, 92, 172),

      damage = 24,
      targets = 1,
      bursts = 3,
      projectileSpeedStart = 9f,
      homingIncreaseRate = 0.05f,
      dustScale = 1.8f,
      lightStrength = 1.6f,
    });

    // Tier 3 (Hellstone)
    // 2 targets
    // Max damage per burst: 42
    // For some sort of "rage" effect to pair with red theme, lower CD
    AddNewSein(new SeinData {
      rarity = ItemRarityID.Orange,
      value = 10000,
      color = new Color(240, 0, 0, 194),

      damage = 29,
      targets = 2,
      maxShotsAtOnce = 2,
      randDegrees = 100,
      cooldownMin = 5,
      cooldownShort = 16,
      cooldownLong = 35,
      projectileSpeedStart = 12.5f,
      projectileSpeedIncreaseRate = 0.7f,
      projectileSpeedIncreaseDelay = 10,
      targetMaxDist = 370f,
      dustScale = 2f,
      lightStrength = 1.275f,
    });

    // Tier 4 (Mythril/Orichalcum)
    // 2 targets, 2 shots to primary, 3 shots max (rather than 4)
    // Max damage per burst: 81
    AddNewSein(new SeinData {
      rarity = ItemRarityID.LightRed,
      value = 25000,
      color = new Color(185, 248, 248),

      damage = 33,
      shotsToPrimaryTarget = 2,
      maxShotsAtOnce = 3,
      randDegrees = 60,
      cooldownMin = 11,
      cooldownShort = 25,
      cooldownLong = 40,
      projectileSpeedStart = 13.5f,
      homingIncreaseRate = 0.06f,
      homingIncreaseDelay = 20,
      dustScale = 2.2f,
      lightStrength = 1.2f,
    });

    // Tier 5 (Hallow)
    // 3 targets, 2 shots to primary, 4 shots max (rather than 5)
    // Max damage per burst: 132
    AddNewSein(new SeinData {
      rarity = ItemRarityID.Pink,
      value = 50000,
      color = new Color(255, 228, 160),

      damage = 37,
      targets = 3,
      maxShotsAtOnce = 4,
      homingIncreaseDelay = 17,
      targetMaxDist = 440f,
      dustScale = 2.4f,
      lightStrength = 1.4f,
    });

    // Tier 6 (Spectral)
    // 3 targets, 3 shots to primary, 5 shots max
    // Max damage per burst: 195
    AddNewSein(new SeinData {
      rarity = ItemRarityID.Yellow,
      value = 100000,
      color = new Color(0, 180, 174, 210),

      damage = 42,
      shotsToPrimaryTarget = 3,
      maxShotsAtOnce = 5,
      cooldownMin = 12,
      cooldownShort = 26,
      cooldownLong = 52,
      targetThroughWallDist = 224f,
      homingIncreaseRate = 0.07f,
      projectileSpeedStart = 15f,
      projectileSpeedIncreaseRate = 0.85f,
      projectileSpeedIncreaseDelay = 14,
      randDegrees = 70,
      dustScale = 2.65f,
      lightStrength = 2.25f,
    });

    // Tier 7 (Lunar)
    // 4 targets, 3 shots to primary, 2 to others, 6 shots max (rather than 9)
    // Max damage per burst: 282
    AddNewSein(new SeinData {
      rarity = ItemRarityID.Cyan,
      value = 250000,
      color = new Color(78, 38, 102),

      damage = 47,
      targets = 4,
      shotsToPrimaryTarget = 3,
      shotsPerTarget = 2,
      maxShotsAtOnce = 9,
      homingIncreaseRate = 0.025f,
      projectileSpeedStart = 16f,
      targetMaxDist = 510f,
      randDegrees = 120,
      dustScale = 3f,
      lightStrength = 4.5f,
    });

    // Tier 8 (Lunar Bars)
    // 5 targets, 4 shots to primary, 2 shots to others, 10 shots max (rather than 12)
    // Max damage per burst: 530 (too high?)
    AddNewSein(new SeinData {
      rarity = ItemRarityID.Red,
      value = 500000,
      color = new Color(220, 220, 220),

      damage = 53,
      bursts = 4,
      targets = 6,
      shotsToPrimaryTarget = 4,
      maxShotsAtOnce = 10,
      cooldownMin = 16,
      cooldownShort = 24,
      cooldownLong = 55,
      homingStrengthStart = 0.05f,
      homingIncreaseDelay = 15,
      projectileSpeedStart = 20f,
      projectileSpeedIncreaseRate = 1.25f,
      projectileSpeedIncreaseDelay = 28,
      randDegrees = 180,
      targetMaxDist = 650f,
      targetThroughWallDist = 370f,
      dustScale = 3.35f,
      lightStrength = 2.5f,
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
  public int damage = 18;

  /// <summary>
  /// Number of NPCs that can be targeted at once.
  /// </summary>
  public int targets = 1;

  /// <summary>
  /// Maximum times the minion can fire with a delay of <see cref="cooldownMin"/> before having a delay of <see cref="cooldownLong"/>.
  /// </summary>
  public int bursts = 2;

  /// <summary>
  /// Maximum number of shots that can be fired at each target.
  /// </summary>
  public int shotsPerTarget = 1;

  /// <summary>
  /// Maximum number of shots that can be fired at the primary target at once.
  /// </summary>
  public int shotsToPrimaryTarget = 1;

  /// <summary>
  /// Maximum number of shots that can be fired at once.
  /// </summary>
  public int maxShotsAtOnce = 1;

  /// <summary>
  /// Delay between each shot in <see cref="bursts"/>.
  /// </summary>
  public int cooldownMin = 10;

  /// <summary>
  /// Shortest time to wait during <see cref="bursts"/> to reset burst count.
  /// </summary>
  public int cooldownShort = 15;

  /// <summary>
  /// Delay between each series of shots, incurred when shots reaches <see cref="bursts"/>.
  /// </summary>
  public int cooldownLong = 30;
  #endregion

  /// <summary>
  /// Maximum angle that fired Spirit Flames will be away from the target.
  /// </summary>
  internal int randDegrees = 40;

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, if there is line of sight between it and the player.
  /// </summary>
  public float targetMaxDist = 240f;
  public float TargetMaxDistSquared => targetMaxDist * targetMaxDist;

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, regardless of line of sight.
  /// </summary>
  public float targetThroughWallDist = 80f;
  public float TargetThroughWallDistSquared => targetThroughWallDist * targetThroughWallDist;

  /// <summary>
  /// The knockback of Spirit Flame.
  /// </summary>
  public float knockback = 0f;

  /// <summary>
  /// Starting homing strength of Spirit Flame.
  /// </summary>
  internal float homingStrengthStart = 0.08f;

  /// <summary>
  /// Rate to increase homing strength every frame after <see cref="homingIncreaseDelay"/>.
  /// </summary>
  internal float homingIncreaseRate = 0.05f;

  /// <summary>
  /// Ticks to wait before increasing homing strength by <see cref="homingIncreaseRate"/>.
  /// </summary>
  internal int homingIncreaseDelay = 12;

  /// <summary>
  /// Speed of Spirit Flame when it is fired.
  /// </summary>
  internal float projectileSpeedStart = 7.5f;

  /// <summary>
  /// Acceleration of Spirit Flame after waiting for <see cref="projectileSpeedIncreaseDelay"/>.
  /// </summary>
  internal float projectileSpeedIncreaseRate = 0.5f;

  /// <summary>
  /// Time to wait before increasing Spirit Flame speed by <see cref="projectileSpeedIncreaseRate"/>.
  /// </summary>
  internal int projectileSpeedIncreaseDelay = 8;

  internal int seinWidth = 10;
  internal int seinHeight = 11;
  internal int spiritFlameWidth = 12;
  internal int spiritFlameHeight = 12;

  /// <summary>
  /// The size of the dust trail emitted from Spirit Flame.
  /// </summary>
  public float dustScale = 1.65f;

  /// <summary>
  /// Rarity of the Spirit Orb.
  /// </summary>
  internal int rarity = 1;

  /// <summary>
  /// Buy value of the Spirit Orb.
  /// </summary>
  internal int value = 1000;

  /// <summary>
  /// Color of the Spirit Orb, Sein, Spirit Flame, and emitted lights.
  /// </summary>
  internal Color color = Color.White;

  /// <summary>
  /// Strength of the light emitted from Sein and Spirit Flame.
  /// </summary>
  internal float lightStrength;

  internal string CalculateStuff(string tierName) {
    int minShotsPerBurst = shotsToPrimaryTarget;
    int minDmgPerBurst = damage * minShotsPerBurst;
    int minDmgPerAllBursts = minDmgPerBurst * bursts;

    int maxShotsPerBurst = Math.Min(shotsToPrimaryTarget + shotsPerTarget * (targets - 1), maxShotsAtOnce);
    int maxDmgPerBurst = damage * maxShotsPerBurst;
    int maxDmgPerAllBursts = maxDmgPerBurst * bursts;

    int minDps = minDmgPerAllBursts * 60 / (cooldownMin * bursts + cooldownLong);
    int maxDps = maxDmgPerAllBursts * 60 / (cooldownMin * bursts + cooldownLong);

    return minShotsPerBurst == maxShotsPerBurst
      ? $"Sein ({tierName}): DPS:{minDps}, Shots:{minShotsPerBurst} Bursts:{bursts} DMG per Burst:{minDmgPerBurst}, DMG per all Bursts:{minDmgPerAllBursts}"
      : $"Sein ({tierName}): DPS:{minDps}-{maxDps}, Shots:{minShotsPerBurst}-{maxShotsPerBurst} Bursts:{bursts} DMG per Burst:{minDmgPerBurst}-{maxDmgPerBurst}, DMG per all Bursts:{minDmgPerAllBursts}-{maxDmgPerAllBursts}";
  }
  #endregion
}
