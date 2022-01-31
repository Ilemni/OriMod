using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// This class is used to handle creation of footstep sounds.
  /// </summary>
  public sealed class FootstepManager : SingleInstance<FootstepManager> {
    private FootstepManager() {
      int count = TileLoader.TileCount;
      _tileFootstepSounds = new FootstepSound[count];

      // Vanilla tiles
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.None, TileID.Plants, TileID.Torches, TileID.Trees, TileID.ClosedDoor, TileID.OpenDoor, TileID.Heart, TileID.Bottles, TileID.Saplings, TileID.Chairs, TileID.Furnaces, TileID.Containers, TileID.CorruptPlants, TileID.DemonAltar, TileID.Sunflower, TileID.Pots, TileID.PiggyBank, TileID.ShadowOrbs, TileID.CorruptThorns, TileID.Candles, TileID.Chandeliers, TileID.Jackolanterns, TileID.Presents, TileID.HangingLanterns, TileID.WaterCandle, TileID.Books, TileID.Cobweb, TileID.Vines, TileID.Signs, TileID.JunglePlants, TileID.JungleVines, TileID.JungleThorns, TileID.MushroomPlants, TileID.MushroomTrees, TileID.Plants2, TileID.JunglePlants2, TileID.Hellforge, TileID.ClayPot, TileID.Beds, TileID.Cactus, TileID.Coral, TileID.ImmatureHerbs, TileID.MatureHerbs, TileID.BloomingHerbs, TileID.Tombstones, TileID.Loom, TileID.Bathtubs, TileID.Banners, TileID.Benches, TileID.Lampposts, TileID.Lampposts, TileID.Kegs, TileID.ChineseLanterns, TileID.CookingPots, TileID.Safes, TileID.SkullLanterns, TileID.TrashCan, TileID.Candelabras, TileID.Thrones, TileID.Bowls, TileID.GrandfatherClocks, TileID.Statues, TileID.Sawmill, TileID.HallowedPlants, TileID.HallowedPlants2, TileID.HallowedVines, TileID.WoodenBeam, TileID.CrystalBall, TileID.DiscoBall, TileID.Mannequin, TileID.Crystals, TileID.InactiveStoneBlock, TileID.Lever, TileID.AdamantiteForge, TileID.PressurePlates, TileID.Switches, TileID.MusicBoxes, TileID.Explosives, TileID.InletPump, TileID.OutletPump, TileID.Timers, TileID.HolidayLights, TileID.Stalactite, TileID.ChristmasTree, TileID.Sinks, TileID.PlatinumCandelabra, TileID.PlatinumCandle, TileID.ExposedGems, TileID.GreenMoss, TileID.BrownMoss, TileID.RedMoss, TileID.BlueMoss, TileID.PurpleMoss, TileID.LongMoss, TileID.SmallPiles, TileID.LargePiles, TileID.LargePiles2, TileID.FleshWeeds, TileID.CrimsonVines, TileID.WaterFountain, TileID.Cannon, TileID.LandMine, TileID.SnowballLauncher, TileID.Rope, TileID.Chain, TileID.Campfire, TileID.Firework, TileID.Blendomatic, TileID.MeatGrinder, TileID.Extractinator, TileID.Solidifier, TileID.DyePlants, TileID.DyeVat, TileID.Larva, TileID.PlantDetritus, TileID.LifeFruit, TileID.LihzahrdAltar, TileID.PlanteraBulb, TileID.Painting3X3, TileID.Painting4X3, TileID.Painting6X4, TileID.ImbuingStation, TileID.BubbleMachine, TileID.Painting2X3, TileID.Painting3X2, TileID.Autohammer, TileID.Pumpkins, TileID.Womannequin, TileID.FireflyinaBottle, TileID.LightningBuginaBottle, TileID.BunnyCage, TileID.SquirrelCage, TileID.MallardDuckCage, TileID.DuckCage, TileID.BirdCage, TileID.BlueJay, TileID.CardinalCage, TileID.FishBowl, TileID.HeavyWorkBench, TileID.SnailCage, TileID.GlowingSnailCage, TileID.AmmoBox, TileID.MonarchButterflyJar, TileID.PurpleEmperorButterflyJar, TileID.RedAdmiralButterflyJar, TileID.UlyssesButterflyJar, TileID.SulphurButterflyJar, TileID.TreeNymphButterflyJar, TileID.ZebraSwallowtailButterflyJar, TileID.JuliaButterflyJar, TileID.ScorpionCage, TileID.BlackScorpionCage, TileID.FrogCage, TileID.MouseCage, TileID.BoneWelder, TileID.FleshCloningVat, TileID.GlassKiln, TileID.LihzahrdFurnace, TileID.LivingLoom, TileID.SkyMill, TileID.IceMachine, TileID.SteampunkBoiler, TileID.HoneyDispenser, TileID.PenguinCage, TileID.WormCage, TileID.MinecartTrack, TileID.BlueJellyfishBowl, TileID.GreenJellyfishBowl, TileID.PinkJellyfishBowl, TileID.ShipInABottle, TileID.SeaweedPlanter, TileID.PalmTree, TileID.BeachPiles, TileID.CopperCoinPile, TileID.SilverCoinPile, TileID.GoldCoinPile, TileID.PlatinumCoinPile, TileID.WeaponsRack, TileID.FireworksBox, TileID.LivingFire, TileID.AlphabetStatues, TileID.FireworkFountain, TileID.GrasshopperCage, TileID.LivingCursedFire, TileID.LivingDemonFire, TileID.LivingFrostFire, TileID.LivingIchor, TileID.LivingUltrabrightFire, TileID.MushroomStatue, TileID.ChimneySmoke, TileID.CrimtaneThorns, TileID.VineRope, TileID.BewitchingTable, TileID.AlchemyTable, TileID.Sundial, TileID.GoldBirdCage, TileID.GoldBunnyCage, TileID.GoldButterflyCage, TileID.GoldFrogCage, TileID.GoldGrasshopperCage, TileID.GoldMouseCage, TileID.GoldWormCage, TileID.SilkRope, TileID.WebRope, TileID.PeaceCandle, TileID.WaterDrip, TileID.LavaDrip, TileID.HoneyDrip, TileID.SharpeningStation, TileID.TargetDummy, TileID.Bubble, TileID.PlanterBox, TileID.VineFlowers, TileID.TrapdoorOpen, TileID.TallGateClosed, TileID.TallGateOpen, TileID.LavaLamp, TileID.CageEnchantedNightcrawler, TileID.CageBuggy, TileID.CageGrubby, TileID.CageSluggy, TileID.ItemFrame, TileID.Chimney, TileID.LunarMonolith, TileID.Detonator, TileID.LunarCraftingStation, TileID.SquirrelOrangeCage, TileID.SquirrelGoldCage, TileID.LogicGateLamp, TileID.LogicGate, TileID.LogicSensor, TileID.WirePipe, TileID.AnnouncementBox, TileID.WeightedPressurePlate, TileID.WireBulb, TileID.GemLocks, TileID.FakeContainers, TileID.ProjectilePressurePad, TileID.GeyserTrap, TileID.BeeHive, TileID.PixelBox, TileID.SillyStreamerBlue, TileID.SillyStreamerGreen, TileID.SillyStreamerPink, TileID.SillyBalloonMachine, TileID.Pigronata, TileID.PartyMonolith, TileID.PartyBundleOfBalloonTile, TileID.PartyPresent, TileID.SandDrip, TileID.DjinnLamp, TileID.DefendersForge, TileID.WarTable, TileID.WarTableBanner, TileID.ElderCrystalStand, TileID.Containers2, TileID.FakeContainers2, TileID.Tables2);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.Grass, TileID.Dirt, TileID.Grass, TileID.CorruptGrass, TileID.ClayBlock, TileID.Mud, TileID.JungleGrass, TileID.MushroomGrass, TileID.HallowedGrass, TileID.PineTree, TileID.LeafBlock, TileID.FleshGrass, TileID.HayBlock, TileID.LavaMoss, TileID.LivingMahoganyLeaves);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.Rock, TileID.Stone, TileID.Iron, TileID.Copper, TileID.Gold, TileID.Silver, TileID.Demonite, TileID.Ebonstone, TileID.Meteorite, TileID.Obsidian, TileID.Hellstone, TileID.Sapphire, TileID.Ruby, TileID.Emerald, TileID.Topaz, TileID.Amethyst, TileID.Diamond, TileID.Cobalt, TileID.Mythril, TileID.Adamantite, TileID.Pearlstone, TileID.ActiveStoneBlock, TileID.Boulder, TileID.IceBlock, TileID.BreakableIce, TileID.CorruptIce, TileID.HallowedIce, TileID.Tin, TileID.Lead, TileID.Tungsten, TileID.Platinum, TileID.BoneBlock, TileID.FleshBlock, TileID.Asphalt, TileID.FleshIce, TileID.Crimstone, TileID.Crimtane, TileID.Chlorophyte, TileID.Palladium, TileID.Orichalcum, TileID.Titanium, TileID.MetalBars, TileID.Cog, TileID.Marble, TileID.Granite, TileID.Sandstone, TileID.HardenedSand, TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowHardenedSand, TileID.HallowSandstone, TileID.DesertFossil, TileID.FossilOre, TileID.LunarOre, TileID.LunarBlockSolar, TileID.LunarBlockVortex, TileID.LunarBlockNebula, TileID.LunarBlockStardust);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.Wood, TileID.Tables, TileID.WorkBenches, TileID.Platforms, TileID.WoodBlock, TileID.Pianos, TileID.Dressers, TileID.Bookcases, TileID.TinkerersWorkbench, TileID.Ebonwood, TileID.RichMahogany, TileID.Pearlwood, TileID.Shadewood, TileID.WoodenSpikes, TileID.SpookyWood, TileID.DynastyWood, TileID.RedDynastyShingles, TileID.BlueDynastyShingles, TileID.BorealWood, TileID.PalmWood, TileID.FishingCrate, TileID.TrapdoorClosed);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.Sand, TileID.Sand, TileID.Ash, TileID.Ebonsand, TileID.Pearlsand, TileID.Silt, TileID.Hive, TileID.CrispyHoneyBlock, TileID.Crimsand);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.Snow, TileID.SnowBlock, TileID.RedStucco, TileID.YellowStucco, TileID.GreenStucco, TileID.GrayStucco, TileID.Cloud, TileID.RainCloud, TileID.Slush, TileID.HoneyBlock, TileID.SnowCloud);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.Mushroom, TileID.CandyCaneBlock, TileID.GreenCandyCaneBlock, TileID.CactusBlock, TileID.MushroomBlock, TileID.SlimeBlock, TileID.FrozenSlimeBlock, TileID.BubblegumBlock, TileID.PumpkinBlock, TileID.Coralstone, TileID.PinkSlimeBlock, TileID.SillyBalloonPink, TileID.SillyBalloonPurple, TileID.SillyBalloonGreen, TileID.SillyBalloonTile);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.LightDark, TileID.Glass, TileID.MagicalIceBlock, TileID.Sunplate, TileID.Teleporter, TileID.AmethystGemsparkOff, TileID.TopazGemsparkOff, TileID.SapphireGemsparkOff, TileID.EmeraldGemsparkOff, TileID.RubyGemsparkOff, TileID.DiamondGemsparkOff, TileID.AmberGemsparkOff, TileID.AmethystGemspark, TileID.TopazGemspark, TileID.SapphireGemspark, TileID.EmeraldGemspark, TileID.RubyGemspark, TileID.DiamondGemspark, TileID.AmberGemspark, TileID.Waterfall, TileID.Lavafall, TileID.Confetti, TileID.ConfettiBlack, TileID.Honeyfall, TileID.CrystalBlock, TileID.LunarBrick, TileID.TeamBlockRed, TileID.TeamBlockRedPlatform, TileID.TeamBlockGreen, TileID.TeamBlockBlue, TileID.TeamBlockYellow, TileID.TeamBlockPink, TileID.TeamBlockWhite, TileID.TeamBlockGreenPlatform, TileID.TeamBlockBluePlatform, TileID.TeamBlockYellowPlatform, TileID.TeamBlockPinkPlatform, TileID.TeamBlockWhitePlatform, TileID.SandFallBlock, TileID.SnowFallBlock);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.SpiritTreeRock, TileID.Anvils, TileID.GrayBrick, TileID.RedBrick, TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick, TileID.GoldBrick, TileID.SilverBrick, TileID.CopperBrick, TileID.Spikes, TileID.ObsidianBrick, TileID.HellstoneBrick, TileID.PearlstoneBrick, TileID.IridescentBrick, TileID.Mudstone, TileID.CobaltBrick, TileID.MythrilBrick, TileID.MythrilAnvil, TileID.Traps, TileID.DemoniteBrick, TileID.SnowBrick, TileID.AdamantiteBeam, TileID.SandstoneBrick, TileID.EbonstoneBrick, TileID.RainbowBrick, TileID.TinBrick, TileID.TungstenBrick, TileID.PlatinumBrick, TileID.IceBrick, TileID.LihzahrdBrick, TileID.PalladiumColumn, TileID.Titanstone, TileID.StoneSlab, TileID.SandStoneSlab, TileID.CopperPlating, TileID.TinPlating, TileID.ChlorophyteBrick, TileID.CrimtaneBrick, TileID.ShroomitePlating, TileID.MartianConduitPlating, TileID.MarbleBlock, TileID.GraniteBlock, TileID.MeteoriteBrick, TileID.Fireplace, TileID.ConveyorBeltLeft, TileID.ConveyorBeltRight);
      _tileFootstepSounds.AssignValueToKeys(FootstepSound.SpiritTreeWood, TileID.LivingWood, TileID.LivingMahogany);

      // Mod tiles
      int missingSoundCount = 0;
      for (int i = TileID.Count; i < count; i++) {
        if (!Main.tileSolid[i] && !Main.tileSolidTop[i]) {
          _tileFootstepSounds[i] = FootstepSound.None;
          continue;
        }
        string tileName = TileLoader.GetTile(i).Name;
        string name = tileName.Substring(tileName.LastIndexOf('.') + 1);
        FootstepSound sound = SoundFromName(name);
        _tileFootstepSounds[i] = sound;

        if (sound == FootstepSound.NoModTranslation) {
          // Print in debug build only, or try implementing more catches for tile names to sounds
          //OriMod.Log.Warn($"Could not get appropriate sound from mod tile name \"{name}\"");
          missingSoundCount++;
        }
      }

      if (missingSoundCount > 0) {
        OriMod.Log.Debug($"Could not guess footstep sounds for {missingSoundCount} tiles.");
      }
    }

    /// <summary>
    /// Array of footstep sounds for a given <see cref="Tile"/>, where the index corresponds to a <see cref="Tile.type"/>
    /// </summary>
    private readonly FootstepSound[] _tileFootstepSounds;

    private readonly RandomChar _rand = new RandomChar();

    /// <summary>
    /// For external mods, attempts to get a sound based on their name.
    /// </summary>
    /// <param name="name">Name of the mod tile.</param>
    /// <returns>A <see cref="FootstepSound"/> that best represents the sound from the name, -or- <see cref="FootstepSound.NoModTranslation"/> if none could be found.</returns>
    private static FootstepSound SoundFromName(string name) {
      name = name.ToLower();
      if (name == "mysterytile" || name == "pendingmysterytile") {
        return FootstepSound.None;
      }
      if (name.Contains("brick")) {
        return FootstepSound.SpiritTreeRock;
      }
      if (name.Contains("living")) {
        return FootstepSound.SpiritTreeWood;
      }
      if (name.Contains("rock") || name.Contains("stone")) {
        return FootstepSound.Rock;
      }
      if (name.Contains("glass")) {
        return FootstepSound.LightDark;
      }
      if (name.Contains("sand")) {
        return FootstepSound.Sand;
      }
      if (name.Contains("snow")) {
        return FootstepSound.Snow;
      }
      if (name.Contains("grass") || name.Contains("dirt") || name.Contains("mud")) {
        return FootstepSound.Grass;
      }
      if (name.Contains("wood")) {
        return FootstepSound.Wood;
      }

      return FootstepSound.NoModTranslation;
    }

    #region Play Footstep Methods
    /// <summary>
    /// Plays a footstep sound effect from the <paramref name="player"/>.
    /// </summary>
    /// <param name="player">Player to play sound effect from.</param>
    /// <returns><see cref="SoundEffectInstance"/> representing the sound that is played.</returns>
    public SoundEffectInstance PlayFootstepFromPlayer(Player player) {
      FootstepSound sound = GetSoundFromPlayerPosition(player);
      string mat = sound.ToString();
      int x = (int)player.Bottom.X, y = (int)player.Bottom.Y;

      SoundEffectInstance Footstep(int randLength, float volume)
        => PlayFootstep($"{mat}/{mat}{_rand.NextNoRepeat(randLength)}", x, y, volume);

      switch (sound) {
        case FootstepSound.Grass:
        case FootstepSound.Mushroom:
          return Footstep(5, 0.15f);
        case FootstepSound.Water:
          return Footstep(4, 1f);
        case FootstepSound.SpiritTreeRock:
        case FootstepSound.SpiritTreeWood:
        case FootstepSound.Rock:
          return Footstep(5, 0.7f);
        case FootstepSound.Snow:
          return Footstep(10, 0.45f);
        case FootstepSound.LightDark:
          return Footstep(10, 0.3f);
        case FootstepSound.Wood:
          return Footstep(5, 0.2f);
        case FootstepSound.Sand:
          return Footstep(8, 0.4f);
        default:
          return null;
      }
    }

    /// <summary>
    /// Plays a landing sound effect from the <paramref name="player"/> after they hit the ground.
    /// </summary>
    /// <remarks>
    /// Not all <see cref="FootstepSound"/>s have an associated Landing sound. For those, a Footstep sound is used.
    /// </remarks>
    /// <param name="player">Player to play sound effect from.</param>
    /// <returns><see cref="SoundEffectInstance"/> representing the sound that is played.</returns>
    public SoundEffectInstance PlayLandingFromPlayer(Player player) {
      FootstepSound sound = GetSoundFromPlayerPosition(player);
      string mat = sound.ToString();
      int x = (int)player.Bottom.X, y = (int)player.Bottom.Y;

      SoundEffectInstance Landing(int randLength, float volume)
        => PlayLanding($"{mat}/seinLands{mat}{_rand.NextNoRepeat(randLength)}", x, y, volume);

      switch (sound) {
        case FootstepSound.Grass:
          return Landing(2, 1);
        case FootstepSound.Mushroom:
          return Landing(5, 0.75f);
        case FootstepSound.SpiritTreeRock:
        case FootstepSound.Rock:
          mat = "Rock";
          return Landing(3, 1f);
        case FootstepSound.Water:
          return Landing(5, 0.15f);
        case FootstepSound.SpiritTreeWood:
        case FootstepSound.Wood:
          mat = "Wood";
          return Landing(5, 0.15f);
        default:
          return PlayFootstepFromPlayer(player);
      }
    }

    /// <summary>
    /// Get a <see cref="FootstepSound"/> based on where the player is standing.
    /// </summary>
    /// <param name="player"><see cref="Player"/> to get footstep sound from.</param>
    /// <returns>A <see cref="FootstepSound"/> based on <paramref name="player"/> position.</returns>
    private FootstepSound GetSoundFromPlayerPosition(Player player) {
      Vector2 testPos = player.Bottom + new Vector2(-12, 4);
      Tile tile = GetTile(testPos);

      // Test for water
      if (tile.liquid > 0f && tile.liquidType() == 0) {
        return FootstepSound.Water;
      }
      testPos.Y -= 8;
      tile = GetTile(testPos);
      if (tile.liquid > 0f && tile.liquidType() == 0) {
        return FootstepSound.Water;
      }

      // Dry land
      testPos.Y += 12;
      tile = GetTile(testPos);
      if (tile.active()) {
        return _tileFootstepSounds[tile.type];
      }
      testPos.Y += 16;
      tile = GetTile(testPos);
      return tile.active() ? _tileFootstepSounds[tile.type] : FootstepSound.None;
    }

    /// <summary>
    /// Get a <see cref="Tile"/> at <paramref name="point"/>.
    /// </summary>
    /// <param name="point">Position of the tile.</param>
    /// <returns>A <see cref="Tile"/> at the provided position.</returns>
    private static Tile GetTile(Point point) => Main.tile[point.X, point.Y];

    /// <summary>
    /// Get a <see cref="Tile"/> at <paramref name="vector"/>. <paramref name="vector"/> is converted to tile coordinates.
    /// </summary>
    /// <param name="vector">World-space position of the tile.</param>
    /// <returns>A <see cref="Tile"/> at the provided position.</returns>
    private static Tile GetTile(Vector2 vector) => GetTile(vector.ToTileCoordinates());

    /// <summary>
    /// Shorthand for <see cref="Main.PlaySound(int, int, int, int, float, float)"/>, for footstep sounds.
    /// </summary>
    private static SoundEffectInstance PlayFootstep(string path, int x, int y, float volume)
      => SoundWrapper.PlaySound(x, y, "OriMod/Sounds/Custom/NewSFX/Ori/Footsteps/" + path, volume);

    /// <summary>
    /// Shorthand for <see cref="Main.PlaySound(int, int, int, int, float, float)"/>, for landing sounds.
    /// </summary>
    private static SoundEffectInstance PlayLanding(string path, int x, int y, float volume)
      => SoundWrapper.PlaySound(x, y, "OriMod/Sounds/Custom/NewSFX/Ori/Land/" + path, volume, 0.1f);
    #endregion

    /// <summary>
    /// Enum to represent footstep sounds.
    /// </summary>
    private enum FootstepSound : byte {
      /// <summary>
      /// Footsteps on grassy terrain.
      /// </summary>
      Grass = 1,
      /// <summary>
      /// Footsteps on rocks and stones.
      /// </summary>
      Rock = 2,
      /// <summary>
      /// Footsteps on wooden surfaces.
      /// </summary>
      Wood = 3,
      /// <summary>
      /// Footsteps on sand and other grainy surfaces, and hive blocks.
      /// </summary>
      Sand = 4,
      /// <summary>
      /// Footsteps on snowy terrain.
      /// </summary>
      Snow = 5,
      /// <summary>
      /// Footsteps on mushroom terrain.
      /// </summary>
      Mushroom = 6,
      /// <summary>
      /// Footsteps on glass surfaces.
      /// </summary>
      LightDark = 7,
      /// <summary>
      /// Footsteps on bricks.
      /// </summary>
      SpiritTreeRock = 8,
      /// <summary>
      /// Footsteps on Living Wood.
      /// </summary>
      SpiritTreeWood = 9,
      /// <summary>
      /// Footsteps on liquids from using water walking.
      /// </summary>
      Water = 10,
      /// <summary>
      /// Failed attempt to convert mod tile name to a footstep sound.
      /// </summary>
      NoModTranslation = 254,
      /// <summary>
      /// For tiles that can never be stepped on (i.e. Banners, Torches), or should not have a sound.
      /// </summary>
      None = 255,
    }
  }
}
