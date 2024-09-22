using Microsoft.Xna.Framework;
using OriMod.Networking;
using OriMod.Utilities;
using System;
using AnimLib.Animations;
using AnimLib.States;
using JetBrains.Annotations;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod;

/// <summary>
/// <see cref="ModPlayer"/> class for <see cref="OriMod"/>. Contains Ori data for a player, such as abilities and animations.
/// </summary>
[UsedImplicitly]
public sealed partial class OriPlayer : ModPlayer {
  private const string AbilitiesTagName = "AnimLibAbilities";

  #region Variables

  internal static Asset<AnimSpriteSheet> SpriteSheet { get; private set; } = null!; // SetStaticDefaults
  internal static Asset<Texture2D> BurrowTimer { get; private set; } = null!;

  /// <summary>
  /// Net-synced controls of this player.
  /// </summary>
  internal OriInput Input { get; private set; } = null!; // Initialize()

  public OriCharacter Character { get; private set; } = null!; // Initialize()

  /// <summary>
  /// Manager for all <see cref="TrailSegment"/>s on this OriPlayer instance.
  /// This will be null if <see cref="Main.dedServ">Main.dedServ</see> is <see langword="true"/>
  /// </summary>
  internal Trail Trail { get; private set; } = null!; // Initialize()

  /// <summary>
  /// The current active <see cref="State"/> which is a substate of <see cref="MovementStates"/>.
  /// </summary>
  public State? ActiveState => Character.Move.ActiveChild;

  /// <summary>
  /// Whether this <see cref="OriPlayer"/> instance should sync with multiplayer this frame.
  /// </summary>
  private bool _netUpdate = true;

  /// <summary>
  /// Debug mode, currently only used to view burrow hitboxes
  /// </summary>
  internal bool DebugMode;

  /// <summary>
  /// Whether player was mounted last frame. Used to decay trail segments when player dismounts.
  /// </summary>
  private bool _wasMounted;

  /// <summary>
  /// When set to true, uses custom movement and player sprites.
  /// <para>External mods that attempt to be compatible with this one will need to use this property.</para>
  /// </summary>
  public bool IsOri {
    get => Character.IsActive;
    set {
      if (value == Character.IsActive) {
        return;
      }

      if (value) {
        Character.TryEnable();
      }
      else {
        Character.Disable();
      }
    }
  }

  /// <summary>
  /// <see langword="true"/> if this <see cref="OriPlayer"/> belongs to the local client, otherwise <see langword="false"/>.
  /// </summary>
  public bool IsLocal { get; private set; }

  /// <summary>
  /// Whether the player has been Ori before during this session. Used to hasten subsequent transformations.
  /// </summary>
  internal bool HasTransformedOnce;

  /// <summary>
  /// Represents if the player is on the ground.
  /// </summary>
  public bool IsGrounded { get; private set; }

  /// <summary>
  /// Represents if the player is on a wall.
  /// </summary>
  public bool OnWall { get; private set; }

  /// <summary>
  /// Represents whether a player is grappling onto a wall.
  /// This will also return <see langword="false"/> if there are active grapples that are not sticking to walls.
  /// </summary>

  // Consider getter-only rather than setting per update, or find better vanilla representation
  internal bool IsGrappling { get; private set; }

  /// <summary>
  /// Used for temporary storing of Rocket boots remaining time for airjump
  /// </summary>
  private int _rocketBootsRemaining = -1;

  /// <summary>
  /// Used for temporary storing of Carpet remaining time for airjump
  /// </summary>
  private int _carpetRemaining = -1;

  /// <summary>
  /// Used for <see cref="CreatePlayerDust"/>. Prevents dust spam when value is above 0.
  /// </summary>
  private int _playerDustTimer;

  /// <summary>
  /// When greater than 0, sets <see cref="Player.immune"/> to true.
  /// </summary>
  internal void SetImmune(int value) {
    if (Player.immuneTime < value) {
      Player.immuneTime = value;
    }

    if (Player.immuneTime > 0) {
      Player.immune = true;
      Player.immuneNoBlink = true;
    }
  }

  private SoundInfo _hurtSound = new("Ori/Hurt/seinHurtRegular", 4, 0.75f);

  private SoundInfo _deathDrown = new("Ori/Death/seinSwimmingDrowningDeath", 3, 1);
  private SoundInfo _deathLava = new("Ori/Death/seinDeathLava", 5, 1);
  private SoundInfo _deathRegular = new("Ori/Death/seinDeathRegular", 5, 1);

  #region Aesthetics

  /// <summary>
  /// Primary color of the Ori sprite for this instance of <see cref="OriPlayer"/>.
  /// </summary>
  public Color SpriteColorPrimary {
    get => _spriteColorPrimary;
    set {
      _spriteColorPrimary = value;
      if (IsLocal) {
        OriMod.ConfigClient.playerColor = value;
      }
    }
  }

