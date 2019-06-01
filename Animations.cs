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
        f(0, 1)
      }},
      {"Running", new Vector3[] {
        h(),
        f(0, 2, 4), f(0, 12, 4)
      }},
      {"Idle", new Vector3[] {
        h(),
        f(0, 13, 9), f(0, 20, 9)
      }},
      {"Dash", new Vector3[] {
        h(incrementType.Select, loop.Once),
        f(1, 19, 36), f(0, 21, 12)
      }},
      {"Bash", new Vector3[] {
        h(incrementType.Select, loop.Once),
        f(0, 22, 40), f(0, 20)
      }},
      {"CrouchStart", new Vector3[] {
        h(incrementType.Single),
        f(0, 24)
      }},
      {"Crouch", new Vector3[] {
        h(incrementType.Single),
        f(0, 25)
      }},
      {"WallJump", new Vector3[] {
        h(incrementType.Single),
        f(1, 0, 12)
      }},
      {"AirJump", new Vector3[] {
        h(incrementType.Single),
        f(1, 1)
      }},
      {"ChargeJump", new Vector3[] {
        h(l:loop.Once, p:playbackMode.PingPong),
        f(1, 2, 4), f(1, 5, 4)
      }},
      {"Falling", new Vector3[] {
        h(),
        f(1, 6, 4), f(1, 9, 4)
      }},
      {"ClimbIdle", new Vector3[] {
        h(incrementType.Single),
        f(1, 11)
      }},
      {"Climb", new Vector3[] {
        h(),
        f(1, 11, 4), f(1, 18, 4)
      }},
      {"WallSlide", new Vector3[] {
        h(),
        f(1, 20, 5), f(1, 23, 5)
      }},
      {"IntoJumpBall", new Vector3[] {
        h(incrementType.Select, loop.Once),
        f(1, 24, 6), f(1, 25, 4)
      }},
      {"IdleAgainst", new Vector3[] {
        h(),
        f(2, 0, 7), f(2, 6, 7)
      }},
      {"Jump", new Vector3[] {
        h(incrementType.Select, p:playbackMode.Reverse),
        f(2, 8), f(2, 9, 14)
      }},
      {"GlideStart", new Vector3[] {
        h(l:loop.Once),
        f(3, 0, 5), f(3, 2, 5)
      }},
      {"GlideIdle", new Vector3[] {
        h(incrementType.Single),
        f(3, 3)
      }},
      {"Glide", new Vector3[] {
        h(),
        f(3, 4, 5), f(3, 9, 5)
      }},
      {"LookUpStart", new Vector3[] { 
        h(incrementType.Single),
        f(3, 10)
      }},
      {"LookUp", new Vector3[] { 
        h(),
        f(3, 11, 8), f(3, 17, 8)
      }},
      {"TransformEnd", new Vector3[] {
        h(incrementType.Select),
        f(3, 18, 6), f(3, 19, 50), f(3, 20, 6), f(3, 21, 60),
        f(3, 22, 10), f(3, 23, 40), f(3, 24, 3), f(3, 25, 60)
      }},
    };
    
    private static string[] Names = tracks.Keys.ToArray();
  }
}