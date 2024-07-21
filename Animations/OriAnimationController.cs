using AnimLib.Abilities;
using AnimLib.Animations;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using System;

namespace OriMod.Animations; 

/// <summary>
/// Container for various <see cref="Animation"/>s and data to be attached to an <see cref="OriPlayer"/>. Manages advancement of frames.
/// </summary>
public class OriAnimationController : AnimationController {
  public override bool PreUpdate() => base.PreUpdate() && !player.frozen && !player.stoned;

  public override void Initialize() {
    PlayerAnim = AddAnimation(OriTextures.Instance.PlayerSprites);
  }

  /// <summary>
  /// Animation for the player sprite.
  /// </summary>
  public Animation PlayerAnim { get; private set; }

  /// <summary>
  /// Updates the player animation by one frame, and changes it depending on various conditions.
  /// </summary>
  public override AnimationOptions Update() {
    OriPlayer oPlayer = Player.GetModPlayer<OriPlayer>();
    OriAbilityManager abilities = oPlayer.abilities;

    // Transformation
    if (oPlayer.Transforming) {
      return new AnimationOptions("Transform", speed: oPlayer.HasTransformedOnce ? OriPlayer.RepeatedTransformRate : 1);
    }
    if (!oPlayer.IsOri) {
      return AnimationOptions.None;
    }

    // Handle some "special" movement
    // Todo, consider dedicated sprites to these actions, i.e. mounted, pulley, grapple
    if (Player.pulley || Player.mount.Active) {
      return new AnimationOptions("Idle", speed: Player.webbed ? 0.3f : 1.0f);
    }
    if (oPlayer.IsGrappling) {
      return Math.Abs(Player.velocity.X) > 0.1f
        ? new AnimationOptions("Jump", frameIndex: 1)
        : new AnimationOptions(oPlayer.OnWall ? "IdleAgainst" : "Default");
    }

    // Abilities
    // Start with simple cases
    if (abilities.bash) {
      return new AnimationOptions("Bash");
    }
    if (abilities.chargeJump.Active) {
      return new AnimationOptions("ChargeJump");
    }
    if (abilities.wallJump) {
      return new AnimationOptions("WallJump");
    }
    if (abilities.airJump) {
      return abilities.glide.InUse
        ? new AnimationOptions("GlideStart", frameIndex: abilities.airJump.Active ? 0 : null)
        : new AnimationOptions("AirJump", rotation: FrameTime * 0.6f * Player.gravDir * Player.direction);
    }
    if (abilities.burrow) {
      float rad = (float)Math.Atan2(abilities.burrow.velocity.X, -abilities.burrow.velocity.Y * Player.gravDir);
      return new AnimationOptions("Burrow", rotation: rad * Player.gravDir);
    }
    if (abilities.dash || abilities.chargeDash) {
      return new AnimationOptions("Dash", frameIndex: Math.Abs(Player.velocity.X) < 12f ? 1 : 0);
    }
    if (abilities.wallChargeJump) {
      return new AnimationOptions("Dash", frameIndex: 0, rotation: abilities.wallChargeJump.Angle * Player.gravDir * abilities.wallChargeJump.XDirection);
    }

    // Switch expressions for animations with start/mid/end segments

    if (abilities.glide) {
      return abilities.glide.state switch {
        AbilityState.Starting => new AnimationOptions("GlideStart"),
        AbilityState.Ending => new AnimationOptions("GlideStart", isReversed: true),
        _ => new AnimationOptions("Glide")
      };
    }

    if (abilities.crouch) {
      return abilities.crouch.state switch {
        AbilityState.Starting => new AnimationOptions("CrouchStart"),
        AbilityState.Ending => new AnimationOptions("CrouchStart", isReversed: true),
        _ => new AnimationOptions("Crouch")
      };
    }

    if (abilities.lookUp) {
      return abilities.lookUp.state switch {
        AbilityState.Starting => new AnimationOptions("LookUpStart"),
        AbilityState.Ending => new AnimationOptions("LookUpStart", isReversed: true),
        _ => new AnimationOptions("LookUp")
      };
    }

    // More complex animations

    if (abilities.stomp) {
      return abilities.stomp.state == AbilityState.Starting
        ? new AnimationOptions("AirJump", rotation: FrameTime * 0.8f)
        : new AnimationOptions("ChargeJump", speed: 2, rotation: (float)Math.PI, loopCount: 0, isPingPong: true);
    }

    if (abilities.launch) {
      if (abilities.launch.Active) {
        // Launch angle needs to be offset by 90 degrees since it uses Stomp animation
        // Disable SpriteEffects as launching should not be flipped
        return new AnimationOptions("ChargeJump", speed:0.67f, rotation: abilities.launch.LaunchAngle + (float)Math.PI / 2 * Player.gravDir, loopCount:0, isPingPong:true, effects: SpriteEffects.None);
      }

      int ct = abilities.launch.stateTime;
      float acceleration = ct * (ct < 5 ? 0.05f : ct < 20 ? 0.03f : 0.02f);
      // Somewhat accelerating speed of rotation
      return new AnimationOptions("AirJump", rotation: SpriteRotation + acceleration * Player.direction);
    }

    if (abilities.climb) {
      if (abilities.climb.Ending) {
        return new AnimationOptions("Jump", frameIndex: 0);
      }
      if (!abilities.climb.IsCharging) {
        return Math.Abs(Player.velocity.Y) < 0.1f
          ? new AnimationOptions("ClimbIdle")
          : new AnimationOptions(Player.velocity.Y * Player.gravDir < 0 ? "Climb" : "WallSlide", speed: Math.Abs(Player.velocity.Y) * 0.4f);
      }

      if (!abilities.wallChargeJump.Charged) {
        return new AnimationOptions("WallChargeJumpCharge", frameIndex: !abilities.wallChargeJump.IsOnCooldown ? null : 0);
      }

      // Aim angle determines frame of sprite.
      // 0 is middle (pointing straight left/right), 1-2 pointing downward, 3-4 pointing upward
      int frame = abilities.wallChargeJump.Angle switch {
        < -0.46f => 2,
        < -0.17f => 1,
        > 0.46f => 4,
        > 0.17f => 3,
        _ => 0
      };
      return new AnimationOptions("WallChargeJumpAim", frameIndex: frame);
    }

    // Generic/misc movement
    if (oPlayer.OnWall && !oPlayer.IsGrounded && !Player.shimmering) {
      return new AnimationOptions("WallSlide", speed: Player.webbed ? 0.3f : 1.0f);
    }
    if (!oPlayer.IsGrounded) {
      // Probably the best way to check for jumping vs falling
      return new AnimationOptions(Player.velocity.Y * Player.gravDir < 0 ? "Jump" : "Falling", frameIndex: Player.webbed ? 1 : null);
    }
    if (Math.Abs(Player.velocity.X) > 0.2f &&
      (Player.controlLeft || Player.controlRight)) {
      // Movement deadzone recommended for running animations
      // Else subtle movements such as sandstorm can cause a running animation.
      // Animation speed is also determined by player speed, as it should be
      return new AnimationOptions("Running", speed: (int)Math.Abs(Player.velocity.X) * 0.45f);
    }
    return new AnimationOptions(oPlayer.OnWall ? "IdleAgainst" : "Idle", speed: Player.webbed ? 0.3f : 1.0f);
  }
}
