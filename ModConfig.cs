using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace OriMod {
  [Label("Client Config")]
  public class OriConfigClient1 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [Header("Gameplay")]
    
    [Label("Ability Cooldowns"), Tooltip("Determines if abilities have cooldowns. Keep enabled for a more balanced experience.")]
    [DefaultValue(true)]
    public bool AbilityCooldowns;
    
    [Label("Player Light"), Tooltip("Determines if your player faintly glows.")]
    [DefaultValue(true)]
    public bool PlayerLight;
    
    [Label("Global Player Light"), Tooltip("Determines if all other players glow based on your config or their config.")]
    [DefaultValue(false)]
    public bool GlobalPlayerLight;    
    
    [Header("Controls")]
    
    [Label("Stomp Activation Delay"), Tooltip("Duration the Stomp key must be held until activating Stomp.")]
    [DefaultValue(0)]
    public float StompHoldDownDelay = 0;
    
    [Label("Soft Crouch"), Tooltip("Allows moving while crouched.")]
    [DefaultValue(false)]
    public bool SoftCrouch;
    
    [Label("Smooth Camera"), Tooltip("Smooths camera movement when it moves.")]
    [DefaultValue(true)]
    public bool SmoothCamera;
    
    // [Label("Auto Burrow"), Tooltip("After burrowing, if holding the Burrow key, automatically re-enter Burrow as soon as possible.")]
    [JsonIgnore] // Buggy implementation
    public bool AutoBurrow;

    [JsonIgnore]
    internal bool BurrowToMouse => BurrowControls == "Mouse";
    
    [Label("Burrow Controls"), Tooltip("Determines which controls you use while burrowing.")]
    [DefaultValue("Mouse"), OptionStrings(new string[] {"WASD", "Mouse"})]
    public string BurrowControls;
    
    [Header("Aesthetics")]

    [Label("Player Color"), Tooltip("The color of your Spirit Guardian.")]
    [DefaultValue(typeof(Color), "210, 255, 255, 255")]
    public Color PlayerColor;
    [Label("Player Color (Secondary)"), Tooltip("The secondary color of your Spirit Guardian.")]
    [DefaultValue(typeof(Color), "0, 0, 0, 0")]
    public Color PlayerColorSecondary;

    public override void OnLoaded() {
      OriMod.ConfigClient = this;
    }
    private int MinColorAny => 100;
    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext stream) {
      float oldMax = MathHelper.Max(MathHelper.Max(PlayerColor.R, PlayerColor.G), PlayerColor.B);
      if (oldMax < MinColorAny) {
        PlayerColor *= ((MinColorAny + 1) / oldMax);
        if (PlayerColor == Color.Black) {
          PlayerColor = new Color(100, 100, 100);
        }
      }
      PlayerColor.A = 255;
      if (StompHoldDownDelay  < 0) StompHoldDownDelay = 0;
    }
  }

  [Label("Ability Config")]
  public class OriConfigClient2 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("Soul Link")]
    
    [Label("Cooldown"), Description("Cooldown of Soul Link, in seconds.")]
    [DefaultValue(30), Range(15f, 300)]
    public float SoulLinkCooldown;

    [Label("Charge Time"), Description("How long it takes to place a Soul Link.")]
    [DefaultValue(0.9f), Range(0.05f, 10)]
    public float SoulLinkChargeRate;

    [Label("Respawn Time"), Description("How long after dying it takes to respawn at a Soul Link.")]
    [DefaultValue(1.2f), Range(1.2f, 10)]
    public float SoulLinkRespawnTime;

    [Header("Air Jump")]
    [Label("Number of Jumps"), Description("Amount of times the player can jump in the air.")]
    [DefaultValue(2)]
    public int AirJumpCount;
    
    [Header("Bash")]

    [Label("Cooldown"), Description("Cooldown of Bash, in frames.")]
    [DefaultValue(0.75f), Range(0.25f, 20)]
    public float BashCooldown;

    [Label("Strength"), Description("How far Bashing pushes the player and NPCs.")]
    [DefaultValue(15), Range(10f, 30)]
    public float BashStrength;

    [Label("Range"), Description("Furthest distance from the player a target can be. 1 tile = 16 units.")]
    [DefaultValue(48), Range(16f, 160)]
    public float BashRange;

    [Header("Stomp")]

    [Label("Cooldown"), Description("Cooldown of Stomp, in seconds.")]
    [DefaultValue(6), Range(1f, 30)]
    public float StompCooldown;

    [Label("Fall Speed"), Description("How fast the player falls while Stomping.")]
    [DefaultValue(28), Range(1f, 100)]
    public float StompFallSpeed;

    [Header("Charge Jump")]

    [Label("Cooldown"), Description("Cooldown of Charge Jump, in seconds.")]
    [DefaultValue(6), Range(1f, 30)]
    public float CJumpCooldown;

    [Label("Speed"), Description("How fast Charge Jump moves the player.")]
    [DefaultValue(1), Range(0.1f, 5)]
    public float CJumpSpeedMultiplier;

    [Header("Wall Charge Jump")]

    [Label("Cooldown"), Description("Cooldown of Wall Charge Jump, in seconds.")]
    [DefaultValue(4), Range(1f, 30)]
    public float WCJumpCooldown;

    [Label("Speed"), Description("How fast Wall Charge Jump moves the player.")]
    [DefaultValue(1), Range(0.1f, 5)]
    public float WCJumpSpeedMultipler;

    [Label("Max Angle"), Description("How far up or down Wall Charge Jump can be aimed.")]
    [DefaultValue(0.65f), Range(0.25f, (float)(Math.PI / 2))]
    public float WCJumpMaxAngle;

    [Header("Dash")]

    [Label("Cooldown"), Description("Cooldown of Dash, in seconds.\nNote this is only when a boss is alive.")]
    [DefaultValue(1), Range(0.25f, 10f)]
    public float DashCooldown;

    [Label("Speed"), Description("How fast Dash moves the player.")]
    [DefaultValue(1), Range(0.1f, 5)]
    public float DashSpeedMultiplier;

    [Header("Charge Dash")]
    
    [Label("Cooldown"), Description("Cooldown of Charge Dash, in seconds.\nNote this is only when a boss is alive.")]
    [DefaultValue(3), Range(0.25f, 30f)]
    public float CDashCooldown;

    [Label("Speed"), Description("How fast Charge Dash moves the player.")]
    [DefaultValue(1), Range(0.1f, 5)]
    public float CDashSpeedMultiplier;

    [Header("Burrow")]
    
    [Label("Burrow Strength"), Description("Determines which tiles Burrow can dig through. Based on Pickaxe power.")]
    [DefaultValue(0), Range(-1, 210)]
    public int BurrowStrength;

    public override void OnLoaded() {
      OriMod.ConfigAbilities = this;
    }
    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext stream) {
      if (AirJumpCount < 0) AirJumpCount = 0;
    }
  }
}