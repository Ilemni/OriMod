using AnimLib.Animations;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Tiles;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for reducing fall velocity to a glide.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing gliding with wings.
/// </remarks>
public sealed class Glide(Player player) : OriAbility(player) {
  public override int MaxLevel => 1;
  public override bool CanEnter() => base.CanEnter() && !IsGrounded;

  private static float RunSlowdown => 0.125f;
  private static float RunAcceleration => 0.2f;
  private static int StartDuration => 5;
  private static int EndDuration => 5;

  internal bool Starting => ActiveTime < StartDuration;
  internal bool Ending => _endingTime > 0;

  private int _endingTime;

  private SoundInfo _startSound = new("Ori/Glide/seinGlideStart", 3, 0.8f);
  private SoundInfo _switchDirectionSound = new("Ori/Glide/seinGlideMoveLeftRight", 5, 0.45f);
  private SoundInfo _endSound = new("Ori/Glide/seinGlideEnd", 3, 0.8f);

  private bool _oldLeft;
  private bool _oldRight;

  protected override void OnEnter(State? fromState) {
    _startSound.Play(Player);
    _oldLeft = Player.controlLeft;
    _oldRight = Player.controlRight;
  }

  protected override void OnExit() {
    _endingTime = 0;
  }

  protected override void OnUpdate() {
    Player.maxFallSpeed = MathHelper.Clamp(Player.gravity * 5, 1f, 2f);

    if (Player.gravDir > 0f) {
      for (int i = 0; i < 45; i++) {
        Tile tile = Main.tile[Player.Center.ToTileCoordinates() + new Point(0, (int)(Player.gravDir * i))];
        if (!OriUtils.IsSolid(tile, true)) {
          continue;
        }

        if (tile.TileType == ModContent.TileType<HotAshTile>()) {
          Player.maxFallSpeed = -2f;
          RestoreAirJumps();
          tile = Main.tile[Player.Center.ToTileCoordinates() + new Point(0, (int)(Player.gravDir * -1))];
          if (i == 44 || OriUtils.IsSolid(tile, true)) Player.maxFallSpeed = 0.001f;
        }

        break;
      }
    }

    Player.runSlowdown = RunSlowdown;
    Player.runAcceleration = RunAcceleration;
    if (!Starting) {
      if (Player.controlLeft != _oldLeft || Player.controlRight != _oldRight) {
        _switchDirectionSound.Play(Player);
      }

      _oldLeft = Player.controlLeft;
      _oldRight = Player.controlRight;
    }

    if (Ending && _endingTime == 1) {
      _endSound.Play(Player);
    }

    Player.controlUseItem = false;
    Player.controlTorch = false;
  }

  protected override void OnPreUpdate() {
    if (!IsLocal && !Main.dedServ) {
      bool b = CanEnter();
      ;
    }
    if (!CanEnter()) {
      CancelState();
      return;
    }

    if (!IsLocal) {
      return;
    }

    if (Input.Jump.JustPressed) {
      TriggerState<AirJump>();
    }

    if (Input.Dash.JustPressed) {
      TriggerState<Dash>();
      return;
    }

    if (Input.Burrow.JustPressed) {
      TriggerState<Burrow>();
      return;
    }

    if (OnWall || IsGrounded || !Input.Glide.Current) {
      if (Starting) {
        CancelState();
        return;
      }

      _endingTime++;
      if (_endingTime > EndDuration) {
        CancelState();
      }
    }
  }

  protected override AnimationOptions? GetAnimationOptions() =>
    Starting ? new AnimationOptions("GlideStart") :
    Ending ? new AnimationOptions("GlideStart", isReversed: true) :
    new AnimationOptions("Glide");
}
