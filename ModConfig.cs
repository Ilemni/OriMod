using System.ComponentModel;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace OriMod {
  [Label("Client Config")]
  public class OriConfigClient1 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [Header("Gameplay"), DefaultValue(true)]
    
    [Label("Ability Cooldowns"), Tooltip("Determines if abilities have cooldowns. Enable this for a more balanced experience.")]
    public bool AbilityCooldowns;
    
    [Label("Player Light"), Tooltip("Determines if your player faintly glows."), DefaultValue(true)]
    public bool PlayerLight;
    
    [Label("Global Player Light"), Tooltip("Determines if all other players glow based on your config or their config."), DefaultValue(false)]
    public bool GlobalPlayerLight;    
    
    [Header("Controls")]
    
    [Label("Stomp Activation Delay"), Tooltip("How many frames the Stomp key must be held until activating Stomp.\nThere are 60 frames in one second."), DefaultValue(0)]
    public int StompHoldDownDelay = 0;
    
    [Label("Smooth Camera"), Tooltip("Smooths camera movement when it moves."), DefaultValue(true)]
    public bool SmoothCamera;
    
    [Label("Soft Crouch"), Tooltip("Allows moving while crouched."), DefaultValue(false)]
    public bool SoftCrouch;


    // [Label("Auto Burrow"), Tooltip("After burrowing, if holding the Burrow key, automatically re-enter Burrow as soon as possible.")]
    [JsonIgnore] // Buggy implementation
    public bool AutoBurrow;

    [JsonIgnore]
    internal bool BurrowToMouse => BurrowControls == "Mouse";
    
    [Label("Burrow Controls"), Tooltip("Determines which controls you use while burrowing."), OptionStrings(new string[] {"WASD", "Mouse"}), DefaultValue("Mouse")]
    public string BurrowControls;
    
    [Header("Aesthetics")]

    [Label("Player Color"), Tooltip("The color of your Spirit Guardian."), DefaultValue(typeof(Color), "220, 255, 255, 255")]
    public Color PlayerColor;
    [Label("Player Color (Secondary)"), Tooltip("The secondary color of your Spirit Guardian.")]
    public Color PlayerColorSecondary;

    public override void OnLoaded() {
      OriMod.ConfigClient = this;
    }
  }
  [Label("Ability Config")]
  public class OriConfigClient2 : ModConfig {
    public override ConfigScope Mode => ConfigScope.ClientSide;
    
    [Header("Burrow")]
    
    [Label("Burrow Strength"), Description("Determines which tiles Burrow can dig through. Based on Pickaxe power."), Range(-1, 210)]
    [DefaultValue(0)]
    public int BurrowStrength;

    public override void OnLoaded() {
      OriMod.ConfigAbilities = this;
    }
  }
}