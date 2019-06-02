using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
      projectile.velocity = new Vector2(0, -maxVelocityInBounds);
      projectile.position = PlayerSpace();
      targetSpawn = PlayerSpace(0, -32);
      minionTargetLocation = PlayerSpace(0, -32);
    }
    // projectile.ai[0] determines if minion can fire
    //  0f = cannot fire
    //  1f = can fire
    // projectile.ai[1] represents current cooldown
    //  0f = off cooldown
    //  1f+ = on cooldown

    // ID of projectile to shoot
    protected int shoot;
    // Number of shots that can be used before triggering longCooldown
    protected int maxShotsPerBurst = 2;
    // Max number of shots that can be fired at once
    protected int maxShotsPerVolley = 1;
    protected int shotsPerTarget = 1;
    protected int shotsToPrimaryTarget = 1;
    private int currShots = 1;
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
    protected int pierce = 1;
    protected float primaryDamageMultiplier = 1;
    // Max rotation of randomness from target the projectile fires
    protected int randDegrees = 75;
    protected float maxTargetDist = 300f;
    protected float maxTargetThroughWallDist = 0f;

    protected Color color;
    protected float lightStrength;
    internal void Init(int upgradeID) {
      SeinUpgrade u = OriMod.SeinUpgrades[upgradeID - 1];
      maxShotsPerBurst = u.shotsPerBurst;
      maxShotsPerVolley = u.maxShotsPerVolley;
      shotsPerTarget = u.shotsPerTarget;
      shotsToPrimaryTarget = u.shotsToPrimaryTarget;
      primaryDamageMultiplier = u.primaryDamageMultiplier;
      pierce = u.pierce;
      maxTargets = u.targets;
      minCooldown = u.minCooldown;
      shortCooldown = u.shortCooldown;
      longCooldown = u.longCooldown;
      randDegrees = u.randDegrees;
      maxTargetDist = u.targetMaxDist;
      if (maxDistFromPlayer < maxTargetDist * 0.8f) { 
        maxDistFromPlayer = maxTargetDist * 0.8f;
      }
      maxTargetThroughWallDist = u.targetThroughWallDist;
      projectile.width = u.minionWidth;
      projectile.height = u.minionHeight;

      upgrade = upgradeID;
      shoot = mod.ProjectileType("SpiritFlame" + (upgradeID));
      color = u.color;
      lightStrength = u.lightStrength;
    }
    protected virtual void CreateDust() { }

    protected virtual void SelectFrame() {}
    
    private float Lerp(float firstFloat, float secondFloat, float by) {
     return firstFloat * (1 - by) + secondFloat * by;
    }
    private Vector2 minionTargetLocation;
    private Vector2 targetSpawn;
    private NPC mainTargetNPC;
    private int targetSide = 1;
    private Vector2 PlayerSpace(float x, float y) {
      return PlayerSpace(new Vector2(x, y));
    }
    private Vector2 PlayerSpace(Vector2 coords=new Vector2()) {
      return Main.player[projectile.owner].position + coords;
    }
    
    // You will definitely need to tweak these.
    protected float minVelocity = 0.1f;        // This is the slowest speed Sein can be at.
    protected float maxVelocityInBounds = 1.32f;        // This is the fastest speed Sein can be at.
    protected float maxVelocityOutOfBounds = 30f;
    protected float nearThreshold = 13f;    // This is the distance from which Sein will begin to slow down.
    protected float damping = 0.9f;    // This is how much Sein's speed is reduced every time Behavior() is called should she be closer than SeinNearThreshold.
    protected float maxDampingOutOfBounds = 0.75f;
    protected float accelerationInBounds = 1.06f;
    protected float accelerationOutofBounds = 1.1f;
    protected float triggerTargetMove = 0.5f;
    protected float maxDistFromPlayer = 240f;
    protected float minDistFromNPC = 64f;
    protected SoundEffectInstance PlaySpiritFlameSound(string Path, float Volume) {
      return Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, ("Sounds/Custom/NewSFX/Ori/SpiritFlame/" + Path)).WithVolume(Volume), projectile.Center);
    }
    private int excludeRand = 0;
    private static readonly Vector2[] TargetPositions = new Vector2[] {
      new Vector2(-24, 7),
      new Vector2(24, -7),
      new Vector2(-24, -2),
      new Vector2(24, 7),
      new Vector2(-24, -7),
      new Vector2(24, -2),
    };
    private static readonly Vector2 bounds = new Vector2(68f, 32f);
    private int targetPosIndex = 0;
    private List<int> targetIDs = new List<int>(0);
    private static Vector2 Normalize(Vector2 vec2) {
        return vec2 / vec2.Length();
    }
    
    private void SetNewMinionTarget(int idx=-1) {
      targetPosIndex = idx != -1 ? idx : targetPosIndex + 1;
      if (targetPosIndex >= TargetPositions.Length) {
        targetPosIndex = 0;
      }
      minionTargetLocation = targetSpawn + TargetPositions[targetPosIndex];
      targetSide = -targetSide;
    }
    private bool IsInBounds() {
      Vector2 p = projectile.position;
      return (
        p.X < targetSpawn.X + bounds.X && 
        p.X > targetSpawn.X - bounds.X && 
        p.Y < targetSpawn.Y + bounds.Y && 
        p.Y > targetSpawn.Y - bounds.Y
      );
    }
    private int SortByDistClosest(int id1, int id2) {
      Vector2 playerPos =  Main.player[projectile.owner].position;
      float length1 = (Main.npc[id1].position - playerPos).Length();
      float length2 = (Main.npc[id2].position - playerPos).Length();
      return length1.CompareTo(length2);
    }
    internal override void CheckActive() {
      Player player = Main.player[projectile.owner];
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>(mod);
      if (player.dead) {
        oPlayer.seinMinionActive = false;
      }

      if (!oPlayer.seinMinionActive || upgrade != oPlayer.seinMinionUpgrade) {
        projectile.active = false;
      }
      else {
        projectile.timeLeft = 2;
      }
    }
    // This is the somewhat subtle swaying about Sein does at any given time in Blind Forest
    private void SeinMovement() {
      Player owner = Main.player[projectile.owner];
      if (projectile.position.HasNaNs()) {
        projectile.position = owner.position;
      }
      if (projectile.velocity.HasNaNs()) {
        projectile.velocity = new Vector2(0, -maxVelocityInBounds);
      }
      Vector2 oldVel = projectile.velocity;
      if (oldVel.Length() == 0) { // Usually spawn, magnitide should never be 0 otherwise, and being 0 would break spawning
        oldVel = new Vector2(0, -maxVelocityInBounds);
      }
      Vector2 oldPos = projectile.position;
      Vector2 oldDir = Normalize(oldVel);
      if ((minionTargetLocation - PlayerSpace()).Length() > 1000 || (targetSpawn - PlayerSpace()).Length() > 1000) {
        minionTargetLocation = PlayerSpace(0, -32);
      }
      Vector2 newDir = Normalize(minionTargetLocation - oldPos);
      
      float dist = (minionTargetLocation - oldPos).Length();
      if (dist > 1050) { // Want to be 800
        projectile.position = PlayerSpace(-newDir * 1000f); // also 800
        projectile.velocity = newDir * maxVelocityOutOfBounds;
        return;
      }
      
      Vector2 newVel = oldVel.Length() * newDir;
      if (IsInBounds()) {
        if (dist < nearThreshold) {
          newVel *= damping;
        }
        else {
          newVel *= accelerationInBounds;
          if (newVel.Length() > maxVelocityInBounds) {
            newVel = newDir * Lerp(newVel.Length(), maxVelocityInBounds, 0.22f);
          }
        }
      }
      else {
        SetNewMinionTarget(newVel.X > 0 ? 3 : 1);
        newVel *= accelerationOutofBounds;
        if (newVel.Length() > maxVelocityOutOfBounds) { // Too fast... maybe
          if (newVel.Length() > owner.velocity.Length()) {
            newVel = newDir * Lerp(newVel.Length(), owner.velocity.Length(), 0.7f);
          }
          else {
            newVel = newDir * Lerp(newVel.Length(), maxVelocityOutOfBounds, 0.2f);
          }
        }
        if (newVel.Length() < (oldVel.Length() * maxDampingOutOfBounds)) { // Damned more than necessary
          newVel = newDir * Lerp(oldVel.Length(), newVel.Length(), maxDampingOutOfBounds);
        }
      }
      if (newVel.Length() < minVelocity * 2f) { // Too slow
        newVel = newDir * minVelocity * 2.1f;
        SetNewMinionTarget();
      }

      if (dist < nearThreshold || dist > 85) {
        projectile.velocity = Normalize(oldVel * 0.25f + newVel * 0.75f) * newVel.Length();
      }
      else {
        projectile.velocity = Normalize(oldVel * 0.8f + newVel * 0.2f) * newVel.Length();
      }
    }
    private void UpdateTargetPosIdle() {
      targetSpawn = PlayerSpace(0, -24f);
      minionTargetLocation = targetSpawn + TargetPositions[targetPosIndex];
    }
    private void UpdateTargetPosToNPC() {
      int mainTarget = targetIDs[0];
      Vector2 playerPos = Main.player[projectile.owner].position;
      Vector2 npcPos = Main.npc[mainTarget].position;
      Vector2 offset = PlayerSpace(0, -24f) - npcPos; 
      offset.Y -= 12f;
      Vector2 dir = Normalize(offset);
      float dist = offset.Length();

      if (dist > maxTargetDist) { // Cannot reach targeted NPC
        if (targetIDs.Count == 1 || Main.player[projectile.owner].HasMinionAttackTargetNPC) {
          UpdateTargetPosIdle();
          return;
        }
        else { // Try reaching closest NPC
          mainTarget = targetIDs[1];
          npcPos = Main.npc[mainTarget].position;
          offset = PlayerSpace(0, -24f) - npcPos;
          offset.Y -= 12f;
          dist = offset.Length();
          dir = Normalize(offset);
          if (dist > maxTargetDist) {
            UpdateTargetPosIdle();
            return;
          }
        }
      }
      if (dist < minDistFromNPC) {
        targetSpawn = PlayerSpace(0, -24f);
      }
      else if (dist > maxDistFromPlayer) {
        targetSpawn = PlayerSpace() - dir * maxDistFromPlayer;
      }
      else {
        targetSpawn = npcPos + dir * minDistFromNPC;
      }
      minionTargetLocation = targetSpawn + TargetPositions[targetPosIndex];
    }
    private void UpdateTargetsPos() {
      Vector2 projToTarget = (projectile.position - minionTargetLocation);
      if (projToTarget.Length() < triggerTargetMove && projectile.velocity.Length() > maxVelocityInBounds) {
        SetNewMinionTarget();
      }
      if (targetIDs.Count == 0 || Main.npc[targetIDs[0]].active == false) { // Idle movement above Ori
        UpdateTargetPosIdle();
      }
      else {
        UpdateTargetPosToNPC();
      }
    }
    private void Fire(int t) {
      Vector2 shootVel = Main.npc[targetIDs[t]].position - projectile.Center;
      if (shootVel == Vector2.Zero) {
        shootVel = new Vector2(0f, 1f);
      }
      shootVel.Normalize();
      shootVel = Utils.RotatedBy(shootVel, (float)Main.rand.Next(-randDegrees, randDegrees) / 180f * (float)Math.PI);
      shootVel *= shootSpeed;
      int proj = Projectile.NewProjectile(projectile.Center, shootVel, shoot, (int)(projectile.damage * (t == 0 ? primaryDamageMultiplier : 1)), projectile.knockBack, Main.myPlayer, targetIDs[t], 0f);
      projectile.velocity += (shootVel * -0.005f);
      Main.projectile[proj].timeLeft = 300;
      Main.projectile[proj].netUpdate = true;
      Main.projectile[proj].penetrate = pierce;
      projectile.netUpdate = true;
    }
    internal override void Behavior() {
      if (!projectile.active) { return; }
      SeinMovement();
      UpdateTargetsPos();
      Lighting.AddLight(projectile.Center, color.ToVector3() * lightStrength);
      
      Player player = Main.player[projectile.owner];
      

      List<Vector2> targetPositions = new List<Vector2>(0);
      List<Int32> newTargetIDs = new List<int>(0);

      Vector2 targetPos = projectile.position;
      bool targeting = false;
      projectile.tileCollide = true;
      
      // If player specifies target, add that target to selection
      if(player.HasMinionAttackTargetNPC) {
        NPC npc = Main.npc[player.MinionAttackTargetNPC];
        if (npc.CanBeChasedBy(this, false)) {
          float distance = Vector2.Distance(projectile.Center, npc.Center);
          if (
            distance < maxTargetDist && 
            (
              Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height) || 
              distance < maxTargetThroughWallDist
            )
          ) {
            targeting = true;
            mainTargetNPC = npc;
          }
        }
      }

      // Otherwise set target based on different enemies, if they can hit
      for (int k = 0; k < Main.maxNPCs; k++) {
        NPC npc = Main.npc[k];
        if (npc.CanBeChasedBy(this, false)) {
          float distance = Vector2.Distance(projectile.Center, npc.Center);
          if (
            distance < maxTargetDist && 
            (
              Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height) || 
              distance < maxTargetThroughWallDist
            )
          ) {
            targeting = true;
            newTargetIDs.Add(npc.whoAmI);
          }
        }
      }
      bool doReplace = false;
      int numExcepts = targetIDs.Except(newTargetIDs).Count();
      if (newTargetIDs.Count == targetIDs.Count && numExcepts == 0) { // See if list needs to be replaced
        float dist = 0;
        for (int t = 0; t < targetIDs.Count; t++) {
          float npcDist = (projectile.position - Main.npc[targetIDs[t]].position).Length();
          if (npcDist < dist) {
            doReplace = true; // List of NPCs is no longer in order of distance
            break;
          }
          else {
            dist = npcDist;
          }
        }
      }
      else {
        doReplace = true; // Number of NPCs or the specific NPCs targeted was changed
      }

      if (doReplace) { // Replace list
        if (newTargetIDs.Count > 1) {
          newTargetIDs.Sort(SortByDistClosest);
        }
        targetIDs.Clear();
        if (mainTargetNPC != null && mainTargetNPC.active) {
          targetIDs.Add(mainTargetNPC.whoAmI);
          targetIDs.AddRange(newTargetIDs.GetRange(0, newTargetIDs.Count > maxTargets - 1 ? maxTargets - 1 : newTargetIDs.Count));
        }
        targetIDs.AddRange(newTargetIDs.GetRange(0, newTargetIDs.Count > maxTargets ? maxTargets : newTargetIDs.Count));
      }
      
      // If not in idle box, no collision
      if (!IsInBounds()) {
        projectile.tileCollide = false;
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
        if (currShots < maxShotsPerBurst && projectile.ai[1] > minCooldown) {
          // Reset shot group
          projectile.ai[1] = 0f;
          if (projectile.ai[1] > shortCooldown) {
            currShots = 1;
          }
          else {
            currShots++;
          }
        }
        if (currShots >= maxShotsPerBurst) {
          if (targeting && projectile.ai[1] > longCooldown) {
            projectile.ai[1] = 0f;
            currShots = 1;
          }
        }
      }
      if (targeting && projectile.ai[1] > minCooldown) { // Finished min cooldown
        if (currShots >= maxShotsPerBurst) {
          if (projectile.ai[1] < longCooldown) { // Finished long cooldown
            projectile.ai[0] = 0f;
            projectile.netUpdate = true;
          }
          else {
            projectile.ai[0] = 1f;
            projectile.ai[1] = 0f;
            projectile.netUpdate = true;
            currShots = 1;
          }
        }
      } 
      if (projectile.ai[1] == 0f && targeting) { // Can fire
        // Orient minion based on target direction (Commnted out bc Sein)
        // if ((targetPos - projectile.Center).X > 0f) {
        //   projectile.spriteDirection = (projectile.direction = -1);
        // } else if ((targetPos - projectile.Center).X < 0f) {
        //   projectile.spriteDirection = (projectile.direction = 1);
        // }

        projectile.ai[1] = 1f;
        if (Main.myPlayer == projectile.owner) { // Fire
          int usedShots = 0;
          int loopCount = 0;
          while (usedShots < maxShotsPerVolley && loopCount < shotsToPrimaryTarget) {
            for (int t = 0; t < targetIDs.Count; t++) {
              if (loopCount < (t == 0 ? shotsToPrimaryTarget : shotsPerTarget)) {
                Fire(t);
                usedShots++;
              }
            }
            loopCount ++;
          }
          string c =
            upgrade == 1 || upgrade == 2 ? "" :
            upgrade == 3 || upgrade == 4 ? "LevelB" :
            upgrade == 5 || upgrade == 6 ? "LevelC" : "LevelD";
          PlaySpiritFlameSound("Throw" + c + OriPlayer.RandomChar(3, ref excludeRand), 0.6f);
        }
      }
    }
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
      fallThrough = true;
      return true;
    }
  }
}