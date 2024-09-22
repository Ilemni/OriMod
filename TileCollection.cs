using JetBrains.Annotations;
using OriMod.Utilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Contains the Pickaxe power of each tile. Used for <see cref="Abilities.Burrow"/>.
/// </summary>
[UsedImplicitly]
internal sealed class TileCollection : ModSystem {
  public override void SetStaticDefaults() {
    TilePickaxeMin = new ushort[TileLoader.TileCount];
  }

  public override void PostSetupContent() {
    // Assign vanilla tiles to 2 (Sand-like is 0, Dirt-like is 1)
    for (int i = 0; i < TileID.Count; i++) {
      TilePickaxeMin[i] = 2;
    }

    TilePickaxeMin.AssignValueToKeys<ushort>(0, stackalloc ushort[] { TileID.Sand, TileID.Slush, TileID.Silt });
    TilePickaxeMin.AssignValueToKeys<ushort>(1, stackalloc ushort[] {
      TileID.Dirt, TileID.Mud, TileID.ClayBlock, TileID.SnowBlock,
      TileID.Grass, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.HallowedGrass, TileID.JungleGrass,
      TileID.MushroomGrass
    });
    TilePickaxeMin.AssignValueToKeys<ushort>(50, stackalloc ushort[] { TileID.Meteorite });
    TilePickaxeMin.AssignValueToKeys<ushort>(55, stackalloc ushort[] { TileID.Demonite, TileID.Crimtane });
    TilePickaxeMin.AssignValueToKeys<ushort>(65, stackalloc ushort[] {
      TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, TileID.Hellstone, TileID.Obsidian, TileID.DesertFossil,
      TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick
    });
    TilePickaxeMin.AssignValueToKeys<ushort>(100, stackalloc ushort[] { TileID.Cobalt, TileID.Palladium });
    TilePickaxeMin.AssignValueToKeys<ushort>(110, stackalloc ushort[] { TileID.Mythril, TileID.Orichalcum });
    TilePickaxeMin.AssignValueToKeys<ushort>(150, stackalloc ushort[] { TileID.Adamantite, TileID.Titanium });
    TilePickaxeMin.AssignValueToKeys<ushort>(200, stackalloc ushort[] { TileID.Chlorophyte });
    TilePickaxeMin.AssignValueToKeys<ushort>(210, stackalloc ushort[] { TileID.LihzahrdBrick, TileID.LihzahrdAltar });

    for (int i = TileID.Count; i < TileLoader.TileCount; i++) {
      ModTile modTile = TileLoader.GetTile(i);
      TilePickaxeMin[i] = (ushort)modTile.MinPick;
    }
  }

  public override void Unload() {
    TilePickaxeMin = null!;
  }

  /// <summary>
  /// Array of pickaxe power for a given <see cref="Terraria.Tile"/>, where the index corresponds to a <see cref="Terraria.Tile.type"/>
  /// </summary>
  public static ushort[] TilePickaxeMin { get; private set; } = null!; // SetStaticDefaults()
}
