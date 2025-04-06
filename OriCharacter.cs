using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AnimLib;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod;

public sealed partial class OriCharacter : AnimCharacter {
  public override string DisplayName => "Ori";

  /// <inheritdoc cref="MovementStates"/>
  [field: AllowNull, MaybeNull]
  public MovementStates MoveStates => field ??= GetState<MovementStates>();

  [field: AllowNull, MaybeNull]
  public OriAnimation Anim => field ??= GetAnimation<OriAnimation>();

  public State? ActiveState => MoveStates.ActiveChild;

  // Todo: a way to customize this in UI? Prefer avoiding config menu.
  public float DyeColorBlend = 0.65f;

  private bool DoPlayerLight => IsLocal || OriMod.ConfigClient.globalPlayerLight
    ? OriMod.ConfigClient.playerLight
    : _multiplayerPlayerLight;


  /// <summary>
  /// Whether the multiplayer client instance of this <see cref="OriCharacter"/> uses light.
  /// </summary>
  private bool _multiplayerPlayerLight;

  private readonly Color _lightColor = new(0.2f, 0.4f, 0.4f);
  internal float LightStrength = 1f;

  public override IEnumerable<State> ActiveChildren => Children;

  /// <summary>
  /// Ori character controls for this player.
  /// </summary>
  internal OriInput Input { get; private set; } = null!; // Initialize()

  /// <summary>
  /// Manager for all <see cref="TrailSegment"/>s on this <see cref="OriCharacter"/> instance.
  /// This will be null if <see cref="Main.dedServ">Main.dedServ</see> is <see langword="true"/>
  /// </summary>
  internal Trail? Trail { get; private set; }

  // Init as true required for player selection menu displaying character as idle
  /// <summary>
  /// Represents if the player is on the ground.
  /// </summary>
  public bool IsGrounded { get; private set; } = true;

  public int GroundedTime { get; private set; }

  public int InAirTime { get; private set; }

  /// <summary>
  /// Represents if the player is on a wall.
  /// </summary>
  public bool OnWall { get; private set; }

  public int OnWallTime { get; private set; }

  public int OffWallTime { get; private set; }

  /// <summary>
  /// Whether player was mounted last tick. Used to decay trail segments when player dismounts.
  /// </summary>
  private bool _wasMounted;

  /// <summary>
  /// Used for <see cref="CreatePlayerDust"/>. Prevents dust spam when value is above 0.
  /// </summary>
  private int _playerDustTimer;

  /// <summary>
  /// Used for temporary storing of Rocket boots remaining time for <see cref="AirJump"/>
  /// </summary>
  private int _rocketBootsRemaining = -1;

  /// <summary>
  /// Used for temporary storing of Carpet remaining time for <see cref="AirJump"/>
  /// </summary>
  private int _carpetRemaining = -1;

  private SoundInfo _hurtSound = new("Ori/Hurt/seinHurtRegular", 4, 1);

  private SoundInfo _deathDrown = new("Ori/Death/seinSwimmingDrowningDeath", 3, 1);
  private SoundInfo _deathLava = new("Ori/Death/seinDeathLava", 5, 1);
  private SoundInfo _deathRegular = new("Ori/Death/seinDeathRegular", 5, 1);

  public override void RegisterChildren(List<State> statesToAdd) {
    statesToAdd.AddRange([
      // TODO: Create class for arm state and register it here.
      //  Arm state may hold vars for how an arm should be drawn.
      //  No transition logic, unless custom Ori actions are added,
      //  such as a custom attack or ability that must operate
      //  independently of MovementStates.
      GetState<MovementStates>()
    ]);
  }

  protected override void OnEnter(State? fromState) {
    MoveStates.SetActive(true);
  }

  public override void SaveData(TagCompound tag) {
    base.SaveData(tag);
    tag.Add("DyeColorBlend", DyeColorBlend);
  }

  public override void LoadData(TagCompound tag) {
    base.LoadData(tag);
    DyeColorBlend = tag.Get<float>("DyeColorBlend");
  }

  public override AnimCharacterStyle GetDefaultStyle() {
    Color defaultSkin = new(210, 255, 255, 255);
    Color defaultScleraColor = Color.White * 0.1f;
    Color defaultFeatherColor = new(35, 26, 91);

    return new AnimCharacterStyle {
      EyeColor = Color.White,
      HairColor = defaultSkin,
      SkinColor = defaultSkin,
      ShirtColor = defaultSkin,
      UnderShirtColor = defaultScleraColor,
      PantsColor = defaultFeatherColor,
      ShoeColor = defaultSkin,
    };
  }

