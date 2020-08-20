using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using OriMod.Utilities;

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
      projectile.minion = true;
      projectile.minionSlots = -0.001f;
      projectile.penetrate = -1;
      projectile.timeLeft = 18000;
      projectile.tileCollide = false;
      projectile.ignoreWater = true;
      projectile.velocity = new Vector2(0, -MaxVelocityInBounds);
      projectile.position = PlayerSpace();
      BoxPos = TargetPos = PlayerSpace(0, -32);
      Initialize(Init);
    }

    /// <summary>
    /// Assigns most fields based on upgradeID
    /// </summary>
    /// <param name="init">Index of OriMod.SeinUpgrades to use. </param>
    private void Initialize(int init) {
      SeinUpgrade u = OriMod.Instance.SeinUpgrades[init - 1];
      MaxShotsPerBurst = u.shotsPerBurst;
      MaxShotsPerVolley = u.maxShotsPerVolley;
      ShotsToTarget = u.shotsPerTarget;
      ShotsToPrimaryTarget = u.shotsToPrimaryTarget;
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

      ShootID = mod.ProjectileType("SpiritFlame" + init);
      Color = u.color;
      LightStrength = u.lightStrength;

      SpiritFlameSound =
        init <= 2 ? "" :
        init <= 4 ? "LevelB" :
        init <= 6 ? "LevelC" :
        init <= 8 ? "LevelD" : "";
    }

    /// <summary>
    /// True if the held item is not the same type that spawned this projectile.
    /// </summary>
    private bool Autoshoot {
      get {
        if (projectile.owner == 255) {
          return false;
        }

        var item = Main.player[projectile.owner].HeldItem;
        return item.shoot != projectile.type;
      }
    }

    /// <summary>
    /// Current Cooldown of Spirit Flame
    /// </summary>
    private int Cooldown {
      get => (int)projectile.ai[0];
      set => projectile.ai[0] = value;
    }

    /// <summary>
    /// ID of projectile to shoot. Assigned in Init()
    /// </summary>
    private int ShootID;

    /// <summary>
    /// Number of shots that can be used before triggering longCooldown. Assigned in Init()
    /// </summary>
    private int MaxShotsPerBurst;

    /// <summary>
    /// Max number of shots that can be fired at once. Assigned in Init()
    /// </summary>
    private int MaxShotsPerVolley;

    /// <summary>
    /// Max number of shots that can be fired at a single target. Assigned in Init()
    /// </summary>
    private int ShotsToTarget;

    /// <summary>
    /// Max number of shots that can be fired at the first target. Assigned in Init()
    /// </summary>
    private int ShotsToPrimaryTarget;

    /// <summary>
    /// Maximum number of targets that can be fired upon at once. Assigned in Init()
    /// </summary>
    private int MaxTargets;

    /// <summary> 
    /// Shortest cooldown between individual shots. Assigned in Init()
    /// </summary>
    private float MinCooldown;

    /// <summary>
    /// Shortest cooldown to count as a seperate burst and not incur longCooldown. Assigned in Init()
    /// </summary>
    private float ShortCooldown;

    /// <summary>
    /// Cooldown between bursts of shots dictated by numShots. Assigned in Init()
    /// </summary>
    private float LongCooldown;

    /// <summary>
    /// Speed of the created projectile
    /// </summary>
    private static float ShootSpeed => 50f;

    /// <summary>
    /// Spirit Flame penetration. Assigned in Init()
    /// </summary>
    private int Pierce = 1;

    /// <summary>
    /// Spirit Flame damage to the first target in Fire(). Assigned in Init()
    /// </summary>
    private float PrimaryDamageMultiplier;

    /// <summary>
    /// How much more manually firing Spirit Flame damages for.
    /// </summary>
    private static float ManualShootDamageMultiplier => 1.4f;

    /// <summary>
    /// Degrees from direction to target NPC that a Spirit Flame can be shot at. Assigned in Init()
    /// </summary>
    private int RandDegrees;

    /// <summary>
    /// Range from player that NPCs can be targeted. Assigned in Init()
    /// </summary>
    private float MaxTargetDist;

    /// <summary>
    /// Range from player that NPCs can be targeted, ignoring walls. Assigned in Init()
    /// </summary>
    private float MaxTargetThroughWallDist;

    /// <summary>
    /// Color used for lighting and sprite drawing. Assigned in Init()
    /// </summary>
    private Color Color;

    /// <summary>
    /// Brightness of light created by this projectile. Assigned in Init()
    /// </summary>
    private float LightStrength;

    /// <summary>
    /// Position that Sein hovers around. This determines this projectile's general location.
    /// </summary>
    private Vector2 BoxPos;

    /// <summary>
    /// Position that this projectile is moving towards. This is specifically where Sein is moving towards.
    /// </summary>
    private Vector2 TargetPos;

    /// <summary>
    /// Targeted NPC using the minion targeting feature.
    /// </summary>
    private NPC MainTargetNPC;

    /// <summary>
    /// Slowest speed this projectile can be at.
    /// </summary>
    private static float MinVelocity => 0.4f;

    /// <summary>
    /// Fastest speed this projectile can be at in-bounds.
    /// </summary>
    private static float MaxVelocityInBounds => 1.32f;

    /// <summary>
    /// Fastest speed this projectile can be at out-of-bounds.
    /// </summary>
    private static float MaxVelocityOutOfBounds => 20f;

    /// <summary>
    /// Distance that this projectile will begin to slow down.
    /// </summary>
    private static float NearThreshold => 12f;

    /// <summary>
    /// How much this projectile's speed is reduced if it is closer than NearThreshold.
    /// </summary>
    private static float Damping => 0.9f;

    /// <summary>
    /// How much this projectile speeds up when in-bounds.
    /// </summary>
    private static float AccelerationInBounds => 1.06f;

    /// <summary>
    /// How much this projectile speeds up when out-of-bounds.
    /// </summary>
    private static float AccelerationOutofBounds => 1.1f;

    /// <summary>
    /// Distance from this projectile to TargetPos to call SetNewMinionTarget().
    /// </summary>
    private static float TriggerTargetMove => 0.5f;

    /// <summary>
    /// The closest BoxPos must be to an NPC that it is moving towards.
    /// </summary>
    private static float MinDistFromNPC => 64f;

    /// <summary>
    /// The furthest BoxPos can be from the player. May be modified in Init()
    /// </summary>
    private float MaxDistFromPlayer = 240f;

    /// <summary>
    /// Sound that plays when firing. Assigned in Init()
    /// </summary>
    private string SpiritFlameSound;

    /// <summary>
    /// Current index of TargetPositions that is active.
    /// </summary>
    private int targetPosIndex = 0;

    /// <summary>
    /// List of NPCs last targeted by Sein.
    /// </summary>
    private List<byte> TargetIDs { get; } = new List<byte>();

    /// <summary>
    /// Current number of shots fired in rapid succession. Used to incur LongCooldown.
    /// </summary>
    private int currShots = 1;

    /// <summary>
    /// Zone around targetSpawn that is considered in-bounds.
    /// </summary>
    private static Vector2 Bounds { get; } = new Vector2(78f, 40f);

    /// <summary>
    /// Positions that this projectile idly moves towards
    /// </summary>
    private static Vector2[] TargetPositions { get; } = new Vector2[] {
      new Vector2(-32, 12),
      new Vector2(32, -12),
      new Vector2(-32, -12),
      new Vector2(32, 12),
      new Vector2(-32, -12),
      new Vector2(32, -12),
    };

    /// <summary>
    /// Checks if Sein is within bounds of targetSpawn.
    /// </summary>
    private bool IsInBounds() {
      Vector2 p = projectile.position;
      return
        p.X < BoxPos.X + Bounds.X &&
        p.X > BoxPos.X - Bounds.X &&
        p.Y < BoxPos.Y + Bounds.Y &&
        p.Y > BoxPos.Y - Bounds.Y
      ;
    }

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(float x, float y) => PlayerSpace(new Vector2(x, y));

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(Vector2 coords = new Vector2()) => Main.player[projectile.owner].Center + coords;

    private void UpdateTargetLocation() => TargetPos = BoxPos + TargetPositions[targetPosIndex];

    private void UpdateTargetPosIdx() {
      if (++targetPosIndex >= TargetPositions.Length) {
        targetPosIndex = 0;
      }
      UpdateTargetLocation();
    }
    
    /// <summary>
    /// Increments or changes targetPosIndex.
    /// </summary>
    /// <param name="idx">TargetPosition index to change to</param>
    private void SetTargetPosIdx(int idx) {
      targetPosIndex = idx;
      if (targetPosIndex >= TargetPositions.Length) {
        targetPosIndex = 0;
      }
      UpdateTargetLocation();
    }

    /// <summary>
    /// Sort method, sorts by NPC distance to player.
    /// </summary>
    private int SortByDistClosest(byte id1, byte id2) {
      Vector2 playerPos = Main.player[projectile.owner].Center;
      float length1 = (Main.npc[id1].position - playerPos).LengthSquared();
      float length2 = (Main.npc[id2].position - playerPos).LengthSquared();
      return length1.CompareTo(length2);
    }

    private void PlaySpiritFlameSound(string Path, float Volume) =>
      Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/Ori/SpiritFlame/" + Path).WithVolume(Volume), projectile.Center);

    private readonly RandomChar randomChar = new RandomChar();

    /// <summary>
    /// This is the somewhat subtle swaying about Sein does at any given time in Blind Forest
    /// </summary>
    private void SeinMovement() {
      if (projectile.position.HasNaNs()) {
        projectile.position = Main.player[projectile.owner].position;
      }
      if (projectile.velocity.HasNaNs()) {
        projectile.velocity = new Vector2(0, -MaxVelocityInBounds);
      }
      if ((TargetPos - PlayerSpace()).LengthSquared() > 1000 * 1000 || (BoxPos - PlayerSpace()).LengthSquared() > 1000 * 1000) {
        BoxPos = PlayerSpace(0, -32);
        UpdateTargetLocation();
      }

      Vector2 oldVel = projectile.velocity != Vector2.Zero ? projectile.velocity : new Vector2(0, -MaxVelocityInBounds);
      float oldSpd = oldVel.Length();

      Vector2 vectToTarget = TargetPos - projectile.position;
      float distToTarget = vectToTarget.Length();

      if (distToTarget < TriggerTargetMove) {
        vectToTarget = TargetPos - projectile.position;
        distToTarget = vectToTarget.Length();
      }

      Vector2 newDir = vectToTarget.Normalized();

      if (distToTarget > 1050) {
        projectile.position = PlayerSpace(-newDir * 1000f);
        projectile.velocity = newDir * MaxVelocityOutOfBounds;
        return;
      }

      bool inBounds = IsInBounds();

      if (!inBounds) {
        SetTargetPosIdx(vectToTarget.X > 0 ? 3 : 1);
      }

      Vector2 newVel = newDir * (distToTarget / 15);
      float newSpd = newVel.Length();

      if (inBounds) {
        if (newSpd - oldSpd > oldSpd * (AccelerationInBounds - 1)) {
          newVel = newVel.Normalized() * oldSpd * AccelerationInBounds;
        }
        if (distToTarget < NearThreshold) {
          newVel = newVel.Normalized() * oldSpd * Damping;
        }
        newSpd = newVel.Length();
        if (newSpd < MinVelocity) {
          newVel = newDir * MinVelocity;
        }
        else if (newSpd > MaxVelocityInBounds) {
          newVel = newDir * MaxVelocityInBounds;
        }
      }
      else {
        if (newSpd - oldSpd > oldSpd * (AccelerationOutofBounds - 1)) {
          newVel = newVel.Normalized() * oldSpd * AccelerationOutofBounds;
        }
        newSpd = newVel.Length();
        if (newSpd > MaxVelocityOutOfBounds) {
          newVel = newDir * MaxVelocityOutOfBounds;
        }
      }

      projectile.velocity = newVel;
    }

    /// <summary>
    /// Moves BoxPos and TargetPos to the player's location.
    /// </summary>
    private void MoveBoxPosToIdle() {
      BoxPos = PlayerSpace(0, -32f);
      TargetPos = BoxPos + TargetPositions[targetPosIndex];
    }

    /// <summary>
    /// Moves BoxPos and TargetPos based on NPC position.
    /// </summary>
    private void MoveBoxPosToNPC() {
      var player = Main.player[projectile.owner];
      Vector2 target = Main.npc[TargetIDs[0]].position;
      Vector2 offset = player.Center - target;

      // Cannot reach targeted NPC
      float dist = offset.LengthSquared();
      if (dist > MaxTargetDist * MaxTargetDist) {
        if (TargetIDs.Count == 1 || player.HasMinionAttackTargetNPC) {
          MoveBoxPosToIdle();
          return;
        }
        target = Main.npc[TargetIDs[1]].position;
        offset = player.Center - target;
        dist = offset.LengthSquared();
        // Cannot reach closest NPC
        if (dist > MaxTargetDist * MaxTargetDist) {
          MoveBoxPosToIdle();
          return;
        }
      }
      BoxPos =
        dist + MinDistFromNPC > MaxDistFromPlayer ? player.Center - offset.Normalized() * MaxDistFromPlayer :
        target + offset.Normalized() * MinDistFromNPC;

      TargetPos = BoxPos + TargetPositions[targetPosIndex];
    }

    /// <summary>
    /// Calls UpdateTargetPosIdx() if close, and calls either MoveBoxPosToIdle() or MoveBoxPosToNPC() based on condition.
    /// </summary>
    private void UpdateTargetsPos() {
      if ((projectile.position - TargetPos).Length() < TriggerTargetMove) {
        UpdateTargetPosIdx();
      }

      if (TargetIDs.Count == 0 || Main.npc[TargetIDs[0]].active == false) {
        MoveBoxPosToIdle();
      }
      else {
        MoveBoxPosToNPC();
      }
    }

    /// <summary>
    /// Creates one Spirit Flame projectile.
    /// </summary>
    /// <param name="npc">NPC to target. If null, fires at the air randomly</param>
    /// <param name="primary">If to increase damage by primary damage multiplier</param>
    private void Fire(NPC npc = null, bool primary = false) {
      Vector2 shootVel;
      float rotation;
      // Fire at enemy NPC
      if (npc != null) {
        shootVel = npc.position - projectile.Center;
        rotation = Main.rand.Next(-RandDegrees, RandDegrees) / 180f * (float)Math.PI;
      }
      // Fire at air
      else {
        shootVel = new Vector2(Main.rand.Next(-12, 12), Main.rand.Next(24, 48)).Normalized();
        rotation = (float)(Main.rand.Next(-180, 180) / 180f * Math.PI);
      }
      if (shootVel == Vector2.Zero) {
        shootVel = Vector2.UnitY;
      }
      shootVel = Utils.RotatedBy(shootVel * ShootSpeed, rotation);

      int dmg = (int)(projectile.damage * Main.player[projectile.owner].minionDamage *
        (primary ? PrimaryDamageMultiplier : 1) *
        (!Autoshoot ? ManualShootDamageMultiplier : 1));

      Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, shootVel, ShootID, dmg, projectile.knockBack, Main.myPlayer, 0, 0)];
      projectile.velocity += shootVel * -0.015f;
      proj.netUpdate = true;
      proj.penetrate = Pierce;
      proj.owner = projectile.owner;

      if (npc is null) {
        var pos = Utils.RotatedBy(new Vector2(projectile.position.X, projectile.position.Y + Main.rand.Next(8, 48)), Main.rand.NextFloat((float)Math.PI * 2));
        proj.ai[0] = pos.X;
        proj.ai[1] = pos.Y;
        proj.timeLeft = 15;
      }
      else {
        proj.ai[0] = npc.whoAmI;
        proj.ai[1] = 0;
        proj.timeLeft = 300;
      }
    }

    protected abstract int BuffType();

    internal override void CheckActive() {
      var player = Main.player[projectile.owner];
      var oPlayer = player.GetModPlayer<OriPlayer>();
      if (player.dead || !player.active) {
        oPlayer.RemoveSeinBuffs();
      }
      else if (projectile.type == oPlayer.SeinMinionType && player.HasBuff(BuffType())) {
        projectile.timeLeft = 2;
      }
    }

    internal override void Behavior() {
      var player = Main.player[projectile.owner];
      SeinMovement();
      UpdateTargetsPos();
      Lighting.AddLight(projectile.Center, Color.ToVector3() * LightStrength);

      #region Targeting
      bool inSight(NPC npc) => Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
      bool hasTarget = false;

      var newTargetIDs = new List<byte>();
      var wormIDs = new List<byte>();

      // If player specifies target, add that target to selection
      if (player.HasMinionAttackTargetNPC) {
        NPC npc = Main.npc[player.MinionAttackTargetNPC];
        if (npc.CanBeChasedBy()) {
          float dist = Vector2.Distance(player.Center, npc.Center);
          if (dist < MaxTargetThroughWallDist || dist < MaxTargetDist && inSight(npc)) {
            // Worms...
            if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
              wormIDs.Add((byte)npc.ai[3]);
            }
            hasTarget = true;
            MainTargetNPC = npc;
          }
        }
      }

      // Set target based on different enemies, if they can be hit
      if (!hasTarget || MaxTargets < 1) {
        for (int i = 0; i < Main.maxNPCs; i++) {
          NPC npc = Main.npc[i];
          if (npc.CanBeChasedBy()) {
            float dist = Vector2.Distance(player.Center, npc.Center);
            if (dist < MaxTargetThroughWallDist || dist < MaxTargetDist && inSight(npc)) {
              // Worms...
              if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
                if (wormIDs.Contains((byte)npc.ai[3])) {
                  continue;
                }

                wormIDs.Add((byte)npc.ai[3]);
              }
              hasTarget = true;
              newTargetIDs.Add((byte)npc.whoAmI);
            }
          }
        }
      }

      // See if list needs to be replaced
      bool replaceList = false;

      // Cheap check, count is different or contains different NPCs
      if (newTargetIDs.Count != TargetIDs.Count || TargetIDs.Except(newTargetIDs).Count() != 0) {
        replaceList = true;
      }
      else {
        // More expensive check, compare position of each NPC
        float dist = 0;
        for (int t = 0, len = TargetIDs.Count; t < len; t++) {
          float npcDist = (player.Center - Main.npc[TargetIDs[t]].position).LengthSquared();
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
        TargetIDs.Clear();
        if (MainTargetNPC?.active ?? false) {
          TargetIDs.Add((byte)MainTargetNPC.whoAmI);
          TargetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, MaxTargets - 1)));
        }
        else {
          TargetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, MaxTargets)));
        }
      }
      #endregion

      #region Cooldown
      float minCooldown = MinCooldown * (Autoshoot ? 1.5f : 1);
      float shortCooldown = ShortCooldown * (Autoshoot ? 1.5f : 1);
      float longCooldown = LongCooldown * (Autoshoot ? 2f : 1);
      if (Cooldown > 0) {
        Cooldown++;
        if (Cooldown > longCooldown) {
          Cooldown = 0;
          currShots = 1;
        }
      }
      #endregion

      #region Firing
      // Spirit Flame
      bool attemptFire;
      if (Autoshoot) {
        attemptFire = hasTarget;
      }
      else {
        attemptFire = PlayerInput.Triggers.JustPressed.MouseLeft && !Main.LocalPlayer.mouseInterface;
      }

      if (attemptFire && (Cooldown == 0 || Cooldown > minCooldown && currShots < MaxShotsPerBurst)) {
        if (Cooldown > shortCooldown) {
          currShots = 1;
        }
        else {
          currShots++;
        }
        Cooldown = 1;

        if (Main.myPlayer == projectile.owner) {
          PlaySpiritFlameSound("Throw" + SpiritFlameSound + randomChar.NextNoRepeat(3), 0.6f);

          if (!hasTarget) {
            // Fire at air - nothing to target
            for (int i = 0; i < ShotsToPrimaryTarget; i++) {
              Fire();
            }
            return;
          }

          int usedShots = 0;
          int loopCount = 0;
          while (loopCount < ShotsToPrimaryTarget) {
            for (int t = 0; t < TargetIDs.Count; t++) {
              bool isPrimary = t == 0;
              int shots = isPrimary ? ShotsToPrimaryTarget : ShotsToTarget;
              if (loopCount < shots) {
                Fire(Main.npc[TargetIDs[t]], isPrimary);
                if (++usedShots >= MaxShotsPerVolley) {
                  break;
                }
              }
            }
            loopCount++;
          }
          projectile.netUpdate = true;
        }
      }
      #endregion
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
      fallThrough = true;
      width = 4;
      height = 4;
      return false;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
      var pos = projectile.BottomRight - Main.screenPosition;
      var tex = OriTextures.Instance.Sein.texture;
      var orig = new Vector2(tex.Width, tex.Width) * 0.5f;
      for (int i = 0; i < 3; i++) {
        var color = Color;
        color.A = 255;
        if (color == Color.Black) {
          color = Color.White;
        }

        color.A = (byte)(i == 0 ? 255 : i == 1 ? 200 : 175);
        var sourceRect = new Rectangle(0, i * tex.Height / 3, tex.Width, tex.Width);
        spriteBatch.Draw(tex, pos, sourceRect, color, projectile.rotation, orig, projectile.scale, SpriteEffects.None, 0f);
      }
    }
  }
}