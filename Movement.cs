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
  public abstract class Ability {
    protected Player player;
    protected OriPlayer OPlayer;
    protected bool isLocalPlayer;
    protected MovementHandler Handler;
    internal Ability(OriPlayer oriPlayer, MovementHandler handler) {
      player = oriPlayer.player;
      OPlayer = oriPlayer;
      isLocalPlayer = player.whoAmI == Main.myPlayer;
      Handler = handler;
    }
    public enum States {
      Inactive = 0,
      Starting = 1,
      Active = 2,
      Ending = 3,
      Failed = 4
    }
    public States State { get; internal set; }
    public bool unlocked { get; internal set; }
    internal virtual bool CanUse { get; set; }
    internal virtual bool Refreshed { get; set; }
    protected int currRand = 0; // Used for random sounds that don't repeat
    public bool IsState(params States[] stat) {
      foreach(int s in stat) {
        States v = (States)s;
        if (State == v) return true;
      }
      return false;
    }
    public bool InUse {
      get {
        return State != States.Inactive;
      }
    }
    protected virtual bool PreUpdate() {
      if (!CanUse && !InUse) return false;
      return true;
    }
    internal virtual bool Update() {
      if (!PreUpdate()) return false;
      switch (State) {
        case States.Active:
          UpdateActive();
          UpdateUsing();
          return true;
        case States.Starting:
          UpdateStarting();
          UpdateUsing();
          return true;
        case States.Ending:
          UpdateEnding();
          UpdateUsing();
          return true;
        case States.Failed:
          UpdateFailed();
          return true;
        default:
          return false;
      }
    }
    protected virtual void UpdateStarting() { }
    protected virtual void UpdateActive() { }
    protected virtual void UpdateEnding() { }
    protected virtual void UpdateFailed() { }
    protected virtual void UpdateUsing() { }
    internal abstract void Tick();
  }
  public sealed partial class MovementHandler {
    #region Variables

    
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
      // soulLink.Update();
    }
    public void WallJump() {
      // wJump.Update();
    }
    public void ChargeFlame() {
      // cFlame.Update();
    }
    public void AirJump() {
      airJump.Update();
    }
    public void Bash() {
    }
    public void Stomp() {
      stomp.Update();
    }
    public void Glide() {
      glide.Update();
    }
    public void Climb() {
      climb.Update();
    }
    public void ChargeJump() {
      // cJump.Update();
    }
    public void Dash() {
      dash.Update();
    }
    public void ChargeDash() {
      cDash.Update();
    }
    public void Grenade(int mouseX=0, int mouseY=0) {
      Grenade(new Vector2(mouseX, mouseY));
    }
    public void Grenade(Vector2 mousePos) {
      // grenade.Update();
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
      // wJump.Tick();
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
        // soulLink.CanUse = false;
        // wJump.CanUse = false;
        airJump.CanUse = false;
        stomp.CanUse = false;
        glide.CanUse = false;
        climb.CanUse = false;
        // cJump.CanUse = false;
        dash.CanUse = false;
        cDash.CanUse = false;
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
      if (glide.InUse) {
        Glide();
      }
    }
  }
}