using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Utilities;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities; 

  /// <summary>
  /// Ability for traveling through solid terrain.
  /// </summary>
  /// <remarks>
  /// This ability was somewhat difficult to balance; the simplest solution was to restrict tiles to whatever pickaxe was in inventory.
  /// </remarks>
  public sealed class Burrow : Ability<OriAbilityManager>, ILevelable {
    public override int Id => AbilityId.Burrow;
    public override int Level => ((ILevelable)this).Level;
    public override bool Unlocked => Level > 0;
    int ILevelable.Level { get; set; }
    int ILevelable.MaxLevel => 3;

    public override bool CanUse => base.CanUse && !player.mount.Active && !InMenu && abilities.crouch;
    public override int Cooldown => 12;
    public override void OnRefreshed() => abilities.RefreshParticles(Color.SandyBrown);

  private int MaxDuration =>
    Level switch {
      1 => 300,
      2 => 480,
      3 => 600,
      _ => 120 + Level * 120
    };

  private float RecoveryRate =>
    Level switch {
      1 => 0.1f,
      2 => 0.2f,
      3 => 0.35f,
      _ => Level * 0.125f
    };

    private static int UiIncrement => 60;
    private static float BaseSpeed => 6f;
    private float currentSpeed;
    private static float FastSpeed => 12f;
    private static float SpeedExitMultiplier => 1.2f;

    private bool InMenu => Main.ingameOptionsWindow || Main.inFancyUI || player.talkNPC >= 0 || player.sign >= 0 || Main.clothesWindow || Main.playerInventory;

    private float _breath = float.MaxValue;
  private int Strength =>
    Level switch {
      0 => 0,
      1 => 55, // Exclude evil biomes, dungeon
      2 => 200, // Exclude lihzahrd
      _ => 100 + Level * 50
    };

    private bool CanBurrowAny => Level >= 3;
    internal static bool IsSolid(Tile tile) => tile.HasTile && !tile.IsActuated && tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];

    internal bool CanBurrow(Tile t) => CanBurrowAny && IsSolid(t) || TileCollection.Instance.tilePickaxeMin[t.TileType] <= Strength;

    private Vector2 _lastPosition;
    internal Vector2 velocity;

    private static Point P(int x, int y) => new Point(x, y);
    /// <summary>
    /// Tile hitbox for determining if the player can enter Burrow state.
    /// </summary>
  internal static TileHitbox EnterHitbox => _eh ??= Unloadable.New(new TileHitbox(
    (0, -1), (0, 0), (0, 1), // Center
    (-1, -1), (-1, 0), (-1, 1), // Left
    (2, -1), (2, 0), (2, 1),  // Right
    (0, -2), (1, -2), // Top
    (0, 2), (1, 2),  // Bottom
    (2, 2), (2, -2), (-1, 2), (-1, -2) // Corners
  ), () => _eh = null);
    /// <summary>
    /// Tile hitbox for determining collisions when in the Burrow state
    /// </summary>
  internal static TileHitbox InnerHitbox => _ih ??= Unloadable.New(new TileHitbox(
    (0, -1), // Top
    (0, 1),  // Bottom
    (-1, 0), // Left
    (1, 0)  // Right
  ), () => _ih = null);
    private static TileHitbox _eh;
    private static TileHitbox _ih;

    /// <summary>
    /// Modify player velocity when they collide with a tile they cannot burrow through.
    /// </summary>
    /// <param name="hitboxIdx">Index of <see cref="InnerHitbox"/> point.</param>
    /// <param name="didX">Whether or not a collision has occured on the X axis.</param>
    /// <param name="didY">Whether or not a collision has occured on the Y axis.</param>
    private void OnCollision(int hitboxIdx, ref bool didX, ref bool didY) {
      abilities.oPlayer.Debug("Bounce! " + hitboxIdx);
      switch (hitboxIdx) {
        case 0: // Top
        case 1: // Bottom
          if (!didY) {
            didY = true;
            velocity.Y *= -1;
          }
          break;
        case 2: // Left
        case 3: // Right
          if (!didX) {
            didX = true;
            velocity.X *= -1;
          }
          break;
        default: // Corners
          if (!didX) {
            didX = true;
            velocity.X *= -1;
          }
          if (!didY) {
            didY = true;
            velocity.Y *= -1;
          }
          break;
      }
    }

    public override void ReadPacket(BinaryReader r) {
      _lastPosition = r.ReadVector2();
      player.position = _lastPosition;
      velocity = r.ReadVector2();
      _breath = r.ReadSingle();
    }

    public override void WritePacket(ModPacket packet) {
      packet.WriteVector2(_lastPosition);
      packet.WriteVector2(velocity);
      packet.Write(_breath);
    }

    public override void UpdateActive() {
      if (abilities.oPlayer.input.leftClick.Current) {
        if (currentSpeed < FastSpeed) {
          currentSpeed = OriUtils.Lerp(currentSpeed, FastSpeed, 0.09f);
        }
      }
      else {
        if (currentSpeed > BaseSpeed) {
          currentSpeed = OriUtils.Lerp(currentSpeed, BaseSpeed, 0.875f);
        }
      }

      if (IsLocal) {
        // Get intended velocity based on input
        Vector2 newVel = Vector2.Zero;
        if (OriMod.ConfigClient.BurrowToMouse) {
          newVel = player.AngleTo(Main.MouseWorld).ToRotationVector2();
          if (player.confused) {
            newVel *= -1;
          }
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
          velocity = Vector2.Lerp(velocity.Normalized(), newVel.Normalized(), 0.1f) * currentSpeed;
        }
      }

      // Detect bouncing
      if (!CanBurrowAny) {
        bool didX = false;
        bool didY = false;
        InnerHitbox.UpdateHitbox(player.Center + velocity.Normalized() * (player.gravDir < 0 ? 48 : 32));
        var innerPoints = InnerHitbox.Points;
        for (int i = 0, len = innerPoints.Length; i < len; i++) {
          Point point = innerPoints[i];
          Tile tile = Main.tile[point];
          if (!CanBurrow(tile)) {
            OnCollision(i, ref didX, ref didY);
          }
        }
      }

      // Apply changes
      player.velocity = Vector2.Zero;
      abilities.oPlayer.CreatePlayerDust();

      // Breath
      if (_breath > 0) {
        _breath -= abilities.oPlayer.input.leftClick.Current ? 2.2f : 1;
      }
    }

    public override void UpdateEnding() {
      // Runs when leaving solid tiles
      player.velocity = velocity * SpeedExitMultiplier;
      player.direction = Math.Sign(velocity.X);
      abilities.oPlayer.UnrestrictedMovement = true;
    }

    public override void UpdateUsing() {
      // Manage suffocation debuff
      if (_breath > 0) {
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
      abilities.oPlayer.KillGrapples();
      player.grapCount = 0;
    }

    public override void PostUpdate() {
      if (!InUse) return;
      player.position = _lastPosition + velocity;
      _lastPosition = player.position;
    }

    internal void DrawEffects(ref PlayerDrawSet drawInfo) {
      if (_breath >= MaxDuration) return;
      // UI indication for breath
      Vector2 baseDrawPos = player.Right - Main.screenPosition;
      baseDrawPos.X += 48;
      baseDrawPos.Y += player.gravDir >= 0 ? 16 : 112;
      Color color = Color.White * (InUse ? 1 : 0.6f);

      Vector2 drawPos = baseDrawPos;
      int uiCount = (int)Math.Ceiling((double)_breath / UiIncrement);
      SpriteEffects effect = player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
      for (int i = 0; i < uiCount; i++) {
        if (i % 10 == 0) {
          drawPos.X = baseDrawPos.X;
          drawPos.Y += 40 * player.gravDir;
        }
        drawPos.X += 24;
        int frameY = 0;

        // Different frameY if this represents a partially filled bar
        if (i * UiIncrement + UiIncrement > _breath) {
          frameY = 4 - (int)_breath % UiIncrement / (UiIncrement / 5);
        }

        Texture2D tex = OriTextures.Instance.burrowTimer.texture;
      DrawData data = new(tex, drawPos, tex.Frame(3, 5, (int)Main.time % 30 / 10, frameY), color, 0, tex.Size() / 2, 1, effect, 0);
        data.ignorePlayerRotation = true;
        drawInfo.DrawDataCache.Add(data);
      }
    }

    public override void PreUpdate() {
      if (InUse) {
        InnerHitbox.UpdateHitbox(player.Center + velocity.Normalized() * (player.gravDir < 0 ? 48 : 32));

        if (Active) {
          bool canBurrow = InnerHitbox.Points.Any(p => IsSolid(Main.tile[p.X, p.Y]));
          if (!canBurrow) {
            SetState(AbilityState.Ending);
          }
        }
        else if (Ending) {
          if (stateTime > 2) {
            SetState(AbilityState.Inactive);
            StartCooldown();
          }
        }

        netUpdate = true;
      }
      else {
        // Not in use
        if (CanUse && abilities.oPlayer.input.burrow.JustPressed) {
          EnterHitbox.UpdateHitbox(player.Center);

          // Check if player can enter Burrow
          bool doBurrow = false;
          var enterPoints = EnterHitbox.Points;
          for (int i = 0, len = enterPoints.Length; i < len; i++) {
            Point p = enterPoints[i];
            Tile t = Main.tile[p.X, p.Y];
            if (CanBurrow(t)) {
              doBurrow = true;
            }
          }

          if (doBurrow) {
            // Enter Burrow
            SetState(AbilityState.Active);
            cooldownLeft = Cooldown;

            // TODO: consider moving this write to an Update method
            currentSpeed = FastSpeed;
            velocity = Vector2.UnitY * player.gravDir * currentSpeed;
            player.position += velocity;
            _lastPosition = player.position;
          }
        }

        if (_breath < MaxDuration) {
          if (_breath < 0) {
            _breath = 0;
          }
          _breath += RecoveryRate;
        }
        if (_breath > MaxDuration) {
          _breath = MaxDuration;
        }
      }
    }
  }
}
