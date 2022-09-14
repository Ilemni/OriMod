using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ReLogic.Utilities;

namespace OriMod.Projectiles.Minions {
  /// <summary>
  /// Minion for the Ori character Sein.
  /// </summary>
  public abstract class Sein : Minion {
    static Sein() => OriMod.OnUnload += _Unload;
    public sealed override string Texture => "OriMod/Projectiles/Minions/Sein";

    public sealed override bool? CanCutTiles() => false;

    public sealed override void SetStaticDefaults() {
      Main.projFrames[Projectile.type] = 3;
      Main.projPet[Projectile.type] = true;
      ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
      ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
      ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; //This is necessary for right-click targeting
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
      Projectile.netImportant = true;
      Projectile.minion = true;
      Projectile.minionSlots = -0.001f;
      Projectile.penetrate = -1;
      Projectile.timeLeft = 18000;
      Projectile.tileCollide = false;
      Projectile.ignoreWater = true;

      byte type = SeinType;
      _data = SeinData.All[type - 1];

      Projectile.width = _data.seinWidth;
      Projectile.height = _data.seinHeight;

      _spiritFlameType = Mod.Find<ModProjectile>("SpiritFlame" + type).Type;
      _spiritFlameSound = type <= 2 ? "" : type <= 4 ? "LevelB" : type <= 6 ? "LevelC" : type <= 8 ? "LevelD" : "";
    }

    private SeinData _data;

    private Player Player => _player ?? (_player = Main.player[Projectile.owner]);

    /// <summary>
    /// Whether the AI should automatically fire projectiles or not.
    /// </summary>
    /// <returns><see langword="true"/> if the held item is not the same type that spawned this projectile.</returns>
    private bool AutoFire => Player.HeldItem.shoot != Projectile.type;

    /// <summary>
    /// Current Cooldown of Spirit Flame.
    /// </summary>
    /// <remarks>Gets and sets to <see cref="Projectile.ai"/>[0].</remarks>
    private int Cooldown {
      get => (int)Projectile.ai[0];
      set => Projectile.ai[0] = value;
    }

    private float CooldownMin => _data.cooldownMin * (AutoFire ? 1.5f : 1);
    private float CooldownShort => _data.cooldownShort * (AutoFire ? 1.5f : 1);
    private float CooldownLong => _data.cooldownLong * (AutoFire ? 2f : 1);

    /// <summary>
    /// ID of <see cref="SpiritFlame"/> to shoot. Assigned in <see cref="SetDefaults"/>
    /// </summary>
    private int _spiritFlameType;

    /// <summary>
    /// Sound that plays when firing. Assigned in <see cref="SetDefaults"/>
    /// </summary>
    private string _spiritFlameSound;

    private readonly RandomChar _rand = new RandomChar();

    /// <summary>
    /// Damage multiplier for when the player manually fires Spirit Flame.
    /// </summary>
    private static float ManualShootDamageMultiplier => 1.4f;


    /// <summary>
    /// Positions that the minion idly moves towards. Positions are relative to <see cref="_goalNpc"/> with a fixed offset, or the player if <see cref="_goalNpc"/> is <see langword="null"/>.
    /// </summary>
    private static Vector2[] GoalPositions => _goalPositions ?? (_goalPositions = new[] {
      new Vector2(-32, 12),
      new Vector2(32, -12),
      new Vector2(-32, -12),
      new Vector2(32, 12),
      new Vector2(-32, -12),
      new Vector2(32, -12),
    });

    private NPC _goalNpc;

    /// <summary>
    /// Current index of <see cref="GoalPositions"/> that is active.
    /// <para>This value automatically wraps to be in-bounds of <see cref="GoalPositions"/>.</para>
    /// </summary>
    private int GoalPositionIdx {
      get => _hPi;
      set => _hPi = value % GoalPositions.Length;
    }

    /// <summary>
    /// Exact position that this minion is moving towards. This is set to be around <see cref="_goalNpc"/>, or the player if <see cref="_goalNpc"/> is <see langword="null"/>.
    /// </summary>
    private Vector2 GoalPosition {
      get {
        Vector2 result;
        if (_goalNpc is null) {
          result = PlayerSpace(0, -56) + GoalPositions[GoalPositionIdx];
        }
        else {
          result = _goalNpc.Top;
          result.Y -= 56;
        }
        return result;
      }
    }


