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
  public override int Level => Math.Max(0, LevelableDependency.Level - 2);
  public override ILevelable LevelableDependency => Abilities.ChargeJump;
  public override bool Unlocked => Level > 0;

  /// <summary>
  /// Bash restrictions, plus in air and bash failed
  /// </summary>
  public override bool CanUse => base.CanUse && Inactive && !IsGrounded && !Player.mount.Active &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.ChargeDash &&
    !Abilities.ChargeJump && !Abilities.Climb &&
    !Abilities.Dash && !Abilities.Stomp && !Abilities.WallChargeJump;

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
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(CurrentChain);
    packet.Write(LaunchAngle);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }

  public override void UpdateUsing() {
    if (!Active) {
      if (IsLocal) {
        OriUtils.GetMouseDirection(oPlayer, out float angle, Vector2.One);
        LaunchAngle = angle;
      }

      Player.velocity *= 0.86f;
      Player.gravity = 0;
      Player.runSlowdown = 0;
    }

    if (IsLocal) {
      NetUpdate = true;
    }

    Player.maxFallSpeed = LaunchSpeed;
    // Allow only quick heal and quick mana
    Player.controlJump = false;
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlHook = false;
    Player.controlInv = false;
    Player.controlMount = false;
    Player.controlSmart = false;
    Player.controlThrow = false;
    Player.controlTorch = false;
    Player.controlUseItem = false;
    Player.controlUseTile = false;
    Player.buffImmune[BuffID.CursedInferno] = true;
    Player.buffImmune[BuffID.Dazed] = true;
    Player.buffImmune[BuffID.Frozen] = true;
    Player.buffImmune[BuffID.Frostburn] = true;
    Player.buffImmune[BuffID.MoonLeech] = true;
    Player.buffImmune[BuffID.Obstructed] = true;
    Player.buffImmune[BuffID.OnFire] = true;
    Player.buffImmune[BuffID.Poisoned] = true;
    Player.buffImmune[BuffID.ShadowFlame] = true;
    Player.buffImmune[BuffID.Silenced] = true;
    Player.buffImmune[BuffID.Slow] = true;
    Player.buffImmune[BuffID.Stoned] = true;
    Player.buffImmune[BuffID.Suffocation] = true;
    Player.buffImmune[BuffID.Venom] = true;
    Player.buffImmune[BuffID.Weak] = true;
    Player.buffImmune[BuffID.WitheredArmor] = true;
    Player.buffImmune[BuffID.WitheredWeapon] = true;
    Player.buffImmune[BuffID.WindPushed] = true;
  }

  public override void UpdateActive() {
    if (StateTime == 0) {
      NewAbilityProjectile<LaunchProjectile>(damage: 70);
    }

    Player.pulley = false;
    Player.velocity = LaunchDirection * LaunchSpeed;
    oPlayer.ImmuneTimer = 5;
  }

  private void End() {
    Player.velocity = LaunchDirection * 10;
    StartCooldown();
  }

  public override void PreUpdate() {
    if (CanUse && Input.Charge.Current && Input.Bash.JustPressed && IsLocal) {
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
      if (StateTime > MaxLaunchDuration || (StateTime >= MinLaunchDuration && !Input.Bash.Current)) {
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
    if (!IsLocal || StateTime <= EndDuration) return;
    if (CurrentChain < MaxChain && Input.Bash.Current) {
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
