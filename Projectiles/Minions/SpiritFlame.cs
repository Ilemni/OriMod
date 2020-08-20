using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  public abstract class SpiritFlameBase : ModProjectile {
    // Our ai fields are a bit unique...
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

    // Projectile lerp with delay before increase
    // 0 = no homing; 1 = full homing
    private float lerp;
    private float lerpRate;
    private int lerpDelay;
    private int currLerpDelay;

    // Projectile speed with delay before increase
    private float speed;
    private float acceleration;
    private int accelDelay;
    private int currAccelDelay;

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
      projectile.timeLeft = 150;
      dustWidth = 10;
      dustHeight = 10;
      Initialize(Init);
    }

    protected abstract int Init { get; }

    private void Initialize(int upgradeID) {
      SeinUpgrade u = OriMod.Instance.SeinUpgrades[upgradeID - 1];

      projectile.knockBack = u.knockback;
      lerp = u.homingStrengthStart;
      lerpRate = u.homingIncreaseRate;
      lerpDelay = u.homingIncreaseDelay;
      speed = u.projectileSpeedStart;
      acceleration = u.projectileSpeedIncreaseRate;
      accelDelay = u.projectileSpeedIncreaseDelay;
      projectile.penetrate = projectile.maxPenetrate = u.pierce;
      projectile.width = u.flameWidth;
      projectile.height = u.flameHeight;
      dustScale = u.dustScale;
      dustType = mod.DustType("SFDustTrail");
      color = u.color;
      lightStrength = u.lightStrength * 0.6f;
    }

    private void CreateDust() {
      int dust = Dust.NewDust(projectile.position, dustWidth, dustHeight, dustType);
      Main.dust[dust].scale = dustScale;
      if (projectile.velocity == Vector2.Zero) {
        Main.dust[dust].velocity.Y -= 1f;
      }
      else {
        Main.dust[dust].velocity = projectile.velocity * 0.05f;
      }

      Main.dust[dust].rotation = (float)(Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - Math.PI / 180 * 270);
      Main.dust[dust].position = projectile.Center;
      Main.dust[dust].noGravity = true;
    }

    // Increase time, or if time is => delay, increase currVal by rate
    private void TickTimerOrValue(ref int time, int delay, ref float currVal, float rate, float maxVal) {
      if (time < delay) {
        time++;
      }
      else if (currVal < maxVal) {
        currVal += rate;
        if (currVal > maxVal) {
          currVal = maxVal;
        }
      }
    }

    public override void AI() {
      Lighting.AddLight(projectile.Center, color.ToVector3() * lightStrength);
      CreateDust();

      // If target dies before projectile hits, target last live position
      if (IsTargetingNPC && Target.active) {
        LastTargetPos = Target.Center;
      }

      // If close enough to last live position, despawn projectile if target dead, or set lerp to max
      float distance = Vector2.Distance(LastTargetPos, projectile.position);
      if (distance < speed) {
        if (IsTargetingNPC && !Target.active) {
          projectile.active = false;
          return;
        }
        lerp = 1;
        currLerpDelay = lerpDelay;
      }

      // Increase homing strength over time
      TickTimerOrValue(ref currLerpDelay, lerpDelay, ref lerp, lerpRate, 1);

      // Increase speed over time
      TickTimerOrValue(ref currAccelDelay, accelDelay, ref speed, acceleration, 30);

      projectile.velocity = Vector2.Lerp(projectile.velocity.Normalized(), (LastTargetPos - projectile.Center).Normalized(), lerp) * speed;
    }
  }
}
