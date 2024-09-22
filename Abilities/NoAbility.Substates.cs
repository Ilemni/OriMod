using System;
using AnimLib.Animations;
using AnimLib.States;
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
      return Player.fullRotation != 0
        // If other mods modify player rotation, assume spinning behaviour
        ? new AnimationOptions("AirJump")
        : new AnimationOptions(Name, frameIndex: Player.webbed ? 1 : null);
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
  }
}
