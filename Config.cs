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
    public static bool AbilityCooldowns = true;
    public static bool SmoothCamera = true;
    public static bool BurrowToMouse = false;
    public static bool AutoBurrow = false;
    public static bool RestrictiveCrouch = true;
    public static int BurrowTier = 0;
    public static int StompHoldDownDelay = 0;
    public static Color OriColor = Color.LightCyan;
    public static Color OriColorSecondary = Color.LightCyan;
    private static Preferences Prefs = new Preferences(ConfigPath);
    public static void Load() {
      bool success = ReadConfig();
      if (!success) {
        ErrorLogger.Log("Could not load configs for OriMod, creating configs.");
        SaveConfig();
      }
    }
    public static void LoadColor(string s, ref Color color, bool alpha=false) {
      string[] arr = s.Split(new char[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
      if (arr.Length == 0) {
        color = Color.LightCyan;
        return;
      }
      byte r = 255;
      byte g = 255;
      byte b = 255;
      byte a = 255;
      byte.TryParse(arr[0], out r);
      byte.TryParse(arr[1], out g);
      byte.TryParse(arr[2], out b);
      if (alpha) byte.TryParse(arr[3], out a);
      color = new Color(r, g, b, a);
    }
    public static bool ReadConfig() {
      if (Prefs.Load()) {
        Prefs.Get("GlobalPlayerLight", ref GlobalPlayerLight);
        Prefs.Get("DoPlayerLight", ref DoPlayerLight);
        Prefs.Get("SmoothCamera", ref SmoothCamera);
        string s = "";
        Prefs.Get("OriColor", ref s);
        LoadColor(s, ref OriColor);
        s = "";
        Prefs.Get("OriColorSecondary", ref s);
        LoadColor(s, ref OriColorSecondary, true);
        Prefs.Get("RestrictiveCrouch", ref RestrictiveCrouch);
        Prefs.Get("AbilityCooldowns", ref AbilityCooldowns);
        Prefs.Get("BurrowToMouse", ref BurrowToMouse);
        Prefs.Get("AutoBurrow", ref AutoBurrow);
        int temp = BurrowTier;
        Prefs.Get("BurrowTier", ref temp);
        Prefs.Get("StompHoldDownDelay", ref StompHoldDownDelay);
        BurrowTier = temp;
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
      Prefs.Put("OriColorSecondary", OriColorSecondary);
      Prefs.Put("RestrictiveCrouch", RestrictiveCrouch);
      Prefs.Put("AbilityCooldowns", AbilityCooldowns);
      Prefs.Put("BurrowToMouse", BurrowToMouse);
      Prefs.Put("AutoBurrow", AutoBurrow);
      Prefs.Put("BurrowTier", BurrowTier);
      Prefs.Put("StompHoldDownDelay", StompHoldDownDelay);
      Prefs.Save();
    }
    public static void ResetConfig() {
      Prefs.Clear();
      Prefs.Put("GlobalPlayerLight", false);
      Prefs.Put("DoPlayerLight", true);
      Prefs.Put("SmoothCamera", true);
      Prefs.Put("OriColor", Color.LightCyan);
      Prefs.Put("OriColorSecondary", Color.LightCyan);
      Prefs.Put("RestrictiveCrouch", true);
      Prefs.Put("AbilityCooldowns", true);
      Prefs.Put("BurrowToMouse", false);
      Prefs.Put("AutoBurrow", false);
      Prefs.Put("BurrowTier", 0);
      Prefs.Put("StompHoldDownDelay", 0);
      Prefs.Save();
    }
  }
}