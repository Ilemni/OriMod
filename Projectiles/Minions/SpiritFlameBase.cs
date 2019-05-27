using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
	public class SpiritFlameBase : ModProjectile {
		protected NPC target = null;
		// Initial lerp value: 0 = no homing; 1 = full homing
		protected float lerp;
		// Value added to lerp every frame
		protected float lerpSmoothRate;
		protected float lerpDelay;
		private float currLerpDelay;
		protected float speed;
		protected float acceleration;
		protected float accelDelay;
		private float currAccelDelay;

		private Vector2 lastTargetPos;
		protected int dustType; 
		protected int dustWidth;
		protected int dustHeight;
		protected float dustScale;
		protected Color color;
		protected float lightStrength;

    public void Init(int upgradeID) {
			SeinUpgrade u = OriMod.SeinUpgrades[upgradeID];
			
			projectile.knockBack = u.knockback;
			lerp = u.homingStrengthStart;
			lerpSmoothRate = u.homingIncreaseRate;
			lerpDelay = u.homingIncreaseDelay;
			speed = u.projectileSpeedStart;
			acceleration = u.projectileSpeedIncreaseRate;
			accelDelay = u.projectileSpeedIncreaseDelay;
			projectile.maxPenetrate = u.pierce;
			projectile.tileCollide = u.tileCollide;
			projectile.width = u.flameWidth;
			projectile.height = u.flameHeight;
			dustScale = u.dustScale;
			dustType = mod.DustType("SFDustTrail");
			color = u.color;
			lightStrength = u.lightStrength * 0.6f;
		}
		
		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionShot[projectile.type] = true;
		}

		public override void SetDefaults() {
			projectile.alpha = 32;
			projectile.friendly = true;
			projectile.ignoreWater = true;
			dustWidth = 10;
			dustHeight = 10;
		}
		public void CreateDust() {
			int dust = Dust.NewDust(projectile.position, dustWidth, dustHeight, dustType);
			Main.dust[dust].scale = dustScale;
			if (projectile.velocity == Vector2.Zero) {
				Main.dust[dust].velocity.Y -= 1f;
			} else {
				Main.dust[dust].velocity = projectile.velocity * 0.05f;
			}
			
			Main.dust[dust].rotation = (float)(Math.Atan2(projectile.velocity.Y, projectile.velocity.X) - (Math.PI/180 * 270));
			Main.dust[dust].position = projectile.Center;
			Main.dust[dust].noGravity = true;
		}

		public override void AI() {
			Lighting.AddLight(projectile.Center, color.ToVector3() * lightStrength);
			float distance = Vector2.Distance(lastTargetPos, projectile.position);
			if (target == null) {
				target = Main.npc[(int)projectile.ai[0]];
			}
			// If target dies before projectile hits, kill projectile when it reaches target
			else if (target.active == false) {
				if (distance < speed) {
					projectile.active = false;
				}
			}
			// Increase homing strength over time
			if (currLerpDelay < lerpDelay) {
				currLerpDelay++;
			} else {
				if (lerp > 1) {
					lerp = 1;
				} else if (lerp < 1) {
					lerp += lerpSmoothRate;
				}
			}
			// Increase speed over time
			if (currAccelDelay <= accelDelay) {
				currAccelDelay++;
			}
			if (currAccelDelay > accelDelay) {
				speed += acceleration;
			}
			if (distance < speed) {
				lerp = lerpSmoothRate * 5f;
				currLerpDelay = lerpDelay * 0.25f;
			}

			//Get the shoot trajectory from the projectile and target
			float shootToX;
			float shootToY;
			// If target dies before projectile hits, target last live position
			if (target.active) {
				lastTargetPos = target.position;
				shootToX = target.position.X + (float)target.width * 0.5f - projectile.Center.X;
				shootToY = target.position.Y - projectile.Center.Y;
			} else {
				shootToX = lastTargetPos.X - projectile.Center.X;
				shootToY = lastTargetPos.Y - projectile.Center.Y;
			}
			if (speed > 30) {
				speed = 30;
			}
			Vector2 currVelocity = projectile.velocity;
			Vector2 currDist = new Vector2(shootToX, shootToY);
			currVelocity.Normalize();
			currDist.Normalize();
			Vector2 newVelocity = Vector2.Lerp(currVelocity, currDist, lerp);
			newVelocity *= speed;
			projectile.velocity = newVelocity;
			CreateDust();
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			
		}

		// public override bool OnTileCollide(Vector2 oldVelocity)
		// {
		// 	projectile.penetrate = -1;
		// 	projectile.maxPenetrate = -1;
		// 	projectile.tileCollide = false;
		// 	projectile.position += projectile.velocity;
		// 	projectile.velocity = Vector2.Zero;
		// 	projectile.timeLeft = 180;
		// 	return false;
		// }
	}
}