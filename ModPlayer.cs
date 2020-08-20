using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Networking;
using OriMod.Upgrades;
using OriMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod {
  public sealed class OriPlayer : ModPlayer {
    #region Variables
    /// <summary>
    /// Local instance of this OriPlayer
    /// </summary>
    public static OriPlayer Local => Main.LocalPlayer.GetModPlayer<OriPlayer>();

    /// <summary>
    /// Manager for all Abilities on this OriPlayer instance.
    /// </summary>
    internal AbilityManager Abilities { get; private set; }

    /// <summary>
    /// Manager for all Upgrades on this OriPlayer instance.
    /// </summary>
    internal UpgradeManager Upgrades { get; private set; }

    /// <summary>
    /// Container for all Animations on this OriPlayer instance.
    /// </summary>
    internal AnimationContainer Animations { get; private set; }

    /// <summary>
    /// Trail created when the player is moving.
    /// </summary>
    internal TrailManager Trails { get; private set; }

    internal bool doNetUpdate = false;

    internal bool debugMode = false;

    private bool useCustomHurtSound = false;

    /// <summary>
    /// When set to true, uses custom movement and player sprites. External mods that attempt to be compatible with this one will need to use this property.
    /// </summary>
    public bool IsOri {
      get => _isOri;
      set {
        if (value != _isOri) {
          doNetUpdate = true;
          _isOri = value;
        }
      }
    }

    /// <summary>
    /// Represents if the player is currently transforming into Ori. While transforming, all player input is disabled.
    /// </summary>
    public bool Transforming {
      get => _transforming;
      internal set {
        if (value != _transforming) {
          doNetUpdate = true;
          _transforming = value;
        }
      }
    }

    internal float TransformTimer = 0;

    /// <summary>
    /// Amount of Spirit Light the player has
    /// </summary>
    public long SpiritLight { get; internal set; }

    /// <summary>
    /// Transform variables used to hasten repeat transforms.
    /// </summary>
    internal bool HasTransformedOnce { get; private set; }

    private float RepeatedTransformRate => 2.5f;

    private int TransformDirection = 0;

    /// <summary>
    /// Represents if the player is on the ground.
    /// </summary>
    public bool IsGrounded { get; private set; }

    /// <summary>
    /// Represents if the player is on a wall
    /// </summary>
    public bool OnWall { get; private set; }

    /// <summary>
    /// When true, sets player.runSlowDown to 0 every frame.
    /// </summary>
    public bool UnrestrictedMovement {
      get => _unrestrictedMovement;
      set {
        if (value != _unrestrictedMovement) {
          doNetUpdate = true;
          _unrestrictedMovement = value;
        }
      }
    }

    /// <summary>
    /// A more persistent player.justJumped
    /// </summary>
    internal bool justJumped;

    /// <summary>
    /// Info about if this player has an OriMod Sein minion summoned. Used to prevent having more than one Sein summoned per player.
    /// </summary>
    public bool SeinMinionActive {
      get => _seinMinionActive;
      internal set {
        if (value != _seinMinionActive) {
          doNetUpdate = true;
          _seinMinionActive = value;
        }
      }
    }

    /// <summary>
    /// The ID of the Sein that is summoned. This is the <see cref="Entity.whoAmI"/> of the Sein projectile.
    /// </summary>
    public int SeinMinionID {
      get => _seinMinionID;
      internal set {
        if (value != _seinMinionID) {
          doNetUpdate = true;
          _seinMinionID = value;
        }
      }
    }
    
    /// <summary>
    /// The current version of Sein that is summoned. Used to prevent re-summons of the same tier of Sein.
    /// </summary>
    public int SeinMinionType {
      get => _seinMinionType;
      internal set {
        if (value != _seinMinionType) {
          doNetUpdate = true;
          _seinMinionType = value;
        }
      }
    }

    private int PlayerDustTimer = 0;

    internal int ImmuneTimer = 0;

    private readonly RandomChar randJump = new RandomChar();
    private readonly RandomChar randHurt = new RandomChar();

    #region Animation Properties
    /// <summary>
    /// Shorthand for AnimationHandler.PlayerAnim.TileSize.X.
    /// </summary>
    internal static int SpriteWidth => AnimationHandler.Instance.PlayerAnim.spriteSize.X;
    
    /// <summary>
    /// Shorthand for `AnimationHandler.PlayerAnim.TileSize.Y.
    /// </summary>
    internal static int SpriteHeight => AnimationHandler.Instance.PlayerAnim.spriteSize.Y;

    /// <summary>
    /// Current pixel placement of the current animation tile on the spritesheet.
    /// </summary>
    public Point AnimFrame {
      get => TileToPixel(AnimTile);
      set => AnimTile = PixelToTile(value);
    }

    /// <summary>
    /// Current tile placement of the animation on the spritesheet. X and Y values are based on the sprite tile coordinates, not pixel coordinates.
    /// </summary>
    public PointByte AnimTile;

    /// <summary>
    /// The name of the animation track currently playing.
    /// </summary>
    public string AnimName {
      get => _animName;
      private set {
        if (value != _animName) {
          OnAnimNameChange(value);
          _animName = value;
        }
      }
    }

    internal int AnimIndex { get; private set; }
    internal float AnimTime { get; private set; }
    internal float AnimRads { get; private set; }
    internal bool AnimReversed = false;
    internal bool Flashing = false;

    internal static PointByte PixelToTile(Point pixel) => new PointByte((byte)(pixel.X / SpriteWidth), (byte)(pixel.Y / SpriteHeight));

    internal static Point TileToPixel(PointByte tile) {
      var result = (Point)tile;
      result.X *= SpriteWidth;
      result.Y *= SpriteHeight;
      return result;
    }
    #endregion

    #region Aesthetics
    internal Color SpriteColorPrimary {
      get => Main.myPlayer == player.whoAmI ? OriMod.ConfigClient.PlayerColor : _mpcSpriteColorPrimary;
      set {
        if (Main.myPlayer != player.whoAmI) {
          _mpcSpriteColorPrimary = value;
        }
      }
    }

    internal Color SpriteColorSecondary {
      get => Main.myPlayer == player.whoAmI ? OriMod.ConfigClient.PlayerColorSecondary : _mpcSpriteColorSecondary;
      set {
        if (Main.myPlayer != player.whoAmI) {
          _mpcSpriteColorSecondary = value;
        }
      }
    }

    internal bool MpcPlayerLight {
      get => _mpcPlayerLight;
      set {
        if (value != _mpcPlayerLight) {
          doNetUpdate = true;
          _mpcPlayerLight = value;
        }
      }
    }

    internal bool DoPlayerLight => (player.whoAmI == Main.myPlayer || OriMod.ConfigClient.GlobalPlayerLight) ? OriMod.ConfigClient.PlayerLight : MpcPlayerLight;
    public Color LightColor = new Color(0.2f, 0.4f, 0.4f);
    #endregion

    #region Backing fields
    private bool _isOri;
    private bool _unrestrictedMovement = false;
    private bool _seinMinionActive = false;
    private int _seinMinionID;
    private int _seinMinionType = 0;
    private bool _transforming = false;
    private string _animName = "Default";
    private Color _mpcSpriteColorPrimary = Color.LightCyan;
    private Color _mpcSpriteColorSecondary = Color.LightCyan;
    private bool _mpcPlayerLight = false;
    #endregion
    #endregion

    #region Internal Methods
    internal SoundEffectInstance PlayNewSound(string Path, float Volume = 1, float Pitch = 0) =>
      Main.PlaySound((int)SoundType.Custom, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NewSFX/" + Path), Volume, Pitch);

    /// <summary> Checks if the key is pressed and this is LocalPlayer. </summary>
    /// <param name="triggerKey">The key that was pressed</param>
    internal bool Input(bool triggerKey) => triggerKey && player.whoAmI == Main.myPlayer;

    /// <summary>
    /// Prints a debug message if "debug mode" is enabled
    /// </summary>
    /// <param name="msg">Message to print</param>
    internal void Debug(string msg) {
      if (debugMode && player.whoAmI == Main.myPlayer) {
        Main.NewText(msg);
      }
    }

    /// <summary>
    /// Sets current animation frame data
    /// </summary>
    /// <param name="name"></param>
    /// <param name="frameIndex"></param>
    /// <param name="time"></param>
    /// <param name="frame"></param>
    /// <param name="animRads"></param>
    internal void SetFrame(string name, int frameIndex, float time, Frame frame, float animRads) {
      AnimName = name;
      AnimIndex = frameIndex;
      AnimTime = time;
      AnimFrame = TileToPixel(frame.Tile);
      AnimRads = animRads;
    }

    /// <summary>
    /// Begins the transformation process from normal state to Spirit state
    /// </summary>
    internal void BeginTransformation() {
      Transforming = true;
      TransformDirection = player.direction;
      TransformTimer = Animations.PlayerAnim.source.tracks["TransformEnd"].Duration + Animations.PlayerAnim.source.tracks["TransformStart"].Duration;
    }

    /// <summary>
    /// Removes all Sein-related buffs from the player.
    /// </summary>
    internal void RemoveSeinBuffs() {
      for (int u = 1; u <= OriMod.Instance.SeinUpgrades.Count; u++) {
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
      if (PlayerDustTimer > 0) {
        return;
      }

      Dust dust = Main.dust[Dust.NewDust(player.position, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.scale = Main.rand.NextFloat(0.7f, 0.9f);
      dust.noGravity = false;
      PlayerDustTimer = Transforming ? Main.rand.Next(3, 8)
        : Abilities.dash.InUse || Abilities.chargeDash.InUse ? Main.rand.Next(2, 4)
        : Abilities.burrow.InUse ? Main.rand.Next(6, 10)
        : Main.rand.Next(10, 15);
    }

    /// <summary>
    /// Updates the frame by one second, and changes it depending on various conditions.
    /// </summary>
    /// <param name="drawPlayer"></param>
    private void UpdateFrame(Player drawPlayer) {
      AnimTime++;
      if (Transforming) {
        IncrementFrame(IsOri ? "TransformEnd" : "TransformStart");
        return;
      }
      if (!IsOri) {
        return;
      }

      if (!HasTransformedOnce) {
        HasTransformedOnce = true;
      }
      if (drawPlayer.pulley || drawPlayer.mount.Active) {
        IncrementFrame("Idle");
        return;
      }
      if (Abilities.burrow.InUse) {
        double rad = Math.Atan2(Abilities.burrow.Velocity.X, -Abilities.burrow.Velocity.Y);
        int deg = (int)(rad * (180 / Math.PI));
        deg *= drawPlayer.direction;
        if (player.gravDir < 0) {
          deg += 180;
        }
        IncrementFrame("Burrow", rotDegrees: deg);
        return;
      }
      if (Abilities.wallChargeJump.Active) {
        float rad = (float)Math.Atan2(player.velocity.Y, player.velocity.X);
        float deg = rad * (float)(180 / Math.PI) * player.direction;
        if (player.direction == -1) {
          deg -= 180f;
        }
        IncrementFrame("Dash", overrideFrame: 0, rotDegrees: deg);
        return;
      }
      if (Abilities.wallJump.InUse) {
        IncrementFrame("WallJump");
        return;
      }
      if (Abilities.airJump.InUse && !(Abilities.dash.InUse || Abilities.chargeDash.InUse)) {
        IncrementFrame("AirJump");
        AnimRads = AnimTime * 0.8f;
        return;
      }
      if (Abilities.bash.InUse) {
        IncrementFrame("Bash");
        return;
      }
      if (Abilities.stomp.InUse) {
        switch (Abilities.stomp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("AirJump");
            AnimRads = AnimTime;
            return;
          case Ability.State.Active:
            IncrementFrame("ChargeJump", rotDegrees: 180f, overrideDur: 2, overrideHeader: new Header(playback: PlaybackMode.PingPong));
            return;
        }
      }
      if (Abilities.glide.InUse) {
        switch (Abilities.glide.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("GlideStart");
            return;
          case Ability.State.Active:
            IncrementFrame("Glide");
            return;
          case Ability.State.Ending:
            IncrementFrame("GlideStart", overrideHeader: new Header(playback: PlaybackMode.Reverse));
            return;
        }
      }
      if (Abilities.climb.InUse) {
        if (Abilities.climb.IsCharging) {
          if (!Abilities.wallChargeJump.Charged) {
            IncrementFrame("WallChargeJumpCharge", overrideFrame: Abilities.wallChargeJump.Refreshed ? -1 : 0);
            return;
          }
          int frame = 0;
          Abilities.wallChargeJump.GetMouseDirection(out float angle);
          if (angle < -0.46f) {
            frame = 2;
          }
          else if (angle < -0.17f) {
            frame = 1;
          }
          else if (angle > 0.46f) {
            frame = 4;
          }
          else if (angle > 0.17f) {
            frame = 3;
          }
          IncrementFrame("WallChargeJumpAim", overrideFrame: frame);
          return;
        }
        if (Math.Abs(player.velocity.Y) < 0.1f) {
          IncrementFrame("ClimbIdle");
        }
        else {
          IncrementFrame(player.velocity.Y * player.gravDir < 0 ? "Climb" : "WallSlide", overrideTime: AnimTime + Math.Abs(drawPlayer.velocity.Y) * 0.1f);
        }
        return;
      }
      if (OnWall && !IsGrounded) {
        IncrementFrame("WallSlide");
        return;
      }
      if (Abilities.dash.InUse || Abilities.chargeDash.InUse) {
        if (Math.Abs(player.velocity.X) > 18f) {
          IncrementFrame("Dash");
        }
        else {
          IncrementFrame("Dash", overrideFrame: 2);
        }
        return;
      }
      if (Abilities.lookUp.InUse) {
        switch (Abilities.lookUp.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("LookUpStart");
            return;
          case Ability.State.Active:
            IncrementFrame("LookUp");
            return;
          case Ability.State.Ending:
            IncrementFrame("LookUpStart", overrideHeader: new Header(playback: PlaybackMode.Reverse));
            return;
        }
      }
      if (Abilities.crouch.InUse) {
        switch (Abilities.crouch.AbilityState) {
          case Ability.State.Starting:
            IncrementFrame("CrouchStart");
            return;
          case Ability.State.Active:
            IncrementFrame("Crouch");
            return;
          case Ability.State.Ending:
            IncrementFrame("CrouchStart", overrideHeader: new Header(playback: PlaybackMode.Reverse));
            return;
        }
      }

      if (Abilities.chargeJump.Active) {
        IncrementFrame("ChargeJump");
        return;
      }
      if (!IsGrounded) {
        // XOR so opposite signs (negative value) means jumping regardless of gravity
        IncrementFrame(((int)drawPlayer.velocity.Y ^ (int)drawPlayer.gravity) <= 0 ? "Jump" : "Falling");
        return;
      }
      if (Math.Abs(drawPlayer.velocity.X) > 0.2f) {
        IncrementFrame("Running", overrideTime: AnimTime + (int)Math.Abs(player.velocity.X) / 3);
        return;
      }
      IncrementFrame(OnWall ? "IdleAgainst" : "Idle");
      return;
    }

    /// <summary>
    /// Calls AnimationHandler.IncrememtFrame() with supplied arguments.
    /// </summary>
    /// <param name="anim">Name of animation track</param>
    /// <param name="overrideFrame">Index of frame to override</param>
    /// <param name="overrideTime">Override current time of the frame</param>
    /// <param name="overrideDur">Override duration of the frame</param>
    /// <param name="overrideHeader">Override header data of the frame</param>
    /// <param name="drawOffset">Override draw position of the frame</param>
    /// <param name="rotDegrees">Rotation of the sprite, in degrees</param>
    private void IncrementFrame(string anim = "Default", int overrideFrame = -1, float overrideTime = 0, int overrideDur = 0, Header overrideHeader = null, Vector2 drawOffset = new Vector2(), float rotDegrees = 0) {
      if (AnimName != null && debugMode) {
        Main.NewText($"Frame called: {AnimName}, Time: {AnimTime}, AnimIndex: {AnimIndex}/{Animations.PlayerAnim.ActiveTrack.Frames.Length}"); // Debug
        var frame = Animations.PlayerAnim.ActiveFrame;
        var tile = Animations.PlayerAnim.ActiveTile;
        Main.NewText($"Frame info: {frame} => {tile}");
      }
      AnimationHandler.Instance.IncrementFrame(this, anim, overrideFrame, overrideTime, overrideDur, overrideHeader, drawOffset, rotDegrees);
    }

    /// <summary>
    /// Called when a different value is supplied to this.AnimName.
    /// </summary>
    /// <param name="value">New value of AnimName</param>
    private void OnAnimNameChange(string value) {
      if (Main.dedServ) {
        return;
      }

      Animations.PlayerAnim.CheckIfValid(value);
      Animations.SecondaryLayer.CheckIfValid(value);
      Animations.TrailAnim.CheckIfValid(value);
      Animations.BashAnim.CheckIfValid(value);
      Animations.GlideAnim.CheckIfValid(value);
    }

    /// <summary>
    /// Resets the data of this OriPlayer instance
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
      Abilities = new AbilityManager(this);
      Upgrades = new UpgradeManager(this);

      if (!Main.dedServ) {
        Animations = new AnimationContainer(this);
        Trails = new TrailManager(this, 26);
      }
    }

    public override void ResetEffects() {
      if (Transforming) {
        float rate = HasTransformedOnce ? RepeatedTransformRate : 1;
        AnimTime += rate - 1;
        TransformTimer -= rate;
        if (TransformTimer < 0 || HasTransformedOnce && TransformTimer < Animations.PlayerAnim.source.tracks["TransformEnd"].Duration - 62) {
          TransformTimer = 0;
          Transforming = false;
          IsOri = true;
        }
      }
      if (IsOri) {
        if (PlayerDustTimer > 0) {
          PlayerDustTimer--;
        }
      }
      if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer && doNetUpdate) {
        ModNetHandler.Instance.oriPlayerHandler.SendOriState(255, player.whoAmI);
        doNetUpdate = false;
      }
    }

    public override void UpdateDead() {
      Abilities.soulLink.UpdateDead();
    }

    public override TagCompound Save()
      => new TagCompound {
        {"OriSet", IsOri},
        {"Debug", debugMode},
        {"SpiritLight", SpiritLight}
      };

    public override void Load(TagCompound tag) {
      IsOri = tag.GetBool("OriSet");
      debugMode = tag.GetBool("Debug");
    }

    public override void PostUpdateMiscEffects() {
      if (player.HasBuff(BuffID.TheTongue)) {
        Abilities.DisableAllAbilities();
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
        if (IsGrounded || player.whoAmI == Main.myPlayer && (PlayerInput.Triggers.Current.Left || Input(PlayerInput.Triggers.Current.Right))) {
          UnrestrictedMovement = false;
        }
        player.runSlowdown = UnrestrictedMovement ? 0 : 1;
        #endregion

        if (OriMod.ConfigClient.SmoothCamera) {
                    // Smooth camera effect reduced while bosses are alive
                    Main.SetCameraLerp(OriUtils.IsAnyBossAlive() ? 0.15f : 0.05f, 1);
        }

        // Reduce gravity when clinging on wall
        if (OnWall) {
          // Either grounded or falling, not climbing
          if ((IsGrounded || player.velocity.Y * player.gravDir < 0) && !Abilities.climb.InUse) {
            player.gravity = 0.1f;
            player.maxFallSpeed = 6f;
            player.jumpSpeedBoost -= 6f;
          }
          // Sliding upward on wall, not stomping
          else if (!IsGrounded && player.velocity.Y * player.gravDir > 0 && !Abilities.stomp.InUse) {
            player.gravity = 0.1f;
            player.maxFallSpeed = 6f;
          }
        }

        Abilities.Update();
      }

      if (Transforming) {
        player.direction = TransformDirection;
        player.controlUseItem = false;
        int dur = AnimationHandler.Instance.PlayerAnim.tracks["TransformEnd"].Duration;
        if (TransformTimer > dur - 10) {
          if (TransformTimer < dur) {
            player.gravity = 9f;
            IsOri = true;
          }
          else {
            player.velocity = new Vector2(0, -0.00055f * TransformTimer);
            player.gravity = 0;
            CreatePlayerDust();
          }
        }
        player.runAcceleration = 0;
        player.maxRunSpeed = 0;
        player.immune = true;
      }

      if (ImmuneTimer > 0) {
        ImmuneTimer--;
        player.immune = true;
      }
    }

    public override void PostUpdate() {
      if (!Main.dedServ) {
        UpdateFrame(player);
      }

      #region Check Sein Buffs
      if (SeinMinionActive) {
        if (!(
          player.HasBuff(mod.GetBuff("SeinBuff1").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff2").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff3").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff4").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff5").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff6").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff7").Type) ||
          player.HasBuff(mod.GetBuff("SeinBuff8").Type)
        )) {
          SeinMinionActive = false;
          SeinMinionType = 0;
        }
      }
      #endregion

      if (!IsOri) {
        return;
      }

      if (DoPlayerLight && !Abilities.burrow.Active) {
        Lighting.AddLight(player.Center, LightColor.ToVector3());
      }

      justJumped = player.justJumped;
      if (justJumped) {
        PlayNewSound("Ori/Jump/seinJumpsGrass" + randJump.NextNoRepeat(5), 0.75f);
      }
      bool oldGrounded = IsGrounded;

      #region Update IsGrounded
      IsGrounded = false;
      Vector2 feetVect = player.gravDir > 0 ? player.Bottom : player.Top;
      feetVect.Y += 1f / 255f * player.gravDir;
      Point pos = feetVect.ToTileCoordinates();
      if (player.fireWalk || player.waterWalk || player.waterWalk2) {
        Tile tile = Main.tile[pos.X, pos.Y];
        bool testblock = tile.liquid > 0 && Main.tile[pos.X, pos.Y - 1].liquid == 0;
        if (testblock) {
          IsGrounded = tile.lava() ? player.fireWalk : (player.waterWalk || player.waterWalk2);
        }
      }
      if (!IsGrounded) {
        IsGrounded = !Collision.IsClearSpotTest(player.position + new Vector2(0, 8 * player.gravDir), 16f, player.width, player.height, false, false, (int)player.gravDir, true, true);
      }
      #endregion

      #region Update OnWall
      Point p = new Vector2(
        player.Center.X + player.direction + player.direction * player.width * 0.5f,
        player.position.Y + (player.gravDir < 0f ? -1f : 2f)
      ).ToTileCoordinates();
      OnWall = WorldGen.SolidTile(p.X, p.Y + 1) && WorldGen.SolidTile(p.X, p.Y + 2);
      #endregion

      // Footstep effects
      if (!Main.dedServ && IsGrounded) {
        bool doDust = false;
        if (!oldGrounded) {
          doDust = true;
          FootstepManager.Instance.PlayLandingFromPlayer(player);
        }
        else if (AnimName == "Running" && AnimIndex == 4 || AnimIndex == 9) {
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

    public override void FrameEffects() {
      if (!IsOri) {
        return;
      }

      if (player.velocity.LengthSquared() > 0.2f) {
        CreatePlayerDust();
      }
      Flashing = !player.immuneNoBlink && player.immuneTime % 12 > 6;
    }

    public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) { // effects when character is hurt
      useCustomHurtSound = false;
      if (!IsOri) {
        return true;
      }

      genGore = false;
      if (Abilities.stomp.InUse || Abilities.chargeDash.InUse || Abilities.chargeJump.InUse) {
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
      if (Main.dedServ) {
        return;
      }

      if (IsOri || Transforming) {
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
        if (Abilities.stomp.InUse || Abilities.airJump.InUse || Abilities.burrow.InUse || Abilities.chargeJump.InUse || Abilities.wallChargeJump.InUse || OnWall || Transforming) {
          PlayerLayer.Wings.visible = false;
        }
        #endregion

        if (Abilities.soulLink.PlacedSoulLink) {
          layers.Insert(0, OriLayers.Instance.SoulLinkLayer);
        }
        int idx = Math.Min(layers.IndexOf(PlayerLayer.HeldItem), layers.IndexOf(PlayerLayer.HeldProjFront));
        if (idx >= 0) {
          idx -= 2;
          if (idx < 0) {
            idx = 0;
          }
          if (IsOri) {
            Animations.TrailAnim.InsertInLayers(layers, idx++);
            Animations.GlideAnim.InsertInLayers(layers, idx++);
            Animations.BashAnim.InsertInLayers(layers, idx++);
          }
          if (!player.dead && !player.invis) {
            layers.Insert(idx++, Animations.PlayerAnim.playerLayer);
            if (IsOri) {
              layers.Insert(idx++, Animations.SecondaryLayer.playerLayer);
            }
          }
        }
        else {
          if (IsOri) {
            Animations.TrailAnim.AddToLayers(layers);
            Animations.GlideAnim.AddToLayers(layers);
            Animations.BashAnim.AddToLayers(layers);
          }
          if (!player.dead && !player.invis) {
            layers.Add(Animations.PlayerAnim.playerLayer);
            if (IsOri) {
              layers.Add(Animations.SecondaryLayer.playerLayer);
            }
          }
        }
        player.head = mod.GetEquipSlot("OriHead", EquipType.Head);
        OriLayers.Instance.Trail.visible = OriLayers.Instance.PlayerSprite.visible && !Abilities.burrow.InUse && !player.mount.Active;
      }
    }

    public override void OnEnterWorld(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.SeinMinionActive = false;
      oPlayer.SeinMinionType = 0;
    }

    public override void OnRespawn(Player player) {
      Abilities.DisableAllAbilities();
    }
  }
}
