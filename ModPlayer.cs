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

namespace OriMod
{
  public class OriPlayer : ModPlayer
  {
    #region Variables
    //OriSet, detecting whether or not Ori is active or not. The name is a remnant of when Ori was activated using the accessory located in Items/Ori/OriAccessory
    public bool OriSet = false;
    public bool OriSetPrevious = false;
    // public bool OriItemOn = false;

    //Variables relating to fixing movement when Ori is active, such that you aren't slowed down mid-air after bashing.
    public bool playerGrounded = false;
    // public bool gravityApplied = true;
    public bool unrestrictedMovement = false;

    //Variables relating to Bash
    public int bashActivate = 0;
    public int bashActiveTimer = 0;
    public float bashAngle = 0;
    public float bashDistance = 20f;
    public bool bashActive = false;
    public bool bashFrameUp = false;
    public float bashNPCAngle = 0;
    public Vector2 bashPosition = new Vector2(0, 0);
    public bool abilityBash = true;
    // public bool bashedNotGrounded = false;
    public bool countering = false;
    public int counterTimer = 0;
    public List<int> counterBashed = new List<int> {
      NPCID.BlazingWheel, NPCID.SpikeBall
    };
    public int bashNPC = 0;
    public bool tempInvincibility = false;
    public int immuneTimer = 0;
    public Vector2 bashNPCPosition = Vector2.Zero;

    //Variables relating to Air Jumping
    public int oriAirJumps = 2;
    public int jumpsAvailable = 2;
    public int oriDoubleJumpAnimTimer = 0;
    public bool oriDoubleJumpActive = false;

    //Variables relating to Dashing
    public bool oriDashing = false;
    public int dashTimer = 0;
    public int dashDirection = 0;
    public int dashAnimation = 0;
    // public bool playerTouchingWall = false;
    public bool canAirDash = true;
    public bool abilityDash = true;
    public bool abilityAirDash = true;
    public int dashDelay = 0;
    public bool chargedash = false;

    //Variables relating to Wall Jumping
    public bool abilityWallJump = true;
    public bool onWall = false;
    // public bool enterWall = false;
    public int wallJumpAnimTimer = 0;
    public bool wallJumped = false;

    //Variables relating to Wall Sliding
    public bool jumpingOnToWall = false;
    public bool jumpUpWall = false;
    public int slideTimer = 0;
    public bool slideAnimate = false;
    public bool groundedOnWall = false;
    public bool doClimbAnimation = false;

    //Variables relating to Climbing
    public bool canClimb = true;
    public float climbTimer = 0;
    public bool climbAnimation = false; //also used by the initial "climb" when jumping at a wall
    public int climbFrame = 0;

    //Variables relating to Charge Jumping
    public bool charged = false;
    public int chargeTimer = 0;
    public int chargeUpTimer = 40;
    public bool abilityChargeJump = true;
    public int chargeJumpAnimTimer = 0;
    public bool upRefresh = false;

    //Variables relating to looking up
    public bool intoLookUp = false;
    public int intoLookUpTimer = 0;
    public bool lookUp = false;
    public int lookUpTimer = 0;
    public bool outLookUp = false;
    public int outLookUpTimer = 0;

    //Variables relating to Stomping
    // public bool canStomp = true;
    public bool intoStomp = false;
    public bool stomping = false;
    public bool outOfStomp = false;
    public int intoStompTimer = 0;
    public int stompingTimer = 0;
    public int outOfStompTimer = 0;
    public float preHeight = 0;
    public int stompHitboxTimer = 0;

    //Variables relating to Swimming
    public bool isSwimming = false;
    // public int boostTimer = 0;
    public bool waterBreath = false;

    //Variables relating to Crouching
    public bool crouching = false;
    public bool intoCrouch = false;
    public int intoCrouchTimer = 0;
    public bool outCrouch = false;
    public int outCrouchTimer = 0;

    //Variables relating to Back Flipping
    public bool backflipping = false;
    // public int backflipTimer = 0;

    //Variables relating to Kuro's Feather
    public bool hasFeather = true;
    public int glideAnimTimer = 0;

    //Variables relating to visual or audible effects
    public bool oriJumpActive = false;
    public int oriFallingAnimTimer = 0;
    public bool oriCanStep = true;
    // public int oriDamageFlash = 0;
    public bool OriFlashing = false;
    public int flashTimer = 0;
    public bool oriDeathParticles = true;
    // public int oriDirection = 1;
    public string floorMaterial = "Grass";
    public string wallMaterial = "Grass";
    // public bool onEdge = false;
    public int frameX = 0;
    public bool frameXset = false;
    public int groundedWallTimer = 0;
    public float walkFrameCounter = 0;
    // public int twoFrame = 0;
    public int walkFrame = 6;
    public float rotation = 0f;
    public float rotRads = 0f;
    public float rotSpeed = 0.25f;
    public bool animRefreshed = false;

    //Variables relating to Sein
    public bool seinMinionActive;
    public int seinMinionUpgrade;
    public bool seinActive = false; //SET THIS TO TRUE TO ENABLE SEIN I DEACTIVATED IT BECAUSE SEIN IS BUGGY
    public Vector2 seinPosition = Vector2.Zero;
    public List<Vector2> seinPing = new List<Vector2> { };
    public int seinPingTimer = 30;
    public bool seinStartMoving = true;
    // public Vector2 seinFrame = Vector2.Zero;
    public float seinMovementAngle = 0;
    public bool seinMovedRecently = false;
    
    readonly int spriteWidth = 104;
    readonly int spriteHeight = 76;
    // public int seinOutsideRange = 0;
    //Spirit Flame
    // public int flameShots = 3;
    // public int betweenShotTimer = 0;
    // public int flameLevel = 6;
    // public int flameDownTimer = 0;
    //These weren't put in use while I was actively working on it.

    private static int[] flashPattern = new int[] {
      53,52,51,50,45,
      44,43,38,37,36,
      31,30,29,24,23,
      22,17,16,15,10,
       9, 8, 3, 2, 1
    };
    public Vector2 OriFrame = Vector2.Zero;

    //Variables relating to Transforming
    public int transformTimer = 0;
    public bool transforming = false;
    public Vector2 blockLocation = Vector2.Zero;
    public int transformDirection = 1;
    public bool animatedTransform = false;

    //Footstep materials
    public List<int> grassFloorMaterials;
    public List<int> lightDarkFloorMaterials;
    public List<int> mushroomFloorMaterials;
    public List<int> rockFloorMaterials;
    public List<int> sandFloorMaterials;
    public List<int> snowFloorMaterials;
    public List<int> spiritTreeRockFloorMaterials;
    public List<int> spiritTreeWoodFloorMaterials;
    public List<int> woodFloorMaterials;

    //Wall materials
    public List<int> grassWallMaterials;
    public List<int> lightDarkWallMaterials;
    public List<int> mushroomWallMaterials;
    public List<int> rockWallMaterials;
    public List<int> woodWallMaterials;

    //Trail variables, for the trails Ori creates
    public List<Vector2> trailPos;
    public List<Vector2> trailFrame;
    public List<float> trailAlpha;
    public List<float> trailRotation;
    public List<int> trailDirection;
    public int trailUpdate = 0;

    public int featherTrailTimer = 0;
    private bool animPlayingInReverse = false;
    public string currAnimName = "Default";
    public int currFrameIndex = 0;
    public int currFrameTime = 0;

    #endregion

