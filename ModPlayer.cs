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
using OriMod.Abilities;

namespace OriMod
{
  public sealed class OriPlayer : ModPlayer {
    internal static TriggersSet JustPressed => PlayerInput.Triggers.JustPressed;
    internal static TriggersSet JustReleased => PlayerInput.Triggers.JustReleased;
    internal static TriggersSet Current => PlayerInput.Triggers.Current;

    internal bool Input(bool TriggerKey) {
      return player.whoAmI == Main.myPlayer && TriggerKey;
    }
    #region Variables
    /// <summary>
    /// Class that contains all of OriPlayer's abilities
    /// </summary>
    /// <value></value>
    internal OriAbilities Abilities { get; set; } // Class used for all of Ori's movements
    
    public WallJump wJump => Abilities.wJump;
    public AirJump airJump => Abilities.airJump;
    public Bash bash => Abilities.bash;
    public Stomp stomp => Abilities.stomp;
    public Glide glide => Abilities.glide;
    public Climb climb => Abilities.climb;
    public Dash dash => Abilities.dash;
    public ChargeDash cDash => Abilities.cDash;
    public Crouch crouch => Abilities.crouch;
    public LookUp lookUp => Abilities.lookUp;

    internal bool doNetUpdate = false;

    /// <summary>
    /// When set to true, uses custom movement and player sprites.
    /// 
    /// External mods that attempt to be compatible with this one will need to use this property.
    /// </summary>
    public bool OriSet {
      get {
        return _oriSet;
      }
      set {
        if (value != _oriSet) {
          doNetUpdate = true;
          _oriSet = value;
        }
      }
    }
    private bool _oriSet;

    // Transform variables used to hasten additional transforms
    private bool HasTransformedOnce = false;
    private const float RepeatedTransformRate = 2.5f;

    // Variables relating to fixing movement when Ori is active, such that you aren't slowed down mid-air after bashing.
    public bool IsGrounded { get; private set; }
    /// <summary>
    /// When true, sets player.runSlowDown to 0 every frame until set to false
    /// </summary>
    
    public bool UnrestrictedMovement {
      get {
        return _unrestrictedMovement;
      }
      set {
        if (value != _unrestrictedMovement) {
          doNetUpdate = true;
          _unrestrictedMovement = value;
        }
      }
    }
    private bool _unrestrictedMovement = false;
    public bool OnWall { get; private set; }

    // Variables relating to Charge Jumping
    public bool charged = false;
    public int chargeTimer = 0;
    public int chargeUpTimer = 40;
    public int chargeJumpAnimTimer = 0;
    public bool upRefresh = false;

    // Variables relating to visual or audible effects
    public bool doOriDeathParticles = true;
    /// <summary>
    /// Name of the current floor material that Ori is standing on.
    /// 
    /// Used exclusively for footstep noises.
    /// </summary>
    /// <value></value>
    public string FloorMaterial { get; private set; }
    /// <summary>
    /// Name of the current wall material that Ori is standing on.
    /// 
    /// This property is currently unused.
    /// </summary>
    /// <value></value>
    /// <seealso cref="FloorMaterial" />
    internal string WallMaterial { get; private set; }
    

    // Variables relating to Sein
    /// <summary>
    /// Info about if this player has an OriMod Sein minion summoned.
    /// Used to prevent having more than one Sein summoned per player.
    /// </summary>
    /// <value></value>
    /// <seealso cref="SeinMinionUpgrade" />
    public bool SeinMinionActive {
      get {
        return _seinMinionActive;
      }
      internal set {
        if (value != _seinMinionActive) {
          doNetUpdate = true;
        }
        _seinMinionActive = value;
      }
    }
    private bool _seinMinionActive = false;
    /// <summary>
    /// The current version of Sein that is summoned
    /// 
    /// Used to prevent re-summons of the same tier of Sein.
    /// </summary>
    /// <value></value>
    /// <seealso cref="SeinMinionActive" />
    public int SeinMinionUpgrade {
      get {
        return _seinMinionUpgrade;
      }
      internal set {
        if (value != _seinMinionUpgrade) {
          doNetUpdate = true;
        }
        _seinMinionUpgrade = value;
      }
    }
    private int _seinMinionUpgrade = 0;

    // Variables relating to Transforming
    internal float TransformTimer = 0;
    /// <summary>
    /// Bool that represents if the player is currently transforming into Ori.
    /// 
    /// While transforming, all player input is disabled.
    /// </summary>
    /// <value></value>
    public bool Transforming {
      get {
        return _transforming;
      }
      internal set {
        if (value != _transforming) {
          doNetUpdate = true;
          _transforming = value;
        }
      }
    }
    private bool _transforming = false;
    /// <summary>
    /// Location of the Spirit Sapling that transformed Ori.
    /// 
    /// Used to create Dust effects.
    /// </summary>
    /// <value></value>
    internal Vector2 TransformBlockLocation { get; set; }
    private int TransformDirection = 0;

