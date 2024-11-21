using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using System;
using System.Diagnostics.CodeAnalysis;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick and fast horizontal dash. May be used in the air.
/// </summary>
public sealed class ChargeDash(Player player) : OriAbility(player) {
  private static int ManaCost => 25;
  private static float MaxRange => 480f;
  private static float MaxRangeSquared => MaxRange * MaxRange;
  private static int Duration => Speeds.Length - 1;

  private static readonly float[] Speeds = [
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
  ];


  private sbyte _direction;
  private Vector2 _startDirection;

  private SoundInfo _sound = new("Ori/ChargeDash/seinChargeDash", 3, 0.5f);

  private ushort _npcId = ushort.MaxValue;


  /// <summary>
  /// Target of this Charge Dash. May be <see langword="null"/>.
  /// </summary>
  private NPC? Target => _npcId < Main.npc.Length ? Main.npc[_npcId] : null;

  [MemberNotNullWhen(true, nameof(Target))]
  private bool HasTarget => Target is { active: true };


  public override int MaxCooldown => 60;

  protected override bool StartCooldownOnExit => true;

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();
    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<Glide>(to: this);
    parent.AddInterruptible<WallJump>(to: this);
    parent.AddInterruptible<Crouch>(to: this);
    parent.AddInterruptible<LookUp>(to: this);
  }

  protected override bool OnPreUpdateInterruptible(State activeState) {
    return Input.Dash.JustPressed && Input.Charge.Current;
  }

  public override bool CanEnter() => base.CanEnter() && !OnWall && Player.CheckMana(ManaCost, blockQuickMana: true);

  protected override void OnEnter(State? fromState) {
    GetParent<MovementStates>().GetChild<Dash>().StartCooldown();
    Player.statMana -= ManaCost;

    NPC? targetNpc = null;

    if (IsLocal && OriMod.ConfigClient.eChargeDashHoming) {
      Player.manaRegenDelay = (int)Player.maxRegenDelay;
      float closestDistFromMouse = MaxRangeSquared * 4;
      foreach (NPC npc in Main.ActiveNPCs) {
        if (npc.friendly ||
            (Player.Center - npc.Center).LengthSquared() > MaxRangeSquared ||
            !Collision.CanHitLine(Player.Center, Player.width, Player.height, npc.Center, 16, 16)) {
          continue;
        }

        float distFromMouse = (Main.MouseWorld - npc.Center).LengthSquared();
        if (distFromMouse >= closestDistFromMouse) {
          continue;
        }

        closestDistFromMouse = distFromMouse;
        targetNpc = npc;
      }
    }

    _npcId = (ushort)(targetNpc?.whoAmI ?? ushort.MaxValue);

    _direction = !(IsLocal && OriMod.ConfigClient.eChargeDashHoming) || targetNpc is null
      ? (sbyte)(Player.controlLeft ? -1 : Player.controlRight ? 1 : Player.direction)
      : (sbyte)(Player.position.X - targetNpc.position.X < 0 ? 1 : -1);
    Player.ChangeDir(_direction);

    if (targetNpc is not null) {
      Vector2 dir = targetNpc.Center - Player.Center;
      dir.Y -= 32f;
      dir.Normalize();
      _startDirection = dir;
    }

    _sound.Play(Player);
    NewAbilityProjectile<ChargeDashProjectile>(damage: 50);
    NetUpdate = true;
  }

  internal void EndByNpcContact(NPC npc) {
    // Force player position to same as target's, and reduce speed.
    Player.position = npc.position;
    Player.position.Y -= 32f;
    Player.velocity = _startDirection * Speeds[^1];
    RestoreAirJumps();
    CancelState();
  }

  /// <summary>
  /// End the Charge Dash.
  /// </summary>
  private void End() {
    if (Math.Abs(Player.velocity.Y) < Math.Abs(Player.velocity.X)) {
      // Reducing velocity. If intended direction is mostly flat (not moving upwards, not jumping), make it flat.
      Vector2 newVel = HasTarget ? Player.velocity : new Vector2(_direction, 0);
      newVel = newVel.SafeNormalize(default) * Speeds[^1];
      Player.velocity = newVel;
    }

    _npcId = ushort.MaxValue;
    CancelState();
  }

  protected override void OnPreUpdate() {
    if (ActiveTime > Duration || OnWall || Player.controlJump) {
      End();
    }
  }

  protected override void OnUpdate() {
    float speed = Speeds[ActiveTime];
    Player.gravity = 0;

    if (HasTarget) {
      Player.maxFallSpeed = speed;
      Vector2 dir = Target.Center - Player.Center;
      dir.Y -= 32f;
      dir.Normalize();
      Player.velocity = dir * speed;
    }
    else {
      Player.velocity.X = speed * _direction * 0.8f;
      Player.velocity.Y = (IsGrounded ? -0.1f : 0.15f * (ActiveTime + 1)) * Player.gravDir;
    }

    Player.runSlowdown = 26f;
    OriPlayer.SetImmune(12);

    NetUpdate = true;
  }

  protected override void NetSync(ISync sync) {
    if (ActiveTime == 0) {
      sync.Sync(ref _direction);
      sync.Sync(ref _npcId);
      sync.SyncPositionAndVelocity(Player);
    }
  }

  protected override bool CanRefresh(bool cooledDown) => !Input.Charge.Current;

  protected override void OnEndCooldown() => RefreshParticles(Color.LightBlue);

  protected override AnimationOptions? GetAnimationOptions() {
    int frameIndex = Math.Abs(Player.velocity.X) < 12f ? 1 : 0;
    return new AnimationOptions("Dash", frameIndex: frameIndex);
  }

  public bool NpcIsTarget(NPC npc) => npc.whoAmI == _npcId;
}
