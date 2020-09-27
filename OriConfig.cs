using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria;
using Terraria.ModLoader.Config;

namespace OriMod {
  /// <summary>
  /// Configurations for the player's own Ori settings.
  /// </summary>
  [Label("Client Config")]
  public class OriConfigClient1 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("Gameplay")]

    [Label("Player Light"), Tooltip("Determines if your player faintly glows.\nDefault: true")]
    [DefaultValue(true)]
    public bool PlayerLight;

    [Label("Global Player Light"), Tooltip("Determines if all other players glow based on your config or their config.\nDefault: false")]
    [DefaultValue(false)]
    public bool GlobalPlayerLight;

    [Header("Controls")]

    [Label("Stomp Activation Delay"), Tooltip("Duration the Stomp key must be held until activating Stomp.\nDefault: 0")]
    [DefaultValue(0)]
    public float StompHoldDownDelay = 0;

    [Label("Soft Crouch"), Tooltip("Allows moving while holding crouch.\nDefault: false")]
    [DefaultValue(false)]
    public bool SoftCrouch;

    [Label("Smooth Camera"), Tooltip("Smooths the camera when it moves.\nDefault: true")]
    [DefaultValue(true)]
    public bool SmoothCamera;

    [JsonIgnore]
    internal bool BurrowToMouse => BurrowControls == "Mouse";

    [Label("Burrow Control"), Tooltip("Determines which controls you use while burrowing.\nDefault: Mouse")]
    [DefaultValue("Mouse"), OptionStrings(new string[] { "WASD", "Mouse" })]
    public string BurrowControls;

    [Header("Aesthetics")]

    [Label("Player Color"), Tooltip("The color of your Spirit.\nDefault: 210, 255, 255")]
    [DefaultValue(typeof(Color), "210, 255, 255, 255"), ColorNoAlpha]
    public Color PlayerColor;

    [Label("Player Color (Secondary)"), Tooltip("The secondary color of your Spirit.\nDefault: 0, 0, 0, 0")]
    [DefaultValue(typeof(Color), "0, 0, 0, 0")]
    public Color PlayerColorSecondary;

    public override void OnLoaded() {
      OriMod.ConfigClient = this;
    }

    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext stream) {
      PlayerColor.A = 255;
      if (StompHoldDownDelay < 0) {
        StompHoldDownDelay = 0;
      }
    }

    public override void OnChanged() {
      var player = Main.LocalPlayer;
      if (player.active) {
        var oPlayer = player.GetModPlayer<OriPlayer>();
        oPlayer.SpriteColorPrimary = PlayerColor;
        oPlayer.SpriteColorSecondary = PlayerColorSecondary;
      }
    }
  }
}
