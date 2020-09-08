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
  public sealed class Burrow : Ability, ILevelable {
    static Burrow() => OriMod.OnUnload += Unload;
    internal Burrow(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Burrow;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 1;

    internal override bool CanUse => base.CanUse && !abilities.dash.InUse && !abilities.chargeDash.InUse && !InMenu;
    protected override int Cooldown => 12;
    protected override Color RefreshColor => Color.SandyBrown;

    private static int MaxDuration => (int)(Config.BurrowDuration * 60);
    private static int UiIncrement => 60;
    private static float Speed => 8f;
    private static float SpeedExitMultiplier => 1.2f;

    private bool InMenu => Main.ingameOptionsWindow || Main.inFancyUI || player.talkNPC >= 0 || player.sign >= 0 || Main.clothesWindow || Main.playerInventory;

    private float breath = MaxDuration;
    private int strength {
      get {
        switch (Level) {
          case 0: return 0;
          case 1: return 55; // Exclude evil biomes, dungeon
          case 2: return 200; // Exclude lihzahrd
          default: return 100 + Level * 50;
        }
      }
    }

    internal static bool CanBurrowAny => OriMod.ConfigAbilities.BurrowStrength < 0;
    internal static bool IsSolid(Tile tile) => tile.active() && !tile.inActive() && tile.nactive() && Main.tileSolid[tile.type];

    internal bool CanBurrow(Tile t) => CanBurrowAny && IsSolid(t) || TileCollection.Instance.TilePickaxeMin[t.type] <= strength;

    internal Vector2 velocity;
    internal bool autoBurrow;

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

    /// <summary>
    /// Modify player velocity when they collide with a tile they cannot burrow through.
    /// </summary>
    /// <param name="hitboxIdx">Index of <see cref="InnerHitbox"/> point.</param>
    /// <param name="didX">Whether or not a collision has occured on the X axis.</param>
    /// <param name="didY">Whether or not a collision has occured on the Y axis.</param>
    private void OnCollision(int hitboxIdx, ref bool didX, ref bool didY) {
      oPlayer.Debug("Bounce! " + hitboxIdx);
      switch (hitboxIdx) {
        case 0: // Top
        case 1: // Bottom
          if (!didY) {
            didY = true;
            velocity.Y = -velocity.Y;
          }
          break;
        case 2: // Left
        case 3: // Right
          if (!didX) {
            didX = true;
            velocity.X = -velocity.X;
          }
          break;
        default: // Corners
          if (!didX) {
            didX = true;
            velocity.X = -velocity.X;
          }
          if (!didY) {
            didY = true;
            velocity.Y = -velocity.Y;
          }
          break;
      }
    }

    protected override void ReadPacket(System.IO.BinaryReader r) {
      player.position = r.ReadVector2();
      velocity = r.ReadVector2();
      breath = r.ReadSingle();
      autoBurrow = r.ReadBoolean();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.WriteVector2(player.position);
      packet.WriteVector2(velocity);
      packet.Write(breath);
      packet.Write(autoBurrow);
    }

    protected override void UpdateActive() {
      if (IsLocal) {
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
          velocity = Vector2.Lerp(velocity.Normalized(), newVel.Normalized(), 0.1f) * Speed;
        }
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
      player.position += velocity;
      player.velocity = Vector2.Zero;
      oPlayer.CreatePlayerDust();

      // Breath
      if (breath > 0) {
        breath--;
      }
    }

    protected override void UpdateEnding() {
      // Runs when leaving solid tiles
      player.velocity = velocity * SpeedExitMultiplier;
      player.direction = Math.Sign(velocity.X);
      oPlayer.UnrestrictedMovement = true;
    }

    protected override void UpdateUsing() {
      // Manage suffocation debuff
      if (breath > 0) {
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
      if (breath != MaxDuration) {

        // UI indication for breath
        Vector2 drawAnchor = player.BottomRight - Main.screenPosition;
        drawAnchor.X += 48;
        drawAnchor.Y += 16;

        var color = Color.White;
        if (!InUse) {
          color *= 0.6f;
        }

        Vector2 drawPos = drawAnchor;
        int uiCount = (int)Math.Ceiling((double)breath / UiIncrement);
        for (int i = 0; i < uiCount; i++) {
          if (i % 10 == 0) {
            drawPos.X = drawAnchor.X;
            drawPos.Y += 40;
          }
          drawPos.X += 24;
          int frameY = 0;

          // Different frameY if this represents a partially filled bar
          if (i * UiIncrement + UiIncrement > breath) {
            frameY = 4 - (int)breath % UiIncrement / (UiIncrement / 5);
          }

          var tex = OriTextures.Instance.BurrowTimer.texture;
          Main.playerDrawData.Add(new DrawData(tex, drawPos, tex.Frame(3, 5, (int)Main.time % 30 / 10, frameY), color, 0, tex.Size() / 2, 1, SpriteEffects.None, 0));
        }
      }
    }
    internal override void Tick() {
      if (InUse) {
        EnterHitbox.UpdateHitbox(player.Center);
        OuterHitbox.UpdateHitbox(player.Center);
        InnerHitbox.UpdateHitbox(player.Center + velocity.Normalized() * 16);
        abilities.glide.SetState(State.Inactive);

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
          }
        }
        else if (Ending) {
          if (CurrentTime > 2) {
            SetState(State.Inactive);
            PutOnCooldown();
          }
        }

        netUpdate = true;
      }
      else {
        // Not in use
        TickCooldown();

        if (CanUse && IsLocal && (OriMod.BurrowKey.JustPressed && abilities.crouch.InUse || autoBurrow)) {
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

            autoBurrow = OriMod.ConfigClient.AutoBurrow && OriMod.BurrowKey.Current;
            // TODO: consider moving this write to an Update method
            velocity = (autoBurrow ? player.velocity.Normalized() : Vector2.UnitY) * Speed;
            player.position += velocity;
          }
        }

        if (breath < MaxDuration) {
          breath += Config.BurrowRecoveryRate;
          if (breath > MaxDuration) {
            breath = MaxDuration;
          }
        }
      }
    }

    private static void Unload() {
      _eh = null;
      _oh = null;
      _ih = null;
    }
  }
}