    /// <summary>
    /// Targeted NPC using the minion targeting feature.
    /// </summary>
    private NPC _mainTargetNpc;

    /// <summary>
    /// Distance this projectile is from the goal position to cycle the goal position.
    /// </summary>
    private static float TriggerGoalMove => 3f;
    private static float TriggerGoalMoveSquared => TriggerGoalMove * TriggerGoalMove;

    private int _timeSinceGoalChanged;

    /// <summary>
    /// List of NPCs last targeted by the minion.
    /// </summary>
    private readonly List<byte> _targetIDs = new List<byte>();

    /// <summary>
    /// Current number of shots fired in rapid succession. Used to incur <see cref="SeinData.cooldownLong"/>.
    /// </summary>
    private int _currentShotsFired = 1;

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(float x, float y) => PlayerSpace(new Vector2(x, y));

    /// <summary>
    /// Coordinates relative to the player's center.
    /// </summary>
    private Vector2 PlayerSpace(Vector2 coords = default) {
      Vector2 result = Player.Center + coords * Player.gravDir;
      if (Player.gravDir < 0) {
        result.Y -= 20;
      }
      return result;
    }

    /// <summary>
    /// Plays a Spirit Flame sound effect with the given <paramref name="path"/> and <paramref name="volume"/>.
    /// </summary>
    /// <param name="path">Path of the sound effect to play. Relative to the Spirit Flame folder.</param>
    /// <param name="volume">Volume to play the sound at.</param>
    private SlotId PlaySpiritFlameSound(string path, float volume, out SoundStyle style) =>
      SoundWrapper.PlaySound(Projectile.Center, "Ori/SpiritFlame/" + path, out style, volume);

    /// <summary>
    /// Ensures that the projectile position and velocity are valid.
    /// </summary>
    private void VerifyNoNaNs() {
      if (Projectile.position.HasNaNs() || (Projectile.position - Player.Center).Length() > 1000) {
        Projectile.position = Player.Center;
      }
      if (Projectile.velocity.HasNaNs()) {
        Projectile.velocity = new Vector2(0, -1);
      }
    }


    /// <summary>
    /// This is the somewhat subtle swaying about Sein does at any given time in Blind Forest.
    /// </summary>
    private void SeinMovement() {
      Vector2 goalOffset = GoalPosition - Projectile.position;
      Vector2 goalVelocity = goalOffset * 0.05f;
      float targetSpeed = (_goalNpc?.velocity ?? Player.velocity).Length();
      if (targetSpeed > 8) {
        goalVelocity *= targetSpeed * 0.125f;
      }

      float goalSpeed = goalVelocity.Length();
      float speed = Projectile.velocity.Length();

      // Limit acceleration
      float newSpeed = MathHelper.Clamp(goalSpeed, speed * 0.95f - 0.05f, speed * 1.1f + 0.05f);
      newSpeed = Math.Min(newSpeed, 16f);
      Projectile.velocity = goalVelocity.Normalized() * newSpeed;
    }

    /// <summary>
    /// Updates where Sein should move towards.
    /// </summary>
    private void UpdateGoalPosition() {
      _timeSinceGoalChanged++;
      if (_goalNpc is null && (Projectile.position - GoalPosition).LengthSquared() < TriggerGoalMoveSquared) {
        GoalPositionIdx++;
      }

      if (!(_goalNpc is null) && !_goalNpc.active) {
        _goalNpc = null;
      }
      if (_targetIDs.Count == 0 || !Main.npc[_targetIDs[0]].active) {
        _goalNpc = null;
      }
      else {
        if (_timeSinceGoalChanged > 20) {
          SetGoalToNpc();
        }
      }
    }

    /// <summary>
    /// Moves the goal position to the target NPC or nearest NPC.
    /// </summary>
    private void SetGoalToNpc() {
      NPC target = Main.npc[_targetIDs[0]];

      //Cannot reach target NPC
      if ((Player.Center - target.Center).LengthSquared() > _data.TargetMaxDistSquared) {
        if (_targetIDs.Count != 1) {
          target = Main.npc[_targetIDs[1]];
          // Cannot reach closest NPC
          if ((Player.Center - target.Center).LengthSquared() > _data.TargetMaxDistSquared) {
            _goalNpc = null;
            return;
          }
        }
        else {
          _goalNpc = null;
          return;
        }
      }

      if (target == _goalNpc) return;
      _goalNpc = target;
      _timeSinceGoalChanged = 0;
    }

