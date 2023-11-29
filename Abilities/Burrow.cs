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
public sealed class Burrow : OriAbility, ILevelable {
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

  private bool InMenu => Main.ingameOptionsWindow || Main.inFancyUI || player.talkNPC >= 0 || player.sign >= 0 ||
    Main.clothesWindow || Main.playerInventory;

  private float _breath = float.MaxValue;
  private int Strength =>
    Level switch {
      0 => 0,
      1 => 55, // Exclude evil biomes, dungeon
      2 => 200, // Exclude lihzahrd
      _ => 100 + Level * 50
    };

  private bool CanBurrowAny => Level >= 3;
  internal static bool IsSolid(Tile tile) => tile is { HasTile: true, IsActuated: false, HasUnactuatedTile: true } &&
    Main.tileSolid[tile.TileType];

  internal bool CanBurrow(Tile t) =>
    CanBurrowAny && IsSolid(t) || TileCollection.Instance.tilePickaxeMin[t.TileType] <= Strength;

  private Vector2 _lastPosition;
  internal Vector2 velocity;
  /// <summary>
  /// Tile hitbox for determining if the player can enter Burrow state.
  /// </summary>
  internal static TileHitbox EnterHitbox => _eh ??= Unloadable.New(new TileHitbox(
    (0, -1), (0, 0), (0, 1), // Center
    (-1, -1), (-1, 0), (-1, 1), // Left
    (2, -1), (2, 0), (2, 1), // Right
    (0, -2), (1, -2), // Top
    (0, 2), (1, 2), // Bottom
    (2, 2), (2, -2), (-1, 2), (-1, -2) // Corners
  ), () => _eh = null);
  /// <summary>
  /// Tile hitbox for determining collisions when in the Burrow state
  /// </summary>
  internal static TileHitbox InnerHitbox => _ih ??= Unloadable.New(new TileHitbox(
    (0, -1), // Top
    (0, 1), // Bottom
    (-1, 0), // Left
    (1, 0) // Right
  ), () => _ih = null);
  private static TileHitbox _eh;
  private static TileHitbox _ih;

  /// <summary>
  /// Modify player velocity when they collide with a tile they cannot burrow through.
  /// </summary>
  /// <param name="point"><see cref="TileHitbox"/> template point.</param>
  /// <param name="didX">Whether a collision has occurred on the X axis.</param>
  /// <param name="didY">Whether a collision has occurred on the Y axis.</param>
  private void OnCollision(Point point, ref bool didX, ref bool didY) {
    if (!didX && point.X != 0) {
      // Left or right
      didX = true;
      velocity.X *= -1;
    }
    if (!didY && point.Y != 0) {
      // Top or bottom
      didY = true;
      velocity.Y *= -1;
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

    if (IsLocal) {
      // Get intended velocity based on input
      bool holdNeutral = false;
      Vector2 newVel = Vector2.Zero;
      if (OriMod.ConfigClient.BurrowToMouse) {
        newVel = player.AngleTo(Main.MouseWorld).ToRotationVector2();
        holdNeutral = Vector2.DistanceSquared(Main.MouseWorld,player.Center) < 3600.0f;
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
        
        if (newVel == Vector2.Zero) {
          holdNeutral = true;
          newVel = velocity;
        }
      }
      if ((velocity.ToRotation() - newVel.ToRotation()).ToRotationVector2().X < 0f) {
        holdNeutral = true;
      }

      currentSpeed = OriUtils.Lerp(currentSpeed,
        input.burrow.Current ? FastSpeed : BaseSpeed * (holdNeutral ? 0.2f : 1f), 0.09f);

      if (newVel == Vector2.Zero) {
        newVel = velocity;
      }
      velocity = Vector2.Lerp(velocity.Normalized(), newVel.Normalized(), 0.1f) * currentSpeed;
    }

    // Detect bouncing
    if (!CanBurrowAny) {
      bool didX = false;
      bool didY = false;
      InnerHitbox.UpdateHitbox(player.Center + velocity.Normalized() * (player.gravDir < 0 ? 48 : 32));
      var innerPoints = InnerHitbox.Points;
      for (int i = 0, len = innerPoints.Length; i < len; i++) {
        Point point = innerPoints[i];
        if (!CanBurrow(Main.tile[point])) {
          OnCollision(InnerHitbox.Template[i], ref didX, ref didY);
        }
      }
    }

    // Apply changes
    player.velocity = Vector2.Zero;
    oPlayer.CreatePlayerDust();
    _breath = Math.Max(_breath -= input.leftClick.Current ? 2.2f : 1, 0);
  }

  public override void UpdateEnding() {
    // Runs when leaving solid tiles
    velocity = velocity.Normalized() * Math.Max(velocity.Length(), BaseSpeed);
    player.velocity = velocity * SpeedExitMultiplier;
    player.direction = Math.Sign(velocity.X);
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
    oPlayer.KillGrapples();
    player.grapCount = 0;
  }

  public override void PostUpdate() {
    if (!InUse) return;
    player.position = _lastPosition + velocity;
    _lastPosition = player.position;
  }

  /// <summary>
  /// Draw breath meter to screen
  /// </summary>
  internal void DrawEffects(ref PlayerDrawSet drawInfo) {
    if (_breath >= MaxDuration || Main.hideUI) return;

    Vector2 baseDrawPosition = player.Right - Main.screenPosition;
    baseDrawPosition.X += 48;
    baseDrawPosition.Y += player.gravDir >= 0 ? 16 : 112;

    Texture2D texture = OriTextures.Instance.burrowTimer.Value;
    Vector2 origin = texture.Size() / 2;
    Color color = Color.White * (InUse ? 1 : 0.6f);
    SpriteEffects effect = player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

    // Adding to drawDataCache multiple times, position is updated each time
    Vector2 currentDrawPosition = baseDrawPosition;
    int uiCount = (int)Math.Ceiling(_breath / UiIncrement);
    for (int i = 0; i < uiCount; i++) {
      if (i % 10 == 0) {
        currentDrawPosition.X = baseDrawPosition.X;
        currentDrawPosition.Y += 40 * player.gravDir;
      }
      currentDrawPosition.X += 24;

      // Different frameY if this represents a partially filled bar
      int frameX = (int)Main.time % 30 / 10;
      int frameY = 0;
      if ((i+1) * UiIncrement > _breath) {
        frameY = 4 - (int)_breath % UiIncrement / (UiIncrement / 5);
      }

      Rectangle rect = texture.Frame(3, 5, frameX, frameY);

      DrawData data = new(texture, currentDrawPosition, rect, color, 0, origin, 1, effect)
        {
          ignorePlayerRotation = true
        };
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
      else if (Ending && stateTime > 2) {
        SetState(AbilityState.Inactive);
        StartCooldown();
      }

      netUpdate = true;
      return;
    }

    // Not in use
    _breath = Math.Clamp(_breath + RecoveryRate, 0, MaxDuration);

    // Is player trying to enter Burrow?
    if (!CanUse || !input.burrow.JustPressed) return;

    // Check if player can enter Burrow
    EnterHitbox.UpdateHitbox(player.Center);
    bool doBurrow = EnterHitbox.Points.Any(p => CanBurrow(Main.tile[p.X, p.Y]));
    if (!doBurrow) return;

    // Enter Burrow
    SetState(AbilityState.Active);
    cooldownLeft = Cooldown;
    currentSpeed = FastSpeed;
    velocity = Vector2.UnitY * player.gravDir * currentSpeed;
    player.position += velocity;
    _lastPosition = player.position;
  }
}
