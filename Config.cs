using Microsoft.Xna.Framework;
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
    public static bool SmoothCamera = true;
    public static bool BurrowToMouse = false;
    public static Color OriColor = Color.LightCyan;
    private static Preferences Prefs = new Preferences(ConfigPath);
    public static void Load() {
      bool success = ReadConfig();
      if (!success) {
        ErrorLogger.Log("Could not load configs for OriMod, creating configs.");
        SaveConfig();
      }
    }
    public static void LoadColor(string s) {
      string[] arr = s.Split(new char[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
      byte r = 255;
      byte g = 255;
      byte b = 255;
      byte a = 255;
      byte.TryParse(arr[0], out r);
      byte.TryParse(arr[1], out g);
      byte.TryParse(arr[2], out b);
      OriColor = new Color(r, g, b, a);
    }
    public static bool ReadConfig() {
      if (Prefs.Load()) {
        Prefs.Get("GlobalPlayerLight", ref GlobalPlayerLight);
        Prefs.Get("DoPlayerLight", ref DoPlayerLight);
        Prefs.Get("SmoothCamera", ref SmoothCamera);
        string s = "";
        Prefs.Get("OriColor", ref s);
        LoadColor(s);
        Prefs.Get("BurrowToMouse", ref BurrowToMouse);
        // Prefs.Get("BlindForestMovement", ref BlindForestMovement);
        return true;
      }
      return false;
    }
    public static void SaveConfig() {
      Prefs.Clear();
      Prefs.Put("GlobalPlayerLight", GlobalPlayerLight);
      Prefs.Put("DoPlayerLight", DoPlayerLight);
      Prefs.Put("SmoothCamera", SmoothCamera);
      Prefs.Put("OriColor", OriColor);
      Prefs.Put("BurrowToMouse", BurrowToMouse);
      // Prefs.Put("BlindForestMovement", BlindForestMovement);
      Prefs.Save();
    }
    public static void ResetConfig() {
      Prefs.Clear();
      Prefs.Put("GlobalPlayerLight", false);
      Prefs.Put("DoPlayerLight", true);
      Prefs.Put("SmoothCamera", true);
      Prefs.Put("OriColor", Color.LightCyan);
      Prefs.Put("BurrowToMouse", false);
      Prefs.Save();
    }
  }
}