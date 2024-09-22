using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions;

/// <summary>
/// Minion for the Ori character Sein.
/// </summary>
public abstract class Sein(int type) : Minion {
  public sealed override string Texture => "OriMod/Projectiles/Minions/Sein";

  public sealed override bool? CanCutTiles() => false;

  public sealed override void SetStaticDefaults() {
    Main.projFrames[Projectile.type] = 3;
    Main.projPet[Projectile.type] = true;
    ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
    ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; //This is necessary for right-click targeting

    _glowTexture = ModContent.Request<Texture2D>("OriMod/Projectiles/Minions/Sein_Glow");
  }

  private static Asset<Texture2D> _glowTexture = null!; // SetStaticDefaults

  /// <summary>
  /// Type used for <see cref="Sein"/>. Values are indices to <see cref="SeinData.Get"/>.
  /// </summary>
  private int SeinType { get; } = type;

  public override void SetDefaults() {
    Projectile.netImportant = true;
    Projectile.penetrate = -1;
    Projectile.timeLeft = 18000;
    Projectile.tileCollide = false;
    Projectile.ignoreWater = true;
    Projectile.ContinuouslyUpdateDamageStats = true;
    Projectile.DamageType = DamageClass.Summon;
    Projectile.minion = true;

    Projectile.width = SeinData.SeinWidth;
    Projectile.height = SeinData.SeinHeight;

    SeinTypeInfo typeInfo = SeinData.GetSeinTypeInfo(SeinType);

    _buffType = typeInfo.Buff;
    _spiritFlameType = typeInfo.SpiritFlame;

    string suffix = SeinType switch {
      <= 2 => "",
      <= 4 => "LevelB",
      <= 6 => "LevelC",
      <= 8 => "LevelD",
      _ => ""
    };
    _spiritFlameSound = new SoundInfo("Ori/SpiritFlame/Throw" + suffix, 3, 1f);
  }

  private ref SeinData Data => ref SeinData.Get(SeinType);

  private Player Player => _player ??= Main.player[Projectile.owner];

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

  private float CooldownMin => Data.CooldownMin * (AutoFire ? 1.5f : 1);
  private float CooldownShort => Data.CooldownShort * (AutoFire ? 1.5f : 1);
  private float CooldownLong => Data.CooldownLong * (AutoFire ? 2f : 1);

  /// <summary>
  /// ID of <see cref="SpiritFlame"/> to shoot. Assigned in <see cref="SetDefaults"/>
  /// </summary>
  private int _spiritFlameType;

  private int _buffType;

  private SoundInfo _spiritFlameSound;

  /// <summary>
  /// Damage multiplier for when the player manually fires Spirit Flame.
  /// </summary>
  private static float ManualShootDamageMultiplier => 1.4f;


  /// <summary>
  /// Positions that the minion idly moves towards. Positions are relative to <see cref="_goalNpc"/> with a fixed offset, or the player if <see cref="_goalNpc"/> is <see langword="null"/>.
  /// </summary>
  private static readonly Vector2[] GoalPositions = [
    new Vector2(-32, 12),
    new Vector2(32, -12),
    new Vector2(-32, -12),
    new Vector2(32, 12),
    new Vector2(-32, -12),
    new Vector2(32, -12)
  ];

  private NPC? _goalNpc;

  [MemberNotNullWhen(true, nameof(_goalNpc))]
  private bool HasGoalNpc => _goalNpc is { active: true };

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
  private Vector2 GoalPosition => HasGoalNpc
    ? new Vector2(_goalNpc.Top.X, _goalNpc.position.Y - 56)
    : PlayerSpace(0, -56) + GoalPositions[GoalPositionIdx];


  /// <summary>
  /// Targeted NPC using the minion targeting feature.
  /// </summary>
  private NPC? _mainTargetNpc;

  /// <summary>
  /// Distance this projectile is from the goal position to cycle the goal position.
  /// </summary>
  private static float TriggerGoalMove => 3f;

  private static float TriggerGoalMoveSquared => TriggerGoalMove * TriggerGoalMove;

  private int _timeSinceGoalChanged;

  /// <summary>
  /// List of NPCs last targeted by the minion.
  /// </summary>
  private readonly List<ushort> _targetIDs = [];

  /// <summary>
  /// Current number of shots fired in rapid succession. Used to incur <see cref="SeinData.CooldownLong"/>.
  /// </summary>
  private int _currentShotsFired = 1;

  private float _lightStrength = 1f;

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
    float targetSpeed = (HasGoalNpc ? _goalNpc.velocity : Player.velocity).Length();
    if (targetSpeed > 8) {
      goalVelocity *= targetSpeed * 0.125f;
    }

