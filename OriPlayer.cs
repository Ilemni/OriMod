using System.Collections.Generic;
using AnimLib;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Buffs;
using OriMod.Networking;
using OriMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod {
  /// <summary>
  /// <see cref="ModPlayer"/> class for <see cref="OriMod"/>. Contains Ori data for a player, such as abilities and animations.
  /// </summary>
  public sealed class OriPlayer : ModPlayer {
    #region Variables

    /// <summary>
    /// Local instance of this OriPlayer.
    /// </summary>
    public static OriPlayer Local => Main.LocalPlayer.GetModPlayer<OriPlayer>();

    /// <summary>
    /// Manager for all <see cref="Ability"/>s on this OriPlayer instance.
    /// </summary>
    internal AbilityManager abilities { get; private set; }

    /// <summary>
    /// Net-synced controls of this player.
    /// </summary>
    internal OriInput input { get; private set; }

    /// <summary>
    /// Container for all <see cref="Animation"/>s on this OriPlayer instance.
    /// </summary>
    internal OriAnimationController Animations =>
      _anim ?? (_anim = AnimLibMod.GetAnimationController<OriAnimationController>(player));

    /// <summary>
    /// Manager for all <see cref="TrailSegment"/>s on this OriPlayer instance.
    /// </summary>
    internal Trail trail { get; private set; }

    /// <summary>
    /// Whether or not this <see cref="OriPlayer"/> instance should sync with multiplayer this frame.
    /// </summary>
    private bool _netUpdate = true;

    internal bool debugMode;

    /// <summary>
    /// Stored between <see cref="PreHurt(bool, bool, ref int, ref int, ref bool, ref bool, ref bool, ref bool, ref PlayerDeathReason)"/> and <see cref="PostHurt(bool, bool, double, int, bool)"/>, determines if custom hurt sounds are played.
    /// </summary>
    private bool _useCustomHurtSound;

    /// <summary>
    /// When set to true, uses custom movement and player sprites.
    /// <para>External mods that attempt to be compatible with this one will need to use this property.</para>
    /// </summary>
    public bool IsOri {
      get => _isOri;
      set {
        if (value == _isOri) return;
        _netUpdate = true;
        _isOri = value;
      }
    }

    /// <summary>
    /// <see langword="true"/> if this <see cref="OriPlayer"/> belongs to the local client, otherwise <see langword="false"/>.
    /// </summary>
    public bool IsLocal { get; private set; }

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
        _transforming = value;
      }
    }

    /// <summary>
    /// Frames since a transformation began.
    /// </summary>
    internal float transformTimer;

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
    /// This will still return <see langword="false"/> if there are grapples in use that do not stick to walls.
    /// </summary>
    public bool IsGrappling { get; private set; }

    /// <summary>
    /// When true, sets <see cref="Player.runSlowdown"/> to 0 every frame.
    /// </summary>
    public bool UnrestrictedMovement {
      get => _unrestrictedMovement;
      set {
        if (value == _unrestrictedMovement) return;
        _netUpdate = true;
        _unrestrictedMovement = value;
      }
    }

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
    internal int immuneTimer;

    private readonly RandomChar _randJump = new RandomChar();
    private readonly RandomChar _randHurt = new RandomChar();

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
    /// Whether or not the multiplayer client instance of this <see cref="OriPlayer"/> uses light.
    /// </summary>
    internal bool multiplayerPlayerLight = false;

    /// <summary>
    /// Whether or not this <see cref="OriPlayer"/> instance uses light.
    /// </summary>
    private bool DoPlayerLight => IsLocal || OriMod.ConfigClient.globalPlayerLight
      ? OriMod.ConfigClient.playerLight
      : multiplayerPlayerLight;

    private Color _lightColor = new Color(0.2f, 0.4f, 0.4f);

    #endregion

    #region Backing fields

    private OriAnimationController _anim;
    private bool _isOri;
    private bool _unrestrictedMovement;
    private bool _seinMinionActive;
    private int _seinMinionId;
    private int _seinMinionType;
    private bool _transforming;
    private Color _spriteColorPrimary = Color.LightCyan;
    private Color _spriteColorSecondary = Color.LightCyan;

    #endregion

    #endregion

    #region Internal Methods

    internal void PlaySound(string path, float volume = 1, float pitch = 0) {
      SoundWrapper.PlaySound((int) SoundType.Custom, (int) player.Center.X, (int) player.Center.Y,
        SoundLoader.GetSoundSlot(SoundType.Custom, "OriMod/Sounds/Custom/NewSFX/" + path), volume, pitch);
    }

    internal void PlayLocalSound(string path, float volume = 1, float pitch = 0) {
      if (IsLocal)
        SoundWrapper.PlaySound((int) SoundType.Custom, (int) player.Center.X, (int) player.Center.Y,
          SoundLoader.GetSoundSlot(SoundType.Custom, "OriMod/Sounds/Custom/NewSFX/" + path), volume, pitch);
    }

    /// <summary>
    /// Prints a debug message if "debug mode" is enabled.
    /// </summary>
    /// <param name="msg">Message to print.</param>
    internal void Debug(string msg) {
      if (debugMode && IsLocal) {
        Main.NewText(msg);
      }
    }

    /// <summary>
    /// Begins the transformation process from normal state to Spirit state.
    /// </summary>
    internal void BeginTransformation() {
      Transforming = true;
      _transformDirection = (sbyte) player.direction;
      transformTimer = 0;
    }

    /// <summary>
    /// Removes all Sein-related buffs from the player.
    /// </summary>
    internal void RemoveSeinBuffs() {
      for (int u = 1; u <= SeinData.All.Length; u++) {
        player.ClearBuff(mod.GetBuff("SeinBuff" + u).Type);
      }
    }

    /// <summary>
    /// Kills all grapples belonging to this player.
    /// </summary>
    internal void KillGrapples() {
      for (int i = 0; i < 1000; i++) {
        Projectile proj = Main.projectile[i];
        if (proj.active && proj.owner == player.whoAmI && proj.aiStyle == 7) {
          proj.Kill();
        }
      }
    }

    /// <summary>
    /// Emits a white dust speck from the player.
    /// </summary>
    internal void CreatePlayerDust() {
      if (_playerDustTimer > 0) {
        return;
      }

      Dust dust = Main.dust[
        Dust.NewDust(player.position, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255))];
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
      dust.noGravity = false;
      _playerDustTimer = Transforming ? Main.rand.Next(3, 8)
        : abilities.dash || abilities.chargeDash ? Main.rand.Next(2, 4)
        : abilities.burrow ? Main.rand.Next(6, 10)
        : Main.rand.Next(10, 15);
    }

    /// <summary>
    /// Reduces the value of <see cref="Player.gravity"/> to the given <paramref name="value"/>.
    /// If <see cref="Player.gravity"/> is lower than <paramref name="value"/>, the value is unchanged.
    /// </summary>
    /// <param name="value"></param>
    private void LowerGravityTo(float value) {
      if (value < player.gravity) {
        player.gravity = value;
      }
    }

    /// <summary>
    /// Resets the data of this <see cref="OriPlayer"/> instance.
    /// </summary>
    internal void ResetData() {
      IsOri = false;
      HasTransformedOnce = false;
      UnrestrictedMovement = false;
      SeinMinionActive = false;
      SeinMinionType = 0;
      abilities.ResetAllAbilities();
    }

    #endregion

    public override void Initialize() {
      abilities = new AbilityManager(this);
      input = new OriInput();

      if (!Main.dedServ) {
        trail = new Trail(this);
      }
    }

    public override void ResetEffects() {
      if (Transforming) {
        transformTimer += HasTransformedOnce ? RepeatedTransformRate : 1;
      }

      if (IsOri) {
        if (_playerDustTimer > 0) {
          _playerDustTimer--;
        }
      }

      if (Transforming) {
        immuneTimer = 2;
      }

      if (immuneTimer > 1) {
        immuneTimer--;
        player.immune = true;
        player.immuneNoBlink = true;
      }

      if (Main.netMode != NetmodeID.Server) {
        trail.hasDrawnThisFrame = false;
      }
    }

    /*public override void UpdateDead() {
      Abilities.soulLink.UpdateDead();
    }*/

    public override void SendClientChanges(ModPlayer clientPlayer) {
      if (_netUpdate) {
        ModNetHandler.Instance.oriPlayerHandler.SendOriState(255, player.whoAmI);
        _netUpdate = false;
      }

      abilities.SendClientChanges();
    }

    public override TagCompound Save() {
      TagCompound tag = new TagCompound {
        ["OriSet"] = IsOri,
        ["Debug"] = debugMode,
        ["Color1"] = SpriteColorPrimary,
        ["Color2"] = SpriteColorSecondary
      };
      abilities.Save(tag);
      return tag;
    }

    public override void Load(TagCompound tag) {
      IsOri = tag.GetBool("OriSet");
      debugMode = tag.GetBool("Debug");
      if (tag.ContainsKey("Color1")) {
        SpriteColorPrimary = tag.Get<Color>("Color1");
        SpriteColorSecondary = tag.Get<Color>("Color2");
      }
      else {
        _spriteColorPrimary = OriMod.ConfigClient.playerColor;
        _spriteColorSecondary = OriMod.ConfigClient.playerColorSecondary;
      }

      abilities.Load(tag);
    }

    public override void ProcessTriggers(TriggersSet triggersSet) {
      input.Update(out bool doNetUpdate);
      if (doNetUpdate) _netUpdate = true;
    }

    public override void PostUpdateMiscEffects() {
      IsGrappling = player.grappling[0] > -1;
      if (player.HasBuff(BuffID.TheTongue) || IsGrappling) {
        abilities.DisableAllAbilities();
      }
    }

    public override void PostUpdateRunSpeeds() {
      if (IsOri && !Transforming) {
        #region Default Spirit Run Speeds

        player.runAcceleration = 0.5f;
        player.maxRunSpeed += 2f;
        player.noFallDmg = true;
        LowerGravityTo(0.35f);
        player.jumpSpeedBoost += 2f;
        if (IsGrounded || player.controlLeft || player.controlRight) {
          UnrestrictedMovement = false;
        }

        player.runSlowdown = UnrestrictedMovement ? 0 : 1;

        #endregion

        if (IsLocal && OriMod.ConfigClient.smoothCamera) {
          // Smooth camera effect reduced while bosses are alive
          Main.SetCameraLerp(OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f, 1);
        }

        // Reduce gravity when clinging on wall
        if (OnWall) {
          // Either grounded or falling, not climbing
          if ((IsGrounded || player.velocity.Y * player.gravDir < 0) && !abilities.climb && !abilities.airJump) {
            LowerGravityTo(0.1f);
            player.maxFallSpeed = 6f;
            player.jumpSpeedBoost -= 6f;
          }
          // Sliding upward on wall, not stomping
          else if (!IsGrounded && player.velocity.Y * player.gravDir > 0 && !abilities.stomp && !abilities.airJump) {
            LowerGravityTo(0.1f);
            player.maxFallSpeed = 6f;
          }
        }

        abilities.Update();
      }

      if (!Transforming) return;
      player.direction = _transformDirection;
      player.controlLeft = false;
      player.controlRight = false;
      player.controlUp = false;
      player.controlDown = false;
      player.controlUseItem = false;
      if (transformTimer < TransformStartDuration - 10) {
        // Starting
        player.velocity = new Vector2(0, -0.0003f * (TransformStartDuration * 1.5f - transformTimer));
        player.gravity = 0;
        CreatePlayerDust();
      }
      else if (transformTimer < TransformStartDuration) {
        // Near end of start
        player.gravity = 9f;
        IsOri = true;
      }
      else if (transformTimer >= TransformEndDuration ||
               HasTransformedOnce && transformTimer > TransformEndEarlyDuration) {
        // End transformation
        transformTimer = 0;
        Transforming = false;
        IsOri = true;
      }

      player.runAcceleration = 0;
      player.maxRunSpeed = 0;
    }

    public override void PostUpdate() {
      if (IsOri && !Transforming) {
        HasTransformedOnce = true;
      }

      if (SeinMinionActive) {
        if (!(
          player.HasBuff(ModContent.BuffType<SeinBuff1>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff2>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff3>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff4>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff5>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff6>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff7>()) ||
          player.HasBuff(ModContent.BuffType<SeinBuff8>())
        )) {
          SeinMinionActive = false;
          SeinMinionType = 0;
        }
      }

      if (IsOri) {
      abilities.PostUpdate();

      if (DoPlayerLight && !abilities.burrow.Active) {
        Lighting.AddLight(player.Center, _lightColor.ToVector3());
      }

      if (input.jump.JustPressed && IsGrounded && !abilities.burrow) {
        PlaySound("Ori/Jump/seinJumpsGrass" + _randJump.NextNoRepeat(5), 0.6f);
      }

      bool oldGrounded = IsGrounded;
      IsGrounded = CheckGrounded();
      OnWall = CheckOnWall();

      // Footstep effects
      if (Main.dedServ || !IsGrounded) return;
      bool doDust = false;
      if (!oldGrounded) {
        doDust = true;
        FootstepManager.Instance.PlayLandingFromPlayer(player);
      }
      else if (Animations.TrackName == "Running" && (Animations.FrameIndex == 4 || Animations.FrameIndex == 9)) {
        doDust = true;
        FootstepManager.Instance.PlayFootstepFromPlayer(player);
      }

      if (doDust) {
      Vector2 dustPos = player.Bottom + new Vector2(player.direction == -1 ? -4 : 2, -2);
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
        input.ResetInputChangedState();
      }
    }

    private bool CheckGrounded() {
      float vel = player.velocity.Y * player.gravDir;
      if (vel < 0 || vel > 0.01f || abilities.climb) {
        return false;
      }

      Vector2 feetPosition = player.gravDir > 0 ? player.Bottom : player.Top;
      feetPosition.Y += 1f / 255f * player.gravDir;
      Point pos = feetPosition.ToTileCoordinates();
      // ReSharper disable once InvertIf
      if (player.fireWalk || player.waterWalk || player.waterWalk2) {
        Tile tile = Main.tile[pos.X, pos.Y];
        bool testBlock = tile.liquid > 0 && Main.tile[pos.X, pos.Y - 1].liquid == 0;
        if (testBlock && tile.lava() ? player.fireWalk : player.waterWalk || player.waterWalk2) {
          return true;
        }
      }

      return !Collision.IsClearSpotTest(player.position + new Vector2(0, 8 * player.gravDir), 16f, player.width,
        player.height, false, false, (int) player.gravDir, true, true);
    }

    private bool CheckOnWall() {
      Point p = new Vector2(
        player.Center.X + player.direction + player.direction * player.width * 0.5f,
        player.position.Y + (player.gravDir < 0f ? -1f : 2f)
      ).ToTileCoordinates();
      return WorldGen.SolidTile(p.X, p.Y + 1) && WorldGen.SolidTile(p.X, p.Y + 2);
    }

    public override void FrameEffects() {
      if (!IsOri) {
        return;
      }

      if (player.velocity.LengthSquared() > 0.2f) {
        CreatePlayerDust();
      }
    }

    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit,
      ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
      if (!IsOri) {
        return true;
      }

      genGore = false;
      if (abilities.stomp || abilities.chargeDash || abilities.chargeJump) {
        return false;
      }

      if (!playSound) return true;
      playSound = false;
      _useCustomHurtSound = true;
      UnrestrictedMovement = true;
      return true;
    }

    public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
      if (!_useCustomHurtSound) return;
      _useCustomHurtSound = false;
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

      if (!genGore) return true;
      genGore = false;
      for (int i = 0; i < 15; i++) {
        Dust dust = Dust.NewDustDirect(player.position, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0,
          new Color(255, 255, 255));
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      }

      return true;
    }

    public override void ModifyDrawLayers(List<PlayerLayer> layers) {
      if (!IsOri && !Transforming) {
        return;
      }

      #region Disable vanilla layers

      PlayerLayer.Skin.visible = false;
      PlayerLayer.Arms.visible = false;
      PlayerLayer.Body.visible = false;
      PlayerLayer.Face.visible = false;
      PlayerLayer.Head.visible = false;
      PlayerLayer.Legs.visible = false;
      PlayerLayer.WaistAcc.visible = false;
      PlayerLayer.NeckAcc.visible = false;
      PlayerLayer.ShieldAcc.visible = false;
      PlayerLayer.FaceAcc.visible = false;
      PlayerLayer.Hair.visible = false;
      PlayerLayer.ShoeAcc.visible = false;
      PlayerLayer.HandOnAcc.visible = false;
      PlayerLayer.HandOffAcc.visible = false;
      if (OnWall || Transforming || abilities.stomp || abilities.airJump || abilities.burrow || abilities.chargeJump ||
          abilities.wallChargeJump) {
        PlayerLayer.Wings.visible = false;
      }

      #endregion

      /*if (Abilities.soulLink.PlacedSoulLink) {
        layers.Insert(0, OriLayers.Instance.SoulLinkLayer);
      }*/
      int idx = layers.IndexOf(PlayerLayer.FaceAcc);

      if (IsOri) {
        if (Animations.playerAnim.Valid && !abilities.burrow && !player.mount.Active) {
          layers.Insert(idx++, OriLayers.Instance.trailLayer);
        }

        if (abilities.glide) {
          layers.Insert(idx++, OriLayers.Instance.featherSprite);
        }

        if (abilities.bash || abilities.launch.Starting) {
          layers.Insert(idx++, OriLayers.Instance.bashArrow);
        }
      }

      if (!player.dead && !player.invis && Animations.playerAnim.Valid) {
        layers.Insert(idx, OriLayers.Instance.playerSprite);
      }

      player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
      OriLayers.Instance.trailLayer.visible =
        OriLayers.Instance.playerSprite.visible && !abilities.burrow && !player.mount.Active;
    }

    public override void OnEnterWorld(Player p) {
      // ...can't we just use implicit "this"? Is GetModPlayer necessary here?
      // *checks decompiler*
      // "player.modPlayers[index].OnEnterWorld(player);"
      // ...completely unnecessary. Player p == this.player;
      OriPlayer oPlayer = p.GetModPlayer<OriPlayer>();
      oPlayer.IsLocal = true;
      oPlayer.SeinMinionActive = false;
      oPlayer.SeinMinionType = 0;
      OriMod.ConfigClient.playerColor = oPlayer.SpriteColorPrimary;
      OriMod.ConfigClient.playerColorSecondary = oPlayer.SpriteColorSecondary;
    }

    public override void OnRespawn(Player p) {
      abilities.DisableAllAbilities();
    }
  }
}