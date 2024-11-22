using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
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
public sealed class Burrow(Player player) : OriAbility(player) {
  public override int MaxLevel => 3;

  private ref BurrowStats Stats => ref IStats<BurrowStats>.Get(Level);

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();
    parent.AddInterruptible<NoAbility>(to: this);
    parent.AddInterruptible<Crouch>(to: this);
  }

  public override bool CanEnter() => base.CanEnter() && !InMenu;

  protected override bool CanTransitionFrom(State fromState) => fromState is Crouch || OnWall;

  protected override bool OnPreUpdateInterruptible(State fromState) {
    if (!Input.Burrow.JustPressed) {
      return false;
    }

    // Not in use
    _breath = Math.Clamp(_breath + Stats.RecoveryRate, 0, Stats.Duration);

    // Check if player can enter Burrow
    EnterHitbox.UpdateHitbox(Player.Center);
    return EnterHitbox.Any(CanBurrow);
  }

  public override bool SupportsCooldown => true;
  public override int MaxCooldown => 12;
  protected override void OnEndCooldown() => RefreshParticles(Color.SandyBrown);

  private static int UiIncrement => 60;
  private static float BaseSpeed => 6f;
  private float _currentSpeed;
  private static float FastSpeed => 12f;
  private static float SpeedExitMultiplier => 1.2f;

  private bool InMenu => Main.ingameOptionsWindow || Main.inFancyUI || Player.talkNPC >= 0 || Player.sign >= 0 ||
    Main.clothesWindow || Main.playerInventory;

  private float _breath = float.MaxValue;

  private int _endingTime;

  private bool Ending => _endingTime > 0;

  private bool CanBurrowAny => Level >= 3;
  private static bool IsSolid(Tile tile) => tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];

  internal bool CanBurrow(Tile t) =>
    CanBurrowAny && IsSolid(t) || TileCollection.TilePickaxeMin[t.TileType] <= Stats.Strength;

  private Vector2 _lastPosition;
  private Vector2 _velocity;

  protected override bool StartCooldownOnExit => true;

  /// <summary>
  /// Tile hitbox for determining if the player can enter Burrow state.
  /// </summary>
  internal static readonly TileHitbox EnterHitbox = new(
    (0, -1), (0, 0), (0, 1), // Center
    (-1, -1), (-1, 0), (-1, 1), // Left
    (2, -1), (2, 0), (2, 1), // Right
    (0, -2), (1, -2), // Top
    (0, 2), (1, 2), // Bottom
    (2, 2), (2, -2), (-1, 2), (-1, -2) // Corners
  );

  /// <summary>
  /// Tile hitbox for determining collisions when in the Burrow state
  /// </summary>
  internal static readonly TileHitbox InnerHitbox = new(
    (0, -1), // Top
    (0, 1), // Bottom
    (-1, 0), // Left
    (1, 0) // Right
  );

  protected override void NetSync(ISync sync) {
    sync.Sync(ref _lastPosition);
    sync.Sync(ref Player.position);
    sync.Sync(ref _velocity);
    sync.Sync(ref _breath);
  }

  protected override void OnEnter(State? fromState) {
    _currentSpeed = FastSpeed;
    _velocity = Vector2.UnitY * Player.gravDir * _currentSpeed;
    Player.position += _velocity;
    _lastPosition = Player.position;
  }

  private void UpdateActive() {
    if (IsLocal) {
      // Get intended velocity based on input
      bool holdNeutral = false;
      Vector2 newVel = Vector2.Zero;
      if (OriMod.ConfigClient.BurrowToMouse) {
        newVel = Player.AngleTo(Main.MouseWorld).ToRotationVector2();
        holdNeutral = Vector2.DistanceSquared(Main.MouseWorld, Player.Center) < 3600.0f;
        if (Player.confused) {
          newVel *= -1;
        }
      }
      else {
        if (Player.controlLeft) {
          newVel.X -= 1;
        }

        if (Player.controlRight) {
          newVel.X += 1;
        }

        if (Player.controlUp) {
          newVel.Y -= Player.gravDir;
        }

        if (Player.controlDown) {
          newVel.Y += Player.gravDir;
        }

        if (newVel == Vector2.Zero) {
          holdNeutral = true;
          newVel = _velocity;
        }
      }

      if ((_velocity.ToRotation() - newVel.ToRotation()).ToRotationVector2().X < 0f) {
        holdNeutral = true;
      }

      _currentSpeed = float.Lerp(_currentSpeed,
        Input.Burrow.Current ? FastSpeed : BaseSpeed * (holdNeutral ? 0.2f : 1f), 0.09f);

      if (newVel == Vector2.Zero) {
        newVel = _velocity;
      }

      _velocity = Vector2.Lerp(_velocity.SafeNormalize(default), newVel.SafeNormalize(default), 0.1f) * _currentSpeed;
    }

    // Detect bouncing
    if (!CanBurrowAny) {
      InnerHitbox.UpdateHitbox(Player.Center + _velocity.SafeNormalize(default) * (Player.gravDir < 0 ? 48 : 32));
      InnerHitbox.GetCollisions(CanBurrow, out bool didX, out bool didY);
      _velocity = (didX, didY) switch {
        (true, true) => _velocity * -1,
        (false, false) => _velocity,
        (true, false) => new Vector2(_velocity.X * -1, _velocity.Y),
        (false, true) => new Vector2(_velocity.X, _velocity.Y * -1)
      };
    }

    // Apply changes
    Player.velocity = Vector2.Zero;
    OriPlayer.CreatePlayerDust();
    _breath = Math.Max(_breath -= Input.LeftClick.Current ? 2.2f : 1, 0);
  }

  protected override void OnUpdate() {
    if (Ending) {
      // Runs when leaving solid tiles
      _velocity = _velocity.SafeNormalize(default) * Math.Max(_velocity.Length(), BaseSpeed);
      Player.velocity = _velocity * SpeedExitMultiplier;
      Player.ChangeDir(Math.Sign(_velocity.X));
    }
    else {
      UpdateActive();
    }

    // Manage suffocation debuff
    if (_breath > 0) {
      Player.buffImmune[BuffID.Suffocation] = true;
    }
    else {
      Player.AddBuff(BuffID.Suffocation, 1);
    }

    // Disable actions while burrowing
    Player.gravity = 0;
    Player.controlJump = false;

    // Allow use item only if it is a warp item, like magic mirror
    if (Player.HeldItem.type is not (ItemID.MagicMirror or ItemID.IceMirror or ItemID.CellPhone or ItemID.RecallPotion
        or ItemID.Shellphone or ItemID.ShellphoneSpawn or ItemID.ShellphoneHell or ItemID.ShellphoneOcean
        or ItemID.ShellphoneDummy or ItemID.PDA or ItemID.GPS
        or ItemID.RodofDiscord or ItemID.TeleportationPotion or ItemID.WormholePotion)) {
      Player.noItems = true;
      Player.controlUseItem = false;
    }

    Player.controlUseTile = false;
    Player.controlThrow = false;
    Player.controlUp = false;
    Player.RemoveAllGrapplingHooks();
  }

  protected override void OnPostUpdate() {
    if (!IsActive) {
      return;
    }

    // Position was modified directly, likely as a result of player warping
    if (!Ending && ActiveTime > 10 && Vector2.DistanceSquared(Player.position, _lastPosition) > 100) {
      CancelState();
      return;
    }

    Player.position = _lastPosition + _velocity;
    _lastPosition = Player.position;
  }

  /// <summary>
  /// Draw breath meter to screen
  /// </summary>
  internal void DrawEffects(ref PlayerDrawSet drawInfo) {
    if (_breath >= Stats.Duration || Main.hideUI) {
      return;
    }

    Vector2 baseDrawPosition = Player.Right - Main.screenPosition;
    baseDrawPosition.X += 48;
    baseDrawPosition.Y += Player.gravDir >= 0 ? 16 : 112;

    Texture2D texture = OriPlayer.BurrowTimer.Value;
    Vector2 origin = texture.Size() / 2;
    Color color = Color.White * (IsActive ? 1 : 0.6f);
    SpriteEffects effect = Player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

    // Adding to drawDataCache multiple times, position is updated each time
    Vector2 drawPosition = baseDrawPosition;
    int uiCount = (int)Math.Ceiling(_breath / UiIncrement);
    for (int i = 0; i < uiCount; i++) {
      if (i % 10 == 0) {
        drawPosition.X = baseDrawPosition.X;
        drawPosition.Y += 40 * Player.gravDir;
      }

      drawPosition.X += 24;

      // Different frameY if this represents a partially filled bar
      int frameX = (int)Main.time % 30 / 10;
      int frameY = 0;
      if ((i + 1) * UiIncrement > _breath) {
        frameY = 4 - (int)_breath % UiIncrement / (UiIncrement / 5);
      }

      Rectangle rect = texture.Frame(3, 5, frameX, frameY);

      DrawData data = new(texture, drawPosition, rect, color, 0, origin, 1, effect) {
        ignorePlayerRotation = true
      };
      drawInfo.DrawDataCache.Add(data);
    }
  }

  protected override void OnPreUpdate() {
    InnerHitbox.UpdateHitbox(Player.Center + _velocity.SafeNormalize(default) * (Player.gravDir < 0 ? 48 : 32));

    if (!InnerHitbox.Any(IsSolid)) {
      _endingTime++;
    }
    else {
      _endingTime = 0;
    }

    if (Ending && _endingTime > 2) {
      CancelState();
    }

    NetUpdate = true;
  }

  protected override AnimationOptions? GetAnimationOptions() {
    float gravDir = Player.gravDir;

    float rad = (float)Math.Atan2(_velocity.X, -_velocity.Y * gravDir) * gravDir;
    return new AnimationOptions("Burrow", rotation: rad);
  }

  protected override void DebugText(DebugUIState ui) {
    ui.DrawAppendLabelValue("Breath", (int)_breath, Stats.Duration);
    ui.DrawAppendLabelValue("Speed", _currentSpeed, format: "F2");
  }

  private readonly record struct BurrowStats(
    int Duration,
    float RecoveryRate,
    int Strength
  ) : IStats<BurrowStats> {
    public static ref BurrowStats[] Values => ref _values;

    // ReSharper disable once ReplaceWithFieldKeyword - Causes CS8145
    private static BurrowStats[] _values = [
      default,
      new(Duration: 300, RecoveryRate: 0.4f, Strength: 55), // Evil biomes, dungeon
      new(Duration: 480, RecoveryRate: 1.2f, Strength: 200), // Pre-Temple
      new(Duration: 600, RecoveryRate: 2.35f, Strength: 300)
    ];

    public static BurrowStats CreateFromLevel(int level) => new(
      Duration: (level + 1) * 120,
      RecoveryRate: level * 0.125f,
      Strength: (level + 2) * 100
    );
  }
}
