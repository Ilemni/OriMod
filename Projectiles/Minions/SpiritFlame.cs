using System;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  /// <summary>
  /// Projectile fired by the minion <see cref="Sein"/>.
  /// </summary>
  public abstract class SpiritFlame : ModProjectile {
    /// <summary>
    /// Current position the projectile is moving towards.
    /// </summary>
    private Vector2 _targetPosition;

    /// <summary>
    /// Stats for speed, damage, etc. Assigned in <see cref="SetDefaults"/>.
    /// </summary>
    private SeinData _data;

    /// <summary>
    /// Current homing strength of the projectile. Increases over time by <see cref="SeinData.homingIncreaseRate"/>.
    /// <para>0 = no homing; 1 = full homing.</para>
    /// </summary>
    private float _lerp;

    /// <summary>
    /// Elapsed time for <see cref="SeinData.homingIncreaseDelay"/>.
    /// </summary>
    private int _currentLerpDelay;

    /// <summary>
    /// Current speed of the projectile. Increases over time by <see cref="SeinData.projectileSpeedIncreaseRate"/>.
    /// </summary>
    private float _speed;
    private float SpeedSquared => _speed * _speed;
    /// <summary>
    /// Elapsed time for <see cref="SeinData.projectileSpeedIncreaseDelay"/>.
    /// </summary>
    private int _currentAccelerationDelay;

    private int _dustType;

    public override string Texture => "OriMod/Projectiles/Minions/SpiritFlame";

    public override void SetStaticDefaults() {
      ProjectileID.Sets.CanDistortWater[projectile.type] = true;
      ProjectileID.Sets.MinionShot[projectile.type] = true;
    }

    public override void SetDefaults() {
      projectile.alpha = 32;
      projectile.friendly = true;
      projectile.minion = true;
      projectile.ignoreWater = true;
      projectile.tileCollide = false;
      _dustType = ModContent.DustType<SpiritFlameDustTrail>();

      _data = SeinData.All[SpiritFlameType - 1];
      projectile.knockBack = _data.knockback;
      projectile.width = _data.spiritFlameWidth;
      projectile.height = _data.spiritFlameHeight;
      _lerp = _data.homingStrengthStart;
      _speed = _data.projectileSpeedStart;
    }

    /// <summary>
    /// Type for <see cref="SpiritFlame"/>. Determines initialized values by using <see cref="SeinData.All"/>.
    /// </summary>
    protected abstract byte SpiritFlameType { get; }

    private void CreateDust() {
      Dust dust = Dust.NewDustDirect(projectile.position, 10, 10, _dustType);
      dust.scale = _data.dustScale;
      dust.velocity = (_targetPosition - projectile.Center).LengthSquared() >= SpeedSquared ? projectile.velocity * 0.01f : Vector2.Zero;

      dust.rotation = (float)(Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - Math.PI / 180 * 270);
      dust.position = projectile.Center;
      dust.color = Color.Lerp(_data.color.Brightened(), Color.White, 0.85f);
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
      if (projectile.ai[0] == 0) {
        NPC npc = Main.npc[(int)projectile.ai[1]];
        if (npc.active) {
          _targetPosition = npc.Center;
        }
      }
      else {
        _targetPosition.X = projectile.ai[0];
        _targetPosition.Y = projectile.ai[1];
      }
    }

    public override void AI() {
      Lighting.AddLight(projectile.Center, _data.color.ToVector3() * _data.lightStrength);
      CreateDust();

      // Update target position until it dies
      // If target dies before projectile hits, target last live position
      UpdateTargetPosition();

      // Despawn when projectile reaches destination
      Vector2 offset = _targetPosition - projectile.Center;
      float distanceSquared = offset.LengthSquared();

      if (distanceSquared < SpeedSquared) {
        projectile.velocity = offset;
        if (projectile.timeLeft > 2) {
          projectile.timeLeft = 2;
        }
        return;
      }

      // Increase homing strength over time
      TickTimerOrValue(ref _currentLerpDelay, _data.homingIncreaseDelay, ref _lerp, 1, _data.homingIncreaseRate);

      // Increase speed over time
      TickTimerOrValue(ref _currentAccelerationDelay, _data.projectileSpeedIncreaseDelay, ref _speed, 30, _data.projectileSpeedIncreaseRate);

      projectile.velocity = Vector2.Lerp(projectile.velocity.Normalized(), offset.Normalized(), _lerp) * _speed;
    }

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      projectile.active = false;
    }
  }
}
