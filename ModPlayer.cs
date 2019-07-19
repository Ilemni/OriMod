using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using OriMod.Abilities;

namespace OriMod {
  public sealed class OriPlayer : ModPlayer {
    internal static TriggersSet JustPressed => PlayerInput.Triggers.JustPressed;
    internal static TriggersSet JustReleased => PlayerInput.Triggers.JustReleased;
    internal static TriggersSet Current => PlayerInput.Triggers.Current;
    internal bool Input(bool TriggerKey) => player.whoAmI == Main.myPlayer && TriggerKey;

    #region Variables
    /// <summary>
    /// Class that contains all of OriPlayer's abilities
    /// </summary>
    /// <value></value>
    internal OriAbilities Abilities { get; private set; } // Class used for all of Ori's movements
    
    public SoulLink soulLink => Abilities.soulLink;
    public WallJump wJump => Abilities.wJump;
    public AirJump airJump => Abilities.airJump;
    public Bash bash => Abilities.bash;
    public Stomp stomp => Abilities.stomp;
    public Glide glide => Abilities.glide;
    public Climb climb => Abilities.climb;
    public ChargeJump cJump => Abilities.cJump;
    public WallChargeJump wCJump => Abilities.wCJump;
    public Dash dash => Abilities.dash;
    public ChargeDash cDash => Abilities.cDash;
    public Crouch crouch => Abilities.crouch;
    public LookUp lookUp => Abilities.lookUp;
    public Burrow burrow => Abilities.burrow;

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
    internal bool HasTransformedOnce { get; private set; }
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
    public bool justJumped { get; private set; }

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

    // Trail variables, for the trails Ori creates
    internal List<Trail> Trails;
    internal int TrailIndex = 0;
    private int TeatherTrailTimer = 0;

    // Animation Variables
    internal static int SpriteWidth => AnimationHandler.PlayerAnim.TileSize.X;
    internal static int SpriteHeight => AnimationHandler.PlayerAnim.TileSize.Y;
    internal Animations Animations;
    internal Point AnimFrame;
    /// <summary>
    /// The current sprite tile of the player in Ori state
    /// 
    /// X and Y values are based on the sprite tile coordinates, not pixel coordinates 
    /// </summary>
    /// <value></value>
    public Point AnimTile {
      get { return PixelToTile(AnimFrame); }
      internal set { AnimFrame = TileToPixel(value); }
    }
    /// <summary>
    /// The name of the animation track currently playing
    /// </summary>
    /// <value></value>
    public string AnimName {
      get {
        return _animName;
      }
      private set {
        if (value != _animName) {
          OnAnimNameChange(value);
        }
        _animName = value;
      }
    }
    private string _animName = "Default";
    internal int AnimIndex { get; private set; }
    internal float AnimTime { get; private set; } // Intentionally a float
    internal float AnimRads { get; private set; }
    internal bool AnimReversed = false;
    internal bool Flashing = false;
    private int FootstepRand = 0;
    private int JumpSoundRand = 0;
    private Point PixelToTile(Point pixel) {
      pixel.X = (int)(pixel.X / SpriteWidth);
      pixel.Y = (int)(pixel.Y / SpriteHeight);
      return pixel;
    }
    private static Point TileToPixel(Point tile) {
      tile.X *= SpriteWidth;
      tile.Y *= SpriteHeight;
      return tile;
    }

