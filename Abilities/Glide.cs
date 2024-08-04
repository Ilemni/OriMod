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
    base.CanUse && !Ending && !Player.mount.Active &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.ChargeDash && !Abilities.ChargeJump &&
    !Abilities.Climb && !Abilities.Dash && !Abilities.Launch && !Abilities.Stomp && !Abilities.WallChargeJump &&
    !Abilities.WallJump;

  private static float RunSlowdown => 0.125f;
  private static float RunAcceleration => 0.2f;
  private static int StartDuration => 5;
  private static int EndDuration => 5;

  private RandomChar _randStart;
  private RandomChar _randActive;
  private RandomChar _randEnd;

  private bool _oldLeft;
  private bool _oldRight;

  public override void UpdateStarting() {
    if (StateTime == 0) {
      PlaySound("Ori/Glide/seinGlideStart" + _randStart.NextNoRepeat(3), 0.8f);
    }
  }

  public override void UpdateActive() {
    if (Player.controlLeft != _oldLeft || Player.controlRight != _oldRight) {
      PlaySound("Ori/Glide/seinGlideMoveLeftRight" + _randActive.NextNoRepeat(5), 0.45f);
    }
    _oldLeft = Player.controlLeft;
    _oldRight = Player.controlRight;
  }

  public override void UpdateEnding() {
    if (StateTime == 0) {
      PlaySound("Ori/Glide/seinGlideEnd" + _randEnd.NextNoRepeat(3), 0.8f);
    }
  }

  public override void UpdateUsing() {
    Player.maxFallSpeed = MathHelper.Clamp(Player.gravity * 5, 1f, 2f);

    for (int i = 0; i < 45; i++) {
      if (Player.gravDir < 0f) break;

      Tile tile = Main.tile[Player.Center.ToTileCoordinates() + new Point(0,(int)(Player.gravDir*i))];
      if (!OriUtils.IsSolid(tile,true)) continue;

      if (tile.TileType == ModContent.TileType<HotAshTile>()) {
        Player.maxFallSpeed = -2f;
        RestoreAirJumps();
        tile = Main.tile[Player.Center.ToTileCoordinates() + new Point(0,(int)(Player.gravDir*-1))];
        if (i == 44 || OriUtils.IsSolid(tile,true)) Player.maxFallSpeed = 0.001f;
      }

      break;
    }

    Player.runSlowdown = RunSlowdown;
    Player.runAcceleration = RunAcceleration;
  }

  public override void PreUpdate() {
    if (!InUse && CanUse && !IsGrounded && !OnWall && Input.Glide.Current) {
      SetState(AbilityState.Starting);
      return;
    }
    if (Abilities.Dash || Abilities.Burrow || Abilities.Launch) {
      SetState(AbilityState.Inactive);
      return;
    }

    if (!InUse) return;
    if (Starting) {
      if (StateTime > StartDuration) {
        SetState(AbilityState.Active);
      }
    }
    else if (Ending) {
      if (StateTime > EndDuration) {
        SetState(AbilityState.Inactive);
      }
    }
    else if (OnWall || IsGrounded || !Input.Glide.Current) {
      SetState(AbilityState.Ending);
    }
  }
}