    //basic sound playing method, with paths starting after NewSFX in the file structure
    public SoundEffectInstance PlayNewSound(string Path) {
      return Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, ("Sounds/Custom/NewSFX/" + Path)), player.Center);
    }
    public SoundEffectInstance PlayNewSoundVolume(string Path, float Volume) {
      return Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, ("Sounds/Custom/NewSFX/" + Path)).WithVolume(Volume), player.Center);
    }
    public SoundEffectInstance PlayNewSoundPlus(string Path, float Volume, float Pitch)
    {
      return Main.PlaySound((int)SoundType.Custom, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/" + Path), Volume, Pitch);
    }

    public void Counter() {
      countering = true;
      counterTimer = 15;
      PlayNewSound("ori/Grenade/seinGrenadeExplode" + RandomChar(2));
    }

    private float DegreeToRadian(float angle) { // i dont know why i put this here
      if (angle > 180) {
        angle -= 180;
      }
      return (float)(Math.PI * angle) / 180.0f;
    }

    public static char RandomChar(int length) { // Returns random letter based on length. Primarily used for sound effects
      char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
      return alphabet[Main.rand.Next(length)];
    }
    
    // Used in multiplayer
    public void SetFrame(int X, int Y) {
      SetFrame(new Vector2(X, Y));
    }
    public void SetFrame(Vector2 frame) {
      OriFrame = frame;
    }

    public Vector2[][] frames = new Vector2[][] {

    };

    // Class with all necessary frame info, should make frame work much more managable
    // As of this commit, this class is not yet implemented
    internal static class FrameHandler {
      // First Vector3 in the Vector3[] properties are not frames, but either of these enums
      private enum incrementType {
        RangeX = 1, // Animation is between a start and end point, going downwards
        RangeY = 2, // Animation is between a start and end point, going right
        Select = 3, // Animation is a series of explicitly specified frames
        Single = 4  // Animation has exactly one frame. Disregards all other metadata
      }
      private enum loopMode {
        Once = 1, // Play once and stay on last frame
        Loop = 2, // Continue looping after reaching end of animation
      }
      private enum playbackMode {
        Normal = 1, // Play from start to end, then jump back to start
        PingPong = 2, // Play from start to end, then play back to start
        Reverse = 3, // Playback in reverse
        Random = 4, // Pick a random frame
      }
      // Shorthanding "new Vector3(x, y, z)" to "f(x, y, z)" and giving context to values
      private static Vector3 f(int frameX, int frameY, int duration=-1) { return new Vector3(frameX, frameY, duration); }
      // Shorthanding metadata
      private static Vector3 m(incrementType i=incrementType.RangeY, loopMode l=loopMode.Loop, playbackMode p=playbackMode.Normal) { return new Vector3((int)i, (int)l, (int)p); }
      
      public static Vector3[] Default   = new Vector3[] {
        m(incrementType.Single), f(0, 0)
      };
      public static Vector3[] FallNoAnim = new Vector3[] {
        m(incrementType.Single), f(0, 1)
      };
      public static Vector3[] Running = new Vector3[] {
        m(),
        f(0, 2, 4), f(0, 12, 4)
      };
      public static Vector3[] Idle = new Vector3[] {
        m(),
        f(0, 13, 6), f(0, 20, 6)
      };
      public static Vector3[] Dash = new Vector3[] {
        m(incrementType.Select, loopMode.Once),
        f(1, 19, 36), f(0, 21, 12)
      };
      public static Vector3[] Bash = new Vector3[] {
        m(l:loopMode.Once),
        f(0, 22, 5), f(0, 23)
      };
      public static Vector3[] Crouch = new Vector3[] {
        m(l:loopMode.Once),
        f(0, 24, 12), f(0, 25)
      };
      public static Vector3[] WallJump = new Vector3[] {
        m(incrementType.Single), f(1, 0, 12)
      };
      public static Vector3[] AirJump = new Vector3[] {
        m(incrementType.Single), f(1, 1)
      };
      public static Vector3[] ChargeJump = new Vector3[] {
        m(l:loopMode.Once, p:playbackMode.PingPong),
        f(1, 2, 4), f(1, 5, 4)
      };
      public static Vector3[] Falling = new Vector3[] {
        m(),
        f(1, 6, 4), f(1, 9, 4)
      };
      public static Vector3[] ClimbIdle = new Vector3[] {
        m(incrementType.Single), f(1, 10)
      };
      public static Vector3[] Climb = new Vector3[] {
        m(),
        f(1, 11, 4), f(1, 18, 4)
      };
      public static Vector3[] SlideDownWall = new Vector3[] {
        m(),
        f(1, 20, 5), f(1, 23, 5)
      };
      public static Vector3[] IntoJumpBall = new Vector3[] {
        m(l:loopMode.Once),
        f(1, 24, 6), f(1, 25, 4)
      };
      public static Vector3[] IdleAgainst = new Vector3[] {
        m(),
        f(2, 0, 6), f(2, 6, 6)
      };
      public static Vector3[] GlideStart = new Vector3[] {
        m(l:loopMode.Once),
        f(3, 0, 4), f(3, 2, 4)
      };
      public static Vector3[] GlideIdle = new Vector3[] {
        m(incrementType.Single), f(3, 3)
      };
      public static Vector3[] Glide = new Vector3[] {
        m(),
        f(3, 4, 4), f(3, 9, 4)
      };
      public static Vector3[] LookUpStart = new Vector3[] { 
        m(incrementType.Single), f(3, 10)
      };
      public static Vector3[] LookUp = new Vector3[] { 
        m(),
        f(3, 11, 8), f(3, 17, 8)
      };
      public static Vector3[] TransformEnd = new Vector3[] { f((int)incrementType.Select, 0, 0),
        f(3, 18, 6), f(3, 19, 50), f(3, 20, 6), f(3, 21, 60),
        f(3, 22, 10), f(3, 23, 40), f(3, 24, 3), f(3, 25, 60)
      };
      
      private static string[] getAnimNames() {
        List<FieldInfo> fields = typeof(FrameHandler).GetFields().ToList();
        fields = fields.FindAll(field => field.GetValue(field).GetType() == typeof(Vector3[]));
        String[] names = fields.Select(field => field.Name).ToArray();
        ErrorLogger.Log("Valid animation names: " + String.Join(", ", names));
        return names;
      }
      private static string[] AnimNames = getAnimNames();
      private static OriPlayer oPlayer;
      private static Vector3[] StrToAnim(string str) {
        if (!AnimNames.Contains(str)) {
          Main.NewText("Error with animation: The animation sequence \"" + str + "\" does not exist.");
          return new Vector3[0];
        }
        Vector3[] frames = (Vector3[])typeof(FrameHandler).GetField(str).GetValue(null);
        return frames;
      }
      
      public static void Init(OriPlayer oriPlayer) {
        oPlayer = oriPlayer;
        
        Array.ForEach(AnimNames, name => {
          Vector3[] frames = StrToAnim(name);
          Vector3 meta = frames[0];
          if ((int)meta.X == (int)incrementType.RangeX || (int)meta.X == (int)incrementType.RangeY) {
            Vector3[] oldFrames = frames.Skip(1).ToArray();
            List<Vector3> newFrames = new List<Vector3>();
            for (int i = 0; i < oldFrames.Length - 1; i++) {
              Vector3 startFrame = oldFrames[i];
              Vector3 endFrame = oldFrames[i + 1];
              if (startFrame.X != endFrame.X) {
                Main.NewText("Warning: Sprite animation along the X axis is not supported. [" + name + " frame " + i + "]");
              }
              // TODO: As of this commit, this part of code is unfinished and is in the process of being worked on
            }
          }
        });
      }
      
      public static void IncrementFrame(string anim="Default", int overrideFrame=0, Vector3 overrideMeta=new Vector3(), Vector2 drawOffset=new Vector2()) {
        if (!AnimNames.Contains(anim)) {
          Main.NewText("Error with animation: The animation sequence \"" + anim + "\" does not exist.");
          return;
        }
        
        Vector3[] frames = StrToAnim(anim);
        Vector3 meta = new Vector3(
          overrideMeta.X != 0 ? overrideMeta.X : frames[0].X,
          overrideMeta.Y != 0 ? overrideMeta.Y : frames[0].Y,
          overrideMeta.Z != 0 ? overrideMeta.Z : frames[0].Z
        ); // X is incrementType (not to be used here), Y is loopMode, Z is playbackMode
        Vector3 newFrame;
        if (overrideFrame != 0 && overrideFrame < frames.Length) { // If override frame, just set frame
          newFrame = frames[overrideFrame];
        }
        if ((int)meta.Z == (int)playbackMode.Random) { // If random, just set frame to random frame
          newFrame = frames[(int)Main.rand.Next(frames.Length - 1) + 1];
        }
        else { // Else actually do work
          int frameIndex;
          Vector2 currFrame = oPlayer.OriFrame;
          frameIndex = Array.FindIndex(frames, f => f.X == currFrame.X && f.Y == currFrame.Y && Array.IndexOf(frames, f) != 0); // Check if this frame already exists
          if ((int)meta.Z == (int)playbackMode.Normal) {
            oPlayer.animPlayingInReverse = false;
            frameIndex = (frameIndex != frames.Length - 1) ?
              frameIndex++ :
              meta.Y == (int)loopMode.Once ? frameIndex = 1 : frameIndex;
          }
          else if ((int)meta.Z == (int)playbackMode.PingPong) {
            if (frameIndex == 1 && (int)meta.Y != (int)loopMode.Once) {
              oPlayer.animPlayingInReverse = false;
              frameIndex++;
            }
            else if (frameIndex == frames.Length - 1 && (int)meta.Y != (int)loopMode.Once) {
              oPlayer.animPlayingInReverse = true;
              frameIndex--;
            }
            else {
              frameIndex += !oPlayer.animPlayingInReverse ? -1 : 1;
            }
          }
          else if ((int)meta.Z == (int)playbackMode.Reverse) {
            oPlayer.animPlayingInReverse = true;
            frameIndex = (frameIndex != 1) ?
              frameIndex-- :
              meta.Y == (int)loopMode.Once ? frames.Length - 1 : frameIndex;
          }
        }
      }
    }
    public void Increment(string anim="Default", int overrideFrame=0, Vector3 overrideMeta=new Vector3(), Vector2 drawOffset=new Vector2()) {
      FrameHandler.IncrementFrame(anim, overrideFrame, overrideMeta, drawOffset);
    }
    public void SetPlayerFrame(string name, int frameIndex, int time) {
      currAnimName = name;
      currFrameIndex = frameIndex;
      currFrameTime = time;
    }
    public void PlayerFrame(int X, int Y) {
      if (player.whoAmI != Main.myPlayer) { return; }
      if (X > 0) {
        frameX = X;
        frameXset = true;
      }
      OriFrame.Y = spriteHeight * Y;
      OriFrame.X = spriteWidth * frameX;
      if (OriFrame.X / spriteWidth == 1 && OriFrame.Y / spriteHeight == 11) {
        rotation += rotSpeed;
        while (rotation > 360) {
          rotation -= 360;
        }
        while (rotation < 0) {
          rotation += 360;
        }
      }
      else {
        rotation = 0;
      }
      rotRads = (float)DegreeToRadian(rotation);
    }

    public void UpdateFrame(Player drawPlayer) {
      if (player.whoAmI != Main.myPlayer) { return; }
      if (!(OriSetPrevious && !transforming && !animRefreshed)) { return; }
        
      animRefreshed = true;
      //this controls animation frames. have fun trying to figure out how it works
      if (!frameXset) {
        frameX = 0;
      }
      else {
        climbFrame = 0;
        climbTimer = 0;
      }

      if (
        drawPlayer.velocity.X != 0 &&
        playerGrounded &&
        !oriDashing &&
        !isSwimming &&
        !bashActive &&
        !onWall && (
          PlayerInput.Triggers.Current.Left ||
          PlayerInput.Triggers.Current.Right)
        ) {
        if (OriFrame.Y / 76 > 16 || OriFrame.Y <= 5) {
          walkFrame = 6;
        }
        float velocity = Math.Abs(drawPlayer.velocity.X);
        if (velocity > 4) {
          velocity = 4;
        }
        velocity = (int)velocity;
        walkFrameCounter += velocity;
        if (walkFrameCounter >= 7f) {
          walkFrame++;
          walkFrameCounter = 0;
        }
        if (walkFrame > 16) {
          walkFrame = 6;
        }
        PlayerFrame(0, walkFrame);
      }
      int t = oriDoubleJumpAnimTimer;
      if (drawPlayer.mount.Cart) {
        PlayerFrame(1, 22);
      }
      else if (intoCrouch || outCrouch) {
        PlayerFrame(2, 23);
      }
      else if (crouching) {
        PlayerFrame(2, 24);
      }
      else if (intoLookUp || outLookUp) {
        PlayerFrame(2, 15);
      }
      else if (lookUp) {
        int p = lookUpTimer;
        if (p >= 1 && p <= 7) {
          PlayerFrame(2, 16);
        }
      else if (p >= 8 && p <= 14) {
          PlayerFrame(2, 17);
        }
      else if (p >= 15 && p <= 21) {
          PlayerFrame(2, 18);
        }
      else if (p >= 22 && p <= 28) {
          PlayerFrame(2, 19);
        }
      else if (p >= 29 && p <= 35) {
          PlayerFrame(2, 20);
        }
      else if (p >= 36 && p <= 42) {
          PlayerFrame(2, 21);
        }
      else if (p >= 43 && p <= 49) {
          PlayerFrame(2, 22);
        }
        lookUpTimer++;
        if (lookUpTimer > 49) {
          lookUpTimer = 1;
        }
      }
      else if (bashFrameUp && bashActive) {
        PlayerFrame(0, 5);
      }
      else if (intoStomp) {
        int a = intoStompTimer;
        if (a == 24 || a == 23 || a == 22) {
          PlayerFrame(1, 4);
        }
        else if (a == 21 || a == 20) {
          PlayerFrame(1, 5);
        }
        else if (a == 19 || a == 18 || a == 17) {
          PlayerFrame(1, 6);
        }
        else if (a <= 16 && a >= 1) {
          PlayerFrame(1, 11);
          rotSpeed = 60;
        }
      }
      else if (stomping) {
        int s = stompingTimer;
        if (s == 20 || s == 19) {
          PlayerFrame(1, 11);
        }
        else if (s == 18 || s == 17) {
          PlayerFrame(1, 6);
        }
        else if (s >= 13 && s <= 16) {
          PlayerFrame(1, 7);
        }
        else if (s >= 9 && s <= 12) {
          PlayerFrame(1, 8);
        }
        else if (s >= 5 && s <= 8) {
          PlayerFrame(1, 9);
        }
        else if (s >= 1 && s <= 4) {
          PlayerFrame(1, 10);
        }
      }
      else if (bashActive) {
        PlayerFrame(1, 1);
        oriDoubleJumpActive = false;
      }
      else if (dashAnimation > 25) {
        PlayerFrame(1, 21);
      }
      else if (
        OriMod.ClimbAndFeather.Current &&
        onWall &&
        !PlayerInput.Triggers.Current.Up &&
        !PlayerInput.Triggers.Current.Down && (
          (!PlayerInput.Triggers.Current.Left && drawPlayer.direction == 1) ||
          (!PlayerInput.Triggers.Current.Right && drawPlayer.direction == -1)
        )
      ) {
        PlayerFrame(0, 17);
      }
      else if (
        onWall &&
        !jumpUpWall &&
        !jumpingOnToWall &&
        !playerGrounded &&
        !intoCrouch &&
        !crouching
      ) {
        TestWallMaterial(drawPlayer);
        if (climbAnimation) {
          climbTimer -= drawPlayer.velocity.Y;
          if (climbTimer >= 10) {
            climbTimer = 0;
            climbFrame++;
            if (climbFrame > 7) {
              climbFrame = 0;
            }
          }
          if (climbTimer <= -1) {
            climbTimer = 9;
            climbFrame--;
            if (climbFrame > 0) {
              climbFrame = 7;
            }
          }
          PlayerFrame(3, climbFrame);
        }
        else {
          slideAnimate = true;
          slideTimer++;
          if (slideTimer > 20) {
            slideTimer = 1;
          }
          if (slideTimer <= 5 || slideTimer >= 0) {
            PlayerFrame(1, 17);
          }
          else if (slideTimer <= 10 || slideTimer >= 6) {
            PlayerFrame(1, 18);
          }
          else if (slideTimer <= 15 || slideTimer >= 11) {
            PlayerFrame(1, 19);
          }
          else if (slideTimer <= 20 || slideTimer >= 16) {
            PlayerFrame(1, 20);
          }
        }
        if (wallMaterial == "Grass") { }
      }
      else if (countering) {
        PlayerFrame(0, 5);
      }
      else if (chargeJumpAnimTimer > 0) {
        if (chargeJumpAnimTimer > 17) {
          drawPlayer.controlJump = false;
        }
        int p = chargeJumpAnimTimer;
        if (p >= 16 && p <= 20) {
          PlayerFrame(3, 8);
        }
        if (p >= 11 && p <= 15) {
          PlayerFrame(3, 9);
        }
        if (p >= 6 && p <= 10) {
          PlayerFrame(3, 10);
        }
        if (p >= 1 && p <= 5) {
          PlayerFrame(3, 11);
        }
      }
      else if (wallJumpAnimTimer > 0) {
        PlayerFrame(1, 16);
      }
      else if (oriDoubleJumpActive && !oriDashing) {
        if (t == 42 || t == 41) {
          PlayerFrame(1, 4);
        }
        else if (t == 40 || t == 39) {
          PlayerFrame(1, 5);
        }
        else if (t == 38 || t == 37) {
          PlayerFrame(1, 6);
        }
        else if (t <= 36 && t >= 1) {
          PlayerFrame(1, 11);
          rotSpeed = 45;
        }
        else {
          oriDoubleJumpActive = false;
        }
        if (playerGrounded || oriDashing) {
          oriDoubleJumpActive = false;
        }
      }
      else if (glideAnimTimer > 0) {
        int g = glideAnimTimer;
        if (g >= 1 && g <= 3) {
          PlayerFrame(3, 16);
        }
        if (g >= 4 && g <= 6) {
          PlayerFrame(3, 17);
        }
        if (g >= 7 && g <= 9) {
          PlayerFrame(3, 18);
        }
        if (g >= 10 && g <= 12) {
          PlayerFrame(3, 19);
        }
        if (g >= 13) {
          if (drawPlayer.velocity.X >= -4 && drawPlayer.velocity.X <= 4) {
            PlayerFrame(3, 19);
          }
          else {
            if (g >= 13 && g <= 17) {
              PlayerFrame(3, 20);
            }
            if (g >= 18 && g <= 22) {
              PlayerFrame(3, 21);
            }
            if (g >= 23 && g <= 27) {
              PlayerFrame(3, 22);
            }
            if (g >= 28 && g <= 32) {
              PlayerFrame(3, 23);
            }
            if (g >= 33 && g <= 37) {
              PlayerFrame(3, 24);
            }
            if (g >= 38 && g <= 42) {
              PlayerFrame(3, 25);
            }
          }
        }
      }
      else if (!oriJumpActive && !playerGrounded) {
        int f = oriFallingAnimTimer;
        if (f >= 1 && f <= 7) {
          PlayerFrame(1, 22);
        }
        if (f >= 8 && f <= 14) {
          PlayerFrame(1, 23);
        }
        if (f >= 15 && f <= 21) {
          PlayerFrame(1, 24);
        }
        if (f >= 22 && f <= 28) {
          PlayerFrame(1, 25);
        }
      }
      else if (playerGrounded && drawPlayer.velocity.X == 0) {
        if (onWall) {
          PlayerFrame(2, (int)(Main.GameUpdateCount / 7 % 7));
        }
        else {
          PlayerFrame(2, (int)(7 + (Main.GameUpdateCount / 9 % 8)));
        }
      }
      if (
        oriCanStep &&
        playerGrounded &&
        OriFrame.X == 0 && (
          (OriFrame.Y / 76) == 10 ||
          (OriFrame.Y / 76) == 15)
      ) {
        TestStepMaterial(drawPlayer);
        if (floorMaterial == "Grass" || floorMaterial == "Mushroom") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomChar(5), 0.15f, 0.1f);
        }
        else if (floorMaterial == "Water") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomChar(4), 1f, 0.1f);
          //            PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomString(4), 1.5f, 0.1f);
        }
        else if (floorMaterial == "SpiritTreeRock") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinSpiritTreeFootstepsRock" + RandomChar(5), 1f, 0.1f);
          //            PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinSpiritTreeFootstepsRock" + RandomString(5), 1.25f, 0.1f);
        }
        else if (floorMaterial == "SpiritTreeWood") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinSpiritTreeFootstepsWood" + RandomChar(5), 1f, 0.1f);
          //            PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinSpiritTreeFootstepsWood" + RandomString(5), 1.25f, 0.1f);
        }
        else if (floorMaterial == "Snow") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomChar(10), 0.85f, 0.1f);
        }
        else if (floorMaterial == "LightDark") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootstepsLightDarkPlatform" + RandomChar(10), 0.85f, 0.1f);
        }
        else if (floorMaterial == "Wood") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomChar(5), 0.85f, 0.1f);
        }
        else if (floorMaterial == "Sand") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomChar(8), 0.85f, 0.1f);
        }
        else if (floorMaterial == "Rock") {
          PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomChar(5), 1f, 0.1f);
          //            PlayNewSoundPlus("Ori/Footsteps/" + floorMaterial + "/seinFootsteps" + floorMaterial + RandomString(5), 1.5f, 0.1f);
        }
        Vector2 position = new Vector2(drawPlayer.Center.X + 2, (drawPlayer.position.Y + drawPlayer.height) - 2);
        if (drawPlayer.direction == -1) {
          position = new Vector2(drawPlayer.Center.X - 4, (drawPlayer.position.Y + drawPlayer.height) - 2);
        }
        for (int i = 0; i <= 3; i++) {
          Dust dust = Main.dust[Terraria.Dust.NewDust(position, 2, 2, 111, 0f, -2.7f, 0, new Color(255, 255, 255), 1f)];
          dust.noGravity = true;
          dust.scale = 0.75f;
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          dust.fadeIn = 0.03947368f;
        }
        oriCanStep = false;
      }
      if ((OriFrame.Y / 76) != 10 && (OriFrame.Y / 76) != 15) {
        oriCanStep = true;
      }
      //Main.NewText(player.AngleTo(Main.MouseWorld));
    }
    public void Transformation(Player player) {
      transforming = true;
      transformDirection = player.direction;
      transformTimer = 627;
    }
    public virtual void InitTestMaterial() {
      grassFloorMaterials = new List<int>();
      lightDarkFloorMaterials = new List<int>();
      mushroomFloorMaterials = new List<int>();
      rockFloorMaterials = new List<int>();
      sandFloorMaterials = new List<int>();
      snowFloorMaterials = new List<int>();
      spiritTreeRockFloorMaterials = new List<int>();
      spiritTreeWoodFloorMaterials = new List<int>();
      woodFloorMaterials = new List<int>();
      grassWallMaterials = new List<int>();
      lightDarkWallMaterials = new List<int>();
      mushroomWallMaterials = new List<int>();
      rockWallMaterials = new List<int>();
      woodWallMaterials = new List<int>();

      grassFloorMaterials.Clear();
      lightDarkFloorMaterials.Clear();
      mushroomFloorMaterials.Clear();
      rockFloorMaterials.Clear();
      sandFloorMaterials.Clear();
      snowFloorMaterials.Clear();
      spiritTreeRockFloorMaterials.Clear();
      spiritTreeWoodFloorMaterials.Clear();
      woodFloorMaterials.Clear();

      int[] grassBlocks = {
        TileID.Dirt, TileID.Grass, TileID.CorruptGrass, TileID.ClayBlock, TileID.Mud,
        TileID.JungleGrass, TileID.MushroomGrass, TileID.HallowedGrass, TileID.PineTree,
        TileID.GreenMoss, TileID.BrownMoss, TileID.RedMoss, TileID.BlueMoss, TileID.PurpleMoss,
        TileID.LeafBlock, TileID.FleshGrass, TileID.HayBlock, TileID.LivingMahoganyLeaves,
        TileID.LavaMoss };

      int[] lightDarkBlocks = {
        TileID.Glass, TileID.MagicalIceBlock, TileID.Sunplate,
        TileID.AmethystGemsparkOff, TileID.TopazGemsparkOff, TileID.SapphireGemsparkOff,
        TileID.EmeraldGemsparkOff, TileID.RubyGemsparkOff, TileID.DiamondGemsparkOff,
        TileID.AmberGemsparkOff, TileID.AmethystGemspark, TileID.TopazGemspark,
        TileID.SapphireGemspark, TileID.EmeraldGemspark, TileID.RubyGemspark,
        TileID.DiamondGemspark, TileID.AmberGemspark, TileID.Waterfall, TileID.Lavafall,
        TileID.Confetti, TileID.ConfettiBlack, TileID.Honeyfall, TileID.CrystalBlock,
        TileID.LunarBrick, TileID.TeamBlockRed, TileID.TeamBlockRedPlatform,
        TileID.TeamBlockGreen, TileID.TeamBlockBlue, TileID.TeamBlockYellow,
        TileID.TeamBlockPink, TileID.TeamBlockWhite, TileID.TeamBlockGreenPlatform,
        TileID.TeamBlockBluePlatform, TileID.TeamBlockYellowPlatform,
        TileID.TeamBlockPinkPlatform, TileID.TeamBlockWhitePlatform, TileID.SandFallBlock,
        TileID.SnowFallBlock };

      int[] mushroomBlocks = {
        TileID.CandyCaneBlock, TileID.GreenCandyCaneBlock,
        TileID.CactusBlock, TileID.MushroomBlock, TileID.SlimeBlock, TileID.FrozenSlimeBlock,
        TileID.BubblegumBlock, TileID.PumpkinBlock,
        TileID.Coralstone, TileID.PinkSlimeBlock, TileID.SillyBalloonPink,
        TileID.SillyBalloonPurple, TileID.SillyBalloonGreen };

      int[] rockBlocks = {
        TileID.Stone, TileID.Iron, TileID.Copper, TileID.Silver, TileID.Gold,
        TileID.Demonite, TileID.Ebonstone, TileID.Meteorite, TileID.Obsidian, TileID.Hellstone,
        TileID.Sapphire, TileID.Ruby, TileID.Emerald, TileID.Topaz, TileID.Amethyst,
        TileID.Diamond, TileID.Cobalt, TileID.Mythril, TileID.Adamantite, TileID.Pearlstone,
        TileID.ActiveStoneBlock, TileID.Boulder, TileID.IceBlock, TileID.BreakableIce,
        TileID.CorruptIce, TileID.HallowedIce, TileID.Tin, TileID.Lead, TileID.Tungsten,
        TileID.Platinum, TileID.BoneBlock, TileID.FleshBlock, TileID.Asphalt, TileID.FleshIce,
        TileID.Crimstone, TileID.Crimtane, TileID.Chlorophyte, TileID.Palladium,
        TileID.Orichalcum, TileID.Titanium, TileID.MetalBars, TileID.Cog, TileID.Marble,
        TileID.Granite, TileID.Sandstone, TileID.HardenedSand, TileID.CorruptHardenedSand,
        TileID.CrimsonHardenedSand, TileID.CorruptSandstone, TileID.CrimsonSandstone,
        TileID.HallowHardenedSand, TileID.HallowSandstone, TileID.DesertFossil,
        TileID.FossilOre, TileID.LunarOre, TileID.LunarBlockSolar, TileID.LunarBlockVortex,
        TileID.LunarBlockNebula, TileID.LunarBlockStardust
      };

      int[] sandBlocks = {
        TileID.Sand, TileID.Ash, TileID.Ebonsand, TileID.Pearlsand,
        TileID.Silt, TileID.Hive, TileID.CrispyHoneyBlock, TileID.Crimsand
      };
      int[] snowBlocks = {
        TileID.SnowBlock, TileID.RedStucco, TileID.YellowStucco,
        TileID.GreenStucco, TileID.GrayStucco, TileID.Cloud, TileID.RainCloud, TileID.Slush,
        TileID.HoneyBlock, TileID.SnowCloud
      };

      int[] spiritTreeRockBlocks = {
        TileID.Anvils, TileID.MythrilAnvil, TileID.GrayBrick, TileID.RedBrick, TileID.BlueDungeonBrick,
        TileID.GreenDungeonBrick, TileID.PinkDungeonBrick, TileID.GoldBrick, TileID.SilverBrick,
        TileID.CopperBrick, TileID.Spikes, TileID.Obsidian, TileID.HellstoneBrick, TileID.DemoniteBrick,
        TileID.PearlstoneBrick, TileID.IridescentBrick, TileID.Mudstone, TileID.CobaltBrick,
        TileID.MythrilBrick, TileID.Traps, TileID.SnowBrick, TileID.AdamantiteBeam, TileID.SandstoneBrick,
        TileID.EbonstoneBrick, TileID.RainbowBrick, TileID.TinBrick, TileID.TungstenBrick,
        TileID.PlatinumBrick, TileID.IceBrick, TileID.LihzahrdBrick, TileID.PalladiumColumn,
        TileID.Titanstone, TileID.StoneSlab, TileID.SandStoneSlab, TileID.CopperPlating,
        TileID.TinPlating, TileID.ChlorophyteBrick, TileID.CrimtaneBrick, TileID.ShroomitePlating,
        TileID.MartianConduitPlating, TileID.MarbleBlock, TileID.GraniteBlock,
        TileID.MeteoriteBrick, TileID.Fireplace
      };

      int[] spiritTreeWoodBlocks = { TileID.LivingWood, TileID.LivingMahogany };

      int[] woodBlocks = {
        TileID.Tables, TileID.WorkBenches, TileID.Platforms, TileID.WoodBlock,
        TileID.Dressers, TileID.Bookcases, TileID.TinkerersWorkbench, TileID.Ebonwood, TileID.RichMahogany,
        TileID.Pearlwood, TileID.SpookyWood, TileID.DynastyWood, TileID.BlueDynastyShingles,
        TileID.RedDynastyShingles, TileID.BorealWood, TileID.PalmWood
      };

      grassFloorMaterials.AddRange(grassBlocks);
      lightDarkFloorMaterials.AddRange(lightDarkBlocks);
      mushroomFloorMaterials.AddRange(mushroomBlocks);
      rockFloorMaterials.AddRange(rockBlocks);
      sandFloorMaterials.AddRange(sandBlocks);
      snowFloorMaterials.AddRange(snowBlocks);
      spiritTreeRockFloorMaterials.AddRange(spiritTreeRockBlocks);
      spiritTreeWoodFloorMaterials.AddRange(spiritTreeWoodBlocks);
      woodFloorMaterials.AddRange(woodBlocks);

      grassWallMaterials.AddRange(grassBlocks);
      grassWallMaterials.AddRange(sandBlocks);
      grassWallMaterials.AddRange(snowBlocks);
      lightDarkWallMaterials.AddRange(lightDarkBlocks);
      mushroomWallMaterials.AddRange(mushroomBlocks);
      rockWallMaterials.AddRange(rockBlocks);
      rockWallMaterials.AddRange(spiritTreeRockBlocks);
      woodWallMaterials.AddRange(woodBlocks);
      woodWallMaterials.AddRange(spiritTreeWoodBlocks);
    }
    
    // Gets the tile that's a given offset from player.Center.X, player.position.Y + player.height
    public Tile getTile(float offsetX, float offsetY) {
        Vector2 pos = new Vector2(player.Center.X + offsetX, (player.position.Y + player.height) + offsetX);
        Vector2 tilepos = new Vector2(pos.ToTileCoordinates().X, pos.ToTileCoordinates().Y);
        return Main.tile[(int)tilepos.X, (int)tilepos.Y];
      }
    public virtual void TestStepMaterial(Player player) { //oh yeah good luck understanding what this is
      if (grassFloorMaterials == null) {
        InitTestMaterial();
      }
      Tile tile = getTile(-12f, 4f);
      if (tile.liquid > 0f && tile.liquidType() == 0) {
        floorMaterial = "Water";
      }
      else {
        tile = getTile(-12f, -4f);
        if (tile.liquid > 0f && tile.liquidType() == 0) {
          floorMaterial = "Water";
        }
        else {
          tile = getTile(-12f, 8);
          if (tile.active()) {
            if (grassFloorMaterials.Contains(tile.type)) {
              floorMaterial = "Grass";
            }
            else if (lightDarkFloorMaterials.Contains(tile.type)) {
              floorMaterial = "LightDark";
            }
            else if (mushroomFloorMaterials.Contains(tile.type)) {
              floorMaterial = "Mushroom";
            }
            else if (rockFloorMaterials.Contains(tile.type)) {
              floorMaterial = "Rock";
            }
            else if (sandFloorMaterials.Contains(tile.type)) {
              floorMaterial = "Sand";
            }
            else if (snowFloorMaterials.Contains(tile.type)) {
              floorMaterial = "Snow";
            }
            else if (spiritTreeRockFloorMaterials.Contains(tile.type)) {
              floorMaterial = "SpiritTreeRock";
            }
            else if (spiritTreeWoodFloorMaterials.Contains(tile.type)) {
              floorMaterial = "SpiritTreeWood";
            }
            else if (woodFloorMaterials.Contains(tile.type)) {
              floorMaterial = "Wood";
            }
          }
          else {
            tile = getTile(-12, 24);
            if (tile.active()) {
              if (grassFloorMaterials.Contains(tile.type)) {
                floorMaterial = "Grass";
              }
              else if (lightDarkFloorMaterials.Contains(tile.type)) {
                floorMaterial = "LightDark";
              }
              else if (mushroomFloorMaterials.Contains(tile.type)) {
                floorMaterial = "Mushroom";
              }
              else if (rockFloorMaterials.Contains(tile.type)) {
                floorMaterial = "Rock";
              }
              else if (sandFloorMaterials.Contains(tile.type)) {
                floorMaterial = "Sand";
              }
              else if (snowFloorMaterials.Contains(tile.type)) {
                floorMaterial = "Snow";
              }
              else if (spiritTreeRockFloorMaterials.Contains(tile.type)) {
                floorMaterial = "SpiritTreeRock";
              }
              else if (spiritTreeWoodFloorMaterials.Contains(tile.type)) {
                floorMaterial = "SpiritTreeWood";
              }
              else if (woodFloorMaterials.Contains(tile.type)) {
                floorMaterial = "Wood";
              }
            }
            else {
              floorMaterial = "Grass";
            }
          }
        }
      }
    }
    public virtual void TestWallMaterial(Player player) //or this, im too tired to comment them
    {
      if (grassFloorMaterials == null) {
        InitTestMaterial();
      }

      Tile tile = getTile(-2f, 34f);
      if (tile.active())
      {
        if (grassWallMaterials.Contains(tile.type)) {
          wallMaterial = "Grass";
        }
        else if (lightDarkWallMaterials.Contains(tile.type)) {
          wallMaterial = "LightDark";
        }
        else if (mushroomWallMaterials.Contains(tile.type)) {
          wallMaterial = "Mushroom";
        }
        else if (rockWallMaterials.Contains(tile.type)) {
          wallMaterial = "Rock";
        }
        else if (woodWallMaterials.Contains(tile.type)) {
          wallMaterial = "Wood";
        }
      }
      else {
        wallMaterial = "Grass";
      }
    }
    public void RemoveSeinBuffs(int exclude=0) {
      for (int u = 1; u <= OriMod.SeinUpgrades.Count; u++) {
        if (u != exclude) {
          player.ClearBuff(mod.GetBuff("SeinBuff" + u).Type);
        }
      }
    }
    public override void PostUpdate() {
      if (seinMinionActive) {
        if (!(
          player.HasBuff(mod.GetBuff("SeinBuff1").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff2").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff3").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff4").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff5").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff6").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff7").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff8").Type)   
        )) {
          seinMinionActive = false;
          seinMinionUpgrade = 0;
        }
      }
      
      // bool canBash = false;
      // bool canDash = false;
      // bool canStomp = false;
      // bool canCDash = false;
      // bool canCJump = false;

      playerGrounded = false;
      // Check if grounded by means of liquid walking
      if (player.fireWalk || player.waterWalk2 || player.waterWalk) {
        Vector2 checkPos = new Vector2(player.position.X + (player.width / 2), player.position.Y + player.height + 4);
        Vector2 check2 = new Vector2(checkPos.ToTileCoordinates().X, checkPos.ToTileCoordinates().Y);
        bool testblock =
          Main.tile[(int)check2.X, (int)check2.Y].liquid > 0 &&
          Main.tile[(int)check2.X, (int)check2.Y - 1].liquid == 0;
        if (testblock) {
          Tile liquidTile = Main.tile[(int)check2.X, (int)check2.Y];
          playerGrounded = liquidTile.lava() ? player.fireWalk : player.waterWalk;
        }
      }
      if (!playerGrounded) {
        playerGrounded = !Collision.IsClearSpotTest(player.position + new Vector2(0, 8), 16f, player.width, player.height, false, false, (int)player.gravDir, true, true);
      }

      //thanks jopo

      Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0f, 0.39f, 0.5f);

      oriAirJumps = 2;

      if (transforming) {
        player.direction = transformDirection;
      }

      if (!OriSetPrevious) { return; }
      if (player.mount.Cart) {
        intoStomp = false;
        intoStompTimer = 0;
        stomping = false;
        stompingTimer = 0;
        outOfStomp = false;
        outOfStompTimer = 0;
      }
      // moves that shouldnt execute when doing other broad specified actions
      if (!player.pulley && !player.minecartLeft && !player.mount.Active && !player.mount.Cart) {
        //Climbing
        if (OriMod.ClimbAndFeather.Current && onWall && canClimb) {
          if (PlayerInput.Triggers.Current.Up) {
            doClimbAnimation = true;
            if (player.velocity.Y < -3) {
              player.velocity.Y++;
            }
            if (player.velocity.Y > -3) {
              player.velocity.Y--;
            }
          }
          else {
            doClimbAnimation = false;
          }
          if (PlayerInput.Triggers.Current.Down) {
            if (player.velocity.Y < 4) {
              player.velocity.Y++;
            }
            if (player.velocity.Y > 4) {
              player.velocity.Y--;
            }
          }
        }
        //Kuro's Feather
        if (
          (
            OriMod.ClimbAndFeather.JustReleased ||
            playerGrounded ||
            oriDoubleJumpActive ||
            oriDashing ||
            bashActive ||
            intoStomp ||
            stomping ||
            outOfStomp ||
            onWall
          ) && glideAnimTimer > 0
        ) {
          PlayNewSound("Ori/Glide/seinGildeEnd" + RandomChar(3));
        }
        if (
          OriMod.ClimbAndFeather.Current &&
          !playerGrounded &&
          !oriDoubleJumpActive &&
          !oriDashing &&
          !bashActive &&
          !intoStomp &&
          !stomping &&
          !outOfStomp &&
          wallJumpAnimTimer == 0 &&
          !onWall
        ) {
          glideAnimTimer++;
          if (glideAnimTimer == 1) {
            PlayNewSound("Ori/Glide/seinGlideStart" + RandomChar(3));
          }
          if (glideAnimTimer > 42) {
            glideAnimTimer = 13;
          }
          if (
            glideAnimTimer > 12 && (
              PlayerInput.Triggers.JustPressed.Left ||
              PlayerInput.Triggers.JustPressed.Right
              )
            ) {
            PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + RandomChar(5));
          }
        }
        else
        {
          if (
            !playerGrounded &&
            !oriDoubleJumpActive &&
            !oriDashing &&
            !bashActive &&
            !intoStomp &&
            !stomping &&
            !outOfStomp &&
            wallJumpAnimTimer == 0
          ) {
            if (glideAnimTimer > 13) {
              glideAnimTimer = 13;
            }
            if (glideAnimTimer > 0) {
              glideAnimTimer--;
            }
          }
          else {
            glideAnimTimer = 0;
          }
        }
        //Stomp
        if (
          PlayerInput.Triggers.JustPressed.Down &&
          !playerGrounded &&
          !bashActive &&
          !intoStomp &&
          !stomping &&
          !onWall
        ) {
          intoStomp = true;
          intoStompTimer = 24;
          player.velocity.X = 0;
          PlayNewSoundPlus("Ori/Stomp/seinStompStart" + RandomChar(3), 1f, 0.2f);
          preHeight = player.position.Y;
        }
        //Crouch
        if (
          PlayerInput.Triggers.JustPressed.Down &&
          playerGrounded &&
          !intoCrouch &&
          !PlayerInput.Triggers.Current.Up
        ) {
          intoCrouch = true;
          intoCrouchTimer = 5;
        }
        //Looking Up
        if (
          PlayerInput.Triggers.Current.Up &&
          playerGrounded &&
          !intoLookUp &&
          !PlayerInput.Triggers.Current.Down &&
          !PlayerInput.Triggers.Current.Left &&
          !PlayerInput.Triggers.Current.Right
        ) {
          intoLookUp = true;
          intoLookUpTimer = 5;
        }
        //some misc stuff i dont care about
        if (intoLookUp) {
          intoLookUpTimer--;
          player.velocity.X = 0;
          if (
            PlayerInput.Triggers.JustPressed.Left ||
            PlayerInput.Triggers.JustPressed.Right ||
            PlayerInput.Triggers.JustPressed.Jump ||
            !PlayerInput.Triggers.Current.Up ||
            !playerGrounded ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            PlayerInput.Triggers.Current.Down ||
            oriDashing
          ) {
            intoLookUpTimer = 0;
            intoLookUp = false;
          }
          else if (intoCrouchTimer == 0) {
            lookUp = true;
            intoLookUp = false;
          }
        }
        if (lookUp) {
          player.velocity.X = 0;
          if (
            PlayerInput.Triggers.JustPressed.Left ||
            PlayerInput.Triggers.JustPressed.Right ||
            PlayerInput.Triggers.JustPressed.Jump ||
            PlayerInput.Triggers.Current.Down ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            !playerGrounded ||
            oriDashing
          ) {
            lookUp = false;
          }
          else if (!PlayerInput.Triggers.Current.Up) {
            outLookUp = true;
            outLookUpTimer = 5;
            lookUp = false;
          }
        }
        if (outLookUp) {
          outLookUpTimer--;
          if (
            PlayerInput.Triggers.JustPressed.Left ||
            PlayerInput.Triggers.JustPressed.Right ||
            PlayerInput.Triggers.Current.Down ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            !playerGrounded ||
            oriDashing ||
            outLookUpTimer == 0
          ) {
            outLookUp = false;
            outLookUpTimer = 0;
          }
        }
        if (intoCrouch)
        {
          intoCrouchTimer--;
          player.velocity.X = 0;
          if (PlayerInput.Triggers.JustPressed.Left) {
            player.controlLeft = false;
            player.direction = -1;
          }
          else if (PlayerInput.Triggers.JustPressed.Right) {
            player.controlRight = false;
            player.direction = 1;
          }
          if (PlayerInput.Triggers.JustPressed.Jump) {
            Vector2 pos = player.position;
            pos = new Vector2(pos.X + 4, pos.Y + 52);
            pos.ToWorldCoordinates();
            if (
              !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y].type] &&
              !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y + 1].type]
            ) {
              backflipping = true;
            }
            intoCrouch = false;
          }
          else if (
            !PlayerInput.Triggers.Current.Down ||
            PlayerInput.Triggers.Current.Up ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            !playerGrounded
          ) {
            intoCrouch = false;
            intoCrouchTimer = 0;
          }
          else if (intoCrouchTimer == 0) {
            crouching = true;
            intoCrouch = false;
          }
        }
        if (crouching) {
          player.velocity.X = 0;
          if (PlayerInput.Triggers.JustPressed.Left) {
            player.controlLeft = false;
            player.direction = -1;
          }
          else if (PlayerInput.Triggers.JustPressed.Right) {
            player.controlRight = false;
            player.direction = 1;
          }
          if (PlayerInput.Triggers.JustPressed.Jump) {
            /*Vector2 pos = player.position;
            pos = new Vector2(pos.X + 4, pos.Y + 52);
            pos.ToWorldCoordinates();
            if (!TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y].type] && !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y + 1].type])
            {
              backflipping = true;
            }*/
            crouching = false;
          }
          else if (
            !PlayerInput.Triggers.Current.Down ||
            PlayerInput.Triggers.Current.Up ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            !playerGrounded
          ) {
            if (
              !PlayerInput.Triggers.Current.Left &&
              !PlayerInput.Triggers.Current.Right &&
              !OriMod.BashKey.Current &&
              playerGrounded
            ) {
              crouching = false;
              outCrouch = true;
              outCrouchTimer = 5;
            }
            else {
              if (OriMod.BashKey.JustPressed) {
                player.position.Y -= 1;
                player.velocity.Y -= 3;
              }
              crouching = false;
            }
          }
        }
        if (outCrouch) {
          outCrouchTimer--;
          if (
            PlayerInput.Triggers.Current.Up ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            outCrouchTimer == 0 ||
            !playerGrounded
          ) {
            outCrouch = false;
            outCrouchTimer = 0;
          }
        }
        if (outOfStomp) {
          player.controlUp = false;
          player.controlDown = false;
          player.controlLeft = false;
          player.controlRight = false;
          if (outOfStompTimer == 0 || oriDashing || bashActive || oriDoubleJumpActive)
          {
            outOfStomp = false;
            outOfStompTimer = 0;
          }
        }
        if (stomping)
        {
          player.controlUp = false;
          player.controlDown = false;
          player.controlLeft = false;
          player.controlRight = false;
          tempInvincibility = true;
          immuneTimer = 15;
          stompHitboxTimer = 3;
          if (PlayerInput.Triggers.JustPressed.Jump)
          {
            stomping = false;
            outOfStomp = true;
            outOfStompTimer = 10;
          }
          if (playerGrounded)
          {
            stomping = false;
            frameX = 0;
            frameXset = true;
            PlayNewSound("Ori/Stomp/seinStompImpact" + RandomChar(3));
            Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
            for (int i = 0; i < 25; i++) //does particles
            {
              Dust dust = Main.dust[Terraria.Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
              dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
              dust.velocity *= new Vector2(2, 0.5f);
              if (dust.velocity.Y > 0) {
                dust.velocity.Y = -dust.velocity.Y;
              }
            }
          }
          if (stompingTimer == 0) {
            frameX = 0;
            frameXset = true;
            stomping = false;
          }
        }
        if (intoStomp) {
          player.controlUp = false;
          player.controlDown = false;
          player.controlLeft = false;
          player.controlRight = false;
          if (intoStompTimer == 0) {
            intoStomp = false;
            stomping = true;
            stompingTimer = 20;
            PlayNewSound("Ori/Stomp/seinStompFall" + RandomChar(3));
            stompHitboxTimer = 3;
            Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1);
          }
          if (player.velocity.Y > 0) {
            player.velocity.Y /= 3;
          }
          if (player.velocity.Y < -2) {
            player.velocity.Y = -2;
          }
          if (PlayerInput.Triggers.JustPressed.Jump && jumpsAvailable > 0) {
            intoStompTimer = 0;
            intoStomp = false;
            player.position.Y = preHeight;
          }
        }
      }
      // Bashing
      if (
        OriMod.BashKey.JustPressed &&
        OriSetPrevious == true &&
        bashActivate == 0 &&
        !oriDashing && // Bash should be available during Dash
        !intoStomp &&  // Bash should be available during Stomp
        abilityBash
      ) {
        bashActivate = 3;
        Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("BashHitbox"), 1, 0f, player.whoAmI, 0, 1);
      }
      // Jump Effects
      if (player.justJumped) {
        PlayNewSoundVolume("Ori/Jump/seinJumpsGrass" + RandomChar(5), 0.75f);
      }
      if (
        OriMod.DashKey.JustPressed &&
        OriSetPrevious == true &&
        dashTimer == 0 &&
        !oriDashing &&
        canAirDash &&
        dashAnimation <= 18 &&
        !intoStomp &&
        !stomping &&
        !outOfStomp
      ) {
        oriDashing = true;
        dashTimer = 12;
        dashAnimation = 37;
        dashDirection = player.direction;
        if (!charged) {
          PlayNewSoundVolume("Ori/Dash/seinDash" + RandomChar(3), 0.3f);
        }
        canAirDash = false;
        player.pulley = false;
      }
      //Currently Dashing
      if (oriDashing) {
        if ((dashDirection != player.direction) || PlayerInput.Triggers.JustPressed.Jump || dashTimer == 0) {
          oriDashing = false;
          if (dashDirection != player.direction) {
            dashAnimation -= dashTimer;
          }
          dashTimer = 0;
          player.velocity.Y = (0 - player.gravity) + 0.01f;
          player.velocity.X = 12 * player.direction;
        }
        if (oriDashing) {
          player.velocity.Y = 0.01f;
          player.moveSpeed = player.maxRunSpeed = charged ? 74f : 52f;
          player.velocity.X = player.direction == -1 ?
            (charged ? -100f : -52f) :
            (charged ? 100f : 52f);
        }
      }
      else if (chargedash) {
        chargedash = false;
      }
      //Curently Bashing
      if (bashActive) {
        player.velocity.X = 0;
        player.velocity.Y = 0 - player.gravity;
        player.controlJump = false;
        player.controlUp = false;
        player.controlDown = false;
        player.controlLeft = false;
        player.controlRight = false;
        player.immune = true;
        player.buffImmune[BuffID.CursedInferno] = true;
        player.buffImmune[BuffID.Dazed] = true;
        player.buffImmune[BuffID.Frozen] = true;
        player.buffImmune[BuffID.Frostburn] = true;
        player.buffImmune[BuffID.MoonLeech] = true;
        player.buffImmune[BuffID.Obstructed] = true;
        player.buffImmune[BuffID.OnFire] = true;
        player.buffImmune[BuffID.Poisoned] = true;
        player.buffImmune[BuffID.ShadowFlame] = true;
        player.buffImmune[BuffID.Silenced] = true;
        player.buffImmune[BuffID.Slow] = true;
        player.buffImmune[BuffID.Stoned] = true;
        player.buffImmune[BuffID.Suffocation] = true;
        player.buffImmune[BuffID.Venom] = true;
        player.buffImmune[BuffID.Weak] = true;
        player.buffImmune[BuffID.WitheredArmor] = true;
        player.buffImmune[BuffID.WitheredWeapon] = true;
        player.buffImmune[BuffID.WindPushed] = true;
        frameX = 1;
        tempInvincibility = true;
        immuneTimer = 15;
        frameXset = true;
        //Bash Sound
        if (bashActiveTimer == 75) {
          PlayNewSoundVolume("Ori/Bash/seinBashLoopA", /*0.7f*/ Main.soundVolume);
        }
        if (Main.npc[bashNPC].active) {
          Main.npc[bashNPC].velocity = Vector2.Zero;
        }
      }
      //Freezing Bash
      if (bashActive && bashActiveTimer < 97) {
        player.Center = bashPosition;
      }
      //Releasing Bash
      if (
        (
          !OriMod.BashKey.Current ||
          OriMod.BashKey.JustReleased ||
          bashActiveTimer == 5 ||
          !Main.npc[bashNPC].active
        ) && 
        OriSetPrevious &&
        bashActive
      ) {
        bashActive = false;
        if (oriDoubleJumpActive) {
          oriDoubleJumpActive = false;
        }
        bashActiveTimer = 4;
        bashAngle = player.AngleFrom(Main.MouseWorld);
        //Main.NewText(bashAngle);
        PlayNewSoundVolume("Ori/Bash/seinBashEnd" + RandomChar(3), /*0.7f*/ Main.soundVolume);
        unrestrictedMovement = true;
        player.velocity = new Vector2((float)(0 - (Math.Cos(bashAngle) * bashDistance)), (float)(0 - (Math.Sin(bashAngle) * bashDistance)));
        player.velocity.Y = player.velocity.Y / 1.3f;
        bashActivate = 50;
        tempInvincibility = true;
        immuneTimer = 15;
        Main.npc[bashNPC].velocity = new Vector2(-(float)(0 - (Math.Cos(bashAngle) * (bashDistance / 1.5f))), -(float)(0 - (Math.Sin(bashAngle) * (bashDistance / 1.5f))));
        Main.npc[bashNPC].Center = bashNPCPosition;
        player.ApplyDamageToNPC(Main.npc[bashNPC], 15, 0, 1, false);
      }
      //Double Jumping
      if (
        !playerGrounded &&
        PlayerInput.Triggers.JustPressed.Jump &&
        jumpsAvailable > 0 &&
        OriSetPrevious &&
        !onWall &&
        !intoStomp &&
        !stomping &&
        !outOfStomp &&
        !player.jumpAgainBlizzard &&
        !player.jumpAgainCloud &&
        !player.jumpAgainFart &&
        !player.jumpAgainSail &&
        !player.jumpAgainSandstorm &&
        !player.mount.Cart
      ) {
        player.velocity.Y = -8.8f;
        if (oriAirJumps == jumpsAvailable) {
          PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + RandomChar(5));
        }
        else if (oriAirJumps == 2 && jumpsAvailable == 1) {
          PlayNewSound("Ori/TripleJump/seinTripleJumps" + RandomChar(5));
        }
        oriDoubleJumpActive = true;
        oriDoubleJumpAnimTimer = 42;
        jumpsAvailable--;
      }
      //Mini Wall Climb
      if (playerGrounded && onWall && PlayerInput.Triggers.JustPressed.Jump) {
        doClimbAnimation = true;
        if (abilityWallJump) {
          canAirDash = true;
          jumpsAvailable = oriAirJumps;
        }
      }
      //Climb Animation
      if (doClimbAnimation && climbAnimation) {
        if ((player.velocity.Y >= 0 || !onWall) && !OriMod.ClimbAndFeather.Current) {
          doClimbAnimation = false;
        }
      }
      //why is there two statements, i mean it works but why (because that's the long way of assigning one bool to another)
      climbAnimation = doClimbAnimation;
      //refresh air abilities
      if (playerGrounded || player.pulley) {
        jumpsAvailable = oriAirJumps;
        canAirDash = abilityAirDash;
      }
      //Wall Jump
      if (onWall && !playerGrounded && PlayerInput.Triggers.JustPressed.Jump && abilityWallJump) {
        wallJumped = true;
        player.velocity.Y = -7.2f;
        onWall = false;
        wallJumpAnimTimer = 12;
        jumpsAvailable = oriAirJumps;
        canAirDash = true;
        PlayNewSound("Ori/WallJump/seinWallJumps" + RandomChar(5));
      }
      //Remove Wall Slide
      if (onWall && wallJumpAnimTimer > 0) {
        player.velocity.Y += 1;
        wallJumpAnimTimer = 0;
      }
      //fling
      if (wallJumped) {
        player.velocity.X = 4 * -(player.direction);
        if (PlayerInput.Triggers.Current.Left || PlayerInput.Triggers.Current.Right || playerGrounded) {
          wallJumped = false;
        }
      }
      //tempinvincibility
      if (tempInvincibility && immuneTimer > 0) {
        player.immune = true;
      }
      else {
        tempInvincibility = false;
        immuneTimer = 0;
      }
      //Charging
      if (
        (
          PlayerInput.Triggers.Current.Up &&
          !upRefresh &&
          !(
            onWall &&
            OriMod.ClimbAndFeather.Current)
          ) || (
            onWall &&
            !playerGrounded &&
            OriMod.ClimbAndFeather.Current &&
            (
              (player.direction == 1 &&
              PlayerInput.Triggers.Current.Left
            ) || (
              player.direction == -1 && PlayerInput.Triggers.Current.Right
            )
          )
        )
      ) {
        if (chargeTimer == 1) {
          PlayNewSoundPlus("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
        }
        if (!charged) {
          chargeTimer++;
          chargeUpTimer = 35;
        }
        if (chargeTimer > chargeUpTimer) {
          chargeTimer = chargeUpTimer;
          PlayNewSound("Ori/ChargeDash/seinChargeDashCharged");
          charged = true;
        }
        if (charged && PlayerInput.Triggers.JustPressed.Jump && playerGrounded) {
          chargeTimer = 0;
          charged = false;
          PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + RandomChar(3));
          chargeJumpAnimTimer = 20;
          upRefresh = true;
          stompHitboxTimer = 3;
          Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1);
        }
        if (charged && OriMod.DashKey.Current) {
          chargeTimer = 0;
          charged = false;
          PlayNewSound("Ori/ChargeDash/seinChargeDash" + RandomChar(3));
          PlayNewSound("Ori/ChargeDash/seinChargeDashChargeStart" + RandomChar(2));
          chargedash = true;
          upRefresh = true;
          stompHitboxTimer = 3;
          Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1);
        }
      }
      else {
        chargeTimer = 0;
        if (charged) {
          PlayNewSoundPlus("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
          charged = false;
        }
      }
      if ((!PlayerInput.Triggers.Current.Up) && (chargeJumpAnimTimer <= 0 && !chargedash)) {
        upRefresh = false;
      }

      //wall detection
      onWall = false;
      //code modified from source code
      float posx = player.position.X;
      float posy = player.position.Y + 2f;
      if (player.direction == 1) {
        posx += player.width;
      }
      posx += player.direction;
      if (player.gravDir < 0f) {
        posy = player.position.Y - 1f;
      }
      posx /= 16f;
      posy /= 16f;
      if (
        WorldGen.SolidTile((int)posx, (int)posy + 1) &&
        WorldGen.SolidTile((int)posx, (int)posy + 2)
      ) {
        oriDashing = false;
        onWall = true;
      }
    }
    public override void PostUpdateRunSpeeds() {
      if (OriSetPrevious) {
        if (tempInvincibility && immuneTimer > 0) {
          player.immune = true;
        }
        else {
          tempInvincibility = false;
          immuneTimer = 0;
        }
        player.noFallDmg = true;
        if (unrestrictedMovement) {
          player.runSlowdown = 0f;
          if (PlayerInput.Triggers.Current.Left || PlayerInput.Triggers.Current.Right || playerGrounded) {
            unrestrictedMovement = false;
          }
        }
        else if (dashAnimation > 20) {
          player.runSlowdown = 26f;
        }
        else if (unrestrictedMovement) {
          player.runSlowdown = 0;
        }
        else if (glideAnimTimer > 0) {
          player.runSlowdown = 0.125f;
        }
        else {
          player.runSlowdown = 1f;
        }
        if (intoCrouch || outCrouch || crouching || intoLookUp || lookUp || outLookUp) {
          player.runAcceleration = 0;
          player.maxRunSpeed = 0;
          player.velocity.X = 0;
        }
        else {
          player.runAcceleration = 0.5f;
          player.maxRunSpeed += 2f;
        }
        if (glideAnimTimer > 0) {
          player.runAcceleration = 0.2f;
        }
        Main.SetCameraLerp(0.05f, 1);
        if (OriMod.ClimbAndFeather.Current && onWall) {
          player.gravity = 0;
          player.runAcceleration = 0;
          player.maxRunSpeed = 0;
          if (
            (
              player.velocity.Y > 1 &&
              !PlayerInput.Triggers.Current.Down
            ) || (
              player.velocity.Y < 1 &&
              !PlayerInput.Triggers.Current.Up
            )
          ) {
            player.velocity.Y /= 3;
          }
          if (
            player.velocity.Y != 0 &&
            player.velocity.Y < 1 &&
            player.velocity.Y > -1 &&
            !PlayerInput.Triggers.Current.Up &&
            !PlayerInput.Triggers.Current.Down
          ) {
            player.velocity.Y = 0;
          }
        }
        else if (
          onWall && (playerGrounded || player.velocity.Y < 0)
        ) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
          player.jumpSpeedBoost -= 6f;
        }
        else if (onWall && player.velocity.Y > 0 && !intoStomp && !stomping && !outOfStomp && !playerGrounded) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
        }
        else if (intoStomp) {
          player.gravity = -0.1f;
          player.runAcceleration = 0;
          player.maxRunSpeed = 0;
        }
        else if (stomping) {
          player.gravity = 4f;
          player.runAcceleration = 0;
          player.maxRunSpeed = 0;
        }
        else if (chargeJumpAnimTimer > 0) {
          player.gravity = 0.1f;
          player.velocity.Y = -3 * chargeJumpAnimTimer;
          if (chargeJumpAnimTimer == 18) {
            player.controlJump = false;
          }
          tempInvincibility = true;
          immuneTimer = 15;
        }
        else {
          player.gravity = 0.35f;
        }
        if (!charged) {
          player.jumpSpeedBoost += 2f;
        }
        if (charged) {
          player.jumpSpeedBoost += 20f;
          if (Main.rand.NextFloat() < 0.7f) {
            Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 172, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          }
          Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0f, 0.6f, 0.9f);
        }
        if (glideAnimTimer > 0) {
          player.maxFallSpeed = 2f;
        }
        else {
          player.maxFallSpeed = 25f;
        }
      }
      if (transforming) {
        if (transformTimer > 235 && !(transformTimer >= 236 && transformTimer <= 239)) {
          player.velocity = new Vector2(0, -0.00055f * transformTimer);
          player.gravity = 0;
          if (featherTrailTimer == 0) {
            Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
            dust.noGravity = false;
            dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
            featherTrailTimer = Main.rand.Next(3, 8);
          }
          float a = player.AngleFrom(blockLocation) + (float)Math.PI + (Main.rand.Next(-1, 1) / Main.rand.Next(10, 20));
        }
        if (transformTimer >= 236 && transformTimer <= 239) {
          player.gravity = 9f;
        }
        player.direction = transformDirection;
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.immune = true;
      }
    }
    public override void FrameEffects() {
      if (!OriSetPrevious) { return; }

      if (player.velocity.Y != 0 || player.velocity.X != 0) {
        if (featherTrailTimer == 0) {
          Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
          dust.noGravity = false;
          featherTrailTimer = oriDashing ? Main.rand.Next(2, 4) : Main.rand.Next(10, 15);
        }
      }
      else if (oriDashing && featherTrailTimer > 4) {
        featherTrailTimer = Main.rand.Next(2, 4);
      }
      OriFlashing = flashTimer > 0 ? OriFlashing = flashPattern.Contains(flashTimer) : false;
    }
    public void BashEffects(NPC target) {
      bashActiveTimer = 100;
      bashActivate = 0;
      bashActive = true;
      PlayNewSoundVolume("Ori/Bash/seinBashStartA", /*0.7f*/ Main.soundVolume);
      bashPosition = player.Center;
      player.pulley = false;
      bashNPC = target.whoAmI;
      bashNPCPosition = target.Center;
      bashFrameUp = (2.0f >= bashNPCAngle && bashNPCAngle >= 1.3f);
      if (bashActiveTimer == 6) {
        target.HitEffect(dmg: 15);
        target.velocity = new Vector2((float)(Math.Cos(bashAngle) * bashDistance), (float)(Math.Sin(bashAngle) * bashDistance));
      }
    }

    public override void OnEnterWorld(Player player) {
      seinPosition = player.position;
      
      FrameHandler.Init(Main.player[Main.myPlayer].GetModPlayer<OriPlayer>());
    }
    public override void OnHitByNPC(NPC npc, int damage, bool crit) {
      if (OriSetPrevious) {
        if (stomping || chargeJumpAnimTimer > 0) {
          damage = 0;
        }
        if (bashActivate > 0 && bashActiveTimer == 0 && !bashActive && !countering) {
          if ((counterBashed.Contains(npc.type) || npc.boss == true || npc.immortal) && !countering) {
            Counter();
            damage = 0;
          }
          else {
            BashEffects(npc);
            damage = 0;
          }
        }
      }
    }
    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) //effects when character is hurt
    {
      if (OriSetPrevious && playSound) //&& basically looks for both to be true
      {
        playSound = false; //stops regular hurt sound from playing
        genGore = false; //stops regular gore from appearing
        if (bashActiveTimer > 0 || bashActive || stomping || intoStomp || chargeJumpAnimTimer > 0) {
          damage = 0;
        }
        else {
          flashTimer = 53;
          PlayNewSoundVolume("Ori/Hurt/seinHurtRegular" + RandomChar(5), /*0.6f*/ Main.soundVolume); //plays randomly oridamage 1 or 2, rand 1,3 generates between 1 and 2 for some reason
          unrestrictedMovement = true;
        }
      }
      return true;
    }
    public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) { }
    public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit) { }
    public override TagCompound Save() {
      return new TagCompound {
        {"OriSet", OriSet},
        {"OriSetPrevious", OriSetPrevious},
        {"bash", abilityBash},
        {"jumps", oriAirJumps},
        {"feather", hasFeather},
        {"water", waterBreath},
        {"climb", canClimb},
        {"dash", abilityDash},
        {"chargejump", abilityChargeJump},
        {"walljump", abilityWallJump},
      };
    }
    public override void Load(TagCompound tag) {
      OriSet = tag.GetBool("OriSet");
      OriSetPrevious = tag.GetBool("OriSetPrevious");
      abilityBash = tag.GetBool("bash");
      oriAirJumps = tag.GetInt("oriAirJumps");
      hasFeather = tag.GetBool("feather");
      waterBreath = tag.GetBool("water");
      canClimb = tag.GetBool("climb");
      abilityDash = tag.GetBool("dash");
      abilityChargeJump = tag.GetBool("chargejump");
      abilityWallJump = tag.GetBool("walljump");
    }
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { //similar to prehurt, but for death
      if (OriSetPrevious) {
        genGore = false;
        playSound = false;
        if (damageSource.SourceOtherIndex == 1) {
          PlayNewSoundVolume("Ori/Death/seinSwimmingDrowningDeath" + RandomChar(3), 3f);
        }
        else if (damageSource.SourceOtherIndex == 2) {
          PlayNewSound("Ori/Death/seinDeathLava" + RandomChar(5));
        }
        else {
          PlayNewSound("Ori/Death/seinDeathRegular" + RandomChar(5));
        }
        /*Main.NewText(damageSource.GetDeathText(player.name));
        Main.NewText(damageSource.SourceOtherIndex);*/
        if (oriDeathParticles) {
          for (int i = 0; i < 15; i++) { //does particles
            Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          }
        }
      }
      return true;
    }
    public override void ModifyDrawLayers(List<PlayerLayer> layers) {
      if (seinActive) {
        layers.Insert(0, seinSprite);
        seinSprite.visible = true;
      }
      if (bashActive) {
        layers.Insert(0, oriBashArrow);
        oriBashArrow.visible = true;
      }
      layers.Insert(9, oriPlayerSprite);
      layers.Insert(0, OriTrail);
      layers.Insert(0, oriTransformSprite);
      oriTransformSprite.visible = (transforming && transformTimer >= 236);
      if (OriSetPrevious) {
        player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
      }
      if (OriSetPrevious || transforming) {
        PlayerLayer.Skin.visible = false;
        PlayerLayer.Arms.visible = false;
        PlayerLayer.Body.visible = false;
        PlayerLayer.Face.visible = false;
        PlayerLayer.Head.visible = false;
        PlayerLayer.Legs.visible = false;
        PlayerLayer.WaistAcc.visible = false;
        PlayerLayer.NeckAcc.visible = false;
        PlayerLayer.ShieldAcc.visible = false;
        PlayerLayer.FaceAcc.visible = false;
        PlayerLayer.Hair.visible = false;
        PlayerLayer.ShoeAcc.visible = false;
        PlayerLayer.HandOnAcc.visible = false;
        PlayerLayer.HandOffAcc.visible = false;

        if (OriSetPrevious || transformTimer < 236) {
          oriPlayerSprite.visible = (!player.dead && !player.invis);
          OriTrail.visible = (!player.dead && !player.invis && !player.mount.Cart);
        }
      }
      else {
        OriTrail.visible = false;
        oriPlayerSprite.visible = false;
      }
    }

    private static float Offset(OriPlayer modPlayer)
    {
      if (modPlayer.OriFrame.X / 104 == 1 && modPlayer.OriFrame.Y / 76 == 11) {
        return 8;
      }
      if (modPlayer.OriFrame.X == 0 && modPlayer.OriFrame.Y / 76 == 17) {
        return 24;
      }
      if (modPlayer.OriFrame.X / 104 == 1 && modPlayer.OriFrame.Y / 76 == 16) {
        return 14;
      }
      else {
        return 0;
      }
    }
    public static readonly PlayerLayer OriTrail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer modPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);

      if (modPlayer.trailPos == null) {
        modPlayer.trailPos = new List<Vector2>();
        modPlayer.trailFrame = new List<Vector2>();
        modPlayer.trailAlpha = new List<float>();
        modPlayer.trailDirection = new List<int>();
        modPlayer.trailRotation = new List<float>();

        for (int i = 0; i <= 27; i++) {
          modPlayer.trailPos.Add(Vector2.Zero);
          modPlayer.trailFrame.Add(Vector2.Zero);
          modPlayer.trailRotation.Add(0);
          modPlayer.trailDirection.Add(1);
        }
        float[] trailAlphas = { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80, 84, 88, 92, 96, 100, 104 };
        modPlayer.trailAlpha.AddRange(trailAlphas);
      }

      Vector2 position = drawPlayer.position;

      //modPlayer.UpdateTrail(drawPlayer);

      modPlayer.UpdateFrame(drawPlayer);
      for (int i = 0; i <= 25; i++) {
        if (modPlayer.trailAlpha[i] - 4 > 0) {
          modPlayer.trailAlpha[i] -= 4;
        }
        if (modPlayer.trailAlpha[i] - 4 < 0) {
          modPlayer.trailAlpha[i] = 0;
        }
      }
      if (!drawPlayer.dead && !drawPlayer.invis) {
        modPlayer.trailUpdate++;
        if (modPlayer.trailUpdate > 25) {
          modPlayer.trailUpdate = 0;
        }
        modPlayer.trailPos[modPlayer.trailUpdate] = drawPlayer.position;
        modPlayer.trailFrame[modPlayer.trailUpdate] = modPlayer.OriFrame;
        modPlayer.trailDirection[modPlayer.trailUpdate] = drawPlayer.direction;
        modPlayer.trailAlpha[modPlayer.trailUpdate] = 2 * (float)(Math.Sqrt(Math.Pow(Math.Abs(drawPlayer.velocity.X), 2) + Math.Pow(Math.Abs(drawPlayer.velocity.Y), 2))) + 44;
        modPlayer.trailRotation[modPlayer.trailUpdate] = modPlayer.rotRads;
        if (modPlayer.trailAlpha[modPlayer.trailUpdate] > 104) {
          modPlayer.trailAlpha[modPlayer.trailUpdate] = 104;
        }
      }
      for (int i = 0; i <= 25; i++) {
        SpriteEffects effect = SpriteEffects.None;

        if (modPlayer.trailDirection[i] == -1) {
          effect = SpriteEffects.FlipHorizontally;
        }

        DrawData data = new DrawData(
          mod.GetTexture("PlayerEffects/OriGlowRight"),
          new Vector2(
            (modPlayer.trailPos[i].X - Main.screenPosition.X) + 10,
            (modPlayer.trailPos[i].Y - Main.screenPosition.Y) + 8 + Offset(modPlayer)
          ),
          new Rectangle(
            (int)(modPlayer.trailFrame[i].X),
            (int)(modPlayer.trailFrame[i].Y), 104, 76),
          Color.White * ((modPlayer.trailAlpha[i] / 2) / 255),
          modPlayer.trailRotation[i],
          new Vector2(52, 38), 1, effect, 0
        );
        Main.playerDrawData.Add(data);
      }
      //public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    public static readonly PlayerLayer oriPlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer modPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/OriPlayer");
      SpriteEffects effect = SpriteEffects.None;

      if (modPlayer.transforming) {
        // TODO: Change to TransformEnd animation
        modPlayer.OriFrame.X = 0;
        int t = modPlayer.transformTimer;
        if (t <= 6) {
          modPlayer.OriFrame.Y = 25 * 76;
        }
        else if (t <= 56) {
          modPlayer.OriFrame.Y = 24 * 76;
        }
        else if (t <= 62) {
          modPlayer.OriFrame.Y = 23 * 76;
        }
        else if (t <= 122) {
          modPlayer.OriFrame.Y = 22 * 76;
        }
        else if (t <= 132) {
          modPlayer.OriFrame.Y = 21 * 76;
        }
        else if (t <= 172) {
          modPlayer.OriFrame.Y = 20 * 76;
        }
        else if (t <= 175) {
          modPlayer.OriFrame.Y = 19 * 76;
        }
        else if (t <= 235) {
          modPlayer.OriFrame.Y = 18 * 76;
        }
      }
      if (drawPlayer.direction == -1) {
        effect = SpriteEffects.FlipHorizontally;
      }

      DrawData data =
        new DrawData(texture,
        new Vector2(
          (drawPlayer.position.X - Main.screenPosition.X) + 10,
          (drawPlayer.position.Y - Main.screenPosition.Y) + 8 + Offset(modPlayer)
        ),
        new Rectangle(
          (int)(modPlayer.OriFrame.X),
          (int)(modPlayer.OriFrame.Y), 104, 76),
        Color.White,
        drawPlayer.direction * modPlayer.rotRads,
        new Vector2(52, 38), 1, effect, 0);
      Main.playerDrawData.Add(data);
      //public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    public static readonly PlayerLayer oriTransformSprite = new PlayerLayer("OriMod", "OriTransform", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer modPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/transform");
      SpriteEffects effect = SpriteEffects.None;

      if (drawPlayer.direction == -1) {
        effect = SpriteEffects.FlipHorizontally;
      }
      int y = 0;
      if (modPlayer.transformTimer > 236) {
        int t = modPlayer.transformTimer - 235;
        if (t > 0) {
          if (t >= 391) {
            y = 0;
          }
          else if (t >= 331) {
            y = 1;
          }
          else if (t >= 271) {
            y = 2;
          }
          else if (t >= 151) {
            y = 3;
          }
          else if (t >= 121) {
            y = 4;
          }
          else if (t >= 91) {
            y = 5;
          }
          else if (t >= 61) {
            y = 6;
          }
          else if (t >= 31) {
            y = 7;
          }
          else if (t >= 1) {
            y = 8;
          }
        }
      }
      DrawData data = new DrawData(
        texture,
        new Vector2(
          (drawPlayer.position.X - Main.screenPosition.X) + 10,
          (drawPlayer.position.Y - Main.screenPosition.Y) + 8 + Offset(modPlayer)
        ),
        new Rectangle(0, y * 76, 104, 76),
        Color.White, drawPlayer.direction * modPlayer.rotRads,
        new Vector2(52, 38), 1, effect, 0);
      Main.playerDrawData.Add(data);
      //public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    public static readonly PlayerLayer seinSprite = new PlayerLayer("OriMod", "seinSprite", delegate (PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer modPlayer = player.GetModPlayer<OriPlayer>(mod);
      Vector2 position = player.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/Sein");
      SpriteEffects effect = SpriteEffects.None;
      if (modPlayer.seinPosition.HasNaNs()) {
        modPlayer.seinPosition = player.position;
      }
      Vector2 calcPosition = modPlayer.seinPosition - player.Center;
      calcPosition.X += 30 * -player.direction;
      calcPosition.Y -= 20;
      if (calcPosition.X >= 6 || calcPosition.X <= -6 || calcPosition.Y >= 6 || calcPosition.Y <= -6) {
        //pre movement checks
        modPlayer.seinPingTimer--;
        if (modPlayer.seinPing.Any() && modPlayer.seinPing.Count > 500) {
          modPlayer.seinPing.Clear();
        }
        if (
          modPlayer.seinPing.Any() && (
            (
              modPlayer.seinPosition.X >= modPlayer.seinPing[0].X + 6 &&
              modPlayer.seinPosition.X <= modPlayer.seinPing[0].X - 6
            ) && (
              modPlayer.seinPosition.Y >= modPlayer.seinPing[0].Y + 6 &&
              modPlayer.seinPosition.Y <= modPlayer.seinPing[0].Y - 6)
            )
          ) {
          modPlayer.seinPing.RemoveAt(0);
        }

        /*
        modPlayer.seinPosition = player.position;
        modPlayer.seinPing.Clear();
        modPlayer.seinMovementAngle = 0;
        */

        //movement
        if (modPlayer.seinPing.Any() && (modPlayer.seinPingTimer == 0 || modPlayer.seinPingTimer < 0)) {
          modPlayer.seinStartMoving = false;
        }
        if (modPlayer.seinPing.Any() && modPlayer.seinPing[0] == new Vector2(-20, 1)) {
          modPlayer.seinPing.RemoveAt(0);
        }
        if (!modPlayer.seinStartMoving) {
          if (modPlayer.seinPing.Any() && modPlayer.seinPing.Count > 0) {
            float a = (float)Math.Atan2((modPlayer.seinPing[0].X - modPlayer.seinPosition.X), 
              (modPlayer.seinPing[0].Y - modPlayer.seinPosition.Y)) + (float)Math.PI;
            if (a > Math.PI) {
              a -= 2 * (float)Math.PI;
            }
            if (a < 0 - Math.PI) {
              a += 2 * (float)Math.PI;
            }
            if (float.IsNaN(modPlayer.seinMovementAngle)) {
              modPlayer.seinMovementAngle = 0;
            }
            float b = modPlayer.seinMovementAngle;
            if (b != a) {
              /*if (modPlayer.seinMovedRecently == false)
              {
                if (Main.rand.Next(0, 1) == 1)
                {
                  modPlayer.seinMovementAngle = player.AngleFrom(modPlayer.seinPosition);
                }
                else
                {
                  modPlayer.seinMovementAngle = player.AngleTo(modPlayer.seinPosition);
                }
                //modPlayer.seinMovementAngle += Main.rand.NextFloat(-0.5f, 0.5f);
                modPlayer.seinMovedRecently = true;
              }*/
              float c = b - a; //?????
              if (c > (float)Math.PI) {
                c -= 2 * (float)Math.PI;
              }
              if (c < -(float)Math.PI) {
                c += 2 * (float)Math.PI;
              }
              if (c == (float)Math.PI || b - a == -(float)Math.PI) {
                a += Main.rand.NextFloat(-0.01f, 0.01f);
              }
              c /= 3f;
              c += a;
              modPlayer.seinMovementAngle = c;
            }
          }
          float multiplier = -((player.Distance(modPlayer.seinPosition) / 60) + 1.5f);
          if (multiplier > 5) {
            multiplier = 5;
          }

          modPlayer.seinPosition.X += multiplier * (float)Math.Cos(modPlayer.seinMovementAngle);
          modPlayer.seinPosition.Y += multiplier * (float)Math.Sin(modPlayer.seinMovementAngle);
        }
        if (modPlayer.seinPingTimer == 0 || modPlayer.seinPingTimer < 0) {
          Vector2 newPosition = player.Center;
          newPosition.X += 30 * -player.direction;
          newPosition.Y -= 20;
          modPlayer.seinPing.Add(newPosition);
          modPlayer.seinPingTimer = 60;
        }
        //timer advance
        modPlayer.seinPingTimer--;
      }
      else {
        modPlayer.seinPingTimer = 60;
        modPlayer.seinMovedRecently = false;
        modPlayer.seinStartMoving = true;
        if (modPlayer.seinPing.Any()) {
          modPlayer.seinPing.Clear();
        }
      }

      DrawData data = new DrawData(
        texture,
        new Vector2(
          (modPlayer.seinPosition.X - Main.screenPosition.X),
          (modPlayer.seinPosition.Y - Main.screenPosition.Y)
        ),
        new Rectangle(0, 0, 28, 28),
        Color.White, 0,
        new Vector2(14, 14), 1, effect, 0);
        Main.playerDrawData.Add(data);
      //this is debug, it shows the point in which sein is supposed to move toward
      
      if (modPlayer.seinPing.Any())
      {
        DrawData data2 = new DrawData(texture,
          new Vector2(
            (modPlayer.seinPing[0].X - Main.screenPosition.X),
            (modPlayer.seinPing[0].Y - Main.screenPosition.Y)
          ),
          new Rectangle(0, 0, 28, 28),
          Color.White, 0,
          new Vector2(14, 14), 1, effect, 0);
        Main.playerDrawData.Add(data2);
      }
      Main.playerDrawData.Add(data);
    });
    public static readonly PlayerLayer oriBashArrow = new PlayerLayer("OriMod", "bashArrow", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer modPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/bashArrow");
      SpriteEffects effect = SpriteEffects.None;

      int frameY = 0;

      if (modPlayer.bashActiveTimer < 55 && modPlayer.bashActiveTimer > 45) {
        frameY = 1;
      }
      else if (modPlayer.bashActiveTimer < 46) {
        frameY = 2;
      }
      DrawData data = new DrawData(texture,
        new Vector2(
          (Main.npc[modPlayer.bashNPC].Center.X - Main.screenPosition.X),
          (Main.npc[modPlayer.bashNPC].Center.Y - Main.screenPosition.Y)
        ),
        new Rectangle(0, frameY * 20, 152, 20),
        Color.White, Main.npc[modPlayer.bashNPC].AngleTo(Main.MouseWorld),
        new Vector2(76, 10), 1, effect, 0);
      Main.playerDrawData.Add(data);
      //public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });

    public static void Send(int toWho, int fromWho, ModPlayer fromPlayer, Mod mod) {
      OriPlayer oPlayer = fromPlayer.player.GetModPlayer<OriPlayer>();
      ModPacket packet = mod.GetPacket();
      if (Main.netMode == NetmodeID.Server) {
        packet.Write(fromPlayer.player.whoAmI);
      }
      packet.Write(oPlayer.OriSet);
      packet.Write(oPlayer.OriSetPrevious);
      packet.WriteVector2(oPlayer.OriFrame);
      packet.Write(oPlayer.OriFlashing);
      // packet.Send(clientPlayer.player.whoAmI, player.whoAmI);
      packet.Send(toWho, fromWho);
    }

    /*
    public override void clientClone(ModPlayer clientClone)
    {
      OriPlayer clone = clientClone as OriPlayer;
      clone.OriSet = OriSet;
      clone.OriSetPrevious = OriSetPrevious;
    }
    */


    public override void ResetEffects() {
      OriSetPrevious = OriSet;
      if (transformTimer > 0) {
        transformTimer--;
        if (transformTimer == 1) {
          transforming = false;
          OriSet = true;
          OriSetPrevious = true;
        }
      }
      
      if (OriSetPrevious) {
        if (bashActivate > 0) { bashActivate--; }
        if (bashActiveTimer > 0) { bashActiveTimer--; }
        if (bashActiveTimer < 0) { bashActiveTimer = 0; }
        if (dashAnimation > 0) { dashAnimation--; }
        if (dashTimer > 0) { dashTimer--; }
        if (dashDelay > 0) { dashDelay--; }
        if (flashTimer > 0) { flashTimer--; }
        if (featherTrailTimer > 0) { featherTrailTimer--; }
        if (frameXset) { frameXset = false; }
        if (immuneTimer > 0) { immuneTimer--; }
        if (intoStompTimer > 0) { intoStompTimer--; }
        if (oriDoubleJumpAnimTimer > 0) { oriDoubleJumpAnimTimer--; }
        if (outOfStompTimer > 0) { outOfStompTimer--; }
        if (stompHitboxTimer > 0) { stompHitboxTimer--; }
        if (stompingTimer > 0) { stompingTimer--; }
        if (wallJumpAnimTimer > 0) { wallJumpAnimTimer--; }
        if (!lookUp) { lookUpTimer = 1; }
        slideAnimate = false;
        animRefreshed = false;

        if (player.velocity.Y != 0) {
          oriFallingAnimTimer++;
          if (oriFallingAnimTimer > 28) { oriFallingAnimTimer = 1; }
        }
        else { oriFallingAnimTimer = 1; }

        if (groundedOnWall) {
          groundedWallTimer++;
          if (groundedWallTimer > 49) { groundedWallTimer = 1; }
        }
        else { groundedWallTimer = 1; }

        if (chargeJumpAnimTimer > 0) {
          chargeJumpAnimTimer--;
          stompHitboxTimer = 3;
        }

        if (counterTimer > 0) {
          counterTimer--;
          if (counterTimer == 0) { countering = false; }
        }
      }
      if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
        Send(255, player.whoAmI, player.GetModPlayer<OriPlayer>(), mod);
      }
    }
  }
}