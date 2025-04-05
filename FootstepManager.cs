using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// This class is used to handle creation of footstep sounds.
/// </summary>
// TODO: Convert this to an Id set?
[UsedImplicitly]
public sealed class FootstepManager : ModSystem {
  /// <summary>
  /// Dictionary of pairs where the key is a sound type, and the value is a list,
  /// where if a <see cref="ModTile"/>'s <see cref="ModType.Name"/> contains the value,
  /// that tile will play that footstep sound.
  /// </summary>
  public static readonly Dictionary<FootstepSound, List<string>> SoundsFromName = new() {
    [FootstepSound.None] = ["mysterytile", "pendingmysterytile", "unloaded"],
    [FootstepSound.Grass] = ["dirt", "grass", "mud"],
    [FootstepSound.Rock] = ["rock", "stone"],
    [FootstepSound.Wood] = ["wood"],
    [FootstepSound.Sand] = ["sand", "ash"],
    [FootstepSound.Snow] = ["snow"],
    [FootstepSound.Mushroom] = ["mushroom"],
    [FootstepSound.LightDark] = ["glass"],
    [FootstepSound.SpiritTreeRock] = ["brick"],
    [FootstepSound.SpiritTreeWood] = ["living"]
  };

  public override void SetStaticDefaults() {
    _tileFootstepSounds = new FootstepSound[TileLoader.TileCount];
    _stepSounds = new SoundInfo[(byte)FootstepSound.Count];
    _landingSounds = new SoundInfo[(byte)FootstepSound.Count];
  }

  public override void PostSetupContent() {
    AssignTiles();
    AssignModTiles();
    SetupPaths();
  }

  public override void Unload() {
    _tileFootstepSounds = null!;
    _stepSounds = null!;
    _landingSounds = null!;
  }

