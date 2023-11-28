using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader.Config;

namespace OriMod;

/// <summary>
/// Configurations for the player's own Ori settings.
/// </summary>
public class OriConfigClient1 : ModConfig {
  private const string ConfigPath = $"$Mods.{nameof(OriMod)}.Configs.{nameof(OriConfigClient1)}";
  private const string HeaderPath = $"{ConfigPath}.Headers";

  public override ConfigScope Mode => ConfigScope.ClientSide;

  [Header($"{HeaderPath}.Gameplay")]

  [DefaultValue(true)]
  public bool playerLight;
 
  [DefaultValue(false)]
  public bool globalPlayerLight;
  
  [Header($"{HeaderPath}.Controls")]

  [DefaultValue(0)]
  public float stompHoldDownDelay;

  [DefaultValue(false)]
  public bool softCrouch;

  [DefaultValue("Default"), OptionStrings(new[] { "Default", "Not Down", "Only Up" })]
  public string airJumpCondition;

  [DefaultValue(true)]
  public bool smoothCamera;

  [JsonIgnore]
  internal bool BurrowToMouse => burrowControls == "Mouse";

  [DefaultValue("Mouse"), OptionStrings(new[] { "WASD", "Mouse" })]
  public string burrowControls;

  [DefaultValue("Target"), OptionStrings(new[] { "Target", "Player" })]
  public string bashMode;

  [DefaultValue("false")]
  public bool blockControlsInMenu;

  [Header($"{HeaderPath}.Aesthetics")]

  [DefaultValue(typeof(Color), "210, 255, 255, 255"), ColorNoAlpha]
  public Color playerColor;

  [DefaultValue(typeof(Color), "0, 0, 0, 0")]
  public Color playerColorSecondary;

  [DefaultValue(typeof(bool), "true")]
  public bool dyeEnabled;

  [DefaultValue(typeof(bool), "true")]
  public bool dyeEnabledAll;

  [DefaultValue(typeof(float), "0.65")]
  public float dyeLerp;

  [DefaultValue("Transparent"), OptionStrings(new[] { "Transparent", "Red", "Disabled" })]
  public string flashMode;

  [Header($"{HeaderPath}.Experimental")]
  [DefaultValue(typeof(bool), "false")]
  public bool eChargeDashHoming;

  public override void OnLoaded() {
    OriMod.ConfigClient = this;
  }

  [OnDeserialized]
  public void OnDeserializedMethod(StreamingContext stream) {
    playerColor.A = 255;
    if (stompHoldDownDelay < 0) {
      stompHoldDownDelay = 0;
    }
  }

  public override void OnChanged() {
    Player player = Main.LocalPlayer;
    if (!player.active) return;
    OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
    oPlayer.SpriteColorPrimary = playerColor;
    oPlayer.SpriteColorSecondary = playerColorSecondary;
    oPlayer.DyeColorBlend = dyeLerp;
  }
}
