using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Graphics;

namespace OriMod.Projectiles.Minions {
  public abstract class Sein : Minion {
    public override sealed string Texture => "OriMod/Projectiles/Minions/Sein";
    
    public override sealed bool? CanCutTiles() => false;

    public override sealed void SetStaticDefaults() {
      Main.projFrames[projectile.type] = 3;
      Main.projPet[projectile.type] = true;
      ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
      ProjectileID.Sets.Homing[projectile.type] = true;
      ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
    }
    
    protected abstract int Init { get; }

    public override void SetDefaults() {
      projectile.netImportant = true;
      projectile.friendly = true;
      projectile.minion = true;
      projectile.minionSlots = -0.001f;
      projectile.penetrate = -1;
      projectile.timeLeft = 18000;
      projectile.tileCollide = false;
      projectile.ignoreWater = true;
      projectile.velocity = new Vector2(0, -MaxVelocityInBounds);
      projectile.position = PlayerSpace();
      BoxPos = TargetPos =  PlayerSpace(0, -32);
      Initialize(Init);
    }

    private Player player => Main.player[projectile.owner];

    /// <summary> Assigns most fields based on `upgradeID` </summary>
    /// <param name="upgradeID">Index of `OriMod.SeinUpgrades` to use. </param>
    private void Initialize(int upgradeID) {
      SeinUpgrade u = OriMod.SeinUpgrades[upgradeID - 1];
      MaxShotsPerBurst = u.shotsPerBurst;
      MaxShotsPerVolley = u.maxShotsPerVolley;
      ShotsToTarget = u.shotsPerTarget;
      ShotsToPrimaryTarget = u.shotsToPrimaryTarget;
      PrimaryDamageMultiplier = u.primaryDamageMultiplier;
      Pierce = u.pierce;
      MaxTargets = u.targets;
      MinCooldown = u.minCooldown;
      ShortCooldown = u.shortCooldown;
      LongCooldown = u.longCooldown;
      RandDegrees = u.randDegrees;
      MaxTargetDist = u.targetMaxDist;
      if (MaxDistFromPlayer < MaxTargetDist * 0.8f) { 
        MaxDistFromPlayer = MaxTargetDist * 0.8f;
      }
      MaxTargetThroughWallDist = u.targetThroughWallDist;
      projectile.width = u.minionWidth;
      projectile.height = u.minionHeight;

      Upgrade = upgradeID;
      ShootID = mod.ProjectileType("SpiritFlame" + (upgradeID));
      Color = u.color;
      LightStrength = u.lightStrength;

      SpiritFlameSound =
        Upgrade == 1 || Upgrade == 2 ? "" :
        Upgrade == 3 || Upgrade == 4 ? "LevelB" :
        Upgrade == 5 || Upgrade == 6 ? "LevelC" :
        Upgrade == 7 || Upgrade == 8 ? "LevelD" : "";
    }

    /// <summary> False if the held item is the item that summoned this projectile. </summary>
    private bool Autoshoot => !projectile.HeldItemShotThis();

    /// <summary> ID of current SeinUpgrade </summary>
    private int Upgrade;
    
    /// <summary> Current Cooldown of Spirit Flame </summary>
    private int Cooldown {
      get => (int)projectile.ai[0];
      set {
        if (value != projectile.ai[0]) {
          projectile.netUpdate = true;
          projectile.ai[0] = value;
        }
      }
    }

    /// <summary> ID of projectile to shoot.
    /// 
    /// Assigned in `Init()` </summary>
    private int ShootID;

    /// <summary> Number of shots that can be used before triggering longCooldown.
    /// 
    /// Assigned in `Init()` </summary>
    private int MaxShotsPerBurst;
    
    /// <summary> Max number of shots that can be fired at once.
    /// 
    /// Assigned in `Init()` </summary>
    private int MaxShotsPerVolley;

    /// <summary> Max number of shots that can be fired at a single target.
    /// 
    /// Assigned in `Init()` </summary>
    private int ShotsToTarget;

