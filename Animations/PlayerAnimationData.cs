using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Animations {
  /// <summary>
  /// Container for various <see cref="Animation"/>s and data to be attached to an <see cref="OriPlayer"/>. Manages advancement of frames.
  /// </summary>
  public class PlayerAnimationData {
    /// <summary>
    /// Creates a new instance of <see cref="PlayerAnimationData"/> for the given <see cref="OriPlayer"/>.
    /// </summary>
    /// <param name="oPlayer"><see cref="OriPlayer"/> instance the animations will belong to.</param>
    /// <exception cref="InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    internal PlayerAnimationData(OriPlayer oPlayer) {
      if (Main.netMode == Terraria.ID.NetmodeID.Server) {
        throw new InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
      this.oPlayer = oPlayer;
      playerAnim = new Animation(this, AnimationTrackData.Instance.PlayerAnim, OriLayers.Instance.PlayerSprite);
      bashAnim = new Animation(this, AnimationTrackData.Instance.BashAnim, OriLayers.Instance.BashArrow);
      glideAnim = new Animation(this, AnimationTrackData.Instance.GlideAnim, OriLayers.Instance.FeatherSprite);
    }

    /// <summary>
    /// The name of the animation track currently playing.
    /// </summary>
    public string TrackName {
      get => _trackName;
      set {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{nameof(value)} cannot be empty.");
        if (value != _trackName) {
          _trackName = value;
          OnTrackSwitched(value);
        }
      }
    }
    private string _trackName = "Default";

    /// <summary>
    /// Current index of the <see cref="Frame"/> being played.
    /// </summary>
    public int FrameIndex { get; private set; }

    /// <summary>
    /// Current time of the <see cref="Frame"/> being played.
    /// </summary>
    public float FrameTime { get; private set; }

    /// <summary>
    /// Current rotation the sprite is set to.
    /// </summary>
    public float SpriteRotation { get; private set; }

    /// <summary>
    /// Whether or not the animation is being played in reverse.
    /// </summary>
    public bool Reversed { get; private set; }

    /// <summary>
    /// The <see cref="OriPlayer"/> instance this <see cref="PlayerAnimationData"/> instance belongs to.
    /// </summary>
    public readonly OriPlayer oPlayer;

    /// <summary>
    /// Animation for the player sprite.
    /// </summary>
    public readonly Animation playerAnim;
    
    /// <summary>
    /// Animation for the Bash arrow sprite.
    /// </summary>
    public readonly Animation bashAnim;

    /// <summary>
    /// Animation for the Glide feather sprite.
    /// </summary>
    public readonly Animation glideAnim;

    /// <summary>
    /// Called when a different value is supplied to this.AnimName.
    /// </summary>
    /// <param name="value">New value of <see cref="TrackName"/>.</param>
    private void OnTrackSwitched(string value) {
      if (Main.dedServ) {
        return;
      }

      playerAnim.CheckIfValid(value);
      bashAnim.CheckIfValid(value);
      glideAnim.CheckIfValid(value);
    }

    /// <summary>
    /// Updates the player animation by one frame, and changes it depending on various conditions.
    /// </summary>
    internal void Update() {
      var player = oPlayer.player;
      var abilities = oPlayer.abilities;

      if (oPlayer.Transforming) {
        IncrementFrame(oPlayer.IsOri ? "TransformEnd" : "TransformStart", timeOffset: oPlayer.HasTransformedOnce ? (OriPlayer.RepeatedTransformRate - 1) : 0);
        return;
      }
      if (!oPlayer.IsOri) {
        return;
      }

      if (player.pulley || player.mount.Active) {
        IncrementFrame("Idle");
        return;
      }
      if (oPlayer.abilities.burrow.InUse) {
        float rad = (float)Math.Atan2(abilities.burrow.velocity.X, -abilities.burrow.velocity.Y);
        rad *= player.direction;
        if (player.gravDir < 0) {
          rad += (float)Math.PI;
        }
        IncrementFrame("Burrow", rotation: rad);
        return;
      }
      if (abilities.wallChargeJump.Active) {
        float rad = (float)Math.Atan2(player.velocity.Y, player.velocity.X);
        if (player.direction == -1) {
          rad -= (float)Math.PI;
        }
        IncrementFrame("Dash", overrideFrameIndex: 0, rotation: rad);
        return;
      }
      if (abilities.wallJump.InUse) {
        IncrementFrame("WallJump");
        return;
      }
      if (abilities.airJump.InUse && !(abilities.dash.InUse || abilities.chargeDash.InUse)) {
        IncrementFrame("AirJump", rotation: FrameTime * 0.6f);
        return;
      }
      if (abilities.bash.InUse) {
        IncrementFrame("Bash");
        return;
      }
      if (abilities.stomp.InUse) {
        switch (abilities.stomp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("AirJump", rotation: FrameTime * 0.8f);
            return;
          case Ability.State.Active:
            IncrementFrame("ChargeJump", rotation: MathHelper.ToRadians(180), overrideDuration: 2, overrideLoopmode: LoopMode.Always, overrideDirection: Direction.PingPong);
            return;
        }
      }
      if (abilities.glide.InUse) {
        switch (abilities.glide.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("GlideStart");
            return;
          case Ability.State.Active:
            IncrementFrame("Glide");
            return;
          case Ability.State.Ending:
            IncrementFrame("GlideStart", overrideDirection: Direction.Reverse);
            return;
        }
      }
      if (abilities.climb.InUse) {
        if (abilities.climb.IsCharging) {
          if (!abilities.wallChargeJump.Charged) {
            IncrementFrame("WallChargeJumpCharge", overrideFrameIndex: abilities.wallChargeJump.Refreshed ? -1 : 0);
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
          IncrementFrame("WallChargeJumpAim", overrideFrameIndex: frame);
          return;
        }
        if (Math.Abs(player.velocity.Y) < 0.1f) {
          IncrementFrame("ClimbIdle");
        }
        else {
          IncrementFrame(player.velocity.Y * player.gravDir < 0 ? "Climb" : "WallSlide", timeOffset: Math.Abs(player.velocity.Y) * 0.1f);
        }
        return;
      }
      if (oPlayer.OnWall && !oPlayer.IsGrounded) {
        IncrementFrame("WallSlide");
        return;
      }
      if (abilities.dash.InUse || abilities.chargeDash.InUse) {
        IncrementFrame("Dash", overrideFrameIndex: Math.Abs(player.velocity.X) < 18f ? 2 : -1);
        return;
      }
      if (abilities.lookUp.InUse) {
        switch (abilities.lookUp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("LookUpStart");
            return;
          case Ability.State.Active:
            IncrementFrame("LookUp");
            return;
          case Ability.State.Ending:
            IncrementFrame("LookUpStart", overrideDirection: Direction.Reverse);
            return;
        }
      }
      if (abilities.crouch.InUse) {
        switch (abilities.crouch.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("CrouchStart");
            return;
          case Ability.State.Active:
            IncrementFrame("Crouch");
            return;
          case Ability.State.Ending:
            IncrementFrame("CrouchStart", overrideDirection: Direction.Reverse);
            return;
        }
      }

      if (abilities.chargeJump.Active) {
        IncrementFrame("ChargeJump");
        return;
      }
      if (!oPlayer.IsGrounded) {
        // XOR so opposite signs (negative value) means jumping regardless of gravity
        IncrementFrame(((int)player.velocity.Y ^ (int)player.gravity) <= 0 ? "Jump" : "Falling");
        return;
      }
      if (Math.Abs(player.velocity.X) > 0.2f) {
        IncrementFrame("Running", timeOffset: (int)Math.Abs(player.velocity.X) / 3);
        return;
      }
      IncrementFrame(oPlayer.OnWall ? "IdleAgainst" : "Idle");
      return;
    }

    private string debug_oldtrack;
    /// <summary>
    /// Logic for managing which frame should play.
    /// </summary>
    /// <param name="anim">Name of the animation track to play/continue.</param>
    /// <param name="overrideFrameIndex">Optional override for the frame to play. This prevents normal playback.</param>
    /// <param name="timeOffset">Optional offset to time.</param>
    /// <param name="overrideDuration">Optional override for the duration of the frame.</param>
    /// <param name="overrideDirection">Optional override for the direction the track plays.</param>
    /// <param name="overrideLoopmode">Optional override for how the track loops.</param>
    /// <param name="rotation">Rotation of the sprite, in radians.</param>
    private void IncrementFrame(string anim, int overrideFrameIndex = -1, float timeOffset = 0, int overrideDuration = -1, LoopMode? overrideLoopmode = null, Direction? overrideDirection = null, float rotation = 0) {
      if (string.IsNullOrWhiteSpace(anim)) {
        throw new ArgumentException($"{nameof(anim)} cannot be empty.", nameof(anim));
      }
      
      FrameTime += 1 + timeOffset;
      SpriteRotation = rotation;

      Track track = playerAnim.source[anim];
      LoopMode loop = overrideLoopmode ?? track.loop;
      Direction direction = overrideDirection ?? track.direction;
      IFrame[] frames = track.frames;
      int lastFrame = frames.Length - 1;

      if (anim != TrackName) {
        if (!playerAnim.source.tracks.ContainsKey(anim)) {
          // Bad animation, set defaults and return
          OriMod.Error("BadTrack", args: anim);
          TrackName = "Default";
          Reversed = false;
          return;
        }
        // Track changed: switch to next track
        TrackName = anim;
        track = playerAnim.source[anim];
        frames = track.frames;
        lastFrame = frames.Length - 1;
        Reversed = direction == Direction.Reverse;
        FrameIndex = Reversed ? lastFrame : 0;
        FrameTime = 0;
      }
      
      if (overrideFrameIndex >= 0 && overrideFrameIndex <= lastFrame) {
        // If overrideFrame was specified, simply set frame
        FrameIndex = overrideFrameIndex;
        FrameTime = 0;
      }

      if (oPlayer.IsLocal && oPlayer.debugMode && anim != debug_oldtrack) {
        debug_oldtrack = anim;
        Main.NewText($"Frame called: {TrackName}{(Reversed ? " (Reversed)" : "")}, Time: {FrameTime}, AnimIndex: {FrameIndex}/{playerAnim.ActiveTrack.frames.Length}"); // Debug
      }

      // Increment frames based on time (this should rarely be above 1)
      int duration = overrideDuration != -1 ? overrideDuration : frames[FrameIndex].duration;
      if (FrameTime < duration || duration < 0) {
        return;
      }

      int framesToAdvance = 0;
      while (FrameTime >= duration) {
        FrameTime -= duration;
        framesToAdvance++;
        if (framesToAdvance + FrameIndex > lastFrame) {
          FrameTime %= duration;
        }
      }

      // Loop logic
      switch (direction) {
        case Direction.Forward: {
            Reversed = false;
            if (FrameIndex == lastFrame) {
              // Forward, end of track w/ transfer: transfer to next track
              if (loop == LoopMode.Transfer) {
                TrackName = track.transferTo;
                FrameIndex = 0;
                FrameTime = 0;
              }
              // Forward, end of track, always loop: replay track forward
              else if (loop == LoopMode.Always) {
                FrameIndex = 0;
              }
            }
            // Forward, middle of track: continue playing track forward
            else {
              FrameIndex += framesToAdvance;
            }
            break;
          }
        case Direction.PingPong: {
            // Ping-pong, always loop, reached start of track: play track forward
            if (FrameIndex == 0 && loop == LoopMode.Always) {
              Reversed = false;
              FrameIndex += framesToAdvance;
            }
            // Ping-pong, always loop, reached end of track: play track backwards
            else if (FrameIndex == lastFrame && loop == LoopMode.Always) {
              Reversed = true;
              FrameIndex -= framesToAdvance;
            }
            // Ping-pong, in middle of track: continue playing track either forward or backwards
            else {
              FrameIndex += Reversed ? -framesToAdvance : framesToAdvance;
            }
            break;
          }
        case Direction.Reverse: {
            Reversed = true;
            // Reverse, if loop: replay track backwards
            if (FrameIndex == 0) {
              if (loop == LoopMode.Transfer) {
                TrackName = track.transferTo;
                FrameIndex = 0;
                FrameTime = 0;
              }
              if (loop == LoopMode.Always) {
                FrameIndex = lastFrame;
              }
            }
            // Reverse, middle of track: continue track backwards
            else {
              FrameIndex -= framesToAdvance;
            }
            break;
          }
      }
      FrameIndex = (int)MathHelper.Clamp(FrameIndex, 0, lastFrame);
    }
  }
}
