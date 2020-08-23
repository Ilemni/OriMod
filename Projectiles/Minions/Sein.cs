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
  /// <summary>
  /// Minion for Sein
  /// </summary>
  public abstract class Sein : Minion {
    static Sein() => OriMod.OnUnload += Unload;
    public override sealed string Texture => "OriMod/Projectiles/Minions/Sein";

    public override sealed bool? CanCutTiles() => false;

    public override sealed void SetStaticDefaults() {
      Main.projFrames[projectile.type] = 3;
      Main.projPet[projectile.type] = true;
      ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
      ProjectileID.Sets.Homing[projectile.type] = true;
      ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
    }

    /// <summary>
    /// Type used for <see cref="Sein"/>. Values are indices to <see cref="OriMod.SeinUpgrades"/>.
    /// </summary>
    protected abstract byte SeinType { get; }

    /// <summary>
    /// Type for <see cref="Buffs.SeinBuff"/>. This value should be from <see cref="ModContent.BuffType{T}"/>
    /// </summary>
    protected abstract ushort BuffType { get; }

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
      baseHoverPosition = hoverPosition = PlayerSpace(0, -32);
      UpdateHoverPosition();
      Initialize();
    }

    /// <summary>
    /// Assigns fields that are based on <see cref="SeinType"/>
    /// </summary>
    private void Initialize() {
      var type = SeinType;
      SeinUpgrade u = OriMod.Instance.SeinUpgrades[type - 1];
      maxShotsPerBurst = u.shotsPerBurst;
      maxShotsPerVolley = u.maxShotsPerVolley;
      shotsToTarget = u.shotsPerTarget;
      shotsToPrimaryTarget = u.shotsToPrimaryTarget;
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

      spiritFlameType = mod.ProjectileType("SpiritFlame" + type);
      color = u.color;
      lightStrength = u.lightStrength;

      spiritFlameSound =
        type <= 2 ? "" :
        type <= 4 ? "LevelB" :
        type <= 6 ? "LevelC" :
        type <= 8 ? "LevelD" : "";
    }

    /// <summary>
    /// Whether the AI should automatically fire projectiles or not.
    /// </summary>
    /// <returns>True if the held item is not the same type that spawned this projectile.</returns>
    private bool AutoFire {
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
    /// <remarks>Uses <see cref="Projectile.ai"/>[0]</remarks>
    private int Cooldown {
      get => (int)projectile.ai[0];
      set => projectile.ai[0] = value;
    }

    /// <summary>
    /// ID of <see cref="SpiritFlame"/> to shoot. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int spiritFlameType;

    /// <summary>
    /// Number of shots that can be used before triggering <see cref="longCooldown"/>. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int maxShotsPerBurst;

    /// <summary>
    /// Max number of shots that can be fired at once. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int maxShotsPerVolley;

    /// <summary>
    /// Max number of shots that can be fired at a single target. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int shotsToTarget;

    /// <summary>
    /// Max number of shots that can be fired at the first target. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int shotsToPrimaryTarget;

    /// <summary>
    /// Maximum number of targets that can be fired upon at once. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int maxTargets;

    /// <summary> 
    /// Shortest cooldown between individual shots. Assigned in <see cref="Initialize"/>
    /// </summary>
    private float minCooldown;

    /// <summary>
    /// Duration a player can optionally wait to avoid incurring <see cref="longCooldown"/>. Assigned in <see cref="Initialize"/>
    /// </summary>
    private float shortCooldown;

    /// <summary>
    /// Cooldown between bursts of shots dictated by <see cref="maxShotsPerBurst"/>. Assigned in <see cref="Initialize"/>
    /// </summary>
    private float longCooldown;

    /// <summary>
    /// Speed of the created <see cref="SpiritFlame"/>
    /// </summary>
    private static float ShootSpeed => 50f;

    /// <summary>
    /// Damage multiplier for when the player manually fires Spirit Flame.
    /// </summary>
    private static float ManualShootDamageMultiplier => 1.4f;

    /// <summary>
    /// Degrees from direction to target NPC that a Spirit Flame can be shot at. Assigned in <see cref="Initialize"/>
    /// </summary>
    private int randDegrees;

    /// <summary>
    /// Range from player that NPCs can be targeted. Walls may obstruct targeting. Assigned in <see cref="Initialize"/>
    /// </summary>
    private float maxTargetDist;

    /// <summary>
    /// Range from player that NPCs can be targeted, ignoring walls. Assigned in <see cref="Initialize"/>
    /// </summary>
    private float maxTargetThroughWallDist;

    /// <summary>
    /// Color used for lighting and sprite drawing. Assigned in <see cref="Initialize"/>
    /// </summary>
    private Color color;

    /// <summary>
    /// Brightness of light created by this minion. Assigned in <see cref="Initialize"/>
    /// </summary>
    private float lightStrength;

    /// <summary>
    /// General position that Sein hovers around. This is a general location, and not precisely where the minion moves.
    /// </summary>
    private Vector2 baseHoverPosition;

    /// <summary>
    /// Exact position that this minion is moving towards. This is set to be around <see cref="baseHoverPosition"/>.
    /// </summary>
    private Vector2 hoverPosition;

    /// <summary>
    /// Targeted NPC using the minion targeting feature.
    /// </summary>
    private NPC mainTargetNPC;

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
    private static float MaxVelocityOutOfBounds => 5f;

    /// <summary>
    /// Distance that this projectile will begin to slow down.
    /// </summary>
    private static float NearThreshold => 12f;

    /// <summary>
    /// How much this projectile's speed is reduced if it is closer than <see cref="NearThreshold"/>.
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
    /// Distance from this projectile to <see cref="hoverPosition"/> to call <see cref="IncrementHoverPositionIdx"/>.
    /// </summary>
    private static float TriggerTargetMove => 0.5f;

    /// <summary>
    /// The closest <see cref="baseHoverPosition"/> must be to an NPC that it is moving towards.
    /// </summary>
    private static float MinDistFromNPC => 64f;

    /// <summary>
    /// The furthest <see cref="baseHoverPosition"/> can be from the player. May be modified in <see cref="Initialize"/>
    /// </summary>
    private float maxDistFromPlayer = 200f;

    /// <summary>
    /// Sound that plays when firing. Assigned in <see cref="Initialize"/>
    /// </summary>
    private string spiritFlameSound;

    /// <summary>
    /// Current index of <see cref="HoverPositions"/> that is active.
    /// </summary>
    private int hoverPositionIdx = 0;

    /// <summary>
    /// List of NPCs last targeted by the minion.
    /// </summary>
    private readonly List<byte> targetIDs = new List<byte>();

    /// <summary>
    /// Current number of shots fired in rapid succession. Used to incur <see cref="longCooldown"/>.
    /// </summary>
    private int currentShotsFired = 1;

    /// <summary>
    /// Zone around <see cref="baseHoverPosition"/> that is considered in-bounds.
    /// </summary>
    private static Vector2 SeinBounds { get; } = new Vector2(78f, 40f);

    /// <summary>
    /// Positions that the minion idly moves towards. Positions are relative to <see cref="baseHoverPosition"/>
    /// </summary>
    private static Vector2[] HoverPositions => _hoverPositions ?? (_hoverPositions = new Vector2[] {
      new Vector2(-32, 12),
      new Vector2(32, -12),
      new Vector2(-32, -12),
      new Vector2(32, 12),
      new Vector2(-32, -12),
      new Vector2(32, -12),
    });
    private static Vector2[] _hoverPositions;

    /// <summary>
    /// Checks if Sein is within bounds of targetSpawn.
    /// </summary>
    private bool IsInBounds() {
      Vector2 p = projectile.position;
      return
        p.X < baseHoverPosition.X + SeinBounds.X &&
        p.X > baseHoverPosition.X - SeinBounds.X &&
        p.Y < baseHoverPosition.Y + SeinBounds.Y &&
        p.Y > baseHoverPosition.Y - SeinBounds.Y
      ;
    }

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(float x, float y) => PlayerSpace(new Vector2(x, y));

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(Vector2 coords = default) => Main.player[projectile.owner].Center + coords;

    private void UpdateHoverPosition() => hoverPosition = baseHoverPosition + HoverPositions[hoverPositionIdx];

    /// <summary>
    /// Increments or wraps <see cref="hoverPositionIdx"/>.
    /// </summary>
    private void IncrementHoverPositionIdx() {
      if (++hoverPositionIdx >= HoverPositions.Length) {
        hoverPositionIdx = 0;
      }
      UpdateHoverPosition();
    }
    
    /// <summary>
    /// Sets <see cref="hoverPositionIdx"/> to <paramref name="idx"/>.
    /// </summary>
    /// <param name="idx">TargetPosition index to change to</param>
    private void SetHoverPositionIdx(int idx) {
      hoverPositionIdx = idx;
      if (hoverPositionIdx >= HoverPositions.Length || hoverPositionIdx < 0) {
        hoverPositionIdx = 0;
      }
      UpdateHoverPosition();
    }

    /// <summary>
    /// Sort method, sorts by <see cref="NPC"/> distance to player.
    /// </summary>
    /// <param name="id1"><see cref="Entity.whoAmI"/> of first <see cref="NPC"/>.</param>
    /// <param name="id2"><see cref="Entity.whoAmI"/> of second <see cref="NPC"/>.</param>
    /// <returns><see cref="float.CompareTo(float)"/> using <see cref="Vector2.LengthSquared"/> between the player and <see cref="NPC"/>.</returns>
    private int SortByDistanceClosest(byte id1, byte id2) {
      Vector2 playerPos = Main.player[projectile.owner].Center;
      float length1 = (Main.npc[id1].position - playerPos).LengthSquared();
      float length2 = (Main.npc[id2].position - playerPos).LengthSquared();
      return length1.CompareTo(length2);
    }

    /// <summary>
    /// Plays a Spirit Flame sound effect with the given <paramref name="path"/> and <paramref name="volume"/>.
    /// </summary>
    /// <param name="path">Path of the sound effect to play. Relative to the Spirit Flame folder.</param>
    /// <param name="volume">Volume to play the sound at.</param>
    private void PlaySpiritFlameSound(string path, float volume) =>
      Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/Ori/SpiritFlame/" + path).WithVolume(volume), projectile.Center);

    private readonly RandomChar rand = new RandomChar();

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
      if ((hoverPosition - PlayerSpace()).LengthSquared() > 1000000 || (baseHoverPosition - PlayerSpace()).LengthSquared() > 1000000) {
        baseHoverPosition = PlayerSpace(0, -32);
        UpdateHoverPosition();
      }

      Vector2 oldVel = projectile.velocity != Vector2.Zero ? projectile.velocity : new Vector2(0, -MaxVelocityInBounds);
      float oldSpd = oldVel.Length();

      Vector2 vectToTarget = hoverPosition - projectile.position;
      float distToTarget = vectToTarget.Length();

      if (distToTarget < TriggerTargetMove) {
        vectToTarget = hoverPosition - projectile.position;
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
        SetHoverPositionIdx(vectToTarget.X > 0 ? 3 : 1);
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
    /// Sets <see cref="baseHoverPosition"/> and <see cref="hoverPosition"/> to the player's location.
    /// </summary>
    private void SetHoverPositionToIdle() {
      baseHoverPosition = PlayerSpace(0, -32f);
      UpdateHoverPosition();
    }

    /// <summary>
    /// Sets <see cref="baseHoverPosition"/> and <see cref="hoverPosition"/> to the closest <see cref="NPC"/>'s position.
    /// </summary>
    private void SetHoverPositionToNPC() {
      var player = Main.player[projectile.owner];
      Vector2 target = Main.npc[targetIDs[0]].position;
      Vector2 offset = player.Center - target;

      // Cannot reach targeted NPC
      float dist = offset.LengthSquared();
      if (dist > maxTargetDist * maxTargetDist) {
        if (targetIDs.Count == 1 || player.HasMinionAttackTargetNPC) {
          SetHoverPositionToIdle();
          return;
        }
        target = Main.npc[targetIDs[1]].position;
        offset = player.Center - target;
        dist = offset.LengthSquared();
        // Cannot reach closest NPC
        if (dist > maxTargetDist * maxTargetDist) {
          SetHoverPositionToIdle();
          return;
        }
      }
      baseHoverPosition =
        dist + MinDistFromNPC > maxDistFromPlayer
          ? player.Center - offset.Normalized() * maxDistFromPlayer
          : target + offset.Normalized() * MinDistFromNPC;
      UpdateHoverPosition();
    }

    /// <summary>
    /// Calls <see cref="IncrementHoverPositionIdx"/> if close, and calls either <see cref="SetHoverPositionToIdle"/> or <see cref="SetHoverPositionToNPC"/> based on condition.
    /// </summary>
    private void UpdateTargetsPos() {
      if ((projectile.position - hoverPosition).Length() < TriggerTargetMove) {
        IncrementHoverPositionIdx();
      }

      if (targetIDs.Count == 0 || Main.npc[targetIDs[0]].active == false) {
        SetHoverPositionToIdle();
      }
      else {
        SetHoverPositionToNPC();
      }
    }

    /// <summary>
    /// Creates one Spirit Flame projectile that targets <paramref name="npc"/> or is fired randomly.
    /// </summary>
    /// <param name="npc">NPC to target. If null, fires at the air randomly</param>
    private void Fire(NPC npc) {
      Vector2 shootVel;
      float rotation;
      if (npc != null) {
        // Fire at enemy NPC
        shootVel = npc.position - projectile.Center;
        rotation = Main.rand.Next(-randDegrees, randDegrees) / 180f * (float)Math.PI;
      }
      else {
        // Fire at air
        shootVel = new Vector2(Main.rand.Next(-12, 12), Main.rand.Next(24, 48)).Normalized();
        rotation = (float)(Main.rand.Next(-180, 180) / 180f * Math.PI);
      }
      if (shootVel == Vector2.Zero) {
        shootVel = Vector2.UnitY;
      }
      shootVel = Utils.RotatedBy(shootVel * ShootSpeed, rotation);

      int dmg = (int)(projectile.damage * Main.player[projectile.owner].minionDamage *
        (!AutoFire ? ManualShootDamageMultiplier : 1));

      Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, shootVel, spiritFlameType, dmg, projectile.knockBack, Main.myPlayer, 0, 0)];
      projectile.velocity += shootVel.SafeNormalize(Vector2.Zero) * -0.2f;
      proj.netUpdate = true;
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
        proj.timeLeft = 30;
      }
    }

    internal override void CheckActive() {
      var player = Main.player[projectile.owner];
      var oPlayer = player.GetModPlayer<OriPlayer>();
      if (player.dead || !player.active) {
        oPlayer.RemoveSeinBuffs();
      }
      else if (projectile.type == oPlayer.SeinMinionType && projectile.whoAmI == oPlayer.SeinMinionID && player.HasBuff(BuffType)) {
        projectile.timeLeft = 2;
      }
    }

    internal override void Behavior() {
      var player = Main.player[projectile.owner];
      SeinMovement();
      UpdateTargetsPos();
      Lighting.AddLight(projectile.Center, color.ToVector3() * lightStrength);

      if (player.whoAmI != Main.myPlayer) {
        return;
      }

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
          if (dist < maxTargetThroughWallDist || dist < maxTargetDist && inSight(npc)) {
            // Worms...
            if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
              wormIDs.Add((byte)npc.ai[3]);
            }
            hasTarget = true;
            mainTargetNPC = npc;
          }
        }
      }

      // Set target based on different enemies, if they can be hit
      if (!hasTarget || maxTargets < 1) {
        for (int i = 0; i < Main.maxNPCs; i++) {
          NPC npc = Main.npc[i];
          if (npc.CanBeChasedBy()) {
            float dist = Vector2.Distance(player.Center, npc.Center);
            if (dist < maxTargetThroughWallDist || dist < maxTargetDist && inSight(npc)) {
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
      if (newTargetIDs.Count != targetIDs.Count || targetIDs.Except(newTargetIDs).Count() != 0) {
        replaceList = true;
      }
      else {
        // More expensive check, compare position of each NPC
        float dist = 0;
        for (int t = 0, len = targetIDs.Count; t < len; t++) {
          float npcDist = (player.Center - Main.npc[targetIDs[t]].position).LengthSquared();
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
          newTargetIDs.Sort(SortByDistanceClosest);
        }
        targetIDs.Clear();
        if (mainTargetNPC?.active ?? false) {
          targetIDs.Add((byte)mainTargetNPC.whoAmI);
          targetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, maxTargets - 1)));
        }
        else {
          targetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, maxTargets)));
        }
      }
      #endregion

      #region Cooldown
      float minCooldown = this.minCooldown * (AutoFire ? 1.5f : 1);
      float shortCooldown = this.shortCooldown * (AutoFire ? 1.5f : 1);
      float longCooldown = this.longCooldown * (AutoFire ? 2f : 1);
      if (Cooldown > 0) {
        Cooldown++;
        if (Cooldown > longCooldown) {
          Cooldown = 0;
          currentShotsFired = 1;
        }
      }
      #endregion

      #region Firing
      // Spirit Flame
      bool attemptFire;
      if (AutoFire) {
        attemptFire = hasTarget;
      }
      else {
        attemptFire = PlayerInput.Triggers.JustPressed.MouseLeft && !Main.LocalPlayer.mouseInterface;
      }

      if (attemptFire && (Cooldown == 0 || Cooldown > minCooldown && currentShotsFired < maxShotsPerBurst)) {
        if (Cooldown > shortCooldown) {
          currentShotsFired = 1;
        }
        else {
          currentShotsFired++;
        }
        Cooldown = 1;

        PlaySpiritFlameSound("Throw" + spiritFlameSound + rand.NextNoRepeat(3), 0.6f);

        if (!hasTarget) {
          // Fire at air - nothing to target
          for (int i = 0; i < shotsToPrimaryTarget; i++) {
            Fire(null);
          }
          return;
        }

        int usedShots = 0;
        int loopCount = 0;
        while (loopCount < shotsToPrimaryTarget) {
          for (int t = 0; t < targetIDs.Count; t++) {
            bool isPrimary = t == 0;
            int shots = isPrimary ? shotsToPrimaryTarget : shotsToTarget;
            if (loopCount < shots) {
              Fire(Main.npc[targetIDs[t]]);
              if (++usedShots >= maxShotsPerVolley) {
                break;
              }
            }
          }
          loopCount++;
        }
        projectile.netUpdate = true;
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
        var color = this.color;
        color.A = 255;
        if (color == Color.Black) {
          color = Color.White;
        }

        color.A = (byte)(i == 0 ? 255 : i == 1 ? 200 : 175);
        var sourceRect = new Rectangle(0, i * tex.Height / 3, tex.Width, tex.Width);
        spriteBatch.Draw(tex, pos, sourceRect, color, projectile.rotation, orig, projectile.scale, SpriteEffects.None, 0f);
      }
    }

    private static void Unload() {
      _hoverPositions = null;
    }
  }
}