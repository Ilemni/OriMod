using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OriMod.Animations {
  internal sealed class AnimationHandler : SingleInstance<AnimationHandler> {
    private AnimationHandler() { }

    private static Frame F(int frameX, int frameY, int duration = -1) => new Frame(frameX, frameY, duration);
    private static Header H(InitType i = InitType.Range, LoopMode l = LoopMode.Always, PlaybackMode p = PlaybackMode.None, ReferencedTexture2D texture = null)
      => new Header(init: i, loop: l, playback: p, rtx: texture);

    private AnimationSource _pa;
    public AnimationSource PlayerAnim => _pa ?? (_pa = new AnimationSource("PlayerEffects/OriPlayer", 128, 128,
      new Dictionary<string, Track> {
        ["Default"] = new Track(Header.Default,
          F(0, 0)
        ),
        ["Idle"] = new Track(Header.Default,
          F(0, 1, 9), F(0, 8, 9)
        ),
        ["IdleAgainst"] = new Track(Header.Default,
          F(0, 9, 7), F(0, 14, 7)
        ),
        ["LookUpStart"] = new Track(Header.Default,
          F(1, 0)
        ),
        ["LookUp"] = new Track(Header.Default,
          F(1, 1, 8), F(1, 7, 8)
        ),
        ["CrouchStart"] = new Track(Header.Default,
          F(1, 8)
        ),
        ["Crouch"] = new Track(Header.Default,
          F(1, 9)
        ),
        ["Running"] = new Track(Header.Default,
          F(2, 0, 4), F(2, 10, 4)
        ),
        ["Dash"] = new Track(Header.None,
          F(2, 12, 36), F(2, 13, 12)
        ),
        ["Bash"] = new Track(Header.None,
          F(2, 14, 40), F(2, 13)
        ),
        ["AirJump"] = new Track(Header.Default,
          F(3, 0, 32)
        ),
        ["Jump"] = new Track(H(i: InitType.None, p: PlaybackMode.Reverse),
          F(3, 1), F(3, 2, 14)
        ),
        ["IntoJumpBall"] = new Track(Header.None,
          F(3, 3, 6), F(3, 4, 4)
        ),
        ["ChargeJump"] = new Track(H(l: LoopMode.None, p: PlaybackMode.PingPong),
          F(3, 5, 4), F(3, 8, 4)
        ),
        ["Falling"] = new Track(Header.Default,
          F(3, 9, 4), F(3, 12, 4)
        ),
        ["FallNoAnim"] = new Track(Header.Default,
          F(3, 13)
        ),
        ["GlideStart"] = new Track(H(l: LoopMode.None),
          F(4, 0, 5), F(4, 2, 5)
        ),
        ["GlideIdle"] = new Track(Header.Default,
          F(4, 3)
        ),
        ["Glide"] = new Track(Header.Default,
          F(4, 4, 5), F(4, 9, 5)
        ),
        ["ClimbIdle"] = new Track(Header.Default,
          F(5, 0)
        ),
        ["Climb"] = new Track(Header.Default,
          F(5, 1, 4), F(5, 8, 4)
        ),
        ["WallSlide"] = new Track(Header.Default,
          F(5, 9, 5), F(5, 12, 5)
        ),
        ["WallJump"] = new Track(Header.Default,
          F(5, 15, 12)
        ),
        ["WallChargeJumpCharge"] = new Track(Header.Default,
          F(6, 0, 16), F(6, 1, 10), F(6, 2)
        ),
        ["WallChargeJumpAim"] = new Track(Header.Default,
          F(6, 2), F(6, 6)
        ),
        ["Burrow"] = new Track(H(i: InitType.Range),
          F(7, 0, 3), F(7, 7, 3)
        ),
        ["TransformStart"] = new Track(H(i: InitType.None, l: LoopMode.Transfer, texture: OriTextures.Instance.Transform), // TODO: Migrate TransformStart textures to OriPlayer
          F(0, 0, 2), F(0, 1, 60), F(0, 2, 60), F(0, 3, 120),
          F(0, 4, 40), F(0, 5, 40), F(0, 6, 40), F(0, 7, 30)
        ),
        ["TransformEnd"] = new Track(H(i: InitType.None),
          F(15, 8, 6), F(15, 9, 50), F(15, 10, 6), F(15, 11, 60),
          F(15, 12, 10), F(15, 13, 40), F(15, 14, 3), F(15, 15, 60)
        ),
      })
    );

    private AnimationSource _ba;
    public AnimationSource BashAnim => _ba ?? (_ba = new AnimationSource("PlayerEffects/BashArrow", 152, 20,
      new Dictionary<string, Track> {
        {"Bash", new Track(H(i:InitType.None),
          F(0, 0)
        )}
      }
    ));

    private AnimationSource _ga;
    public AnimationSource GlideAnim => _ga ?? (_ga = new AnimationSource("PlayerEffects/Feather", 128, 128,
      new Dictionary<string, Track> {
        {"GlideStart", new Track(H(l:LoopMode.None),
          F(0, 0, 5), F(0, 2, 5)
        )},
        {"GlideIdle", new Track(H(),
          F(0, 3)
        )},
        {"Glide", new Track(H(),
          F(0, 4, 5), F(0, 9, 5)
        )},
      }
    ));

    // TODO: move to OriPlayer
    private Header OverrideHeader = Header.Default;
    
    internal void IncrementFrame(OriPlayer oPlayer, string anim = "Default", int overrideFrameIndex = 0, float overrideTime = 0, int overrideDur = 0, Header overrideHeader = null, Vector2 drawOffset = new Vector2(), float rotation = 0) {
      if (oPlayer is null) {
        throw new ArgumentNullException(nameof(oPlayer));
      }
      if (string.IsNullOrWhiteSpace(anim)) {
        throw new ArgumentException($"{nameof(anim)} cannot be empty.", nameof(anim));
      }

      Track track = PlayerAnim[anim];
      Frame[] frames = track.Frames;
      float radians = (float)(rotation / 180 * Math.PI);
      
      if (overrideHeader is null) {
        overrideHeader = track.Header;
      }

      if (!PlayerAnim.tracks.ContainsKey(anim)) {
        // Bad animation, set defaults and return
          OriMod.Error("BadTrack", args: anim);
        anim = "Default";
        oPlayer.AnimReversed = false;
        oPlayer.SetFrame(anim, 1, overrideTime, frames[0], radians);
        return;
      }

      // During refactoring, noticed that use of OverrideHeader may cause unwanted visual behavior in multiplayer scenarios, as it is currently a singleton field.
      Header header = track.Header.CopySome(overrideHeader);
      if (anim != oPlayer.AnimName) {
        OverrideHeader = Header.Default;
      }
      if (overrideHeader != Header.None) {
        OverrideHeader = overrideHeader;
        header = overrideHeader;
      }
      if (OverrideHeader != Header.None && anim == oPlayer.AnimName) {
        header = OverrideHeader;
      }

      if (overrideFrameIndex != -1 && overrideFrameIndex < frames.Length) {
        // If overrideFrame was specified, simply set frame
        oPlayer.AnimReversed = header.Playback == PlaybackMode.Reverse;
        oPlayer.SetFrame(anim, overrideFrameIndex, 0, frames[overrideFrameIndex], radians);
        return;
      }
      
      // Figure out the desired frame
      int frameIndex = oPlayer.AnimIndex;
        float time = overrideTime != 0 ? overrideTime : oPlayer.AnimTime;
        Point currFrame = oPlayer.AnimTile;

      // Logic for switching frames
        if (anim == oPlayer.AnimName) {
        // If keeping frame, ensure current frame already exists
        int testFrame = Array.FindIndex(frames, f => f.Tile == currFrame);
          if (testFrame == -1) {
            OriMod.Error("BadFrame", args: new object[] { anim, currFrame });
            frameIndex = header.Playback == PlaybackMode.Reverse ? frames.Length - 1 : 0;
          }
        }
        else {
          frameIndex = header.Playback == PlaybackMode.Reverse ? frames.Length - 1 : 0;
          time = 0;
        }

      // Increment frames based on time
      int framesToAdvance = 0;
        int dur = overrideDur != 0 ? overrideDur : frames[frameIndex].Duration;
        while (time > dur && dur != -1) {
          time -= dur;
          framesToAdvance++;
          if (framesToAdvance + frameIndex > frames.Length - 1) {
            time %= dur;
          }
        }
      
      // Loop logic
        if (framesToAdvance != 0) {
        if (header.Playback == PlaybackMode.None) {
            oPlayer.AnimReversed = false;
            if (frameIndex == frames.Length - 1) {
              if (header.Loop == LoopMode.Transfer) {
                anim = header.TransferTo;
                frameIndex = 0;
                time = 0;
              }
            else if (header.Loop == LoopMode.Always) {
                frameIndex = 0;
              }
            }
            else {
              frameIndex += framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
            }
          }
          else if (header.Playback == PlaybackMode.PingPong) {
          if (frameIndex == 0 && header.Loop == LoopMode.Always) {
              oPlayer.AnimReversed = false;
              frameIndex += framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
            }
          else if (frameIndex == frames.Length - 1 && header.Loop == LoopMode.Always) {
              oPlayer.AnimReversed = true;
              frameIndex -= framesToAdvance;
              if (frameIndex < 0) {
                frameIndex = 0;
              }
            }
            else {
              frameIndex += oPlayer.AnimReversed ? -framesToAdvance : framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
              else if (frameIndex < 0) {
                frameIndex = 0;
              }
            }
          }
          else if (header.Playback == PlaybackMode.Reverse) {
            oPlayer.AnimReversed = true;
            if (frameIndex == 0) {
            if (header.Loop == LoopMode.Always) {
                frameIndex = frames.Length - 1;
              }
            }
            else {
              frameIndex -= framesToAdvance;
              if (frameIndex < 0) {
                frameIndex = 0;
              }
            }
          }
        }
  
      oPlayer.SetFrame(anim, frameIndex, time, frames[frameIndex], radians);
    }
  }
}
