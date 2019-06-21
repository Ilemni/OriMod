using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  internal static partial class AnimationHandler {
    
    private static Header OverrideHeader = Header.Default;
    
    internal static void IncrementFrame(OriPlayer oPlayer, string anim="Default", int overrideFrame=0, float overrideTime=0, int overrideDur=0, Header overrideHeader=null, Vector2 drawOffset=new Vector2(), float rotDegrees=0) {
      if (oPlayer == null) return;
      if (overrideHeader == null) overrideHeader = Tracks[anim].Header;
      float rotRads = (float)(rotDegrees / 180 * Math.PI);
      if (!Names.Contains(anim)) {
        if (anim != null && anim.Length > 0) {
          Main.NewText("Error with animation: The animation sequence \"" + anim + "\" does not exist.", Color.Red);
          ErrorLogger.Log("Error with animation: The animation sequence \"" + anim + "\" does not exist.");
        }
        anim = "Default";
        Track track = Tracks[anim];
        oPlayer.AnimReversed = false;
        oPlayer.SetFrame(anim, 1, overrideTime, track.Frames[0], rotRads);
        return;
      }
      Frame[] frames = Tracks[anim].Frames;
      Header header = Tracks[anim].Header.CopySome(overrideHeader); // X is incrementType (no reason to be used in IncrementFrame()), Y is loopMode, Z is playbackMode
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
      if (overrideFrame != 0 && overrideFrame < frames.Length) { // If override frame, just set frame
        newFrame = frames[overrideFrame];
        oPlayer.AnimReversed = header.Playback == PlaybackMode.Reverse;
        oPlayer.SetFrame(anim, overrideFrame, 0, newFrame, rotRads);
      }
      else { // Else actually do work
        int frameIndex = oPlayer.AnimIndex; // frameIndex's lowest value is 1, as frames[0] contains header data for the track
        float time = overrideTime != 0 ? overrideTime : oPlayer.AnimTime;
        Point currFrame = oPlayer.AnimTile;
        
        if (anim == oPlayer.AnimName) {
          int testFrame = Array.FindIndex(frames, f => (f.Tile == currFrame)); // Check if this frame already exists
          if (testFrame == -1) {
            Main.NewText("Invalid frame for \"" + anim + "\": " + currFrame, Color.Red);
            ErrorLogger.Log("Invalid frame for \"" + anim + "\": " + currFrame);
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
            time = time % dur;
          }
        }
        if (framesToAdvance != 0) {
          if (header.Playback == PlaybackMode.Normal) {
            oPlayer.AnimReversed = false;
            if (frameIndex == frames.Length - 1) {
              if (header.Loop != LoopMode.Once) {
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
  }
}