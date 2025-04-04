using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

  [MemberNotNullWhen(true, nameof(Npc))]
  [MemberNotNullWhen(false, nameof(NonNpcPosition))]
  private bool HasNpcTarget => Projectile.ai[0] == 0;

  private NPC? Npc => HasNpcTarget ? Main.npc[(int)Projectile.ai[1]] : null;

  private Vector2 NonNpcPosition => new(Projectile.ai[0], Projectile.ai[1]);

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

    ref readonly SeinData data = ref SeinLoader.Get(type);
    Projectile.knockBack = data.Knockback;
    Projectile.width = SeinData.SpiritFlameWidth;
    Projectile.height = SeinData.SpiritFlameHeight;
    _lerp = data.HomingStrengthStart;
    _speed = data.ProjectileSpeedStart;
  }

  private void CreateDust() {
    ref readonly SeinData data = ref SeinLoader.Get(type);
    Dust dust = Dust.NewDustDirect(Projectile.position, 10, 10, _dustType);
    dust.scale = data.DustScale;
    dust.velocity = (_targetPosition - Projectile.Center).LengthSquared() >= SpeedSquared
      ? Projectile.velocity * 0.01f
      : Vector2.Zero;

    dust.rotation = MathF.Atan2(Projectile.velocity.Y, Projectile.velocity.X) - MathF.PI / 180 * 270;
    dust.position = Projectile.Center;
    dust.color = Color.Lerp(data.Color.Brightened(), Color.White, 0.85f);
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void TickTimerOrValue(ref int currentTime, int maxTime, ref float currentValue, float maxValue,
    float valueRate) {
    if (currentTime < maxTime) {
      currentTime++;
    }
    else if (currentValue < maxValue) {
      currentValue = Math.Min(currentValue + valueRate, maxValue);
    }
  }

  /// <summary>
  /// Update the target position.
  /// </summary>
  /// <remarks>
  /// Our ai fields are a bit weird...
  /// <para>If <c>ai[0]</c> is zero, we are targeting an NPC, whose whoAmI is <c>ai[1]</c>.</para>
  /// <para>If <c>ai[0]</c> is non-zero, we are not targeting an NPC, so <c>ai</c> fields are a Vector2 for where to land.</para>
  /// </remarks>
  private void UpdateTargetPosition() {
    if (HasNpcTarget) {
      NPC npc = Npc;
      if (npc.active) {
        _targetPosition = npc.Center;
      }
    }
    else {
      _targetPosition = NonNpcPosition;
    }
  }

  public override void AI() {
    ref readonly SeinData data = ref SeinLoader.Get(type);

    Lighting.AddLight(Projectile.Center, data.Color.ToVector3() * data.LightStrength);
    CreateDust();

    // Update target position until it dies
    // If target dies before projectile hits, target last live position
    UpdateTargetPosition();

    // Despawn when projectile reaches destination
    Vector2 offset = _targetPosition - Projectile.Center;

    if (offset.LengthSquared() < SpeedSquared) {
      Projectile.velocity = offset;
      if (Projectile.timeLeft > 2) {
        Projectile.timeLeft = 2;
      }

      return;
    }

    // Increase homing strength over time
    TickTimerOrValue(ref _currentLerpDelay, data.HomingIncreaseDelay, ref _lerp, 1, data.HomingIncreaseRate);

    // Increase speed over time
    TickTimerOrValue(ref _currentAccelerationDelay, data.ProjectileSpeedIncreaseDelay, ref _speed, 30,
      data.ProjectileSpeedIncreaseRate);

    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(default), offset.SafeNormalize(default), _lerp) * _speed;
  }

  public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
    Projectile.active = false;
  }
}
