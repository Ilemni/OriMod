using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OriMod.Animations {
  internal static class AnimationHandler {
    private static Frame F(int frameX, int frameY, int duration = -1) => new Frame(frameX, frameY, duration);
    private static Header H(InitType i = InitType.Range, LoopMode l = LoopMode.Always, PlaybackMode p = PlaybackMode.Normal, ReferencedTexture2D texture = null)
      => new Header(init: i, loop: l, playback: p, rtx: texture);

    private static AnimationSource _pa;
    internal static AnimationSource PlayerAnim => _pa ?? (_pa = new AnimationSource("PlayerEffects/OriPlayer", 128, 128,
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
        ["Dash"] = new Track(H(i: InitType.Select, l: LoopMode.Once),
          F(2, 12, 36), F(2, 13, 12)
        ),
        ["Bash"] = new Track(H(i: InitType.Select, l: LoopMode.Once),
          F(2, 14, 40), F(2, 13)
        ),
        ["AirJump"] = new Track(Header.Default,
          F(3, 0, 32)
        ),
        ["Jump"] = new Track(H(i: InitType.Select, p: PlaybackMode.Reverse),
          F(3, 1), F(3, 2, 14)
        ),
        ["IntoJumpBall"] = new Track(H(i: InitType.Select, l: LoopMode.Once),
          F(3, 3, 6), F(3, 4, 4)
        ),
        ["ChargeJump"] = new Track(H(l: LoopMode.Once, p: PlaybackMode.PingPong),
          F(3, 5, 4), F(3, 8, 4)
        ),
        ["Falling"] = new Track(Header.Default,
          F(3, 9, 4), F(3, 12, 4)
        ),
        ["FallNoAnim"] = new Track(Header.Default,
          F(3, 13)
        ),
        ["GlideStart"] = new Track(H(l: LoopMode.Once),
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
        ["TransformStart"] = new Track(H(i: InitType.Select, l: LoopMode.Transfer, texture: OriTextures.Instance.Transform), // TODO: Migrate TransformStart textures to OriPlayer
          F(0, 0, 2), F(0, 1, 60), F(0, 2, 60), F(0, 3, 120),
          F(0, 4, 40), F(0, 5, 40), F(0, 6, 40), F(0, 7, 30)
        ),
        ["TransformEnd"] = new Track(H(i: InitType.Select),
          F(15, 8, 6), F(15, 9, 50), F(15, 10, 6), F(15, 11, 60),
          F(15, 12, 10), F(15, 13, 40), F(15, 14, 3), F(15, 15, 60)
        ),
      })
    );

    private static AnimationSource _ba;
    internal static AnimationSource BashAnim => _ba ?? (_ba = new AnimationSource("PlayerEffects/BashArrow", 152, 20,
      new Dictionary<string, Track> {
        {"Bash", new Track(H(i:InitType.Select),
          F(0, 0)
        )}
      }
    ));

    private static AnimationSource _ga;
    internal static AnimationSource GlideAnim => _ga ?? (_ga = new AnimationSource("PlayerEffects/Feather", 128, 128,
      new Dictionary<string, Track> {
        {"GlideStart", new Track(H(l:LoopMode.Once),
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

    private static Header OverrideHeader {
      get => _oh ?? (_oh = Header.Default);
      set => _oh = value;
    }
    private static Header _oh;
    
    internal static void IncrementFrame(OriPlayer oPlayer, string anim = "Default", int overrideFrame = 0, float overrideTime = 0, int overrideDur = 0, Header overrideHeader = null, Vector2 drawOffset = new Vector2(), float rotDegrees = 0) {
      if (oPlayer is null) {
        return;
      }

      if (overrideHeader is null) {
        overrideHeader = PlayerAnim[anim].Header;
      }

      float rotRads = (float)(rotDegrees / 180 * Math.PI);
      if (!PlayerAnim.TrackNames.Contains(anim)) {
        if (anim != null && anim.Length > 0) {
          OriMod.Error("BadTrack", args: anim);
        }
        anim = "Default";
        Track track = PlayerAnim[anim];
        oPlayer.AnimReversed = false;
        oPlayer.SetFrame(anim, 1, overrideTime, track.Frames[0], rotRads);
        return;
      }
      Frame[] frames = PlayerAnim[anim].Frames;
      Header header = PlayerAnim[anim].Header.CopySome(overrideHeader); // X is incrementType (no reason to be used in IncrementFrame()), Y is loopMode, Z is playbackMode
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
      Frame newFrame;
      if (overrideFrame != -1 && overrideFrame < frames.Length) { // If override frame, just set frame
        newFrame = frames[overrideFrame];
        oPlayer.AnimReversed = header.Playback == PlaybackMode.Reverse;
        oPlayer.SetFrame(anim, overrideFrame, 0, newFrame, rotRads);
      }
      else { // Else actually do work
        int frameIndex = oPlayer.AnimIndex; // frameIndex's lowest value is 1, as frames[0] contains header data for the track
        float time = overrideTime != 0 ? overrideTime : oPlayer.AnimTime;
        Point currFrame = oPlayer.AnimTile;

        if (anim == oPlayer.AnimName) {
          int testFrame = Array.FindIndex(frames, f => f.Tile == currFrame); // Check if this frame already exists
          if (testFrame == -1) {
            OriMod.Error("BadFrame", args: new object[] { anim, currFrame });
            frameIndex = header.Playback == PlaybackMode.Reverse ? frames.Length - 1 : 0;
          }
        }
        else {
          frameIndex = header.Playback == PlaybackMode.Reverse ? frames.Length - 1 : 0;
          time = 0;
        }
        int dur = overrideDur != 0 ? overrideDur : frames[frameIndex].Duration;
        int framesToAdvance = 0;
        while (time > dur && dur != -1) {
          time -= dur;
          framesToAdvance++;
          if (framesToAdvance + frameIndex > frames.Length - 1) {
            time %= dur;
          }
        }
        if (framesToAdvance != 0) {
          if (header.Playback == PlaybackMode.Normal) {
            oPlayer.AnimReversed = false;
            if (frameIndex == frames.Length - 1) {
              if (header.Loop == LoopMode.Transfer) {
                anim = header.TransferTo;
                frameIndex = 0;
                time = 0;
              }
              else if (header.Loop != LoopMode.Once) {
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
            if (frameIndex == 0 && header.Loop != LoopMode.Once) {
              oPlayer.AnimReversed = false;
              frameIndex += framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
            }
            else if (frameIndex == frames.Length - 1 && header.Loop != LoopMode.Once) {
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
              if (header.Loop != LoopMode.Once) {
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
        newFrame = frames[frameIndex];
        oPlayer.SetFrame(anim, frameIndex, time, newFrame, rotRads);
      }
    }
  
    public static void Unload() {
      _pa = null;
      _ba = null;
      _ga = null;
      _oh = null;
    }
  }
}
