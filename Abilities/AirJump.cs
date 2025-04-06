using System;
using System.Collections.Generic;
using AnimLib;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for jumping in the air.
/// </summary>
public sealed class AirJump : OriAbility {
  private static float JumpVelocity => 8.8f;
  private static int EndDuration => 32;

  private int _currentCount;
  private sbyte _gravityDirection;
  private sbyte _startDirection;
  private ChargeJump _chargeJump = null!; // OnInitialize()
  private SoundInfo _tripleJumpSound = new("Ori/TripleJump/seinTripleJumps", 5, 0.6f);
  private SoundInfo _doubleJumpSound = new("Ori/DoubleJump/seinDoubleJumps", 4, 0.5f);

  public override int MaxLevel => 4;
  public override bool SupportsCooldown => true;
  private int MaxJumps => Level;
  private ref ExtraJumpState AirJumpExtraJumpState => ref Player.GetJumpState<ExtraAirJump>();

  public bool HasJumpsLeft => _currentCount < MaxJumps;

  public override bool ShowHintInUI => NPC.downedBoss1 || NPC.downedBoss2 || Main.hardMode;

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility.Jumping>(),
      GetState<NoAbility.Falling>(),
      GetState<Dash>(),
      GetState<Glide>()
    ]);
  }

  public override void Initialize() {
    _chargeJump = GetState<ChargeJump>();
  }

  public override bool CanEnter() {
    return base.CanEnter() && !IsGrounded && !OnWall && !Player.justJumped && _currentCount < MaxJumps &&
      !_chargeJump.CanEnter() && !Player.AnyExtraJumpUsable() &&
      !(OriMod.ConfigClient.AirJumpNotDown && Player.controlDown) &&
      !(OriMod.ConfigClient.AirJumpOnlyUp && !Player.controlUp);
  }

  protected override bool UpdateInterrupt(State activeState) {
    return Input.Jump.JustPressed && !IsGrounded && Character.InAirTime > 2;
  }

  protected override void OnEnter(State? fromState) {
    AirJumpExtraJumpState.Enable();
    _currentCount++;
    _gravityDirection = (sbyte)Player.gravDir;
    _startDirection = (sbyte)Player.direction;

    if (MaxJumps == 1 || _currentCount != MaxJumps) {
      _doubleJumpSound.Play(Player);
    }
    else {
      _tripleJumpSound.Play(Player);
    }

    if (_currentCount == MaxJumps) {
      StartCooldown();
    }

    Player.velocity.Y = -JumpVelocity * _gravityDirection;
  }

  protected override void OnExit(State? toState) {
    AirJumpExtraJumpState.Disable();
  }

  protected override void NetSync(NetSyncer sync) {
    sync.Sync(ref _gravityDirection);
    sync.SyncPositionAndVelocity(Player);
    sync.Sync7BitEncodedInt(ref _currentCount);
  }

  public override void SetControls() {
    Player.controlTorch = false;
  }

  [HookCondition(HookConditionFlags.Strict)]
  public override bool PreItemCheck() => false;

  public override void PostUpdateMiscEffects() {
    if (!IsLocal) {
      return;
    }

    if (IsGrounded) {
      CancelState();
      return;
    }

    if (ActiveTime <= EndDuration && !(Player.velocity.Y * Player.gravDir > 0)) {
      return;
    }

    // Prevent NoAbility transition if we can glide instead
    // Without this, Player.controlTorch may flicker for one tick
    // TODO: check if this prevention still works after FSM/hook rework.
    if (Input.Glide.Current && TriggerState<Glide>()) {
      return;
    }

    CancelState();
  }

  public override void PostUpdateRunSpeeds() {
    float newVel = -JumpVelocity * ((float)(EndDuration + 5 - ActiveTime) / (EndDuration + 5)) * _gravityDirection;
    if (Math.Abs(Player.velocity.Y) < Math.Abs(newVel)) {
      Player.velocity.Y = newVel;
    }

    Player.ChangeDir(_startDirection);
  }

  protected override bool CanRefresh(bool cooledDown) =>
    IsGrounded || Character.ActiveState is Bash or Launch or Climb;

  protected override void OnEndCooldown(bool justCooledDown) {
    _currentCount = 0;
  }

  public override AnimationOptions? GetAnimationOptions() {
    Player.ChangeDir(_startDirection);
    bool doRotation = Anim.CurrentFrame.UserData.HasFlag("spin");
    float rotation = doRotation ? ActiveTime * Player.gravDir * _startDirection : 0;

    return new AnimationOptions("AirJump") { Rotation = rotation };
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Jumps", _currentCount, MaxJumps);
    ui.DrawAppendLabelProgressBar("Jump Duration", ActiveTime, EndDuration);
  }

  protected override void DebugCooldownReason(UIStateInfo ui) {
    ui.DrawAppendLine("Must touch ground, or enter Bash, Launch, or Climb");
  }

  [UsedImplicitly]
  private sealed class ExtraAirJump : ExtraJump {
    public override Position GetDefaultPosition() => BeforeBottleJumps;
    public override float GetDurationMultiplier(Player player) => 1;

    public override bool CanStart(Player player) =>
      player.GetState<AirJump>().CanEnter();

    public override void OnStarted(Player player, ref bool playSound) {
      AirJump airJump = player.GetState<AirJump>();
      player.GetJumpState<ExtraAirJump>().Available = airJump.HasJumpsLeft;
    }
  }
}
