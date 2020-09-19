using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  /// <summary>
  /// Minion for the Ori character Sein.
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
    /// Type used for <see cref="Sein"/>. Values are indices to <see cref="SeinData.All"/>.
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

      var type = SeinType;
      data = SeinData.All[type - 1];

      if (MaxDistFromPlayer < data.targetMaxDist * 0.8f) {
        MaxDistFromPlayer = data.targetMaxDist * 0.8f;
      }

      projectile.width = data.seinWidth;
      projectile.height = data.seinHeight;

      spiritFlameType = mod.ProjectileType("SpiritFlame" + type);
      spiritFlameSound = type <= 2 ? "" : type <= 4 ? "LevelB" : type <= 6 ? "LevelC" : type <= 8 ? "LevelD" : "";
    }

    private SeinData data;

    public Player player => _player ?? (_player = Main.player[projectile.owner]);

    /// <summary>
    /// Whether the AI should automatically fire projectiles or not.
    /// </summary>
    /// <returns><see langword="true"/> if the held item is not the same type that spawned this projectile.</returns>
    private bool AutoFire => player.HeldItem.shoot != projectile.type;

    /// <summary>
    /// Current Cooldown of Spirit Flame.
    /// </summary>
    /// <remarks>Gets and sets to <see cref="Projectile.ai"/>[0].</remarks>
    private int Cooldown {
      get => (int)projectile.ai[0];
      set => projectile.ai[0] = value;
    }

    private float CooldownMin => data.cooldownMin * (AutoFire ? 1.5f : 1);
    private float CooldownShort => data.cooldownShort * (AutoFire ? 1.5f : 1);
    private float CooldownLong => data.cooldownLong * (AutoFire ? 2f : 1);

    /// <summary>
    /// ID of <see cref="SpiritFlame"/> to shoot. Assigned in <see cref="SetDefaults"/>
    /// </summary>
    private int spiritFlameType;

    /// <summary>
    /// Sound that plays when firing. Assigned in <see cref="SetDefaults"/>
    /// </summary>
    private string spiritFlameSound;

    private readonly RandomChar rand = new RandomChar();

    /// <summary>
    /// Damage multiplier for when the player manually fires Spirit Flame.
    /// </summary>
    private static float ManualShootDamageMultiplier => 1.4f;


    /// <summary>
    /// Positions that the minion idly moves towards. Positions are relative to <see cref="baseGoalPosition"/>
    /// </summary>
    private static Vector2[] GoalPositions => _goalPositions ?? (_goalPositions = new Vector2[] {
      new Vector2(-32, 12),
      new Vector2(32, -12),
      new Vector2(-32, -12),
      new Vector2(32, 12),
      new Vector2(-32, -12),
      new Vector2(32, -12),
    });

    /// <summary>
    /// General position that Sein hovers around. This is a general location, and not precisely where the minion moves.
    /// </summary>
    private Vector2 baseGoalPosition;

    /// <summary>
    /// Current index of <see cref="GoalPositions"/> that is active.
    /// <para>This value automatically wraps to be in-bounds of <see cref="GoalPositions"/>.</para>
    /// </summary>
    private int goalPositionIdx {
      get => _hPI;
      set => _hPI = value % GoalPositions.Length;
    }

    /// <summary>
    /// Exact position that this minion is moving towards. This is set to be around <see cref="baseGoalPosition"/>.
    /// </summary>
    public Vector2 GoalPosition => baseGoalPosition + GoalPositions[goalPositionIdx];


    /// <summary>
    /// Targeted NPC using the minion targeting feature.
    /// </summary>
    private NPC mainTargetNPC;

    /// <summary>
    /// Distance this projectile is from the goal position to cycle the goal position.
    /// </summary>
    private static float TriggerGoalMove => 3f;
    private static float TriggerGoalMoveSquared => TriggerGoalMove * TriggerGoalMove;

    /// <summary>
    /// The closest <see cref="baseGoalPosition"/> must be to an NPC that it is moving towards.
    /// </summary>
    private static float MinDistFromNPC => 64f;
    private static float MinDistFromNpcSquared => MinDistFromNPC * MinDistFromNPC;

    /// <summary>
    /// The furthest <see cref="baseGoalPosition"/> can be from the player. May be modified in <see cref="SetDefaults"/>.
    /// </summary>
    private float MaxDistFromPlayer = 200f;
    private float MaxDistFromPlayerSquared => MaxDistFromPlayer * MaxDistFromPlayer;

    /// <summary>
    /// List of NPCs last targeted by the minion.
    /// </summary>
    private readonly List<byte> targetIDs = new List<byte>();

    /// <summary>
    /// Current number of shots fired in rapid succession. Used to incur <see cref="SeinData.cooldownLong"/>.
    /// </summary>
    private int currentShotsFired = 1;

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(float x, float y) => PlayerSpace(new Vector2(x, y));

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(Vector2 coords = default) => player.Center + coords;

    /// <summary>
    /// Plays a Spirit Flame sound effect with the given <paramref name="path"/> and <paramref name="volume"/>.
    /// </summary>
    /// <param name="path">Path of the sound effect to play. Relative to the Spirit Flame folder.</param>
    /// <param name="volume">Volume to play the sound at.</param>
    private void PlaySpiritFlameSound(string path, float volume) =>
      Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/Ori/SpiritFlame/" + path).WithVolume(volume), projectile.Center);

    /// <summary>
    /// Ensures that the projectile position and velocity are valid.
    /// </summary>
    private void VerifyNoNANs() {
      if (projectile.position.HasNaNs() || (projectile.position - player.Center).Length() > 1000) {
        projectile.position = player.Center;
      }
      if (projectile.velocity.HasNaNs()) {
        projectile.velocity = new Vector2(0, -1);
      }
    }

    /// <summary>
    /// This is the somewhat subtle swaying about Sein does at any given time in Blind Forest.
    /// </summary>
    private void SeinMovement() {
      Vector2 goalOffset = GoalPosition - projectile.position;
      Vector2 goalVelocity = goalOffset * 0.05f;
      float goalSpeed = goalVelocity.Length();
      float speed = projectile.velocity.Length();

      // Limit acceleration
      float newSpeed = MathHelper.Clamp(goalSpeed, speed * 0.95f, speed * 1.1f);
      newSpeed = Math.Min(newSpeed, 10f);
      projectile.velocity = goalVelocity.Normalized() * newSpeed;
    }

    /// <summary>
    /// Updates where Sein should move towards.
    /// </summary>
    private void UpdateGoalPosition() {
      if ((projectile.position - GoalPosition).LengthSquared() < TriggerGoalMoveSquared) {
        goalPositionIdx++;
      }

      if (targetIDs.Count == 0 || !Main.npc[targetIDs[0]].active ||
        (baseGoalPosition - player.Center).Length() > 1000) {
        SetGoalToIdle();
      }
      else {
        SetGoalToNPC();
      }
    }

    /// <summary>
    /// Moves the goal position to above the player's head.
    /// </summary>
    private void SetGoalToIdle() => baseGoalPosition = PlayerSpace(0, -56);

    /// <summary>
    /// Moves the goal position to the target NPC or nearest NPC.
    /// </summary>
    private void SetGoalToNPC() {
      Vector2 target = Main.npc[targetIDs[0]].Top;
      Vector2 offset = player.Center - target;

      float distanceSquared = offset.LengthSquared();
      var maxDistanceSquared = data.targetMaxDist * data.targetMaxDist;

      //Cannot reach target NPC
      if (distanceSquared > maxDistanceSquared) {
        if (targetIDs.Count == 1 || player.HasMinionAttackTargetNPC) {
          SetGoalToIdle();
          return;
        }
        target = Main.npc[targetIDs[1]].Top;
        offset = player.Center - target;
        distanceSquared = offset.LengthSquared();
        // Cannot reach closest NPC
        if (distanceSquared > maxDistanceSquared) {
          SetGoalToIdle();
          return;
        }
      }

      bool inRange = distanceSquared + MinDistFromNpcSquared > MaxDistFromPlayerSquared;
      baseGoalPosition = inRange
          ? player.Center - offset.Normalized() * MaxDistFromPlayer
          : target + offset.Normalized() * MinDistFromNPC;
    }

    /// <summary>
    /// Updates the list of <see cref="NPC"/>s that <see cref="Sein"/> can attack.
    /// </summary>
    /// <returns><see langword="true"/> if there are any <see cref="NPC"/>s that <see cref="Sein"/> can attack; otherwise, <see langword="false"/></returns>
    private bool UpdateTargets() {
      bool inSight(NPC npc) => Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);
      int SortByDistanceClosest(byte id1, byte id2) {
        var npc1 = Main.npc[id1];
        var npc2 = Main.npc[id2];
        if (!(mainTargetNPC is null)) {
          if (npc1.whoAmI == mainTargetNPC.whoAmI) return -1;
          if (npc2.whoAmI == mainTargetNPC.whoAmI) return 1;
        }

        Vector2 playerPos = player.Center;
        float length1 = (npc1.position - playerPos).LengthSquared();
        float length2 = (npc2.position - playerPos).LengthSquared();
        return length1.CompareTo(length2);
      }

      var newTargetIDs = new List<byte>();
      var wormIDs = new List<byte>();

      // If player specifies target, add that target to selection
      mainTargetNPC = null;
      if (player.HasMinionAttackTargetNPC) {
        NPC npc = Main.npc[player.MinionAttackTargetNPC];
        if (npc.CanBeChasedBy()) {
          float dist = Vector2.Distance(player.Center, npc.Center);
          if (dist < data.targetThroughWallDist || dist < data.targetMaxDist && inSight(npc)) {
            // Worms...
            if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
              wormIDs.Add((byte)npc.ai[3]);
            }
            mainTargetNPC = npc;
            newTargetIDs.Add((byte)npc.whoAmI);
          }
        }
      }

      // Set target based on different enemies, if they can be hit
      for (int i = 0; i < Main.npc.Length; i++) {
        NPC npc = Main.npc[i];
        if (npc.CanBeChasedBy()) {
          float dist = Vector2.DistanceSquared(player.Center, npc.Center);
          if (dist < data.targetThroughWallDistSquared || dist < data.targetMaxDistSquared && inSight(npc)) {
            // Worms...
            if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
              if (wormIDs.Contains((byte)npc.ai[3])) {
                continue;
              }

              wormIDs.Add((byte)npc.ai[3]);
            }
            newTargetIDs.Add((byte)npc.whoAmI);
          }
        }
      }

      if (newTargetIDs.Count > 1) {
        newTargetIDs.Sort(SortByDistanceClosest);
      }
      targetIDs.Clear();
      targetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, data.targets)));

      return !(mainTargetNPC is null) || newTargetIDs.Count > 0;
    }

    /// <summary>
    /// Updates the cooldown.
    /// </summary>
    private void TickCooldown() {
      if (Cooldown > 0) {
        Cooldown++;
        if (Cooldown > CooldownLong) {
          Cooldown = 0;
          currentShotsFired = 1;
        }
      }
    }

    /// <summary>
    /// Fires a burst of Spirit Flame projectiles.
    /// </summary>
    /// <param name="hasTarget"></param>
    private void Attack(bool hasTarget) {
      if (Cooldown > CooldownShort) {
        currentShotsFired = 1;
      }
      else {
        currentShotsFired++;
      }
      Cooldown = 1;

      PlaySpiritFlameSound("Throw" + spiritFlameSound + rand.NextNoRepeat(3), 0.6f);

      if (!hasTarget) {
        // Fire at air - nothing to target
        for (int i = 0; i < data.shotsToPrimaryTarget; i++) {
          Shoot(null);
        }
        return;
      }

      int usedShots = 0;
      int loopCount = 0;
      while (loopCount < data.shotsToPrimaryTarget) {
        for (int t = 0; t < targetIDs.Count; t++) {
          bool isPrimary = t == 0;
          int shots = isPrimary ? data.shotsToPrimaryTarget : data.shotsPerTarget;
          if (loopCount < shots) {
            Shoot(Main.npc[targetIDs[t]]);
            if (++usedShots >= data.maxShotsAtOnce) {
              break;
            }
          }
        }
        loopCount++;
      }
      projectile.netUpdate = true;
    }

    /// <summary>
    /// Creates one Spirit Flame projectile that targets <paramref name="npc"/> or is fired randomly.
    /// </summary>
    /// <param name="npc">NPC to target, -or- <see langword="null"/> to fires at the air randomly.</param>
    private void Shoot(NPC npc) {
      Vector2 shootVel;
      float rotation;
      if (npc is null) {
        // Fire at air
        shootVel = new Vector2(Main.rand.Next(-12, 12), Main.rand.Next(24, 48)).Normalized();
        rotation = (float)(Main.rand.Next(-180, 180) / 180f * Math.PI);
      }
      else {
        // Fire at enemy NPC
        shootVel = npc.position - projectile.Center;
        rotation = Main.rand.Next(-data.randDegrees, data.randDegrees) / 180f * (float)Math.PI;
      }
      if (shootVel == Vector2.Zero) {
        shootVel = Vector2.UnitY;
      }
      shootVel = Utils.RotatedBy(shootVel * data.projectileSpeedStart, rotation);
      projectile.velocity += shootVel.Normalized() * -0.2f;

      int dmg = (int)(projectile.damage * player.minionDamage *
        (!AutoFire ? ManualShootDamageMultiplier : 1));


      Projectile spiritFlame = Projectile.NewProjectileDirect(projectile.Center, shootVel, spiritFlameType, dmg, projectile.knockBack, projectile.owner, 0, 0);
      if (npc is null) {
        var pos = Utils.RotatedBy(new Vector2(projectile.position.X, projectile.position.Y + Main.rand.Next(8, 48)), Main.rand.NextFloat((float)Math.PI * 2));
        spiritFlame.ai[0] = pos.X != 0 ? pos.X : float.Epsilon;
        spiritFlame.ai[1] = pos.Y;
        spiritFlame.timeLeft = 20;
      }
      else {
        spiritFlame.ai[0] = 0;
        spiritFlame.ai[1] = npc.whoAmI;
        spiritFlame.timeLeft = 70;
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
      SeinMovement();
      UpdateGoalPosition();
      TickCooldown();
      VerifyNoNANs();

      Lighting.AddLight(projectile.Center, data.color.ToVector3() * data.lightStrength);
      if (player.whoAmI != Main.myPlayer) {
        return;
      }

      bool hasTarget = UpdateTargets();
      bool attemptFire = AutoFire ? hasTarget : player.controlUseItem && !Main.LocalPlayer.mouseInterface;

      if (attemptFire && (Cooldown == 0 || Cooldown > CooldownMin && currentShotsFired < data.bursts)) {
        Attack(hasTarget);
      }
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
        var color = data.color;
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
      _goalPositions = null;
    }

    private static Vector2[] _goalPositions;
    private Player _player;
    private int _hPI;
  }
}