    float goalSpeed = goalVelocity.Length();
    float speed = Projectile.velocity.Length();

    // Limit acceleration
    float newSpeed = MathHelper.Clamp(goalSpeed, speed * 0.95f - 0.05f, speed * 1.1f + 0.05f);
    newSpeed = Math.Min(newSpeed, 16f);
    Projectile.velocity = goalVelocity.SafeNormalize(default) * newSpeed;
  }

  /// <summary>
  /// Updates where Sein should move towards.
  /// </summary>
  private void UpdateGoalPosition() {
    _timeSinceGoalChanged++;
    if (!HasGoalNpc && (Projectile.position - GoalPosition).LengthSquared() < TriggerGoalMoveSquared) {
      GoalPositionIdx++;
    }

    if (_goalNpc is not { active: true } || _targetIDs.Count == 0 || !Main.npc[_targetIDs[0]].active) {
      _goalNpc = null;
    }

    if (HasGoalNpc && _timeSinceGoalChanged > 20) {
      SetGoalToNpc();
    }
  }

  /// <summary>
  /// Moves the goal position to the target NPC or nearest NPC.
  /// </summary>
  private void SetGoalToNpc() {
    NPC target = Main.npc[_targetIDs[0]];

    float targetMaxDistSquared = Data.TargetMaxDistSquared;
    if ((Player.Center - target.Center).LengthSquared() > targetMaxDistSquared) {
      if (_targetIDs.Count == 1) {
        // The only available NPC is too far away
        _goalNpc = null;
        return;
      }

      target = Main.npc[_targetIDs[1]];
      if ((Player.Center - target.Center).LengthSquared() > targetMaxDistSquared) {
        // The closest NPC is too far away
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
    Span<TargetInfo> newTargetIDs = stackalloc TargetInfo[255]; // Possible candidates
    Span<ushort> wormIDs = stackalloc ushort[255]; // Exclude worm body segments
    int newTargetCount = 0;
    int wormCount = 0;

    _mainTargetNpc = null;
    ref SeinData data = ref Data;

    // If player specifies target, add that target to selection
    if (Player.HasMinionAttackTargetNPC) {
      NPC npc = Main.npc[Player.MinionAttackTargetNPC];
      TryAddNpc(npc, newTargetIDs, ref newTargetCount, wormIDs, ref wormCount);
    }

    // Set target based on different enemies, if they can be hit
    foreach (NPC npc in Main.ActiveNPCs) {
      if (npc.whoAmI == Player.MinionAttackTargetNPC) {
        continue;
      }

      TryAddNpc(npc, newTargetIDs, ref newTargetCount, wormIDs, ref wormCount);
    }

    if (newTargetCount > 1) {
      newTargetIDs[..newTargetCount].Sort(SortByDistanceClosest);
    }

    _targetIDs.Clear();
    for (int i = 0, count = Math.Min(newTargetCount, data.Targets); i < count; i++) {
      _targetIDs.Add(newTargetIDs[i].NpcId);
    }

    return _mainTargetNpc is not null || newTargetIDs.Length > 0;

    static int SortByDistanceClosest(TargetInfo npcInfo1, TargetInfo npcInfo2) {
      if (npcInfo1.IsMainTarget) return -1;
      if (npcInfo2.IsMainTarget) return 1;

      return npcInfo1.Distance.CompareTo(npcInfo2.Distance);
    }
  }

  private void TryAddNpc(NPC npc, Span<TargetInfo> targetIDs, ref int count, Span<ushort> wormIDs, ref int wormCount) {
    if (!npc.CanBeChasedBy()) {
      return;
    }

    SeinData data = Data;

    float dist = Vector2.Distance(Player.Center, npc.Center);
    if (dist > data.TargetThroughWallDistSquared ||
        dist > data.TargetMaxDistSquared && InSight(Projectile, npc)) {
      return;
    }

    // Make sure we're not adding worm body segment which already has worm of same head added
    if (wormCount > 0 && npc.aiStyle is NPCAIStyleID.Worm or NPCAIStyleID.TheDestroyer) {
      // TODO: Sort targeted worm piece by closest rather than whoAmI
      int id = (int)npc.ai[3];
      for (int i = 0; i < wormCount; i++) {
        if (wormIDs[i] == id) {
          return;
        }
      }

      wormIDs[wormCount++] = (ushort)npc.ai[3];
    }

    bool isMainTarget = _mainTargetNpc is null;
    if (isMainTarget) {
      _mainTargetNpc = npc;
    }

    targetIDs[count++] = new TargetInfo((ushort)npc.whoAmI, dist, isMainTarget);
  }

  private static bool InSight(Projectile self, Entity entity) =>
    Collision.CanHitLine(
      self.position, self.width, self.height,
      entity.position, entity.width, entity.height);

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
    _spiritFlameSound.Play(Projectile.Center);
    ref SeinData data = ref Data;

    if (!hasTarget) {
      // Fire at air - nothing to target
      for (int i = 0; i < data.ShotsToPrimaryTarget; i++) {
        Shoot(null);
      }

      return;
    }

    int usedShots = 0;
    int loopCount = 0;
    while (loopCount < data.ShotsToPrimaryTarget) {
      for (int t = 0; t < _targetIDs.Count; t++) {
        bool isPrimary = t == 0;
        int shots = isPrimary ? data.ShotsToPrimaryTarget : data.ShotsPerTarget;
        if (loopCount >= shots) continue;

        Shoot(Main.npc[_targetIDs[t]]);
        if (++usedShots >= data.MaxShotsAtOnce) {
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
  private void Shoot(NPC? npc) {
    ref SeinData data = ref Data;

    Vector2 shootVel;
    float rotation;

    if (npc is null) {
      // Fire at air
      shootVel = new Vector2(Main.rand.Next(-12, 12), Main.rand.Next(24, 48)).SafeNormalize(default);
      rotation = (float)(Main.rand.Next(-180, 180) / 180f * Math.PI);
    }
    else {
      // Fire at enemy NPC
      shootVel = npc.position - Projectile.Center;
      rotation = Main.rand.Next(-data.RandDegrees, data.RandDegrees) / 180f * (float)Math.PI;
    }

    if (shootVel == Vector2.Zero) {
      shootVel = Vector2.UnitY;
    }

    shootVel = (shootVel * data.ProjectileSpeedStart).RotatedBy(rotation);
    Projectile.velocity += shootVel.SafeNormalize(default) * -0.2f;

    int dmg = (int)(Projectile.damage * (!AutoFire ? ManualShootDamageMultiplier : 1));


    Projectile spiritFlame = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
      shootVel, _spiritFlameType, dmg, Projectile.knockBack, Projectile.owner);
    spiritFlame.netUpdate = true;
    Projectile.netUpdate = true;
    if (npc is null) {
      Vector2 targetPos =
        new Vector2(Projectile.position.X, Projectile.position.Y + Main.rand.NextFloat(8, 48))
          .RotatedBy(Main.rand.NextFloat((float)Math.PI * 2));
      spiritFlame.ai[0] = targetPos.X != 0 ? targetPos.X : float.Epsilon;
      spiritFlame.ai[1] = targetPos.Y;
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

    if (player.HasBuff(_buffType)) {
      Projectile.timeLeft = 2;
    }
  }

  protected override void Behavior() {
    SeinMovement();
    UpdateGoalPosition();
    TickCooldown();
    VerifyNoNaNs();

    if (Main.dontStarveWorld) {
      _lightStrength -= 0.004f;
    }

    ref SeinData data = ref Data;

    Lighting.AddLight(Projectile.Center, data.Color.ToVector3() * data.LightStrength * _lightStrength);
    if (!Main.dedServ) {
      Vector3 tileLight = Lighting.GetColor(Projectile.Center.ToTileCoordinates()).ToVector3();
      float brightness = tileLight.X + tileLight.Y + tileLight.Z;
      _lightStrength = Math.Min(Math.Max(_lightStrength, brightness / 3), 1f);
    }

    if (Player.whoAmI != Main.myPlayer) {
      return;
    }

    OriPlayer oPlayer = Player.GetModPlayer<OriPlayer>();
    bool hasTarget = UpdateTargets();
    bool attemptFire = AutoFire ? hasTarget : oPlayer.Input.LeftClick.JustPressed && !Player.mouseInterface;

    if (!attemptFire || Cooldown != 0 && (Cooldown <= CooldownMin || _currentShotsFired >= data.Bursts)) {
      return;
    }

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
    Texture2D tex = _glowTexture.Value;
    Vector2 orig = new Vector2(tex.Width, tex.Width) * 0.5f;
    for (int i = 0; i < 3; i++) {
      Color color = Data.Color;
      color.A = i switch {
        0 => 255,
        1 => 200,
        _ => 175
      };

      Rectangle sourceRect = new(0, i * tex.Height / 3, tex.Width, tex.Width);
      Main.EntitySpriteDraw(tex, pos, sourceRect, color, Projectile.rotation, orig, Projectile.scale,
        SpriteEffects.None);
    }
  }

  private Player? _player;
  private int _hPi;

  private readonly record struct TargetInfo(ushort NpcId, float Distance, bool IsMainTarget);
}
