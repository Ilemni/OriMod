using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Terraria;
using Terraria.DataStructures;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick and fast horizontal dash. May be used in the air.
/// </summary>
public sealed class ChargeDash : OriAbility {
  [field: AllowNull, MaybeNull]
  public Dash Dash => field ??= GetState<Dash>();

  private static int ManaCost => 25;
  private static float MaxRange => 480f;
  private static float MaxRangeSquared => MaxRange * MaxRange;
  private static int Duration => Speeds.Length - 1;

  private static readonly float[] Speeds = [
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
  ];

  private float Speed => Speeds[Math.Clamp(ActiveTime, 0, Speeds.Length - 1)];


  private sbyte _direction;
  private Vector2 _startDirection;

  private SoundInfo _sound = new("Ori/ChargeDash/seinChargeDash", 3, 0.5f);


  /// <summary>
  /// Target of this Charge Dash. May be <see langword="null"/>.
  /// </summary>
  private Entity? _target;

  [MemberNotNullWhen(true, nameof(_target))]
  private bool HasTarget => _target is { active: true };

  public bool NpcIsTarget(NPC npc) => _target is NPC target && npc.whoAmI == target.whoAmI;

  protected override bool StartCooldownOnExit => true;

  public override bool ShowHintInUI => Dash.IsMaxLevel && NPC.downedGolemBoss;

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility.Idle>(),
      GetState<NoAbility.Running>(),
      GetState<NoAbility.Jumping>(),
      GetState<NoAbility.Falling>(),
      GetState<Glide>(),
      GetState<WallJump>(),
      GetState<LookUp>()
    ]);
  }

  protected override bool UpdateInterrupt(State activeState) {
    return Input.Dash.JustPressed && Input.Charge.Current;
  }

  public override bool CanEnter() => base.CanEnter() && !OnWall && Player.CheckMana(ManaCost, blockQuickMana: true);

  protected override void OnEnter(State? fromState) {
    Dash.StartCooldown();
    Player.CheckMana(ManaCost, pay: true, blockQuickMana: true);

    NPC? targetNpc = null;

    if (IsLocal && OriMod.ConfigClient.eChargeDashHoming) {
      Player.manaRegenDelay = Player.maxRegenDelay;
      float closestDistFromMouse = MaxRangeSquared * 4;
      Vector2 mouseWorld = Main.MouseWorld;
      foreach (NPC npc in Main.ActiveNPCs) {
        if (npc.friendly ||
            (Player.Center - npc.Center).LengthSquared() > MaxRangeSquared ||
            !Collision.CanHitLine(Player.Center, Player.width, Player.height, npc.Center, 16, 16)) {
          continue;
        }

        float distFromMouse = (mouseWorld - npc.Center).LengthSquared();
        if (distFromMouse >= closestDistFromMouse) {
          continue;
        }

        closestDistFromMouse = distFromMouse;
        targetNpc = npc;
      }
    }

    _target = targetNpc;

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

    _target = null;
    CancelState();
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => Active;

  public override void PostUpdateMiscEffects() {
    if (ActiveTime > Duration || OnWall || Player.controlJump) {
      End();
    }
  }

  public override void PostUpdateRunSpeeds() {
    float speed = Speed;
    Player.gravity = 0;

    if (HasTarget) {
      Player.maxFallSpeed = speed;
      Vector2 dir = _target.Center - Player.Center;
      dir.Y -= 32f;
      dir.Normalize();
      Player.velocity = dir * speed;
    }
    else {
      Player.velocity.X = speed * _direction * 0.8f;
      Player.velocity.Y = (IsGrounded ? -0.1f : 0.15f * (ActiveTime + 1)) * Player.gravDir;
    }

    Player.runSlowdown = 26f;

    NetUpdate = true;
  }

  protected override void NetSync(NetSyncer sync) {
    if (ActiveTime == 0) {
      sync.Sync(ref _direction);
      sync.SyncEntity(ref _target);
      sync.SyncPositionAndVelocity(Player);
    }
  }

  protected override bool CanRefresh(bool cooledDown) => !Input.Charge.Current;

  protected override void OnEndCooldown(bool justCooledDown) {
    if (justCooledDown) {
      RefreshParticles(Color.LightBlue);
    }
  }

  public override AnimationOptions? GetAnimationOptions() {
    int frameIndex = Math.Abs(Player.velocity.X) < 12f ? 1 : 0;
    return new AnimationOptions("Dash") { FrameIndex = frameIndex };
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Duration", ActiveTime, Duration, Color.Blue);
  }

  protected override void DebugCooldownReason(UIStateInfo ui) {
    ui.DrawAppendLine("Must release Charge key");
  }
}
