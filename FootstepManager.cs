using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// This class is used to handle creation of footstep sounds
  /// </summary>
  public sealed class FootstepManager : SingleInstance<FootstepManager> {
    private FootstepManager() {
      int count = TileLoader.TileCount;
      TileFootstepSounds = new FootstepSound[count];

      // Vanilla tiles
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.None, new int[] {
        TileID.Plants, TileID.Torches, TileID.Trees, TileID.ClosedDoor, TileID.OpenDoor, TileID.Heart, TileID.Bottles, TileID.Saplings,
        TileID.Chairs, TileID.Furnaces, TileID.Containers, TileID.CorruptPlants, TileID.DemonAltar, TileID.Sunflower, TileID.Pots, TileID.PiggyBank, TileID.ShadowOrbs,
        TileID.CorruptThorns, TileID.Candles, TileID.Chandeliers, TileID.Jackolanterns, TileID.Presents,
        TileID.HangingLanterns, TileID.WaterCandle, TileID.Books, TileID.Cobweb, TileID.Vines, TileID.Signs,
        TileID.JunglePlants, TileID.JungleVines, TileID.JungleThorns, TileID.MushroomPlants, TileID.MushroomTrees, TileID.Plants2,
        TileID.JunglePlants2, TileID.Hellforge, TileID.ClayPot, TileID.Beds, TileID.Cactus, TileID.Coral, TileID.ImmatureHerbs,
        TileID.MatureHerbs, TileID.BloomingHerbs, TileID.Tombstones, TileID.Loom, TileID.Bathtubs, TileID.Banners, TileID.Benches,
        TileID.Lampposts, TileID.Lampposts, TileID.Kegs, TileID.ChineseLanterns, TileID.CookingPots, TileID.Safes,
        TileID.SkullLanterns, TileID.TrashCan, TileID.Candelabras, TileID.Thrones, TileID.Bowls,
        TileID.GrandfatherClocks, TileID.Statues, TileID.Sawmill, TileID.HallowedPlants, TileID.HallowedPlants2,
        TileID.HallowedVines, TileID.WoodenBeam, TileID.CrystalBall, TileID.DiscoBall,
        TileID.Mannequin, TileID.Crystals, TileID.InactiveStoneBlock, TileID.Lever, TileID.AdamantiteForge, TileID.PressurePlates, TileID.Switches,
        TileID.MusicBoxes, TileID.Explosives, TileID.InletPump, TileID.OutletPump, TileID.Timers,
        TileID.HolidayLights, TileID.Stalactite, TileID.ChristmasTree, TileID.Sinks, TileID.PlatinumCandelabra, TileID.PlatinumCandle,
        TileID.ExposedGems, TileID.GreenMoss, TileID.BrownMoss, TileID.RedMoss, TileID.BlueMoss, TileID.PurpleMoss,
        TileID.LongMoss, TileID.SmallPiles, TileID.LargePiles, TileID.LargePiles2, TileID.FleshWeeds,
        TileID.CrimsonVines, TileID.WaterFountain, TileID.Cannon, TileID.LandMine, TileID.SnowballLauncher,
        TileID.Rope, TileID.Chain, TileID.Campfire, TileID.Firework, TileID.Blendomatic, TileID.MeatGrinder,
        TileID.Extractinator, TileID.Solidifier, TileID.DyePlants, TileID.DyeVat, TileID.Larva, TileID.PlantDetritus,
        TileID.LifeFruit, TileID.LihzahrdAltar, TileID.PlanteraBulb, TileID.Painting3X3,
        TileID.Painting4X3, TileID.Painting6X4, TileID.ImbuingStation, TileID.BubbleMachine, TileID.Painting2X3,
        TileID.Painting3X2, TileID.Autohammer, TileID.Pumpkins, TileID.Womannequin, TileID.FireflyinaBottle,
        TileID.LightningBuginaBottle, TileID.BunnyCage, TileID.SquirrelCage, TileID.MallardDuckCage, TileID.DuckCage,
        TileID.BirdCage, TileID.BlueJay, TileID.CardinalCage, TileID.FishBowl, TileID.HeavyWorkBench,
        TileID.SnailCage, TileID.GlowingSnailCage, TileID.AmmoBox, TileID.MonarchButterflyJar,
        TileID.PurpleEmperorButterflyJar, TileID.RedAdmiralButterflyJar, TileID.UlyssesButterflyJar,
        TileID.SulphurButterflyJar, TileID.TreeNymphButterflyJar, TileID.ZebraSwallowtailButterflyJar,
        TileID.JuliaButterflyJar, TileID.ScorpionCage, TileID.BlackScorpionCage, TileID.FrogCage, TileID.MouseCage,
        TileID.BoneWelder, TileID.FleshCloningVat, TileID.GlassKiln, TileID.LihzahrdFurnace, TileID.LivingLoom,
        TileID.SkyMill, TileID.IceMachine, TileID.SteampunkBoiler, TileID.HoneyDispenser, TileID.PenguinCage,
        TileID.WormCage, TileID.MinecartTrack, TileID.BlueJellyfishBowl, TileID.GreenJellyfishBowl,
        TileID.PinkJellyfishBowl, TileID.ShipInABottle, TileID.SeaweedPlanter, TileID.PalmTree, TileID.BeachPiles,
        TileID.CopperCoinPile, TileID.SilverCoinPile, TileID.GoldCoinPile, TileID.PlatinumCoinPile,
        TileID.WeaponsRack, TileID.FireworksBox, TileID.LivingFire, TileID.AlphabetStatues, TileID.FireworkFountain,
        TileID.GrasshopperCage, TileID.LivingCursedFire, TileID.LivingDemonFire, TileID.LivingFrostFire, TileID.LivingIchor,
        TileID.LivingUltrabrightFire, TileID.MushroomStatue, TileID.ChimneySmoke, TileID.CrimtaneThorns, TileID.VineRope,
        TileID.BewitchingTable, TileID.AlchemyTable, TileID.Sundial, TileID.GoldBirdCage, TileID.GoldBunnyCage,
        TileID.GoldButterflyCage, TileID.GoldFrogCage, TileID.GoldGrasshopperCage, TileID.GoldMouseCage, TileID.GoldWormCage,
        TileID.SilkRope, TileID.WebRope, TileID.PeaceCandle, TileID.WaterDrip, TileID.LavaDrip, TileID.HoneyDrip,
        TileID.SharpeningStation, TileID.TargetDummy, TileID.Bubble, TileID.PlanterBox, TileID.VineFlowers,
        TileID.TrapdoorOpen, TileID.TallGateClosed, TileID.TallGateOpen, TileID.LavaLamp, TileID.CageEnchantedNightcrawler,
        TileID.CageBuggy, TileID.CageGrubby, TileID.CageSluggy, TileID.ItemFrame, TileID.Chimney, TileID.LunarMonolith,
        TileID.Detonator, TileID.LunarCraftingStation, TileID.SquirrelOrangeCage, TileID.SquirrelGoldCage, TileID.LogicGateLamp,
        TileID.LogicGate, TileID.LogicSensor, TileID.WirePipe, TileID.AnnouncementBox, TileID.WeightedPressurePlate,
        TileID.WireBulb, TileID.GemLocks, TileID.FakeContainers, TileID.ProjectilePressurePad, TileID.GeyserTrap, TileID.BeeHive,
        TileID.PixelBox, TileID.SillyStreamerBlue, TileID.SillyStreamerGreen, TileID.SillyStreamerPink,
        TileID.SillyBalloonMachine, TileID.Pigronata, TileID.PartyMonolith, TileID.PartyBundleOfBalloonTile, TileID.PartyPresent,
        TileID.SandDrip, TileID.DjinnLamp, TileID.DefendersForge, TileID.WarTable, TileID.WarTableBanner,
        TileID.ElderCrystalStand, TileID.Containers2, TileID.FakeContainers2, TileID.Tables2
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.Grass, new int[] {
        TileID.Dirt, TileID.Grass, TileID.CorruptGrass, TileID.ClayBlock, TileID.Mud, TileID.JungleGrass, TileID.MushroomGrass,
        TileID.HallowedGrass, TileID.PineTree, TileID.LeafBlock, TileID.FleshGrass, TileID.HayBlock, TileID.LavaMoss,
        TileID.LivingMahoganyLeaves
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.Rock, new int[] {
        TileID.Stone, TileID.Iron, TileID.Copper, TileID.Gold, TileID.Silver, TileID.Demonite, TileID.Ebonstone,
        TileID.Meteorite, TileID.Obsidian, TileID.Hellstone, TileID.Sapphire, TileID.Ruby, TileID.Emerald, TileID.Topaz,
        TileID.Amethyst, TileID.Diamond, TileID.Cobalt, TileID.Mythril, TileID.Adamantite, TileID.Pearlstone,
        TileID.ActiveStoneBlock, TileID.Boulder, TileID.IceBlock, TileID.BreakableIce, TileID.CorruptIce, TileID.HallowedIce,
        TileID.Tin, TileID.Lead, TileID.Tungsten, TileID.Platinum, TileID.BoneBlock, TileID.FleshBlock, TileID.Asphalt,
        TileID.FleshIce, TileID.Crimstone, TileID.Crimtane, TileID.Chlorophyte, TileID.Palladium, TileID.Orichalcum,
        TileID.Titanium, TileID.MetalBars, TileID.Cog, TileID.Marble, TileID.Granite, TileID.Sandstone, TileID.HardenedSand,
        TileID.CorruptHardenedSand, TileID.CrimsonHardenedSand, TileID.CorruptSandstone, TileID.CrimsonSandstone,
        TileID.HallowHardenedSand, TileID.HallowSandstone, TileID.DesertFossil, TileID.FossilOre, TileID.LunarOre,
        TileID.LunarBlockSolar, TileID.LunarBlockVortex, TileID.LunarBlockNebula, TileID.LunarBlockStardust
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.Wood, new int[] {
        TileID.Tables, TileID.WorkBenches, TileID.Platforms, TileID.WoodBlock, TileID.Pianos, TileID.Dressers, TileID.Bookcases,
        TileID.TinkerersWorkbench, TileID.Ebonwood, TileID.RichMahogany, TileID.Pearlwood, TileID.Shadewood, TileID.WoodenSpikes, TileID.SpookyWood,
        TileID.DynastyWood, TileID.RedDynastyShingles, TileID.BlueDynastyShingles, TileID.BorealWood, TileID.PalmWood, TileID.FishingCrate, TileID.TrapdoorClosed
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.Sand, new int[] {
        TileID.Sand, TileID.Ash, TileID.Ebonsand, TileID.Pearlsand, TileID.Silt, TileID.Hive, TileID.CrispyHoneyBlock, TileID.Crimsand
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.Snow, new int[] {
        TileID.SnowBlock, TileID.RedStucco, TileID.YellowStucco, TileID.GreenStucco, TileID.GrayStucco,
        TileID.Cloud, TileID.RainCloud, TileID.Slush, TileID.HoneyBlock, TileID.SnowCloud
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.Mushroom, new int[] {
        TileID.CandyCaneBlock, TileID.GreenCandyCaneBlock, TileID.CactusBlock, TileID.MushroomBlock, TileID.SlimeBlock,
        TileID.FrozenSlimeBlock, TileID.BubblegumBlock, TileID.PumpkinBlock, TileID.Coralstone, TileID.PinkSlimeBlock,
        TileID.SillyBalloonPink, TileID.SillyBalloonPurple, TileID.SillyBalloonGreen, TileID.SillyBalloonTile
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.LightDark, new int[] {
        TileID.Glass, TileID.MagicalIceBlock, TileID.Sunplate, TileID.Teleporter, TileID.AmethystGemsparkOff, TileID.TopazGemsparkOff,
        TileID.SapphireGemsparkOff, TileID.EmeraldGemsparkOff, TileID.RubyGemsparkOff, TileID.DiamondGemsparkOff,
        TileID.AmberGemsparkOff, TileID.AmethystGemspark, TileID.TopazGemspark, TileID.SapphireGemspark,
        TileID.EmeraldGemspark, TileID.RubyGemspark, TileID.DiamondGemspark, TileID.AmberGemspark, TileID.Waterfall,
        TileID.Lavafall, TileID.Confetti, TileID.ConfettiBlack, TileID.Honeyfall, TileID.CrystalBlock, TileID.LunarBrick,
        TileID.TeamBlockRed, TileID.TeamBlockRedPlatform, TileID.TeamBlockGreen, TileID.TeamBlockBlue, TileID.TeamBlockYellow,
        TileID.TeamBlockPink, TileID.TeamBlockWhite, TileID.TeamBlockGreenPlatform, TileID.TeamBlockBluePlatform,
        TileID.TeamBlockYellowPlatform, TileID.TeamBlockPinkPlatform, TileID.TeamBlockWhitePlatform, TileID.SandFallBlock,
        TileID.SnowFallBlock
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.SpiritTreeRock, new int[] {
        TileID.Anvils, TileID.GrayBrick, TileID.RedBrick, TileID.BlueDungeonBrick, TileID.GreenDungeonBrick,
        TileID.PinkDungeonBrick, TileID.GoldBrick, TileID.SilverBrick, TileID.CopperBrick, TileID.Spikes, TileID.ObsidianBrick,
        TileID.HellstoneBrick, TileID.PearlstoneBrick, TileID.IridescentBrick, TileID.Mudstone, TileID.CobaltBrick,
        TileID.MythrilBrick, TileID.MythrilAnvil, TileID.Traps, TileID.DemoniteBrick,  TileID.SnowBrick, TileID.AdamantiteBeam,
        TileID.SandstoneBrick, TileID.EbonstoneBrick, TileID.RainbowBrick, TileID.TinBrick, TileID.TungstenBrick,
        TileID.PlatinumBrick, TileID.IceBrick, TileID.LihzahrdBrick, TileID.PalladiumColumn, TileID.Titanstone, TileID.StoneSlab,
        TileID.SandStoneSlab, TileID.CopperPlating, TileID.TinPlating, TileID.ChlorophyteBrick, TileID.CrimtaneBrick,
        TileID.ShroomitePlating, TileID.MartianConduitPlating, TileID.MarbleBlock, TileID.GraniteBlock, TileID.MeteoriteBrick,
        TileID.Fireplace, TileID.ConveyorBeltLeft, TileID.ConveyorBeltRight
      });
      OriUtils.AssignValueToKeys(TileFootstepSounds, FootstepSound.SpiritTreeWood, new int[] {
        TileID.LivingWood, TileID.LivingMahogany
      });

      // Mod tiles
      for (int i = TileID.Count; i < count; i++) {
        if (!Main.tileSolid[i] && !Main.tileSolidTop[i]) {
          TileFootstepSounds[i] = FootstepSound.None;
          continue;
        }
        var tileName = TileLoader.GetTile(i).Name;
        var name = tileName.Substring(tileName.LastIndexOf('.') + 1);
        TileFootstepSounds[i] = SoundFromName(name);
      }
    }

    public readonly FootstepSound[] TileFootstepSounds;

    private readonly RandomChar randomChar = new RandomChar();

    private FootstepSound SoundFromName(string name) {
      name = name.ToLower();
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

      OriMod.Log.Warn($"Could not get appropriate sound from mod tile name \"{name}\"");
      return FootstepSound.NoModTranslation;
    }

    #region Play Footstep Methods
    public SoundEffectInstance PlayFootstepFromPlayer(Player player) {
      var sound = GetSoundFromPlayerPosition(player);
      string mat = sound.ToString();
      int x = (int)player.Bottom.X, y = (int)player.Bottom.Y;

      SoundEffectInstance Footstep(int randLength, float volume)
        => PlayFootstep($"{mat}/{mat}{randomChar.NextNoRepeat(randLength)}", x, y, volume);

      switch (sound) {
        case FootstepSound.Grass:
        case FootstepSound.Mushroom:
          return Footstep(5, 0.15f);
        case FootstepSound.Water:
          return Footstep(4, 1f);
        case FootstepSound.SpiritTreeRock:
        case FootstepSound.SpiritTreeWood:
        case FootstepSound.Rock:
          return Footstep(5, 1f);
        case FootstepSound.Snow:
        case FootstepSound.LightDark:
          return Footstep(10, 0.85f);
        case FootstepSound.Wood:
          return Footstep(5, 0.85f);
        case FootstepSound.Sand:
          return Footstep(8, 0.85f);
        default:
          return null;
      }
    }

    public SoundEffectInstance PlayLandingFromPlayer(Player player) {
      var sound = GetSoundFromPlayerPosition(player);
      string mat = sound.ToString();
      int x = (int)player.Bottom.X, y = (int)player.Bottom.Y;

      SoundEffectInstance Landing(int randLength, float volume)
        => PlayLanding($"{mat}/seinLands{mat}{randomChar.NextNoRepeat(randLength)}", x, y, volume);

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
          return Landing(5, 0.25f);
        case FootstepSound.SpiritTreeWood:
        case FootstepSound.Wood:
          mat = "Wood";
          return Landing(5, 0.85f);
        default:
          return PlayFootstepFromPlayer(player);
      }
    }

    private FootstepSound GetSoundFromPlayerPosition(Player player) {
      var testPos = player.Bottom + new Vector2(-12, 4);
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
        return TileFootstepSounds[tile.type];
      }
      testPos.Y += 16;
      tile = GetTile(testPos);
      if (tile.active()) {
        return TileFootstepSounds[tile.type];
      }

      // Nothing
      return FootstepSound.None;
    }

    private Tile GetTile(Point point) => Main.tile[point.X, point.Y];
    
    private Tile GetTile(Vector2 vector) => GetTile(vector.ToTileCoordinates());

    private SoundEffectInstance PlayFootstep(string path, int x, int y, float volume)
      => Main.PlaySound((int)SoundType.Custom, x, y, OriMod.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/Ori/Footsteps/" + path), volume);
    
    private SoundEffectInstance PlayLanding(string path, int x, int y, float volume)
      => Main.PlaySound((int)SoundType.Custom, x, y, OriMod.Instance.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/Ori/Land/" + path), volume, 0.1f);
    #endregion

    public enum FootstepSound : byte {
      /// <summary>
      /// TileIDs pending assignment
      /// </summary>
      Unassigned = 0,
      Grass = 1,
      Rock = 2,
      Wood = 3,
      Sand = 4,
      Snow = 5,
      Mushroom = 6,
      LightDark = 7,
      SpiritTreeRock = 8,
      SpiritTreeWood = 9,
      Water = 10,
      /// <summary>
      /// Failed attempt to convert string to a footstep sound
      /// </summary>
      NoModTranslation = 254,
      /// <summary>
      /// For tiles that can never be stepped on (i.e. Banners, Torches)
      /// </summary>
      None = 255,
    }
  }
}
