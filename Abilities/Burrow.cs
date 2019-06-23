using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities {
  public class Burrow : Ability {
    internal Burrow(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool DoUpdate => InUse || oPlayer.Input(OriMod.BurrowKey.Current);
    public static readonly ushort[][] Burrowable = new ushort[][] {
      new ushort[] {
        TileID.Sand, TileID.Ebonsand, TileID.Crimsand, TileID.Pearlsand, TileID.Silt, TileID.Slush,
        TileID.LeafBlock, TileID.LivingMahoganyLeaves
      },
      new ushort[] {
        TileID.Dirt, TileID.Mud,
        TileID.Grass, TileID.CorruptGrass, TileID.FleshGrass, TileID.HallowedGrass, TileID.JungleGrass, TileID.MushroomGrass
      },
      new ushort[] {
        TileID.LivingWood, TileID.LivingMahogany,
        TileID.Stone, TileID.ActiveStoneBlock, TileID.Ebonstone, TileID.Pearlstone, TileID.Crimstone,
        TileID.Sandstone, TileID.CorruptSandstone, TileID.CrimsonSandstone, TileID.HallowSandstone,
        TileID.Hellstone
      }
    };
    internal static bool CanBurrowAny => Config.BurrowTier < 0;
    public static List<ushort> CurrentBurrowable = new List<ushort>();
    private const float Speed = 8f; 
    private const float SpeedExitMultiplier = 1.5f;
    private int TimeUntilEnd = 0;
    internal Vector2 Velocity = Vector2.Zero;
    internal override bool CanUse => base.CanUse && !Handler.dash.InUse && !Handler.cDash.InUse;
    protected override int Cooldown => 12;
    protected override Color RefreshColor => Color.SandyBrown;
    internal bool AutoBurrow = false;
    internal bool IsSolid(Tile tile) => tile.active() && !tile.inActive() && tile.nactive() && Main.tileSolid[tile.type];
    private static Point p(int x, int y) => new Point(x, y);
    private static readonly Point[] BurrowEnterTemplate = new Point[] {
      p(0, -1),  p(0, 0),  p(0, 1),   // Center
      p(-1, -1), p(-1, 0), p(-1, 1), // Left
      p(2, -1),  p(2, 0),  p(2, 1),  // Right
      p(0, -2),  p(1, -2), // Top
      p(0, 2),   p(1, 2),  // Bottom
      p(2, 2),   p(2, -2), p(-1, 2),  p(-1, -2), // Corners
    };
    public static readonly Point[] BurrowEnterOuterTemplate = new Point[] {
      p(-2, -2), p(-2, -1), p(-2, 0), p(-2, 1), p(-2, 2),  // Left
      p(3, -2),  p(3, -1),  p(3, 0),  p(3, 1),  p(3, 2),   // Right
      p(-1, -2), p(0, -2),  p(1, -2), p(2, -2),  // Top
      p(-1, 3),  p(0, 3),   p(1, 3),  p(2, 3),   // Bottom
      p(3, 3),   p(3, -3),  p(-2, 3), p(-2, -3), // Corners
    };
    private static readonly Point[] BurrowInnerTemplate = new Point[] {
      p(0, -1), // Top
      p(0, 1),  // Bottom
      p(-1, 0), // Left
      p(1, 0),  // Right
    };
    internal Point[] BurrowEnter = new Point[BurrowEnterTemplate.Length];
    internal Point[] BurrowEnterOuter = new Point[BurrowEnterOuterTemplate.Length];
    internal Point[] BurrowInner = new Point[BurrowInnerTemplate.Length];
    internal static void UpdateBurrowableTiles(int tier) {
      CurrentBurrowable.Clear();
      for (int i = 0; i < tier + 1; i++) {
        CurrentBurrowable.AddRange(Burrowable[i]);
      }
    }
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
    internal void UpdateBurrowEnterBox() {
      UpdateBox(ref BurrowEnter, BurrowEnterTemplate, player.Center);
      UpdateBox(ref BurrowEnterOuter, BurrowEnterOuterTemplate, player.Center);
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
      UpdateBurrowInnerBox();
      if (Velocity == Vector2.Zero) {
        Velocity = new Vector2(0, Speed);
      }
      Vector2 oldVel = Velocity;
      Vector2 newVel = Vector2.Zero;
      if (Config.BurrowToMouse) {
        newVel = player.AngleTo(Main.MouseWorld).ToRotationVector2();
      }
      else {
        if (player.controlLeft) {
          newVel.X -= 1;
        }
        if (player.controlRight) {
          newVel.X += 1;
        }
        if (oPlayer.Input(OriPlayer.Current.Up)) {
          newVel.Y -= player.gravDir;
        }
        if (player.controlDown) {
          newVel.Y += player.gravDir;
        }
        if (newVel == Vector2.Zero) {
          newVel = oldVel;
        }
      }
      oldVel.Normalize();
      newVel.Normalize();
      newVel = Vector2.Lerp(oldVel, newVel, 0.1f);
      Velocity = newVel *= Speed;
      bool didX = false;
      bool didY = false;
      if (!CanBurrowAny) {
        for (int i = 0; i < BurrowInner.Length; i++) {
          Point v = BurrowInner[i];
          Tile t = Main.tile[(int)v.X, (int)v.Y];
          // if (i == 0) oPlayer.Debug(!t.active() + " || " + t.inActive() + " || " + !t.nactive());
          if (!IsSolid(t)) continue;
          if (!CurrentBurrowable.Contains(t.type)) {
            OnBurrowCollision(i, ref didX, ref didY);
          }
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
      player.controlUp = false;
      player.grappling[0] = -1;
      player.grapCount = 0;
    }
    internal override void Tick() {
      if (AutoBurrow && !OriMod.BurrowKey.Current) {
        AutoBurrow = false;
      }
      if (InUse) {
        Handler.glide.Inactive = true;
        if (Active) {
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
        else if (Ending) {
          TimeUntilEnd--;
          if (TimeUntilEnd < 1) {
            Inactive = true;
          }
        }
      }
      else {
        if (CurrCooldown > Cooldown) {
          CurrCooldown--;
        }
        else {
          Refreshed = true;
        }
      }
      if (CanUse && !InUse && (OriMod.BurrowKey.JustPressed || AutoBurrow)) {
        UpdateBurrowEnterBox();
        Vector2 vel = Vector2.Zero;
        for (int i = 0; i < BurrowEnterTemplate.Length; i++) {
          Point v = BurrowEnter[i];
          Tile t = Main.tile[v.X, v.Y];
          if (IsSolid(t) && (CanBurrowAny || CurrentBurrowable.Contains(t.type))) {
            vel += BurrowEnterTemplate[i].ToVector2().Norm();
          }
        }
        if (vel == Vector2.Zero) {
          oPlayer.Debug("Cannot burrow");
          return;
        }
        oPlayer.Debug("Can burrow");
        if (Config.AutoBurrow) AutoBurrow = true;
        Active = true;
        Refreshed = false;
        CurrCooldown = Cooldown;
        if (AutoBurrow) {
          vel = player.velocity.Norm();
        }
        if (!AutoBurrow || vel.HasNaNs()) {
          for (int i = 0; i < BurrowEnterOuterTemplate.Length; i++) {
            Point v = BurrowEnterOuter[i];
            Tile t = Main.tile[v.X, v.Y];
            if (IsSolid(t) && (CanBurrowAny || CurrentBurrowable.Contains(t.type))) {
              vel += BurrowEnterOuterTemplate[i].ToVector2().Norm();
            }
          }
          vel.Normalize();
        }
        if (vel.HasNaNs()) {
          vel = Vector2.UnitY;
        }
        vel = vel.Norm() * 64;
        player.position += vel;
        Velocity = vel;
      }
    }
  }
}