  /// <summary>
  /// Secondary color of the Ori sprite for this instance of <see cref="OriPlayer"/>.
  /// </summary>
  public Color SpriteColorSecondary {
    get => _spriteColorSecondary;
    set {
      _spriteColorSecondary = value;
      if (IsLocal) {
        OriMod.ConfigClient.playerColorSecondary = value;
      }
    }
  }

  /// <summary>
  /// Coef. of ori and dye color lerp for this instance of <see cref="OriPlayer"/>.
  /// </summary>
  public float DyeColorBlend {
    get => _dyeColorBlend;
    set {
      _dyeColorBlend = value;
      if (IsLocal) {
        OriMod.ConfigClient.dyeLerp = value;
      }
    }
  }

  /// <summary>
  /// Whether the multiplayer client instance of this <see cref="OriPlayer"/> uses light.
  /// </summary>
  internal bool MultiplayerPlayerLight = false;

  /// <summary>
  /// Whether this <see cref="OriPlayer"/> instance uses light.
  /// </summary>
  private bool DoPlayerLight => IsLocal || OriMod.ConfigClient.globalPlayerLight
    ? OriMod.ConfigClient.playerLight
    : MultiplayerPlayerLight;

  private Color _lightColor = new(0.2f, 0.4f, 0.4f);
  private float _lightStrength = 1f;

  /// <summary>
  /// Current dye_shader data, used for dye shader base color extraction.
  /// </summary>
  internal ArmorShaderData? PrimaryDyeShader {
    get {
      int dye = Player.dye[1].netID;
      if (_primaryArmorDye == dye) {
        return _primaryDyeShader;
      }

      _primaryArmorDye = dye;

      return _primaryDyeShader = GameShaders.Armor.GetShaderFromItemId(dye);
    }
  }

  internal ArmorShaderData? SecondaryDyeShader {
    get {
      int dye = Player.dye[0].netID;
      if (_secondaryArmorDye == dye) {
        return _secondaryDyeShader;
      }

      _secondaryArmorDye = dye;

      return _secondaryDyeShader = GameShaders.Armor.GetShaderFromItemId(dye);
    }
  }

  private ArmorShaderData? _primaryDyeShader;
  private ArmorShaderData? _secondaryDyeShader;
  private int _primaryArmorDye;
  private int _secondaryArmorDye;

  #endregion

  #region Backing fields

  private Color _spriteColorPrimary = Color.LightCyan;
  private Color _spriteColorSecondary = Color.LightCyan;
  private float _dyeColorBlend = 0.65f;

  #endregion

  #endregion

  #region Internal Methods

  /// <summary>
  /// Emits a white dust speck from the player.
  /// </summary>
  internal void CreatePlayerDust() {
    if (Main.dedServ || _playerDustTimer > 0 || !Character.GraphicsEnabledCompat || _lightStrength < 0.1f) {
      return;
    }

    Dust dust = Main.dust[
      Dust.NewDust(Player.position, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255))];
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

  /// <summary>
  /// Resets the data of this <see cref="OriPlayer"/> instance.
  /// </summary>
  internal void ResetData() {
    IsOri = false;
    HasTransformedOnce = false;
    Character.ResetAllAbilities();
  }

  #endregion

  public override void SetStaticDefaults() {
    SpriteSheet = ModContent.Request<AnimSpriteSheet>("OriMod/Animations/PlayerAnim");
    BurrowTimer = ModContent.Request<Texture2D>("OriMod/PlayerEffects/BurrowTimer");

    StarlightCompat.Initialize();
  }

  public override void Initialize() {
    Character = new OriCharacter(this);
    Input = new OriInput();

    if (!Main.dedServ) {
      Trail = new Trail(this);
    }
  }

  public override void ResetEffects() {
    if (IsOri && _playerDustTimer > 0) {
      _playerDustTimer--;
    }
  }

  // TODO: Consider thinking about standardizing network operations for Ori state.
  public override void CopyClientState(ModPlayer clientClone) {
    // Changes are tracked via NetUpdate calls.
    base.CopyClientState(clientClone);
  }

  public override void SendClientChanges(ModPlayer clientPlayer) {
    if (_netUpdate) {
      ModNetHandler.OriPlayerHandler.SendOriState(255, Player.whoAmI);
      _netUpdate = false;
    }
  }

  public override void SaveData(TagCompound tag) {
    tag["OriSet"] = IsOri;
    tag["Debug"] = DebugMode;
    tag["TransformedOnce"] = HasTransformedOnce;
    tag["Color1"] = SpriteColorPrimary;
    tag["Color2"] = SpriteColorSecondary;
    tag["DyeColLerp"] = DyeColorBlend;

    //Backward compatibility don't pay attention
    //TODO: Remove old save data once ready
    // Character.OldSave(tag);

    tag[AbilitiesTagName] = Character.Save();
  }

