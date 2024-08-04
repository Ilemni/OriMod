using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for an air-to-ground Area of Effect attack.
/// </summary>
public sealed class Stomp : OriAbility, ILevelable {
  public override int Id => AbilityId.Stomp;
  public override int Level => ((ILevelable)this).Level;
  int ILevelable.Level { get; set; }
  int ILevelable.MaxLevel => 3;
  public override bool Unlocked => Level > 0;

  public override bool CanUse => base.CanUse && !IsGrounded && !InUse && !Player.mount.Active && Player.grapCount == 0 &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.ChargeDash && !Abilities.ChargeJump && !Abilities.Climb &&
    !Abilities.Dash && !Abilities.Glide.Active && !Abilities.Launch && !Abilities.WallChargeJump;
  public override int Cooldown => Math.Min(30 + Level * 30, 600);
  public override void OnRefreshed() => Abilities.RefreshParticles(Color.Orange);

  private int Damage => 30 + Level * 20;

  private static float Gravity => 8f;
  private int StartDuration =>
    Level switch {
      1 => 24,
      2 => 20,
      _ => 16
    };

  private static int MinDuration => 30;
  private float MaxFallSpeed =>
    Level switch {
      1 => 28,
      2 => 36,
      _ => 25 + Level * 5
    };

  /// <summary>
  /// Minimum frames required to hold <see cref="Player.controlDown"/> before Stomp can start.
  /// </summary>
  private static int HoldDownDelay => (int)(OriMod.ConfigClient.stompHoldDownDelay * 30);

  private int _currentHoldDown;

  private RandomChar _randStart;
  private RandomChar _randActive;
  private RandomChar _randEnd;

  public override void UpdateStarting() {
    if (StateTime == 0) {
      Abilities.oPlayer.PlaySound("Ori/Stomp/seinStompStart" + _randStart.NextNoRepeat(3), 0.8f, 0.2f);
    }
    Player.velocity.X = 0;
    Player.velocity.Y *= 0.9f;
    Player.gravity = -0.1f;
  }

  public override void UpdateActive() {
    if (StateTime == 0) {
      Abilities.oPlayer.PlaySound("Ori/Stomp/seinStompFall" + _randActive.NextNoRepeat(3), 0.8f);
      NewAbilityProjectile<StompProjectile>(damage: Damage * 2);
    }
    if (Abilities.AirJump.Active) {
      return;
    }

    Player.maxRunSpeed = 1f;
    Player.runSlowdown = 8;
    Player.gravity = Gravity;
    Player.maxFallSpeed = MaxFallSpeed;
    oPlayer.ImmuneTimer = 12;

    if (IsLocal) NetUpdate = true;
  }

  internal void EndStomp() {
    PlaySound("Ori/Stomp/seinStompImpact" + _randEnd.NextNoRepeat(3), 0.9f);
    RestoreAirJumps();
    Player.velocity = Vector2.Zero;
    Vector2 position = new(Player.position.X, Player.position.Y + 32);
    for (int i = 0; i < 25; i++) {
      Dust dust = Dust.NewDustDirect(position, 30, 15, DustID.Clentaminator_Cyan, 0f, 0f, 0, Color.White);
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.velocity *= new Vector2(6, 1.5f);
      dust.velocity.Y = -Math.Abs(dust.velocity.Y);
    }
    StartCooldown();
    NewAbilityProjectile<StompEnd>(damage: Damage);
    SetState(AbilityState.Inactive);
  }

  public override void UpdateUsing() {
    Player.controlUp = false;
    Player.controlDown = false;
    if (Starting) {
      Player.controlLeft = false;
      Player.controlRight = false;
    }
    Player.controlJump = false;
    Player.controlHook = false;
    Player.controlMount = false;
    Player.controlThrow = false;
    Player.controlUseItem = false;
    Player.controlUseTile = false;
    oPlayer.KillGrapples();
  }

  public override void PreUpdate() {
    if (Inactive) {
      if (CanUse) {
        if (Input.Stomp.JustPressed) {
          _currentHoldDown = 1;
        }
        if (_currentHoldDown >= 1 && Player.controlDown && Input.Stomp.Current && IsLocal) {
          _currentHoldDown++;
          if (_currentHoldDown > HoldDownDelay) {
            _currentHoldDown = 0;
            SetState(AbilityState.Starting);
          }
        }
      }
      if (Starting) {
        _currentHoldDown = 0;
      }
    }
    else if (Starting) {
      if (StateTime > StartDuration) {
        SetState(AbilityState.Active);
      }
      if (Abilities.AirJump.State == AbilityState.Active) {
        SetState(AbilityState.Inactive);
        StartCooldown();
      }
    }
    else if (Active) {
      if ((StateTime > MinDuration && !Player.controlDown) || Abilities.AirJump) {
        SetState(AbilityState.Inactive);
        StartCooldown();
      }
      if (IsGrounded) {
        EndStomp();
      }
    }
  }

  public override void ReadPacket(BinaryReader r) {
    _currentHoldDown = r.ReadInt32();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_currentHoldDown);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }
}