    /// <summary>
    /// Updates the list of <see cref="NPC"/>s that <see cref="Sein"/> can attack.
    /// </summary>
    /// <returns><see langword="true"/> if there are any <see cref="NPC"/>s that <see cref="Sein"/> can attack; otherwise, <see langword="false"/></returns>
    private bool UpdateTargets() {
      bool InSight(NPC npc) => Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
      int SortByDistanceClosest(byte id1, byte id2) {
        NPC npc1 = Main.npc[id1];
        NPC npc2 = Main.npc[id2];
        if (!(_mainTargetNpc is null)) {
          if (npc1.whoAmI == _mainTargetNpc.whoAmI) return -1;
          if (npc2.whoAmI == _mainTargetNpc.whoAmI) return 1;
        }

        Vector2 playerPos = Player.Center;
        float length1 = (npc1.position - playerPos).LengthSquared();
        float length2 = (npc2.position - playerPos).LengthSquared();
        return length1.CompareTo(length2);
      }

      var newTargetIDs = new List<byte>();
      var wormIDs = new List<byte>();

      // If player specifies target, add that target to selection
      _mainTargetNpc = null;
      if (Player.HasMinionAttackTargetNPC) {
        NPC npc = Main.npc[Player.MinionAttackTargetNPC];
        if (npc.CanBeChasedBy()) {
          float dist = Vector2.Distance(Player.Center, npc.Center);
          if (dist < _data.targetThroughWallDist || dist < _data.targetMaxDist && InSight(npc)) {
            // Worms...
            if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
              wormIDs.Add((byte)npc.ai[3]);
            }
            _mainTargetNpc = npc;
            newTargetIDs.Add((byte)npc.whoAmI);
          }
        }
      }

      // Set target based on different enemies, if they can be hit
      foreach (NPC npc in Main.npc) {
        if (!npc.CanBeChasedBy()) continue;
        float dist = Vector2.DistanceSquared(Player.Center, npc.Center);
        if (!(dist < _data.TargetThroughWallDistSquared) && 
           (!(dist < _data.TargetMaxDistSquared) || !InSight(npc))) continue;
        // Worms...
        if (npc.aiStyle == 6 || npc.aiStyle == 37) { // TODO: Sort targeted worm piece by closest rather than whoAmI
          if (wormIDs.Contains((byte)npc.ai[3])) {
            continue;
          }

          wormIDs.Add((byte)npc.ai[3]);
        }
        newTargetIDs.Add((byte)npc.whoAmI);
      }

      if (newTargetIDs.Count > 1) {
        newTargetIDs.Sort(SortByDistanceClosest);
      }
      _targetIDs.Clear();
      _targetIDs.AddRange(newTargetIDs.GetRange(0, Math.Min(newTargetIDs.Count, _data.targets)));

      return !(_mainTargetNpc is null) || newTargetIDs.Count > 0;
    }

    /// <summary>
    /// Updates the cooldown.
    /// </summary>
    private void TickCooldown() {
      if (Cooldown <= 0) return;
      Cooldown++;
      if (Cooldown <= CooldownLong) return;
      Cooldown = 0;
      _currentShotsFired = 0;
    }

    /// <summary>
    /// Fires a burst of Spirit Flame projectiles.
    /// </summary>
    /// <param name="hasTarget"></param>
    private void Attack(bool hasTarget) {
      PlaySpiritFlameSound("Throw" + _spiritFlameSound + _rand.NextNoRepeat(3), 0.6f, out SoundStyle _);

      if (!hasTarget) {
        // Fire at air - nothing to target
        for (int i = 0; i < _data.shotsToPrimaryTarget; i++) {
          Shoot(null);
        }
        return;
      }

      int usedShots = 0;
      int loopCount = 0;
      while (loopCount < _data.shotsToPrimaryTarget) {
        for (int t = 0; t < _targetIDs.Count; t++) {
          bool isPrimary = t == 0;
          int shots = isPrimary ? _data.shotsToPrimaryTarget : _data.shotsPerTarget;
          if (loopCount >= shots) continue;
          Shoot(Main.npc[_targetIDs[t]]);
          if (++usedShots >= _data.maxShotsAtOnce) {
            break;
          }
        }
        loopCount++;
      }
      Projectile.netUpdate = true;
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
        shootVel = npc.position - Projectile.Center;
        rotation = Main.rand.Next(-_data.randDegrees, _data.randDegrees) / 180f * (float)Math.PI;
      }
      if (shootVel == Vector2.Zero) {
        shootVel = Vector2.UnitY;
      }
      shootVel = (shootVel * _data.projectileSpeedStart).RotatedBy(rotation);
      Projectile.velocity += shootVel.Normalized() * -0.2f;