  public override void LoadData(TagCompound tag) {
    IsOri = tag.TryGet("OriSet", out bool set) && set;
    DebugMode = tag.TryGet("Debug", out bool debug) && debug;
    HasTransformedOnce = tag.TryGet("TransformedOnce", out bool value) && value;
    _spriteColorPrimary = tag.TryGet("Color1", out Color color1) ? color1 : OriMod.ConfigClient.playerColor;
    _spriteColorSecondary = tag.TryGet("Color2", out Color color2) ? color2 : OriMod.ConfigClient.playerColorSecondary;
    _dyeColorBlend = tag.TryGet("DyeColLerp", out float blend) ? blend : OriMod.ConfigClient.dyeLerp;

    //Backward compatibility don't pay attention
    // Character.OldLoad(tag);

    //This is current version
    if (tag.TryGet(AbilitiesTagName, out TagCompound compound)) {
      Character.Load(compound);
    }

    //Backward compatibility don't pay attention
    // if (Character.OldAbility is not null) {
    //   foreach (AbilityStateMachine asm in Character.AbilityStateMachines) {
    //     asm.Level = Character.OldAbility[asm.Id];
    //   }
    //
    //   Character.OldAbility = null;
    // }
    //Backward compatibility don't pay attention
  }

  public override void ProcessTriggers(TriggersSet triggersSet) {
    if (!IsLocal) {
      return;
    }

    bool inMenu = Main.ingameOptionsWindow || Main.inFancyUI || Player.talkNPC >= 0 || Player.sign >= 0 ||
      Main.clothesWindow || Main.playerInventory;
    if (OriMod.ConfigClient.blockControlsInMenu && inMenu) {
      Input.DisableAll();
    }
    else {
      Input.Update();
    }
  }

