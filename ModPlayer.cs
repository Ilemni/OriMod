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
using Terraria.World;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

namespace OriMod
{
  public class OriPlayer : ModPlayer {
    
    #region Variables
    public MovementHandler Movement; // Class used for all of Ori's movements

    // OriSet, detecting whether or not Ori is active or not. The name is a remnant of when Ori was activated using the accessory located in Items/Ori/OriAccessory
    public bool OriSet = false;

    // Transform variables used to hasten additional transforms
    public bool hasTransformedOnce = false;
    public readonly float repeatedTransformRate = 2.5f;

    // Variables relating to fixing movement when Ori is active, such that you aren't slowed down mid-air after bashing.
    public bool isGrounded = false;
    public bool unrestrictedMovement = false;

    // Variables relating to Bash
    public int bashActivate = 0;
    public int bashActiveTimer = 0;
    public float bashAngle = 0;
    public float bashDistance = 20f;
    public bool bashActive = false;
    public bool bashFrameUp = false;
    public float bashNPCAngle = 0;
    public Vector2 bashPosition = new Vector2(0, 0);
    public bool abilityBash = true;
    public bool countering = false;
    public int counterTimer = 0;
    public static readonly List<int> CannotBash = new List<int> {
      NPCID.BlazingWheel, NPCID.SpikeBall
    };
    public int bashNPC = 0;
    public bool tempInvincibility = false;
    public int immuneTimer = 0;
    public Vector2 bashNPCPosition = Vector2.Zero;

    // Variables relating to Air Jumping
    public int oriAirJumps = 2; // Only used in Save/Load

    // Variables relating to Dashing
    public bool abilityDash = true; // Only used in Save/Load

    // Variables relating to Wall Jumping
    public bool abilityWallJump = true; // Only used in Save/Load
    public bool onWall = false;

    // Variables relating to Climbing
    public bool canClimb = true; // Only used in Save/Load

    // Variables relating to Charge Jumping
    public bool charged = false;
    public int chargeTimer = 0;
    public int chargeUpTimer = 40;
    public bool abilityChargeJump = true;
    public int chargeJumpAnimTimer = 0;
    public bool upRefresh = false;

    // Variables relating to looking up
    public bool intoLookUp = false;
    public int intoLookUpTimer = 0;
    public bool lookUp = false;
    public int lookUpTimer = 0;
    public bool outLookUp = false;
    public int outLookUpTimer = 0;

    // Variables relating to Stomping
    public bool intoStomp = false;
    public bool stomping = false;
    public bool outOfStomp = false;
    public int intoStompTimer = 0;
    public int stompingTimer = 0;
    public int outOfStompTimer = 0;
    public float preHeight = 0;
    public int stompHitboxTimer = 0;
    public bool waterBreath = false; // Only used in Save/Load

    // Variables relating to Crouching
    public bool crouching = false;
    public bool intoCrouch = false;
    public int intoCrouchTimer = 0;
    public bool outCrouch = false;
    public int outCrouchTimer = 0;

    // Variables relating to Back Flipping
    public bool backflipping = false;

    // Variables relating to Kuro's Feather
    public bool hasFeather = true; // Only used in Save/Load

    // Variables relating to visual or audible effects
    public bool doOriDeathParticles = true;
    public string floorMaterial = "Grass";
    public string wallMaterial = "Grass";
    

    // Variables relating to Sein
    public bool seinMinionActive;
    public int seinMinionUpgrade;

    // Variables relating to Transforming
    public float transformTimer = 0;
    public bool transforming = false;
    public Vector2 blockLocation = Vector2.Zero;
    public int transformDirection = 1;
    public bool animatedTransform = true;

    // Footstep materials
    public List<int> grassFloorMaterials;
    public List<int> lightDarkFloorMaterials;
    public List<int> mushroomFloorMaterials;
    public List<int> rockFloorMaterials;
    public List<int> sandFloorMaterials;
    public List<int> snowFloorMaterials;
    public List<int> spiritTreeRockFloorMaterials;
    public List<int> spiritTreeWoodFloorMaterials;
    public List<int> woodFloorMaterials;

    // Wall materials
    public List<int> grassWallMaterials;
    public List<int> lightDarkWallMaterials;
    public List<int> mushroomWallMaterials;
    public List<int> rockWallMaterials;
    public List<int> woodWallMaterials;

    // Trail variables, for the trails Ori creates
    public List<Vector2> trailPos;
    public List<Vector2> trailFrame;
    public List<float> trailAlpha;
    public List<float> trailRotation;
    public List<int> trailDirection;
    public int trailUpdate = 0;

    public int featherTrailTimer = 0;

