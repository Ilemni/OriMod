using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.IO;

namespace OriMod {
  public static class Config {
    internal const string ObsoleteMsg = "This member remains in use for tModLoader version 0.10.1.5. In the tML v0.11.2.2 release, remove this class.";
    [System.Obsolete(Config.ObsoleteMsg, false)]
    public static string ConfigPath = Path.Combine(Main.SavePath, "Mod Configs", "OriMod.json");
    [System.Obsolete(Config.ObsoleteMsg, false)]
    public static bool BurrowToMouse { get { return OriMod.ConfigClient.BurrowToMouse; } set { OriMod.ConfigClient.BurrowControls = value ? "Mouse" : "WASD"; } }
    [System.Obsolete(Config.ObsoleteMsg, false)]
    private static Preferences Prefs = new Preferences(ConfigPath);
    [System.Obsolete(Config.ObsoleteMsg, false)]
    public static void Load() {
      bool success = ReadConfig();
      if (!success) {
        // OriMod.Log.Error("Could not load configs for OriMod, creating configs.");
        SaveConfig();
      }
    }
    private static void LoadColor(string s, out Color color, bool alpha=false) {
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
      color = new Color(r, g, b, a);
    }
    private static void GetColor(this Preferences p, string name, ref Color defaultValue, bool alpha=false) {
      string s = p.Get(name, defaultValue.ToString());
      LoadColor(s, out defaultValue, alpha);
    }
    [System.Obsolete(Config.ObsoleteMsg, false)]
    public static bool ReadConfig() {
      if (Prefs.Load()) {
        bool b = false;
        Prefs.Get("GlobalPlayerLight", ref OriMod.ConfigClient.GlobalPlayerLight);
        Prefs.Get("DoPlayerLight", ref OriMod.ConfigClient.PlayerLight);
        Prefs.Get("SmoothCamera", ref OriMod.ConfigClient.SmoothCamera);
        Prefs.Get("SoftCrouch", ref OriMod.ConfigClient.SoftCrouch);
        Prefs.Get("AbilityCooldowns", ref OriMod.ConfigClient.AbilityCooldowns);
        Prefs.Get("BurrowToMouse", ref b);
        BurrowToMouse = b;
        Prefs.Get("BurrowStrength", ref OriMod.ConfigAbilities.BurrowStrength);
        Prefs.Get("StompHoldDownDelay", ref OriMod.ConfigClient.StompHoldDownDelay);
        Prefs.GetColor("OriColor", ref OriMod.ConfigClient.PlayerColor);
        Prefs.GetColor("OriColorSecondary", ref OriMod.ConfigClient.PlayerColorSecondary);
        return true;
      }
      return false;
    }
    [System.Obsolete(Config.ObsoleteMsg, false)]
    public static void SaveConfig() {
      Prefs.Clear();
      Prefs.Put("GlobalPlayerLight", OriMod.ConfigClient.GlobalPlayerLight);
      Prefs.Put("DoPlayerLight", OriMod.ConfigClient.PlayerLight);
      Prefs.Put("SmoothCamera", OriMod.ConfigClient.SmoothCamera);
      Prefs.Put("OriColor", OriMod.ConfigClient.PlayerColor);
      Prefs.Put("OriColorSecondary", OriMod.ConfigClient.PlayerColorSecondary);
      Prefs.Put("SoftCrouch", OriMod.ConfigClient.SoftCrouch);
      Prefs.Put("AbilityCooldowns", OriMod.ConfigClient.AbilityCooldowns);
      Prefs.Put("BurrowToMouse", OriMod.ConfigClient.BurrowToMouse);
      Prefs.Put("AutoBurrow", OriMod.ConfigClient.AutoBurrow);
      Prefs.Put("BurrowStrength", OriMod.ConfigAbilities.BurrowStrength);
      Prefs.Put("StompHoldDownDelay", OriMod.ConfigClient.StompHoldDownDelay);
      Prefs.Save();
    }
    [System.Obsolete(Config.ObsoleteMsg, false)]
    public static void ResetConfig() {
      Prefs.Clear();
      Prefs.Put("GlobalPlayerLight", false);
      Prefs.Put("DoPlayerLight", true);
      Prefs.Put("SmoothCamera", true);
      Prefs.Put("OriColor", Color.LightCyan);
      Prefs.Put("OriColorSecondary", Color.LightCyan);
      Prefs.Put("SoftCrouch", true);
      Prefs.Put("AbilityCooldowns", true);
      Prefs.Put("BurrowToMouse", false);
      Prefs.Put("AutoBurrow", false);
      Prefs.Put("BurrowStrength", 0);
      Prefs.Put("StompHoldDownDelay", 0);
      Prefs.Save();
    }
  }
} 