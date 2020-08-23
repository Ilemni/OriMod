using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  /// <summary>
  /// Projectile fired by the minion <see cref="Sein"/>.
  /// </summary>
  public abstract class SpiritFlame : ModProjectile {
    // Our ai fields are a bit weird...
    // If we are **not** targeting something, projectile.ai values are filled with a vector2 for where to land
    // Otherwise, ai[0] is the npc id that we are targeting, and ai[1] is 0
    // Whichever this state is, is based on if ai[1] is 0
    // In gameplay, non-target position in ai[1] should *never* be 0
    private NPC Target => _target ?? (IsTargetingNPC ? (_target = Main.npc[(int)projectile.ai[0]]) : null);
    private NPC _target = null;

    private bool IsTargetingNPC => projectile.ai[1] == 0;

    private Vector2 NonTargetPos => _nonTargetPos != Vector2.Zero ? _nonTargetPos : (_nonTargetPos = new Vector2(projectile.ai[0], projectile.ai[1]));
    private Vector2 _nonTargetPos = Vector2.Zero;

    private Vector2 LastTargetPos {
      get => IsTargetingNPC ? _lastTargetPos : NonTargetPos;
      set => _lastTargetPos = value;
    }
    private Vector2 _lastTargetPos;

    /// <summary>
    /// Homing strength of the projectile.
    /// <para>0 = no homing; 1 = full homing</para>
    /// </summary>
    private float lerp;
    /// <summary>
    /// Rate that <see cref="lerp"/> will increase at.
    /// </summary>
    private float lerpRate;
    /// <summary>
    /// Total time before the projectile homing can increase.
    /// </summary>
    private int lerpDelay;
    /// <summary>
    /// Elapsed time for <see cref="lerpDelay"/>
    /// </summary>
    private int currentLerpDelay;

    /// <summary>
    /// Speed of the projectile.
    /// </summary>
    private float speed;
    /// <summary>
    /// Rate that <see cref="speed"/> will increase.
    /// </summary>
    private float acceleration;
    /// <summary>
    /// Total time before the projectile speed can increase.
    /// </summary>
    private int accelerationDelay;
    /// <summary>
    /// Elapsed time for <see cref="accelerationDelay"/>.
    /// </summary>
    private int currentAccelerationDelay;

    private int dustType;
    private int dustWidth;
    private int dustHeight;
    private float dustScale;
    private Color color;
    private float lightStrength;

    public override string Texture => "OriMod/Projectiles/Minions/SpiritFlame";

    // There is probably a better way of doing this...
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
      projectile.timeLeft = 45;
      dustWidth = 10;
      dustHeight = 10;
      Initialize();
    }

    /// <summary>
    /// Type for <see cref="SpiritFlame"/>. Determines initialized values by using <see cref="OriMod.SeinUpgrades"/>.
    /// </summary>
    protected abstract byte SpiritFlameType { get; }

    private void Initialize() {
      var upgradeID = SpiritFlameType;
      SeinUpgrade u = OriMod.Instance.SeinUpgrades[upgradeID - 1];

      projectile.knockBack = u.knockback;
      lerp = u.homingStrengthStart;
      lerpRate = u.homingIncreaseRate;
      lerpDelay = u.homingIncreaseDelay;
      speed = u.projectileSpeedStart;
      acceleration = u.projectileSpeedIncreaseRate;
      accelerationDelay = u.projectileSpeedIncreaseDelay;
      projectile.width = u.flameWidth;
      projectile.height = u.flameHeight;
      dustScale = u.dustScale;
      dustType = mod.DustType("SFDustTrail");
      color = u.color;
      lightStrength = u.lightStrength * 0.6f;
    }

    private void CreateDust() {
      Dust dust = Main.dust[Dust.NewDust(projectile.position, dustWidth, dustHeight, dustType)];
      dust.scale = dustScale;
      if (projectile.velocity == Vector2.Zero) {
        dust.velocity.Y -= 1f;
      }
      else {
        dust.velocity = projectile.velocity * 0.05f;
      }

      dust.rotation = (float)(Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - Math.PI / 180 * 270);
      dust.position = projectile.Center;
      dust.noGravity = true;
    }

    /// <summary>
    /// Increment <paramref name="currentTime"/> if less than <paramref name="maxTime"/>, else increase <paramref name="currentValue"/> by <paramref name="valueRate"/> until <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="currentTime">Current time.</param>
    /// <param name="maxTime">Time needed before increasing <paramref name="currentValue"/>.</param>
    /// <param name="currentValue">Value to increase if enough time has passed.</param>
    /// <param name="maxValue">Maximum value <paramref name="currentValue"/> can be.</param>
    /// <param name="valueRate">Rate that <paramref name="currentValue"/> will increase by.</param>
    private void TickTimerOrValue(ref int currentTime, int maxTime, ref float currentValue, float maxValue, float valueRate) {
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

    public override void AI() {
      Lighting.AddLight(projectile.Center, color.ToVector3() * lightStrength);
      CreateDust();

      // Update target position until it dies
      // If target dies before projectile hits, target last live position
      if (IsTargetingNPC && Target.active) {
        LastTargetPos = Target.Center;
      }

      // Despawn when projectile reaches destination
      float distance = Vector2.DistanceSquared(LastTargetPos, projectile.position);
      if (distance < speed * speed) {
        if (projectile.timeLeft > 3) {
          projectile.timeLeft = 3;
        }
        return;
      }

      // Increase homing strength over time
      TickTimerOrValue(ref currentLerpDelay, lerpDelay, ref lerp, 1, lerpRate);

      // Increase speed over time
      TickTimerOrValue(ref currentAccelerationDelay, accelerationDelay, ref speed, 30, acceleration);

      projectile.velocity = Vector2.Lerp(projectile.velocity.Normalized(), (LastTargetPos - projectile.Center).Normalized(), lerp) * speed;
    }

    public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
      projectile.active = false;
    }
  }
}
