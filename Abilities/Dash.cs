using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick horizontal dash. May be used in the air.
/// </summary>
/// <remarks>
/// This ability is derived from the Ori games, despite Terraria already allowing dashing with the Shield of Cthuhlu.
/// </remarks>
public sealed class Dash : OriAbility, ILevelable {
  public override int Id => AbilityId.Dash;
  public override int Level => ((ILevelable)this).Level;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 3;
  public override bool Unlocked => Level > 0;

  public override bool CanUse => base.CanUse && !InUse && !IsOnCooldown && !OnWall && !player.mount.Active && (Level >= 2 || IsGrounded) &&
    !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.launch && !abilities.stomp;
  public override int Cooldown => Level >= 3 ? 0 : 60;
  public override void OnRefreshed() => abilities.RefreshParticles(Color.White);

  private static float[] Speeds => _speeds ??= Unloadable.New(new float[25] {
    50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
    19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
  }, () => _speeds = null);
  private static float[] _speeds;
  private static int Duration => Speeds.Length - 1;

  private sbyte _direction;

  private readonly RandomChar _rand = new();

  internal void StartDash() {
    _direction = (sbyte)(player.controlLeft ? -1 : player.controlRight ? 1 : player.direction);
    PlaySound("Ori/Dash/seinDash" + _rand.NextNoRepeat(3), 0.2f);
    player.pulley = false;
  }

  public override void ReadPacket(BinaryReader r) {
    _direction = r.ReadSByte();
    player.position = r.ReadVector2();
    player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_direction);
    packet.WriteVector2(player.position);
    packet.WriteVector2(player.velocity);
  }

  public override void UpdateActive() {
    if (player.controlJump && (player.canJumpAgain_Blizzard || player.canJumpAgain_Cloud || player.canJumpAgain_Fart || player.canJumpAgain_Sail || player.canJumpAgain_Sandstorm)) {
      SetState(AbilityState.Inactive);
      StartCooldown();
      return;
    }
    player.velocity.X = Speeds[stateTime] * 0.5f * _direction;
    player.velocity.Y = 0.25f * (stateTime + 1) * player.gravDir;
    if (stateTime > 20) {
      player.runSlowdown = 26f;
    }
    if (IsLocal) netUpdate = true;
  }

  public override bool RefreshCondition() => abilities.bash || OnWall || IsGrounded || player.mount.Active;

  public override void PreUpdate() {
    if (abilities.chargeDash) {
      SetState(AbilityState.Inactive);
      return;
    }

    if (CanUse && input.dash.JustPressed && !(abilities.burrow.CanUse && input.burrow.JustPressed)) {
      SetState(AbilityState.Active);
      StartDash();
      return;
    }
    if (!InUse) return;
    UpdateCooldown();
    if (abilities.airJump) {
      SetState(AbilityState.Inactive);
      player.velocity.X = Speeds[24] * _direction; // Rip hyperspeed dash-jump
      StartCooldown(); //force = true
    }
    else if (stateTime > Duration || OnWall || abilities.bash) {
      SetState(AbilityState.Inactive);
      StartCooldown(); //force = true
    }
  }
}
