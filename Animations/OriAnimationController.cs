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

      // Transformation
      if (oPlayer.Transforming) {
        PlayTrack("Transform", speed: oPlayer.HasTransformedOnce ? OriPlayer.RepeatedTransformRate : 1);
        return;
      }
      if (!oPlayer.IsOri) {
        return;
      }

      // Handle some "special" movement
      // Todo, consider dedicated sprites to these actions, i.e. mounted, pulley, grapple
      if (player.pulley || player.mount.Active) {
        PlayTrack("Idle");
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

      // Abilities
      // Start with simple cases
      if (abilities.bash) {
        PlayTrack("Bash");
        return;
      }
      if (abilities.chargeJump.Active) {
        PlayTrack("ChargeJump");
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
      if (abilities.burrow) {
        float rad = (float)Math.Atan2(abilities.burrow.velocity.X, -abilities.burrow.velocity.Y * player.gravDir);
        PlayTrack("Burrow", rotation: rad * player.gravDir);
        return;
      }
      if (abilities.dash || abilities.chargeDash) {
        PlayTrack("Dash", frameIndex: Math.Abs(player.velocity.X) < 12f ? 1 : 0);
        return;
      }
      if (abilities.wallChargeJump) {
        PlayTrack("Dash", frameIndex: 0, rotation: abilities.wallChargeJump.Angle * player.gravDir * abilities.wallChargeJump.xDirection);
        return;
      }

      // Switch-case for animations with start/mid/end segments

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

      // More complex animations

      if (abilities.stomp) {
        switch (abilities.stomp.AbilityState) {
          case Ability.State.Starting:
            PlayTrack("AirJump", rotation: FrameTime * 0.8f);
            return;
          case Ability.State.Active:
            PlayTrack("ChargeJump", duration: 2, rotation: (float)Math.PI, loop: LoopMode.Always, direction: Direction.PingPong);
            return;
        }
      }

      if (abilities.launch) {
        if (abilities.launch.Active) {
          // Launch angle needs to be offset by 90 degrees since it uses Stomp animation
          // Disable spriteeffects as launching should not be flipped
          PlayTrack("ChargeJump", duration: 6, rotation: abilities.launch.launchAngle + (float)Math.PI / 2 * player.gravDir, loop: LoopMode.Always, direction: Direction.PingPong, effects: SpriteEffects.None);
        }
        else {
          var ct = abilities.launch.CurrentTime;
          var accel = ct * (ct < 5 ? 0.05f : ct < 20 ? 0.03f : 0.02f);
          // Somewhat accelerating speed of rotation
          PlayTrack("AirJump", rotation: SpriteRotation + accel * player.direction);
        }
        return;
      }

      if (abilities.climb) {
        if (abilities.climb.Ending) {
          PlayTrack("Jump", frameIndex: 0);
          return;
        }
        if (!abilities.climb.IsCharging) {
          if (Math.Abs(player.velocity.Y) < 0.1f) {
            PlayTrack("ClimbIdle");
          }
          else {
            PlayTrack(player.velocity.Y * player.gravDir < 0 ? "Climb" : "WallSlide", speed: Math.Abs(player.velocity.Y) * 0.4f);
          }
          return;
        }
        else if (!abilities.wallChargeJump.Charged) {
          PlayTrack("WallChargeJumpCharge", frameIndex: abilities.wallChargeJump.Refreshed ? null : (int?)0);
          return;
        }

        // Aim angle determines frame of sprite.
        // 0 is middle (pointing straight left/right), 1-2 pointing downward, 3-4 pointing upward
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

      // Generic/misc movement
      if (oPlayer.OnWall && !oPlayer.IsGrounded) {
        PlayTrack("WallSlide");
        return;
      }
      if (!oPlayer.IsGrounded) {
        // Probably the best way to check for jumping vs falling
        PlayTrack(player.velocity.Y * player.gravDir < 0 ? "Jump" : "Falling");
        return;
      }
      if (Math.Abs(player.velocity.X) > 0.2f) {
        // Movement deadzone recommended for running animations
        // Else subtle movements such as sandstorm can cause a running animation
        // Animation speed is also determined by player speed, as it should be
        PlayTrack("Running", speed: (int)Math.Abs(player.velocity.X) * 0.45f);
        return;
      }
      PlayTrack(oPlayer.OnWall ? "IdleAgainst" : "Idle");
      return;
    }
  }
}
