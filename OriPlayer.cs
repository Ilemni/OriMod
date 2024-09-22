using AnimLib;
using AnimLib.Abilities;
using AnimLib.Extensions;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Networking;
using OriMod.Utilities;
using System;
using System.Linq;
using JetBrains.Annotations;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace OriMod;

/// <summary>
/// <see cref="ModPlayer"/> class for <see cref="OriMod"/>. Contains Ori data for a player, such as abilities and animations.
/// </summary>
[UsedImplicitly]
public sealed class OriPlayer : ModPlayer {
  private const string AbilitiesTagName = "AnimLibAbilities";

  #region Variables

  /// <summary>
  /// Local instance of this OriPlayer.
  /// </summary>
  public static OriPlayer Local => Main.LocalPlayer.GetModPlayer<OriPlayer>();

  private bool InMenu => Main.ingameOptionsWindow || Main.inFancyUI || Player.talkNPC >= 0 || Player.sign >= 0 || Main.clothesWindow || Main.playerInventory;

  /// <summary>
  /// Net-synced controls of this player.
  /// </summary>
  internal OriInput Input { get; private set; }

  internal bool ControlsBlocked;

  private AnimCharacterWrapper<OriAnimationController, OriAbilityManager> _characterWrapper;

  internal AnimCharacterWrapper<OriAnimationController, OriAbilityManager> CharacterWrapper =>
    _characterWrapper ??= this.GetAnimCharacter().GetWrapped<OriAnimationController, OriAbilityManager>();

  public AnimCharacter Character => CharacterWrapper.Character;

  /// <summary>
  /// Container for all <see cref="Animation"/>s on this OriPlayer instance.
  /// </summary>
  internal OriAnimationController Animations => CharacterWrapper.AnimationController;

  /// <summary>
  /// Manager for all <see cref="Ability"/>s on this OriPlayer instance.
  /// </summary>
  internal OriAbilityManager Abilities => CharacterWrapper.AbilityManager;

  /// <summary>
  /// Manager for all <see cref="TrailSegment"/>s on this OriPlayer instance.
  /// </summary>
  internal Trail Trail { get; private set; }

  /// <summary>
  /// Whether this <see cref="OriPlayer"/> instance should sync with multiplayer this frame.
  /// </summary>
  private bool _netUpdate = true;

  internal bool DebugMode;

  private bool _wasMounted;

  /// <summary>
  /// When set to true, uses custom movement and player sprites.
  /// <para>External mods that attempt to be compatible with this one will need to use this property.</para>
  /// </summary>
  public bool IsOri {
    get => Character.IsActive && (!Transforming || TransformTimer >= TransformStartDuration - 10);
    set {
      if (value == _isOri) return;
      _netUpdate = true;
      if (value) Character.TryEnable();
      else Character.TryDisable();
      _isOri = value;
    }
  }

  /// <summary>
  /// <see langword="true"/> if this <see cref="OriPlayer"/> belongs to the local client, otherwise <see langword="false"/>.
  /// </summary>
  public bool IsLocal { get; private set; }

  /// <summary>
  /// Stores previous armor dye item ID, from armor dye slot, used for armor shader update.
  /// </summary>
  internal int ArmorDye;

  /// <summary>
  /// Current dye_shader data, used for dye shader base color extraction.
  /// </summary>
  internal ArmorShaderData DyeShader;

  #region Transformation

  /// <summary>
  /// Represents if the player is currently transforming into Ori.
  /// </summary>
  /// <remarks>While transforming, all player input is disabled.</remarks>
  public bool Transforming {
    get => _transforming;
    internal set {
      if (value == _transforming) return;
      _netUpdate = true;
      if (value) Character.TryEnable();
      _transforming = value;
    }
  }

  /// <summary>
  /// Frames since a transformation began.
  /// </summary>
  internal float TransformTimer;

  /// <summary>
  /// Whether the player has been Ori before during this session. Used to hasten subsequent transformations.
  /// </summary>
  internal bool HasTransformedOnce { get; private set; }

  /// <summary>
  /// Speed multiplier to play subsequent transformations at.
  /// </summary>
  internal static float RepeatedTransformRate => 2.5f;

  /// <summary>
  /// Direction a transformation began at. Facing direction is locked until the transformation ends.
  /// </summary>
  private sbyte _transformDirection;

