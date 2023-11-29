using AnimLib.Abilities;
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
public sealed class Glide : OriAbility, ILevelable {
  public override int Id => AbilityId.Glide;
  public override int Level => ((ILevelable)this).Level;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 1;
  public override bool Unlocked => Level > 0;

  public override bool CanUse =>
    base.CanUse && !Ending && !player.mount.Active &&
    !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.chargeJump &&
    !abilities.climb && !abilities.dash && !abilities.launch && !abilities.stomp && !abilities.wallChargeJump &&
    !abilities.wallJump;

  private static float RunSlowdown => 0.125f;
  private static float RunAcceleration => 0.2f;
  private static int StartDuration => 5;
  private static int EndDuration => 5;

  private readonly RandomChar _randStart = new();
  private readonly RandomChar _randActive = new();
  private readonly RandomChar _randEnd = new();

  private bool _oldLeft;
  private bool _oldRight;

  public override void UpdateStarting() {
    if (stateTime == 0) {
      PlaySound("Ori/Glide/seinGlideStart" + _randStart.NextNoRepeat(3), 0.8f);
    }
  }

  public override void UpdateActive() {
    if (player.controlLeft != _oldLeft || player.controlRight != _oldRight) {
      PlaySound("Ori/Glide/seinGlideMoveLeftRight" + _randActive.NextNoRepeat(5), 0.45f);
    }
    _oldLeft = player.controlLeft;
    _oldRight = player.controlRight;
  }

  public override void UpdateEnding() {
    if (stateTime == 0) {
      PlaySound("Ori/Glide/seinGlideEnd" + _randEnd.NextNoRepeat(3), 0.8f);
    }
  }

  public override void UpdateUsing() {
    player.maxFallSpeed = MathHelper.Clamp(player.gravity * 5, 1f, 2f);

    Tile tile = Main.tile[player.Center.ToTileCoordinates()];
    for (int i = 0; i < 45; i++) {
      if (player.gravDir < 0f) break;

      tile = Main.tile[player.Center.ToTileCoordinates() + new Point(0,(int)(player.gravDir*i))];
      if (!OriUtils.IsSolid(tile,true)) continue;

      if (i < 45 && tile.TileType == ModContent.TileType<HotAshTile>()) {
        player.maxFallSpeed = -2f;
        RestoreAirJumps();
        tile = Main.tile[player.Center.ToTileCoordinates() + new Point(0,(int)(player.gravDir*-1))];
        if (i == 44 || OriUtils.IsSolid(tile,true)) player.maxFallSpeed = 0.001f;
      }

      break;
    }

    player.runSlowdown = RunSlowdown;
    player.runAcceleration = RunAcceleration;
  }

  public override void PreUpdate() {
    if (!InUse && CanUse && !IsGrounded && !OnWall && input.glide.Current) {
      SetState(AbilityState.Starting);
      return;
    }
    if (abilities.dash || abilities.burrow || abilities.launch) {
      SetState(AbilityState.Inactive);
      return;
    }

    if (!InUse) return;
    if (Starting) {
      if (stateTime > StartDuration) {
        SetState(AbilityState.Active);
      }
    }
    else if (Ending) {
      if (stateTime > EndDuration) {
        SetState(AbilityState.Inactive);
      }
    }
    else if (OnWall || IsGrounded || !input.glide.Current) {
      SetState(AbilityState.Ending);
    }
  }
}
