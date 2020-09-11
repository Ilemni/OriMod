using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using AnimLib.Animations;

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
    /// The <see cref="OriPlayer"/> instance this <see cref="OriAnimationController"/> instance belongs to.
    /// </summary>
    public OriPlayer oPlayer => _op ?? (_op = player.GetModPlayer<OriPlayer>());
    private OriPlayer _op;

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
      var player = oPlayer.player;
      var abilities = oPlayer.abilities;

      if (oPlayer.Transforming) {
        IncrementFrame("Transform", speed: oPlayer.HasTransformedOnce ? OriPlayer.RepeatedTransformRate : 1);
        return;
      }
      if (!oPlayer.IsOri) {
        return;
      }

      if (player.pulley || player.mount.Active) {
        IncrementFrame("Idle");
        return;
      }
      if (oPlayer.abilities.burrow) {
        float rad = (float)Math.Atan2(abilities.burrow.velocity.X, -abilities.burrow.velocity.Y);
        rad *= player.direction;
        if (player.gravDir < 0) {
          rad += (float)Math.PI;
        }
        IncrementFrame("Burrow", rotation: rad);
        return;
      }
      if (abilities.wallChargeJump.Active) {
        IncrementFrame("Dash", frameIndex: 0, rotation: abilities.wallChargeJump.Angle);
        return;
      }
      if (abilities.wallJump) {
        IncrementFrame("WallJump");
        return;
      }
      if (abilities.airJump && !(abilities.dash || abilities.chargeDash)) {
        IncrementFrame("AirJump", rotation: FrameTime * 0.6f);
        return;
      }
      if (abilities.bash) {
        IncrementFrame("Bash");
        return;
      }
      if (abilities.launch) {
        if (!abilities.launch.Ending) {
          var ct = abilities.launch.CurrentTime;
          var accel = ct * (ct < 5 ? 0.05f : ct < 20 ? 0.03f : 0.02f);
          // Somewhat accelerating speed of rotation
          IncrementFrame("AirJump", rotation: SpriteRotation + accel);
        }
        else {
          IncrementFrame("ChargeJump", duration: 6, rotation: (abilities.launch.launchAngle + (float)Math.PI / 2) * player.direction, loop: LoopMode.Always, direction: Direction.PingPong);
        }
        return;
      }
      if (abilities.stomp) {
        switch (abilities.stomp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("AirJump", rotation: FrameTime * 0.8f);
            return;
          case Ability.State.Active:
            IncrementFrame("ChargeJump", duration: 2, rotation: MathHelper.ToRadians(180), loop: LoopMode.Always, direction: Direction.PingPong);
            return;
        }
      }
      if (abilities.glide) {
        switch (abilities.glide.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("GlideStart");
            return;
          case Ability.State.Active:
            IncrementFrame("Glide");
            return;
          case Ability.State.Ending:
            IncrementFrame("GlideStart", direction: Direction.Reverse);
            return;
        }
      }
      if (abilities.climb) {
        if (abilities.climb.IsCharging) {
          if (!abilities.wallChargeJump.Charged) {
            IncrementFrame("WallChargeJumpCharge", frameIndex: abilities.wallChargeJump.Refreshed ? null : (int?)0);
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
          IncrementFrame("WallChargeJumpAim", frameIndex: frame);
          return;
        }
        if (Math.Abs(player.velocity.Y) < 0.1f) {
          IncrementFrame("ClimbIdle");
        }
        else {
          IncrementFrame(player.velocity.Y * player.gravDir < 0 ? "Climb" : "WallSlide", speed: Math.Abs(player.velocity.Y) * 0.4f);
        }
        return;
      }
      if (oPlayer.OnWall && !oPlayer.IsGrounded) {
        IncrementFrame("WallSlide");
        return;
      }
      if (abilities.dash || abilities.chargeDash) {
        IncrementFrame("Dash", frameIndex: Math.Abs(player.velocity.X) < 18f ? 1 : 0);
        return;
      }
      if (abilities.lookUp) {
        switch (abilities.lookUp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("LookUpStart");
            return;
          case Ability.State.Active:
            IncrementFrame("LookUp");
            return;
          case Ability.State.Ending:
            IncrementFrame("LookUpStart", direction: Direction.Reverse);
            return;
        }
      }
      if (abilities.crouch) {
        switch (abilities.crouch.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("CrouchStart");
            return;
          case Ability.State.Active:
            IncrementFrame("Crouch");
            return;
          case Ability.State.Ending:
            IncrementFrame("CrouchStart", direction: Direction.Reverse);
            return;
        }
      }

      if (abilities.chargeJump.Active) {
        IncrementFrame("ChargeJump");
        return;
      }
      if (!oPlayer.IsGrounded) {
        IncrementFrame(player.velocity.Y * player.gravDir < 0 ? "Jump" : "Falling");
        return;
      }
      if (Math.Abs(player.velocity.X) > 0.2f) {
        IncrementFrame("Running", speed: (int)Math.Abs(player.velocity.X) * 0.45f);
        return;
      }
      IncrementFrame(oPlayer.OnWall ? "IdleAgainst" : "Idle");
      return;
    }
  }
}
