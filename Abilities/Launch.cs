﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for launching the player in the desired direction. Used in the air.
  /// </summary>
  /// <remarks>
  /// Seems a common consensus that as an ability that is actually a leveled version of another ability,
  /// it would be more fitting as a Charge Jump Lv3, rather than Bash Lv3.
  /// This must be a separate class rather than built into Bash, as this would otherwise require
  /// Bash to be unlocked as well to be usable.
  /// </remarks>
  public sealed class Launch : Ability {
    internal Launch(AbilityManager manager) : base(manager) {
    }

    public override int Id => AbilityId.Launch;
    public override byte Level => (byte) Math.Max(0, levelableDependency.Level - 2);
    protected override ILevelable levelableDependency => abilities.chargeJump;

    /// <summary>
    /// Bash restrictions, plus in air and bash failed
    /// </summary>
    internal override bool CanUse => base.CanUse && Inactive && !oPlayer.IsGrounded && !player.mount.Active &&
                                     !abilities.bash && !abilities.burrow && !abilities.chargeDash &&
                                     !abilities.chargeJump && !abilities.climb &&
                                     !abilities.dash && !abilities.stomp && !abilities.wallChargeJump;

    private ushort CurrentChain { get; set; }

    private ushort MaxChain {
      get {
        switch (Level) {
          case 1: return 1;
          case 2: return 3;
          case 3: return 7;
          default: return (ushort) (Level * 3 + 1);
        }
      }
    }

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
    private Vector2 LaunchDirection => new Vector2((float) Math.Cos(LaunchAngle), (float) Math.Sin(LaunchAngle));
    private readonly RandomChar _rand = new RandomChar();

    protected override void ReadPacket(BinaryReader r) {
      if (!InUse) return;
      CurrentChain = r.ReadUInt16();
      LaunchAngle = r.ReadSingle();
    }

    protected override void WritePacket(ModPacket packet) {
      if (!InUse) return;
      packet.Write(CurrentChain);
      packet.Write(LaunchAngle);
    }

    protected override void UpdateUsing() {
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

    protected override void UpdateActive() {
      if (CurrentTime == 0) {
        NewAbilityProjectile<LaunchProjectile>(damage: 70);
      }

      player.pulley = false;
      player.velocity = LaunchDirection * LaunchSpeed;
      oPlayer.immuneTimer = 5;
    }

    private void End() {
      player.velocity = LaunchDirection * 10;
      oPlayer.UnrestrictedMovement = true;
      Refreshed = false;
    }

    internal override void Tick() {
      if (CanUse && input.bash.JustPressed) {
        if (CurrentChain == 0) {
          oPlayer.PlayLocalSound("Ori/Bash/seinBashStartA", 0.5f);
        }

        SetState(State.Starting);
        CurrentChain = 1;
      }
      else if (!InUse) {
        TickCooldown();
        return;
      }

      if (oPlayer.IsGrounded || oPlayer.OnWall) {
        // Prevent any usage of Launch while not in air
        SetState(State.Inactive);
        return;
      }

      if (Starting) {
        if (CurrentTime > MaxLaunchDuration || CurrentTime > MinLaunchDuration && !input.bash.Current) {
          SetState(State.Active);
        }
        else if (CurrentChain > 1 && !input.bash.Current) {
          SetState(State.Inactive);
        }

        return;
      }

      if (!Active) return;
      if (oPlayer.IsGrounded || oPlayer.OnWall) {
        SetState(State.Inactive);
      }

      // Post-ending state depends on player input
      // Maybe too sensitive to rely on input packet
      if (!IsLocal || CurrentTime <= EndDuration) return;
      if (CurrentChain < MaxChain && input.bash.Current) {
        CurrentChain++;
        SetState(State.Starting);
        oPlayer.PlaySound("Ori/Bash/seinBashEnd" + _rand.NextNoRepeat(3), Level == 3 ? 0.15f : 0.35f);
      }
      else {
        End();
        SetState(State.Inactive);
        oPlayer.PlaySound("Ori/Bash/seinBashEnd" + _rand.NextNoRepeat(3), 0.55f);
      }
    }

    protected override void TickCooldown() {
      if (!oPlayer.IsGrounded && !abilities.bash) return;
      Refreshed = true;
      CurrentChain = 0;
    }
  }
}