    // Footstep materials
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on grass
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> GrassFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on crystal or glass
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> LightDarkFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on mushroom blocks
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> MushroomFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on stone
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> RockFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on sand
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> SandFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on snow
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> SnowFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on rock, with an additional, echo-like effect
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> SpiritTreeRockFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on wood, with an additional, echo-like effect
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> SpiritTreeWoodFloorMaterials;
    /// <summary>
    /// TileIDs used by <c>floorMaterial</c> to create footstep noises that sound like walking on wood
    /// 
    /// <seealso cref="FloorMaterial" />
    /// </summary>
    public List<int> WoodFloorMaterials;
    internal List<int> UnassignedTiles;

    // Wall materials
    public List<int> GrassWallMaterials;
    public List<int> LightDarkWallMaterials;
    public List<int> MushroomWallMaterials;
    public List<int> RockWallMaterials;
    public List<int> WoodWallMaterials;

    // Trail variables, for the trails Ori creates
    private List<Vector2> TrailPos;
    private List<Vector2> TrailFrame;
    private List<float> TrailAlpha;
    private List<float> TrailRotation;
    private List<int> TrailDirection;
    private int TrailUpdate = 0;

    private int TeatherTrailTimer = 0;

    // Animation Variables
    internal const int SpriteWidth = 104;
    internal const int SpriteHeight = 76;
    private Vector2 _animFrame;
    private Vector2 AnimFrame {
      get {
        return _animFrame;
      }
      set {
        if (value != _animFrame) {
          doNetUpdate = true;
          _animFrame = value;
        }
      }
    }
    /// <summary>
    /// The current sprite tile of the player in Ori state
    /// 
    /// X and Y values are based on the sprite tile coordinates, not pixel coordinates 
    /// </summary>
    /// <value></value>
    public Vector2 AnimTile {
      get { return PixelToTile(AnimFrame); }
      internal set { AnimFrame = TileToPixel(value); }
    }
    /// <summary>
    /// The name of the animation track currently playing
    /// </summary>
    /// <value></value>
    public string AnimName { get; private set; }
    internal int AnimIndex { get; private set; }
    internal float AnimTime { get; private set; } // Intentionally a float
    internal float AnimRads { get; private set; }
    internal bool AnimReversed = false;
    internal bool Flashing = false;
    internal int FlashTimer = 0;
    private static readonly int[] flashPattern = new int[] {
      53,52,51,50,45,
      44,43,38,37,36,
      31,30,29,24,23,
      22,17,16,15,10,
       9, 8, 3, 2, 1
    };
    private int FootstepRand = 0;
    private int JumpSoundRand = 0;
    private Vector2 PixelToTile(Vector2 pixel) {
      pixel.X = (int)(pixel.X / SpriteWidth);
      pixel.Y = (int)(pixel.Y / SpriteHeight);
      return pixel;
    }
    private static Vector2 TileToPixel(Vector2 tile) {
      tile.X *= SpriteWidth;
      tile.Y *= SpriteHeight;
      return tile;
    }

    private Color _spriteColor = Color.LightCyan;
    internal Color SpriteColor {
      get {
        return _spriteColor;
      }
      set {
        if (value != _spriteColor) {
          doNetUpdate = true;
        }
        _spriteColor = value;
      }
    }
    internal bool debugMode = false;
    private bool _mpcPlayerLight = false;
    internal bool MpcPlayerLight {
      get {
        return _mpcPlayerLight;
      }
      set {
        if (value != _mpcPlayerLight) {
          doNetUpdate = true;
        }
        _mpcPlayerLight = value;
      }
    }
    internal bool DoPlayerLight => Config.GlobalPlayerLight ? Config.DoPlayerLight : MpcPlayerLight;
    internal void Debug(string msg) {
      Debug(msg, this);
    }
    internal static void Debug(string msg, OriPlayer oPlayer) {
      if (oPlayer.debugMode && oPlayer.player.whoAmI == Main.myPlayer) {
        Main.NewText(msg);
      }
    }
    public Color LightColor = new Color(0.2f, 0.4f, 0.4f);
    #endregion
    
