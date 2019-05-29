using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  
  // This partial class is for Movement-specific implementations
  public abstract class Movement {
    protected Player player;
    protected OriPlayer oPlayer;
    protected bool isLocalPlayer;
    protected MovementHandler Handler;
    public Movement(OriPlayer oriPlayer, MovementHandler handler) {
      player = oriPlayer.player;
      oPlayer = oriPlayer;
      isLocalPlayer = player.whoAmI == Terraria.Main.myPlayer;
      Handler = handler;
    }
    public enum State {
      Inactive = 0,
      Starting = 1,
      Active = 2,
      Ending = 3,
      Failed = 4
    }
    public State state = State.Inactive;
    public bool unlocked = true;
    public bool canUse = false;
    public bool IsState(params State[] stat) {
      foreach(int s in stat) {
        State v = (State)s;
        if (state == v) return true;
      }
      return false;
    }
    public bool isInUse {
      get {
        return state != State.Inactive;
      }
    }
    public virtual void Main() {
      if (!canUse && !isInUse) return;
      Ability();
    }
    public virtual bool Ability() {
      switch (state) {
        case State.Active: Active();
          return true;
        case State.Starting: Starting();
          return true;
        case State.Ending: Ending();
          return true;
        case State.Failed: Failed();
          return true;
        default:
          return false;
      }
    }
    public virtual void Starting() { }
    public virtual void Active() { }
    public virtual void Ending() { }
    public virtual void Failed() { }
    public abstract void Tick();
  }
  public partial class MovementHandler {
    #region Variables

    public Dictionary<string, int[]> movementStates;
    public enum MoveType {
      SoulLink = 1,
      WallJump = 2,
      ChargeFlame = 3,
      AirJump = 4,
      Bash = 5,
      Stomp = 6,
      Glide = 7,
      Climb = 8,
      ChargeJump = 9,
      Dash = 10,
      ChargeDash = 11,
      Grenade = 12,
      Crouch = 13,
      LookUp = 14
    }
    
    // Starting, Active, and Ending state all depend on how the move implements those three states
    //   Glide is fairly straightforward and uses all three for the purpose of different animations and sounds
    //   Wall Jump only makes use of the Active state, and for a few frames 
    //   TODO: Soul Link uses the Ending state as a cooldown, and Starting state as holding down the required key after CanUse
    public enum State {
      Disable = 0, // This move cannot be used, and any code for that move's behavior. Only use on myPlayer
      CanUse = 1,  // With the required keypress(es), this move can be activated 
      Starting = 2,
      Active = 3, // The bulk of a move should run if its state is Active
      Ending = 4,
      Failed = 6, // Associated with abilities that have a behavior for attempts to use while disabled (i.e. Bash)
    }

    // Readonly's are variables specific to how movement behaves
    // Only one currTime is used per movement, reused for Starting, Active, and Ending states, if applicible
    // IsRefreshed is used for movement that cannot be used again until set by one or more conditions
    //   For example: Double Jump and Dash are refreshed from onWall and using Bash
    
    private const float defaultFallSpeed = 16f;
    
    private const int soulLinkActivateTime = 48;
    private const int soulLinkRechargeTime = 60 * 60;
    public int soulLinkCurrTime = 0;
    public int soulLinkCurrRecharge = 0;

    private static readonly Vector2 wallJumpVelocity = new Vector2(4, -7.2f);
    private const int wallJumpEndDur = 12;
    public int wallJumpCurrDur = 0;

    private const int bashMinTime = 40;
    private const int bashMaxTime = 150;
    private const int doubleBashWindow = 2;
    public byte bashCurrNPC = 255;
    public int bashCurrTime = 0;
    public float bashCurrAngle = 0;

    public const int stompStartDur = 24;
    public const int stompMinDur = 60;
    public const float stompGrav = 4f;
    public const float stompMaxFallSpeed = 28f;
    public int stompCurrDur = 0;
    public Projectile stompProj;

    public const float glideMaxFallSpeed = 2f;
    public const float glideRunSlowdown = 0.125f;
    public const float glideRunAcceleration = 0.2f;
    private const int glideStartDuration = 8;
    private const int glideEndDuration = 10;
    public int glideCurrTime = 0;

    private const int chargeJumpStartTime = 60;
    public int chargeJumpCurrTime = 0;

    public Vector2 grenadePos = Vector2.Zero;
    
    private static readonly float[] dashSpeeds = new float[] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    };
    private const int dashDuration = 24;
    public int dashCurrTime = 0;
    public int dashCurrDirection = 1;
    
    private static readonly float[] chargeDashSpeeds = new float[] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 25f
    };
    private const int chargeDashDuration = 15;
    
    public byte chargeDashCurrNPC = 255;
    private int chargeDashCurrTime = 0;
    public int chargeDashCurrDirection = 1;
    #endregion
  
    public void SoulLink() {
      switch (GetState("SoulLink")) {
        default:
          return;
      }
    }
    public void WallJump() {
      switch (GetState("WallJump")) {
        case State.Active: {
          player.velocity.Y = wallJumpVelocity.Y;
          oPlayer.onWall = false;
          oPlayer.PlayNewSound("Ori/WallJump/seinWallJumps" + OriPlayer.RandomChar(5));
          break;
        }
        case State.Ending: {
          if (oPlayer.onWall) player.velocity.Y--;
          break;
        }
        default:
          return;
      }
      player.velocity.X = wallJumpVelocity.X * -player.direction;
      oPlayer.unrestrictedMovement = true;
      return;
    }
    public void ChargeFlame() {
      switch (GetState("ChargeFlame")) {
        default:
          return;
      }
    }
    public void AirJump() {
      airJump.Ability();
    }
    public void Bash() {
      switch (GetState("Bash")) {
        default:
          return;
      }
    }
    public void Stomp() {
      switch (GetState("Stomp")) {
        case State.Starting: {
          if (stompCurrDur == 0) {
            oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + OriPlayer.RandomChar(3), 1f, 0.2f);
          }
          player.velocity.X = 0;
          player.velocity.Y *= 0.9f;
          player.gravity = -0.1f;
          break;
        }
        case State.Active: {
          if (stompCurrDur == 0) {
            oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + OriPlayer.RandomChar(3));
            stompProj = Main.projectile[Projectile.NewProjectile(player.Center, new Vector2(0, 0), oPlayer.mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1)];
          }
          player.velocity.X = 0;
          player.gravity = stompGrav;
          player.maxFallSpeed = stompMaxFallSpeed;
          player.immune = true;
          break;
        }
        case State.Ending: {
          oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + OriPlayer.RandomChar(3));
          Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
          for (int i = 0; i < 25; i++) { // does particles
            Dust dust = Main.dust[Terraria.Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
            dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
            dust.velocity *= new Vector2(2, 0.5f);
            if (dust.velocity.Y > 0) {
              dust.velocity.Y = -dust.velocity.Y;
            }
          }
          stompProj.width = 600;
          stompProj.height = 320;
          break;
        }
        default:
          return;
      }
      player.controlUp = false;
      player.controlDown = false;
      player.controlLeft = false;
      player.controlRight = false;
    }
    public void Glide() {
      switch (GetState("Glide")) {
        case State.Disable: {
          player.maxFallSpeed = defaultFallSpeed; // Default fall speed
          return;
        }
        case State.Starting: {
          if (glideCurrTime == 0) oPlayer.PlayNewSound("Ori/Glide/seinGlideStart" + OriPlayer.RandomChar(3), 0.8f);
          break;
        }
        case State.Ending: {
          if (glideCurrTime == 0) oPlayer.PlayNewSound("Ori/Glide/seinGildeEnd" + OriPlayer.RandomChar(3), 0.8f);
          break;
        }
        case State.Active: {
          if (PlayerInput.Triggers.JustPressed.Left || PlayerInput.Triggers.JustPressed.Right) {
            oPlayer.PlayNewSound("Ori/Glide/seinGlideMoveLeftRight" + OriPlayer.RandomChar(5), 0.45f);
          }
          break;
        }
      }
      player.maxFallSpeed = glideMaxFallSpeed;
      player.runSlowdown = glideRunSlowdown;
      player.runAcceleration = glideRunAcceleration;
    }
    public void Climb() {
      switch (GetState("Climb")) {
        case State.Active: {
          if (PlayerInput.Triggers.Current.Up) {
            if (player.velocity.Y < -2) {
              player.velocity.Y++;
            }
            else {
              player.velocity.Y--;
            }
          }
          else if (PlayerInput.Triggers.Current.Down) {
            if (player.velocity.Y < 4) {
              player.velocity.Y++;
            }
            else {
              player.velocity.Y--;
            }
          }
          break;
        }
      }
    }
    public void ChargeJump() {
      switch (GetState("ChargeJump")) {
        default:
          return;
      }
    }
    public void Dash() {
      switch (GetState("Dash")) {
        case State.Starting: {
          dashCurrDirection = player.direction;
          dashCurrTime = 0;
          oPlayer.PlayNewSound("Ori/Dash/seinDash" + OriPlayer.RandomChar(3), 0.2f);
          player.pulley = false;
          break;
        }
        case State.Active: {
          break;
        }
        default:
          return;
      }
      player.velocity.X = dashSpeeds[dashCurrTime] * dashCurrDirection * 0.65f;
      player.velocity.Y = 0.25f * dashCurrTime;
      if (dashCurrTime > 20) player.runSlowdown = 26f;
    }
    public void ChargeDash() {
      // Funny story. I was going to implement rocket jumping later...
      // ...but it so happens that this code already has rocket jumping as a side effect
      // So no need to bother with that, huh?
      //   Edit: no longer the case, somehow :/
      switch (GetState("ChargeDash")) {
        case State.Starting: {
          float tempDist = 720f;
          int tempNPC = -1;
          for (int n = 0; n < Main.maxNPCs; n++) {
            NPC npc = Main.npc[n];
            if (!npc.active) continue;
            float dist = (player.position - npc.position).Length();
            if (dist < tempDist) {
              tempDist = dist;
              tempNPC = npc.whoAmI;
            }
          }
          if (tempNPC != -1) {
            chargeDashCurrNPC = (byte)tempNPC;
          }
          chargeDashCurrDirection = player.direction;
          chargeDashCurrTime = 0;
          oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + OriPlayer.RandomChar(3), .5f);
          // oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDashChargeStart" + OriPlayer.RandomChar(2), .5f);
          break;
        }
        case State.Active: {
          break;
        }
        default:
          return;
      }
      if (chargeDashCurrNPC < Main.maxNPCs) {
        Vector2 dir = (Main.npc[chargeDashCurrNPC].position - player.position);
        dir.Y -= 32f;
        dir.Normalize();
        player.maxFallSpeed = chargeDashSpeeds[chargeDashCurrTime];
        player.velocity = dir * chargeDashSpeeds[chargeDashCurrTime] * 0.8f;
        player.gravity = 0;
        if (chargeDashCurrTime != chargeDashDuration - 1 && (player.Bottom - Main.npc[chargeDashCurrNPC].Top).Length() < chargeDashSpeeds[chargeDashCurrTime + 1]) {
          SetState("ChargeDash", State.Disable);
          player.position = Main.npc[chargeDashCurrNPC].position;
          player.position.Y -= 32f;
          chargeDashCurrNPC = 255;
          player.velocity = dir * chargeDashSpeeds[chargeDashSpeeds.Length - 1];
        }
      }
      else {
        player.velocity.X = chargeDashSpeeds[chargeDashCurrTime] * chargeDashCurrDirection * 0.8f;
        player.velocity.Y = oPlayer.isGrounded ? -0.1f : 0.15f * chargeDashCurrTime;
      }
      player.runSlowdown = 26f;
    }
    public void Grenade(int mouseX=0, int mouseY=0) {
      Grenade(new Vector2(mouseX, mouseY));
    }
    public void Grenade(Vector2 mousePos) {
      switch (GetState("Grenade")) {
        default:
          return;
      }
    }
    public void Tick() {
      if (oPlayer == null || player.whoAmI != Main.myPlayer) {
        return;
      }
      List<int> prevStates = new List<int>();
      string[] eNames = Enum.GetNames(typeof(MoveType));
      int[] eValues = (int[])Enum.GetValues(typeof(MoveType));
      foreach(string str in eNames) {
        prevStates.Add((int)GetState(str));
      }
      
      if (IsUnlocked("SoulLink")) {
        if (IsInUse("SoulLink")) {}
      }

      if (IsUnlocked("WallJump")) {
        if (IsState("WallJump", State.Ending)) {
          wallJumpCurrDur++;
          if (wallJumpCurrDur > wallJumpEndDur || PlayerInput.Triggers.JustPressed.Right || PlayerInput.Triggers.JustPressed.Left || oPlayer.isGrounded) {
            SetState("WallJump", State.Disable);
          }
        }
        else if (IsState("WallJump", State.Active)) {
          SetState("WallJump", State.Ending);
          wallJumpCurrDur = 0;
        }
        if (oPlayer.onWall && !oPlayer.isGrounded && !IsInUse("WallJump")) {
          SetState("WallJump", State.CanUse);
        }
        else {
          SetState("WallJump", State.Disable);
        }
        if (CanUse("WallJump") && PlayerInput.Triggers.JustPressed.Jump) {
          SetState("WallJump", State.Active);
        }
      }

      if (IsUnlocked("ChargeFlame")) {
        if (IsInUse("ChargeFlame")) {}
      }
      
      airJump.Tick();
      
      if (IsUnlocked("Bash")) {
        if (IsInUse("Bash")) {}
      }
      
      if (IsUnlocked("Stomp")) {
        if (!oPlayer.isGrounded && !IsInUse("Stomp") && !IsInUse("Dash") && !IsInUse("ChargeDash") && !IsInUse("Glide")) {
          SetState("Stomp", State.CanUse);
        }
        if (PlayerInput.Triggers.JustPressed.Down && CanUse("Stomp")) {
          SetState("Stomp", State.Starting);
          stompCurrDur = 0;
        }
        else if (IsState("Stomp", State.Starting)) {
          stompCurrDur++;
          if (stompCurrDur > stompStartDur) {
            stompCurrDur = 0;
            SetState("Stomp", State.Active);
          }
        }
        else if (IsState("Stomp", State.Active)) {
          stompCurrDur++;
          if (stompCurrDur > stompMinDur && !PlayerInput.Triggers.Current.Down) {
            SetState("Stomp", State.CanUse);
          }
          if (oPlayer.isGrounded) {
            SetState("Stomp", State.Ending);
          }
        }
        else if (IsState("Stomp", State.Ending)) {
          SetState("Stomp", State.Disable);
        }
      }

      if (IsUnlocked("Glide")) {
        if (IsInUse("Glide")) {
          if (IsState("Glide", State.Starting)) {
            glideCurrTime++;
            if (glideCurrTime > glideStartDuration) {
              SetState("Glide", State.Active);
              glideCurrTime = 0;
            }
          }
          else if (IsState("Glide", State.Ending)) {
            glideCurrTime++;
            if (glideCurrTime > glideEndDuration) {
              SetState("Glide", State.CanUse);
              glideCurrTime = 0;
            }
          }
          if (player.velocity.Y < 0 || oPlayer.onWall) {
            SetState("Glide", IsInUse("Glide") ? State.Ending : State.Disable);
          }
          else if (OriMod.FeatherKey.JustReleased) {
            SetState("Glide", State.Ending);
          }
        }
        else {
          if (player.velocity.Y > 0 && !oPlayer.onWall && (OriMod.FeatherKey.JustPressed || OriMod.FeatherKey.Current)) {
            SetState("Glide", State.Starting);
          }
        }
      }

      if (IsUnlocked("Climb")) {
        if (IsInUse("Climb")) {
          if (OriMod.ClimbKey.JustReleased) SetState("Climb", State.CanUse);
          if (!oPlayer.onWall) SetState("Climb", State.Disable);
        }
        else {
          if (oPlayer.onWall) {
            SetState("Climb", State.CanUse);
          }
          if (CanUse("Climb") && (OriMod.ClimbKey.JustPressed || OriMod.ClimbKey.Current)) {
            SetState("Climb", State.Active);
          }
        }
      }

      if (IsUnlocked("ChargeJump")) {
        if (IsInUse("ChargeJump")) {}
      }

      if (IsUnlocked("Dash")) {
        if (IsInUse("Dash")) {
          dashCurrTime++;
          if (dashCurrTime > dashDuration || oPlayer.onWall) {
            SetState("Dash", State.Disable);
          }
          if (IsState("Dash", State.Starting)) {
            SetState("Dash", State.Active);
          }
        }
        else {
          if (oPlayer.bashActive || oPlayer.onWall || oPlayer.isGrounded) {
            dashCurrTime = 0;
          }
          if (dashCurrTime == 0 && !oPlayer.bashActive /*TODO: Replace with IsInUse */ && !oPlayer.onWall) {
            SetState("Dash", State.CanUse);
          }
          if (CanUse("Dash") && OriMod.DashKey.JustPressed) {
            SetState("Dash", State.Starting);
          }
        }
      }
      
      if (IsUnlocked("ChargeDash")) {
        if (IsInUse("ChargeDash")) {
          SetState("Dash", State.Disable);
          if (PlayerInput.Triggers.JustPressed.Jump) {
            SetState("ChargeDash", State.Disable);
          }
          chargeDashCurrTime++;
          if (chargeDashCurrTime > chargeDashDuration || oPlayer.onWall) {
            SetState("ChargeDash", State.Disable);
            chargeDashCurrNPC = 255;
            player.velocity *= 0.2f;
          }
          if (IsState("ChargeDash", State.Starting)) {
            SetState("ChargeDash", State.Active);
          }
        }
        else {
          chargeDashCurrTime = 0;
          if (!oPlayer.bashActive /*TODO: Replace with IsInUse */ && !oPlayer.onWall) {
            SetState("ChargeDash", State.CanUse);
          }
          if (CanUse("ChargeDash") && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
            SetState("ChargeDash", State.Starting);
          }
        }
      }

      if (IsInUse("Grenade")) {}
      
      // List of things Minecart should disable
      if (player.mount.Cart) {
        SetState("SoulLink", State.Disable);
        SetState("WallJump", State.Disable);
        SetState("AirJump", State.Disable);
        SetState("Stomp", State.Disable);
        SetState("Glide", State.Disable);
        SetState("Climb", State.Disable);
        SetState("ChargeJump", State.Disable);
        SetState("Dash", State.Disable);
        SetState("ChargeDash", State.Disable);
      }
      List<string> changes = new List<string>();
      for (int e = 0; e < prevStates.Count; e++) {
        int newState = (byte)GetState(eNames[e]);
        if (newState != prevStates[e]) {
          changes.Add(eNames[e]);
        }
      }
      if (changes.Count > 0) {
        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
          ModNetHandler.movementPacketHandler.SendMovementState(255, player.whoAmI, changes);
        }
      }
    }
    public void TickOtherClient() {
      if (IsInUse("Glide")) {
        Glide();
      }
    }
  }
}