  public override AnimCharacterStyleUISettings GetStyleUISettings() {
    TextureDictionary primary = ModContent.Request<TextureDictionary>
      ("OriMod/UI/CharCreation/ColorPrimary", AssetRequestMode.ImmediateLoad).Value;
    TextureDictionary secondary = ModContent.Request<TextureDictionary>
      ("OriMod/UI/CharCreation/ColorSecondary", AssetRequestMode.ImmediateLoad).Value;

    AnimCharacterStyleUISettings uiSettings = new() {
      HideHairColorOption = true,
      HideShoeColorOption = true,
      SkinColorIcon = (primary["Colored"], primary["Uncolored"]),
      ShirtColorIcon = (secondary["Colored"], secondary["Uncolored"]),
      UnderShirtColorIcon = (
        Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorEyeBack"),
        Main.Assets.Request<Texture2D>("Images/UI/CharCreation/ColorEye")
      ),
      PantsColorIcon = (ModContent.Request<Texture2D>("OriMod/UI/CharCreation/ColorGlide"), null),
    };
    uiSettings.OnCategoriesBarChanged += (pickers, visual) => {
      // Move eye color button to 2nd last position
      visual.Append(pickers[4]);
      // Move glide color button (pants) to last position
      visual.Append(pickers[8]);
    };

    return uiSettings;
  }

  public override void SetStaticDefaults() {
    StarlightCompat.Initialize();
  }

  public override void Initialize() {
    Input = new OriInput();
    Trail = !Main.dedServ ? new Trail(this) : null;
  }

  [HookCondition(HookConditionFlags.LocalPlayer | HookConditionFlags.CharacterActive)]
  public override void ResetEffects() {
    if (_playerDustTimer > 0) {
      _playerDustTimer--;
    }

    Trail!.UpdateSegments();
    if (Player is { dead: false, invis: false }) {
      Trail.ResetNextSegment();
    }
  }

  public override void ProcessTriggers(TriggersSet triggersSet) {
    if (IsLocal) {
      Input.Update();
    }
  }

  public override void UpdateEquips() {
    if (ActiveState is Climb or WallChargeJump) {
      Player.portableStoolInfo.HasAStool = false;
    }

    if (_rocketBootsRemaining != -1) {
      Player.rocketTime = _rocketBootsRemaining;
      _rocketBootsRemaining = -1;
    }

    if (_carpetRemaining != -1) {
      Player.carpetTime = _carpetRemaining;
      _carpetRemaining = -1;
    }
  }

  public override void PostUpdateEquips() {
    bool isRefreshable = ActiveState is AirJump || OnWall;
    bool canAirJump = GetState<AirJump>().CanEnter();

    if (isRefreshable || canAirJump) {
      _rocketBootsRemaining = Player.rocketTime;
      Player.rocketTime = 0;
      if (isRefreshable) {
        _carpetRemaining = Player.carpetTime;
        Player.carpetTime = 0;
      }
    }
  }

  public override void PostUpdateRunSpeeds() {
    if (ActiveState is Transform) {
      return;
    }

    // Run speeds
    Player.maxRunSpeed += 2f;
    Player.noFallDmg = true;
    Player.jumpSpeedBoost += 2f;

    if (ActiveState is not Stomp) {
      Player.gravity = Math.Min(Player.gravity, 0.35f);
      if (IsGrounded) {
        Player.runAcceleration = Math.Clamp(MathF.Pow(Player.runAcceleration, 3f) * 980f, Player.runAcceleration, 0.5f);
        Player.runSlowdown = Math.Clamp(MathF.Pow(Player.runSlowdown, 2f) * 25f, Player.runSlowdown, 1);
      }
      else {
        Player.runAcceleration = Player.runAcceleration is > 0.01f and < 0.3f ? 0.3f : Player.runAcceleration;
        Player.runSlowdown = Player.runAcceleration is > 0.01f and < 0.5f ? 0.5f : Player.runAcceleration;
      }
    }

    if (IsLocal && OriMod.ConfigClient.smoothCamera) {
      // Smooth camera effect reduced while bosses are alive
      Main.SetCameraLerp(OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f, 1);
    }

    if (!OnWall) {
      return;
    }

    Player.blockExtraJumps = true;

    // Reduce gravity when clinging on wall
    if (ActiveState is AirJump or Stomp or Climb) {
      return;
    }

    // Either grounded or falling, not climbing
    float yDirection = Player.velocity.Y * Player.gravDir;
    if (IsGrounded || yDirection < 0) {
      Player.gravity = Math.Min(Player.gravity, 0.1f);
      Player.maxFallSpeed = 6f;
      Player.jumpSpeedBoost -= 6f;
    }

    // Sliding upward on wall, not stomping
    else if (!IsGrounded && yDirection > 0) {
      Player.gravity = Math.Min(Player.gravity, 0.1f);
      Player.maxFallSpeed = 6f;
    }
  }

