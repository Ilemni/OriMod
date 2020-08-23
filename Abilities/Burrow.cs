using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for traveling through solid terrain.
  /// </summary>
  /// <remarks>
  /// This ability was somewhat difficult to balance; the simplest solution was to restrict tiles to whatever pickaxe was in inventory.
  /// </remarks>
  public sealed class Burrow : Ability {
    internal Burrow(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Burrow;

    internal override bool UpdateCondition => InUse || oPlayer.Input(OriMod.BurrowKey.Current);
    internal override bool CanUse => base.CanUse && !Manager.dash.InUse && !Manager.chargeDash.InUse && !InMenu;
    protected override int Cooldown => 12;
    protected override Color RefreshColor => Color.SandyBrown;

    private static int MaxDuration => (int)(Config.BurrowDuration * 60);
    private static int UiIncrement => 60;
    private float Breath = MaxDuration;

    private bool InMenu => Main.ingameOptionsWindow || Main.inFancyUI || player.talkNPC >= 0 || player.sign >= 0 || Main.clothesWindow || Main.playerInventory;
    private float Speed => 8f;
    private float SpeedExitMultiplier => 1.5f;
    private int Strength;

    internal bool CanBurrow(Tile t) {
      if (CanBurrowAny && IsSolid(t)) {
        return true;
      }

      return TileCollection.Instance.TilePickaxeMin[t.type] <= Strength;
    }

    internal static bool CanBurrowAny => OriMod.ConfigAbilities.BurrowStrength < 0;
    internal static bool IsSolid(Tile tile) => tile.active() && !tile.inActive() && tile.nactive() && Main.tileSolid[tile.type];

    internal Vector2 Velocity;
    internal Vector2 LastPosition;
    internal bool AutoBurrow;
    private int TimeLeft;

    private static Point P(int x, int y) => new Point(x, y);
    internal static TileHitbox EnterHitbox => _eh ?? (_eh = new TileHitbox(
      P(0, -1), P(0, 0), P(0, 1), // Center
      P(-1, -1), P(-1, 0), P(-1, 1), // Left
      P(2, -1), P(2, 0), P(2, 1),  // Right
      P(0, -2), P(1, -2), // Top
      P(0, 2), P(1, 2),  // Bottom
      P(2, 2), P(2, -2), P(-1, 2), P(-1, -2) // Corners
    ));
    internal static TileHitbox OuterHitbox => _oh ?? (_oh = new TileHitbox(
      P(-2, -2), P(-2, -1), P(-2, 0), P(-2, 1), P(-2, 2),  // Left
      P(3, -2), P(3, -1), P(3, 0), P(3, 1), P(3, 2),   // Right
      P(-1, -2), P(0, -2), P(1, -2), P(2, -2),  // Top
      P(-1, 3), P(0, 3), P(1, 3), P(2, 3),   // Bottom
      P(3, 3), P(3, -3), P(-2, 3), P(-2, -3) // Corners
    ));
    internal static TileHitbox InnerHitbox => _ih ?? (_ih = new TileHitbox(
      P(0, -1), // Top
      P(0, 1),  // Bottom
      P(-1, 0), // Left
      P(1, 0)  // Right
    ));
    private static TileHitbox _eh;
    private static TileHitbox _oh;
    private static TileHitbox _ih;

    private void OnCollision(int hitboxIdx, ref bool didX, ref bool didY) {
      oPlayer.Debug("Bounce! " + hitboxIdx);
      switch (hitboxIdx) {
        case 0: // Top
        case 1: // Bottom
          if (!didY) {
            didY = true;
            Velocity.Y = -Velocity.Y;
          }
          break;
        case 2: // Left
        case 3: // Right
          if (!didX) {
            didX = true;
            Velocity.X = -Velocity.X;
          }
          break;
        default: // Corners
          if (!didX) {
            didX = true;
            Velocity.X = -Velocity.X;
          }
          if (!didY) {
            didY = true;
            Velocity.Y = -Velocity.Y;
          }
          break;
      }
    }

    protected override void ReadPacket(System.IO.BinaryReader r) {
      if (InUse) {
        player.position = r.ReadVector2();
      }
    }

    protected override void WritePacket(ModPacket packet) {
      if (InUse) {
        packet.WriteVector2(player.position);
      }
    }

    protected override void UpdateActive() {
      EnterHitbox.UpdateHitbox(player.Center);
      OuterHitbox.UpdateHitbox(player.Center);
      InnerHitbox.UpdateHitbox(player.Center + Velocity.Normalized() * 16);

      // Get intended velocity based on input
      var newVel = Vector2.Zero;
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
        if (player.controlUp) {
          newVel.Y -= player.gravDir;
        }
        if (player.controlDown) {
          newVel.Y += player.gravDir;
        }
      }

      if (newVel != Vector2.Zero) {
        Velocity = Vector2.Lerp(Velocity.Normalized(), newVel.Normalized(), 0.1f) * Speed;
      }

      // Detect bouncing
      if (!CanBurrowAny) {
        bool didX = false;
        bool didY = false;
        var innerPoints = InnerHitbox.Points;
        for (int i = 0, len = innerPoints.Length; i < len; i++) {
          Point point = innerPoints[i];
          Tile tile = Main.tile[point.X, point.Y];
          if (!CanBurrow(tile)) {
            OnCollision(i, ref didX, ref didY);
          }
        }
      }

      // Apply changes
      player.position += Velocity;
      player.velocity = Vector2.Zero;
      LastPosition = player.position;
      oPlayer.CreatePlayerDust();

      // Breath
      if (Breath > 0) {
        Breath--;
      }
    }

    protected override void UpdateEnding() {
      // Runs when leaving solid tiles
      player.velocity = Velocity * SpeedExitMultiplier;
      player.direction = Math.Sign(Velocity.X);
      oPlayer.UnrestrictedMovement = true;
    }

    protected override void UpdateUsing() {
      // Manage suffocation debuff
      if (Breath > 0) {
        player.buffImmune[BuffID.Suffocation] = true;
      }
      else {
        player.AddBuff(BuffID.Suffocation, 1);
      }

      // Disable actions while burrowing
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

    internal override void DrawEffects() {
      if (Breath != MaxDuration) {

        // UI indication for breath
        Vector2 drawAnchor = player.BottomRight - Main.screenPosition;
        drawAnchor.X += 48;
        drawAnchor.Y += 16;

        var color = Color.White;
        if (!InUse) {
          color *= 0.6f;
        }

        Vector2 drawPos = drawAnchor;
        int uiCount = (int)Math.Ceiling((double)Breath / UiIncrement);
        for (int i = 0; i < uiCount; i++) {
          if (i % 10 == 0) {
            drawPos.X = drawAnchor.X;
            drawPos.Y += 40;
          }
          drawPos.X += 24;
          int frameY = 0;

          // Different frameY if this represents a partially filled bar
          if (i * UiIncrement + UiIncrement > Breath) {
            frameY = 4 - (int)Breath % UiIncrement / (UiIncrement / 5);
          }

          var tex = OriTextures.Instance.BurrowTimer.texture;
          Main.playerDrawData.Add(new DrawData(tex, drawPos, tex.Frame(3, 5, (int)Main.time % 30 / 10, frameY), color, 0, tex.Size() / 2, 1, SpriteEffects.None, 0));
        }
      }
    }

    public void UpdateBurrowStrength(bool force = false) {
      if (force || (int)Main.time % 64 == 0) {
        int pick = OriMod.ConfigAbilities.BurrowStrength;
        foreach (Item item in player.inventory) {
          if (item.pick > pick) {
            pick = item.pick;
          }
        }
        Strength = pick;
      }
    }

    internal override void Tick() {
      if (InUse) {
        UpdateBurrowStrength();
        Manager.glide.SetState(State.Inactive);

        if (Active) {
          bool canBurrow = false;
          var points = InnerHitbox.Points;
          foreach (Point p in points) {
            if (IsSolid(Main.tile[p.X, p.Y])) {
              canBurrow = true;
              break;
            }
          }
          if (!canBurrow || player.dead) {
            SetState(State.Ending);
            TimeLeft = 2;
          }
        }
        else if (Ending) {
          TimeLeft--;
          if (TimeLeft < 1) {
            SetState(State.Inactive);
            PutOnCooldown();
          }
        }

        if ((int)Main.time % 20 == 0) {
          netUpdate = true;
        }
      }
      else {
        // Not in use
        TickCooldown();

        if (CanUse && (OriMod.BurrowKey.JustPressed && Manager.crouch.InUse || AutoBurrow)) {
          UpdateBurrowStrength(force: true);
          EnterHitbox.UpdateHitbox(player.position);

          // Check if player can enter Burrow
          Vector2 vel = Vector2.Zero;
          var enterPoints = EnterHitbox.Points;
          for (int i = 0, len = enterPoints.Length; i < len; i++) {
            Point p = enterPoints[i];
            Tile t = Main.tile[p.X, p.Y];
            if (CanBurrow(t)) {
              vel += p.ToVector2().Normalized();
            }
          }

          if (vel != Vector2.Zero) {
            // Enter Burrow
            SetState(State.Active);
            currentCooldown = Cooldown;

            AutoBurrow = OriMod.ConfigClient.AutoBurrow && OriMod.BurrowKey.Current;
            if (AutoBurrow) {
              vel = player.velocity.Normalized();
            }
            else {
              // Set velocity based on surrounding terrain
              var outerPoints = OuterHitbox.Points;
              for (int i = 0, len = outerPoints.Length; i < len; i++) {
                Point p = outerPoints[i];
                Tile t = Main.tile[p.X, p.Y];
                if (CanBurrow(t)) {
                  vel += p.ToVector2().Normalized();
                }
              }
              vel.Normalize();
            }
            Velocity = vel * Speed;
            player.position += Velocity;
            LastPosition = player.position;
          }
        }

        if (Breath < MaxDuration) {
          Breath += Config.BurrowRecoveryRate;
          if (Breath > MaxDuration) {
            Breath = MaxDuration;
          }
        }
      }
    }

    internal static void Unload() {
      _eh = null;
      _oh = null;
      _ih = null;
    }
  }
}
