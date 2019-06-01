using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OriMod.Movements;

namespace OriMod {
  // This partial class is for how movement is handled. I'd recommend not modifying this code.
  public sealed partial class MovementHandler {
    public OriPlayer oPlayer { get; private set; }
    public Player player { get; private set; }
    public List<Ability> Movements { get; private set; }
    public Ability wJump { get; private set; }
    public Ability airJump { get; private set; }
    public Ability glide { get; private set; }
    public Ability stomp { get; private set; }
    public Ability climb { get; private set; }
    public Ability dash { get; private set; }
    public ChargeDash cDash { get; private set; }
    internal MovementHandler(OriPlayer o) {
      oPlayer = o;
      player = o.player;
      Movements = new List<Ability> {
        { wJump = new WallJump(o, this) },
        { airJump = new AirJump(o, this) },
        { glide = new Glide(o, this) },
        { stomp = new Stomp(o, this) },
        { climb = new Climb(o, this) },
        { dash = new Dash(o, this) },
        { cDash = new ChargeDash(o, this) }
      };
    }
    internal void UseMovement(string move) { // To be used my players that are not Main.myPlayer
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