  private static void AssignTiles() {
    for (int i = 0; i < _tileFootstepSounds.Length; i++) {
      _tileFootstepSounds[i] = FootstepSound.None;
    }

    _tileFootstepSounds.AssignValueToKeys(FootstepSound.None, [
      TileID.Plants, TileID.Torches, TileID.Trees,
      TileID.ClosedDoor, TileID.OpenDoor, TileID.Heart, TileID.Bottles, TileID.Saplings, TileID.Chairs, TileID.Furnaces,
      TileID.Containers, TileID.CorruptPlants, TileID.DemonAltar, TileID.Sunflower, TileID.Pots, TileID.PiggyBank,
      TileID.ShadowOrbs, TileID.CorruptThorns, TileID.Candles, TileID.Chandeliers, TileID.Jackolanterns,
      TileID.Presents, TileID.HangingLanterns, TileID.WaterCandle, TileID.Books, TileID.Cobweb, TileID.Vines,
      TileID.Signs, TileID.JunglePlants, TileID.JungleVines, TileID.JungleThorns, TileID.MushroomPlants,
      TileID.MushroomTrees, TileID.Plants2, TileID.JunglePlants2, TileID.Hellforge, TileID.ClayPot, TileID.Beds,
      TileID.Cactus, TileID.Coral, TileID.ImmatureHerbs, TileID.MatureHerbs, TileID.BloomingHerbs, TileID.Tombstones,
      TileID.Loom, TileID.Bathtubs, TileID.Banners, TileID.Benches, TileID.Lampposts, TileID.Lampposts, TileID.Kegs,
      TileID.ChineseLanterns, TileID.CookingPots, TileID.Safes, TileID.SkullLanterns, TileID.TrashCan,
      TileID.Candelabras, TileID.Thrones, TileID.Bowls, TileID.GrandfatherClocks, TileID.Statues, TileID.Sawmill,
      TileID.HallowedPlants, TileID.HallowedPlants2, TileID.HallowedVines, TileID.WoodenBeam, TileID.CrystalBall,
      TileID.DiscoBall, TileID.Mannequin, TileID.Crystals, TileID.InactiveStoneBlock, TileID.Lever,
      TileID.AdamantiteForge, TileID.PressurePlates, TileID.Switches, TileID.MusicBoxes, TileID.Explosives,
      TileID.InletPump, TileID.OutletPump, TileID.Timers, TileID.HolidayLights, TileID.Stalactite, TileID.ChristmasTree,
      TileID.Sinks, TileID.PlatinumCandelabra, TileID.PlatinumCandle, TileID.ExposedGems, TileID.GreenMoss,
      TileID.BrownMoss, TileID.RedMoss, TileID.BlueMoss, TileID.PurpleMoss, TileID.LongMoss, TileID.SmallPiles,
      TileID.LargePiles, TileID.LargePiles2, TileID.CrimsonPlants, TileID.CrimsonVines, TileID.WaterFountain,
      TileID.Cannon, TileID.LandMine, TileID.SnowballLauncher, TileID.Rope, TileID.Chain, TileID.Campfire,
      TileID.Firework, TileID.Blendomatic, TileID.MeatGrinder, TileID.Extractinator, TileID.Solidifier,
      TileID.DyePlants, TileID.DyeVat, TileID.Larva, TileID.PlantDetritus, TileID.LifeFruit, TileID.LihzahrdAltar,
      TileID.PlanteraBulb, TileID.Painting3X3, TileID.Painting4X3, TileID.Painting6X4, TileID.ImbuingStation,
      TileID.BubbleMachine, TileID.Painting2X3, TileID.Painting3X2, TileID.Autohammer, TileID.Pumpkins,
      TileID.Womannequin, TileID.FireflyinaBottle, TileID.LightningBuginaBottle, TileID.BunnyCage, TileID.SquirrelCage,
      TileID.MallardDuckCage, TileID.DuckCage, TileID.BirdCage, TileID.BlueJay, TileID.CardinalCage, TileID.FishBowl,
      TileID.HeavyWorkBench, TileID.SnailCage, TileID.GlowingSnailCage, TileID.AmmoBox, TileID.MonarchButterflyJar,
      TileID.PurpleEmperorButterflyJar, TileID.RedAdmiralButterflyJar, TileID.UlyssesButterflyJar,
      TileID.SulphurButterflyJar, TileID.TreeNymphButterflyJar, TileID.ZebraSwallowtailButterflyJar,
      TileID.JuliaButterflyJar, TileID.ScorpionCage, TileID.BlackScorpionCage, TileID.FrogCage, TileID.MouseCage,
      TileID.BoneWelder, TileID.FleshCloningVat, TileID.GlassKiln, TileID.LihzahrdFurnace, TileID.LivingLoom,
      TileID.SkyMill, TileID.IceMachine, TileID.SteampunkBoiler, TileID.HoneyDispenser, TileID.PenguinCage,
      TileID.WormCage, TileID.MinecartTrack, TileID.BlueJellyfishBowl, TileID.GreenJellyfishBowl,
      TileID.PinkJellyfishBowl, TileID.ShipInABottle, TileID.SeaweedPlanter, TileID.PalmTree, TileID.BeachPiles,
      TileID.CopperCoinPile, TileID.SilverCoinPile, TileID.GoldCoinPile, TileID.PlatinumCoinPile, TileID.WeaponsRack,
      TileID.FireworksBox, TileID.LivingFire, TileID.AlphabetStatues, TileID.FireworkFountain, TileID.GrasshopperCage,
      TileID.LivingCursedFire, TileID.LivingDemonFire, TileID.LivingFrostFire, TileID.LivingIchor,
      TileID.LivingUltrabrightFire, TileID.MushroomStatue, TileID.ChimneySmoke, TileID.CrimsonThorns, TileID.VineRope,
      TileID.BewitchingTable, TileID.AlchemyTable, TileID.Sundial, TileID.GoldBirdCage, TileID.GoldBunnyCage,
      TileID.GoldButterflyCage, TileID.GoldFrogCage, TileID.GoldGrasshopperCage, TileID.GoldMouseCage,
      TileID.GoldWormCage, TileID.SilkRope, TileID.WebRope, TileID.PeaceCandle, TileID.WaterDrip, TileID.LavaDrip,
      TileID.HoneyDrip, TileID.SharpeningStation, TileID.TargetDummy, TileID.Bubble, TileID.PlanterBox,
      TileID.VineFlowers, TileID.TrapdoorOpen, TileID.TallGateClosed, TileID.TallGateOpen, TileID.LavaLamp,
      TileID.CageEnchantedNightcrawler, TileID.CageBuggy, TileID.CageGrubby, TileID.CageSluggy, TileID.ItemFrame,
      TileID.Chimney, TileID.LunarMonolith, TileID.Detonator, TileID.LunarCraftingStation, TileID.SquirrelOrangeCage,
      TileID.SquirrelGoldCage, TileID.LogicGateLamp, TileID.LogicGate, TileID.LogicSensor, TileID.WirePipe,
      TileID.AnnouncementBox, TileID.WeightedPressurePlate, TileID.WireBulb, TileID.GemLocks, TileID.FakeContainers,
      TileID.ProjectilePressurePad, TileID.GeyserTrap, TileID.BeeHive, TileID.PixelBox, TileID.SillyStreamerBlue,
      TileID.SillyStreamerGreen, TileID.SillyStreamerPink, TileID.SillyBalloonMachine, TileID.Pigronata,
      TileID.PartyMonolith, TileID.PartyBundleOfBalloonTile, TileID.PartyPresent, TileID.SandDrip, TileID.DjinnLamp,
      TileID.DefendersForge, TileID.WarTable, TileID.WarTableBanner, TileID.ElderCrystalStand, TileID.Containers2,
      TileID.FakeContainers2, TileID.Tables2
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.Grass, [
      TileID.Dirt, TileID.Grass, TileID.CorruptGrass,
      TileID.ClayBlock, TileID.Mud, TileID.JungleGrass, TileID.MushroomGrass, TileID.HallowedGrass, TileID.PineTree,
      TileID.LeafBlock, TileID.CrimsonGrass, TileID.HayBlock, TileID.LavaMoss, TileID.LivingMahoganyLeaves
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.Rock, [
      TileID.Stone, TileID.Iron, TileID.Copper, TileID.Gold,
      TileID.Silver, TileID.Demonite, TileID.Ebonstone, TileID.Meteorite, TileID.Obsidian, TileID.Hellstone,
      TileID.Sapphire, TileID.Ruby, TileID.Emerald, TileID.Topaz, TileID.Amethyst, TileID.Diamond, TileID.Cobalt,
      TileID.Mythril, TileID.Adamantite, TileID.Pearlstone, TileID.ActiveStoneBlock, TileID.Boulder, TileID.IceBlock,
      TileID.BreakableIce, TileID.CorruptIce, TileID.HallowedIce, TileID.Tin, TileID.Lead, TileID.Tungsten,
      TileID.Platinum, TileID.BoneBlock, TileID.FleshBlock, TileID.Asphalt, TileID.FleshIce, TileID.Crimstone,
      TileID.Crimtane, TileID.Chlorophyte, TileID.Palladium, TileID.Orichalcum, TileID.Titanium, TileID.MetalBars,
      TileID.Cog, TileID.Marble, TileID.Granite, TileID.Sandstone, TileID.HardenedSand, TileID.CorruptHardenedSand,
      TileID.CrimsonHardenedSand, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowHardenedSand,
      TileID.HallowSandstone, TileID.DesertFossil, TileID.FossilOre, TileID.LunarOre, TileID.LunarBlockSolar,
      TileID.LunarBlockVortex, TileID.LunarBlockNebula, TileID.LunarBlockStardust
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.Wood, [
      TileID.Tables, TileID.WorkBenches, TileID.Platforms,
      TileID.WoodBlock, TileID.Pianos, TileID.Dressers, TileID.Bookcases, TileID.TinkerersWorkbench, TileID.Ebonwood,
      TileID.RichMahogany, TileID.Pearlwood, TileID.Shadewood, TileID.WoodenSpikes, TileID.SpookyWood,
      TileID.DynastyWood, TileID.RedDynastyShingles, TileID.BlueDynastyShingles, TileID.BorealWood, TileID.PalmWood,
      TileID.FishingCrate, TileID.TrapdoorClosed
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.Sand, [
      TileID.Sand, TileID.Ash, TileID.Ebonsand,
      TileID.Pearlsand, TileID.Silt, TileID.Hive, TileID.CrispyHoneyBlock, TileID.Crimsand
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.Snow, [
      TileID.SnowBlock, TileID.RedStucco, TileID.YellowStucco,
      TileID.GreenStucco, TileID.GrayStucco, TileID.Cloud, TileID.RainCloud, TileID.Slush, TileID.HoneyBlock,
      TileID.SnowCloud
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.Mushroom, [
      TileID.CandyCaneBlock, TileID.GreenCandyCaneBlock,
      TileID.CactusBlock, TileID.MushroomBlock, TileID.SlimeBlock, TileID.FrozenSlimeBlock, TileID.BubblegumBlock,
      TileID.PumpkinBlock, TileID.Coralstone, TileID.PinkSlimeBlock, TileID.SillyBalloonPink, TileID.SillyBalloonPurple,
      TileID.SillyBalloonGreen, TileID.SillyBalloonTile
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.LightDark, [
      TileID.Glass, TileID.MagicalIceBlock,
      TileID.Sunplate, TileID.Teleporter, TileID.AmethystGemsparkOff, TileID.TopazGemsparkOff,
      TileID.SapphireGemsparkOff, TileID.EmeraldGemsparkOff, TileID.RubyGemsparkOff, TileID.DiamondGemsparkOff,
      TileID.AmberGemsparkOff, TileID.AmethystGemspark, TileID.TopazGemspark, TileID.SapphireGemspark,
      TileID.EmeraldGemspark, TileID.RubyGemspark, TileID.DiamondGemspark, TileID.AmberGemspark, TileID.Waterfall,
      TileID.Lavafall, TileID.Confetti, TileID.ConfettiBlack, TileID.Honeyfall, TileID.CrystalBlock, TileID.LunarBrick,
      TileID.TeamBlockRed, TileID.TeamBlockRedPlatform, TileID.TeamBlockGreen, TileID.TeamBlockBlue,
      TileID.TeamBlockYellow, TileID.TeamBlockPink, TileID.TeamBlockWhite, TileID.TeamBlockGreenPlatform,
      TileID.TeamBlockBluePlatform, TileID.TeamBlockYellowPlatform, TileID.TeamBlockPinkPlatform,
      TileID.TeamBlockWhitePlatform, TileID.SandFallBlock, TileID.SnowFallBlock
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.SpiritTreeRock, [
      TileID.Anvils, TileID.GrayBrick,
      TileID.RedBrick, TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick, TileID.GoldBrick,
      TileID.SilverBrick, TileID.CopperBrick, TileID.Spikes, TileID.ObsidianBrick, TileID.HellstoneBrick,
      TileID.PearlstoneBrick, TileID.IridescentBrick, TileID.Mudstone, TileID.CobaltBrick, TileID.MythrilBrick,
      TileID.MythrilAnvil, TileID.Traps, TileID.DemoniteBrick, TileID.SnowBrick, TileID.AdamantiteBeam,
      TileID.SandstoneBrick, TileID.EbonstoneBrick, TileID.RainbowBrick, TileID.TinBrick, TileID.TungstenBrick,
      TileID.PlatinumBrick, TileID.IceBrick, TileID.LihzahrdBrick, TileID.PalladiumColumn, TileID.Titanstone,
      TileID.StoneSlab, TileID.SandStoneSlab, TileID.CopperPlating, TileID.TinPlating, TileID.ChlorophyteBrick,
      TileID.CrimtaneBrick, TileID.ShroomitePlating, TileID.MartianConduitPlating, TileID.MarbleBlock,
      TileID.GraniteBlock, TileID.MeteoriteBrick, TileID.Fireplace, TileID.ConveyorBeltLeft, TileID.ConveyorBeltRight
    ]);
    _tileFootstepSounds.AssignValueToKeys(FootstepSound.SpiritTreeWood, [
      TileID.LivingWood, TileID.LivingMahogany
    ]);
  }

  private static void AssignModTiles() {
    int missingSoundCount = 0;
    Span<char> loweredName = stackalloc char[256];
    for (int i = TileID.Count; i < TileLoader.TileCount; i++) {
      if (_tileFootstepSounds[i] != FootstepSound.None) {
        continue;
      }

      if (!Main.tileSolid[i] && !Main.tileSolidTop[i]) {
        _tileFootstepSounds[i] = FootstepSound.None;
        continue;
      }

      string tileName = TileLoader.GetTile(i).Name;
      var name = tileName.AsSpan(start: tileName.LastIndexOf('.') + 1);
      name.ToLower(loweredName, null);
      FootstepSound sound = SoundFromName(loweredName[..name.Length]);
      _tileFootstepSounds[i] = sound;

      if (sound != FootstepSound.NoModTranslation) {
#if DEBUG
        OriMod.Log.Debug($"Matched sound {tileName} to {sound}");
#endif
        continue;
      }

#if DEBUG
      OriMod.Log.Debug($"Could not get appropriate sound from mod tile name \"{name}\"");
      missingSoundCount++;
#endif
    }

    if (missingSoundCount > 0) {
      OriMod.Log.Debug($"Could not guess footstep sounds for {missingSoundCount} tiles.");
    }
  }

  private static void SetupPaths() {
    AddFootstep(FootstepSound.Grass, 5, 0.15f);
    AddFootstep(FootstepSound.Rock, 5, 0.7f);
    AddFootstep(FootstepSound.Wood, 5, 0.2f);
    AddFootstep(FootstepSound.Sand, 8, 0.4f);
    AddFootstep(FootstepSound.Snow, 10, 0.45f);
    AddFootstep(FootstepSound.Mushroom, 5, 0.15f);
    AddFootstep(FootstepSound.LightDark, 10, 0.3f);
    AddFootstep(FootstepSound.SpiritTreeRock, 5, 0.7f);
    AddFootstep(FootstepSound.SpiritTreeWood, 5, 0.7f);
    AddFootstep(FootstepSound.Water, 4, 1f);

    AddLanding(FootstepSound.Grass, 2, 1f);
    AddLanding(FootstepSound.Rock, 3, 1f);
    AddLanding(FootstepSound.Wood, 5, 0.15f);
    AddLandingFromFootstep(FootstepSound.Sand);
    AddLandingFromFootstep(FootstepSound.Snow);
    AddLanding(FootstepSound.Mushroom, 5, 0.75f);
    AddLandingFromFootstep(FootstepSound.LightDark);
    AddLandingFromFootstep(FootstepSound.SpiritTreeRock);
    AddLandingFromFootstep(FootstepSound.SpiritTreeWood);
    AddLanding(FootstepSound.Water, 5, 0.15f);
    return;

    static void AddFootstep(FootstepSound sound, byte random, float volume, float pitch = 0.1f) =>
      _stepSounds[(byte)sound] = new SoundInfo($"Ori/Footsteps/{sound}/{sound}", random, volume, pitch);

    static void AddLanding(FootstepSound sound, byte random, float volume, float pitch = 0.1f) =>
      _landingSounds[(byte)sound] = new SoundInfo($"Ori/Land/{sound}/seinLands{sound}", random, volume, pitch);

    static void AddLandingFromFootstep(FootstepSound sound, float pitch = 0.2f) {
      byte index = (byte)sound;
      _landingSounds[index] = _stepSounds[index] with { Pitch = pitch };
    }
  }

  /// <summary>
  /// For external mods, attempts to get a sound based on their name.
  /// </summary>
  /// <param name="name">
  /// Name of the mod tile.
  /// </param>
  /// <returns>
  /// A <see cref="FootstepSound"/> that best represents the sound from the name, -or-
  /// <see cref="FootstepSound.NoModTranslation"/> if none could be found.
  /// </returns>
  private static FootstepSound SoundFromName(ReadOnlySpan<char> name) {
    foreach ((FootstepSound sound, var candidates) in SoundsFromName) {
      foreach (string candidate in candidates) {
        if (name.Contains(candidate, StringComparison.Ordinal)) {
          return sound;
        }
      }
    }

    return FootstepSound.NoModTranslation;
  }

  /// <summary>
  /// Array of footstep sounds, where the index corresponds to a <see cref="Tile.type"/>
  /// </summary>
  private static FootstepSound[] _tileFootstepSounds = null!; // SetStaticDefaults()

  /// <summary>
  /// Sound data for footsteps.
  /// </summary>
  private static SoundInfo[] _stepSounds = null!; // SetStaticDefaults()

  /// <summary>
  /// Sound data for when landing from the air.
  /// </summary>
  private static SoundInfo[] _landingSounds = null!; // SetStaticDefaults()

  /// <summary>
  /// Plays a footstep sound effect from the <paramref name="player"/>.
  /// </summary>
  /// <param name="player">Player to play sound effect from.</param>
  /// <returns><see cref="SlotId"/> Check this, otherwise style is default.</returns>
  public static void PlayFootstepFromPlayer(Player player) {
    if (!Main.dedServ && GetSoundFromPlayerPosition(player, out FootstepSound sound)) {
      _stepSounds[(byte)sound].Play(player.Bottom);
    }
  }

  /// <summary>
  /// Plays a landing sound effect from the <paramref name="player"/> after they hit the ground.
  /// </summary>
  /// <remarks>
  /// Not all <see cref="FootstepSound"/>s have an associated Landing sound. For those, a Footstep sound is used.
  /// </remarks>
  /// <param name="player">Player to play sound effect from.</param>
  /// <returns><see cref="SlotId"/> Check this, otherwise style is default.</returns>
  public static void PlayLandingFromPlayer(Player player) {
    if (!Main.dedServ && GetSoundFromPlayerPosition(player, out FootstepSound sound)) {
      _landingSounds[(byte)sound].Play(player.Bottom);
    }
  }

  private static bool GetSoundFromPlayerPosition(Player player, out FootstepSound sound) {
    sound = GetSoundFromPlayerPosition(player);
    return sound is not (FootstepSound.None or FootstepSound.NoModTranslation);
  }

  /// <summary>
  /// Get a <see cref="FootstepSound"/> based on where the player is standing.
  /// </summary>
  /// <param name="player"><see cref="Player"/> to get footstep sound from.</param>
  /// <returns>A <see cref="FootstepSound"/> based on <paramref name="player"/> position.</returns>
  private static FootstepSound GetSoundFromPlayerPosition(Player player) {
    Vector2 testPos = player.Bottom + new Vector2(-12, 4);
    Tile tile = Main.tile[testPos.ToTileCoordinates()];

    // Test for water
    if (tile.LiquidAmount > 0f && tile.LiquidType == LiquidID.Water) {
      return FootstepSound.Water;
    }

    testPos.Y -= 8;
    tile = Main.tile[testPos.ToTileCoordinates()];
    if (tile.LiquidAmount > 0f && tile.LiquidType == LiquidID.Water) {
      return FootstepSound.Water;
    }

    // Dry land
    testPos.Y += 12;
    tile = Main.tile[testPos.ToTileCoordinates()];
    if (tile.HasTile) {
      return _tileFootstepSounds[tile.TileType];
    }

    testPos.Y += 16;
    tile = Main.tile[testPos.ToTileCoordinates()];
    return tile.HasTile ? _tileFootstepSounds[tile.TileType] : FootstepSound.None;
  }

  /// <summary>
  /// Represents different footstep sounds.
  /// </summary>
  public enum FootstepSound : byte {
    /// <summary>
    /// Footsteps on grassy terrain.
    /// </summary>
    Grass = 0,

    /// <summary>
    /// Footsteps on rocks and stones.
    /// </summary>
    Rock = 1,

    /// <summary>
    /// Footsteps on wooden surfaces.
    /// </summary>
    Wood = 2,

    /// <summary>
    /// Footsteps on sand and other grainy surfaces, and hive blocks.
    /// </summary>
    Sand = 3,

    /// <summary>
    /// Footsteps on snowy terrain.
    /// </summary>
    Snow = 4,

    /// <summary>
    /// Footsteps on mushroom terrain.
    /// </summary>
    Mushroom = 5,

    /// <summary>
    /// Footsteps on glass surfaces.
    /// </summary>
    LightDark = 6,

    /// <summary>
    /// Footsteps on bricks.
    /// </summary>
    SpiritTreeRock = 7,

    /// <summary>
    /// Footsteps on Living Wood.
    /// </summary>
    SpiritTreeWood = 8,

    /// <summary>
    /// Footsteps on liquids from using water walking.
    /// </summary>
    Water = 9,

    /// <summary>
    /// Number of valid footstep sounds which should produce a sound.
    /// </summary>
    Count = 10,

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