    // basic sound playing method, with paths starting after NewSFX in the file structure
    internal SoundEffectInstance PlayNewSound(string Path) => PlayNewSound(Path, 1, 0);
    internal SoundEffectInstance PlayNewSound(string Path, float Volume) => PlayNewSound(Path, Volume, 0);
    internal SoundEffectInstance PlayNewSound(string Path, float Volume, float Pitch) =>
      Main.PlaySound((int)SoundType.Custom, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/" + Path), Volume, Pitch);
    internal SoundEffectInstance PlayFootstep(string Material, int rand, float Volume) {
      char randChar = RandomChar(rand, ref FootstepRand);
      return PlayNewSound("Ori/Footsteps/" + Material + "/" + Material + randChar, Volume, 0.1f);
    }
    /// <summary>
    /// Retrieves a random character of an alphabet between indices 0 and <c>length</c>
    /// </summary>
    /// <param name="length">Max letter indice to use</param>
    /// <returns>char between A and <c>alphabet[length]</c></returns>

    public static char RandomChar(int length) { // Returns random letter based on length. Primarily used for sound effects
      char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
      return alphabet[Main.rand.Next(length)];
    }
    /// <summary>
    /// Retrieves a random, non-repeating character of an alphabet between indices 0 and <c>length</c>
    /// </summary>
    /// <param name="length">Max letter indice to use</param>
    /// <param name="exclude">Letter indice to exclude from result. Must be non-negative and less than length</param>
    /// <returns>char between A and <c>alphabet[length]</c>, except <c>alphabet[exclude]</c></returns>
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
    internal void Increment(string anim="Default", int overrideFrame=0, float overrideTime=0, int overrideDur=0, Vector3 overrideMeta=new Vector3(), Vector2 drawOffset=new Vector2(), float rotDegrees=0) {
      // Main.NewText("Frame called: " + AnimName + ", Time: " + AnimTime + ", AnimIndex: " + AnimIndex); // Debug
      AnimationHandler.IncrementFrame(this, anim, overrideFrame, overrideTime, overrideDur, overrideMeta, drawOffset, rotDegrees);
    }
    
    internal void SetFrame(string name, int frameIndex, float time, Vector3 frame, float animRads) =>
      SetFrame(name, frameIndex, time, new Vector2(frame.X, frame.Y), animRads);
    internal void SetFrame(string name, int frameIndex, float time, Vector2 frame, float animRads) {
      AnimName = name;
      AnimIndex = frameIndex;
      AnimTime = time;
      AnimFrame = TileToPixel(frame);
      AnimRads = animRads;
    }

    private void UpdateFrame(Player drawPlayer) {
      if (!OriSet) return;
      if (!doUpdateFrame) return;
      doUpdateFrame = false;
      AnimTime++;
      
      if (!Transforming && !HasTransformedOnce) {
        HasTransformedOnce = true;
      }

      if (Transforming) {
        Increment("TransformEnd");
        return;
      }
      if (drawPlayer.mount.Active) {
        Increment("Idle");
        // TODO: Minecart animation?
        return;
      }
      if (wJump.InUse) {
        Increment("WallJump");
        return;
      }
      if (airJump.InUse && !(dash.InUse || cDash.InUse)) {
        Increment("AirJump");
        AnimRads = AnimTime * 0.8f;
        return;
      }
      if (bash.InUse) {
        Increment("Bash");
        return;
      }
      if (stomp.InUse) {
        switch (stomp.State) {
          case Ability.States.Starting:
            Increment("AirJump");
            AnimRads = AnimTime;
            return;
          case Ability.States.Active:
            Increment("ChargeJump", rotDegrees:180f, overrideDur:2, overrideMeta:new Vector3(0,2,0));
            return;
        }
      }
      if (glide.InUse) {
        switch (glide.State) {
          case Ability.States.Starting:
            Increment("GlideStart");
            return;
          case Ability.States.Active:
            Increment("Glide");
            return;
          case Ability.States.Ending:
            Increment("GlideStart", overrideMeta:new Vector3(0, 0, 3));
            return;
        }
      }
      if (climb.InUse) {
        if (Math.Abs(player.velocity.Y) < 0.1f) {
          Increment("ClimbIdle");
        }
        else {
          Increment(player.velocity.Y < 0 ? "Climb" : "WallSlide", overrideTime:AnimTime+Math.Abs(drawPlayer.velocity.Y)*0.1f);
        }
        return;
      }
      if (OnWall && !IsGrounded) {
        Increment("WallSlide");
        return;
      }
      if (dash.InUse || cDash.InUse) {
        if (Math.Abs(player.velocity.X) > 18f) {
          Increment("Dash");
        }
        else {
          Increment("Dash", overrideFrame:2);
        }
        return;
      }
      if (lookUp.InUse) {
        switch (lookUp.State) {
          case Ability.States.Starting:
            Increment("LookUpStart");
            return;
          case Ability.States.Active:
            Increment("LookUp");
            return;
          case Ability.States.Ending:
            Increment("LookUpStart", overrideMeta:new Vector3(0, 2, 0));
            return;
        }
      }
      if (crouch.InUse) {
        switch (crouch.State) {
          case Ability.States.Starting:
            Increment("CrouchStart");
            return;
          case Ability.States.Active:
            Increment("Crouch");
            return;
          case Ability.States.Ending:
            Increment("CrouchStart", overrideMeta:new Vector3(0, 2, 0));
            return;
        }
      }
          
      if (chargeJumpAnimTimer > 0) {
        Increment("ChargeJump");
        if (chargeJumpAnimTimer > 17) {
          drawPlayer.controlJump = false;
        }
        return;
      }
      if (!IsGrounded) {
        // XOR so opposite signs means jumping
        Increment(((int)drawPlayer.velocity.Y ^ (int)drawPlayer.gravity) <= 0 ? "Jump" : "Falling");
        return;
      }
      if (Math.Abs(drawPlayer.velocity.X) < 0.2f) {
        Increment(OnWall ? "IdleAgainst" : "Idle");
        return;
      }
      Increment("Running", overrideTime:AnimTime+(int)Math.Abs(player.velocity.X) / 3);
      if (AnimIndex == 4 || AnimIndex == 9) {
        TestStepMaterial(drawPlayer);
        switch (FloorMaterial) {
          case "Grass":
          case "Mushroom":
            PlayFootstep(FloorMaterial, 5, 0.15f);
            break;
          case "Water":
            PlayFootstep(FloorMaterial, 4, 1f);
            break;
          case "SpiritTreeRock":
          case "SpiritTreeWood":
          case "Rock":
            PlayFootstep(FloorMaterial, 5, 1f);
            break;
          case "Snow":
          case "LightDark":
            PlayFootstep(FloorMaterial, 10, 0.85f);
            break;
          case "Wood":
            PlayFootstep(FloorMaterial, 5, 0.85f);
            break;
          case "Sand":
            PlayFootstep(FloorMaterial, 8, 0.85f);
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
    internal int ImmuneTimer = 0;
    internal void DoTransformation() {
      OriSet = true;
      return; // Temporarily disable animations
      Transforming = true;
      TransformDirection = player.direction;
      TransformTimer = 627;
    }
    private void InitTestMaterial() {
      GrassFloorMaterials = new List<int>();
      LightDarkFloorMaterials = new List<int>();
      MushroomFloorMaterials = new List<int>();
      RockFloorMaterials = new List<int>();
      SandFloorMaterials = new List<int>();
      SnowFloorMaterials = new List<int>();
      SpiritTreeRockFloorMaterials = new List<int>();
      SpiritTreeWoodFloorMaterials = new List<int>();
      WoodFloorMaterials = new List<int>();
      GrassWallMaterials = new List<int>();
      LightDarkWallMaterials = new List<int>();
      MushroomWallMaterials = new List<int>();
      RockWallMaterials = new List<int>();
      WoodWallMaterials = new List<int>();
      UnassignedTiles = new List<int>();

      GrassFloorMaterials.Clear();
      LightDarkFloorMaterials.Clear();
      MushroomFloorMaterials.Clear();
      RockFloorMaterials.Clear();
      SandFloorMaterials.Clear();
      SnowFloorMaterials.Clear();
      SpiritTreeRockFloorMaterials.Clear();
      SpiritTreeWoodFloorMaterials.Clear();
      WoodFloorMaterials.Clear();

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

      GrassFloorMaterials.AddRange(grassBlocks);
      LightDarkFloorMaterials.AddRange(lightDarkBlocks);
      MushroomFloorMaterials.AddRange(mushroomBlocks);
      RockFloorMaterials.AddRange(rockBlocks);
      SandFloorMaterials.AddRange(sandBlocks);
      SnowFloorMaterials.AddRange(snowBlocks);
      SpiritTreeRockFloorMaterials.AddRange(spiritTreeRockBlocks);
      SpiritTreeWoodFloorMaterials.AddRange(spiritTreeWoodBlocks);
      WoodFloorMaterials.AddRange(woodBlocks);

      GrassWallMaterials.AddRange(grassBlocks);
      GrassWallMaterials.AddRange(sandBlocks);
      GrassWallMaterials.AddRange(snowBlocks);
      LightDarkWallMaterials.AddRange(lightDarkBlocks);
      MushroomWallMaterials.AddRange(mushroomBlocks);
      RockWallMaterials.AddRange(rockBlocks);
      RockWallMaterials.AddRange(spiritTreeRockBlocks);
      WoodWallMaterials.AddRange(woodBlocks);
      WoodWallMaterials.AddRange(spiritTreeWoodBlocks);
    }
    
    // Gets the tile that's a given offset from player.Center.X, player.position.Y + player.height
    private Tile GetTile(float offsetX, float offsetY) {
      Vector2 pos = new Vector2(player.Center.X + offsetX, (player.position.Y + player.height) + offsetY);
      Vector2 tilepos = new Vector2(pos.ToTileCoordinates().X, pos.ToTileCoordinates().Y);
      return Main.tile[(int)tilepos.X, (int)tilepos.Y];
    }
    private void TestStepMaterial(Player player) { // oh yeah good luck understanding what this is
      Tile tile = GetTile(-12f, 4f);
      if (tile.liquid > 0f && tile.liquidType() == 0) {
        FloorMaterial = "Water";
      }
      else {
        tile = GetTile(-12f, -4f);
        if (tile.liquid > 0f && tile.liquidType() == 0) {
          FloorMaterial = "Water";
        }
        else {
          tile = GetTile(-12f, 8);
          if (tile.active()) {
            if (GrassFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "Grass";
            }
            else if (LightDarkFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "LightDark";
            }
            else if (MushroomFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "Mushroom";
            }
            else if (RockFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "Rock";
            }
            else if (SandFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "Sand";
            }
            else if (SnowFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "Snow";
            }
            else if (SpiritTreeRockFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "SpiritTreeRock";
            }
            else if (SpiritTreeWoodFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "SpiritTreeWood";
            }
            else if (WoodFloorMaterials.Contains(tile.type)) {
              FloorMaterial = "Wood";
            }
          }
          else {
            tile = GetTile(-12, 24);
            if (tile.active()) {
              if (GrassFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "Grass";
              }
              else if (LightDarkFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "LightDark";
              }
              else if (MushroomFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "Mushroom";
              }
              else if (RockFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "Rock";
              }
              else if (SandFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "Sand";
              }
              else if (SnowFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "Snow";
              }
              else if (SpiritTreeRockFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "SpiritTreeRock";
              }
              else if (SpiritTreeWoodFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "SpiritTreeWood";
              }
              else if (WoodFloorMaterials.Contains(tile.type)) {
                FloorMaterial = "Wood";
              }
            }
            else {
              FloorMaterial = "Grass";
              if (!UnassignedTiles.Contains(tile.type)) {
              // Main.NewText("Tile ID " + tile.type + " is not assigned to a material"); // Debug
                UnassignedTiles.Add(tile.type);
              }
            }
          }
        }
      }
    }
    private void TestWallMaterial(Player player) { // or this, im too tired to comment them
      Tile tile = GetTile(-2f, 34f);
      if (tile.active()) {
        if (GrassWallMaterials.Contains(tile.type)) {
          WallMaterial = "Grass";
        }
        else if (LightDarkWallMaterials.Contains(tile.type)) {
          WallMaterial = "LightDark";
        }
        else if (MushroomWallMaterials.Contains(tile.type)) {
          WallMaterial = "Mushroom";
        }
        else if (RockWallMaterials.Contains(tile.type)) {
          WallMaterial = "Rock";
        }
        else if (WoodWallMaterials.Contains(tile.type)) {
          WallMaterial = "Wood";
        }
      }
      else {
        WallMaterial = "Grass";
      }
    }
    internal void RemoveSeinBuffs(int exclude=0) {
      for (int u = 1; u <= OriMod.SeinUpgrades.Count; u++) {
        if (u != exclude) {
          player.ClearBuff(mod.GetBuff("SeinBuff" + u).Type);
        }
      }
    }
    internal bool doUpdateFrame = true;
    public override void PostUpdate() {
      doUpdateFrame = true;
      if (SeinMinionActive) {
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
          SeinMinionActive = false;
          SeinMinionUpgrade = 0;
        }
      }
      IsGrounded = false;
      // Check if grounded by means of liquid walking
      if (player.fireWalk || player.waterWalk2 || player.waterWalk) {
        Vector2 checkPos = new Vector2(player.position.X + (player.width / 2), player.position.Y + player.height + 4);
        Vector2 check2 = new Vector2(checkPos.ToTileCoordinates().X, checkPos.ToTileCoordinates().Y);
        bool testblock =
          Main.tile[(int)check2.X, (int)check2.Y].liquid > 0 &&
          Main.tile[(int)check2.X, (int)check2.Y - 1].liquid == 0;
        if (testblock) {
          Tile liquidTile = Main.tile[(int)check2.X, (int)check2.Y];
          IsGrounded = liquidTile.lava() ? player.fireWalk : player.waterWalk;
        }
      }
      if (!IsGrounded) {
        IsGrounded = !Collision.IsClearSpotTest(player.position + new Vector2(0, 8), 16f, player.width, player.height, false, false, (int)player.gravDir, true, true);
      }

      // thanks jopo

      if (DoPlayerLight) Lighting.AddLight(player.Center, LightColor.ToVector3());

      if (Transforming) {
        player.direction = TransformDirection;
      }

      if (!OriSet) { return; }
      // Jump Effects
      if (player.justJumped) {
        PlayNewSound("Ori/Jump/seinJumpsGrass" + RandomChar(5, ref JumpSoundRand), 0.75f);
      }
      // Charging
      if (
        ( // Ground CJump
          Input(OriMod.ChargeKey.Current) &&
          !upRefresh &&
          !(OnWall && Input(OriMod.ClimbKey.Current))
        ) || ( // Wall CJump
          climb.InUse &&
          (
            (player.direction == 1 && Input(Current.Left)) ||
            (player.direction == -1 && Input(Current.Right))
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
        if (charged && Input(JustPressed.Jump) && IsGrounded) {
          chargeTimer = 0;
          charged = false;
          PlayNewSound("Ori/ChargeJump/seinChargeJumpJump" + RandomChar(3));
          chargeJumpAnimTimer = 20;
          upRefresh = true;
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
      if (!Input(OriMod.ChargeKey.Current) && chargeJumpAnimTimer <= 0) {
        upRefresh = false;
      }

      // wall detection
      OnWall = false;
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
        OnWall = true;
      }
    }
    public override void PostUpdateRunSpeeds() {
      if (OriSet) {
        Abilities.Tick();

        player.noFallDmg = true;
        if (Input(Current.Left) || Input(Current.Right) || IsGrounded) {
          UnrestrictedMovement = false;
        }
        player.runSlowdown = UnrestrictedMovement ? 0 : 1;
        if (!crouch.InUse) {
          player.runAcceleration = 0.5f;
          player.maxRunSpeed += 2f;
        }
        if (Config.SmoothCamera) Main.SetCameraLerp(0.05f, 1);
        if (OnWall && (IsGrounded || player.velocity.Y < 0) && !climb.InUse) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
          player.jumpSpeedBoost -= 6f;
        }
        else if (OnWall &&  !IsGrounded && player.velocity.Y > 0 && !stomp.InUse) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
        }
        else if (chargeJumpAnimTimer > 0) {
          player.gravity = 0.1f;
          player.velocity.Y = -3 * chargeJumpAnimTimer;
          if (chargeJumpAnimTimer == 18) {
            player.controlJump = false;
          }
          player.immune = true;
        }
        else {
          player.gravity = 0.35f;
        }
        if (charged) {
          player.jumpSpeedBoost += 20f;
          if (Main.rand.NextFloat() < 0.7f) {
            Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 172, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          }
          if (DoPlayerLight) Lighting.AddLight(player.Center, LightColor.ToVector3());
        }
        else {
          player.jumpSpeedBoost += 2f;
        }
        if (Input(OriMod.FeatherKey.Current || OriMod.FeatherKey.JustReleased)) {
          glide.Update();
        }
        if (Input(JustPressed.Jump) || airJump.InUse) {
          airJump.Update();
        }
        if ((Input(OriMod.DashKey.JustPressed) && !cDash.InUse) || dash.InUse) {
          dash.Update();
        }
        else if ((Input(OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) && cDash.Refreshed) || cDash.InUse) {
          cDash.Update();
        }
         
        if ((Input(JustPressed.Jump) && OnWall && !IsGrounded) || wJump.InUse) {
          wJump.Update();
        }
        if (Input(OriMod.ClimbKey.Current) && OnWall) {
          climb.Update();
        }
        if (Input(JustPressed.Down) || stomp.InUse) {
          stomp.Update();
        }
        if (Input(Current.Up) || lookUp.InUse) {
          lookUp.Update();
        }
        if (Input(Current.Down) || crouch.InUse) {
          crouch.Update();
        }
        if (Input(OriMod.BashKey.Current) || bash.InUse) {
          bash.Update();
        }
      }
      else if (Transforming) {
        if (TransformTimer > 235) {
          if (TransformTimer < 240) {
            player.gravity = 9f;
          }
          else {
            player.velocity = new Vector2(0, -0.00055f * TransformTimer);
            player.gravity = 0;
            if (TeatherTrailTimer == 0) {
              Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
              dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
              dust.noGravity = false;
              dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
              TeatherTrailTimer = Main.rand.Next(3, 8);
            }
          }
        }
        player.direction = TransformDirection;
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.immune = true;
      }
      
      if (ImmuneTimer > 0) {
        ImmuneTimer--;
        player.immune = true;
      }
    }
    public override void FrameEffects() {
      if (!OriSet) { return; }

      if (player.velocity.Y != 0 || player.velocity.X != 0) {
        if (TeatherTrailTimer == 0) {
          Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
          dust.noGravity = false;
          TeatherTrailTimer = dash.InUse ? Main.rand.Next(2, 4) : Main.rand.Next(10, 15);
        }
      }
      else if (dash.InUse && TeatherTrailTimer > 4) {
        TeatherTrailTimer = Main.rand.Next(2, 4);
      }
      Flashing = flashPattern.Contains(FlashTimer);
    }
    public override void OnHitByNPC(NPC npc, int damage, bool crit) {
      if (OriSet) {
        if (stomp.InUse || chargeJumpAnimTimer > 0 || bash.InUse || cDash.InUse) {
          damage = 0;
        }
      }
    }
    private int hurtRand = 0;
    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { // effects when character is hurt
      if (OriSet && playSound) {
        playSound = false; // stops regular hurt sound from playing
        genGore = false; // stops regular gore from appearing
        if (bash.InUse || stomp.InUse || cDash.InUse || chargeJumpAnimTimer > 0) {
          damage = 0;
          return false;
        }
        else {
          FlashTimer = 53;
          PlayNewSound("Ori/Hurt/seinHurtRegular" + RandomChar(4, ref hurtRand));
          UnrestrictedMovement = true;
        }
      }
      return true;
    }
    public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) { }
    public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit) { }
    public override TagCompound Save() {
      return new TagCompound {
        {"OriSet", OriSet},
      };
    }
    public override void Load(TagCompound tag) {
      OriSet = tag.GetBool("OriSet");
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
      if (bash.InUse && bash.Npc != null) {
        layers.Insert(0, oriBashArrow);
        oriBashArrow.visible = true;
      }
      layers.Insert(9, oriPlayerSprite);
      layers.Insert(0, OriTrail);
      layers.Insert(0, oriTransformSprite);
      oriTransformSprite.visible = (Transforming && TransformTimer > 235);
      if (OriSet) {
        player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
      }
      if (OriSet || Transforming) {
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

        if (OriSet || TransformTimer < 236) {
          oriPlayerSprite.visible = (!player.dead && !player.invis);
          OriTrail.visible = (!player.dead && !player.invis && !player.mount.Active);
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
    internal readonly PlayerLayer OriTrail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      if (oPlayer.TrailPos == null) {
        oPlayer.TrailPos = new List<Vector2>();
        oPlayer.TrailFrame = new List<Vector2>();
        oPlayer.TrailAlpha = new List<float>();
        oPlayer.TrailDirection = new List<int>();
        oPlayer.TrailRotation = new List<float>();

        for (int i = 0; i < 27; i++) {
          oPlayer.TrailPos.Add(Vector2.Zero);
          oPlayer.TrailFrame.Add(Vector2.Zero);
          oPlayer.TrailRotation.Add(0);
          oPlayer.TrailDirection.Add(1);
          oPlayer.TrailAlpha.Add((i + 1) * 4);
        }
      }

      Vector2 position = drawPlayer.position;

      // modPlayer.UpdateTrail(drawPlayer);
      for (int i = 0; i < 26; i++) {
        oPlayer.TrailAlpha[i] -= 4;
        if (oPlayer.TrailAlpha[i] < 0) {
          oPlayer.TrailAlpha[i] = 0;
        }
      }
      if (!drawPlayer.dead && !drawPlayer.invis) {
        oPlayer.TrailUpdate++;
        if (oPlayer.TrailUpdate > 25) {
          oPlayer.TrailUpdate = 0;
        }
        oPlayer.TrailPos[oPlayer.TrailUpdate] = drawPlayer.position;
        oPlayer.TrailFrame[oPlayer.TrailUpdate] = oPlayer.AnimFrame;
        oPlayer.TrailDirection[oPlayer.TrailUpdate] = drawPlayer.direction;
        oPlayer.TrailAlpha[oPlayer.TrailUpdate] = 2 * (float)(Math.Sqrt(Math.Pow(Math.Abs(drawPlayer.velocity.X), 2) + Math.Pow(Math.Abs(drawPlayer.velocity.Y), 2))) + 44;
        oPlayer.TrailRotation[oPlayer.TrailUpdate] = oPlayer.AnimRads;
        if (oPlayer.TrailAlpha[oPlayer.TrailUpdate] > 104) {
          oPlayer.TrailAlpha[oPlayer.TrailUpdate] = 104;
        }
      }
      for (int i = 0; i < 26; i++) {
        SpriteEffects effect = SpriteEffects.None;

        if (oPlayer.TrailDirection[i] == -1) {
          effect = SpriteEffects.FlipHorizontally;
        }

        DrawData data = new DrawData(
          mod.GetTexture("PlayerEffects/OriGlowRight"),
          new Vector2(
            (oPlayer.TrailPos[i].X - Main.screenPosition.X) + 10,
            (oPlayer.TrailPos[i].Y - Main.screenPosition.Y) + 8
          ),
          new Rectangle(
            (int)(oPlayer.TrailFrame[i].X),
            (int)(oPlayer.TrailFrame[i].Y), 104, 76),
          Color.White * ((oPlayer.TrailAlpha[i] / 2) / 255),
          oPlayer.TrailRotation[i],
          new Vector2(52, 38), 1, effect, 0
        );
        data.position += oPlayer.Offset(oPlayer);
        Main.playerDrawData.Add(data);
      }
      // public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    internal static PlayerDrawInfo dInfo;
    internal static readonly PlayerLayer oriPlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      Player drawPlayer = drawInfo.drawPlayer;
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      oPlayer.UpdateFrame(drawPlayer);
      Vector2 position = drawPlayer.position;
      dInfo = drawInfo;
      Texture2D spriteTexture = mod.GetTexture("PlayerEffects/OriPlayer");
      
      SpriteEffects effect = SpriteEffects.None;

      if (drawPlayer.direction == -1) {
        effect = SpriteEffects.FlipHorizontally;
      }
      Vector2 pos = new Vector2(
        (drawPlayer.position.X - Main.screenPosition.X) + 10,
        (drawPlayer.position.Y - Main.screenPosition.Y) + 8
      );
      Rectangle rect = new Rectangle((int)(oPlayer.AnimFrame.X), (int)(oPlayer.AnimFrame.Y), 104, 76);
      Vector2 orig = new Vector2(52, 38);
      
      DrawData data = new DrawData(spriteTexture, pos, rect, oPlayer.SpriteColor, drawPlayer.direction * oPlayer.AnimRads, orig, 1, effect, 0);
      data.position += oPlayer.Offset(oPlayer);
      Main.playerDrawData.Add(data);
      // public DrawData(Texture2D texture, Vector2 position, Rectangle? sourceRect, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effect, int inactiveLayerDepth);
    });
    internal static readonly PlayerLayer oriTransformSprite = new PlayerLayer("OriMod", "OriTransform", delegate (PlayerDrawInfo drawInfo) {
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
      if (oPlayer.TransformTimer > 236) { // Transform Start
        float t = oPlayer.TransformTimer - 235;
        if (t > 0) {
          y = t > 390 ? 0 : t > 330 ? 1 : t > 270 ? 2 : t > 150 ? 3 : t > 110 ? 4 : t > 70 ? 5 : t > 30 ? 6 : 7;
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
    internal static readonly PlayerLayer oriBashArrow = new PlayerLayer("OriMod", "bashArrow", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/bashArrow");
      SpriteEffects effect = SpriteEffects.None;

      int frameY = 0;

      if (oPlayer.bash.CurrDuration > 40) {
        frameY = oPlayer.bash.CurrDuration < 50 ? 1 : 2;
      }
      DrawData data = new DrawData(texture,
        new Vector2(
          (oPlayer.bash.Npc.Center.X - Main.screenPosition.X),
          (oPlayer.bash.Npc.Center.Y - Main.screenPosition.Y)
        ),
        new Rectangle(0, frameY * 20, 152, 20),
        Color.White, oPlayer.bash.Npc.AngleTo(Main.MouseWorld),
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
      if (TransformTimer > 0) {
        TransformTimer -= HasTransformedOnce ? RepeatedTransformRate : 1;
        if (TransformTimer <= 0 || (TransformTimer < 236 - 62 && HasTransformedOnce)) {
          TransformTimer = 0;
          Transforming = false;
          OriSet = true;
        }
      }
      
      if (OriSet) {
        if (FlashTimer > 0) { FlashTimer--; }
        if (TeatherTrailTimer > 0) { TeatherTrailTimer--; }

        if (chargeJumpAnimTimer > 0) {
          chargeJumpAnimTimer--;
        }
      }
      if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer && doNetUpdate) {
        ModNetHandler.oriPlayerHandler.SendOriState(255, player.whoAmI);
        doNetUpdate = false;
      }
    }
    public override void Initialize() {
      Abilities = new OriAbilities(this);
      InitTestMaterial();
    }
    public override void OnEnterWorld(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.SeinMinionActive = false;
      oPlayer.SeinMinionUpgrade = 0;
    }
  }
}