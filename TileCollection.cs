using OriMod.Utilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// Contains the Pickaxe power of each tile. Used for <see cref="Abilities.Burrow"/>.
  /// </summary>
  internal class TileCollection : SingleInstance<TileCollection> {
    private TileCollection() {
      tilePickaxeMin = new ushort[TileLoader.TileCount];

      // Assign vanilla tiles to 2 (Sand-like is 0, Dirt-like is 1)
      int i;
      for (i = 0; i < TileID.Count; i++) {
        tilePickaxeMin[i] = 2;
      }

      tilePickaxeMin.AssignValueToKeys<ushort>(0, TileID.Sand, TileID.Slush, TileID.Silt);
      tilePickaxeMin.AssignValueToKeys<ushort>(1,
        TileID.Dirt, TileID.Mud, TileID.ClayBlock, TileID.SnowBlock,
        TileID.Grass, TileID.CorruptGrass, TileID.FleshGrass, TileID.HallowedGrass, TileID.JungleGrass, TileID.MushroomGrass
      );
      tilePickaxeMin.AssignValueToKeys<ushort>(50, TileID.Meteorite);
      tilePickaxeMin.AssignValueToKeys<ushort>(55, TileID.Demonite, TileID.Crimtane);
      tilePickaxeMin.AssignValueToKeys<ushort>(65,
        TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, TileID.Hellstone, TileID.Obsidian, TileID.DesertFossil,
        TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick
      );
      tilePickaxeMin.AssignValueToKeys<ushort>(100, TileID.Cobalt, TileID.Palladium);
      tilePickaxeMin.AssignValueToKeys<ushort>(110, TileID.Mythril, TileID.Orichalcum);
      tilePickaxeMin.AssignValueToKeys<ushort>(150, TileID.Adamantite, TileID.Titanium);
      tilePickaxeMin.AssignValueToKeys<ushort>(200, TileID.Chlorophyte);
      tilePickaxeMin.AssignValueToKeys<ushort>(210, TileID.LihzahrdBrick, TileID.LihzahrdAltar);

      for (i = TileID.Count; i < TileLoader.TileCount; i++) {
        ModTile modTile = TileLoader.GetTile(i);
        tilePickaxeMin[i] = (ushort)modTile.minPick;
      }
    }

    /// <summary>
    /// Array of pickaxe power for a given <see cref="Terraria.Tile"/>, where the index corresponds to a <see cref="Terraria.Tile.type"/>
    /// </summary>
    internal readonly ushort[] tilePickaxeMin;
  }
}