  public override void PostUpdate() {
    IsGrounded = CheckGrounded();
    OnWall = CheckOnWall();

    GroundedTime = IsGrounded ? GroundedTime + 1 : 0;
    InAirTime = !IsGrounded ? InAirTime + 1 : 0;
    OnWallTime = OnWall ? OnWallTime + 1 : 0;
    OffWallTime = !OnWall ? OffWallTime + 1 : 0;

    if (Active && (IsGrounded || OnWall)) {
      RestoreAirJumps();
    }

    if (!DoPlayerLight || ActiveState is Burrow) {
      return;
    }

    if (Main.dontStarveWorld) {
      LightStrength -= 0.004f;
    }

    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable - Color.ToVector3 is pure
    Lighting.AddLight(Player.Center, _lightColor.ToVector3() * LightStrength);
    Vector3 tileLight = Lighting.GetColor(Player.Center.ToTileCoordinates()).ToVector3();
    float brightness = tileLight.X + tileLight.Y + tileLight.Z;
    LightStrength = Math.Max(Math.Min(LightStrength, 1f), brightness / 2);
  }

  protected override void NetSync(NetSyncer sync) {
    sync.Sync(ref _multiplayerPlayerLight);
  }

  public override void OnExtraJumpRefreshed(ExtraJump jump) {
    Player.canCarpet = true;
    Player.rocketTime = Player.rocketTimeMax;
    Player.wingTime = Player.wingTimeMax;
  }

  public override void FrameEffects() {
    if (Player.velocity.LengthSquared() > 0.2f) {
      CreatePlayerDust();
    }
  }

  public override bool FreeDodge(Player.HurtInfo info) =>
    ActiveState is Stomp or ChargeDash or ChargeJump;

