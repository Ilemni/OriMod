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
[Label($"{ConfigPath}.DisplayName")]
public class OriConfigClient1 : ModConfig {
  private const string ConfigPath = $"$Mods.{nameof(OriMod)}.Configs.{nameof(OriConfigClient1)}";
  private const string HeaderPath = $"{ConfigPath}.Headers";

  public override ConfigScope Mode => ConfigScope.ClientSide;

  [Header($"{HeaderPath}.Gameplay")]

  [LocalizedLabel(nameof(playerLight))]
  [LocalizedTooltip(nameof(playerLight))]
  [DefaultValue(true)]
  public bool playerLight;

  [LocalizedLabel(nameof(globalPlayerLight))]
  [LocalizedTooltip(nameof(globalPlayerLight))]
  [DefaultValue(false)]
  public bool globalPlayerLight;
  
  [Header($"{HeaderPath}.Controls")]

  [LocalizedLabel(nameof(stompHoldDownDelay))]
  [LocalizedTooltip(nameof(stompHoldDownDelay))]
  [DefaultValue(0)]
  public float stompHoldDownDelay;

  [LocalizedLabel(nameof(softCrouch))]
  [LocalizedTooltip(nameof(softCrouch))]
  [DefaultValue(false)]
  public bool softCrouch;

  [LocalizedLabel(nameof(smoothCamera))]
  [LocalizedTooltip(nameof(smoothCamera))]
  [DefaultValue(true)]
  public bool smoothCamera;

  [JsonIgnore]
  internal bool BurrowToMouse => burrowControls == "Mouse";

  [LocalizedLabel(nameof(burrowControls))]
  [LocalizedTooltip(nameof(burrowControls))]
  [DefaultValue("Mouse"), OptionStrings(new[] { "WASD", "Mouse" })]
  public string burrowControls;

  [LocalizedLabel(nameof(bashMode))]
  [LocalizedTooltip(nameof(bashMode))]
  [DefaultValue("Target"), OptionStrings(new[] { "Target", "Player" })]
  public string bashMode;

  [LocalizedLabel(nameof(blockControlsInMenu))]
  [LocalizedTooltip(nameof(blockControlsInMenu))]
  [DefaultValue("false")]
  public bool blockControlsInMenu;

  [Header($"{HeaderPath}.Aesthetics")]

  [LocalizedLabel(nameof(playerColor))]
  [LocalizedTooltip(nameof(playerColor))]
  [DefaultValue(typeof(Color), "210, 255, 255, 255"), ColorNoAlpha]
  public Color playerColor;

  [LocalizedLabel(nameof(playerColorSecondary))]
  [LocalizedTooltip(nameof(playerColorSecondary))]
  [DefaultValue(typeof(Color), "0, 0, 0, 0")]
  public Color playerColorSecondary;

  [LocalizedLabel(nameof(dyeEnabled))]
  [LocalizedTooltip(nameof(dyeEnabled))]
  [DefaultValue(typeof(bool), "true")]
  public bool dyeEnabled;

  [LocalizedLabel(nameof(dyeEnabledAll))]
  [LocalizedTooltip(nameof(dyeEnabledAll))]
  [DefaultValue(typeof(bool), "true")]
  public bool dyeEnabledAll;

  [LocalizedLabel(nameof(dyeLerp))]
  [LocalizedTooltip(nameof(dyeLerp))]
  [DefaultValue(typeof(float), "0.65")]
  public float dyeLerp;

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

  // Shorthand [LocalizedLabel("MyKey")] vs [Label("$Mods.OriMod.Configs.ConfigClient1.MyKey.Label")]
  // When compiling to 1.4.4, all attributes should be removed.
  class LocalizedLabelAttribute : LabelAttribute {
    public LocalizedLabelAttribute(string labelKey) : base($"{ConfigPath}.{labelKey}.Label") { }
  }
  class LocalizedTooltipAttribute : TooltipAttribute {
    public LocalizedTooltipAttribute(string tooltipKey) : base($"{ConfigPath}.{tooltipKey}.Tooltip") { }
  }
}
