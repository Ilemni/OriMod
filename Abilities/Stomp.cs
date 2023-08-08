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

  public override bool CanUse => base.CanUse && !IsGrounded && !InUse && !player.mount.Active && player.grapCount == 0 &&
    !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.chargeJump && !abilities.climb &&
    !abilities.dash && !abilities.glide.Active && !abilities.launch && !abilities.wallChargeJump;
  public override int Cooldown => Math.Min(30 + Level * 30, 600);
  public override void OnRefreshed() => abilities.RefreshParticles(Color.Orange);

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

  private readonly RandomChar _randStart = new();
  private readonly RandomChar _randActive = new();
  private readonly RandomChar _randEnd = new();

  public override void UpdateStarting() {
    if (stateTime == 0) {
      abilities.oPlayer.PlaySound("Ori/Stomp/seinStompStart" + _randStart.NextNoRepeat(3), 0.8f, 0.2f);
    }
    player.velocity.X = 0;
    player.velocity.Y *= 0.9f;
    player.gravity = -0.1f;
  }

  public override void UpdateActive() {
    if (stateTime == 0) {
      abilities.oPlayer.PlaySound("Ori/Stomp/seinStompFall" + _randActive.NextNoRepeat(3), 0.8f);
      NewAbilityProjectile<StompProjectile>(damage: Damage * 2);
    }
    if (abilities.airJump.Active) {
      return;
    }

    player.maxRunSpeed = 1f;
    player.runSlowdown = 8;
    player.gravity = Gravity;
    player.maxFallSpeed = MaxFallSpeed;
    oPlayer.immuneTimer = 12;

    if (IsLocal) netUpdate = true;
  }

  internal void EndStomp() {
    PlaySound("Ori/Stomp/seinStompImpact" + _randEnd.NextNoRepeat(3), 0.9f);
    RestoreAirJumps();
    player.velocity = Vector2.Zero;
    Vector2 position = new(player.position.X, player.position.Y + 32);
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
    player.controlUp = false;
    player.controlDown = false;
    if (Starting) {
      player.controlLeft = false;
      player.controlRight = false;
    }
    player.controlJump = false;
    player.controlHook = false;
    player.controlMount = false;
    player.controlThrow = false;
    player.controlUseItem = false;
    player.controlUseTile = false;
    oPlayer.KillGrapples();
  }

  public override void PreUpdate() {
    if (Inactive) {
      if (CanUse) {
        if (input.stomp.JustPressed) {
          _currentHoldDown = 1;
        }
        if (_currentHoldDown >= 1 && player.controlDown && input.stomp.Current && IsLocal) {
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
      if (stateTime > StartDuration) {
          SetState(AbilityState.Active);
      }
      if (abilities.airJump.state == AbilityState.Active) {
          SetState(AbilityState.Inactive);
      }
    }
    else if (Active) {
      if ((stateTime > MinDuration && !player.controlDown) || abilities.airJump) {
          SetState(AbilityState.Inactive);
      }
      if (IsGrounded) {
          EndStomp();
      }
    }
  }

  public override void ReadPacket(BinaryReader r) {
    _currentHoldDown = r.ReadInt32();
    player.position = r.ReadVector2();
    player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_currentHoldDown);
    packet.WriteVector2(player.position);
    packet.WriteVector2(player.velocity);
  }
}
