using System;
using Microsoft.Xna.Framework;

namespace OriMod;

public readonly struct SeinData {
  public SeinData() {
  }

  #region Stats

  #region Stats responsible for DPS

  /// <summary>
  /// Damage dealt by Spirit Flame.
  /// </summary>
  public int Damage { get; init; }

  /// <summary>
  /// Number of NPCs that can be targeted at once.
  /// </summary>
  public int Targets { get; init; }

  /// <summary>
  /// Maximum times the minion can fire with a delay of <see cref="CooldownMin"/> before having a delay of <see cref="CooldownLong"/>.
  /// </summary>
  public int Bursts { get; init; }

  /// <summary>
  /// Maximum number of shots that can be fired at each target.
  /// </summary>
  public int ShotsPerTarget { get; init; }

  /// <summary>
  /// Maximum number of shots that can be fired at the primary target at once.
  /// </summary>
  public int ShotsToPrimaryTarget { get; init; }

  /// <summary>
  /// Maximum number of shots that can be fired at once.
  /// </summary>
  public int MaxShotsAtOnce { get; init; }

  /// <summary>
  /// Delay between each shot in <see cref="Bursts"/>.
  /// </summary>
  public int CooldownMin { get; init; }

  /// <summary>
  /// Shortest time to wait during <see cref="Bursts"/> to reset burst count.
  /// </summary>
  public int CooldownShort { get; init; }

  /// <summary>
  /// Delay between each series of shots, incurred when shots reaches <see cref="Bursts"/>.
  /// </summary>
  public int CooldownLong { get; init; }

  #endregion

  /// <summary>
  /// Maximum angle that fired Spirit Flames will be away from the target.
  /// </summary>
  internal int RandDegrees { get; init; }

  /// <summary>
  /// Value of <see cref="TargetMaxDist"/>, in tiles.
  /// <para/> <inheritdoc cref="TargetMaxDist"/>
  /// </summary>
  public float TargetMaxDistTiles {
    get => TargetMaxDist / 16;
    init => TargetMaxDist = value * 16;
  }

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, if there is line of sight between it and the player.
  /// </summary>
  public float TargetMaxDist { get; init; }

  /// <summary>
  /// Squared version of <see cref="TargetMaxDist"/>
  /// <para/> <inheritdoc cref="TargetMaxDist"/>
  /// </summary>
  public float TargetMaxDistSquared => MathF.Pow(TargetMaxDist, 2);

  /// <summary>
  /// Value of <see cref="TargetThroughWallDist"/>, in tiles.
  /// <para/> <inheritdoc cref="TargetThroughWallDist"/>
  /// </summary>
  public float TargetThroughWallDistTiles {
    get => TargetThroughWallDist / 16;
    init => TargetThroughWallDist = value * 16;
  }

  /// <summary>
  /// NPCs within this distance from the player can be targeted by the minion, regardless of line of sight.
  /// </summary>
  public float TargetThroughWallDist { get; init; }

  /// <summary>
  /// Squared version of <see cref="TargetThroughWallDist"/>
  /// <para/> <inheritdoc cref="TargetThroughWallDist"/>
  /// </summary>
  public float TargetThroughWallDistSquared => MathF.Pow(TargetThroughWallDist, 2);

  /// <summary>
  /// The knockback of Spirit Flame.
  /// </summary>
  public float Knockback { get; init; } = 0f;

  /// <summary>
  /// Starting homing strength of Spirit Flame.
  /// </summary>
  internal float HomingStrengthStart { get; init; }

  /// <summary>
  /// Rate to increase homing strength every tick after <see cref="HomingIncreaseDelay"/>.
  /// </summary>
  internal float HomingIncreaseRate { get; init; }

  /// <summary>
  /// Ticks to wait before increasing homing strength by <see cref="HomingIncreaseRate"/>.
  /// </summary>
  internal int HomingIncreaseDelay { get; init; }

  /// <summary>
  /// Speed of Spirit Flame when it is fired.
  /// </summary>
  internal float ProjectileSpeedStart { get; init; }

  /// <summary>
  /// Acceleration of Spirit Flame after waiting for <see cref="ProjectileSpeedIncreaseDelay"/>.
  /// </summary>
  internal float ProjectileSpeedIncreaseRate { get; init; }

  /// <summary>
  /// Time to wait before increasing Spirit Flame speed by <see cref="ProjectileSpeedIncreaseRate"/>.
  /// </summary>
  internal int ProjectileSpeedIncreaseDelay { get; init; }

  internal const int SeinWidth = 10;
  internal const int SeinHeight = 11;
  internal const int SpiritFlameWidth = 12;
  internal const int SpiritFlameHeight = 12;

  /// <summary>
  /// The size of the dust trail emitted from Spirit Flame.
  /// </summary>
  public float DustScale { get; init; }

  /// <summary>
  /// Rarity of the Spirit Orb.
  /// </summary>
  internal int Rarity { get; init; }

  /// <summary>
  /// Buy value of the Spirit Orb.
  /// </summary>
  internal int Value { get; init; }

  /// <summary>
  /// Color of the Spirit Orb, Sein, Spirit Flame, and emitted lights.
  /// </summary>
  internal Color Color { get; init; }

  /// <summary>
  /// Strength of the light emitted from Sein and Spirit Flame.
  /// </summary>
  internal float LightStrength { get; init; }

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
