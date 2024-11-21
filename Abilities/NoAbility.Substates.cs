using System;
using AnimLib.Animations;
using AnimLib.States;
using AnimLib.UI.Debug;
using Terraria;

namespace OriMod.Abilities;

public sealed partial class NoAbility {
  internal abstract class Grounded(Player player) : OriState(player) {
    protected override void OnEnter(State? fromState) {
      if (fromState is InAir) {
        FootstepManager.PlayLandingFromPlayer(Player);
        OriPlayer.FootstepDust();
      }
    }

    protected override void OnPreUpdate() {
      if (Player.controlDown && TriggerState<Crouch>()) {
        return;
      }

      if (Player.controlUp || Input.Charge.Current) {
        TriggerState<LookUp>();
      }
    }

    protected override AnimationOptions? GetAnimationOptions() =>
      new(Name, speed: Player.webbed ? 0.3f : 1);

    protected override void DebugText(DebugUIState ui) {
      // No useful fields to display, prevent calling base.DebugText
    }
  }

  internal abstract class InAir(Player player) : OriState(player) {
    protected override void OnPreUpdate() {
      if (Input.Jump.JustPressed && TriggerState<AirJump>()) {
        return;
      }

      if (Input.Glide.Current && TriggerState<Glide>()) {
        return;
      }

      if (Input.Dash.JustPressed && Input.Charge.Current) {
        if (TriggerState<ChargeDash>()) {
          return;
        }

        if (TriggerState<Dash>()) {
          return;
        }
      }
    }

    protected override AnimationOptions? GetAnimationOptions() {
      // If other mods modify player rotation, assume spinning behaviour
      return Player.fullRotation != 0
        ? new AnimationOptions("AirJump")
        : new AnimationOptions(Name, frameIndex: Player.webbed ? 1 : null);
    }

    protected override void DebugText(DebugUIState ui) {
      // No useful fields to display, prevent calling base.DebugText
    }
  }

  internal sealed class Idle(Player player) : Grounded(player);

  internal sealed class IdleAgainst(Player player) : Grounded(player);

  internal sealed class Running(Player player) : Grounded(player) {
    protected override AnimationOptions? GetAnimationOptions() =>
      new("Running", speed: Math.Abs(Player.velocity.X) * 0.45f);
  }

  internal sealed class Jump(Player player) : InAir(player) {
    protected override void OnEnter(State? fromState) {
      if (fromState is Grounded) {
        _jumpSound.Play(Player);
      }
    }

    private SoundInfo _jumpSound = new("Ori/Jump/seinJumpsGrass", 5, 0.6f);

    private float _velocityLastFrame;

    protected override AnimationOptions? GetAnimationOptions() {
      float velocityY = Player.velocity.Y * Player.gravDir;
      int frame = _velocityLastFrame * 0.98f > velocityY ? 0 : 1;
      _velocityLastFrame = velocityY;
      return new AnimationOptions("Jump", frameIndex: Player.webbed ? 1 : frame);
    }
  }

  internal sealed class Falling(Player player) : InAir(player);

  internal sealed class WallSlide(Player player) : OriState(player) {
    protected override void OnPreUpdate() {
      if (!IsLocal) {
        return;
      }

      if (Input.Jump.JustPressed && TriggerState<WallJump>()) {
        return;
      }

      if (Input.Climb.Current && !Player.controlDown && TriggerState<Climb>()) {
        return;
      }
    }

    protected override AnimationOptions? GetAnimationOptions() =>
      new("WallSlide", speed: Player.webbed ? 0.3f : 1.0f);

    protected override void DebugText(DebugUIState ui) {
      // No useful fields to display, prevent calling base.DebugText
    }
  }
}
