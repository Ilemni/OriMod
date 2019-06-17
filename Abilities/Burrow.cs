using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities {
  public class Burrow : Ability {
    public Burrow(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    public static readonly int[] Burrowable = new int[] { TileID.Sand, TileID.Ebonsand, TileID.Crimsand, TileID.Pearlsand, TileID.Silt, TileID.Slush };
    private const float Speed = 6f; 
    private int TimeUntilCheck = 0;
    private int TimeUntilEnd = 0;
    internal Vector2 Velocity = Vector2.Zero;
    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && !Handler.dash.InUse && !Handler.cDash.InUse;
    private Vector2 Normalize(Vector2 vector) {
      Vector2 v = vector;
      v.Normalize();
      return v;
    }
    internal Vector2 FrontTilePos {
      get {
        Vector2 pos = player.Center + Normalize(Velocity) * (player.height);
        pos.X = (int)(pos.X / 16);
        pos.Y = (int)(pos.Y / 16);
        return pos;
      }
    }
    internal Vector2[] FrontTilePosArr = new Vector2[0];
    private static readonly Vector2[] HitboxPos = new Vector2[] {
      new Vector2(0, -2), // Top
      new Vector2(0, 2),  // Bottom
      new Vector2(-2, 0), // Left
      new Vector2(2, 0),  // Right
      new Vector2(1, 1),  // Corners
      new Vector2(1, -1),
      new Vector2(-1, 1),
      new Vector2(-1, -1),
    };
    internal void UpdateFrontTilePosArray() {
      Vector2 pos = player.Center + Normalize(Velocity) * 16;
      pos.X = (int)(pos.X / 16);
      pos.Y = (int)(pos.Y / 16);
      List<Vector2> posList = new List<Vector2>();
      foreach(Vector2 v in HitboxPos) {
        posList.Add(pos + v);
      }
      FrontTilePosArr = posList.ToArray();
    }
    private void OnBurrowCollision(int hitboxIdx, ref bool didX, ref bool didY) {
      oPlayer.Debug("Bounce! " + hitboxIdx);
      switch (hitboxIdx) {
        case 0: // Top
        case 1: // Bottom
          if (!didY) {
            Velocity.Y = -Velocity.Y;
            didY = true;
          }
          break;
        case 2: // Left
        case 3: // Right
          if (!didX) {
            Velocity.X = -Velocity.X;
            didX = true;
          }
          break;
        default: // Corners
          if (!didX) {
            Velocity.X = -Velocity.X;
            didX = true;
          }
          if (!didY) {
            Velocity.Y = -Velocity.Y;
            didY = true;
          }
          break;
      }
    }
    internal Tile FrontTile {
      get {
        Vector2 pos = FrontTilePos;
        return Main.tile[(int)pos.X, (int)pos.Y];
      }
    }
    internal Vector2 BackTilePos {
      get {
        Vector2 pos = player.Center - Normalize(Velocity) * (player.height);
        pos /= 16;
        return pos;
      }
    }
    internal Tile BackTile {
      get {
        Vector2 pos = BackTilePos;
        return Main.tile[(int)pos.X, (int)pos.Y];
      }
    }
    
    protected override void UpdateActive() {
      UpdateFrontTilePosArray();
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
      Vector2[] arr = FrontTilePosArr;
      bool didX = false;
      bool didY = false;
      for (int i = 0; i < arr.Length; i++) {
        Vector2 v = arr[i];
        Tile t = Main.tile[(int)v.X, (int)v.Y];
        if (!Burrowable.Contains(t.type)) {
          OnBurrowCollision(i, ref didX, ref didY);
        }
      }
      player.position += Velocity;
      player.velocity = Vector2.Zero;
    }
    protected override void UpdateEnding() {
      player.position += Velocity;
      if (!BackTile.active()) {
        player.velocity = Velocity * 1.25f;
      }
      oPlayer.UnrestrictedMovement = true;
    }
    protected override void UpdateUsing() {
      player.buffImmune[BuffID.Suffocation] = true;
      player.noItems = true;
      player.gravity = 0;
      player.controlJump = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
      player.controlThrow = false;
      player.grappling[0] = -1;
      player.grapCount = 0;
    }
    internal override void Tick() {
      if (InUse) {
        if (Active) {
          if (TimeUntilCheck > 0) {
            TimeUntilCheck--;
          }
          if (TimeUntilCheck == 0) {
            Tile tile = FrontTile;
            if (!tile.active() || player.dead) {
              oPlayer.Debug("No longer burrowing!");
              Ending = true;
              TimeUntilEnd = 10;
            }
          }
        }
        else if (Ending) {
          TimeUntilEnd--;
          if (TimeUntilEnd < 1) {
            Inactive = true;
          }
        }
      }
      if (CanUse && !InUse && OriMod.BurrowKey.JustPressed) {
        Vector2 left = new Vector2(player.Left.X, player.Bottom.Y + 4).ToTileCoordinates().ToVector2();
        Vector2 right = new Vector2(player.Right.X, player.Bottom.Y + 4).ToTileCoordinates().ToVector2();
        bool canBurrow = true;
        bool isAllAir = true;
        for (int x = (int)left.X; x <= right.X; x++) {
          Tile tile = Main.tile[x, (int)left.Y];
          if (!tile.active() || !tile.nactive() || tile.inActive()) continue;
          isAllAir = false;
          if (!Burrowable.Contains(tile.type)) {
            canBurrow = false;
            break;
          }
        }
        if (canBurrow && !isAllAir) {
          oPlayer.Debug("Can burrow");
          Active = true;
          TimeUntilCheck = 15;
          player.position.Y += 64;
          Velocity = new Vector2(0, 64);
        }
        else {
          oPlayer.Debug("Cannot burrow");
        }
      }
    }
  }
}