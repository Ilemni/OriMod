using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for launching the player in the desired direction. Used in the air.
/// </summary>
/// <remarks>
/// Seems a common consensus that as an ability that is actually a leveled version of another ability,
/// it would be more fitting as a Charge Jump Lv3, rather than Bash Lv3.
/// This must be a separate class rather than built into Bash, as this would otherwise require
/// Bash to be unlocked as well to be usable.
/// </remarks>
public sealed class Launch : OriAbility {
  public override int Id => AbilityId.Launch;
  public override int Level => Math.Max(0, levelableDependency.Level - 2);
  public override ILevelable levelableDependency => abilities.chargeJump;
  public override bool Unlocked => Level > 0;

  /// <summary>
  /// Bash restrictions, plus in air and bash failed
  /// </summary>
  public override bool CanUse => base.CanUse && Inactive && !IsGrounded && !player.mount.Active &&
    !abilities.bash && !abilities.burrow && !abilities.chargeDash &&
    !abilities.chargeJump && !abilities.climb &&
    !abilities.dash && !abilities.stomp && !abilities.wallChargeJump;

  public ushort CurrentChain { get; set; }

  private ushort MaxChain =>
    Level switch {
      1 => 1,
      2 => 3,
      3 => 7,
      _ => (ushort)(Level * 3 + 1)
    };

  // Surely there's a better way to do this
  private int MinLaunchDuration =>
    Level == 3
      ? CurrentChain == 1 ? 8 : 11
      : CurrentChain == 1 ? 15 : 20;

  private int MaxLaunchDuration =>
    Level == 3
      ? CurrentChain == 1 ? 20 : 15
      : CurrentChain == 1 ? 45 : 30;

  private int EndDuration =>
    Level == 3
      ? CurrentChain == 1 || CurrentChain == MaxChain ? 9 : 4
      : CurrentChain == 1 || CurrentChain == MaxChain ? 12 : 6;

  private float LaunchSpeed =>
    Level == 3
      ? CurrentChain == 1 ? 40 : 45
      : CurrentChain == 1 ? 25 : 40;

  public float LaunchAngle { get; private set; }
  private Vector2 LaunchDirection => new((float)Math.Cos(LaunchAngle), (float)Math.Sin(LaunchAngle));
  private readonly RandomChar _rand = new();

  public override void ReadPacket(BinaryReader r) {
    CurrentChain = r.ReadUInt16();
    LaunchAngle = r.ReadSingle();
    player.position = r.ReadVector2();
    player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(CurrentChain);
    packet.Write(LaunchAngle);
    packet.WriteVector2(player.position);
    packet.WriteVector2(player.velocity);
  }

  public override void UpdateUsing() {
    if (!Active) {
      if (IsLocal) {
        OriUtils.GetMouseDirection(oPlayer, out float angle, Vector2.One);
        LaunchAngle = angle;
      }

      player.velocity *= 0.86f;
      player.gravity = 0;
      player.runSlowdown = 0;
    }

    if (IsLocal) {
      netUpdate = true;
    }

    player.maxFallSpeed = LaunchSpeed;
    // Allow only quick heal and quick mana
    player.controlJump = false;
    player.controlUp = false;
    player.controlDown = false;
    player.controlLeft = false;
    player.controlRight = false;
    player.controlHook = false;
    player.controlInv = false;
    player.controlMount = false;
    player.controlSmart = false;
    player.controlThrow = false;
    player.controlTorch = false;
    player.controlUseItem = false;
    player.controlUseTile = false;
    player.buffImmune[BuffID.CursedInferno] = true;
    player.buffImmune[BuffID.Dazed] = true;
    player.buffImmune[BuffID.Frozen] = true;
    player.buffImmune[BuffID.Frostburn] = true;
    player.buffImmune[BuffID.MoonLeech] = true;
    player.buffImmune[BuffID.Obstructed] = true;
    player.buffImmune[BuffID.OnFire] = true;
    player.buffImmune[BuffID.Poisoned] = true;
    player.buffImmune[BuffID.ShadowFlame] = true;
    player.buffImmune[BuffID.Silenced] = true;
    player.buffImmune[BuffID.Slow] = true;
    player.buffImmune[BuffID.Stoned] = true;
    player.buffImmune[BuffID.Suffocation] = true;
    player.buffImmune[BuffID.Venom] = true;
    player.buffImmune[BuffID.Weak] = true;
    player.buffImmune[BuffID.WitheredArmor] = true;
    player.buffImmune[BuffID.WitheredWeapon] = true;
    player.buffImmune[BuffID.WindPushed] = true;
  }

  public override void UpdateActive() {
    if (stateTime == 0) {
      NewAbilityProjectile<LaunchProjectile>(damage: 70);
    }

    player.pulley = false;
    player.velocity = LaunchDirection * LaunchSpeed;
    oPlayer.immuneTimer = 5;
  }

  private void End() {
    player.velocity = LaunchDirection * 10;
    StartCooldown();
  }

  public override void PreUpdate() {
    if (CanUse && input.bash.JustPressed && IsLocal) {
      if (CurrentChain == 0) {
        PlayLocalSound("Ori/Bash/seinBashStartA", 0.5f);
      }

      SetState(AbilityState.Starting);
      RestoreAirJumps();
      CurrentChain = 1;
    }
    else if (!InUse) return;

    if (IsGrounded || OnWall) {
      // Prevent any usage of Launch while not in air
      SetState(AbilityState.Inactive);
      return;
    }

    if (Starting) {
      if (stateTime > MaxLaunchDuration || (stateTime >= MinLaunchDuration && !input.bash.Current)) {
        SetState(AbilityState.Active);
      }

      return;
    }

    if (!Active) return;
    if (IsGrounded || OnWall) {
      SetState(AbilityState.Inactive);
    }

    // Post-ending state depends on player input
    // Maybe too sensitive to rely on input packet
    if (!IsLocal || stateTime <= EndDuration) return;
    if (CurrentChain < MaxChain && input.bash.Current) {
      CurrentChain++;
      SetState(AbilityState.Starting);
      PlaySound("Ori/Bash/seinBashEnd" + _rand.NextNoRepeat(3), Level == 3 ? 0.15f : 0.35f);
    }
    else {
      End();
      SetState(AbilityState.Inactive);
      PlaySound("Ori/Bash/seinBashEnd" + _rand.NextNoRepeat(3), 0.55f);
    }
  }

  public override void UpdateCooldown() {
    if (CurrentChain == 0) EndCooldown();
  }
}
