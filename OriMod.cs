using System.IO;
using log4net;
using Microsoft.Xna.Framework;
using OriMod.Networking;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.KeybindLoader;
using static Terraria.ModLoader.EquipLoader;

namespace OriMod;

/// <summary>
/// The mod of this assembly.
/// </summary>
public sealed class OriMod : Mod {
  public OriMod() {
    ContentAutoloadingEnabled = true;
    GoreAutoloadingEnabled = true;
    MusicAutoloadingEnabled = true;
    instance = this;
  }

  /// <summary>
  /// Singleton instance of this mod.
  /// </summary>
  public static OriMod instance;

  /// <summary>
  /// <inheritdoc cref="OriConfigClient1"/>
  /// </summary>
  public static OriConfigClient1 ConfigClient { get; internal set; }

  /// <summary>
  /// GitHub profile that the mod's repository is stored on.
  /// </summary>
  public static string GithubUserName => "TwiliChaos";

  /// <summary>
  /// Name of the GitHub repository this mod is stored on.
  /// </summary>
  public static string GithubProjectName => "OriMod";

  #region Logging Shortcuts

  internal static ILog Log => instance.Logger;

  /// <summary>
  /// Gets localized text with key <c>Mods.OriMod.<paramref name="key"/></c>.
  /// </summary>
  /// <param name="key">Key in lang file.</param>
  internal static LocalizedText GetText(string key) => Language.GetText($"Mods.OriMod.{key}");

  /// <summary>
  /// Gets localized text with key <c>Mods.OriMod.Error.<paramref name="key"/></c>.
  /// </summary>
  /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
  private static LocalizedText GetErrorText(string key) => Language.GetText($"Mods.OriMod.Error.{key}");

  /// <summary>
  /// Shows an error in chat and in the logger, with key <c>Mods.OriMod.Error.<paramref name="key"/></c>.
  /// </summary>
  /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
  /// <param name="log">Whether or not to write to logger.</param>
  internal static void Error(string key, bool log = true) => PrintError(GetErrorText(key).Value, log);

  /// <summary> Shows an error in chat and in the logger, using default localized text. Has formatting.</summary>
  /// <param name="key">Key in lang file, that would start with <c>Error.</c></param>
  /// <param name="log">Whether or not to write to logger.</param>
  /// <param name="args">Formatting args.</param>
  internal static void Error(string key, bool log = true, params object[] args) =>
    PrintError(GetErrorText(key).Format(args), log);

  /// <summary> Shows an error in chat and in the logger, using a string literal.</summary>
  /// <param name="text">String literal to print.</param>
  /// <param name="log">Whether or not to write to logger.</param>
  private static void PrintError(string text, bool log = true) {
    if (log) {
      Log.Error(text);
    }

    Main.NewText(text, Color.Red);
  }

  #endregion

  /// <summary>
  /// Key used for controlling <see cref="Abilities.Bash"/>.
  /// </summary>
  public static ModKeybind bashKey;

  /// <summary>
  /// Key used for activating <see cref="Abilities.Dash"/> and <see cref="Abilities.ChargeDash"/>.
  /// </summary>
  public static ModKeybind dashKey;

  /// <summary>
  /// Key used for controlling <see cref="Abilities.Climb"/>.
  /// </summary>
  public static ModKeybind climbKey;

  /// <summary>
  /// Key used for controlling <see cref="Abilities.Glide"/>.
  /// </summary>
  public static ModKeybind featherKey;

  /// <summary>
  /// Key used for the charging of <see cref="Abilities.ChargeDash"/> and <see cref="Abilities.ChargeJump"/>.
  /// </summary>
  public static ModKeybind chargeKey;

  /// <summary>
  /// Key used for activating <see cref="Abilities.Burrow"/>.
  /// </summary>
  public static ModKeybind burrowKey;

    /// <summary>
  /// Key used for activating <see cref="Abilities.Stomp"/>.
  /// </summary>
  public static ModKeybind stompKey;

  public override void Load() {
    //SoulLinkKey = RegisterKeybind(instance, "SoulLink", "E");
    bashKey = RegisterKeybind(instance, "Bash", "Mouse2");
    dashKey = RegisterKeybind(instance, "Dash", "LeftControl");
    climbKey = RegisterKeybind(instance, "Climbing", "LeftShift");
    featherKey = RegisterKeybind(instance, "Feather", "LeftShift");
    chargeKey = RegisterKeybind(instance, "Charge", "W");
    burrowKey = RegisterKeybind(instance, "Burrow", "LeftControl");
    stompKey = RegisterKeybind(instance, "Stomp", "S");
    if (!Main.dedServ) {
      AddEquipTexture(instance, "OriMod/PlayerEffects/OriHead", EquipType.Head, null, "OriHead",
        GetEquipTexture(instance, "OriHead", EquipType.Head));
    }

    SeinData.Load();
  }

  public override void PostSetupContent() {
    FootstepManager.Initialize();
    TileCollection.Initialize();
    if(!Main.dedServ) OriTextures.Initialize();
  }

  public override void Unload() {
    Unloadable.Unload();
    instance = null;

    bashKey = null;
    dashKey = null;
    climbKey = null;
    featherKey = null;
    chargeKey = null;
    burrowKey = null;
    stompKey = null;
    //SoulLinkKey = null;
    ConfigClient = null;
  }

  public override void HandlePacket(BinaryReader reader, int fromWho) {
    if (Main.netMode == NetmodeID.MultiplayerClient) {
      // If packet is sent TO server, it is FROM player.
      // If packet is sent TO player, it is FROM server (This block) and fromWho is 255.
      // Server-written packet includes the fromWho, the player that created it.
      // Now in either case of this being server or player, the fromWho is the player.
      fromWho = reader.ReadUInt16();
    }

    ModNetHandler.Instance.HandlePacket(reader, fromWho);
  }

  public override object Call(params object[] args) => OriModCall.Call(args);
}
