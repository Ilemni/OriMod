using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

namespace OriMod {
  public static partial class AnimationHandler {
    
    private static Vector3 OverrideMeta = new Vector3();
    public static Vector3[] GetTrack(string str) {
      Vector3[] value;
      bool b = tracks.TryGetValue(str, out value);
      return b ? value : null;
    }
    
    // Set oPlayer and autofill tracks set to Range
    public static void Init() {
      List<Vector3[]> newTracks = new List<Vector3[]>();
      string[] keys = tracks.Keys.ToArray();
      foreach(string key in keys) {
        if ((int)tracks[key][0].X != (int)incrementType.Range) {
          newTracks.Add(tracks[key]);
          continue;
        }
        List<Vector3> newTrack = new List<Vector3>();
        List<Vector3> track = tracks[key].ToList();
        newTrack.Add(track[0]);
        Vector3[] frames = track.Skip(1).ToArray();
        for (int i = 0; i < frames.Length - 1; i++) {
          Vector3 startFrame = frames[i];
          Vector3 endFrame = frames[i + 1];
          for (int y = (int)startFrame.Y; y < (int)endFrame.Y; y++) {
            newTrack.Add(new Vector3(startFrame.X, y, startFrame.Z));
          }
        }
        newTrack.Add(frames[frames.Length - 1]);
        newTracks.Add(newTrack.ToArray());
      }
      for (int k = 0; k < keys.Length; k++) {
        if ((int)(newTracks[k][0].X) == (int)incrementType.Range) {
          tracks[keys[k]] = newTracks[k];
        }
      }
    }
    public static void IncrementFrame(OriPlayer oPlayer, string anim="Default", int overrideFrame=0, float overrideTime=0, int overrideDur=0, Vector3 overrideMeta=new Vector3(), Vector2 drawOffset=new Vector2(), float rotDegrees=0) {
      if (oPlayer == null || oPlayer.player.whoAmI != Main.myPlayer) {
        return;
      }
      if (!names.Contains(anim)) {
        Main.NewText("Error with animation: The animation sequence \"" + anim + "\" does not exist.", Color.Red);
        anim = "Default";
        Vector3[] fr = tracks[anim];
        oPlayer.AnimReversed = false;
        oPlayer.SetFrame(anim, 1, overrideTime, fr[1]);
        return;
      }
      // Main.NewText("Active animation: " + anim + " frame " + oPlayer.currFrameIndex + " [" + oPlayer.currFrameTime + "]"); // Debug only
      Vector3[] frames = tracks[anim];
      Vector3 meta = new Vector3(
        overrideMeta.X != 0 ? overrideMeta.X : frames[0].X,
        overrideMeta.Y != 0 ? overrideMeta.Y : frames[0].Y,
        overrideMeta.Z != 0 ? overrideMeta.Z : frames[0].Z
      ); // X is incrementType (no reason to be used in IncrementFrame()), Y is loopMode, Z is playbackMode
      if (anim != oPlayer.AnimName) {
        OverrideMeta = Vector3.Zero;
      }
      if (overrideMeta != Vector3.Zero) {
        OverrideMeta = overrideMeta;
        meta = overrideMeta;
      }
      if (OverrideMeta != Vector3.Zero && anim == oPlayer.AnimName) {
        meta = OverrideMeta;
      }
      Vector3 newFrame;
      if (overrideFrame != 0 && overrideFrame < frames.Length) { // If override frame, just set frame
        newFrame = frames[overrideFrame];
        oPlayer.AnimReversed = (int)meta.Z == (int)playbackMode.Reverse;
        oPlayer.SetFrame(anim, overrideFrame, 0, newFrame);
      }
      else if ((int)meta.Z == (int)playbackMode.Random) { // If random, just set frame to random frame
        int rand = (int)Main.rand.Next(frames.Length - 1) + 1;
        newFrame = frames[rand];
        oPlayer.SetFrame(anim, rand, overrideTime, newFrame);
      }
      else { // Else actually do work
        int frameIndex = oPlayer.AnimIndex; // frameIndex's lowest value is 1, as frames[0] contains header data for the track
        float time = overrideTime != 0 ? overrideTime : oPlayer.AnimTime;
        Vector2 currFrame = oPlayer.AnimTile;
        
        if (anim == oPlayer.AnimName) {
          int testFrame = Array.FindIndex(frames.Skip(1).ToArray(), f => (f.X == currFrame.X && f.Y == currFrame.Y)); // Check if this frame already exists
          if (testFrame == -1) {
            Main.NewText("Invalid frame for \"" + anim + "\": " + currFrame, Color.Red);
            frameIndex = meta.Z == 3 ? frames.Length - 1 : 1;
          }
        }
        else {
          frameIndex = meta.Z == 3 ? frames.Length - 1 : 1;
          time = 0;
        }
        int dur = overrideDur != 0 ? overrideDur : (int)frames[frameIndex].Z;
        int framesToAdvance = 0;
        while (time > dur && dur != -1) {
          time -= dur;
          framesToAdvance++;
          if (framesToAdvance + frameIndex > frames.Length - 1) {
            time = time % dur;
          }
        }
        if (framesToAdvance != 0) {
          if ((int)meta.Z == (int)playbackMode.Normal) {
            oPlayer.AnimReversed = false;
            if (frameIndex == frames.Length - 1) {
              if (meta.Y != (int)loop.Once) {
                frameIndex = 1;
              }
            }
            else {
              frameIndex += framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
            }
          }
          else if ((int)meta.Z == (int)playbackMode.PingPong) {
            if (frameIndex == 1 && (int)meta.Y != (int)loop.Once) {
              oPlayer.AnimReversed = false;
              frameIndex += framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
            }
            else if (frameIndex == frames.Length - 1 && (int)meta.Y != (int)loop.Once) {
              oPlayer.AnimReversed = true;
              frameIndex -= framesToAdvance;
              if (frameIndex < 1) {
                frameIndex = 1;
              }
            }
            else {
              frameIndex += oPlayer.AnimReversed ? -framesToAdvance : framesToAdvance;
              if (frameIndex > frames.Length - 1) {
                frameIndex = frames.Length - 1;
              }
              else if (frameIndex < 1) {
                frameIndex = 1;
              }
            }
          }
          else if ((int)meta.Z == (int)playbackMode.Reverse) {
            oPlayer.AnimReversed = true;
            if (frameIndex == 1) {
              if (meta.Y != (int)loop.Once) {
                frameIndex = frames.Length - 1;
              }
            }
            else {
              frameIndex -= framesToAdvance;
              if (frameIndex < 1) {
                frameIndex = 1;
              }
            }
          }
        }
        newFrame = frames[frameIndex];
        oPlayer.SetFrame(anim, frameIndex, time, newFrame);
        oPlayer.AnimRads = (float)(rotDegrees / 180 * Math.PI);
      }
    }
  }
}