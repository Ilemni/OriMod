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
      isLocalPlayer = player.whoAmI == Main.myPlayer;
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
    public bool refreshed = false;
    public bool IsState(params State[] stat) {
      foreach(int s in stat) {
        State v = (State)s;
        if (state == v) return true;
      }
      return false;
    }
    public bool inUse {
      get {
        return state != State.Inactive;
      }
    }
    protected virtual bool PreAbility() {
      if (!canUse && !inUse) return false;
      return true;
    }
    public virtual bool Ability() {
      if (!PreAbility()) return false;
      switch (state) {
        case State.Active:
          Active();
          Using();
          return true;
        case State.Starting:
          Starting();
          Using();
          return true;
        case State.Ending:
          Ending();
          Using();
          return true;
        case State.Failed:
          Failed();
          return true;
        default:
          return false;
      }
    }
    public virtual void Starting() { }
    public virtual void Active() { }
    public virtual void Ending() { }
    public virtual void Failed() { }
    public virtual void Using() { }
    public abstract void Tick();
  }
  public partial class MovementHandler {
    #region Variables

    public Dictionary<string, int[]> movementStates;
    
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

    private const int chargeJumpStartTime = 60;
    public int chargeJumpCurrTime = 0;

    public Vector2 grenadePos = Vector2.Zero;
    #endregion
  
    public void SoulLink() {
      // soulLink.Ability();
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
      // cFlame.Ability();
    }
    public void AirJump() {
      airJump.Ability();
    }
    public void Bash() {
    }
    public void Stomp() {
      stomp.Ability();
    }
    public void Glide() {
      glide.Ability();
    }
    public void Climb() {
      climb.Ability();
    }
    public void ChargeJump() {
      // cJump.Ability();
    }
    public void Dash() {
      dash.Ability();
    }
    public void ChargeDash() {
      cDash.Ability();
    }
    public void Grenade(int mouseX=0, int mouseY=0) {
      Grenade(new Vector2(mouseX, mouseY));
    }
    public void Grenade(Vector2 mousePos) {
      // grenade.Ability();
    }
    public void Tick() {
      if (oPlayer == null || player.whoAmI != Main.myPlayer) {
        return;
      }
      List<int> prevStates = new List<int>();
      // string[] eNames = Enum.GetNames(typeof(MoveType)); // TODO: Replace with Movements
      // int[] eValues = (int[])Enum.GetValues(typeof(MoveType));
      // foreach(string str in eNames) {
      //   prevStates.Add((int)GetState(str));
      // }
      
      // soulLink.Tick();

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

      // soulLink.Tick();
      // cFlame.Tick();
      airJump.Tick();
      // bash.Tick();
      stomp.Tick();
      glide.Tick();
      climb.Tick();
      // cJump.Tick();
      dash.Tick();
      cDash.Tick();
      // grenade.Tick();
      
      // List of things Minecart should disable
      if (player.mount.Cart) {
        // soulLink.canUse = false;
        // wJump.canUse = false;
        airJump.canUse = false;
        stomp.canUse = false;
        glide.canUse = false;
        climb.canUse = false;
        // cJump.canUse = false;
        dash.canUse = false;
        cDash.canUse = false;
      }
      List<string> changes = new List<string>();
      // for (int e = 0; e < prevStates.Count; e++) { // TODO: Replace with Movements
      //   int newState = (byte)GetState(eNames[e]);
      //   if (newState != prevStates[e]) {
      //     changes.Add(eNames[e]);
      //   }
      // }
      if (changes.Count > 0) {
        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
          ModNetHandler.movementPacketHandler.SendMovementState(255, player.whoAmI, changes);
        }
      }
    }
    public void TickOtherClient() {
      if (glide.inUse) {
        Glide();
      }
    }
  }
}