    /// <summary> Max number of shots that can be fired at the first target.
    /// 
    /// Assigned in `Init()` </summary>
    private int ShotsToPrimaryTarget;
    
    /// <summary> Maximum number of targets that can be fired upon at once.
    /// 
    /// Assigned in `Init()` </summary>
    private int MaxTargets;
    
    /// <summary> Shortest cooldown between individual shots.
    /// 
    /// Assigned in `Init()` </summary>
    private float MinCooldown;
    
    /// <summary> Shortest cooldown to count as a seperate burst and not incur longCooldown.
    /// 
    /// Assigned in `Init()` </summary>
    private float ShortCooldown;
    
    /// <summary> Cooldown between bursts of shots dictated by numShots.
    /// 
    /// Assigned in `Init()` </summary>
    private float LongCooldown;
    
    /// <summary> Speed of the created projectile </summary>
    private static float ShootSpeed => 50f;

    /// <summary> Spirit Flame penetration
    /// 
    /// Assigned in `Init()` </summary>
    private int Pierce = 1;

    /// <summary> Spirit Flame damage to the first target in `Fire()`.
    /// 
    /// Assigned in `Init()` </summary>
    private float PrimaryDamageMultiplier;
    
    /// <summary> How much more manually firing Spirit Flame damages for. </summary>
    private static float ManualShootDamageMultiplier => 1.4f;
    
    /// <summary> Degrees from direction to target NPC that a Spirit Flame can be shot at.
    /// 
    /// Assigned in `Init()` </summary>
    private int RandDegrees;

    /// <summary> Range from player that NPCs can be targeted.
    /// 
    /// Assigned in `Init()` </summary>
    private float MaxTargetDist;

    /// <summary> Range from player that NPCs can be targeted, ignoring walls.
    /// 
    /// Assigned in `Init()` </summary>
    private float MaxTargetThroughWallDist;

    /// <summary> Color used for lighting and sprite drawing.
    /// 
    /// Assigned in `Init()` </summary>
    private Color Color;

    /// <summary> Brightness of light cast by Sein.
    /// 
    /// Assigned in `Init()` </summary>
    private float LightStrength;

    /// <summary> Position that Sein hovers around. This determines Sein's general location. </summary>
    private Vector2 BoxPos;

    /// <summary> Position that Sein is moving towards. This is specifically where Sein is moving towards. </summary>
    private Vector2 TargetPos;

    /// <summary> Targeted NPC using the minion targeting feature. </summary>
    private NPC MainTargetNPC;
    
    /// <summary> Slowest speed Sein can be at. </summary>
    private static float MinVelocity => 0.2f;

    /// <summary> Fastest speed Sein can be at in-bounds. </summary>
    private static float MaxVelocityInBounds => 1.32f;

    /// <summary> Fastest speed Sein can be at out-of-bounds.</summary>
    private static float MaxVelocityOutOfBounds => 30f;
    
    /// <summary> Distance that Sein will begin to slow down. </summary>
    private static float NearThreshold => 13f;
    
    /// <summary> How much Sein's speed is reduced if Sein is closer than NearThreshold. </summary>
    private static float Damping => 0.9f;

    /// <summary> The most that Sein's speed can be reduced out of bounds. </summary>
    private static float MaxDampingOutOfBounds => 0.75f;

    /// <summary> How much Sein speeds up when in-bounds. </summary>
    private static float AccelerationInBounds => 1.06f;

    /// <summary> How much Sein speeds up when out-of-bounds. </summary>
    private static float AccelerationOutofBounds => 1.1f;

    /// <summary> Distance from Sein to `TargetPos` to call `SetNewMinionTarget()`. </summary>
    private static float TriggerTargetMove => 0.5f;

    /// <summary> The closest BoxPos must be to an NPC that it is moving towards. </summary>
    private static float MinDistFromNPC => 64f;

