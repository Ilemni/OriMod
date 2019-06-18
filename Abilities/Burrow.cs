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
    private const float Speed = 8f; 
    private const float SpeedExitMultiplier = 1.5f;
    private int TimeUntilCheck = 0;
    private int TimeUntilEnd = 0;
    internal Vector2 Velocity = Vector2.Zero;
    internal override bool CanUse => base.CanUse && !Handler.dash.InUse && !Handler.cDash.InUse;
    internal bool IsSolid(Tile tile) => tile.active() && !tile.inActive() && tile.nactive() && Main.tileSolid[tile.type];
    private static readonly Point[] HitboxTemplate = new Point[] {
      new Point(0, -2), // Top
      new Point(0, 2),  // Bottom
      new Point(-2, 0), // Left
      new Point(2, 0),  // Right
      new Point(1, 1),  // Corners
      new Point(1, -1),
      new Point(-1, 1),
      new Point(-1, -1),
    };
    private static readonly Point[] BurrowEnterTemplate = new Point[] {
      new Point(-1, 1), // Top
      new Point(-1, 0),
      new Point(2, 1), // Bottom
      new Point(2, 0),
      new Point(1, -1), // Left
      new Point(0, -1),
      new Point(1, 2), // Right
      new Point(0, 2),
      new Point(2, 2), // Corners
      new Point(2, -1),
      new Point(-1, 2),
      new Point(-1, -1),
    };
    private static readonly Point[] BurrowInnerTemplate = new Point[] {
      new Point(-1, -1),
      new Point(-1, 0),
      new Point(-1, 1),
      new Point(0, -1),
      new Point(0, 0),
      new Point(0, 1),
      new Point(1, -1),
      new Point(1, 0),
      new Point(1, 1),
    };
    internal Point[] Hitbox = new Point[HitboxTemplate.Length];
    internal Point[] BurrowEnter = new Point[BurrowEnterTemplate.Length];
    internal Point[] BurrowInner = new Point[BurrowInnerTemplate.Length];
    internal void UpdateBox(ref Point[] Box, Point[] Template, Vector2 pos) {
      UpdateBox(ref Box, Template, pos.ToTileCoordinates());
    }
    internal void UpdateBox(ref Point[] Box, Point[] Template, Point pos) {
      List<Point> posList = new List<Point>();
      foreach(Point v in Template) {
        posList.Add(pos.Add(v));
      }
      Box = posList.ToArray();
    }
    internal void UpdateHitbox() {
      Vector2 pos = (player.Center + Velocity.Norm() * 16);
      UpdateBox(ref Hitbox, HitboxTemplate, pos);
      List<Point> posList = new List<Point>();
    }
    internal void UpdateBurrowEnterBox() {
      UpdateBox(ref BurrowEnter, BurrowEnterTemplate, player.Center);
    }
    internal void UpdateBurrowInnerBox() {
      UpdateBox(ref BurrowInner, BurrowInnerTemplate, player.Center + Velocity.Norm() * 16);
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
      UpdateBurrowInnerBox();
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
        Point v = Hitbox[i];
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
      player.velocity = Velocity * SpeedExitMultiplier;
      player.direction = Math.Sign(Velocity.X);
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
            bool canBurrow = false;
            foreach(Point p in BurrowInner) {
              if (IsSolid(Main.tile[p.X, p.Y])) {
                canBurrow = true;
              }
            }
            if (!canBurrow || player.dead) {
              oPlayer.Debug("No longer burrowing!");
              Ending = true;
              TimeUntilEnd = 2;
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
        UpdateBurrowEnterBox();
        Vector2 vel = Vector2.Zero;
        for (int i = 0; i < BurrowEnterTemplate.Length; i++) {
          Point v = BurrowEnter[i];
          Tile t = Main.tile[(int)v.X, (int)v.Y];
          if (IsSolid(t) && Burrowable.Contains(t.type)) {
            vel = vel.Add(BurrowEnterTemplate[i]);
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