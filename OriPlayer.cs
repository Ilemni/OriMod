using System.Collections.Generic;
using AnimLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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
    /// Container for all <see cref="Animation"/>s on this OriPlayer instance.
    /// </summary>
    internal OriAnimationController animations => _anim ?? (_anim = AnimLibMod.GetAnimationController<OriAnimationController>(player));
    private OriAnimationController _anim;
    /// <summary>
    /// Manager for all <see cref="TrailSegment"/>s on this OriPlayer instance.
    /// </summary>
    internal Trail trail { get; private set; }

    /// <summary>
    /// Whether or not this <see cref="OriPlayer"/> instance should sync with multiplayer this frame.
    /// </summary>
    internal bool netUpdate = false;

    internal bool debugMode = false;

    /// <summary>
    /// Stored between <see cref="PreHurt(bool, bool, ref int, ref int, ref bool, ref bool, ref bool, ref bool, ref PlayerDeathReason)"/> and <see cref="PostHurt(bool, bool, double, int, bool)"/>, determines if custom hurt sounds are played.
    /// </summary>
    private bool useCustomHurtSound = false;

    /// <summary>
    /// When set to true, uses custom movement and player sprites.
    /// <para>External mods that attempt to be compatible with this one will need to use this property.</para>
    /// </summary>
    public bool IsOri {
      get => _isOri;
      set {
        if (value != _isOri) {
          netUpdate = true;
          _isOri = value;
        }
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
        if (value != _transforming) {
          netUpdate = true;
          _transforming = value;
        }
      }
    }

    /// <summary>
    /// Frames since a transformation began.
    /// </summary>
    internal float transformTimer = 0;

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
    private sbyte transformDirection = 0;

    /// <summary>
    /// Duration of Transformation where <see cref="IsOri"/> stays false.
    /// </summary>
    private int TransformStartDuration => 392;

    /// <summary>
    /// Total duration of Transformation before <see cref="Transforming"/> ends.
    /// </summary>
    private int TransformEndDuration => TransformStartDuration + 225;

    /// <summary>
    /// Shorter duration of Transformation, for cancelling the animation early on subsequent transformations.
    /// </summary>
    private int TransformEndEarlyDuration => TransformStartDuration + 68;
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
    /// When true, sets <see cref="Player.runSlowdown"/> to 0 every frame.
    /// </summary>
    public bool UnrestrictedMovement {
      get => _unrestrictedMovement;
      set {
        if (value != _unrestrictedMovement) {
          netUpdate = true;
          _unrestrictedMovement = value;
        }
      }
    }

    /// <summary>
    /// Synced input for pressing Jump.
    /// </summary>
    public bool justPressedJumped {
      get => _justJumped;
      set {
        if (value != _justJumped) {
          _justJumped = value;
          netUpdate = true;
        }
      }
    }

    /// <summary>
    /// Synced input for using <see cref="Glide"/>.
    /// </summary>
    public bool featherKeyDown {
      get => _featherKeyDown;
      set {
        if (value != _featherKeyDown) {
          _featherKeyDown = value;
          netUpdate = true;
        }
      }
    }

    /// <summary>
    /// Info about if this player has a <see cref="Projectiles.Minions.Sein"/> minion summoned. Used to prevent having more than one Sein summoned per player.
    /// </summary>
    public bool SeinMinionActive {
      get => _seinMinionActive;
      internal set {
        if (value != _seinMinionActive) {
          netUpdate = true;
          _seinMinionActive = value;
        }
      }
    }

    /// <summary>
    /// The ID of the <see cref="Projectiles.Minions.Sein"/> that is summoned. This is the <see cref="Entity.whoAmI"/> of the Sein projectile.
    /// </summary>
    public int SeinMinionID {
      get => _seinMinionID;
      internal set {
        if (value != _seinMinionID) {
          netUpdate = true;
          _seinMinionID = value;
        }
      }
    }

    /// <summary>
    /// The current type of <see cref="Projectiles.Minions.Sein"/> that is summoned. Used to prevent re-summons of the same tier of Sein.
    /// </summary>
    public int SeinMinionType {
      get => _seinMinionType;
      internal set {
        if (value != _seinMinionType) {
          netUpdate = true;
          _seinMinionType = value;
        }
      }
    }

    /// <summary>
    /// Used for <see cref="CreatePlayerDust"/>.
    /// </summary>
    private int playerDustTimer = 0;

    /// <summary>
    /// When greater than 0, sets <see cref="Player.immune"/> to true.
    /// </summary>
    internal int immuneTimer = 0;

    private readonly RandomChar randJump = new RandomChar();
    private readonly RandomChar randHurt = new RandomChar();

    #region Aesthetics
    /// <summary>
    /// Primary color of the Ori sprite for this instance of <see cref="OriPlayer"/>.
    /// </summary>
    public Color SpriteColorPrimary {
      get => _spriteColorPrimary;
      set {
        _spriteColorPrimary = value;
        if (IsLocal) {
          OriMod.ConfigClient.PlayerColor = value;
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
          OriMod.ConfigClient.PlayerColorSecondary = value;
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
    internal bool DoPlayerLight => (IsLocal || OriMod.ConfigClient.GlobalPlayerLight) ? OriMod.ConfigClient.PlayerLight : multiplayerPlayerLight;
    public Color LightColor = new Color(0.2f, 0.4f, 0.4f);

    /// <summary>
    /// For making the player sprite appear red during hurt animations.
    /// </summary>
    internal bool flashing = false;
    #endregion

    #region Backing fields
    private bool _isOri;
    private bool _unrestrictedMovement = false;
    private bool _seinMinionActive = false;
    private int _seinMinionID;
    private int _seinMinionType = 0;
    private bool _transforming = false;
    private Color _spriteColorPrimary = Color.LightCyan;
    private Color _spriteColorSecondary = Color.LightCyan;
    private bool _justJumped;
    private bool _featherKeyDown;
    #endregion
    #endregion

    #region Internal Methods
    internal SoundEffectInstance PlayNewSound(string Path, float Volume = 1, float Pitch = 0, bool localOnly = false) {
      if (localOnly && !IsLocal) {
        return null;
      }
      return Main.PlaySound((int)SoundType.Custom, (int)player.Center.X, (int)player.Center.Y, SoundLoader.GetSoundSlot(SoundType.Custom, "OriMod/Sounds/Custom/NewSFX/" + Path), Volume, Pitch);
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
      transformDirection = (sbyte)player.direction;
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
      if (playerDustTimer > 0) {
        return;
      }

      Dust dust = Main.dust[Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
      dust.noGravity = false;
      playerDustTimer = Transforming ? Main.rand.Next(3, 8)
        : abilities.dash.InUse || abilities.chargeDash.InUse ? Main.rand.Next(2, 4)
        : abilities.burrow.InUse ? Main.rand.Next(6, 10)
        : Main.rand.Next(10, 15);
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
    }
    #endregion

    public override void Initialize() {
      abilities = new AbilityManager(this);

      if (!Main.dedServ) {
        trail = new Trail(this, 26);
      }
    }

    public override void ResetEffects() {
      if (Transforming) {
        transformTimer += HasTransformedOnce ? RepeatedTransformRate : 1;
      }
      if (IsOri) {
        if (playerDustTimer > 0) {
          playerDustTimer--;
        }
      }
    }

    /*public override void UpdateDead() {
      Abilities.soulLink.UpdateDead();
    }*/

    public override void SendClientChanges(ModPlayer clientPlayer) {
      if (netUpdate) {
        ModNetHandler.Instance.oriPlayerHandler.SendOriState(255, player.whoAmI);
        netUpdate = false;
      }
      abilities.SendClientChanges();
    }

    public override TagCompound Save() {
      var tag = new TagCompound {
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
        _spriteColorPrimary = OriMod.ConfigClient.PlayerColor;
        _spriteColorSecondary = OriMod.ConfigClient.PlayerColorSecondary;
      }
      abilities.Load(tag);
    }

    public override void ProcessTriggers(TriggersSet triggersSet) {
      justPressedJumped = PlayerInput.Triggers.JustPressed.Jump;
      featherKeyDown = OriMod.FeatherKey.Current;
    }

    public override void PostUpdateMiscEffects() {
      if (player.HasBuff(BuffID.TheTongue)) {
        abilities.DisableAllAbilities();
      }
    }

    public override void PostUpdateRunSpeeds() {
      if (IsOri && !Transforming) {
        #region Default Spirit Run Speeds
        player.runAcceleration = 0.5f;
        player.maxRunSpeed += 2f;
        player.noFallDmg = true;
        player.gravity = 0.35f;
        player.jumpSpeedBoost += 2f;
        if (IsGrounded || player.controlLeft || player.controlRight) {
          UnrestrictedMovement = false;
        }
        player.runSlowdown = UnrestrictedMovement ? 0 : 1;
        #endregion

        if (OriMod.ConfigClient.SmoothCamera) {
          // Smooth camera effect reduced while bosses are alive
          Main.SetCameraLerp(OriUtils.AnyBossAlive() ? 0.15f : 0.05f, 1);
        }

        // Reduce gravity when clinging on wall
        if (OnWall) {
          // Either grounded or falling, not climbing
          if ((IsGrounded || player.velocity.Y * player.gravDir < 0) && !abilities.climb.InUse) {
            player.gravity = 0.1f;
            player.maxFallSpeed = 6f;
            player.jumpSpeedBoost -= 6f;
          }
          // Sliding upward on wall, not stomping
          else if (!IsGrounded && player.velocity.Y * player.gravDir > 0 && !abilities.stomp.InUse) {
            player.gravity = 0.1f;
            player.maxFallSpeed = 6f;
          }
        }

        abilities.Update();
      }

      if (Transforming) {
        player.direction = transformDirection;
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
        else if (transformTimer >= TransformEndDuration || HasTransformedOnce && transformTimer > TransformEndEarlyDuration) {
          // End transformation
          transformTimer = 0;
          Transforming = false;
          IsOri = true;
        }
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.immune = true;
      }

      if (immuneTimer > 0) {
        immuneTimer--;
        player.immune = true;
      }
    }

    public override void PostUpdate() {
      if (IsOri && !Transforming) {
        HasTransformedOnce = true;
      }
      #region Check Sein Buffs
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
      #endregion

      if (!IsOri) {
        return;
      }

      if (DoPlayerLight && !abilities.burrow.Active) {
        Lighting.AddLight(player.Center, LightColor.ToVector3());
      }
      if (justPressedJumped && IsGrounded) {
        PlayNewSound("Ori/Jump/seinJumpsGrass" + randJump.NextNoRepeat(5), 0.75f);
      }
      bool oldGrounded = IsGrounded;
      IsGrounded = CheckGrounded();
      OnWall = CheckOnWall();

      // Footstep effects
      if (!Main.dedServ && IsGrounded) {
        bool doDust = false;
        if (!oldGrounded) {
          doDust = true;
          FootstepManager.Instance.PlayLandingFromPlayer(player);
        }
        else if (animations.TrackName == "Running" && (animations.FrameIndex == 4 || animations.FrameIndex == 9)) {
          doDust = true;
          FootstepManager.Instance.PlayFootstepFromPlayer(player);
        }

        if (doDust) {
          var dustPos = player.Bottom + new Vector2(player.direction == -1 ? -4 : 2, -2);
          for (int i = 0; i < 4; i++) {
            Dust dust = Main.dust[Dust.NewDust(dustPos, 2, 2, 111, 0f, -2.7f, 0, new Color(255, 255, 255), 1f)];
            dust.noGravity = true;
            dust.scale = 0.75f;
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
            dust.shader.UseColor(Color.White);
            dust.fadeIn = 0.03947368f;
          }
        }
      }
    }

    private bool CheckGrounded() {
      float vel = player.velocity.Y * player.gravDir;
      if (vel < 0 && vel > 0.1f) {
        return false;
      }
      Vector2 feetVect = player.gravDir > 0 ? player.Bottom : player.Top;
      feetVect.Y += 1f / 255f * player.gravDir;
      Point pos = feetVect.ToTileCoordinates();
      if (player.fireWalk || player.waterWalk || player.waterWalk2) {
        Tile tile = Main.tile[pos.X, pos.Y];
        bool testblock = tile.liquid > 0 && Main.tile[pos.X, pos.Y - 1].liquid == 0;
        if (testblock && tile.lava() ? player.fireWalk : (player.waterWalk || player.waterWalk2)) {
          return true;
        }
      }
      return !Collision.IsClearSpotTest(player.position + new Vector2(0, 8 * player.gravDir), 16f, player.width, player.height, false, false, (int)player.gravDir, true, true);
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
      flashing = !player.immuneNoBlink && player.immuneTime % 12 > 6;
    }

    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
      if (!IsOri) {
        return true;
      }

      genGore = false;
      if (abilities.stomp.InUse || abilities.chargeDash.InUse || abilities.chargeJump.InUse) {
        return false;
      }
      if (playSound) {
        playSound = false;
        useCustomHurtSound = true;
        UnrestrictedMovement = true;
      }
      return true;
    }

    public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
      if (useCustomHurtSound) {
        useCustomHurtSound = false;
        PlayNewSound("Ori/Hurt/seinHurtRegular" + randHurt.NextNoRepeat(4));
      }
    }

    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { // similar to prehurt, but for death
      if (IsOri) {
        if (playSound) {
          playSound = false;
          switch (damageSource.SourceOtherIndex) {
            case 1:
              PlayNewSound("Ori/Death/seinSwimmingDrowningDeath" + RandomChar.Next(3), 3f);
              break;
            case 2:
              PlayNewSound("Ori/Death/seinDeathLava" + RandomChar.Next(5));
              break;
            default:
              PlayNewSound("Ori/Death/seinDeathRegular" + RandomChar.Next(5));
              break;
          }
        }
        if (genGore) {
          genGore = false;
          for (int i = 0; i < 15; i++) {
            Dust dust = Main.dust[Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
          }
        }
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
      if (OnWall || Transforming || abilities.stomp.InUse || abilities.airJump.InUse || abilities.burrow.InUse || abilities.chargeJump.InUse || abilities.wallChargeJump.InUse) {
        PlayerLayer.Wings.visible = false;
      }
      #endregion

      /*if (Abilities.soulLink.PlacedSoulLink) {
        layers.Insert(0, OriLayers.Instance.SoulLinkLayer);
      }*/
      int idx = layers.Contains(PlayerLayer.HeldItem) ? layers.IndexOf(PlayerLayer.HeldItem) : (layers.Count - 1);

      if (IsOri) {
        if (animations.playerAnim.Valid && !abilities.burrow.InUse && !player.mount.Active) {
          layers.Insert(idx++, OriLayers.Instance.Trail);
        }
        animations.glideAnim.TryInsertInLayers(layers, OriLayers.Instance.FeatherSprite, idx++);
        animations.bashAnim.TryInsertInLayers(layers, OriLayers.Instance.BashArrow, idx++);
      }
      if (!player.dead && !player.invis) {
        animations.playerAnim.TryInsertInLayers(layers, OriLayers.Instance.PlayerSprite, idx++);
      }
      player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
      OriLayers.Instance.Trail.visible = OriLayers.Instance.PlayerSprite.visible && !abilities.burrow.InUse && !player.mount.Active;
    }

    public override void OnEnterWorld(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.IsLocal = true;
      oPlayer.SeinMinionActive = false;
      oPlayer.SeinMinionType = 0;
      OriMod.ConfigClient.PlayerColor = oPlayer.SpriteColorPrimary;
      OriMod.ConfigClient.PlayerColorSecondary = oPlayer.SpriteColorSecondary;
    }

    public override void OnRespawn(Player player) {
      abilities.DisableAllAbilities();
    }
  }
}
