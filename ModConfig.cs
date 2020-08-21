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

    [Label("Ability Cooldowns"), Tooltip("Determines if abilities have cooldowns. Keep enabled for a more balanced experience.\nDefault: true")]
    [DefaultValue(true)]
    public bool AbilityCooldowns;

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

    [Label("Soft Crouch"), Tooltip("Allows moving while crouched.\nDefault: false")]
    [DefaultValue(false)]
    public bool SoftCrouch;

    [Label("Smooth Camera"), Tooltip("Smooths the camera when it moves.\nDefault: true")]
    [DefaultValue(true)]
    public bool SmoothCamera;

    // [Label("Auto Burrow"), Tooltip("After burrowing, if holding the Burrow key, automatically re-enter Burrow as soon as possible.\nDefault: false")]
    // [DefaultValue(false)]
    [JsonIgnore] // Buggy implementation
    public bool AutoBurrow;

    [JsonIgnore]
    internal bool BurrowToMouse => BurrowControls == "Mouse";

    [Label("Burrow Control"), Tooltip("Determines which controls you use while burrowing.\nDefault: Mouse")]
    [DefaultValue("Mouse"), OptionStrings(new string[] { "WASD", "Mouse" })]
    public string BurrowControls;

    [Header("Aesthetics")]

    [Label("Player Color"), Tooltip("The color of your Spirit Guardian.\nDefault: 210, 255, 255")]
    [DefaultValue(typeof(Color), "210, 255, 255, 255"), ColorNoAlpha]
    public Color PlayerColor;

    [Label("Player Color (Secondary)"), Tooltip("The secondary color of your Spirit Guardian.\nDefault: 0, 0, 0, 0")]
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
  }

  [Label("Ability Config")]
  public class OriConfigClient2 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("Soul Link")]

    [Label("Cooldown"), Tooltip("Cooldown of Soul Link, in seconds.\nDefault: 30")]
    [DefaultValue(30), Range(0f, 150), Increment(1f)]
    [System.Obsolete] internal float SoulLinkCooldown;

    [Label("Charge Time"), Tooltip("How long it takes to place a Soul Link, in seconds.\nDefault: 0.9")]
    [DefaultValue(0.9f), Range(0.1f, 10), Increment(0.1f)]
    [System.Obsolete] internal float SoulLinkChargeRate;

    [Label("Respawn Time"), Tooltip("How long after dying it takes to respawn at a Soul Link, in seconds.\nDefault: 2.4")]
    [DefaultValue(2.4f), Range(1f, 10), Increment(0.1f)]
    [System.Obsolete] internal float SoulLinkRespawnTime;

    [Header("Air Jump")]

    [Label("Number of Jumps"), Tooltip("Amount of times the player can jump in the air.\nDefault: 2")]
    [DefaultValue(2), Range(1, int.MaxValue)]
    public int AirJumpCount;

    [Header("Bash")]

    [Label("Cooldown"), Tooltip("Cooldown of Bash, in seconds.\nDefault: 0.75")]
    [DefaultValue(0.75f), Range(0f, 20), Increment(0.25f)]
    public float BashCooldown;

    [Label("Strength"), Tooltip("How far Bashing pushes the player and NPCs.\nDefault: 15")]
    [DefaultValue(15), Range(0f, 50), Increment(0.5f)]
    public float BashStrength;

    [Label("Range"), Tooltip("Furthest distance from the player a target can be. 1 tile = 16 units.\nDefault: 64")]
    [DefaultValue(64), Range(8f, 160), Increment(8f)]
    public float BashRange;

    [Label("Allow Bashing Projectiles"), Tooltip("Allow bashing most projectiles.\nDefault: true")]
    [DefaultValue(true)]
    public bool BashOnProjectiles;

    [Label("Allow Bashing Friendly Projectiles"), Tooltip("Allows you to bash projectiles that damage enemies.\n\"Allow Bashing Projectiles\" must be enabled.\nDefault: false")]
    [DefaultValue(false)]
    public bool BashOnProjectilesFriendly;

    [Header("Stomp")]

    [Label("Cooldown"), Tooltip("Cooldown of Stomp, in seconds.\nDefault: 6")]
    [DefaultValue(6), Range(0f, 30), Increment(0.25f)]
    public float StompCooldown;

    [Label("Fall Speed"), Tooltip("How fast the player falls while Stomping.\nDefault: 28")]
    [DefaultValue(28), Range(1f, 100), Increment(0.5f)]
    public float StompFallSpeed;

    [Label("Number of Targets"), Tooltip("How many targets Stomp can damage at once.\nDefault: 8")]
    [DefaultValue(8), Range(1, 32)]
    public int StompNumTargets;

    [Header("Charge Jump")]

    [Label("Cooldown"), Tooltip("Cooldown of Charge Jump, in seconds.\nDefault: 6")]
    [DefaultValue(6), Range(0f, 30), Increment(0.25f)]
    public float CJumpCooldown;

    [Label("Speed"), Tooltip("How fast Charge Jump moves the player.\nDefault: 1")]
    [DefaultValue(1), Range(0f, 10), Increment(0.1f)]
    public float CJumpSpeedMultiplier;

    [Header("Wall Charge Jump")]

    [Label("Cooldown"), Tooltip("Cooldown of Wall Charge Jump, in seconds.\nDefault: 4")]
    [DefaultValue(4), Range(0f, 30), Increment(0.25f)]
    public float WCJumpCooldown;

    [Label("Speed"), Tooltip("How fast Wall Charge Jump moves the player.\nDefault: 1")]
    [DefaultValue(1), Range(0f, 10), Increment(0.1f)]
    public float WCJumpSpeedMultipler;

    [Label("Max Angle"), Tooltip("How far up or down Wall Charge Jump can be aimed.\nDefault: 0.65")]
    [DefaultValue(0.65f), Range(0.25f, (float)(Math.PI / 2))]
    public float WCJumpMaxAngle;

    [Header("Dash")]

    [Label("Cooldown"), Tooltip("Cooldown of Dash, in seconds.\nNote this is only when a boss is alive.\nDefault: 1")]
    [DefaultValue(1), Range(0f, 10), Increment(0.25f)]
    public float DashCooldown;

    [Label("Speed"), Tooltip("How fast Dash moves the player.\nDefault: 1")]
    [DefaultValue(1), Range(0f, 10), Increment(0.1f)]
    public float DashSpeedMultiplier;

    [Header("Charge Dash")]

    [Label("Cooldown"), Tooltip("Cooldown of Charge Dash, in seconds.\nNote this is only when a boss is alive.\nDefault: 3")]
    [DefaultValue(3), Range(0f, 30), Increment(0.25f)]
    public float CDashCooldown;

    [Label("Speed"), Tooltip("How fast Charge Dash moves the player.\nDefault: 1")]
    [DefaultValue(1), Range(0f, 10), Increment(0.1f)]
    public float CDashSpeedMultiplier;

    [Header("Burrow")]

    [Label("Duration"), Tooltip("How long Burrow can be used until suffocating, in seconds.\nDefault: 4")]
    [DefaultValue(4), Range(1f, 30), Increment(1f)]
    public float BurrowDuration;

    [Label("Recovery"), Tooltip("Rate that Burrow replenishes itself when not in use.\nDefault: 1")]
    [DefaultValue(1), Range(0.1f, 3), Increment(0.1f)]
    public float BurrowRecoveryRate;

    [Label("Strength"), Tooltip("Determines which tiles Burrow can dig through. Based on Pickaxe power.\nDefault: 0")]
    [DefaultValue(0), Range(-1, 210), Slider()]
    public int BurrowStrength;

    public override void OnLoaded() {
      OriMod.ConfigAbilities = this;
    }

    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext stream) {
      if (AirJumpCount < 0) {
        AirJumpCount = 0;
      }

      if (StompNumTargets < 1) {
        StompNumTargets = 1;
      }
    }
  }
}
