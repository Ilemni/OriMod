using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  internal static class TileCollection {
    internal static Dictionary<ushort, int> TilePickaxeMin = new Dictionary<ushort, int>();
    private static void AddMultiKey(this Dictionary<ushort, int> d, int v, List<ushort> keys)
      => keys.ForEach(k => d.Add(k, v));
    internal static void AddModTile(ModTile m) {
      OriPlayer.Debug($"Adding to TilePickaxeMin <{m?.Type ?? -1}, {m?.minPick ?? -1}> (Name: {m?.Name ?? "Unknown name"}, Mod {m?.mod?.Name ?? "Unknown Mod"})", Main.LocalPlayer.GetModPlayer<OriPlayer>());
      TilePickaxeMin.Add(m.Type, m.minPick);
    }
    internal static void Init() {
      if (TilePickaxeMin.Count != 0) return; // Prevent multiple Inits that causes crashing or massive lag
      ErrorLogger.Log("Initializing TileCollection...");
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
        if (Main.tileSolid[i] && !TilePickaxeMin.ContainsKey(t)) {
          TilePickaxeMin.Add(t, 0);
          ErrorLogger.Log($"Added unspecified tile {t}");
        }
      }
    }
  }
}