using OriMod.Utilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary> This class is used purely to handle Burrowing.
  /// 
  /// This class assigns and stores the Pickaxe power of each tile, with some exceptions. </summary>
  internal class TileCollection : SingleInstance<TileCollection> {
    private TileCollection() {
      if (TilePickaxeMin != null) {
        return;
      }

      TilePickaxeMin = new int[TileLoader.TileCount];

      // Assign vanilla tiles to 2 (Sand-like is 0, Dirt-like is 1)
      int i;
      for (i = 0; i < TileID.Count; i++) {
        TilePickaxeMin[i] = 2;
      }
      // Assign ModTiles to -1 until they're assigned.
      for (i = TileID.Count; i < TilePickaxeMin.Length; i++) {
        TilePickaxeMin[i] = -1;
      }

      TilePickaxeMin.AssignValueToKeys(0, TileID.Sand, TileID.Slush, TileID.Silt);
      TilePickaxeMin.AssignValueToKeys(1,
        TileID.Dirt, TileID.Mud, TileID.ClayBlock, TileID.SnowBlock,
        TileID.Grass, TileID.CorruptGrass, TileID.FleshGrass, TileID.HallowedGrass, TileID.JungleGrass, TileID.MushroomGrass
      );
      TilePickaxeMin.AssignValueToKeys(50, TileID.Meteorite);
      TilePickaxeMin.AssignValueToKeys(55, TileID.Demonite, TileID.Crimtane);
      TilePickaxeMin.AssignValueToKeys(65,
        TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, TileID.Hellstone, TileID.Obsidian, TileID.DesertFossil,
        TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick
      );
      TilePickaxeMin.AssignValueToKeys(100, TileID.Cobalt, TileID.Palladium);
      TilePickaxeMin.AssignValueToKeys(110, TileID.Mythril, TileID.Orichalcum);
      TilePickaxeMin.AssignValueToKeys(150, TileID.Adamantite, TileID.Titanium);
      TilePickaxeMin.AssignValueToKeys(200, TileID.Chlorophyte);
      TilePickaxeMin.AssignValueToKeys(210, TileID.LihzahrdBrick, TileID.LihzahrdAltar);

      for (i = TileID.Count; i < TileLoader.TileCount; i++) {
        var modTile = TileLoader.GetTile(i);
        TilePickaxeMin[i] = modTile.minPick;
      }
    }

    internal readonly int[] TilePickaxeMin;

    /// <summary>
    /// Add's a ModTile's minPick to the TilePickaxeMin list
    /// </summary>
    internal void AddModTile(ModTile m) {
      OriMod.Log.Debug($"Adding to TilePickaxeMin <{m.Type}, {m.minPick}> (Name: {m.Name}, Mod {m.mod?.Name ?? "Unknown mod"})");
      TilePickaxeMin[m.Type] = m.minPick;
    }
  }
}
