using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
using JetBrains.Annotations;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for jumping in the air.
/// </summary>
public sealed class AirJump(Player player) : OriAbility(player) {
  private static float JumpVelocity => 8.8f;
  private static int EndDuration => 32;

  private int _currentCount;
  private sbyte _gravityDirection;
  private ChargeJump _chargeJump = null!; // OnInitialize()
  private SoundInfo _tripleJumpSound = new("Ori/TripleJump/seinTripleJumps", 5, 0.6f);
  private SoundInfo _doubleJumpSound = new("Ori/DoubleJump/seinDoubleJumps", 4, 0.5f);

  public override int MaxLevel => 4;
  public override bool SupportsCooldown => true;
  private int MaxJumps => Level;
  private ref ExtraJumpState AirJumpExtraJumpState => ref Player.GetJumpState<ExtraAirJump>();

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();
    _chargeJump = parent.GetChild<ChargeJump>();

    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<Dash>(to: this);
    parent.AddInterruptible<Glide>(to: this);
  }

  public override bool CanEnter() {
    return base.CanEnter() && !IsGrounded && !OnWall && _currentCount < MaxJumps &&
      !_chargeJump.CanEnter() && !Player.AnyExtraJumpUsable() &&
      !(OriMod.ConfigClient.AirJumpNotDown && Player.controlDown) &&
      !(OriMod.ConfigClient.AirJumpOnlyUp && !Player.controlUp);
  }

  protected override void OnEnter(State? fromState) {
    AirJumpExtraJumpState.Enable();
    _currentCount++;
    _gravityDirection = (sbyte)Player.gravDir;

    if (MaxJumps == 1 || _currentCount != MaxJumps) {
      _doubleJumpSound.Play(Player);
    }
    else {
      _tripleJumpSound.Play(Player);
    }

    if (_currentCount == MaxJumps) {
      StartCooldown();
    }
  }

  protected override void OnExit() {
    AirJumpExtraJumpState.Disable();
  }

  protected override void NetSync(ISync sync) {
    sync.Sync(ref _gravityDirection);
    sync.SyncPositionAndVelocity(Player);
    sync.Sync7BitEncodedInt(ref _currentCount);
  }

  protected override bool OnPreUpdateInterruptible(State activeState) {
    return Input.Jump.JustPressed;
  }

  protected override void OnPreUpdate() {
    if (!IsLocal) {
      return;
    }

    ref ExtraJumpState airJumpState = ref AirJumpExtraJumpState;
    if (CanEnter()) {
      airJumpState.Available = true;
    }

    if (IsGrounded) {
      CancelState();
      return;
    }

    if (ActiveTime <= EndDuration && !(Player.velocity.Y * Player.gravDir > 0)) {
      return;
    }

    if (_currentCount < MaxJumps) {
      airJumpState.Available = true;
    }

    // Prevent NoAbility transition if we can glide instead
    // Without this, Player.controlTorch may flicker for one tick
    if (Input.Glide.Current && TriggerState<Glide>()) {
      return;
    }

    CancelState();
  }

  protected override void OnUpdate() {
    float newVel = -JumpVelocity * ((float)(EndDuration - ActiveTime) / EndDuration) * _gravityDirection;
    Player.velocity.Y = newVel;
    Player.controlTorch = false;
  }

  protected override bool CanRefresh(bool cooledDown) => IsGrounded || OriPlayer.ActiveState is Bash or Launch or Climb;

  protected override void OnEndCooldown() {
    _currentCount = 0;
  }

  protected override AnimationOptions? GetAnimationOptions() {
    float rotation = ActiveTime * Player.gravDir * Player.direction;
    return new AnimationOptions("AirJump", rotation: rotation);
  }

  protected override void DebugText(DebugUIState ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelValue("Jumps", _currentCount, MaxJumps);
    ui.DrawAppendLabelValue("Jump Duration", ActiveTime, EndDuration);
  }

  [UsedImplicitly]
  private sealed class ExtraAirJump : ExtraJump {
    public override Position GetDefaultPosition() => BeforeBottleJumps;
    public override float GetDurationMultiplier(Player player) => 1;

    public override bool CanStart(Player player) =>
      player.GetModPlayer<OriPlayer>().Character.Move.GetChild<AirJump>().CanEnter();
  }
}
