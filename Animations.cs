using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OriMod {
  internal static partial class AnimationHandler {
    private enum incrementType {
      Range = 1, // Animation is between a start and end point, going downwards (Used for track initialization)
      Select = 2, // Animation is a series of explicitly specified frames
      Single = 3  // Animation has exactly one frame. Disregards all other metadata
    }
    private enum loop {
      Once = 1, // Play once and stay on last frame (Not fully functioning, set last frame duration to -1 instead)
      Always = 2, // Continue looping after reaching end of animation
    }
    private enum playbackMode {
      Normal = 1, // Play from start to end, then jump back to start
      PingPong = 2, // Play from start to end, then play back to start
      Reverse = 3, // Playback in reverse
      Random = 4, // Pick a random frame
    }
    // Shorthanding "new Vector3(x, y, z)" to "f(x, y, z)" and giving context to values
    private static Vector3 f(int frameX, int frameY, int duration=-1) { return new Vector3(frameX, frameY, duration); }
    // Shorthanding headers and giving context to values
    private static Vector3 h(incrementType i=incrementType.Range, loop l=loop.Always, playbackMode p=playbackMode.Normal) { return new Vector3((int)i, (int)l, (int)p); }
    
    // First Vector3 in the Vector3[] properties are not frames, headers consisting of these enums
    private static Dictionary<string, Vector3[]> tracks = new Dictionary<string, Vector3[]> {
      {"Default", new Vector3[] {
        h(incrementType.Single),
        f(0, 0)
      }},
      {"FallNoAnim", new Vector3[] {
        h(incrementType.Single),
        f(3, 13)
      }},
      {"Running", new Vector3[] {
        h(),
        f(2, 0, 4), f(2, 10, 4)
      }},
      {"Idle", new Vector3[] {
        h(),
        f(0, 1, 9), f(0, 8, 9)
      }},
      {"Dash", new Vector3[] {
        h(incrementType.Select, loop.Once),
        f(2, 12, 36), f(2, 13, 12)
      }},
      {"Bash", new Vector3[] {
        h(incrementType.Select, loop.Once),
        f(2, 14, 40), f(2, 13)
      }},
      {"CrouchStart", new Vector3[] {
        h(incrementType.Single),
        f(1, 8)
      }},
      {"Crouch", new Vector3[] {
        h(incrementType.Single),
        f(1, 9)
      }},
      {"WallJump", new Vector3[] {
        h(incrementType.Single),
        f(5, 15, 12)
      }},
      {"AirJump", new Vector3[] {
        h(incrementType.Single),
        f(3, 0)
      }},
      {"ChargeJump", new Vector3[] {
        h(l:loop.Once, p:playbackMode.PingPong),
        f(3, 5, 4), f(3, 8, 4)
      }},
      {"Falling", new Vector3[] {
        h(),
        f(3, 9, 4), f(3, 12, 4)
      }},
      {"ClimbIdle", new Vector3[] {
        h(incrementType.Single),
        f(5, 0)
      }},
      {"Climb", new Vector3[] {
        h(),
        f(5, 1, 4), f(5, 8, 4)
      }},
      {"WallSlide", new Vector3[] {
        h(),
        f(5, 9, 5), f(5, 12, 5)
      }},
      {"IntoJumpBall", new Vector3[] {
        h(incrementType.Select, loop.Once),
        f(3, 3, 6), f(3, 4, 4)
      }},
      {"IdleAgainst", new Vector3[] {
        h(),
        f(0, 9, 7), f(0, 14, 7)
      }},
      {"Jump", new Vector3[] {
        h(incrementType.Select, p:playbackMode.Reverse),
        f(3, 1), f(3, 2, 14)
      }},
      {"GlideStart", new Vector3[] {
        h(l:loop.Once),
        f(4, 0, 5), f(4, 2, 5)
      }},
      {"GlideIdle", new Vector3[] {
        h(incrementType.Single),
        f(4, 3)
      }},
      {"Glide", new Vector3[] {
        h(),
        f(4, 4, 5), f(4, 9, 5)
      }},
      {"LookUpStart", new Vector3[] { 
        h(incrementType.Single),
        f(1, 0)
      }},
      {"LookUp", new Vector3[] { 
        h(),
        f(1, 1, 8), f(1, 7, 8)
      }},
      {"TransformEnd", new Vector3[] {
        h(incrementType.Select),
        f(15, 8, 6), f(15, 9, 50), f(15, 10, 6), f(15, 11, 60),
        f(15, 12, 10), f(15, 13, 40), f(15, 14, 3), f(15, 15, 60)
      }},
      {"Burrow", new Vector3[] {
        h(incrementType.Range),
        f(7, 0, 3), f(7, 7, 3)
      }},
    };
    
    private static string[] Names = tracks.Keys.ToArray();
  }
}