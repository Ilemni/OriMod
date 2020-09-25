using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using AnimLib.Animations;
using Microsoft.Xna.Framework.Graphics;

namespace OriMod.Animations {
  /// <summary>
  /// Container for various <see cref="Animation"/>s and data to be attached to an <see cref="OriPlayer"/>. Manages advancement of frames.
  /// </summary>
  public class OriAnimationController : AnimationController {
    public override void Initialize() {
      playerAnim = GetAnimation<PlayerAnim>();
      bashAnim = GetAnimation<BashAnim>();
      glideAnim = GetAnimation<GlideAnim>();
      SetMainAnimation(playerAnim);
    }

    /// <summary>
    /// Animation for the player sprite.
    /// </summary>
    public Animation playerAnim { get; private set; }

    /// <summary>
    /// Animation for the Bash arrow sprite.
    /// </summary>
    public Animation bashAnim { get; private set; }

    /// <summary>
    /// Animation for the Glide feather sprite.
    /// </summary>
    public Animation glideAnim { get; private set; }

    /// <summary>
    /// Updates the player animation by one frame, and changes it depending on various conditions.
    /// </summary>
    public override void Update() {
      var oPlayer = player.GetModPlayer<OriPlayer>();
      var abilities = oPlayer.abilities;

      if (oPlayer.Transforming) {
        PlayTrack("Transform", speed: oPlayer.HasTransformedOnce ? OriPlayer.RepeatedTransformRate : 1);
        return;
      }
      if (!oPlayer.IsOri) {
        return;
      }

      if (player.pulley || player.mount.Active) {
        PlayTrack("Idle");
        return;
      }

      // TODO: consider "switch (oPlayer.abilities.GetActiveAbility())
      // Requires ensuring only one ability can ever be active at once
      if (oPlayer.abilities.burrow) {
        float rad = (float)Math.Atan2(abilities.burrow.velocity.X, -abilities.burrow.velocity.Y * player.gravDir);
        PlayTrack("Burrow", rotation: rad * player.gravDir);
        return;
      }
      if (abilities.wallChargeJump) {
        PlayTrack("Dash", frameIndex: 0, rotation: abilities.wallChargeJump.Angle * player.gravDir);
        return;
      }
      if (abilities.wallJump) {
        PlayTrack("WallJump");
        return;
      }
      if (abilities.airJump) {
        PlayTrack("AirJump", rotation: FrameTime * 0.6f * player.gravDir * player.direction);
        return;
      }
      if (abilities.bash) {
        PlayTrack("Bash");
        return;
      }
      if (abilities.launch) {
        if (abilities.launch.Ending) {
          PlayTrack("ChargeJump", duration: 6, rotation: abilities.launch.launchAngle + (float)Math.PI / 2 * player.gravDir, loop: LoopMode.Always, direction: Direction.PingPong, effects:SpriteEffects.None);
        }
        else {
          var ct = abilities.launch.CurrentTime;
          var accel = ct * (ct < 5 ? 0.05f : ct < 20 ? 0.03f : 0.02f);
          // Somewhat accelerating speed of rotation
          PlayTrack("AirJump", rotation: SpriteRotation + accel * player.direction);
        }
        return;
      }
      if (abilities.stomp) {
        switch (abilities.stomp.AbilityState) {
          case Ability.State.Starting:
            PlayTrack("AirJump", rotation: FrameTime * 0.8f);
            return;
          case Ability.State.Active:
            PlayTrack("ChargeJump", duration: 2, rotation: MathHelper.ToRadians(180), loop: LoopMode.Always, direction: Direction.PingPong);
            return;
        }
      }
      if (abilities.glide) {
        switch (abilities.glide.AbilityState) {
          case Ability.State.Starting:
            PlayTrack("GlideStart");
            return;
          case Ability.State.Active:
            PlayTrack("Glide");
            return;
          case Ability.State.Ending:
            PlayTrack("GlideStart", direction: Direction.Reverse);
            return;
        }
      }
      if (abilities.climb) {
        if (abilities.climb.IsCharging) {
          if (!abilities.wallChargeJump.Charged) {
            PlayTrack("WallChargeJumpCharge", frameIndex: abilities.wallChargeJump.Refreshed ? null : (int?)0);
            return;
          }
          // TODO: Multiplayer sync of aim position
          int frame = 0;
          float angle = abilities.wallChargeJump.Angle;
          if (angle < -0.46f) {
            frame = 2;
          }
          else if (angle < -0.17f) {
            frame = 1;
          }
          else if (angle > 0.46f) {
            frame = 4;
          }
          else if (angle > 0.17f) {
            frame = 3;
          }
          PlayTrack("WallChargeJumpAim", frameIndex: frame);
          return;
        }
        if (Math.Abs(player.velocity.Y) < 0.1f) {
          PlayTrack("ClimbIdle");
        }
        else {
          PlayTrack(player.velocity.Y * player.gravDir < 0 ? "Climb" : "WallSlide", speed: Math.Abs(player.velocity.Y) * 0.4f);
        }
        return;
      }
      if (abilities.dash || abilities.chargeDash) {
        PlayTrack("Dash", frameIndex: Math.Abs(player.velocity.X) < 12f ? 1 : 0);
        return;
      }
      if (abilities.lookUp) {
        switch (abilities.lookUp.AbilityState) {
          case Ability.State.Starting:
            PlayTrack("LookUpStart");
            return;
          case Ability.State.Active:
            PlayTrack("LookUp");
            return;
          case Ability.State.Ending:
            PlayTrack("LookUpStart", direction: Direction.Reverse);
            return;
        }
      }
      if (abilities.crouch) {
        switch (abilities.crouch.AbilityState) {
          case Ability.State.Starting:
            PlayTrack("CrouchStart");
            return;
          case Ability.State.Active:
            PlayTrack("Crouch");
            return;
          case Ability.State.Ending:
            PlayTrack("CrouchStart", direction: Direction.Reverse);
            return;
        }
      }
      if (abilities.chargeJump.Active) {
        PlayTrack("ChargeJump");
        return;
      }

      if (oPlayer.IsGrappling) {
        if (Math.Abs(player.velocity.X) > 0.1f) {
          PlayTrack("Jump", frameIndex: 1);
          return;
        }
        PlayTrack(oPlayer.OnWall ? "IdleAgainst" : "Default");
        return;
      }
      if (oPlayer.OnWall && !oPlayer.IsGrounded) {
        PlayTrack("WallSlide");
        return;
      }
      if (!oPlayer.IsGrounded) {
        PlayTrack(player.velocity.Y * player.gravDir < 0 ? "Jump" : "Falling");
        return;
      }
      if (Math.Abs(player.velocity.X) > 0.2f) {
        PlayTrack("Running", speed: (int)Math.Abs(player.velocity.X) * 0.45f);
        return;
      }
      PlayTrack(oPlayer.OnWall ? "IdleAgainst" : "Idle");
      return;
    }
  }
}
