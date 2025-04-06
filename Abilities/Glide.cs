using System;
using System.Collections.Generic;
using AnimLib.Animations;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Tiles;
using OriMod.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for reducing fall velocity to a glide.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing gliding with wings.
/// </remarks>
public sealed class Glide : OriAbility {
  public override int MaxLevel => 1;

  private static float RunSlowdown => 0.125f;
  private static float RunAcceleration => 0.2f;
  private static int StartDuration => 20;
  private static int EndDuration => 10;

  private bool Starting => ActiveTime < StartDuration;
  private bool Ending => _endingTime > 0;

  private int _endingTime;

  private SoundInfo _startSound = new("Ori/Glide/seinGlideStart", 3, 0.8f);
  private SoundInfo _switchDirectionSound = new("Ori/Glide/seinGlideMoveLeftRight", 5, 0.45f);
  private SoundInfo _endSound = new("Ori/Glide/seinGlideEnd", 3, 0.8f);

  private bool _oldLeft;
  private bool _oldRight;

  public override bool ShowHintInUI => Main.hardMode;

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility.Falling>(),
    ]);
  }

  protected override bool UpdateInterrupt(State activeState) {
    return activeState.ActiveTime > 4 && Input.Glide.Current;
  }

  public override bool CanEnter() => base.CanEnter() && !IsGrounded;

  protected override void OnEnter(State? fromState) {
    _startSound.Play(Player);
    _oldLeft = Player.controlLeft;
    _oldRight = Player.controlRight;
  }

  protected override void OnExit(State? toState) {
    _endingTime = 0;
  }

  public override void SetControls() {
    Player.controlUseItem = false;
    Player.controlTorch = false;
  }

  [HookCondition(HookConditionFlags.Strict)]
  public override bool PreItemCheck() => false;

  [HookCondition(HookConditionFlags.Strict)]
  public override void HideDrawLayers(PlayerDrawSet drawInfo) {
    PlayerDrawLayers.HeldItem.Hide();
  }

  public override void PostUpdateRunSpeeds() {
    Player.maxFallSpeed = Math.Clamp(Player.gravity * 5, 1f, 2f);

    if (Player.gravDir > 0f) {
      Point playerTilePos = Player.Center.ToTileCoordinates();
      for (int i = 0; i < 45; i++) {
        Tile tile = Main.tile[playerTilePos + new Point(0, i)];
        if (!OriUtils.IsSolid(tile, true)) {
          continue;
        }

        if (tile.TileType == ModContent.TileType<HotAshTile>()) {
          Player.maxFallSpeed = -2f;
          RestoreAirJumps();
          tile = Main.tile[playerTilePos + new Point(0, -i)];
          if (i == 44 || OriUtils.IsSolid(tile, true)) {
            Player.maxFallSpeed = 0.001f;
          }
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
  }

  public override void PostUpdateMiscEffects() {
    if (!CanEnter()) {
      CancelState();
      return;
    }

    if (!IsLocal) {
      return;
    }

    if (!OnWall && !IsGrounded && Input.Glide.Current) {
      return;
    }

    if (Starting) {
      CancelState();
      return;
    }

    _endingTime++;
    if (_endingTime > EndDuration) {
      CancelState();
    }
  }

  public override AnimationOptions? GetAnimationOptions() => this switch {
    _ when Starting => new AnimationOptions("GlideStart"),
    _ when Ending => this switch {
      _ when !IsGrounded && Anim.HasTag("GlideEndAir") => new AnimationOptions("GlideEndAir"),
      _ when Anim.HasTag("GlideEnd") => new AnimationOptions("GlideEnd"),
      _ => new AnimationOptions("GlideStart") { IsReversed = true },
    },
    _ => new AnimationOptions("Glide")
  };
}
