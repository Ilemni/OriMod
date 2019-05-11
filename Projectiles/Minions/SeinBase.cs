using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OriMod.Projectiles.Minions {
  public abstract class SeinBase : Minion {
    protected int upgrade;
    public override void SetStaticDefaults() {
			Main.projFrames[projectile.type] = 3;
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
			ProjectileID.Sets.Homing[projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
    }
    public override void SetDefaults() {
			projectile.netImportant = true;
			projectile.friendly = true;
			projectile.minion = true;
			projectile.minionSlots = 0;
			projectile.penetrate = -1;
			projectile.timeLeft = 18000;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
    }
    // projectile.ai[0] determines if minion can fire
    //  1f = cannot fire
    //  0f = can fire
    // projectile.ai[1] represents current cooldown
    //  0f = off cooldown
    //  1f+ = on cooldown

    // ID of projectile to shoot
    protected int shoot;
    // Number of shots that can be used before triggering longCooldown
    protected int maxShots = 2;
    protected int currShots = 1;
    // Maximum number of targets that can be fired upon at once
    protected int maxTargets = 3;
    // Shortest cooldown between individual shots
    protected float minCooldown = 12f;
    // Shortest cooldown to count as a seperate burst and not be punished by longCooldown
    protected float shortCooldown = 18f;
    // Cooldown between bursts of shots dictated by numShots
    protected float longCooldown = 60f;
    // Speed of the created projectile
    protected float shootSpeed = 50f;
    // Max distance between minion and target
    protected float minionSpacing = 1f;
    // Max rotation of randomness from target the projectile fires
    protected int randDegrees = 75;
    protected float maxTargetDist = 300f;
    protected float maxTargetThroughWallDist = 0f;
    protected float idleDist = 50f;
    protected float idleAcceleration = 0.5f;
    protected float chaseDist = 200f;
    protected float chaseAcceleration = 6f;
    protected float inertia = 20f;

    protected Color color;
    protected float lightStrength;
    public void Init(int upgradeID) {
      SeinUpgrade u = OriMod.SeinUpgrades[upgradeID - 1];
      maxShots = u.shotsPerBurst;
      maxTargets = u.targets;
      minCooldown = u.minCooldown;
      shortCooldown = u.shortCooldown;
      longCooldown = u.longCooldown;
      randDegrees = u.randDegrees;
      maxTargetDist = u.targetMaxDist;
      maxTargetThroughWallDist = u.targetThroughWallDist;
      projectile.width = u.minionWidth;
      projectile.height = u.minionHeight;
      upgrade = upgradeID;
      shoot = mod.ProjectileType("SpiritFlame" + (upgradeID));
      color = u.color;
      lightStrength = u.lightStrength;
    }
    public virtual void CreateDust() {}

    public virtual void SelectFrame() {}
    public override void CheckActive() {
      Player player = Main.player[projectile.owner];
      OriPlayer modPlayer = player.GetModPlayer<OriPlayer>(mod);
      if (player.dead) {
        modPlayer.seinMinionActive = false;
      }

      if (modPlayer.seinMinionActive && upgrade == modPlayer.seinMinionUpgrade) {
        projectile.timeLeft = 2;
      }
    }
    private static Vector2 Rotate(Vector2 v, float degrees) {
      return new Vector2(
        (float)(v.X * Math.Cos(degrees) - v.Y * Math.Sin(degrees)),
        (float)(v.X * Math.Sin(degrees) + v.Y * Math.Cos(degrees))
      );
    }
    public override void Behavior() {
      Lighting.AddLight(projectile.Center, color.ToVector3() * lightStrength);
      if (!projectile.active) {
        return;
      }
      List<Vector2> targetPositions = new List<Vector2>(0);
      List<Int32> targetIDs = new List<int>(0);
      List<string> targetNames = new List<string>(0);
      Player player = Main.player[projectile.owner];

      // Maintain distance from other minions
      float spacing = (float)projectile.width * minionSpacing;
      for (int k = 0; k < 1000; k++) {
        Projectile otherProj = Main.projectile[k];
        if (
          k != projectile.whoAmI // Not this projectile
          && otherProj.active
          && otherProj.owner == projectile.owner
          && otherProj.type == projectile.type // Same minion
          && System.Math.Abs(projectile.position.X - otherProj.position.X)
           + System.Math.Abs(projectile.position.Y - otherProj.position.Y) < spacing
        ) {
          if (projectile.position.X < Main.projectile[k].position.X) {
            projectile.velocity.X -= idleAcceleration;
          } else {
            projectile.velocity.X += idleAcceleration;
          }
          if (projectile.position.Y < Main.projectile[k].position.Y) {
            projectile.velocity.Y -= idleAcceleration;
          } else {
            projectile.velocity.Y += idleAcceleration;
          }
        }
      }

      Vector2 targetPos = projectile.position;
      bool targeting = false;
      projectile.tileCollide = true;

      // If player specifies target, attack that target
      if(player.HasMinionAttackTargetNPC) {
        NPC npc = Main.npc[player.MinionAttackTargetNPC];
        if(Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height))
        {
          targetPos = npc.Center;
          targeting = true;
          targetPositions.Add(npc.Center);
          targetIDs.Add(npc.whoAmI);
          targetNames.Add(npc.FullName);
        }
      }

      // Otherwise set target based on different enemies, if they can hit
      for (int k = 0; k < 200; k++) {
        if (targetPositions.Count >= maxTargets) break;
        NPC npc = Main.npc[k];
        if (npc.CanBeChasedBy(this, false)) {
          float distance = Vector2.Distance(npc.Center, projectile.Center);
          if (
            distance < maxTargetDist && 
            (
              Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height) || 
              distance < maxTargetThroughWallDist
            )
          )
          {
            targetPos = npc.Center;
            targeting = true;
            targetPositions.Add(npc.Center);
            // Main.NewText($"NPC is: {npc.FullName}");
            targetIDs.Add(npc.whoAmI);
            targetNames.Add(npc.FullName);
          }
        }
      }

      // If too far from player, disable firing
      if (Vector2.Distance(player.Center, projectile.Center) > (targeting ? chaseDist : idleDist)) {
        projectile.ai[0] = 1f;
        projectile.netUpdate = true;
      }

      // If not able to fire, disable collision
      if (projectile.ai[0] == 1f) {
        projectile.tileCollide = false;
      }

      // If targeting and able to fire, move to target
      if (targeting && projectile.ai[0] == 0f) {
        Vector2 direction = targetPos - projectile.Center;
        // If too far from target, chase target
        if (direction.Length() > chaseDist) {
          direction.Normalize();
          projectile.velocity = (projectile.velocity * inertia + direction * chaseAcceleration) / (inertia + 1);
        }
        else {
          projectile.velocity *= (float)Math.Pow(0.97, 40.0 / inertia);
        }
      }
      else {
        float speed = 6f;
        if (projectile.ai[0] == 1f) {
          speed = 15f;
        }
        Vector2 center = projectile.Center;
        Vector2 direction = player.Center - center;
        // projectile.ai[1] = 3600f;
        // projectile.netUpdate = true;
        int num = 1;
        for (int k = 0; k < projectile.whoAmI; k++) {
          if (
            Main.projectile[k].active
            && Main.projectile[k].owner == projectile.owner
            && Main.projectile[k].type == projectile.type
          ) {
            num++;
          }
        }
        direction.X -= (float)((10 + num * 40) * player.direction);
        direction.Y -= 70f;
        float distanceTo = direction.Length();
        if (distanceTo > 200f && speed < 9f) {
          speed = 9f;
        }
        if (
          distanceTo < 100f
          && projectile.ai[0] == 1f
          && !Collision.SolidCollision(projectile.position, projectile.width, projectile.height)
        ) {
          projectile.ai[0] = 0f;
          projectile.netUpdate = true;
        }
        if (distanceTo > 2000f) { // If way too far from player
          projectile.Center = player.Center;
        }
        if (distanceTo > 48f) {
          direction.Normalize();
          direction *= speed;
          float temp = inertia / 2f;
          projectile.velocity = (projectile.velocity * temp + direction) / (temp + 1);
        }
        else {
          projectile.direction = Main.player[projectile.owner].direction;
          projectile.velocity *= (float)Math.Pow(0.9, 40.0 / inertia);
        }
      }
      projectile.rotation = projectile.velocity.X * 0.05f;
      SelectFrame();
      CreateDust();
      // Orient minion based on movement direction (Commented out bc Sein)
      // if (projectile.velocity.X > 0f) {
      //   projectile.spriteDirection = (projectile.direction = -1);
      // }
      // else if (projectile.velocity.X < 0f) {
      //   projectile.spriteDirection = (projectile.direction = 1);
      // }
      // Manage Cooldown
      if (projectile.ai[1] > 0f) { // If on cooldown, increase cooldown
        projectile.ai[1] += 1f;

        // If below max shots, allow firing
        if (currShots < maxShots && projectile.ai[1] > minCooldown) {
          // Reset shot group
          projectile.ai[1] = 0f;
          if (projectile.ai[1] > shortCooldown) {
            currShots = 1;
          }
          else {
            currShots += 1;
          }
        }
        if (currShots >= maxShots) {
          if (targeting && projectile.ai[1] > longCooldown) {
            projectile.ai[1] = 0f;
            currShots = 1;
          }
        }
      }
      if (targeting && projectile.ai[1] > minCooldown) { // Finished min cooldown
          if (currShots >= maxShots) {
            if (projectile.ai[1] < longCooldown) { // Finished long cooldown
              projectile.ai[0] = 1f;
              projectile.netUpdate = true;
            }
            else {
              projectile.ai[0] = 0f;
              projectile.ai[1] = 0f;
              projectile.netUpdate = true;
              currShots = 1;
            }
        }
      } 
      if (projectile.ai[1] == 0f && targeting) { // Can fire{
        // Orient minion based on target direction (Commnted out bc Sein)
        // if ((targetPos - projectile.Center).X > 0f) {
        //   projectile.spriteDirection = (projectile.direction = -1);
        // } else if ((targetPos - projectile.Center).X < 0f) {
        //   projectile.spriteDirection = (projectile.direction = 1);
        // }

        projectile.ai[1] = 1f;
        if (Main.myPlayer == projectile.owner) { // Fire
          for (int t = 0; t < targetPositions.Count; t++) {
            Vector2 shootVel = targetPositions[t] - projectile.Center;
            if (shootVel == Vector2.Zero) {
              shootVel = new Vector2(0f, 1f);
            }
            shootVel.Normalize();
            shootVel = Rotate(shootVel, ((float)Main.rand.Next(-randDegrees, randDegrees) / 360f) * 2f * (float)Math.PI);
            shootVel *= shootSpeed;
            int proj = Projectile.NewProjectile(projectile.Center, shootVel, shoot, projectile.damage, projectile.knockBack, Main.myPlayer, targetIDs[t], 0f);
            projectile.velocity += (shootVel * -0.005f);
            Main.projectile[proj].timeLeft = 300;
            Main.projectile[proj].netUpdate = true;
            projectile.netUpdate = true;
          }
        }
      }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
      fallThrough = true;
      return true;
    }
  }
}