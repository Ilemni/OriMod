using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using log4net;
using Microsoft.Xna.Framework;
using OriMod.Networking;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.KeybindLoader;
using static Terraria.ModLoader.EquipLoader;

namespace OriMod;

/// <summary>
/// The mod of this assembly.
/// </summary>
[UsedImplicitly]
public sealed partial class OriMod : Mod {
  public OriMod() {
    ContentAutoloadingEnabled = true;
    GoreAutoloadingEnabled = true;
    MusicAutoloadingEnabled = true;
  }

  /// <summary>
  /// Singleton instance of this mod.
  /// </summary>
  public static OriMod Instance => ContentInstance<OriMod>.Instance;

  /// <summary>
  /// <inheritdoc cref="OriConfigClient1"/>
  /// </summary>
  public static OriConfigClient1 ConfigClient { get; internal set; } = null!; // ModConfig.OnLoaded()

  /// <summary>
  /// GitHub profile that the mod's repository is stored on.
  /// </summary>
  [UsedImplicitly]
  public static string GithubUserName => "Ilemni";

  /// <summary>
  /// Name of the GitHub repository this mod is stored on.
  /// </summary>
  [UsedImplicitly]
  public static string GithubProjectName => "OriMod";

  #region Logging Shortcuts

  internal static ILog Log => Instance.Logger;

  /// <summary> Shows an error in chat and in the logger, using default localized text. Has formatting.</summary>
  /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
  /// <param name="log">Whether to write to logger.</param>
  /// <param name="args">Formatting args.</param>
  internal static void Error(string key, bool log = true, params object[] args) {
    string text = Language.GetText($"Mods.OriMod.Error.{key}").Format(args);
    if (log) {
      Log.Error(text);
    }

    Main.NewText(text, Color.Red);
  }

  #endregion

  /// <summary>
  /// Key used for controlling <see cref="Abilities.Bash"/>.
  /// </summary>
  public static ModKeybind BashKey { get; private set; } = null!; // Load()

  /// <summary>
  /// Key used for activating <see cref="Abilities.Dash"/> and <see cref="Abilities.ChargeDash"/>.
  /// </summary>
  public static ModKeybind DashKey { get; private set; } = null!; // Load()

  /// <summary>
  /// Key used for controlling <see cref="Abilities.Climb"/>.
  /// </summary>
  public static ModKeybind ClimbKey { get; private set; } = null!; // Load()

  /// <summary>
  /// Key used for controlling <see cref="Abilities.Glide"/>.
  /// </summary>
  public static ModKeybind FeatherKey { get; private set; } = null!; // Load()

  /// <summary>
  /// Key used for the charging of <see cref="Abilities.ChargeDash"/> and <see cref="Abilities.ChargeJump"/>.
  /// </summary>
  public static ModKeybind ChargeKey { get; private set; } = null!; // Load()

  /// <summary>
  /// Key used for activating <see cref="Abilities.Burrow"/>.
  /// </summary>
  public static ModKeybind BurrowKey { get; private set; } = null!; // Load()

  /// <summary>
  /// Key used for activating <see cref="Abilities.Stomp"/>.
  /// </summary>
  public static ModKeybind StompKey { get; private set; } = null!; // Load()

  public override void Load() {
    BashKey = RegisterKeybind(Instance, "Bash", "Mouse2");
    DashKey = RegisterKeybind(Instance, "Dash", "LeftControl");
    ClimbKey = RegisterKeybind(Instance, "Climbing", "LeftShift");
    FeatherKey = RegisterKeybind(Instance, "Feather", "LeftShift");
    ChargeKey = RegisterKeybind(Instance, "Charge", "W");
    BurrowKey = RegisterKeybind(Instance, "Burrow", "LeftControl");
    StompKey = RegisterKeybind(Instance, "Stomp", "S");
    if (!Main.dedServ) {
      AddEquipTexture(Instance, "OriMod/PlayerEffects/OriHead", EquipType.Head, null, "OriHead",
        GetEquipTexture(Instance, "OriHead", EquipType.Head));
    }
  }

  public override void Unload() {
    SoundInfo.Unload();

    BashKey = null!;
    DashKey = null!;
    ClimbKey = null!;
    FeatherKey = null!;
    ChargeKey = null!;
    BurrowKey = null!;
    StompKey = null!;
    ConfigClient = null!;
  }

  public override void HandlePacket(BinaryReader reader, int fromWho) => ModNetHandler.HandlePacket(reader, fromWho);

  [Conditional("DEBUG")]
  public static void Debug(string value) {
    Log.Debug(value);
    Main.NewText(value);
  }
}