    /// <summary> The furthest BoxPos can be from the player.
    /// 
    /// May be modified in `Init()`</summary>
    private float MaxDistFromPlayer = 240f;

    /// <summary> Sound that plays when firing.
    /// 
    /// Assigned in `Init()` </summary>
    private string SpiritFlameSound;
    
    /// <summary> Positions that Sein idly moves to </summary>
    private static Vector2[] TargetPositions { get; } = new Vector2[] {
      new Vector2(-24, 7),
      new Vector2(24, -7),
      new Vector2(-24, -2),
      new Vector2(24, 7),
      new Vector2(-24, -7),
      new Vector2(24, -2),
    };

    /// <summary> Current index of TargetPositions that is active. </summary>
    private int targetPosIndex = 0;

    /// <summary> List of NPCs last targeted by Sein. </summary>
    private List<byte> targetIDs = new List<byte>();
    
    /// <summary> Current number of shots fired in rapid succession. Used to incur LongCooldown. </summary>
    private int currShots = 1;

    /// <summary> Zone around `targetSpawn` that is considered in-bounds. </summary>
    private static Vector2 Bounds { get; } = new Vector2(68f, 32f);

    
    /// <summary> Checks if Sein is within bounds of `targetSpawn`. </summary>
    private bool IsInBounds() {
      Vector2 p = projectile.position;
      return (
        p.X < BoxPos.X + Bounds.X && 
        p.X > BoxPos.X - Bounds.X && 
        p.Y < BoxPos.Y + Bounds.Y && 
        p.Y > BoxPos.Y - Bounds.Y
      );
    }

    /// <summary> Coordinates relative to the player's Center. </summary>
    private Vector2 PlayerSpace(float x, float y) => PlayerSpace(new Vector2(x, y));

    /// <summary> Coordinates relative to the player's Center. </summary>
    private Vector2 PlayerSpace(Vector2 coords=new Vector2()) => player.Center + coords;
    
    /// <summary> Increments or changes `targetPosIndex`. </summary>
    /// <param name="idx">TargetPosition index to change to, or increments by default.</param>
    private void ChangeTargetPos(int idx=-1) {
      targetPosIndex = idx != -1 ? idx : targetPosIndex + 1;
      if (targetPosIndex >= TargetPositions.Length) {
        targetPosIndex = 0;
      }
      UpdateTargetPos();
    }
    private void UpdateTargetPos() => TargetPos = BoxPos + TargetPositions[targetPosIndex];

    /// <summary> Sort method, sorts by NPC distance to player. </summary>
    private int SortByDistClosest(byte id1, byte id2) {
      Vector2 playerPos =  player.Center;
      float length1 = (Main.npc[id1].position - playerPos).Length();
      float length2 = (Main.npc[id2].position - playerPos).Length();
      return length1.CompareTo(length2);
    }

