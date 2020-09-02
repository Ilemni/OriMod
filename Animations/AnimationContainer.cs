using System;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;

namespace OriMod.Animations {
  /// <summary>
  /// Container for various <see cref="Animation"/>s and data to be attached to an <see cref="OriPlayer"/>. Manages advancement of frames.
  /// </summary>
  public class AnimationContainer {
    /// <summary>
    /// Creates a new instance of <see cref="AnimationContainer"/> for the given <see cref="OriPlayer"/>.
    /// </summary>
    /// <param name="oPlayer"><see cref="OriPlayer"/> instance the animations will belong to.</param>
    /// <exception cref="InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    internal AnimationContainer(OriPlayer oPlayer) {
      if (Main.netMode == Terraria.ID.NetmodeID.Server) {
        throw new InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
      this.oPlayer = oPlayer;
      playerAnim = new Animation(this, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.PlayerSprite);
      bashAnim = new Animation(this, AnimationHandler.Instance.BashAnim, OriLayers.Instance.BashArrow);
      glideAnim = new Animation(this, AnimationHandler.Instance.GlideAnim, OriLayers.Instance.FeatherSprite);
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
    /// The <see cref="OriPlayer"/> instance this <see cref="AnimationContainer"/> instance belongs to.
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
    /// <param name="value">New value of <see cref="AnimationName"/>.</param>
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
    /// <param name="player"></param>
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
        double rad = Math.Atan2(abilities.burrow.velocity.X, -abilities.burrow.velocity.Y);
        int deg = (int)(rad * (180 / Math.PI));
        deg *= player.direction;
        if (player.gravDir < 0) {
          deg += 180;
        }
        IncrementFrame("Burrow", rotation: deg);
        return;
      }
      if (abilities.wallChargeJump.Active) {
        float rad = (float)Math.Atan2(player.velocity.Y, player.velocity.X);
        float deg = rad * (float)(180 / Math.PI) * player.direction;
        if (player.direction == -1) {
          deg -= 180f;
        }
        IncrementFrame("Dash", overrideFrameIndex: 0, rotation: deg);
        return;
      }
      if (abilities.wallJump.InUse) {
        IncrementFrame("WallJump");
        return;
      }
      if (abilities.airJump.InUse && !(abilities.dash.InUse || abilities.chargeDash.InUse)) {
        IncrementFrame("AirJump");
        SpriteRotation = FrameTime * 0.8f;
        return;
      }
      if (abilities.bash.InUse) {
        IncrementFrame("Bash");
        return;
      }
      if (abilities.stomp.InUse) {
        switch (abilities.stomp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("AirJump");
            SpriteRotation = FrameTime;
            return;
          case Ability.State.Active:
            IncrementFrame("ChargeJump", rotation: 180f, overrideDuration: 2, overrideLoopmode: LoopMode.Always, overrideDirection: Direction.PingPong);
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
        if (Math.Abs(player.velocity.X) > 18f) {
          IncrementFrame("Dash");
        }
        else {
          IncrementFrame("Dash", overrideFrameIndex: 2);
        }
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

    /// <summary>
    /// Logic for managing which frame should play.
    /// </summary>
    /// <param name="oPlayer">Player to animate.</param>
    /// <param name="anim">Name of the animation track to play/continue.</param>
    /// <param name="overrideFrameIndex">Optional override for the frame to play.</param>
    /// <param name="timeOffset">Optional offset to time.</param>
    /// <param name="overrideDuration">Optional override for the duration of the frame.</param>
    /// <param name="overrideHeader">Optional override for the header of the track.</param>
    /// <param name="rotation">Rotation of the sprite, in degrees.</param>
    private void IncrementFrame(string anim, int overrideFrameIndex = -1, float timeOffset = 0, int overrideDuration = -1, LoopMode? overrideLoopmode = null, Direction? overrideDirection = null, float rotation = 0) {
      if (TrackName != null && OriPlayer.Local.debugMode && !oPlayer.IsLocal) {
        //Main.NewText($"Frame called: {AnimationName}, Time: {AnimationTime}, AnimIndex: {AnimationIndex}/{animations.PlayerAnim.ActiveTrack.frames.Length}"); // Debug
      }
      if (string.IsNullOrWhiteSpace(anim)) {
        throw new ArgumentException($"{nameof(anim)} cannot be empty.", nameof(anim));
      }

      FrameTime += 1 + timeOffset;

      Track track = playerAnim.source[anim];
      Frame[] frames = track.frames;
      int lastFrame = frames.Length - 1;
      float radians = (float)(rotation / 180 * Math.PI);
      var loop = overrideLoopmode ?? track.header.loop;
      var direction = overrideDirection ?? track.header.direction;

      if (!playerAnim.source.tracks.ContainsKey(anim)) {
        // Bad animation, set defaults and return
        OriMod.Error("BadTrack", args: anim);
        anim = "Default";
        oPlayer.animations.Reversed = false;
        SetFrame(anim, 0, 0, radians);
        return;
      }

      if (overrideFrameIndex >= 0 && overrideFrameIndex < frames.Length) {
        // If overrideFrame was specified, simply set frame
        oPlayer.animations.Reversed = direction == Direction.Reverse;
        SetFrame(anim, overrideFrameIndex, 0, radians);
        return;
      }

      // Figure out the desired frame
      int frameIndex = oPlayer.animations.FrameIndex;
      float time = oPlayer.animations.FrameTime;

      // Logic for switching tracks
      if (anim != oPlayer.animations.TrackName) {
        // Track changed: switch to next track
        frameIndex = direction == Direction.Reverse ? frames.Length - 1 : 0;
        time = 0;
      }

      if (time > 0) {
        // Increment frames based on time (this should rarely be above 1)
        int framesToAdvance = 0;
        int duration = overrideDuration != -1 ? overrideDuration : frames[frameIndex].Duration;
        while (time > duration && duration != -1) {
          time -= duration;
          framesToAdvance++;
          if (framesToAdvance + frameIndex > lastFrame) {
            time %= duration;
          }
        }

        // Loop logic
        if (framesToAdvance != 0) {
          switch (direction) {
            case Direction.Forward: {
                oPlayer.animations.Reversed = false;
                if (frameIndex == lastFrame) {
                  // Forward, end of track w/ transfer: transfer to next track
                  if (loop == LoopMode.Transfer) {
                    anim = track.header.transferTo;
                    frameIndex = 0;
                    time = 0;
                  }
                  // Forward, end of track, always loop: replay track forward
                  else if (loop == LoopMode.Always) {
                    frameIndex = 0;
                  }
                }
                // Forward, middle of track: continue playing track forward
                else {
                  frameIndex += framesToAdvance;
                }
                break;
              }
            case Direction.PingPong: {
                // Ping-pong, always loop, reached start of track: play track forward
                if (frameIndex == 0 && loop == LoopMode.Always) {
                  oPlayer.animations.Reversed = false;
                  frameIndex += framesToAdvance;
                }
                // Ping-pong, always loop, reached end of track: play track backwards
                else if (frameIndex == lastFrame && loop == LoopMode.Always) {
                  oPlayer.animations.Reversed = true;
                  frameIndex -= framesToAdvance;
                }
                // Ping-pong, in middle of track: continue playing track either forward or backwards
                else {
                  frameIndex += oPlayer.animations.Reversed ? -framesToAdvance : framesToAdvance;
                }
                break;
              }
            case Direction.Reverse: {
                oPlayer.animations.Reversed = true;
                // Reverse, if loop: replay track backwards
                if (frameIndex == 0) {
                  if (loop == LoopMode.Transfer) {
                    anim = track.header.transferTo;
                    frameIndex = 0;
                    time = 0;
                  }
                  if (loop == LoopMode.Always) {
                    frameIndex = lastFrame;
                  }
                }
                // Reverse, middle of track: continue track backwards
                else {
                  frameIndex -= framesToAdvance;
                }
                break;
              }
          }
        }
      }

      frameIndex = (int)MathHelper.Clamp(frameIndex, 0, lastFrame);

      SetFrame(anim, frameIndex, time, radians);
    }

    /// <summary>
    /// Sets current animation frame data.
    /// </summary>
    /// <param name="name">Sets <see cref="AnimationName"/>.</param>
    /// <param name="frameIndex">Sets <see cref="AnimationIndex"/>.</param>
    /// <param name="time">Sets <see cref="AnimationTime"/>.</param>
    /// <param name="animRads">Sets <see cref="AnimationRotation"/>.</param>
    /// 
    private void SetFrame(string name, int frameIndex, float time, float animRads) {
      TrackName = name;
      this.FrameIndex = frameIndex;
      FrameTime = time;
      SpriteRotation = animRads;
    }
  }
}
