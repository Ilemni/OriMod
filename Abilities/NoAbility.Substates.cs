using System;
using AnimLib.Animations;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Skins.Slots;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace OriMod.Abilities;

public sealed partial class NoAbility {
  internal abstract class Grounded : OriState {
    protected override void OnEnter(State? fromState) {
      if (fromState is InAir) {
        FootstepManager.PlayLandingFromPlayer(Player);
        FootstepDust();
      }
    }

    public override void PostUpdateMiscEffects() {
      if (!Active) {
        return;
      }

      if (Player.controlDown && TriggerState<Crouch>()) {
        return;
      }

      if (Player.controlUp || Input.Charge.Current) {
        TriggerState<LookUp>();
      }
    }

    public override AnimationOptions? GetAnimationOptions() => new(Name) { Speed = AnimationSpeed };

    protected void FootstepDust() {
      if (Main.dedServ || Character.LightStrength <= 0.1f) {
        return;
      }

      Vector2 dustPos = Player.Bottom + new Vector2(Player.direction == -1 ? -4 : 2, -2);
      for (int i = 0; i < 4; i++) {
        Dust dust = Dust.NewDustDirect(dustPos, 2, 2, DustID.Clentaminator_Cyan, 0f, -2.7f, 0, Color.White);
        dust.noGravity = true;
        dust.scale = 0.75f;
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.shader.UseColor(Color.White);
        dust.fadeIn = 0.03947368f;
      }
    }
  }

  internal abstract class InAir : OriState {
    private static int MinHighRotationTimeToAirJump => 3;
    private bool ShouldAirJumpAnimation => _highRotationTime > MinHighRotationTimeToAirJump;
    private float _lastFullRotation;
    private int _highRotationTime;

    public override AnimationOptions? GetAnimationOptions() {
      float delta = Math.Abs(Player.fullRotation - _lastFullRotation);
      _lastFullRotation = Player.fullRotation;
      if (delta > 0.5f) { // TODO: test value
        _highRotationTime++;
      }
      else {
        _highRotationTime = 0;
      }

      return ShouldAirJumpAnimation
        ? new AnimationOptions("AirJump")
        : new AnimationOptions(Name) { FrameIndex = Player.webbed ? 1 : null };
    }
  }

  internal sealed class Idle : Grounded;

  internal sealed class IdleAgainst : Grounded;

  internal sealed class Running : Grounded {
    [HookCondition(HookConditionFlags.SelfActive)]
    public override void FrameEffects() {
      if (Anim.FrameChangedThisTick && Anim.CurrentFrame.UserData.HasFlag("footstep")) {
        FootstepManager.PlayFootstepFromPlayer(Player);
        FootstepDust();
      }
    }

    public override AnimationOptions? GetAnimationOptions() {
      if (Player.direction * Player.velocity.X >= 0) {
        return new AnimationOptions("Running") { Speed = Math.Abs(Player.velocity.X) * 0.45f };
      }

      // Play running back animation if we have it, else play running animation in reverse
      bool hasRunningBackTag = Anim.HasTag("RunningBack");
      return new AnimationOptions(hasRunningBackTag ? "RunningBack" : "Running") {
        Speed = Math.Abs(Player.velocity.X) * 0.45f,
        IsReversed = !hasRunningBackTag
      };
    }
  }

  internal sealed class Jumping : InAir {
    private float _velocityLastFrame;
    private SoundInfo _jumpSound = new("Ori/Jump/seinJumpsGrass", 5, 0.6f);

    protected override void OnEnter(State? fromState) {
      if (fromState is Grounded) {
        _jumpSound.Play(Player);
      }
    }

    public override AnimationOptions? GetAnimationOptions() {
      float velocityY = Player.velocity.Y * Player.gravDir;
      int frame = _velocityLastFrame * 0.98f > velocityY ? 0 : 1;
      _velocityLastFrame = velocityY;
      return new AnimationOptions("Jump") { FrameIndex = Player.webbed ? 1 : frame };
    }
  }

  internal sealed class Falling : InAir;

  internal sealed class WallSlide : OriState {
    public override void PostUpdateMiscEffects() {
      if (!Active || !IsLocal) {
        return;
      }

      if (Input.Jump.JustPressed && TriggerState<WallJump>()) {
        return;
      }

      if (Input.Climb.Current && !Player.controlDown && TriggerState<Climb>()) {
        return;
      }
    }

    public override AnimationOptions? GetAnimationOptions() {
      string tag = "WallSlide";
      if (!Character.Anim.HasTag(tag)) {
        tag = Player.velocity.Y * Player.gravDir < 0 ? "Jump" : "Falling";
      }
      return new AnimationOptions(tag) { Speed = AnimationSpeed };
    }
  }

  /// Treat this as a placeholder state
  internal sealed class Default : OriState {
    public override AnimationOptions? GetAnimationOptions() => new("Default");
  }
}
