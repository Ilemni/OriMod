using Terraria.ID;
using Terraria.ModLoader;
using OriMod.Utilities;

namespace OriMod {
  /// <summary>
  /// Contains the Pickaxe power of each tile. Used for Burrowing.
  /// </summary>
  internal class TileCollection : SingleInstance<TileCollection> {
    private TileCollection() {
      TilePickaxeMin = new ushort[TileLoader.TileCount];

      // Assign vanilla tiles to 2 (Sand-like is 0, Dirt-like is 1)
      int i;
      for (i = 0; i < TileID.Count; i++) {
        TilePickaxeMin[i] = 2;
      }

      TilePickaxeMin.AssignValueToKeys<ushort>(0, TileID.Sand, TileID.Slush, TileID.Silt);
      TilePickaxeMin.AssignValueToKeys<ushort>(1,
        TileID.Dirt, TileID.Mud, TileID.ClayBlock, TileID.SnowBlock,
        TileID.Grass, TileID.CorruptGrass, TileID.FleshGrass, TileID.HallowedGrass, TileID.JungleGrass, TileID.MushroomGrass
      );
      TilePickaxeMin.AssignValueToKeys<ushort>(50, TileID.Meteorite);
      TilePickaxeMin.AssignValueToKeys<ushort>(55, TileID.Demonite, TileID.Crimtane);
      TilePickaxeMin.AssignValueToKeys<ushort>(65,
        TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, TileID.Hellstone, TileID.Obsidian, TileID.DesertFossil,
        TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick
      );
      TilePickaxeMin.AssignValueToKeys<ushort>(100, TileID.Cobalt, TileID.Palladium);
      TilePickaxeMin.AssignValueToKeys<ushort>(110, TileID.Mythril, TileID.Orichalcum);
      TilePickaxeMin.AssignValueToKeys<ushort>(150, TileID.Adamantite, TileID.Titanium);
      TilePickaxeMin.AssignValueToKeys<ushort>(200, TileID.Chlorophyte);
      TilePickaxeMin.AssignValueToKeys<ushort>(210, TileID.LihzahrdBrick, TileID.LihzahrdAltar);

      for (i = TileID.Count; i < TileLoader.TileCount; i++) {
        var modTile = TileLoader.GetTile(i);
        TilePickaxeMin[i] = (ushort)modTile.minPick;
      }
    }

    internal readonly ushort[] TilePickaxeMin;
  }
}
