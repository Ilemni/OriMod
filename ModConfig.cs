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
    
    [Label("Smooth Camera"), Tooltip("Smooths the camera when it moves.")]
    [DefaultValue(true)]
    public bool SmoothCamera;
    
    // [Label("Auto Burrow"), Tooltip("After burrowing, if holding the Burrow key, automatically re-enter Burrow as soon as possible.")]
    [JsonIgnore] // Buggy implementation
    public bool AutoBurrow;

    [JsonIgnore]
    internal bool BurrowToMouse => BurrowControls == "Mouse";
    
    [Label("Burrow Control"), Tooltip("Determines which controls you use while burrowing.")]
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
    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext stream) {
      PlayerColor.A = 255;
      if (StompHoldDownDelay  < 0) StompHoldDownDelay = 0;
    }
  }

  [Label("Ability Config")]
  public class OriConfigClient2 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    // [Header("Soul Link")]
    
    // [Label("Cooldown"), Tooltip("Cooldown of Soul Link, in seconds.")]
    // [DefaultValue(30), Range(0f, 150)]
    internal float SoulLinkCooldown;

    // [Label("Charge Time"), Tooltip("How long it takes to place a Soul Link, in seconds.")]
    // [DefaultValue(0.9f), Range(0.05f, 10)]
    internal float SoulLinkChargeRate;

    // [Label("Respawn Time"), Tooltip("How long after dying it takes to respawn at a Soul Link, in seconds.")]
    // [DefaultValue(1.2f), Range(1f, 10)]
    internal float SoulLinkRespawnTime;

    [Header("Air Jump")]

    [Label("Number of Jumps"), Tooltip("Amount of times the player can jump in the air.")]
    [DefaultValue(2)]
    public int AirJumpCount;
    
    [Header("Bash")]

    [Label("Cooldown"), Tooltip("Cooldown of Bash, in seconds.")]
    [DefaultValue(0.75f), Range(0f, 20)]
    public float BashCooldown;

    [Label("Strength"), Tooltip("How far Bashing pushes the player and NPCs.")]
    [DefaultValue(15), Range(0f, 50)]
    public float BashStrength;

    [Label("Range"), Tooltip("Furthest distance from the player a target can be. 1 tile = 16 units.")]
    [DefaultValue(48), Range(8f, 160)]
    public float BashRange;

    [Header("Stomp")]

    [Label("Cooldown"), Tooltip("Cooldown of Stomp, in seconds.")]
    [DefaultValue(6), Range(0f, 30)]
    public float StompCooldown;

    [Label("Fall Speed"), Tooltip("How fast the player falls while Stomping.")]
    [DefaultValue(28), Range(1f, 100)]
    public float StompFallSpeed;

    [Label("Number of Targets"), Tooltip("How many targets Stomp can damage at once.")]
    [DefaultValue(8), Range(1, 32)]
    public int StompNumTargets;

    [Header("Charge Jump")]

    [Label("Cooldown"), Tooltip("Cooldown of Charge Jump, in seconds.")]
    [DefaultValue(6), Range(0f, 30)]
    public float CJumpCooldown;

    [Label("Speed"), Tooltip("How fast Charge Jump moves the player.")]
    [DefaultValue(1), Range(0f, 10)]
    public float CJumpSpeedMultiplier;

    [Header("Wall Charge Jump")]

    [Label("Cooldown"), Tooltip("Cooldown of Wall Charge Jump, in seconds.")]
    [DefaultValue(4), Range(0f, 30)]
    public float WCJumpCooldown;

    [Label("Speed"), Tooltip("How fast Wall Charge Jump moves the player.")]
    [DefaultValue(1), Range(0f, 10)]
    public float WCJumpSpeedMultipler;

    [Label("Max Angle"), Tooltip("How far up or down Wall Charge Jump can be aimed.")]
    [DefaultValue(0.65f), Range(0.25f, (float)(Math.PI / 2))]
    public float WCJumpMaxAngle;

    [Header("Dash")]

    [Label("Cooldown"), Tooltip("Cooldown of Dash, in seconds.\nNote this is only when a boss is alive.")]
    [DefaultValue(1), Range(0f, 10)]
    public float DashCooldown;

    [Label("Speed"), Tooltip("How fast Dash moves the player.")]
    [DefaultValue(1), Range(0f, 10)]
    public float DashSpeedMultiplier;

    [Header("Charge Dash")]
    
    [Label("Cooldown"), Tooltip("Cooldown of Charge Dash, in seconds.\nNote this is only when a boss is alive.")]
    [DefaultValue(3), Range(0f, 30)]
    public float CDashCooldown;

    [Label("Speed"), Tooltip("How fast Charge Dash moves the player.")]
    [DefaultValue(1), Range(0f, 10)]
    public float CDashSpeedMultiplier;

    [Header("Burrow")]
    
    [Label("Strength"), Tooltip("Determines which tiles Burrow can dig through. Based on Pickaxe power.")]
    [DefaultValue(0), Range(-1, 210)]
    public int BurrowStrength;

    public override void OnLoaded() {
      OriMod.ConfigAbilities = this;
    }
    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext stream) {
      if (AirJumpCount < 0) AirJumpCount = 0;
      if (StompNumTargets < 1) StompNumTargets = 1;
    }
  }
}