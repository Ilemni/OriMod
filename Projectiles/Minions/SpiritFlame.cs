using System;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions;

/// <summary>
/// Projectile fired by the minion <see cref="Sein"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class SpiritFlame(int type) : ModProjectile {
  /// <summary>
  /// Current position the projectile is moving towards.
  /// </summary>
  private Vector2 _targetPosition;

  /// <summary>
  /// Stats for speed, damage, etc. Assigned in <see cref="SetDefaults"/>.
  /// </summary>
  private SeinData _data;

  /// <summary>
  /// Current homing strength of the projectile. Increases over time by <see cref="SeinData.HomingIncreaseRate"/>.
  /// <para>0 = no homing; 1 = full homing.</para>
  /// </summary>
  private float _lerp;

  /// <summary>
  /// Elapsed time for <see cref="SeinData.HomingIncreaseDelay"/>.
  /// </summary>
  private int _currentLerpDelay;

  /// <summary>
  /// Current speed of the projectile. Increases over time by <see cref="SeinData.ProjectileSpeedIncreaseRate"/>.
  /// </summary>
  private float _speed;
  private float SpeedSquared => _speed * _speed;
  /// <summary>
  /// Elapsed time for <see cref="SeinData.ProjectileSpeedIncreaseDelay"/>.
  /// </summary>
  private int _currentAccelerationDelay;

  private int _dustType;

  public override string Texture => "OriMod/Projectiles/Minions/SpiritFlame";

  public override void SetStaticDefaults() {
    ProjectileID.Sets.CanDistortWater[Projectile.type] = true;
    ProjectileID.Sets.MinionShot[Projectile.type] = true;
  }

  public override void SetDefaults() {
    Projectile.alpha = 32;
    Projectile.friendly = true;
    Projectile.minion = true;
    Projectile.ignoreWater = true;
    Projectile.tileCollide = false;
    _dustType = ModContent.DustType<SpiritFlameDustTrail>();

    _data = SeinData.All[SpiritFlameType - 1];
    Projectile.knockBack = _data.Knockback;
    Projectile.width = _data.SpiritFlameWidth;
    Projectile.height = _data.SpiritFlameHeight;
    _lerp = _data.HomingStrengthStart;
    _speed = _data.ProjectileSpeedStart;
  }

  /// <summary>
  /// Type for <see cref="SpiritFlame"/>. Determines initialized values by using <see cref="SeinData.Get"/>.
  /// </summary>
  private int SpiritFlameType { get; } = type;

  private void CreateDust() {
    Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, _dustType);
    dust.scale = _data.DustScale;
    dust.velocity = (_targetPosition - Projectile.Center).LengthSquared() >= SpeedSquared ? Projectile.velocity * 0.01f : Vector2.Zero;

    dust.rotation = (float)(Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) - Math.PI / 180 * 270);
    dust.position = Projectile.Center;
    dust.color = Color.Lerp(_data.Color.Brightened(), Color.White, 0.85f);
    dust.color.A = 230;
  }

  /// <summary>
  /// Increment <paramref name="currentTime"/> if less than <paramref name="maxTime"/>, else increase <paramref name="currentValue"/> by <paramref name="valueRate"/> until <paramref name="maxValue"/>.
  /// </summary>
  /// <param name="currentTime">Current time.</param>
  /// <param name="maxTime">Time needed before increasing <paramref name="currentValue"/>.</param>
  /// <param name="currentValue">Value to increase if enough time has passed.</param>
  /// <param name="maxValue">Maximum value <paramref name="currentValue"/> can be.</param>
  /// <param name="valueRate">Rate that <paramref name="currentValue"/> will increase by.</param>
  private static void TickTimerOrValue(ref int currentTime, int maxTime, ref float currentValue, float maxValue, float valueRate) {
    if (currentTime < maxTime) {
      currentTime++;
    }
    else if (currentValue < maxValue) {
      currentValue += valueRate;
      if (currentValue > maxValue) {
        currentValue = maxValue;
      }
    }
  }

  /// <summary>
  /// Update the target position.
  /// </summary>
  /// <remarks>
  /// Our ai fields are a bit weird...
  /// <para>If ai[0] is zero, we are targeting an NPC, whose whoAmI is ai[1].</para>
  /// <para>If ai[0] is non-zero, we are not targeting an NPC, so ai fields are a Vector2 for where to land.</para>
  /// </remarks>
  private void UpdateTargetPosition() {
    if (Projectile.ai[0] == 0) {
      NPC npc = Main.npc[(int)Projectile.ai[1]];
      if (npc.active) {
        _targetPosition = npc.Center;
      }
    }
    else {
      _targetPosition.X = Projectile.ai[0];
      _targetPosition.Y = Projectile.ai[1];
    }
  }

  public override void AI() {
    Lighting.AddLight(Projectile.Center, _data.Color.ToVector3() * _data.LightStrength);
    CreateDust();

    // Update target position until it dies
    // If target dies before projectile hits, target last live position
    UpdateTargetPosition();

    // Despawn when projectile reaches destination
    Vector2 offset = _targetPosition - Projectile.Center;
    float distanceSquared = offset.LengthSquared();

    if (distanceSquared < SpeedSquared) {
      Projectile.velocity = offset;
      if (Projectile.timeLeft > 2) {
        Projectile.timeLeft = 2;
      }
      return;
    }

    // Increase homing strength over time
    TickTimerOrValue(ref _currentLerpDelay, _data.HomingIncreaseDelay, ref _lerp, 1, _data.HomingIncreaseRate);

    // Increase speed over time
    TickTimerOrValue(ref _currentAccelerationDelay, _data.ProjectileSpeedIncreaseDelay, ref _speed, 30, _data.ProjectileSpeedIncreaseRate);

    Projectile.velocity = Vector2.Lerp(Projectile.velocity.Normalized(), offset.Normalized(), _lerp) * _speed;
  }

  public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
    Projectile.active = false;
  }
}
