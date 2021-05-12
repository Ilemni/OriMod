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
    public bool playerLight;

    [Label("Global Player Light"), Tooltip("Determines if all other players glow based on your config or their config.\nDefault: false")]
    [DefaultValue(false)]
    public bool globalPlayerLight;

    [Header("Controls")]

    [Label("Stomp Activation Delay"), Tooltip("Duration the Stomp key must be held until activating Stomp.\nDefault: 0")]
    [DefaultValue(0)]
    public float stompHoldDownDelay;

    [Label("Soft Crouch"), Tooltip("Allows moving while holding crouch.\nDefault: false")]
    [DefaultValue(false)]
    public bool softCrouch;

    [Label("Smooth Camera"), Tooltip("Smooths the camera when it moves.\nDefault: true")]
    [DefaultValue(true)]
    public bool smoothCamera;

    [JsonIgnore]
    internal bool BurrowToMouse => _burrowControls == "Mouse";

    [Label("Burrow Control"), Tooltip("Determines which controls you use while burrowing.\nDefault: Mouse")]
    [DefaultValue("Mouse"), OptionStrings(new[] { "WASD", "Mouse" })]
    private string _burrowControls;

    [Header("Aesthetics")]

    [Label("Player Color"), Tooltip("The color of your Spirit.\nDefault: 210, 255, 255")]
    [DefaultValue(typeof(Color), "210, 255, 255, 255"), ColorNoAlpha]
    public Color playerColor;

    [Label("Player Color (Secondary)"), Tooltip("The secondary color of your Spirit.\nDefault: 0, 0, 0, 0")]
    [DefaultValue(typeof(Color), "0, 0, 0, 0")]
    public Color playerColorSecondary;

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
    }
  }
}