    // Animation Variables
    public const int spriteWidth = 104;
    public const int spriteHeight = 76;
    public Vector2 AnimFrame = Vector2.Zero;
    public Vector2 AnimTile {
      get { return PixelToTile(AnimFrame); }
      set { AnimFrame = TileToPixel(value); }
    }
    public string AnimName = "Default";
    public int AnimIndex = 0;
    public float AnimTime = 0; // Intentionally a float
    public float AnimRads = 0f;
    public bool AnimReversed = false;
    public bool flashing = false;
    public int flashTimer = 0;
    private static readonly int[] flashPattern = new int[] {
      53,52,51,50,45,
      44,43,38,37,36,
      31,30,29,24,23,
      22,17,16,15,10,
       9, 8, 3, 2, 1
    };
    private int footstepRand = 0;

    #endregion
    public Vector2 PixelToTile(Vector2 pixel) {
      pixel.X = (int)(pixel.X / spriteWidth);
      pixel.Y = (int)(pixel.Y / spriteHeight);
      return pixel;
    }
    public static Vector2 TileToPixel(int x, int y) {
      return TileToPixel(new Vector2(x, y));
    }
    public static Vector2 TileToPixel(Vector2 tile) {
      tile.X *= spriteWidth;
      tile.Y *= spriteHeight;
      return tile;
    }
    // basic sound playing method, with paths starting after NewSFX in the file structure
    public SoundEffectInstance PlayNewSound(string Path) {
      return PlayNewSound(Path, 1, 0);
    }
    public SoundEffectInstance PlayNewSound(string Path, float Volume) {
      return PlayNewSound(Path, Volume, 0);
    }
    public SoundEffectInstance PlayNewSound(string Path, float Volume, float Pitch) {
      return Main.PlaySound((int)SoundType.Custom, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/" + Path), Volume, Pitch);
    }
    public SoundEffectInstance PlayFootstep(string Material, int rand, float Volume, float Pitch) {
      char randChar = RandomChar(rand, ref footstepRand);
      return PlayNewSound("Ori/Footsteps/" + Material + "/" + Material + randChar, Volume, Pitch);
    }

    public void DoCounter() {
      countering = true;
      counterTimer = 15;
      PlayNewSound("ori/Grenade/seinGrenadeExplode" + RandomChar(2));
    }

    private float DegreeToRadian(float angle) { // i dont know why i put this here
      angle = angle % 360;
      if (angle < 0) {
        angle += 360;
      }
      return (float)(Math.PI * angle) / 180.0f;
    }
    private float RadianToDegree(float rad) {
      return (float)(rad * 180 / Math.PI);
    }

    public static char RandomChar(int length) { // Returns random letter based on length. Primarily used for sound effects
      char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
      return alphabet[Main.rand.Next(length)];
    }
    public static char RandomChar(int length, ref int exclude) {
      char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
      if (exclude >= 0 && exclude < length) {
        int[] ints = new int[length - 1];
        int n = 0;
        for (int i = 0; i < length; i++) {
          if (i != exclude) {
            ints[n] = i;
            n++;
          }
        }
        int num = Main.rand.Next(ints);
        exclude = num;
        return alphabet[num];
      }
      else {
        exclude = length - 1;
        return RandomChar(length);
      }
    }
    
    // Class with all necessary animation frame info, should make frame work much more managable
    public void Increment(string anim="Default", int overrideFrame=0, float overrideTime=0, int overrideDur=0, Vector3 overrideMeta=new Vector3(), Vector2 drawOffset=new Vector2(), float rotDegrees=0) {
      // Main.NewText("Frame called: " + AnimName + ", Time: " + AnimTime + ", AnimIndex: " + AnimIndex); // Debug
      AnimationHandler.IncrementFrame(this, anim, overrideFrame, overrideTime, overrideDur, overrideMeta, drawOffset, rotDegrees);
    }
    
    public void SetFrame(string name, int frameIndex, float time, Vector3 frame) {
      SetFrame(name, frameIndex, time, new Vector2(frame.X, frame.Y));
    }
    public void SetFrame(string name, int frameIndex, float time, Vector2 frame) {
      AnimName = name;
      AnimIndex = frameIndex;
      AnimTime = time;
      AnimFrame = TileToPixel(frame);
    }

    public void UpdateFrame(Player drawPlayer) {
      if (!OriSet || transforming) return;
      AnimTime++;
      if (player.whoAmI != Main.myPlayer) {
        // Increment(AnimName);
        return;
      }
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>();

      if (Movement.IsInUse("AirJump") && !(Movement.IsInUse("Dash") || Movement.IsInUse("ChargeDash"))) {
        Increment("AirJump");
        AnimRads = AnimTime * 0.8f;
        return;
      }
      if (Movement.IsInUse("Glide")) {
        if (Movement.IsState("Glide", MovementHandler.State.Starting)) Increment("GlideStart");
        else if (Movement.IsState("Glide", MovementHandler.State.Active)) Increment("Glide");
        else Increment("GlideStart", overrideMeta:new Vector3(0, 0, 3));
        return;
      }
      if (Movement.IsInUse("Dash") || Movement.IsInUse("ChargeDash")) {
        if (Math.Abs(player.velocity.X) > 18f) {
          Increment("Dash");
        }
        else {
          Increment("Dash", overrideFrame:2);
        }
        return;
      }
      if (Movement.IsInUse("WallJump")) {
        Increment("WallJump");
        return;
      }
      if (OriSet && !transforming && !hasTransformedOnce) {
        hasTransformedOnce = true;
      }
      // this controls animation frames. have fun trying to figure out how it works
      
      if (drawPlayer.mount.Cart) {
        Increment("Default");
        // TODO: Minecart animation?
        return;
      }
      if (intoCrouch || outCrouch) {
        Increment("CrouchStart");
        return;
      }
      if (crouching) {
        Increment("Crouch");
        return;
      }
      if (intoLookUp) {
        Increment("LookUpStart");
        return;
      }
      if (outLookUp) {
        Increment("LookUpStart", overrideMeta:new Vector3(0, 2, 0));
        return;
      }
      if (lookUp) {
        if (lookUpTimer < 10) {
          Increment("LookUpStart");
        }
        else {
          Increment("LookUp");
        }
        lookUpTimer++;
        return;
      }
      if (bashFrameUp && bashActive) {
        Increment("Bash");
        return;
      }
      if (intoStomp) {
        Increment("AirJump");
        AnimRads = AnimTime;
        return;
      }
      if (stomping) {
        Increment("ChargeJump", rotDegrees:180f, overrideDur:2, overrideMeta:new Vector3(0,2,0));
        return;
      }
      if (bashActive) {
        Increment("Bash");
        return;
      }
      if (OriMod.ClimbKey.Current && onWall && !PlayerInput.Triggers.Current.Up && !PlayerInput.Triggers.Current.Down) {
        Increment("ClimbIdle");
        return;
      }
      if (onWall && !isGrounded) {
        if (Movement.IsInUse("Climb") && player.velocity.Y < 0) {
          Increment("Climb", overrideTime:AnimTime+Math.Abs(drawPlayer.velocity.Y)*0.1f);
        }
        else {
          Increment("WallSlide");
        }
        return;
      }
      if (countering) {
        // TODO: Figure out what countering is
        Increment("Default");
        // PlayerFrame(0, 5);
        return;
      }
      if (chargeJumpAnimTimer > 0) {
        Increment("ChargeJump");
        if (chargeJumpAnimTimer > 17) {
          drawPlayer.controlJump = false;
        }
        return;
      }
      if (!isGrounded && !Movement.IsInUse("Glide")) {
        Increment(drawPlayer.velocity.Y < 0 ? "Jump" : "Falling");
        return;
      }
      if (isGrounded && Math.Abs(drawPlayer.velocity.X) < 0.2f) {
        Increment(onWall ? "IdleAgainst" : "Idle");
        return;
      }
      if (drawPlayer.velocity.X != 0 && isGrounded &&
        !Movement.IsInUse("Dash") &&
        !bashActive &&
        !onWall && (
          PlayerInput.Triggers.Current.Left ||
          PlayerInput.Triggers.Current.Right)
      ) {
        Increment("Running", overrideTime:AnimTime+(int)Math.Abs(player.velocity.X) / 3);
        if (AnimIndex == 4 || AnimIndex == 9) {
          TestStepMaterial(drawPlayer);
          switch (floorMaterial) {
            case "Grass":
            case "Mushroom":
              PlayFootstep(floorMaterial, 5, 0.15f, 0.1f);
              break;
            case "Water":
              PlayFootstep(floorMaterial, 4, 1f, 0.1f);
              break;
            case "SpiritTreeRock":
            case "SpiritTreeWood":
            case "Rock":
              PlayFootstep(floorMaterial, 5, 1f, 0.1f);
              break;
            case "Snow":
            case "LightDark":
              PlayFootstep(floorMaterial, 10, 0.85f, 0.1f);
              break;
            case "Wood":
              PlayFootstep(floorMaterial, 5, 0.85f, 0.1f);
              break;
            case "Sand":
              PlayFootstep(floorMaterial, 8, 0.85f, 0.1f);
              break;
          }

          Vector2 position = new Vector2(
            drawPlayer.Center.X + (drawPlayer.direction == -1 ? -4 : 2),
            drawPlayer.position.Y + drawPlayer.height - 2);
          for (int i = 0; i < 4; i++) {
            Dust dust = Main.dust[Terraria.Dust.NewDust(position, 2, 2, 111, 0f, -2.7f, 0, new Color(255, 255, 255), 1f)];
            dust.noGravity = true;
            dust.scale = 0.75f;
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
            dust.shader.UseColor(Color.White);
            dust.fadeIn = 0.03947368f;
          }
        }
      }
    }
    public void DoTransformation(Player player) {
      transforming = true;
      transformDirection = player.direction;
      transformTimer = 627;
    }
    public void InitTestMaterial() {
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
      Vector2 pos = new Vector2(player.Center.X + offsetX, (player.position.Y + player.height) + offsetY);
      Vector2 tilepos = new Vector2(pos.ToTileCoordinates().X, pos.ToTileCoordinates().Y);
      return Main.tile[(int)tilepos.X, (int)tilepos.Y];
    }
    public virtual void TestStepMaterial(Player player) { // oh yeah good luck understanding what this is
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
    public virtual void TestWallMaterial(Player player) { // or this, im too tired to comment them
      Tile tile = getTile(-2f, 34f);
      if (tile.active()) {
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

      isGrounded = false;
      // Check if grounded by means of liquid walking
      if (player.fireWalk || player.waterWalk2 || player.waterWalk) {
        Vector2 checkPos = new Vector2(player.position.X + (player.width / 2), player.position.Y + player.height + 4);
        Vector2 check2 = new Vector2(checkPos.ToTileCoordinates().X, checkPos.ToTileCoordinates().Y);
        bool testblock =
          Main.tile[(int)check2.X, (int)check2.Y].liquid > 0 &&
          Main.tile[(int)check2.X, (int)check2.Y - 1].liquid == 0;
        if (testblock) {
          Tile liquidTile = Main.tile[(int)check2.X, (int)check2.Y];
          isGrounded = liquidTile.lava() ? player.fireWalk : player.waterWalk;
        }
      }
      if (!isGrounded) {
        isGrounded = !Collision.IsClearSpotTest(player.position + new Vector2(0, 8), 16f, player.width, player.height, false, false, (int)player.gravDir, true, true);
      }

      // thanks jopo

      Lighting.AddLight(player.Center, 0.4f, 0.8f, 0.8f);

      if (transforming) {
        player.direction = transformDirection;
      }

      if (!OriSet) { return; }
      if (player.mount.Cart) {
        intoStomp = false;
        intoStompTimer = 0;
        stomping = false;
        stompingTimer = 0;
        outOfStomp = false;
        outOfStompTimer = 0;
      }
      // Moves that shouldn't execute when doing other broad specified actions
      if (!player.pulley && !player.minecartLeft && !player.mount.Active && !player.mount.Cart) {
        // Climbing
        // Stomp
        if (
          PlayerInput.Triggers.JustPressed.Down &&
          !isGrounded &&
          !bashActive &&
          !intoStomp &&
          !stomping &&
          !onWall
        ) {
          intoStomp = true;
          intoStompTimer = 24;
          player.velocity.X = 0;
          PlayNewSound("Ori/Stomp/seinStompStart" + RandomChar(3), 1f, 0.2f);
          preHeight = player.position.Y;
        }
        // Crouch
        if (
          PlayerInput.Triggers.JustPressed.Down &&
          isGrounded &&
          !intoCrouch &&
          !PlayerInput.Triggers.Current.Up
        ) {
          intoCrouch = true;
          intoCrouchTimer = 5;
        }
        // Looking Up
        if (
          PlayerInput.Triggers.Current.Up &&
          isGrounded &&
          !intoLookUp &&
          !PlayerInput.Triggers.Current.Down &&
          !PlayerInput.Triggers.Current.Left &&
          !PlayerInput.Triggers.Current.Right
        ) {
          intoLookUp = true;
          intoLookUpTimer = 5;
        }
        // some misc stuff i dont care about
        if (intoLookUp) {
          intoLookUpTimer--;
          player.velocity.X = 0;
          if (
            PlayerInput.Triggers.JustPressed.Left ||
            PlayerInput.Triggers.JustPressed.Right ||
            PlayerInput.Triggers.JustPressed.Jump ||
            !PlayerInput.Triggers.Current.Up ||
            !isGrounded ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            PlayerInput.Triggers.Current.Down ||
            Movement.IsInUse("Dash")
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
            !isGrounded ||
            Movement.IsInUse("Dash")
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
            !isGrounded ||
            Movement.IsInUse("Dash") ||
            outLookUpTimer == 0
          ) {
            outLookUp = false;
            outLookUpTimer = 0;
          }
        }
        if (intoCrouch) {
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
            !isGrounded
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
            if (!TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y].type] && !TileID.Sets.Platforms[Main.tile[(int)pos.X, (int)pos.Y + 1].type]) {
              backflipping = true;
            }*/
            crouching = false;
          }
          else if (
            !PlayerInput.Triggers.Current.Down ||
            PlayerInput.Triggers.Current.Up ||
            OriMod.BashKey.JustPressed ||
            OriMod.DashKey.JustPressed ||
            !isGrounded
          ) {
            if (
              !PlayerInput.Triggers.Current.Left &&
              !PlayerInput.Triggers.Current.Right &&
              !OriMod.BashKey.Current &&
              isGrounded
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
            !isGrounded
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
          if (outOfStompTimer == 0 || Movement.IsInUse("Dash") || bashActive || Movement.IsInUse("AirJump")) {
            outOfStomp = false;
            outOfStompTimer = 0;
          }
        }
        if (stomping) {
          player.controlUp = false;
          player.controlDown = false;
          player.controlLeft = false;
          player.controlRight = false;
          tempInvincibility = true;
          immuneTimer = 15;
          stompHitboxTimer = 3;
          if (PlayerInput.Triggers.JustPressed.Jump) {
            stomping = false;
            outOfStomp = true;
            outOfStompTimer = 10;
          }
          if (isGrounded) {
            stomping = false;
            PlayNewSound("Ori/Stomp/seinStompImpact" + RandomChar(3));
            Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
            for (int i = 0; i < 25; i++) { // does particles
              Dust dust = Main.dust[Terraria.Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
              dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
              dust.velocity *= new Vector2(2, 0.5f);
              if (dust.velocity.Y > 0) {
                dust.velocity.Y = -dust.velocity.Y;
              }
            }
          }
          if (stompingTimer == 0) {
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
          // if (PlayerInput.Triggers.JustPressed.Jump && jumpsAvailable > 0) { TODO: Stomp affected by DJump
          //   intoStompTimer = 0;
          //   intoStomp = false;
          //   player.position.Y = preHeight;
          // }
        }
      }
      // Bashing
      if (
        OriMod.BashKey.JustPressed &&
        bashActivate == 0 &&
        !Movement.IsInUse("Dash") && // Bash should be available during Dash
        !intoStomp &&  // Bash should be available during Stomp
        abilityBash
      ) {
        bashActivate = 3;
        Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("BashHitbox"), 1, 0f, player.whoAmI, 0, 1);
      }
      // Jump Effects
      if (player.justJumped) {
        PlayNewSound("Ori/Jump/seinJumpsGrass" + RandomChar(5), 0.75f);
      }
      // Curently Bashing
      if (bashActive) {
        player.velocity.X = 0;
        player.velocity.Y = 0 - player.gravity;
        // Allow only quick heal and quick mana
        player.controlJump = false;
        player.controlUp = false;
        player.controlDown = false;
        player.controlLeft = false;
        player.controlRight = false;
        player.controlHook = false;
        player.controlInv = false;
        player.controlMount = false;
        player.controlSmart = false;
        player.controlThrow = false;
        player.controlTorch = false;
        player.controlUseItem = false;
        player.controlUseTile = false;
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
        tempInvincibility = true;
        immuneTimer = 15;
        // Bash Sound
        if (bashActiveTimer == 75) {
          PlayNewSound("Ori/Bash/seinBashLoopA", /*0.7f*/ Main.soundVolume);
        }
        if (Main.npc[bashNPC].active) {
          Main.npc[bashNPC].velocity = Vector2.Zero;
        }
      }
      // Freezing Bash
      if (bashActive && bashActiveTimer < 97) {
        player.Center = bashPosition;
      }
      // Releasing Bash
      if (
        (
          !OriMod.BashKey.Current ||
          OriMod.BashKey.JustReleased ||
          bashActiveTimer == 5 ||
          !Main.npc[bashNPC].active
        ) &&
        bashActive
      ) {
        bashActive = false;
        bashActiveTimer = 4;
        bashAngle = player.AngleFrom(Main.MouseWorld);
        // Main.NewText(bashAngle);
        PlayNewSound("Ori/Bash/seinBashEnd" + RandomChar(3), /*0.7f*/ Main.soundVolume);
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
      // Wall Jump
      
      // tempinvincibility
      if (tempInvincibility && immuneTimer > 0) {
        player.immune = true;
      }
      else {
        tempInvincibility = false;
        immuneTimer = 0;
      }
      // Charging
      if (
        ( // Ground CJump
          PlayerInput.Triggers.Current.Up &&
          !upRefresh &&
          !(onWall && OriMod.ClimbKey.Current)
        ) || ( // Wall CJump
          onWall &&
          !isGrounded &&
          OriMod.ClimbKey.Current &&
          (
            (player.direction == 1 && PlayerInput.Triggers.Current.Left) ||
            (player.direction == -1 && PlayerInput.Triggers.Current.Right)
          )
        )
      ) {
        if (chargeTimer == 1) {
          PlayNewSound("Ori/ChargeJump/seinChargeJumpChargeB", 1f, .2f);
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
        if (charged && PlayerInput.Triggers.JustPressed.Jump && isGrounded) {
          chargeTimer = 0;
          charged = false;
          PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + RandomChar(3));
          chargeJumpAnimTimer = 20;
          upRefresh = true;
          stompHitboxTimer = 3;
          Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1);
        }
        if (charged && OriMod.DashKey.Current) {
          // TODO: Implement the below Projectile into Movement.ChargeDash()
          Projectile.NewProjectile(player.Center, new Vector2(0, 0), mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1);
        }
      }
      else {
        chargeTimer = 0;
        if (charged) {
          PlayNewSound("Ori/ChargeDash/seinChargeDashUncharge", 1f, .3f);
          charged = false;
        }
      }
      if ((!PlayerInput.Triggers.Current.Up) && chargeJumpAnimTimer <= 0) {
        upRefresh = false;
      }

      // wall detection
      onWall = false;
      // code modified from source code
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
        onWall = true;
      }
    }
    public override void PostUpdateRunSpeeds() {
      if (OriSet) {
        if (player.whoAmI == Main.myPlayer) {
          Movement.Tick();
        }
        else {
          Movement.TickOtherClient();
        }

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
          if (PlayerInput.Triggers.Current.Left || PlayerInput.Triggers.Current.Right || isGrounded) {
            unrestrictedMovement = false;
          }
        }
        else if (unrestrictedMovement) {
          player.runSlowdown = 0;
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
        Main.SetCameraLerp(0.05f, 1);
        if (OriMod.ClimbKey.Current && onWall) {
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
        else if (onWall && (isGrounded || player.velocity.Y < 0)) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
          player.jumpSpeedBoost -= 6f;
        }
        else if (onWall && player.velocity.Y > 0 && !intoStomp && !stomping && !outOfStomp && !isGrounded) {
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
        if (charged) {
          player.jumpSpeedBoost += 20f;
          if (Main.rand.NextFloat() < 0.7f) {
            Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 172, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          }
          Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0f, 0.6f, 0.9f);
        }
        else {
          player.jumpSpeedBoost += 2f;
        }
        if (OriMod.FeatherKey.JustPressed || OriMod.FeatherKey.Current || OriMod.FeatherKey.JustReleased) {
          Movement.Glide();
        }
        if (PlayerInput.Triggers.JustPressed.Jump) {
          Movement.AirJump();
        }
        if (OriMod.DashKey.JustPressed || Movement.IsInUse("Dash") || Movement.IsInUse("ChargeDash")) {
          if (OriMod.ChargeKey.Current || Movement.IsInUse("ChargeDash")) {
            Movement.ChargeDash();
          }
          else {
            Movement.Dash();
          }
        }
        if ((PlayerInput.Triggers.JustPressed.Jump && onWall && !isGrounded) || Movement.IsInUse("WallJump")) {
          Movement.WallJump();
        }
        if (OriMod.ClimbKey.Current && onWall) {
          Movement.Climb();
        }
      }
      else if (transforming) {
        if (transformTimer > 235) {
          if (transformTimer < 240) {
            player.gravity = 9f;
          }
          else {
            player.velocity = new Vector2(0, -0.00055f * transformTimer);
            player.gravity = 0;
            if (featherTrailTimer == 0) {
              Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
              dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
              dust.noGravity = false;
              dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
              featherTrailTimer = Main.rand.Next(3, 8);
            }
          }
        }
        player.direction = transformDirection;
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.immune = true;
      }
    }
    public override void FrameEffects() {
      if (!OriSet) { return; }

      if (player.velocity.Y != 0 || player.velocity.X != 0) {
        if (featherTrailTimer == 0) {
          Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
          dust.noGravity = false;
          featherTrailTimer = Movement.IsInUse("Dash") ? Main.rand.Next(2, 4) : Main.rand.Next(10, 15);
        }
      }
      else if (Movement.IsInUse("Dash") && featherTrailTimer > 4) {
        featherTrailTimer = Main.rand.Next(2, 4);
      }
      flashing = flashPattern.Contains(flashTimer);
    }
    public void BashEffects(NPC target) {
      bashActiveTimer = 100;
      bashActivate = 0;
      bashActive = true;
      PlayNewSound("Ori/Bash/seinBashStartA", /*0.7f*/ Main.soundVolume);
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
    public override void OnHitByNPC(NPC npc, int damage, bool crit) {
      if (OriSet) {
        if (stomping || chargeJumpAnimTimer > 0) {
          damage = 0;
        }
        if (bashActivate > 0 && bashActiveTimer == 0 && !bashActive && !countering) {
          if ((CannotBash.Contains(npc.type) || npc.boss == true || npc.immortal) && !countering) {
            DoCounter();
            damage = 0;
          }
          else {
            BashEffects(npc);
            damage = 0;
          }
        }
      }
    }
    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { // effects when character is hurt
      if (OriSet && playSound) {
        playSound = false; // stops regular hurt sound from playing
        genGore = false; // stops regular gore from appearing
        if (bashActiveTimer > 0 || bashActive || stomping || intoStomp || chargeJumpAnimTimer > 0) {
          damage = 0;
        }
        else {
          flashTimer = 53;
          PlayNewSound("Ori/Hurt/seinHurtRegular" + RandomChar(5), /*0.6f*/ Main.soundVolume);
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
        {"OriSetPrevious", OriSet},
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
      OriSet = tag.GetBool("OriSetPrevious");
      abilityBash = tag.GetBool("bash");
      oriAirJumps = tag.GetInt("oriAirJumps");
      hasFeather = tag.GetBool("feather");
      waterBreath = tag.GetBool("water");
      canClimb = tag.GetBool("climb");
      abilityDash = tag.GetBool("dash");
      abilityChargeJump = tag.GetBool("chargejump");
      abilityWallJump = tag.GetBool("walljump");
    }
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { // similar to prehurt, but for death
      if (OriSet) {
        genGore = false;
        playSound = false;
        switch (damageSource.SourceOtherIndex) {
          case 1:
            PlayNewSound("Ori/Death/seinSwimmingDrowningDeath" + RandomChar(3), 3f);
            break;
          case 2:
            PlayNewSound("Ori/Death/seinDeathLava" + RandomChar(5));
            break;
          default:
            PlayNewSound("Ori/Death/seinDeathRegular" + RandomChar(5));
            break;
        }
        if (doOriDeathParticles) {
          for (int i = 0; i < 15; i++) {
            Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          }
        }
      }
      return true;
    }
    public override void ModifyDrawLayers(List<PlayerLayer> layers) {
      if (bashActive) {
        layers.Insert(0, oriBashArrow);
        oriBashArrow.visible = true;
      }
      layers.Insert(9, oriPlayerSprite);
      layers.Insert(0, OriTrail);
      layers.Insert(0, oriTransformSprite);
      oriTransformSprite.visible = (transforming && transformTimer > 235);
      if (OriSet) {
        player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
      }
      if (OriSet || transforming) {
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

        if (OriSet || transformTimer < 236) {
          oriPlayerSprite.visible = (!player.dead && !player.invis);
          OriTrail.visible = (!player.dead && !player.invis && !player.mount.Cart);
        }
      }
      else {
        OriTrail.visible = false;
        oriPlayerSprite.visible = false;
      }
    }

    private Vector2 Offset(OriPlayer oPlayer, int x=0, int y=0) {
      Vector2 offset = new Vector2();
      if (x != 0 || y != 0) {
        offset.X = x;
        offset.Y = y;
        return offset;
      }
      switch (oPlayer.AnimName) {
        case "AirJump": offset.Y = 8;
        break;
        // case "ClimbIdle": offset.Y = 24;
        // break;
      }
      return offset;
    }
    public readonly PlayerLayer OriTrail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      if (oPlayer.trailPos == null) {
        oPlayer.trailPos = new List<Vector2>();
        oPlayer.trailFrame = new List<Vector2>();
        oPlayer.trailAlpha = new List<float>();
        oPlayer.trailDirection = new List<int>();
        oPlayer.trailRotation = new List<float>();

        for (int i = 0; i < 27; i++) {
          oPlayer.trailPos.Add(Vector2.Zero);
          oPlayer.trailFrame.Add(Vector2.Zero);
          oPlayer.trailRotation.Add(0);
          oPlayer.trailDirection.Add(1);
          oPlayer.trailAlpha.Add((i + 1) * 4);
        }
      }

      Vector2 position = drawPlayer.position;

      // modPlayer.UpdateTrail(drawPlayer);
      for (int i = 0; i <= 25; i++) {
        oPlayer.trailAlpha[i] -= 4;
        if (oPlayer.trailAlpha[i] < 0) {
          oPlayer.trailAlpha[i] = 0;
        }
      }
      if (!drawPlayer.dead && !drawPlayer.invis) {
        oPlayer.trailUpdate++;
        if (oPlayer.trailUpdate > 25) {
          oPlayer.trailUpdate = 0;
        }
        oPlayer.trailPos[oPlayer.trailUpdate] = drawPlayer.position;
        oPlayer.trailFrame[oPlayer.trailUpdate] = oPlayer.AnimFrame;
        oPlayer.trailDirection[oPlayer.trailUpdate] = drawPlayer.direction;
        oPlayer.trailAlpha[oPlayer.trailUpdate] = 2 * (float)(Math.Sqrt(Math.Pow(Math.Abs(drawPlayer.velocity.X), 2) + Math.Pow(Math.Abs(drawPlayer.velocity.Y), 2))) + 44;
        oPlayer.trailRotation[oPlayer.trailUpdate] = oPlayer.AnimRads;
        if (oPlayer.trailAlpha[oPlayer.trailUpdate] > 104) {
          oPlayer.trailAlpha[oPlayer.trailUpdate] = 104;
        }
      }
      for (int i = 0; i <= 25; i++) {
        SpriteEffects effect = SpriteEffects.None;

        if (oPlayer.trailDirection[i] == -1) {
          effect = SpriteEffects.FlipHorizontally;
        }

        DrawData data = new DrawData(
          mod.GetTexture("PlayerEffects/OriGlowRight"),
          new Vector2(
            (oPlayer.trailPos[i].X - Main.screenPosition.X) + 10,
            (oPlayer.trailPos[i].Y - Main.screenPosition.Y) + 8
          ),
          new Rectangle(
            (int)(oPlayer.trailFrame[i].X),
            (int)(oPlayer.trailFrame[i].Y), 104, 76),
          Color.White * ((oPlayer.trailAlpha[i] / 2) / 255),
          oPlayer.trailRotation[i],
          new Vector2(52, 38), 1, effect, 0
        );
        data.position += oPlayer.Offset(oPlayer);
        Main.playerDrawData.Add(data);
      }
      // public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    public static PlayerDrawInfo dInfo;
    public static readonly PlayerLayer oriPlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      Player drawPlayer = drawInfo.drawPlayer;
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      oPlayer.UpdateFrame(drawPlayer);
      Vector2 position = drawPlayer.position;
      dInfo = drawInfo;
      Texture2D texture = mod.GetTexture("PlayerEffects/OriPlayer");
      SpriteEffects effect = SpriteEffects.None;

      if (oPlayer.transforming) {
        oPlayer.Increment(oPlayer.transformTimer > 0 ? "TransformEnd" : "Idle");
      }
      if (drawPlayer.direction == -1) {
        effect = SpriteEffects.FlipHorizontally;
      }

      DrawData data = new DrawData(
        texture,
        new Vector2(
          (drawPlayer.position.X - Main.screenPosition.X) + 10,
          (drawPlayer.position.Y - Main.screenPosition.Y) + 8
        ),
        new Rectangle(
          (int)(oPlayer.AnimFrame.X),
          (int)(oPlayer.AnimFrame.Y), 104, 76),
        Color.White,
        drawPlayer.direction * oPlayer.AnimRads,
        new Vector2(52, 38), 1, effect, 0
      );
      data.position += oPlayer.Offset(oPlayer);
      Main.playerDrawData.Add(data);
      // public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    public static readonly PlayerLayer oriTransformSprite = new PlayerLayer("OriMod", "OriTransform", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/transform");
      SpriteEffects effect = SpriteEffects.None;

      if (drawPlayer.direction == -1) {
        effect = SpriteEffects.FlipHorizontally;
      }
      int y = 0;
      if (oPlayer.transformTimer > 236) { // Transform Start
        float t = oPlayer.transformTimer - 235;
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
          else if (t >= 111) {
            y = 4;
          }
          else if (t >= 71) {
            y = 5;
          }
          else if (t >= 31) {
            y = 6;
          }
          else if (t >= 1) {
            y = 7;
          }
        }
      }
      DrawData data = new DrawData(
        texture,
        new Vector2(
          (drawPlayer.position.X - Main.screenPosition.X) + 10,
          (drawPlayer.position.Y - Main.screenPosition.Y) + 8
        ),
        new Rectangle(0, y * 76, 104, 76),
        Color.White, drawPlayer.direction * oPlayer.AnimRads,
        new Vector2(52, 38), 1, effect, 0);
        data.position += oPlayer.Offset(oPlayer);
      Main.playerDrawData.Add(data);
      // public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    public static readonly PlayerLayer oriBashArrow = new PlayerLayer("OriMod", "bashArrow", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/bashArrow");
      SpriteEffects effect = SpriteEffects.None;

      int frameY = 0;

      if (oPlayer.bashActiveTimer < 55 && oPlayer.bashActiveTimer > 45) {
        frameY = 1;
      }
      else if (oPlayer.bashActiveTimer < 46) {
        frameY = 2;
      }
      DrawData data = new DrawData(texture,
        new Vector2(
          (Main.npc[oPlayer.bashNPC].Center.X - Main.screenPosition.X),
          (Main.npc[oPlayer.bashNPC].Center.Y - Main.screenPosition.Y)
        ),
        new Rectangle(0, frameY * 20, 152, 20),
        Color.White, Main.npc[oPlayer.bashNPC].AngleTo(Main.MouseWorld),
        new Vector2(76, 10), 1, effect, 0);
      Main.playerDrawData.Add(data);
      // public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });

    /*public override void clientClone(ModPlayer clientClone) {
      OriPlayer clone = clientClone as OriPlayer;
      clone.OriSet = OriSet;
      clone.OriSetPrevious = OriSetPrevious;
    }
    */
    public override void ResetEffects() {
      Increment(this.AnimName);
      if (transformTimer > 0) {
        transformTimer -= hasTransformedOnce ? repeatedTransformRate : 1;
        if (transformTimer <= 0 || (transformTimer < 236 - 62 && hasTransformedOnce)) {
          transformTimer = 0;
          transforming = false;
          OriSet = true;
        }
      }
      
      if (OriSet) {
        if (bashActivate > 0) { bashActivate--; }
        if (bashActiveTimer > 0) { bashActiveTimer--; }
        if (bashActiveTimer < 0) { bashActiveTimer = 0; }
        if (flashTimer > 0) { flashTimer--; }
        if (featherTrailTimer > 0) { featherTrailTimer--; }
        if (intoStompTimer > 0) { intoStompTimer--; }
        if (outOfStompTimer > 0) { outOfStompTimer--; }
        if (stompHitboxTimer > 0) { stompHitboxTimer--; }
        if (stompingTimer > 0) { stompingTimer--; }
        if (!lookUp) { lookUpTimer = 1; }

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
        ModNetHandler.oriPlayerHandler.SendOriState(255, player.whoAmI);
      }
    }
    public override void Initialize() {
      Movement = new MovementHandler(this);
      InitTestMaterial();
    }
  }
}