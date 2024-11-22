using System;
using AnimLib.States;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// State to represent when no ability is in use. Contains various basic movement substates.
/// </summary>
public sealed partial class NoAbility(Player player) : StateMachine(player) {
  private Player Player => (Player)Entity;

  private OriPlayer _oriPlayer = null!; // OnInitialize()
  private Idle _idle = null!; // OnInitialize()
  private IdleAgainst _idleAgainst = null!; // OnInitialize()
  private WallSlide _wallSlide = null!; // OnInitialize()
  private Running _running = null!; // OnInitialize()
  private Jump _jump = null!; // OnInitialize()
  private Falling _falling = null!; // OnInitialize()
  private Default _default = null!; // OnInitialize()

  protected override void OnInitialize() {
    _oriPlayer = Player.GetModPlayer<OriPlayer>();
    _idle = AddChild(new Idle(Player));
    _idleAgainst = AddChild(new IdleAgainst(Player));
    _wallSlide = AddChild(new WallSlide(Player));
    _running = AddChild(new Running(Player));
    _jump = AddChild(new Jump(Player));
    _falling = AddChild(new Falling(Player));
    _default = AddChild(new Default(Player));
  }

  protected override void OnEnter(State? fromState) => UpdateChildState();

  protected override void OnPreUpdate() {
    UpdateChildState();
  }

  protected override void OnPostUpdate() {
    if (Player.grapCount > 0 || Player.pulley || Player.dead || Player.stoned) {
      // I'd rather not stick a transition in PostUpdate, but Player.pulley is always false during OnPreUpdate
      TrySetActiveChild(_default, checkTransition: false, silent: true);
    }
  }

  private void UpdateChildState() {
    if (Player.grapCount > 0 || Player.pulley || Player.dead || Player.stoned) {
      TrySetActiveChild(_default, checkTransition: false, silent: true);
      return;
    }

    State desiredState = (_oriPlayer.IsGrounded, _oriPlayer.OnWall) switch {
      (IsGrounded: true, OnWall: false) when IsIdle() => _idle,
      (IsGrounded: true, OnWall: false) => _running,
      (IsGrounded: false, OnWall: false) when IsJumping() => _jump,
      (IsGrounded: false, OnWall: false) => _falling,
      (IsGrounded: true, OnWall: true) => _idleAgainst,
      (IsGrounded: false, OnWall: true) => _wallSlide
    };

    // Should be deterministic based on OnWall/IsGrounded or some Player vars.
    // Avoids spamming packets
    TrySetActiveChild(desiredState, checkTransition: false, silent: true);
  }

  /// <summary>
  /// Always false when player is trying to move and isn't on a wall.
  /// True if slow movement, or on slippery surface.
  /// </summary>
  /// <returns></returns>
  private bool IsIdle() => (Player is { controlLeft: false, controlRight: false } && !_oriPlayer.OnWall) ||
    Math.Abs(Player.velocity.X) < 0.2f || Player is not { slippy: false, slippy2: false, sliding: false };

  private bool IsJumping() => Player.velocity.Y * Player.gravDir < 0;
}