  /// <summary>
  /// Duration of Transformation where <see cref="IsOri"/> stays false.
  /// </summary>
  private static int TransformStartDuration => 392;

  /// <summary>
  /// Total duration of Transformation before <see cref="Transforming"/> ends.
  /// </summary>
  private static int TransformEndDuration => TransformStartDuration + 225;

  /// <summary>
  /// Shorter duration of Transformation, for cancelling the animation early on subsequent transformations.
  /// </summary>
  private static int TransformEndEarlyDuration => TransformStartDuration + 68;

  #endregion

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
  /// Info about if this player has a <see cref="Projectiles.Minions.Sein"/> minion summoned. Used to prevent having more than one Sein summoned per player.
  /// </summary>
  public bool SeinMinionActive {
    get => _seinMinionActive;
    set {
      if (value == _seinMinionActive) return;
      _netUpdate = true;
      _seinMinionActive = value;
    }
  }

  /// <summary>
  /// The ID of the <see cref="Projectiles.Minions.Sein"/> that is summoned. This is the <see cref="Entity.whoAmI"/> of the Sein projectile.
  /// </summary>
  public int SeinMinionId {
    get => _seinMinionId;
    internal set {
      if (value == _seinMinionId) return;
      _netUpdate = true;
      _seinMinionId = value;
    }
  }

  /// <summary>
  /// The current type of <see cref="Projectiles.Minions.Sein"/> that is summoned. Used to prevent re-summons of the same tier of Sein.
  /// </summary>
  public int SeinMinionType {
    get => _seinMinionType;
    internal set {
      if (value == _seinMinionType) return;
      _netUpdate = true;
      _seinMinionType = value;
    }
  }

  /// <summary>
  /// Used for <see cref="CreatePlayerDust"/>.
  /// </summary>
  private int _playerDustTimer;

  /// <summary>
  /// When greater than 0, sets <see cref="Player.immune"/> to true.
  /// </summary>
  internal int ImmuneTimer {
    get => Player.immuneTime;
    set {
      if (Player.immuneTime < value) Player.immuneTime = value;
      Player.immune = true;
      Player.immuneNoBlink = true;
    }
  }

  private RandomChar _randJump;
  private RandomChar _randHurt;

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

  #endregion

  #region Backing fields

  private OriAnimationController _anim;
  private bool _isOri;
  private bool _seinMinionActive;
  private int _seinMinionId;
  private int _seinMinionType;
  private bool _transforming;
  private Color _spriteColorPrimary = Color.LightCyan;
  private Color _spriteColorSecondary = Color.LightCyan;
  private float _dyeColorBlend = 0.65f;

  #endregion

  #endregion

  #region Internal Methods

  internal void PlaySound(string path, float volume = 1, float pitch = 0) {
    SoundWrapper.PlaySound(Player.Center, path, out SoundStyle _, volume, pitch);
  }

  internal void PlayLocalSound(string path, float volume = 1, float pitch = 0) {
    if (IsLocal)
      SoundWrapper.PlaySound(Player.Center, path, out SoundStyle _, volume, pitch);
  }

  /// <summary>
  /// Prints a debug message if "debug mode" is enabled.
  /// </summary>
  /// <param name="msg">Message to print.</param>
  internal void Debug(string msg) {
    if (DebugMode && IsLocal) {
      Main.NewText(msg);
    }
  }

  /// <summary>
  /// Begins the transformation process from normal state to Spirit state.
  /// </summary>
  internal void BeginTransformation() {
    Transforming = true;
    _transformDirection = (sbyte)Player.direction;
    TransformTimer = 0;
  }

  /// <summary>
  /// Removes all Sein-related buffs from the player.
  /// </summary>
  internal void RemoveSeinBuffs() {
    for (int u = 0; u < SeinData.All.Length; u++) {
      Player.ClearBuff(SeinData.SeinBuffs[u]);
    }
  }

  /// <summary>
  /// Kills all grapples belonging to this player.
  /// </summary>
  internal void KillGrapples() {
    for (int i = 0; i < 1000; i++) {
      Projectile proj = Main.projectile[i];
      if (proj.active && proj.owner == Player.whoAmI && proj.aiStyle == ProjAIStyleID.Hook) {
        proj.Kill();
      }
    }
  }

