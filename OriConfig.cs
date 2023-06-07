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
  [LocalizedLabel("Title")]
  public class OriConfigClient1 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("$Mods.OriMod.Config.Header.Gameplay")]
    
    [LocalizedLabel("PlayerLight")]
    [LocalizedTooltip("PlayerLight")]
    [DefaultValue(true)]
    public bool playerLight;

    [LocalizedLabel("PlayerLightGlobal")]
    [LocalizedTooltip("PlayerLightGlobal")]
    [DefaultValue(false)]
    public bool globalPlayerLight;

    [Header("$Mods.OriMod.Config.Header.Controls")]
    
    [LocalizedLabel("StompDelay")]
    [LocalizedTooltip("StompDelay")]
    [DefaultValue(0)]
    public float stompHoldDownDelay;

    [LocalizedLabel("SoftCrouch")]
    [LocalizedTooltip("SoftCrouch")]
    [DefaultValue(false)]
    public bool softCrouch;

    [LocalizedLabel("SmoothCamera")]
    [LocalizedTooltip("SmoothCamera")]
    [DefaultValue(true)]
    public bool smoothCamera;

    [JsonIgnore]
    internal bool BurrowToMouse => burrowControls == "Mouse";

    [LocalizedLabel("BurrowControls")]
    [LocalizedTooltip("BurrowControls")]
    [DefaultValue("Mouse"), OptionStrings(new[] { "WASD", "Mouse" })]
    public string burrowControls;

    [LocalizedLabel("BlockControlsInMenu")]
    [LocalizedTooltip("BlockControlsInMenu")]
    [DefaultValue("false")]
    public bool blockControlsInMenu;

    [Header("$Mods.OriMod.Config.Header.Aesthetics")]

    [LocalizedLabel("Color1")]
    [LocalizedTooltip("Color1")]
    [DefaultValue(typeof(Color), "210, 255, 255, 255"), ColorNoAlpha]
    public Color playerColor;

    [LocalizedLabel("Color2")]
    [LocalizedTooltip("Color2")]
    [DefaultValue(typeof(Color), "0, 0, 0, 0")]
    public Color playerColorSecondary;

    [LocalizedLabel("DyeEnabled")]
    [LocalizedTooltip("DyeEnabled")]
    [DefaultValue(typeof(bool), "true")]
    public bool dyeEnabled;

    [LocalizedLabel("DyeEnabledAll")]
    [LocalizedTooltip("DyeEnabledAll")]
    [DefaultValue(typeof(bool), "true")]
    public bool dyeEnabledAll;

    [LocalizedLabel("DyeLerp")]
    [LocalizedTooltip("DyeLerp")]
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
  }

  // Shorthand [LocalizedLabel("MyKey")] vs [Label("$Mods.OriMod.Config.Label.MyKey")]
  // Does not work with HeaderAttribute, since ModLoader.Config.UI.UIModConfig uses "obj.GetType() == typeof(HeaderAttribute)"
  class LocalizedLabelAttribute : LabelAttribute {
    public LocalizedLabelAttribute(string labelKey) : base($"$Mods.OriMod.Config.Label.{labelKey}") {
    }
  }
  class LocalizedTooltipAttribute : TooltipAttribute {
    public LocalizedTooltipAttribute(string tooltipKey) : base($"$Mods.OriMod.Config.Tooltip.{tooltipKey}") { }
}
