using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities {
  public class Burrow : Ability {
    public Burrow(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    public static readonly int[] Burrowable = new int[] { 0, TileID.Sand, TileID.Ebonsand, TileID.Crimsand, TileID.Pearlsand, TileID.Silt, TileID.Slush };
    private const float Speed = 6f; 
    private int Time = 0;
    internal Vector2 Velocity = Vector2.Zero;
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Handler.dash.InUse && !Handler.cDash.InUse;
    
    protected override void UpdateActive() {
      Vector2 oldVel = Velocity;
      Vector2 newVel = Vector2.Zero;
      if (player.controlLeft) {
        newVel.X -= 1;
      }
      if (player.controlRight) {
        newVel.X += 1;
      }
      if (player.controlUp) {
        newVel.Y -= 1;
      }
      if (player.controlDown) {
        newVel.Y += 1;
      }
      if (newVel == Vector2.Zero) {
        if (oldVel == Vector2.Zero) {
          oldVel = Velocity = new Vector2(0, Speed);
        }
        newVel = oldVel;
      }
      oldVel.Normalize();
      newVel.Normalize();
      newVel = Vector2.Lerp(oldVel, newVel, 0.2f);
      Velocity = newVel *= Speed;
      player.position += Velocity;
      player.velocity = Vector2.Zero;
      Main.NewText("Burrowing!");
    }
    protected override void UpdateUsing() {
      player.buffImmune[BuffID.Suffocation] = true;
      player.noItems = true;
      player.gravity = 0;
      player.controlJump = false;
      player.velocity.X = 0;
      player.velocity.Y = 0;
      player.controlUseItem = false;
      player.controlUseTile = false;
      player.controlThrow = false;
      player.grappling[0] = -1;
      player.grapCount = 0;
    }
    internal override void Tick() {
      if (InUse) {
        Time++;
        if (Time > 5) {
          Vector2 tilePos = player.position.ToTileCoordinates().ToVector2();
          Tile tile = Main.tile[(int)tilePos.X, (int)tilePos.Y];
          if (tile.type == 0 || player.dead) {
            Main.NewText("No longer burrowing!");
            State = States.Inactive;
          }
        }
      }
      if (CanUse && OriMod.BurrowKey.JustPressed) {
        Vector2 left = new Vector2(player.Left.X, player.Bottom.Y + 4).ToTileCoordinates().ToVector2();
        Vector2 right = new Vector2(player.Right.X, player.Bottom.Y + 4).ToTileCoordinates().ToVector2();
        bool canBurrow = true;
        bool isAllAir = true;
        for (int x = (int)left.X; x <= right.X; x++) {
          Tile tile = Main.tile[x, (int)left.Y];
          if (tile.type == 0) continue;
          if (!tile.active() || !tile.nactive() || tile.inActive()) continue;
          isAllAir = false;
          if (!Burrowable.Contains(tile.type)) {
            canBurrow = false;
            break;
          }
        }
        if (canBurrow && !isAllAir) {
          Main.NewText("Can burrow");
          State = States.Active;
          player.position.Y += 48;
          Velocity = new Vector2(0, 48);
        }
        else {
          Main.NewText("Cannot burrow");
        }
      }
    }
  }
}