      var _summon_damage = Player.GetDamage<SummonDamageClass>();

      float _summon_damage_mul = _summon_damage.Additive * _summon_damage.Multiplicative;

      int dmg = (int)(((Projectile.damage+_summon_damage.Base) * _summon_damage_mul + _summon_damage.Flat) *
        (!AutoFire ? ManualShootDamageMultiplier : 1));


      Projectile spiritFlame = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, shootVel, _spiritFlameType, dmg, Projectile.knockBack, Projectile.owner);
      spiritFlame.originalDamage = _data.damage;
      spiritFlame.netUpdate = true;
      Projectile.netUpdate = true;
      if (npc is null) {
        Vector2 pos = new Vector2(Projectile.position.X, Projectile.position.Y + Main.rand.Next(8, 48)).RotatedBy(Main.rand.NextFloat((float)Math.PI * 2));
        spiritFlame.ai[0] = pos.X != 0 ? pos.X : float.Epsilon;
        spiritFlame.ai[1] = pos.Y;
        spiritFlame.timeLeft = 20;
      }
      else {
        spiritFlame.ai[0] = 0;
        spiritFlame.ai[1] = npc.whoAmI;
        spiritFlame.timeLeft = 300;
      }
    }

    protected override void CheckActive() {
      Player player = Main.player[Projectile.owner];
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      if (player.dead || !player.active) {
        oPlayer.RemoveSeinBuffs();
      }
      else if (Projectile.type == oPlayer.SeinMinionType && Projectile.whoAmI == oPlayer.SeinMinionId && player.HasBuff(BuffType)) {
        Projectile.timeLeft = 2;
      }
    }

    protected override void Behavior() {
      SeinMovement();
      UpdateGoalPosition();
      TickCooldown();
      VerifyNoNaNs();

      Lighting.AddLight(Projectile.Center, _data.color.ToVector3() * _data.lightStrength);
      if (Player.whoAmI != Main.myPlayer) {
        return;
      }

      OriPlayer oPlayer = Player.GetModPlayer<OriPlayer>();
      bool hasTarget = UpdateTargets();
      bool attemptFire = AutoFire ? hasTarget : oPlayer.input.leftClick.JustPressed && !Player.mouseInterface;

      if (!attemptFire || Cooldown != 0 && (!(Cooldown > CooldownMin) || _currentShotsFired >= _data.bursts)) return;
      if (Cooldown > CooldownShort) {
        _currentShotsFired = 0;
      }
      else {
        _currentShotsFired++;
      }
      Cooldown = 1;
      Attack(hasTarget);
    }

    // ReSharper disable RedundantAssignment
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 centerFrac) {
      // ReSharper restore RedundantAssignment
      fallThrough = true;
      width = 4;
      height = 4;
      centerFrac = new Vector2(0.5f, 0.5f);
      return false;
    }

    public override void PostDraw(Color lightColor) {
      Vector2 pos = Projectile.BottomRight - Main.screenPosition;
      Texture2D tex = OriTextures.Instance.sein.texture;
      Vector2 orig = new Vector2(tex.Width, tex.Width) * 0.5f;
      for (int i = 0; i < 3; i++) {
        Color color = _data.color;
        color.A = 255;
        if (color == Color.Black) {
          color = Color.White;
        }

        color.A = (byte)(i == 0 ? 255 : i == 1 ? 200 : 175);
        Rectangle sourceRect = new Rectangle(0, i * tex.Height / 3, tex.Width, tex.Width);
        Main.EntitySpriteDraw(tex, pos, sourceRect, color, Projectile.rotation, orig, Projectile.scale, SpriteEffects.None, 0);
      }
    }

    private static void _Unload() {
      _goalPositions = null;
    }

    private static Vector2[] _goalPositions;
    private Player _player;
    private int _hPi;
  }
}