  public override void PostUpdateMiscEffects() {
    IsGrappling = Player.grappling[0] > -1;
    if (Player.HasBuff(BuffID.TheTongue) || IsGrappling || Player.pulley) {
      Main.NewText("FORCE NOABILITY");
      Character.Move.TriggerState<NoAbility>();
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
    bool isAirJumping = ActiveState is AirJump;
    bool canAirJump = Character.Move.GetChild<AirJump>().CanEnter();

    if (canAirJump || isAirJumping || OnWall) {
      _rocketBootsRemaining = Player.rocketTime;
      Player.rocketTime = 0;
    }

    if (isAirJumping || OnWall) {
      _carpetRemaining = Player.carpetTime;
      Player.carpetTime = 0;
    }
  }

  public override void PostUpdateRunSpeeds() {
    if (!IsOri || ActiveState is Transform) {
      return;
    }

    // Run speeds
    Player.maxRunSpeed += 2f;
    Player.noFallDmg = true;
    Player.jumpSpeedBoost += 2f;

    if (ActiveState is not Stomp) {
      Player.gravity = Math.Min(Player.gravity, 0.35f);
    }

    if (IsGrounded) {
      Player.runAcceleration = Math.Clamp(MathF.Pow(Player.runAcceleration, 3f) * 980f, Player.runAcceleration, 0.5f);
      Player.runSlowdown = Math.Clamp(MathF.Pow(Player.runSlowdown, 2f) * 25f, Player.runSlowdown, 1);
    }
    else {
      Player.runAcceleration = Player.runAcceleration is > 0.01f and < 0.3f ? 0.3f : Player.runAcceleration;
      Player.runSlowdown = Player.runAcceleration is > 0.01f and < 0.5f ? 0.5f : Player.runAcceleration;
    }

    if (IsLocal && OriMod.ConfigClient.smoothCamera) {
      // Smooth camera effect reduced while bosses are alive
      Main.SetCameraLerp(OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f, 1);
    }

    // Reduce gravity when clinging on wall
    if (OnWall) {
      Player.blockExtraJumps = true;

      // Either grounded or falling, not climbing
      State? activeState = ActiveState;
      if (activeState is not (AirJump or Stomp or Climb)) {
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
    }
  }

  public override void PostUpdate() {
    if (IsOri) {
      bool isBurrowing = ActiveState is Burrow;
      if (DoPlayerLight && !isBurrowing) {
        if (Main.dontStarveWorld) {
          _lightStrength -= 0.004f;
        }

        Lighting.AddLight(Player.Center, _lightColor.ToVector3() * _lightStrength);
        Vector3 tileLight = Lighting.GetColor(Player.Center.ToTileCoordinates()).ToVector3();
        float brightness = tileLight.X + tileLight.Y + tileLight.Z;
        _lightStrength = Math.Min(Math.Max(_lightStrength, brightness / 2), 1f);
      }

      IsGrounded = CheckGrounded();
      OnWall = CheckOnWall();

      if (IsGrounded || OnWall) {
        RestoreAirJumps();
      }

      // Footstep effects
      if (Main.dedServ || !IsGrounded) {
        return;
      }
    }

    if (!IsLocal) {
      Input.ResetInputChangedState();
    }
  }

  internal void Footstep() {
    if (Main.dedServ) {
      return;
    }

    FootstepManager.PlayFootstepFromPlayer(Player);
    FootstepDust();
  }

  internal void FootstepDust() {
    if (Main.dedServ) {
      return;
    }

    if (_lightStrength <= 0.1f) {
      return;
    }

    Vector2 dustPos = Player.Bottom + new Vector2(Player.direction == -1 ? -4 : 2, -2);
    for (int i = 0; i < 4; i++) {
      int newDust = Dust.NewDust(dustPos, 2, 2, DustID.Clentaminator_Cyan, 0f, -2.7f, 0, new Color(255, 255, 255));
      Dust dust = Main.dust[newDust];
      dust.noGravity = true;
      dust.scale = 0.75f;
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.shader.UseColor(Color.White);
      dust.fadeIn = 0.03947368f;
    }
  }

  private bool CheckGrounded() {
    if (ActiveState is Climb) {
      return false;
    }

    float gravDir = Player.gravDir;
    if (Player.velocity.Y * gravDir is < 0 or > 0.01f) {
      return false;
    }

    if (Player.waterWalk || Player.waterWalk2) {
      Point pos = new Vector2 {
        X = Player.position.X + (float)Player.width / 2,
        Y = Player.position.Y + (gravDir > 0 ? Player.height : 0) + 1f / 255f * gravDir
      }.ToTileCoordinates();

      Tile tile = Main.tile[pos];
      bool isLiquidSurface = tile.LiquidAmount > 0 && Main.tile[pos.X, pos.Y - 1].LiquidAmount == 0;
      if (isLiquidSurface && Player.waterWalk2 || tile.LiquidType != LiquidID.Lava) {
        return true;
      }
    }

    if (CheckGrounded_StarlightRiverBasePlatform(Player)) {
      return true;
    }

    return !Collision.IsClearSpotTest(Player.position + new Vector2(0, 8 * gravDir), 16f, Player.width,
      Player.height, false, false, (int)gravDir, true, true);
  }

  private bool CheckOnWall() {
    Point p = new Vector2(
      Player.Center.X + Player.direction + Player.direction * Player.width * 0.5f,
      Player.position.Y + (Player.gravDir < 0f ? -1f : 2f)
    ).ToTileCoordinates();
    return WorldGen.SolidTile(p.X, p.Y + 1) && WorldGen.SolidTile(p.X, p.Y + 2);
  }

  /// <summary>
  /// Refreshes airborne abilities,
  /// allowing the player to jump and dash again before touching the ground.
  /// </summary>
  public void RestoreAirJumps() {
    Player.RefreshExtraJumps();
    MovementStates move = Character.Move;
    move.GetChild<AirJump>().EndCooldown();
    move.GetChild<Dash>().EndCooldown();
    move.GetChild<Launch>().EndCooldown();
  }

  public override void OnExtraJumpRefreshed(ExtraJump jump) {
    Player.canCarpet = true;
    Player.rocketTime = Player.rocketTimeMax;
    Player.wingTime = Player.wingTimeMax;
  }


  public override void FrameEffects() {
    if (!IsOri) {
      return;
    }

    if (Player.velocity.LengthSquared() > 0.2f) {
      CreatePlayerDust();
    }
  }

  public override bool FreeDodge(Player.HurtInfo info) =>
    IsOri && ActiveState is Stomp or ChargeDash or ChargeJump;

  public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
    if (!IsOri) return;

    modifiers.DisableDust();
    modifiers.DisableSound();
  }

  public override void PostHurt(Player.HurtInfo info) {
    if (!IsOri) return;

    _hurtSound.Play(Player);
  }

  public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore,
    ref PlayerDeathReason damageSource) {
    // similar to prehurt, but for death
    if (!IsOri) {
      return true;
    }

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
        new Color(255, 255, 255));
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
    }

    return true;
  }

  public override void HideDrawLayers(PlayerDrawSet drawInfo) {
    if (Main.dedServ || !Character.GraphicsEnabledCompat || !IsOri) {
      return;
    }

    if (Player.mount.Active) {
      _wasMounted = true;
    }
    else if (_wasMounted) {
      Trail.DecayAllSegments();
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

  public override void OnEnterWorld() {
    IsLocal = true;
    OriMod.ConfigClient.playerColor = SpriteColorPrimary;
    OriMod.ConfigClient.playerColorSecondary = SpriteColorSecondary;
    OriMod.ConfigClient.dyeLerp = DyeColorBlend;
  }

  public override void OnRespawn() {
    Character.Move.TriggerState<NoAbility>();
  }
}