    private void PlaySpiritFlameSound(string Path, float Volume) =>
      Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, ("Sounds/Custom/NewSFX/Ori/SpiritFlame/" + Path)).WithVolume(Volume), projectile.Center);
    private int excludeRand = 0;

    /// <summary> This is the somewhat subtle swaying about Sein does at any given time in Blind Forest </summary>
    private void SeinMovement() {
      if (projectile.position.HasNaNs()) {
        projectile.position = player.position;
      }
      if (projectile.velocity.HasNaNs()) {
        projectile.velocity = new Vector2(0, -MaxVelocityInBounds);
      }
      if ((TargetPos - PlayerSpace()).Length() > 1000 || (BoxPos - PlayerSpace()).Length() > 1000) {
        BoxPos = PlayerSpace(0, -32);
        UpdateTargetPos();
      }

      Vector2 oldVel = projectile.velocity != Vector2.Zero ? projectile.velocity : new Vector2(0, -MaxVelocityInBounds);
      float oldSpd = oldVel.Length();

      Vector2 vectToTarget = TargetPos - projectile.position;
      float distToTarget = vectToTarget.Length();

      Vector2 newDir = vectToTarget.Norm();
      Vector2 newVel = newDir * oldSpd;

      if (distToTarget > 1050) {
        projectile.position = PlayerSpace(-newDir * 1000f);
        projectile.velocity = newDir * MaxVelocityOutOfBounds;
        return;
      }

      if (IsInBounds()) {
        if (distToTarget < NearThreshold) {
          newVel *= Damping;
        }
        else {
          newVel *= AccelerationInBounds;
          if (newVel.Length() > MaxVelocityInBounds) {
            newVel = newDir * OriModUtils.Lerp(newVel.Length(), MaxVelocityInBounds, 0.22f);
          }
        }
      }
      else {
        ChangeTargetPos(newVel.X > 0 ? 3 : 1);
        newVel *= AccelerationOutofBounds;
        if (newVel.Length() > MaxVelocityOutOfBounds) { // Too fast... maybe
          if (newVel.Length() > player.velocity.Length()) {
            newVel = newDir * OriModUtils.Lerp(newVel.Length(), player.velocity.Length(), 0.7f);
          }
          else {
            newVel = newDir * OriModUtils.Lerp(newVel.Length(), MaxVelocityOutOfBounds, 0.2f);
          }
        }
        if (newVel.Length() < (oldSpd * MaxDampingOutOfBounds)) { // Damned more than necessary
          newVel = newDir * OriModUtils.Lerp(oldSpd, newVel.Length(), MaxDampingOutOfBounds);
        }
      }
      if (newVel.Length() < MinVelocity * 2f) { // Too slow
        newVel = newDir * MinVelocity * 2.1f;
        ChangeTargetPos();
      }

      if (distToTarget < NearThreshold || distToTarget > 85) {
        projectile.velocity = (oldVel * 0.25f + newVel * 0.75f).Norm() * newVel.Length();
      }
      else {
        projectile.velocity = (oldVel * 0.8f + newVel * 0.2f).Norm() * newVel.Length();
      }
    }

    /// <summary> Moves `BoxPos` and `TargetPos` to the player's location. </summary>
    private void MoveBoxPosToIdle() {
      BoxPos = PlayerSpace(0, -32f);
      TargetPos = BoxPos + TargetPositions[targetPosIndex];
    }
    
    /// <summary> Moves `BoxPos` and `TargetPos` based on NPC position. </summary>
    private void MoveBoxPosToNPC() {
      Vector2 npcPos = Main.npc[targetIDs[0]].position;
      Vector2 offset = PlayerSpace(0, -32f) - npcPos;

      // Cannot reach targeted NPC
      if (offset.Length() > MaxTargetDist) {
        if (targetIDs.Count == 1 || player.HasMinionAttackTargetNPC) {
          MoveBoxPosToIdle();
          return;
        }
        npcPos = Main.npc[targetIDs[1]].position;
        offset = PlayerSpace(0, -32f) - npcPos;
        // Cannot reach closest NPC
        if (offset.Length() > MaxTargetDist) {
          MoveBoxPosToIdle();
          return;
        }
      }
      float dist = offset.Length();
      BoxPos =
        dist < MinDistFromNPC ? PlayerSpace(0, -32f) :
        dist > MaxDistFromPlayer ? PlayerSpace() - offset.Norm() * MaxDistFromPlayer :
        npcPos + offset.Norm() * MinDistFromNPC;
      
      TargetPos = BoxPos + TargetPositions[targetPosIndex];
    }

    /// <summary> Calls `SetNewMinionTarget()` if close, and calls either `UpdateTargetPosIdle()` or `UpdateTargetPosToNPC()` based on condition. </summary>
    private void UpdateTargetsPos() {
      if ((projectile.position - TargetPos).Length() < TriggerTargetMove && projectile.velocity.Length() > MaxVelocityInBounds)
        ChangeTargetPos();
      if (targetIDs.Count == 0 || Main.npc[targetIDs[0]].active == false)
        MoveBoxPosToIdle();
      else
        MoveBoxPosToNPC();
    }

    /// <summary> Creates one Spirit Flame projectile. </summary>
    /// <param name="t">Index of target in TargetIDs</param>
    private void Fire(int t) {
      Vector2 shootVel;
      Vector2 nonTargetPos = projectile.position;
      float rotation;
      // Fire at enemy NPC
      if (t != -1) {
        shootVel = Main.npc[targetIDs[t]].position - projectile.Center;
        rotation = (float)Main.rand.Next(-RandDegrees, RandDegrees) / 180f * (float)Math.PI;
      }
      // Fire at air
      else {
        shootVel = new Vector2(Main.rand.Next(-12, 12), Main.rand.Next(24, 48));
        rotation = (float)Main.rand.Next(-180, 180) / 180f * (float)Math.PI;
        nonTargetPos.Y += Main.rand.Next(8, 48);
        nonTargetPos = Utils.RotatedBy(nonTargetPos, (float)Main.rand.NextFloat((float)Math.PI * 2));
      }
      if (shootVel == Vector2.Zero) {
        shootVel.Y = 1f;
      }
      shootVel = shootVel.Norm() * ShootSpeed;
      shootVel = Utils.RotatedBy(shootVel, rotation);

      int dmg = (int)(projectile.damage * player.minionDamage *
        (t != 0 ? 1 : PrimaryDamageMultiplier) *
        (Autoshoot ? 1 : ManualShootDamageMultiplier));
      
      int p = Projectile.NewProjectile(projectile.Center, shootVel, ShootID, dmg, projectile.knockBack, Main.myPlayer, 0, 0);
      Projectile proj = Main.projectile[p];
      projectile.velocity += (shootVel * -0.015f);
      if (t == -1) {
        proj.ai[0] = nonTargetPos.X;
        proj.ai[1] = nonTargetPos.Y;
        proj.timeLeft = 15;
      }
      else {
        proj.ai[0] = targetIDs[t];
        proj.ai[1] = 0;
        proj.timeLeft = 300;
      }
      proj.netUpdate = true;
      proj.penetrate = Pierce;
      proj.owner = projectile.owner;
    }

    internal override void CheckActive() {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>(mod);
      if (projectile.whoAmI == oPlayer.SeinMinionID) {
        projectile.timeLeft = 2;
      }
    }

    internal override void Behavior() {
      SeinMovement();
      UpdateTargetsPos();
      Lighting.AddLight(projectile.Center, Color.ToVector3() * LightStrength);

      bool targeting = false;
      
      // If player specifies target, add that target to selection
      if (player.HasMinionAttackTargetNPC) {
        NPC npc = Main.npc[player.MinionAttackTargetNPC];
        if (npc.CanBeChasedBy(this, false)) {
          float dist = Vector2.Distance(player.Center, npc.Center);
          if (
            dist < MaxTargetThroughWallDist ||
            dist < MaxTargetDist && Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height)
          ) {
            targeting = true;
            MainTargetNPC = npc;
          }
        }
      }

      var newTargetIDs = new List<byte>();
      var wormIDs = new List<byte>();

      // Set target based on different enemies, if they can be hit
      for (int k = 0; k < Main.maxNPCs; k++) {
        NPC npc = Main.npc[k];
        if (!npc.CanBeChasedBy(this, false) || !npc.active) continue;
        float dist = Vector2.Distance(player.Center, npc.Center);
        if (
          dist < MaxTargetThroughWallDist ||
          dist < MaxTargetDist && Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height)
        ) {
          // Worms...
          if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
            if (wormIDs.Contains((byte)npc.ai[3])) continue;
            wormIDs.Add((byte)npc.ai[3]);
          }
          targeting = true;
          newTargetIDs.Add((byte)npc.whoAmI);
        }
      }

       // See if list needs to be replaced
      bool replaceList = false;

      // Cheap check, count is different or contains different NPCs
      if (newTargetIDs.Count != targetIDs.Count || targetIDs.Except(newTargetIDs).Count() != 0) {
        replaceList = true; 
      }
      else {
        // Expensive check, compare position of each NPC
        float dist = 0;
        for (int t = 0; t < targetIDs.Count; t++) {
          float npcDist = (player.Center - Main.npc[targetIDs[t]].position).Length();
          if (npcDist < dist) {
            replaceList = true; // List of NPCs is no longer in order of distance
            break;
          }
          else {
            dist = npcDist;
          }
        }
      }

      if (replaceList) {
        if (newTargetIDs.Count > 1) {
          newTargetIDs.Sort(SortByDistClosest);
        }
        targetIDs.Clear();
        if (MainTargetNPC?.active ?? false) {
          targetIDs.Add((byte)MainTargetNPC.whoAmI);
          targetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, MaxTargets - 1)));
        }
        targetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, MaxTargets)));
      }

      // Manage Cooldown
      float minCooldown = MinCooldown * (Autoshoot ? 1.5f : 1);
      float shortCooldown = ShortCooldown * (Autoshoot ? 1.5f : 1);
      float longCooldown = LongCooldown * (Autoshoot ? 2f : 1);
      if (Cooldown > 0) {
        Cooldown += 1;
        if (Cooldown > longCooldown) {
          Cooldown = 0;
          currShots = 1;
        }
      }

      // Spirit Flame
      bool attemptFire = Autoshoot ? targeting : (PlayerInput.Triggers.JustPressed.MouseLeft && !Main.LocalPlayer.mouseInterface);
      if (attemptFire && (Cooldown == 0 || Cooldown > minCooldown && currShots < MaxShotsPerBurst)) {
        currShots = Cooldown > shortCooldown ? 1 : currShots + 1;
        Cooldown = 1;

        PlaySpiritFlameSound("Throw" + SpiritFlameSound + OriPlayer.RandomChar(3, ref excludeRand), 0.6f);
        if (Main.myPlayer == projectile.owner) {
          if (!targeting) {
            // Fire at air - nothing to target
            for (int i = 0; i < ShotsToPrimaryTarget; i++) {
              Fire(-1);
            }
            return;
          }

          int usedShots = 0;
          int loopCount = 0;
          while (loopCount < ShotsToPrimaryTarget) {
            for (int t = 0; t < targetIDs.Count; t++) {
              if (loopCount < (t == 0 ? ShotsToPrimaryTarget : ShotsToTarget)) {
                Fire(t);
                if (++usedShots >= MaxShotsPerVolley) break;
              }
            }
            loopCount++;
          }
          projectile.netUpdate = true;
        }
      }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
      projectile.tileCollide = false;
      fallThrough = true;
      width = 4;
      height = 4;
      return false;
    }

    /// <summary> Texture of the sprite </summary>
    private Texture2D GlowTex => !_tex?.IsDisposed ?? false ? _tex : (_tex = mod.GetTexture("Projectiles/Minions/Sein_Glow"));
    private Texture2D _tex;

    public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
      var pos = new Vector2(projectile.BottomRight.X - Main.screenPosition.X, projectile.BottomRight.Y - Main.screenPosition.Y);
      Vector2 orig = new Vector2(GlowTex.Width, GlowTex.Width) * 0.5f;
      for (int i = 0; i < 3; i++) {
        Color color = this.Color;
        color.A = 255;
        if (color == Color.Black) color = Color.White;
        color.A = (byte)(i == 0 ? 255 : i == 1 ? 200 : 175);
        var sourceRect = new Rectangle(0, i * GlowTex.Height / 3, GlowTex.Width, GlowTex.Width);
        spriteBatch.Draw(GlowTex, pos, sourceRect, color, projectile.rotation, orig, projectile.scale, SpriteEffects.None, 0f);
      }
    }
  }
}