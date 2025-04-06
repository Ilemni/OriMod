using System;
using System.Collections.Generic;
using AnimLib.States;

namespace OriMod.Abilities;

/// <summary>
/// State to represent when no ability is in use. Contains various basic movement substates.
/// </summary>
public sealed partial class NoAbility : StateMachine {
  public override OriCharacter Character => (OriCharacter)base.Character!;

  private Idle _idle = null!; // OnInitialize()
  private IdleAgainst _idleAgainst = null!; // OnInitialize()
  private WallSlide _wallSlide = null!; // OnInitialize()
  private Running _running = null!; // OnInitialize()
  private Jumping _jumping = null!; // OnInitialize()
  private Falling _falling = null!; // OnInitialize()

  public override void RegisterChildren(List<State> statesToAdd) {
    statesToAdd.AddRange([
      GetState<Idle>(),
      GetState<IdleAgainst>(),
      GetState<WallSlide>(),
      GetState<Running>(),
      GetState<Jumping>(),
      GetState<Falling>(),
      GetState<Default>()
    ]);
  }

  public override void Initialize() {
    _idle = GetState<Idle>();
    _idleAgainst = GetState<IdleAgainst>();
    _wallSlide = GetState<WallSlide>();
    _running = GetState<Running>();
    _jumping = GetState<Jumping>();
    _falling = GetState<Falling>();
    base.Initialize();
  }

  protected override void OnEnter(State? fromState) => UpdateChildState();

  public override void PreUpdate() => UpdateChildState();

  private void UpdateChildState() {
    if (Player.grapCount > 0 || Player.pulley || Player.dead || Player.stoned || (Player.mount?.Active ?? false)) {
      TrySetActiveChild<Default>(checkTransition: false, silent: true);
      return;
    }

    State desiredState = (Character.IsGrounded, Character.OnWall) switch {
      (IsGrounded: true, OnWall: false) when IsIdle() => _idle,
      (IsGrounded: true, OnWall: false) => _running,
      (IsGrounded: false, OnWall: false) when IsJumping() => _jumping,
      (IsGrounded: false, OnWall: false) => _falling,
      (IsGrounded: true, OnWall: true) => _idleAgainst,
      (IsGrounded: false, OnWall: true) => _wallSlide
    };

    // Should be deterministic based on OnWall/IsGrounded or some Player vars.
    // Avoids spamming packets
    TrySetActiveChild(desiredState, checkTransition: false, silent: true);
    return;

    // Always false when player is trying to move and isn't on a wall.
    // True if slow movement, or on slippery surface.
    bool IsIdle() {
      float velX = Math.Abs(Player.velocity.X);
      return Player is { controlLeft: false, controlRight: false } ||
        velX < 0.2f || // Idle while slipping or otherwise barely moving
        velX < 0.6f && Player is not { slippy: false, slippy2: false, sliding: false };
    }

    bool IsJumping() => Player.velocity.Y * Player.gravDir < 0;
  }
}