  public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
    modifiers.DisableDust();
    modifiers.DisableSound();
  }

  public override void PostHurt(Player.HurtInfo info) {
    if (info.SoundDisabled) {
      _hurtSound.Play(Player);
    }
  }

  public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore,
    ref PlayerDeathReason damageSource) {
    // similar to prehurt, but for death
    if (playSound) {
      playSound = false;
      switch (damageSource.SourceOtherIndex) {
        case 1:
          _deathDrown.Play(Player);
          break;
        case 2:
          _deathLava.Play(Player);
          break;
        default:
          _deathRegular.Play(Player);
          break;
      }
    }

    if (!genGore) {
      return true;
    }

    genGore = false;
    for (int i = 0; i < 15; i++) {
      Dust dust = Dust.NewDustDirect(Player.position, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0,
        Color.White);
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
    }

    return true;
  }

  public override void HideDrawLayers(PlayerDrawSet drawInfo) {
    if (Player.mount.Active) {
      _wasMounted = true;
    }
    else if (_wasMounted) {
      Trail!.DecayAllSegments();
      _wasMounted = false;
    }

    PlayerDrawLayers.ArmorLongCoat.Hide();
    PlayerDrawLayers.ArmOverItem.Hide();
    PlayerDrawLayers.BackAcc.Hide();
    PlayerDrawLayers.BalloonAcc.Hide();

    //PlayerDrawLayers.BeetleBuff.Hide();
    PlayerDrawLayers.BladedGlove.Hide();
    PlayerDrawLayers.ElectrifiedDebuffBack.Hide();
    PlayerDrawLayers.ElectrifiedDebuffFront.Hide();
    PlayerDrawLayers.FaceAcc.Hide();
    PlayerDrawLayers.FinchNest.Hide();
    PlayerDrawLayers.FrontAccBack.Hide();
    PlayerDrawLayers.FrontAccFront.Hide();

    //PlayerDrawLayers.FrozenOrWebbedDebuff.Hide();
    PlayerDrawLayers.HairBack.Hide();
    PlayerDrawLayers.HandOnAcc.Hide();
    PlayerDrawLayers.Head.Hide();
    PlayerDrawLayers.HeadBack.Hide();

    //PlayerDrawLayers.IceBarrier.Hide();
    PlayerDrawLayers.JimsCloak.Hide();
    PlayerDrawLayers.Leggings.Hide();
    PlayerDrawLayers.NeckAcc.Hide();
    PlayerDrawLayers.OffhandAcc.Hide();
    PlayerDrawLayers.ProjectileOverArm.Hide();
    PlayerDrawLayers.Robe.Hide();

    //PlayerDrawLayers.SafemanSun.Hide();
    PlayerDrawLayers.Shield.Hide();
    PlayerDrawLayers.Shoes.Hide();
    PlayerDrawLayers.Skin.Hide();
    PlayerDrawLayers.SkinLongCoat.Hide();
    PlayerDrawLayers.Tails.Hide();
    PlayerDrawLayers.Torso.Hide();
    PlayerDrawLayers.WaistAcc.Hide();

    //PlayerDrawLayers.WebbedDebuffBack.Hide();

    if (ActiveState is Transform or AirJump or Burrow or ChargeJump or WallChargeJump) {
      PlayerDrawLayers.HeldItem.Hide();
      PlayerDrawLayers.Wings.Hide();
      PlayerDrawLayers.Backpacks.Hide();
      PlayerDrawLayers.ForbiddenSetRing.Hide();
      PlayerDrawLayers.LeinforsHairShampoo.Hide();

      //PlayerDrawLayers.EyebrellaCloud.Hide();
      PlayerDrawLayers.SolarShield.Hide();

      //PlayerDrawLayers.PortableStool.Hide();
    }
  }

  public override void OnRespawn() {
    MoveStates.TriggerState<NoAbility>();
  }


  /// <summary>
  /// Refreshes airborne abilities,
  /// allowing the player to jump and dash again before touching the ground.
  /// </summary>
  public void RestoreAirJumps() {
    Player.RefreshExtraJumps();
    GetState<AirJump>().EndCooldown(force: true);
    GetState<Dash>().EndCooldown(force: true);
    GetState<Launch>().EndCooldown(force: true);
  }

  private bool CheckGrounded() {
    if (ActiveState is Climb) {
      return false;
    }

    float gravDir = Player.gravDir;
    if (Player.velocity.Y * gravDir is < 0 or > 0.01f) {
      return false;
    }

    if (CheckGrounded_StarlightRiverBasePlatform(Player)) {
      return true;
    }

    if (Player.waterWalk || Player.waterWalk2) {
      // waterWalk includes walking on lava
      // waterWalk2 does not allow lava walking
      Point pos = new Vector2 {
        X = Player.Center.X,
        Y = Player.position.Y + (gravDir > 0 ? Player.height : 0) + 1f / 255f * gravDir
      }.ToTileCoordinates();

      Tile tile = Main.tile[pos];
      bool isLiquidSurface = tile.LiquidAmount > 0 && Main.tile[pos.X, pos.Y - 1].LiquidAmount == 0;
      if (isLiquidSurface && (Player.waterWalk || tile.LiquidType != LiquidID.Lava)) {
        return true;
      }
    }

    return !Collision.IsClearSpotTest(Player.position + new Vector2(0, 8 * gravDir), 16f, Player.width,
      Player.height, false, false, (int)gravDir, true, true);
  }

  private bool CheckOnWall() {
    Point p = new Vector2(
      Player.position.X + (Player.direction < 0 ? -1 : Player.width + 1),
      Player.position.Y + (Player.gravDir < 0 ? -1 : 2)
    ).ToTileCoordinates();
    return WorldGen.SolidTile(p.X, p.Y + 1) && WorldGen.SolidTile(p.X, p.Y + 2);
  }

  /// <summary>
  /// Emits a white dust speck from the player.
  /// </summary>
  internal void CreatePlayerDust() {
    if (Main.dedServ || _playerDustTimer > 0 || !Active || !GraphicsEnabledCompat ||
        LightStrength < 0.1f) {
      return;
    }

    Dust dust = Main.dust[
      Dust.NewDust(Player.position, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0, Color.White)];
    dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
    dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
    dust.noGravity = false;

    _playerDustTimer = ActiveState switch {
      Transform => Main.rand.Next(3, 8),
      Dash or ChargeDash => Main.rand.Next(2, 4),
      Burrow => Main.rand.Next(6, 10),
      _ => Main.rand.Next(10, 15)
    };
  }
}
