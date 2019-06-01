using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OriMod.Movements;

namespace OriMod {
  // This partial class is for how movement is handled. I'd recommend not modifying this code.
  public partial class MovementHandler {
    public OriPlayer oPlayer;
    public Player player;
    public List<Movement> Movements;
    public Movement airJump;
    public Movement glide;
    public Movement stomp;
    public Movement climb;
    public Movement dash;
    public ChargeDash cDash;
    public MovementHandler(OriPlayer o) {
      oPlayer = o;
      player = o.player;
      movementStates  = new Dictionary<string, int[]> {
        { "WallJump", new int[] { 1, 0 } },
        { "ChargeJump", new int[] { 1, 0 } },
      };
      Movements = new List<Movement> {
        { airJump = new AirJump(o, this) },
        { glide = new Glide(o, this) },
        { stomp = new Stomp(o, this) },
        { climb = new Climb(o, this) },
        { dash = new Dash(o, this) },
        { cDash = new ChargeDash(o, this) }
      };
    }
    private int GetValue(string move, int idx) {
      int[] value = new int[1];
      bool success = movementStates.TryGetValue(move, out value);
      if (!success) {
        Main.NewText("Invalid use of GetValue in AbilityHandler: " + move + " is not a valid ability name.");
        return 0;
      }
      if (idx > value.Length - 1 || idx < 0) {
        Main.NewText("Invalid use of GetValue in AbilityHandler: " + idx + " is not a valid index for " + move + ".", Color.Red);
        return 0;
      }
      return value[idx];
    }
    private void SetValue(string move, int idx, int value) {
      if (GetValue(move, idx) == value) return;
      if (idx > movementStates[move].Length || idx < 0) {
        Main.NewText("Invalid use of SetValue in AbilityHandler: " + idx + " is not a valid index for " + move + ".", Color.Red);
      }
      movementStates[move][idx] = value;
    }
    public bool IsState(string move, params State[] stat) {
      int v = GetValue(move, 1);
      foreach(int s in stat) {
        if (v == s) return true;
      }
      return false;
    }
    public bool IsUnlocked(string move) {
      return GetValue(move, 0) == 1;
    }
    public bool CanUse(string move) {
      return IsState(move, State.CanUse);
    }
    public bool IsInUse(string move) {
      return IsState(move, State.Starting, State.Active, State.Ending);
    }
    public State GetState(string move) {
      return (State)GetValue(move, 1);
    }
    public void SetUnlock(string move, int value) {
      SetValue(move, 0, value);
    }
    public void SetState(string move, int value) {
      if ((int)GetState(move) == value) return;
      SetValue(move, 1, value);
    }
    public void SetState(string move, State value) {
      if (IsState(move, value)) return;
      SetValue(move, 1, (int)value);
    }
    public void UseMovement(string move) { // To be used my players that are not Main.myPlayer
      if (player.whoAmI == Main.myPlayer) {
        return;
      }
      switch (move) {
        case "SoulLink":
          SoulLink();
          break;
        case "WallJump":
          WallJump();
          break;
        case "ChargeFlame":
          ChargeFlame();
          break;
        case "AirJump":
          // OriMod.Log("Calling AirJump() from UseMovement() for " + oPlayer.player.name);
          AirJump();
          break;
        case "Bash":
          Bash();
          break;
        case "Stomp":
          Stomp();
          break;
        case "Glide":
          Glide();
          break;
        case "Climb":
          Climb();
          break;
        case "ChargeJump":
          ChargeJump();
          break;
        case "Dash":
          Dash();
          break;
        case "ChargeDash":
          ChargeDash();
          break;
        case "Grenade":
          Grenade();
          break;
        default:
          Main.NewText("MovementHandler.UseMovement(): Invalid Move " + move, Color.Red);
          break;
      }
    }
  }
}