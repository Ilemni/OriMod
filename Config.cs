using System.IO;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;

namespace OriMod {
  public static class Config {
    public static string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs", "OriMod.json");
    public static bool GlobalPlayerLight = true; // If true, sets all players on this side to `DoPlayerLight`
    public static bool DoPlayerLight = true; // If the player should emit light
    public static bool BlindForestMovement = true; // TODO: Movement preset, Blind Forest style vs balanced for Terraria
    private static Preferences Prefs = new Preferences(ConfigPath);
    public static void Load() {
      bool success = ReadConfig();
      if (!success) {
        ErrorLogger.Log("Could not load configs for OriMod, creating configs.");
        CreateConfig();
      }
    }
    public static bool ReadConfig() {
      if (Prefs.Load()) {
        Prefs.Get("GlobalPlayerLight", ref GlobalPlayerLight);
        Prefs.Get("DoPlayerLight", ref DoPlayerLight);
        // Prefs.Get("BlindForestMovement", ref BlindForestMovement);
        return true;
      }
      return false;
    }
    public static void CreateConfig() {
      Prefs.Clear();
      Prefs.Put("GlobalPlayerLight", GlobalPlayerLight);
      Prefs.Put("DoPlayerLight", DoPlayerLight);
      // Prefs.Put("BlindForestMovement", BlindForestMovement);
      Prefs.Save();
    }
  }
}