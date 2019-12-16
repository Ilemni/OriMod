using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary> This class is used purely to handle Burrowing.
  /// 
  /// This class assigns and stores the Pickaxe power of each tile, with some exceptions. </summary>
  internal static class TileCollection {
    internal static int[] TilePickaxeMin;

    /// <summary> Assigns multiple indexes of an array to the same value</summary>
    /// <param name="value">The value to assign to</param>
    /// <param name="keys"></param>
    private static void AddMultiKey(this int[] arr, int value, params ushort[] keys) {
      for (int i = 0, len = keys.Length; i < len; i++) {
        arr[keys[i]] = value;
      }
    }

    /// <summary> Add's a ModTile's minPick to the TilePickaxeMin list </summary>
    internal static void AddModTile(ModTile m) {
      OriMod.Log.Debug($"Adding to TilePickaxeMin <{m.Type}, {m.minPick}> (Name: {m.Name ?? "Unknown name"}, Mod {m.mod?.Name ?? "Unknown mod"})");
      TilePickaxeMin[m.Type] = m.minPick;
    }

    /// <summary> Only call in an initlalize class. </summary>
    internal static void Init() {
      if (TilePickaxeMin != null) {
        return;
      }

      TilePickaxeMin = new int[ushort.MaxValue];

      // Assign vanilla tiles to 2 (Sand-like is 0, Dirt-like is 1)
      for (int i = 0; i < TileID.Count; i++) {
        TilePickaxeMin[i] = 2;
      }
      // Assign ModTiles to -1 until they're assigned.
      for (int i = TileID.Count, len = TilePickaxeMin.Length; i < len; i++) {
        TilePickaxeMin[i] = -1;
      }

      TilePickaxeMin.AddMultiKey(0, TileID.Sand, TileID.Slush, TileID.Silt);
      TilePickaxeMin.AddMultiKey(1,
        TileID.Dirt, TileID.Mud, TileID.ClayBlock, TileID.SnowBlock,
        TileID.Grass, TileID.CorruptGrass, TileID.FleshGrass, TileID.HallowedGrass, TileID.JungleGrass, TileID.MushroomGrass
      );
      TilePickaxeMin.AddMultiKey(50, TileID.Meteorite);
      TilePickaxeMin.AddMultiKey(55, TileID.Demonite, TileID.Crimtane);
      TilePickaxeMin.AddMultiKey(65,
        TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, TileID.Hellstone, TileID.Obsidian, TileID.DesertFossil,
        TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick
      );
      TilePickaxeMin.AddMultiKey(100, TileID.Cobalt, TileID.Palladium);
      TilePickaxeMin.AddMultiKey(110, TileID.Mythril, TileID.Orichalcum);
      TilePickaxeMin.AddMultiKey(150, TileID.Adamantite, TileID.Titanium);
      TilePickaxeMin.AddMultiKey(200, TileID.Chlorophyte);
      TilePickaxeMin.AddMultiKey(210, TileID.LihzahrdBrick, TileID.LihzahrdAltar);
    }
  }
}
