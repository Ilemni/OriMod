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
    private bool IsSolid(Tile tile) => tile.active() && !tile.inActive() && tile.nactive();
    private static readonly Vector2[] HitboxPosTemplate = new Vector2[] {
      new Vector2(0, -2), // Top
      new Vector2(0, 2),  // Bottom
      new Vector2(-2, 0), // Left
      new Vector2(2, 0),  // Right
      new Vector2(1, 1),  // Corners
      new Vector2(1, -1),
      new Vector2(-1, 1),
      new Vector2(-1, -1),
    };
    private static readonly Vector2[] BurrowBoxTemplate = new Vector2[] {
      new Vector2(-1, 1), // Top
      new Vector2(-1, 0),
      new Vector2(2, 1), // Bottom
      new Vector2(2, 0),
      new Vector2(1, -1), // Left
      new Vector2(0, -1),
      new Vector2(1, 2), // Right
      new Vector2(0, 2),
      new Vector2(2, 2), // Corners
      new Vector2(2, -1),
      new Vector2(-1, 2),
      new Vector2(-1, -1),
    };
    internal Vector2[] Hitbox = new Vector2[HitboxPosTemplate.Length];
    internal Vector2[] BurrowBox = new Vector2[BurrowBoxTemplate.Length];
    internal Vector2 FrontTilePos => Hitbox[0];
    internal Tile FrontTile => Main.tile[(int)FrontTilePos.X, (int)FrontTilePos.Y];
    internal Vector2 BackTilePos => Hitbox[1];
    internal Tile BackTile => Main.tile[(int)BackTilePos.X, (int)BackTilePos.Y];
    internal void UpdateHitbox() {
      Vector2 pos = player.Center + Normalize(Velocity) * 16;
      pos.X = (int)(pos.X / 16);
      pos.Y = (int)(pos.Y / 16);
      List<Vector2> posList = new List<Vector2>();
      foreach(Vector2 v in HitboxPosTemplate) {
        posList.Add(pos + v);
      }
      Hitbox = posList.ToArray();
    }
    internal void UpdateEnterBurrowBox() {
      Vector2 pos = player.Center;
      pos.X = (int)(pos.X / 16);
      pos.Y = (int)(pos.Y / 16);
      List<Vector2> posList = new List<Vector2>();
      foreach(Vector2 v in BurrowBoxTemplate) {
        posList.Add(pos + v);
      }
      BurrowBox = posList.ToArray();

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
    protected override void UpdateActive() {
      UpdateHitbox();
      if (Velocity == Vector2.Zero) {
        Velocity = new Vector2(0, Speed);
      }
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
        newVel = oldVel;
      }
      oldVel.Normalize();
      newVel.Normalize();
      newVel = Vector2.Lerp(oldVel, newVel, 0.1f);
      Velocity = newVel *= Speed;
      bool didX = false;
      bool didY = false;
      for (int i = 0; i < Hitbox.Length; i++) {
        Vector2 v = Hitbox[i];
        Tile t = Main.tile[(int)v.X, (int)v.Y];
        // if (i == 0) oPlayer.Debug(!t.active() + " || " + t.inActive() + " || " + !t.nactive());
        if (!IsSolid(t)) continue;
        if (!Burrowable.Contains(t.type)) {
          OnBurrowCollision(i, ref didX, ref didY);
        }
      }
      player.position += Velocity;
      player.velocity = Vector2.Zero;
    }
    protected override void UpdateEnding() {
      player.position += Velocity;
      if (!IsSolid(Main.tile[(int)(player.Center.X / 16), (int)(player.Center.Y / 16)])) {
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
        UpdateEnterBurrowBox();
        Vector2 vel = Vector2.Zero;
        for (int i = 0; i < BurrowBoxTemplate.Length; i++) {
          Vector2 v = BurrowBox[i];
          Tile t = Main.tile[(int)v.X, (int)v.Y];
          if (IsSolid(t) && Burrowable.Contains(t.type)) {
            vel += BurrowBoxTemplate[i];
          }
        }
        if (vel == Vector2.Zero) {
          oPlayer.Debug("Cannot burrow");
          return;
        }
        oPlayer.Debug("Can burrow");
        Active = true;
        TimeUntilCheck = 15;
        vel.Normalize();
        vel *= 64;
        player.position += vel;
        Velocity = vel;
      }
    }
  }
}