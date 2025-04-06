using System;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a charged jump off walls.
/// </summary>
public sealed class WallChargeJump : OriAbility {
  private static readonly float[] Speeds = [
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 82.8f, 76f, 69f, 61f, 51f, 40f, 30f, 22f, 15f,
    12f
  ];


  private Vector2 _direction;
  private float _angle;

  private SoundInfo _startSound = new("Ori/ChargeJump/seinChargeJumpJump", 3, 0.8f);

  protected override bool StartCooldownOnEnter => true;

  public override bool ShowHintInUI => NPC.downedPlantBoss
    && GetState<ChargeJump>().Unlocked
    && GetState<Climb>().Unlocked;

  protected override void OnEnter(State? fromState) {
    _startSound.Play(Player);
    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
      ModContent.ProjectileType<ChargeJumpProjectile>(), 30, 0f,
      Player.whoAmI, 0, 1);
    Player.velocity = _direction * (Speeds[ActiveTime] * 0.5f);
  }

  protected override void NetSync(NetSyncer sync) {
    sync.Sync(ref _direction);
    sync.Sync(ref _angle);
    sync.SyncPositionAndVelocity(Player);
  }

  public override void SetControls() {
    Player.controlJump = false;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlTorch = false;
    Player.controlUseItem = false;
  }

  public override void PostUpdateRunSpeeds() {
    Player.velocity = _direction * (Speeds[ActiveTime] * 0.5f);
    Player.ChangeDir(Math.Sign(Player.velocity.X));
    Player.maxFallSpeed = Math.Abs(Player.velocity.Y);

    // NetUpdate = true;
  }

  public override void PostUpdateMiscEffects() {
    if (ActiveTime <= Speeds.Length - 1) {
      return;
    }

    // At end of WallChargeJump, see if we should transition right to gliding
    if (Input.Glide.Current && TriggerState<Glide>()) {
      return;
    }

    CancelState();
  }

  public override AnimationOptions? GetAnimationOptions() {
    float angle = _angle * Player.gravDir * (_direction.X < 0 ? -1 : 1);
    return new AnimationOptions("Dash") { FrameIndex = 0, Rotation = angle };
  }

  internal void SetAimAndDirection(float angle, Vector2 direction) {
    _angle = angle;
    _direction = direction;
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Duration", ActiveTime, Speeds.Length - 1, Color.Blue);
    ui.DrawAppendLabelValue(_angle, format: ['F']);
  }
}
