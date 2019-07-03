using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  internal static class TileCollection {
    internal static int[] TilePickaxeMin;
    private static void AddMultiKey(this int[] a, int v, List<ushort> keys)
      => keys.ForEach(k => a[k] = v);
    internal static void AddModTile(ModTile m) {
      OriPlayer.Debug($"Adding to TilePickaxeMin <{m?.Type ?? -1}, {m?.minPick ?? -1}> (Name: {m?.Name ?? "Unknown name"}, Mod {m?.mod?.Name ?? "Unknown Mod"})", Main.LocalPlayer.GetModPlayer<OriPlayer>());
      TilePickaxeMin[m.Type] = m.minPick;
    }
    internal static void Init() {
      if (TilePickaxeMin != null) return;
      ErrorLogger.Log("Initializing TileCollection...");
      
      TilePickaxeMin = new int[ushort.MaxValue];
      for (int i = 0; i < TilePickaxeMin.Length; i++) {
        TilePickaxeMin[i] = -1;
      }
      TilePickaxeMin.AddMultiKey(0, new List<ushort> {
        TileID.Sand, TileID.Slush, TileID.Silt,
      });
      TilePickaxeMin.AddMultiKey(1, new List<ushort> {
        TileID.Dirt, TileID.Mud, TileID.ClayBlock, TileID.SnowBlock,
        TileID.Grass, TileID.CorruptGrass, TileID.FleshGrass, TileID.HallowedGrass, TileID.JungleGrass, TileID.MushroomGrass
      });
      TilePickaxeMin.AddMultiKey(50, new List<ushort> {
        TileID.Meteorite
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 50");
      TilePickaxeMin.AddMultiKey(55, new List<ushort> {
        TileID.Demonite, TileID.Crimtane
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 55");
      TilePickaxeMin.AddMultiKey(65, new List<ushort> {
        TileID.Ebonstone, TileID.Crimstone, TileID.Pearlstone, TileID.Hellstone, TileID.Obsidian, TileID.DesertFossil,
        TileID.BlueDungeonBrick, TileID.GreenDungeonBrick, TileID.PinkDungeonBrick,
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 65");
      TilePickaxeMin.AddMultiKey(100, new List<ushort> {
        TileID.Cobalt, TileID.Palladium,
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 100");
      TilePickaxeMin.AddMultiKey(110, new List<ushort> {
        TileID.Mythril, TileID.Orichalcum,
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 110");
      TilePickaxeMin.AddMultiKey(150, new List<ushort> {
        TileID.Adamantite, TileID.Titanium,
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 150");
      TilePickaxeMin.AddMultiKey(200, new List<ushort> {
        TileID.Chlorophyte,
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 200");
      TilePickaxeMin.AddMultiKey(210, new List<ushort> {
        TileID.LihzahrdBrick, TileID.LihzahrdAltar
      });
      ErrorLogger.Log("Added tiles for Pickaxe power 210");

      for (int i = 0; i < TileID.Count; i++) {
        ushort t = (ushort)i;
        if (Main.tileSolid[i] && TilePickaxeMin[t] == -1) {
          TilePickaxeMin[t] = 2;
        }
      }
    }
  }
}