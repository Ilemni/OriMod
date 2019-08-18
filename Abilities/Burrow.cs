using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  public class Burrow : Ability {
    internal Burrow(OriAbilities handler) : base(handler) { }
    public override int id => AbilityID.Burrow;
    internal override bool DoUpdate => InUse || oPlayer.Input(OriMod.BurrowKey.Current);
    internal override bool CanUse => base.CanUse && !Handler.dash.InUse && !Handler.cDash.InUse && !inMenu;
    protected override int Cooldown => 12;
    protected override Color RefreshColor => Color.SandyBrown;

    private static int BurrowDurMax => (int)(Config.BurrowDuration * 60);
    private static int UiIncrement => 60;
    private static int UiMax { get; } = BurrowDurMax / UiIncrement;
    private float BurrowDur = BurrowDurMax;
    
    private bool inMenu => Main.ingameOptionsWindow || Main.inFancyUI || player.talkNPC >= 0 || player.sign >= 0 || Main.clothesWindow || Main.playerInventory;
    private float Speed => 8f; 
    private float SpeedExitMultiplier => 1.5f;
    protected int Strength;
    
    internal bool CanBurrow(Tile t) {
      if (CanBurrowAny) return true;
      if (TileCollection.TilePickaxeMin[t.type] == -1) {
        ModTile mt = TileLoader.GetTile(t.type);
        if ((mt?.Type ?? -1) != -1) {
          TileCollection.AddModTile(mt);
        }
        else return true;
      }
      return TileCollection.TilePickaxeMin[t.type] <= Strength;
    }
    internal static bool CanBurrowAny => OriMod.ConfigAbilities.BurrowStrength < 0;
    internal static bool IsSolid(Tile tile) => tile.active() && !tile.inActive() && tile.nactive() && Main.tileSolid[tile.type];
    
    internal Vector2 Velocity;
    internal Vector2 LastPos;
    internal bool AutoBurrow;
    private int TimeUntilEnd;
    
    private static Point p(int x, int y) => new Point(x, y);
    private static readonly Point[] BurrowEnterTemplate = new Point[] {
      p(0, -1),  p(0, 0),  p(0, 1),   // Center
      p(-1, -1), p(-1, 0), p(-1, 1), // Left
      p(2, -1),  p(2, 0),  p(2, 1),  // Right
      p(0, -2),  p(1, -2), // Top
      p(0, 2),   p(1, 2),  // Bottom
      p(2, 2),   p(2, -2), p(-1, 2),  p(-1, -2), // Corners
    };
    private static readonly Point[] BurrowEnterOuterTemplate = new Point[] {
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
    
    private void UpdateBurrowEnterBox() {
      BurrowEnter.UpdateHitbox(BurrowEnterTemplate, player.Center);
      BurrowEnterOuter.UpdateHitbox(BurrowEnterOuterTemplate, player.Center);
    }
    private void UpdateBurrowInnerBox() =>
      BurrowInner.UpdateHitbox(BurrowInnerTemplate, player.Center + Velocity.Norm() * 16);
    
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
    
    protected override void ReadPacket(System.IO.BinaryReader r) {
      if (InUse) {
        player.position = r.ReadPackedVector2();
      }
    }
    protected override void WritePacket(ModPacket packet) {
      if (InUse) {
        packet.WritePackedVector2(player.position);
      }
    }

    protected override void UpdateActive() {
      if (BurrowDur > 0) {
        BurrowDur--;
      }
      player.position = LastPos;
      UpdateBurrowInnerBox();
      if (Velocity == Vector2.Zero) {
        Velocity = new Vector2(0, Speed);
      }
      Vector2 oldVel = Velocity;
      Vector2 newVel = Vector2.Zero;
      if (OriMod.ConfigClient.BurrowToMouse) {
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
          Point p = BurrowInner[i];
          Tile t = Main.tile[p.X, p.Y];
          // if (i == 0) oPlayer.Debug(!t.active() + " || " + t.inActive() + " || " + !t.nactive());
          if (!IsSolid(t)) continue;
          if (!CanBurrow(t)) {
            OnBurrowCollision(i, ref didX, ref didY);
          }
        }
      }
      player.position += Velocity;
      player.velocity = Vector2.Zero;
      LastPos = player.position;
      oPlayer.CreateTeatherDust();
    }
    protected override void UpdateEnding() {
      player.velocity = Velocity * SpeedExitMultiplier;
      player.direction = Math.Sign(Velocity.X);
      oPlayer.UnrestrictedMovement = true;
    }
    protected override void UpdateUsing() {
      if (BurrowDur > 0) {
        player.buffImmune[BuffID.Suffocation] = true;
      }
      else {
        player.AddBuff(BuffID.Suffocation, 1);
      }
      player.noItems = true;
      player.gravity = 0;
      player.controlJump = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
      player.controlThrow = false;
      player.controlUp = false;
      oPlayer.KillGrapples();
      player.grapCount = 0;
    }

    private Texture2D TimerTex => !_tex?.IsDisposed ?? false ? _tex : (_tex = OriMod.Instance.GetTexture("PlayerEffects/BurrowTimer"));
    private Texture2D _tex;
    internal override void DrawEffects() {
      if (BurrowDur == BurrowDurMax) return;
      
      Vector2 drawPos = player.BottomRight - Main.screenPosition;
      drawPos.X += 48;
      drawPos.Y += 16;
      Vector2 defaultDrawPos = drawPos;
      int uiCount = (int)Math.Ceiling((double)BurrowDur / UiIncrement);
      
      for (int i = 0; i < uiCount; i++) {
        if (i % 10 == 0) {
          drawPos.X = defaultDrawPos.X;
          drawPos.Y += 40;
        }
        drawPos.X += 24;
        var color = Color.White;
        int frameY = 0;
        if (i * UiIncrement + UiIncrement > BurrowDur) {
          frameY = 4 - ((int)BurrowDur % UiIncrement / (UiIncrement / 5));
        }
        if (!InUse) color *= 0.6f;
        var data = new DrawData(TimerTex, drawPos, TimerTex.Frame(3, 5, (int)Main.time % 30 / 10, frameY), color, 0, TimerTex.Size() / 2, 1, SpriteEffects.None, 0);
        Main.playerDrawData.Add(data);
      }
    }

    public void UpdateBurrowStrength(bool force=false) {
      if (!force && (int)Main.time % 64 != 0) return;
      int pick = OriMod.ConfigAbilities.BurrowStrength;
      foreach(Item item in player.inventory) {
        if (item.pick > pick) pick = item.pick;
      }
      Strength = pick;
    }
    internal override void Tick() {
      if (AutoBurrow && !OriMod.BurrowKey.Current) {
        AutoBurrow = false;
      }
      if (InUse) {
        if ((int)Main.time % 20 == 0) {
          netUpdate = true;
        }
        UpdateBurrowStrength();
        Handler.glide.Inactive = true;
        if (Active) {
          bool canBurrow = false;
          foreach(Point p in BurrowInner) {
            if (IsSolid(Main.tile[p.X, p.Y])) {
              canBurrow = true;
            }
          }
          if (!canBurrow || player.dead) {
            Ending = true;
            TimeUntilEnd = 2;
          }
        }
        else if (Ending) {
          TimeUntilEnd--;
          if (TimeUntilEnd < 1) {
            Inactive = true;
            PutOnCooldown();
          }
        }
      }
      else {
        TickCooldown();
        if (BurrowDur < BurrowDurMax) {
          BurrowDur += Config.BurrowRecoveryRate;
          if (BurrowDur > BurrowDurMax) {
            BurrowDur = BurrowDurMax;
          }
        }
      }
      if (CanUse && !InUse && (OriMod.BurrowKey.JustPressed || AutoBurrow)) {
        UpdateBurrowStrength(force:true);
        UpdateBurrowEnterBox();
        Vector2 vel = Vector2.Zero;
        for (int i = 0; i < BurrowEnterTemplate.Length; i++) {
          Point v = BurrowEnter[i];
          Tile t = Main.tile[v.X, v.Y];
          if (IsSolid(t) && CanBurrow(t)) {
            vel += BurrowEnterTemplate[i].ToVector2().Norm();
          }
        }
        if (vel == Vector2.Zero) {
          return;
        }
        if (OriMod.ConfigClient.AutoBurrow) AutoBurrow = true;
        Active = true;
        CurrCooldown = Cooldown;
        if (AutoBurrow) {
          vel = player.velocity.Norm();
        }
        if (!AutoBurrow || vel.HasNaNs()) {
          for (int i = 0; i < BurrowEnterOuterTemplate.Length; i++) {
            Point v = BurrowEnterOuter[i];
            Tile t = Main.tile[v.X, v.Y];
            if (IsSolid(t) && CanBurrow(t)) {
              vel += BurrowEnterOuterTemplate[i].ToVector2().Norm();
            }
          }
          vel.Normalize();
        }
        if (vel.HasNaNs()) {
          vel = Vector2.UnitY;
        }
        vel = vel.Norm() * Speed;
        player.position += vel;
        Velocity = vel;
        LastPos = player.position;
      }
    }

    public override void Dispose() {
      base.Dispose();
      if (_tex != null) {
        _tex.Dispose();
        _tex = null;
      }
    }
  }
}