    private Color _spriteColor = Color.LightCyan;
    internal Color SpriteColor {
      get { return Main.myPlayer == player.whoAmI ? OriMod.ConfigClient.PlayerColor : _spriteColor; }
      set {
        if (Main.myPlayer != player.whoAmI) {
          _spriteColor = value;
        }
      }
    }
    private Color _spriteColorSecondary = Color.LightCyan;
    internal Color SpriteColorSecondary {
      get { return Main.myPlayer == player.whoAmI ? OriMod.ConfigClient.PlayerColorSecondary : _spriteColorSecondary; }
      set {
        if (Main.myPlayer != player.whoAmI) {
          _spriteColorSecondary = value;
        }
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
    internal bool DoPlayerLight => (player.whoAmI == Main.myPlayer || OriMod.ConfigClient.GlobalPlayerLight) ? OriMod.ConfigClient.PlayerLight : MpcPlayerLight;
    public Color LightColor = new Color(0.2f, 0.4f, 0.4f);
        #endregion

    internal void Debug(string msg) => Debug(msg, this);
    internal static void Debug(string msg, OriPlayer oPlayer) {
      if (oPlayer.debugMode && oPlayer.player.whoAmI == Main.myPlayer) {
        Main.NewText(msg);
        // ErrorLogger.Log(msg);
      }
    }
    internal SoundEffectInstance PlayNewSound(string Path) => PlayNewSound(Path, 1, 0);
    internal SoundEffectInstance PlayNewSound(string Path, float Volume) => PlayNewSound(Path, Volume, 0);
    internal SoundEffectInstance PlayNewSound(string Path, float Volume, float Pitch) =>
      Main.PlaySound((int)SoundType.Custom, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/" + Path), Volume, Pitch);
    internal SoundEffectInstance PlayFootstep(string Material, int rand, float Volume) =>
      PlayNewSound($"Ori/Footsteps/{Material}/{Material + RandomChar(rand, ref FootstepRand)}", Volume, 0.1f);
    /// <summary>
    /// Retrieves a random character of an alphabet between indices 0 and <c>length</c>
    /// </summary>
    /// <param name="length">Max letter indice to use</param>
    /// <returns>char between A and <c>alphabet[length]</c></returns>
    private static char[] alphabet { get; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    public static char RandomChar(int length) => alphabet[Main.rand.Next(length)];
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
    internal void Increment(string anim="Default", int overrideFrame=-1, float overrideTime=0, int overrideDur=0, Header overrideHeader=null, Vector2 drawOffset=new Vector2(), float rotDegrees=0) {
      if (AnimName != null) {
        // Main.NewText($"Frame called: {AnimName}, Time: {AnimTime}, AnimIndex: {AnimIndex}/{Animations.PlayerAnim.Tracks[AnimName].Frames.Length}"); // Debug
      }
      AnimationHandler.IncrementFrame(this, anim, overrideFrame, overrideTime, overrideDur, overrideHeader, drawOffset, rotDegrees);
    }
    
    internal void SetFrame(string name, int frameIndex, float time, Frame frame, float animRads) {
      AnimName = name;
      AnimIndex = frameIndex;
      AnimTime = time;
      AnimFrame = TileToPixel(frame.Tile);
      AnimRads = animRads;
    }

    private void UpdateFrame(Player drawPlayer) {
      AnimTime++;
      if (Transforming) {
        Increment(OriSet ? "TransformEnd" : "TransformStart");
        return;
      }
      if (!OriSet) return;
      if (!HasTransformedOnce) {
        HasTransformedOnce = true;
      }
      if (drawPlayer.pulley || drawPlayer.mount.Active) {
        Increment("Idle");
        return;
      }
      if (burrow.InUse) {
        double rad = Math.Atan2(burrow.Velocity.X, -burrow.Velocity.Y);
        int deg = (int)(rad * (180 / Math.PI));
        deg *= drawPlayer.direction;
        if (player.gravDir < 0) {
          deg += 180;
        }
        Increment("Burrow", rotDegrees:deg);
        return;
      }
      if (wCJump.Active) {
        float rad = (float)Math.Atan2(player.velocity.Y, player.velocity.X);
        float deg = rad * (float)(180 / Math.PI) * player.direction;
        if (player.direction == -1) {
          deg -= 180f;
        }
        Increment("Dash", overrideFrame:0, rotDegrees:deg);
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
            Increment("ChargeJump", rotDegrees:180f, overrideDur:2, overrideHeader:new Header(playback:PlaybackMode.PingPong));
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
            Increment("GlideStart", overrideHeader:new Header(playback:PlaybackMode.Reverse));
            return;
        }
      }
      if (climb.InUse) {
        if (climb.IsCharging) {
          if (!wCJump.Charged) {
            Increment("WallChargeJumpCharge", overrideFrame: wCJump.Refreshed ? -1 : 0);
            return;
          }
          int frame = 0;
          float angle;
          wCJump.GetMouseDirection(out angle);
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
          Increment("WallChargeJumpAim", overrideFrame:frame);
          return;
        }
        if (Math.Abs(player.velocity.Y) < 0.1f) {
          Increment("ClimbIdle");
        }
        else {
          Increment(player.velocity.Y * player.gravDir < 0 ? "Climb" : "WallSlide", overrideTime:AnimTime+Math.Abs(drawPlayer.velocity.Y)*0.1f);
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
            Increment("LookUpStart", overrideHeader:new Header(playback:PlaybackMode.Reverse));
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
            Increment("CrouchStart", overrideHeader:new Header(playback:PlaybackMode.Reverse));
            return;
        }
      }
          
      if (cJump.Active) {
        Increment("ChargeJump");
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
        TestStepMaterial();
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
          drawPlayer.Top.X + (drawPlayer.direction == -1 ? -4 : 2),
          drawPlayer.Top.Y + drawPlayer.height - 2);
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
      Transforming = true;
      TransformDirection = player.direction;
      TransformTimer = Animations.PlayerAnim.Source.Tracks["TransformEnd"].Duration + Animations.PlayerAnim.Source.Tracks["TransformStart"].Duration;
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
    }
    
    // Gets the tile that's a given offset from player.Center.X, player.position.Y + player.height
    private Tile GetTile(float offsetX, float offsetY) {
      Vector2 pos = new Vector2(player.Center.X + offsetX, (player.position.Y + player.height) + offsetY);
      Vector2 tilepos = new Vector2(pos.ToTileCoordinates().X, pos.ToTileCoordinates().Y);
      return Main.tile[(int)tilepos.X, (int)tilepos.Y];
    }
    private void GetMaterial(Tile tile) {
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
    private void TestStepMaterial() {
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
            GetMaterial(tile);
          }
          else {
            tile = GetTile(-12, 24);
            if (tile.active()) {
              GetMaterial(tile);
            }
            else {
              FloorMaterial = "Grass";
              if (!UnassignedTiles.Contains(tile.type)) {
                Debug("Tile ID " + tile.type + " is not assigned to a material");
                UnassignedTiles.Add(tile.type);
              }
            }
          }
        }
      }
    }
    internal void RemoveSeinBuffs(int exclude=0) {
      for (int u = 1; u <= OriMod.SeinUpgrades.Count; u++) {
        if (u != exclude) {
          player.ClearBuff(mod.GetBuff("SeinBuff" + u).Type);
        }
      }
    }
    public override void PostUpdate() {
      if (!Main.dedServ) UpdateFrame(player);
      CheckSeinBuffs();
      if (!OriSet) return;
      if (DoPlayerLight && !burrow.Active) Lighting.AddLight(player.Center, LightColor.ToVector3());
      justJumped = player.justJumped;
      if (justJumped) {
        PlayNewSound("Ori/Jump/seinJumpsGrass" + RandomChar(5, ref JumpSoundRand), 0.75f);
      }
      CheckOnGround();
      CheckOnWall();
    }
    private void CheckSeinBuffs() {
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
    }
    private void CheckOnGround() {
      IsGrounded = false;
      if (player.fireWalk || player.waterWalk || player.waterWalk2) {
        Vector2 feetVect = player.gravDir > 0 ? player.Bottom : player.Top;
        feetVect.Y += (1f / 255f) * player.gravDir;
        Point pos = feetVect.ToTileCoordinates();
        bool testblock = Main.tile[pos.X, pos.Y].liquid > 0 && Main.tile[pos.X, pos.Y - 1].liquid == 0;
        if (testblock) {
          Tile tile = Main.tile[pos.X, pos.Y];
          IsGrounded = tile.lava() ? player.fireWalk : (player.waterWalk || player.waterWalk2);
        }
      }
      if (!IsGrounded) {
        IsGrounded = !Collision.IsClearSpotTest(player.position + new Vector2(0, 8 * player.gravDir), 16f, player.width, player.height, false, false, (int)player.gravDir, true, true);
      }
    }
    private void CheckOnWall() {
      Point p = new Vector2(
        player.Center.X + player.direction + player.direction * player.width * 0.5f,
        player.position.Y + (player.gravDir < 0f ? -1f : 2f)
      ).ToTileCoordinates();
      OnWall = WorldGen.SolidTile(p.X, p.Y + 1) && WorldGen.SolidTile(p.X, p.Y + 2);
    }
    internal void KillGrapples() {
      for (int j = 0; j < 1000; j++) {
        Projectile proj = Main.projectile[j];
        if (proj.active && proj.owner == player.whoAmI && proj.aiStyle == 7) {
          Main.projectile[j].Kill();
        }
      }
    }
    public override void PostUpdateRunSpeeds() {
      if (OriSet && !Transforming) {
        DefaultPostRunSpeeds();
        if (OriMod.ConfigClient.SmoothCamera) {
          Main.SetCameraLerp(OriModUtils.IsAnyBossAlive(check:Main.time % 120 == 0) ? 0.15f : 0.05f, 1);
        }
        if (OnWall && (IsGrounded || player.velocity.Y < 0) && !climb.InUse) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
          player.jumpSpeedBoost -= 6f;
        }
        else if (OnWall && !IsGrounded && player.velocity.Y > 0 && !stomp.InUse) {
          player.gravity = 0.1f;
          player.maxFallSpeed = 6f;
        }
        Abilities.Tick();
        Abilities.Update();
      }
      if (Transforming) {
        player.direction = TransformDirection;
        int dur = AnimationHandler.PlayerAnim.Tracks["TransformEnd"].Duration;
        if (TransformTimer > dur - 10) {
          if (TransformTimer < dur) {
            player.gravity = 9f;
            OriSet = true;
          }
          else {
            player.velocity = new Vector2(0, -0.00055f * TransformTimer);
            player.gravity = 0;
            if (TeatherTrailTimer == 0) {
              CreateTeatherDust();
              TeatherTrailTimer = Main.rand.Next(3, 8);
            }
          }
        }
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.immune = true;
      }
      
      if (ImmuneTimer > 0) {
        ImmuneTimer--;
        player.immune = true;
      }
    }
    private void DefaultPostRunSpeeds() {
      player.runAcceleration = 0.5f;
      player.maxRunSpeed += 2f;
      player.noFallDmg = true;
      player.gravity = 0.35f;
      player.jumpSpeedBoost += 2f;
      if (Input(Current.Left) || Input(Current.Right) || IsGrounded) {
        UnrestrictedMovement = false;
      }
      player.runSlowdown = UnrestrictedMovement ? 0 : 1;
    }
    public void CreateTeatherDust() {
      Dust dust = Main.dust[Terraria.Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
      dust.noGravity = false;
    }
    public override void FrameEffects() {
      if (!OriSet) return;

      if (player.velocity != Vector2.Zero && TeatherTrailTimer == 0) {
        CreateTeatherDust();
        TeatherTrailTimer = dash.InUse || cDash.InUse ? Main.rand.Next(2, 4) : Main.rand.Next(10, 15);
      }
      else if ((dash.InUse || cDash.InUse) && TeatherTrailTimer > 4) {
        TeatherTrailTimer = Main.rand.Next(2, 4);
      }
      Flashing = player.immuneTime % 12 > 6 && !player.immuneNoBlink;
    }
    public override void OnHitByNPC(NPC npc, int damage, bool crit) {
      OriNPC oNpc = npc.GetGlobalNPC<OriNPC>(mod);
      if (oNpc.IsBashed || stomp.InUse || cDash.InUse || cJump.InUse) {
        damage = 0;
      }
    }
    private int hurtRand = 0;
    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { // effects when character is hurt
      if (!OriSet) return true;
      playSound = false;
      genGore = false;
      if (stomp.InUse || cDash.InUse || cJump.InUse) {
        damage = 0;
        return false;
      }
      else {
        PlayNewSound("Ori/Hurt/seinHurtRegular" + RandomChar(4, ref hurtRand));
        UnrestrictedMovement = true;
      }
      return true;
    }
    public override TagCompound Save() {
      return new TagCompound {
        {"OriSet", OriSet},
        {"Debug", debugMode},
      };
    }
    public override void Load(TagCompound tag) {
      OriSet = tag.GetBool("OriSet");
      debugMode = tag.GetBool("Debug");
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
    public override void UpdateDead() {
      soulLink.UpdateDead();
    }
    public override void ModifyDrawLayers(List<PlayerLayer> layers) {
      if (Main.dedServ) return;
      if (OriSet || Transforming) {
        DisableVanillaLayers();
        if (soulLink.PlacedSoulLink) layers.Insert(0, OriLayers.SoulLinkLayer);
        Animations.PlayerAnim.Draw(layers, 9, force:true);
        if (OriSet) {
          Animations.SecondaryLayer.Draw(layers, 10, force:true);
        }
        Animations.TrailAnim.Draw(layers);
        Animations.GlideAnim.Draw(layers);
        Animations.BashAnim.Draw(layers);
        player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
        OriLayers.PlayerSprite.visible = (!player.dead && !player.invis);
        OriLayers.Trail.visible = OriLayers.PlayerSprite.visible && !player.mount.Active;
      }
    }
    private void DisableVanillaLayers() {
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
      if (stomp.InUse || airJump.InUse || burrow.InUse || cJump.InUse || wCJump.InUse || OnWall || Transforming) {
        PlayerLayer.Wings.visible = false;
      }
    }
    public override void ResetEffects() {
      if (Transforming) {
        float rate = HasTransformedOnce ? RepeatedTransformRate : 1; 
        AnimTime += rate - 1;
        TransformTimer -= rate;
        if (TransformTimer < 0 || (HasTransformedOnce && TransformTimer < Animations.PlayerAnim.Source.Tracks["TransformEnd"].Duration - 62)) {
          TransformTimer = 0;
          Transforming = false;
          OriSet = true;
        }
      }
      if (OriSet) {
        if (TeatherTrailTimer > 0) TeatherTrailTimer--;
      }
      if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer && doNetUpdate) {
        ModNetHandler.oriPlayerHandler.SendOriState(255, player.whoAmI);
        doNetUpdate = false;
      }
    }
    private void OnAnimNameChange(string value) {
      if (Main.dedServ) return;
      Animations.PlayerAnim.OnAnimNameChange(value);
      Animations.SecondaryLayer.OnAnimNameChange(value);
      Animations.TrailAnim.OnAnimNameChange(value);
      Animations.BashAnim.OnAnimNameChange(value);
      Animations.GlideAnim.OnAnimNameChange(value);
    }
    public override void Initialize() {
      Abilities = new OriAbilities(this);
      if (!Main.dedServ) {
        Animations = new Animations(this);
      }
      InitTestMaterial();
      Trails = new List<Trail>();
      for (int i = 0; i < 26; i++) {
        Trails.Add(new Trail());
      }
      TileCollection.Init();
    }
    public override void OnEnterWorld(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.SeinMinionActive = false;
      oPlayer.SeinMinionUpgrade = 0;
    }
    internal void ResetData() {
      OriSet = false;
      HasTransformedOnce = false;
      UnrestrictedMovement = false;
      SeinMinionActive = false;
      SeinMinionUpgrade = 0;
    }
  }
}