  /// <summary>
  /// Emits a white dust speck from the player.
  /// </summary>
  internal void CreatePlayerDust() {
    if (Main.dedServ || _playerDustTimer > 0 || !Animations.GraphicsEnabledCompat || _lightStrength < 0.1f) {
      return;
    }

    Dust dust = Main.dust[
      Dust.NewDust(Player.position, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255))];
    dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
    dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
    dust.noGravity = false;
    _playerDustTimer = Transforming ? Main.rand.Next(3, 8)
      : Abilities.Dash || Abilities.ChargeDash ? Main.rand.Next(2, 4)
      : Abilities.Burrow ? Main.rand.Next(6, 10)
      : Main.rand.Next(10, 15);
  }

  /// <summary>
  /// Reduces the value of <see cref="Player.gravity"/> to the given <paramref name="value"/>.
  /// If <see cref="Player.gravity"/> is lower than <paramref name="value"/>, the value is unchanged.
  /// </summary>
  /// <param name="value"></param>
  private void LowerGravityTo(float value) {
    if (value < Player.gravity) {
      Player.gravity = value;
    }
  }

  /// <summary>
  /// Resets the data of this <see cref="OriPlayer"/> instance.
  /// </summary>
  internal void ResetData() {
    IsOri = false;
    HasTransformedOnce = false;
    SeinMinionActive = false;
    SeinMinionType = 0;
    Abilities.ResetAllAbilities();
  }

  #endregion

  public override void Initialize() {
    Input = new OriInput();

    if (!Main.dedServ) {
      Trail = new Trail(this);
    }
  }

  public override void ResetEffects() {
    if (Transforming) {
      TransformTimer += HasTransformedOnce ? RepeatedTransformRate : 1;
    }

    if (IsOri) {
      if (_playerDustTimer > 0) {
        _playerDustTimer--;
      }
    }

    if (Transforming) {
      ImmuneTimer = 2;
    }

    if (Main.netMode != NetmodeID.Server) {
      Trail.HasDrawnThisFrame = false;
    }
  }

  /*public override void UpdateDead() {
    Abilities.soulLink.UpdateDead();
  }*/

  // TODO: Consider thinking about standardizing network operations for Ori state.
  public override void CopyClientState(ModPlayer clientClone) {
    base.CopyClientState(clientClone);
  }

  public override void SendClientChanges(ModPlayer clientPlayer) {
    if (_netUpdate) {
      ModNetHandler.Instance.OriPlayerHandler.SendOriState(255, Player.whoAmI);
      _netUpdate = false;
    }
  }

  public override void SaveData(TagCompound tag) {
    tag["OriSet"] = IsOri;
    tag["Debug"] = DebugMode;
    tag["Color1"] = SpriteColorPrimary;
    tag["Color2"] = SpriteColorSecondary;
    tag["DyeColLerp"] = DyeColorBlend;

    //Backward compatibility don't pay attention
    //TODO: Remove old save data once ready
    Abilities.OldSave(tag);

    tag[AbilitiesTagName] = Abilities.Save();
  }

  public override void LoadData(TagCompound tag) {
    IsOri = tag.GetBool("OriSet");
    DebugMode = tag.GetBool("Debug");
    _spriteColorPrimary = tag.TryGet("Color1", out Color color1) ? color1 : OriMod.ConfigClient.playerColor;
    _spriteColorSecondary = tag.TryGet("Color2", out Color color2) ? color2 : OriMod.ConfigClient.playerColorSecondary;
    _dyeColorBlend = tag.TryGet("DyeColLerp", out float blend) ? blend : OriMod.ConfigClient.dyeLerp;

    //Backward compatibility don't pay attention
    Abilities.OldLoad(tag);

    //This is current version
    if (tag.ContainsKey(AbilitiesTagName))
      Abilities.Load(tag.GetCompound(AbilitiesTagName));

    //Backward compatibility don't pay attention
    if (Abilities.OldAbility is not null) {
      foreach (Ability ability in Abilities) {
        if (ability is ILevelable { Level: 0 } levelable) {
          levelable.Level = Abilities.OldAbility[ability.Id];
        }
      }

      Abilities.OldAbility = null;
    }
    //Backward compatibility don't pay attention
  }

  public override void ProcessTriggers(TriggersSet triggersSet) {
    if (IsLocal) {
      ControlsBlocked = OriMod.ConfigClient.blockControlsInMenu && InMenu;
    }

    Input.Update(out bool doNetUpdate);
    if (doNetUpdate) _netUpdate = true;
  }

  public override void PostUpdateMiscEffects() {
    IsGrappling = Player.grappling[0] > -1;
    if (Player.HasBuff(BuffID.TheTongue) || IsGrappling) {
      Abilities.DisableAllAbilities();
    }
  }

  public override void UpdateEquips() {
    if (Abilities.Climb || Abilities.WallChargeJump) {
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
    if (Abilities.AirJump.CanUse || Abilities.AirJump.InUse || OnWall) {
      _rocketBootsRemaining = Player.rocketTime;
      Player.rocketTime = 0;
    }

    if (Abilities.AirJump.InUse || OnWall) {
      _carpetRemaining = Player.carpetTime;
      Player.carpetTime = 0;
    }

    ExtraJumpState dummyJump = Player.GetJumpState<DummyJump>();
    dummyJump.Enable();
    dummyJump.Available = false;
  }

  public void PostUpdatePhysics() {
    if (IsOri && !Transforming) {
      #region Default Spirit Run Speeds

      Player.maxRunSpeed += 2f;
      Player.noFallDmg = true;
      LowerGravityTo(0.35f);
      Player.jumpSpeedBoost += 2f;

      if (IsGrounded) {
        Player.runAcceleration = Math.Max(Math.Min(MathF.Pow(Player.runAcceleration, 3f) * 980f, 0.5f),
          Player.runAcceleration);
        Player.runSlowdown = Math.Max(Math.Min(MathF.Pow(Player.runSlowdown, 2f) * 25f, 1f), Player.runSlowdown);
      }
      else {
        Player.runAcceleration = Player.runAcceleration is > 0.01f and < 0.3f ? 0.3f : Player.runAcceleration;
        Player.runSlowdown = Player.runAcceleration is > 0.01f and < 0.5f ? 0.5f : Player.runAcceleration;
      }

      #endregion

      if (IsLocal && OriMod.ConfigClient.smoothCamera) {
        // Smooth camera effect reduced while bosses are alive
        Main.SetCameraLerp(OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f, 1);
      }

      // Reduce gravity when clinging on wall
      if (OnWall) {
        Player.blockExtraJumps = true;
        // Either grounded or falling, not climbing
        if ((IsGrounded || Player.velocity.Y * Player.gravDir < 0) && !Abilities.Climb && !Abilities.AirJump) {
          LowerGravityTo(0.1f);
          Player.maxFallSpeed = 6f;
          Player.jumpSpeedBoost -= 6f;
        }
        // Sliding upward on wall, not stomping
        else if (!IsGrounded && Player.velocity.Y * Player.gravDir > 0 && !Abilities.Stomp && !Abilities.AirJump) {
          LowerGravityTo(0.1f);
          Player.maxFallSpeed = 6f;
        }
      }
    }

    if (!Transforming) return;
    Player.direction = _transformDirection;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlJump = false;
    Player.controlMount = false;
    Player.controlHook = false;
    Player.controlUseItem = false;
    if (TransformTimer < TransformStartDuration - 10) {
      // Starting
      Player.velocity = new Vector2(0, -0.0003f * (TransformStartDuration * 1.5f - TransformTimer));
      Player.gravity = 0;
      CreatePlayerDust();
    }
    else if (TransformTimer < TransformStartDuration) {
      // Near end of start
      Player.gravity = 9f;
      IsOri = true;
    }
    else if (TransformTimer >= TransformEndDuration ||
      HasTransformedOnce && TransformTimer > TransformEndEarlyDuration) {
      // End transformation
      TransformTimer = 0;
      Transforming = false;
      IsOri = true;
    }

    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
  }

  public override void PostUpdate() {
    if (IsOri && !Transforming) {
      HasTransformedOnce = true;
    }

    if (SeinMinionActive) {
      if (!SeinData.SeinBuffs.Any(Player.HasBuff)) {
        SeinMinionActive = false;
        SeinMinionType = 0;
      }
    }

    if (IsOri) {
      if (DoPlayerLight && !Abilities.Burrow.Active) {
        if (Main.dontStarveWorld) _lightStrength -= 0.004f;
        Lighting.AddLight(Player.Center, _lightColor.ToVector3() * _lightStrength);
        Vector3 tileLight = Lighting.GetColor(Player.Center.ToTileCoordinates()).ToVector3();
        float brightness = tileLight.X + tileLight.Y + tileLight.Z;
        _lightStrength = Math.Min(Math.Max(_lightStrength, brightness / 2), 1f);
      }

      if (!Main.dedServ && !Transforming && Animations.GraphicsEnabledCompat && Input.Jump.JustPressed && IsGrounded && !Abilities.Burrow) {
        PlaySound("Ori/Jump/seinJumpsGrass" + _randJump.NextNoRepeat(5), 0.6f);
      }

      bool oldGrounded = IsGrounded;
      IsGrounded = CheckGrounded();
      OnWall = CheckOnWall();

      if (IsGrounded || OnWall) RestoreAirJumps();

      // Footstep effects
      if (Main.dedServ || !IsGrounded) return;
      bool doDust = false;
      if (!oldGrounded && Animations.GraphicsEnabledCompat) {
        doDust = true;
        FootstepManager.Instance.PlayLandingFromPlayer(Player, out SoundStyle _);
      }
      else if (Animations.TagName == "Running" && Animations.FrameIndex is 4 or 9
               && Animations.GraphicsEnabledCompat) {
        doDust = true;
        FootstepManager.Instance.PlayFootstepFromPlayer(Player, out SoundStyle _);
      }

      if (doDust && _lightStrength > 0.1f) {
        Vector2 dustPos = Player.Bottom + new Vector2(Player.direction == -1 ? -4 : 2, -2);
        for (int i = 0; i < 4; i++) {
          Dust dust = Main.dust[
            Dust.NewDust(dustPos, 2, 2, DustID.Clentaminator_Cyan, 0f, -2.7f, 0, new Color(255, 255, 255))];
          dust.noGravity = true;
          dust.scale = 0.75f;
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          dust.shader.UseColor(Color.White);
          dust.fadeIn = 0.03947368f;
        }
      }
    }

    if (!IsLocal) {
      Input.ResetInputChangedState();
    }
  }

  private static Type _starlightRiverBasePlatform;

  private bool CheckGrounded() {
    float vel = Player.velocity.Y * Player.gravDir;
    if (vel < 0 || vel > 0.01f || Abilities.Climb) {
      return false;
    }

    Vector2 feetPosition = Player.gravDir > 0 ? Player.Bottom : Player.Top;
    feetPosition.Y += 1f / 255f * Player.gravDir;
    Point pos = feetPosition.ToTileCoordinates();
    // ReSharper disable once InvertIf
    if (Player.fireWalk || Player.waterWalk || Player.waterWalk2) {
      Tile tile = Main.tile[pos.X, pos.Y];
      bool testBlock = tile.LiquidAmount > 0 && Main.tile[pos.X, pos.Y - 1].LiquidAmount == 0;
      if (testBlock && (tile.LiquidType == LiquidID.Lava ? Player.fireWalk : Player.waterWalk || Player.waterWalk2)) {
        return true;
      }
    }

    if (_starlightRiverBasePlatform is not null) {
      Rectangle playerRect = new((int)Player.position.X, (int)Player.position.Y + Player.height, Player.width, 1);
      foreach (NPC npc in Main.npc) {
        if (!npc.active || !_starlightRiverBasePlatform.IsInstanceOfType(npc.ModNPC)) {
          continue;
        }

        Rectangle npcRect = new((int)npc.position.X, (int)npc.position.Y, npc.width,
          8 + (Player.velocity.Y > 0 ? (int)Player.velocity.Y : 0) + (int)Math.Abs(npc.velocity.Y));

        if (playerRect.Intersects(npcRect) && Player.position.Y <= npc.position.Y)
          return true;
      }
    }

    return !Collision.IsClearSpotTest(Player.position + new Vector2(0, 8 * Player.gravDir), 16f, Player.width,
      Player.height, false, false, (int)Player.gravDir, true, true);
  }

  private bool CheckOnWall() {
    Point p = new Vector2(
      Player.Center.X + Player.direction + Player.direction * Player.width * 0.5f,
      Player.position.Y + (Player.gravDir < 0f ? -1f : 2f)
    ).ToTileCoordinates();
    return WorldGen.SolidTile(p.X, p.Y + 1) && WorldGen.SolidTile(p.X, p.Y + 2);
  }

  /// <summary>
  /// Refreshes your airborne abilities, allowing you to jump and dash again before touching the ground.
  /// </summary>
  public void RestoreAirJumps() {
    Player.RefreshExtraJumps();
  }

  public override void OnExtraJumpRefreshed(ExtraJump jump) {
    Abilities.AirJump.CurrentCount = 0;
    Abilities.Dash.CurrentCount = 0;
    Abilities.Launch.CurrentChain = 0;
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
    IsOri && (Abilities.Stomp || Abilities.ChargeDash || Abilities.ChargeJump);

  public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
    if (!IsOri) return;

    modifiers.DisableDust();
    modifiers.DisableSound();
  }

  public override void PostHurt(Player.HurtInfo info) {
    if (!IsOri) return;

    PlaySound("Ori/Hurt/seinHurtRegular" + _randHurt.NextNoRepeat(4), 0.75f);
  }

  public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore,
    ref PlayerDeathReason damageSource) {
    // similar to prehurt, but for death
    if (!IsOri) return true;
    if (playSound) {
      playSound = false;
      switch (damageSource.SourceOtherIndex) {
        case 1:
          PlaySound("Ori/Death/seinSwimmingDrowningDeath" + RandomChar.Next(3));
          break;
        case 2:
          PlaySound("Ori/Death/seinDeathLava" + RandomChar.Next(5));
          break;
        default:
          PlaySound("Ori/Death/seinDeathRegular" + RandomChar.Next(5));
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
    if (Player.mount.Active) {
      _wasMounted = true;
    }

    if (Main.dedServ || !Animations.GraphicsEnabledCompat) return;
    if (!IsOri && !Transforming) {
      OriLayers.OriSprite.Hide();
      OriLayers.OriTrail.Hide();
      OriLayers.BashArrow.Hide();
      return;
    }

    if (Player.dead || Player.invis) {
      OriLayers.OriSprite.Hide();
    }

    if (!Player.mount.Active) {
      if (_wasMounted) Trail.DecayAllSegments();
      _wasMounted = false;
    }

    if (Abilities.Burrow || Player.mount.Active) {
      OriLayers.OriTrail.Hide();
    }

    if (!Abilities.Bash && !Abilities.Launch.Starting) {
      OriLayers.BashArrow.Hide();
    }

    #region Disable vanilla layers

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

    if (OnWall || Transforming || Abilities.Stomp ||
        (Abilities.AirJump && !Abilities.Glide) ||
        Abilities.Burrow || Abilities.ChargeJump ||
        Abilities.WallChargeJump) {
      PlayerDrawLayers.HeldItem.Hide();
      PlayerDrawLayers.Wings.Hide();
      PlayerDrawLayers.Backpacks.Hide();
      PlayerDrawLayers.ForbiddenSetRing.Hide();
      PlayerDrawLayers.LeinforsHairShampoo.Hide();
      //PlayerDrawLayers.EyebrellaCloud.Hide();
      PlayerDrawLayers.SolarShield.Hide();
      //PlayerDrawLayers.PortableStool.Hide();
    }

    #endregion
  }

  public override void OnEnterWorld() {
    IsLocal = true;
    SeinMinionActive = false;
    SeinMinionType = 0;
    OriMod.ConfigClient.playerColor = SpriteColorPrimary;
    OriMod.ConfigClient.playerColorSecondary = SpriteColorSecondary;
    OriMod.ConfigClient.dyeLerp = DyeColorBlend;

    if (!IsLocal) {
      return;
    }

    Mod mod = ModLoader.Mods.FirstOrDefault(x => x.Name == "StarlightRiver");
    if (mod is not null) {
      _starlightRiverBasePlatform = AssemblyManager.GetLoadableTypes(mod.Code)
        .FirstOrDefault(x => x.FullName == "StarlightRiver.Content.NPCs.BaseTypes.MovingPlatform", null);
    }

    if (!Main.dedServ) {
      OriTextures.Instance.BurrowTimer.Wait();
      OriTextures.Instance.Sein.Wait();
    }
  }

  public override void OnRespawn() {
    Abilities.DisableAllAbilities();
  }
}

/// <summary>
/// Dummy <see cref="ExtraJump"/>, forces <see cref="OriPlayer.OnExtraJumpRefreshed"/> to be called even if there's no other enabled double jumps
/// </summary>
public class DummyJump : ExtraJump {
  public override Position GetDefaultPosition() => AfterBottleJumps;
  public override float GetDurationMultiplier(Player player) => 0f;
  public override bool CanStart(Player player) => false;
}
