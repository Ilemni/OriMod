using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System;
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

  public override bool CanUse => base.CanUse && !InUse && !IsOnCooldown && !OnWall && !Player.mount.Active && (Level >= 2 || IsGrounded) &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.ChargeDash && !Abilities.Launch && !Abilities.Stomp && !Abilities.ChargeJump && !Abilities.WallChargeJump;
  public override int Cooldown => Level >= 3 ? 0 : 60;
  public override void OnRefreshed() => Abilities.RefreshParticles(Color.White);

  private static float[] Speeds => _speeds ??= Unloadable.New(new float[25] {
    50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
    19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
  }, () => _speeds = null);
  private static float[] _speeds;
  private static int Duration => Speeds.Length - 1;

  private sbyte _direction;
  
  internal ushort CurrentCount;
  private int MaxDashes => 1;

  private RandomChar _rand;

  internal void StartDash() {
    SetState(AbilityState.Active);
    _direction = (sbyte)(Player.controlLeft ? -1 : Player.controlRight ? 1 : Player.direction);
    PlaySound("Ori/Dash/seinDash" + _rand.NextNoRepeat(3), 0.2f);
    Player.pulley = false;
    CurrentCount++;
  }

  public override void ReadPacket(BinaryReader r) {
    _direction = r.ReadSByte();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_direction);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }

  public override void UpdateActive() {
    if (Player.controlJump && Player.AnyExtraJumpUsable()) {
      SetState(AbilityState.Inactive);
      StartCooldown();
      return;
    }
    Player.velocity.X = Speeds[StateTime] * 0.5f * _direction;
    Player.velocity.Y = 0.25f * (StateTime + 1) * Player.gravDir;
    if (IsLocal) NetUpdate = true;
  }

  public override bool RefreshCondition() => CurrentCount < MaxDashes || Player.mount.Active;

  public override void PreUpdate() {
    if (Abilities.ChargeDash) {
      SetState(AbilityState.Inactive);
      return;
    }

    if (CanUse && Input.Dash.JustPressed && 
      !(Abilities.ChargeDash.CanUse && Input.Charge.Current) && 
      !(Abilities.Burrow.CanUse && Input.Burrow.JustPressed))
    {
      StartDash();
      return;
    }
    if (!InUse) return;
    UpdateCooldown();
    if (Abilities.AirJump || Input.Jump.JustPressed) {
      SetState(AbilityState.Inactive);
      Player.velocity.X = Math.Min(Speeds[24], Math.Abs(Player.velocity.X)) * _direction; // Rip hyperspeed dash-jump
      StartCooldown(); //force = true
    }
    else if (StateTime > Duration || OnWall || Abilities.Bash) {
      SetState(AbilityState.Inactive);
      StartCooldown(); //force = true
    }
  }
}
