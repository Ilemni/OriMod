using System;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for an air-to-ground Area of Effect attack.
  /// </summary>
  public sealed class Stomp : Ability, ILevelable {
    internal Stomp(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityId.Stomp;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 3;

    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !InUse && !player.mount.Active && player.grapCount == 0 &&
      !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.chargeJump && !abilities.climb &&
      !abilities.dash && !abilities.glide.Active && !abilities.launch && !abilities.wallChargeJump;
    protected override int Cooldown => Math.Min(30 + Level * 30, 600);
    protected override Color RefreshColor => Color.Orange;

    private int Damage => 30 + Level * 20;

    private static float Gravity => 8f;
    private int StartDuration {
      get {
        switch (Level) {
          case 0: return 100000; // \o/ \\
          case 1: return 24;
          case 2: return 20;
          default: return 16;
        }
      }
    }

    private static int MinDuration => 30;
    private float MaxFallSpeed {
      get {
        switch (Level) {
          case 0: return 300;
          case 1: return 28;
          case 2: return 36;
          default: return 25 + Level * 5;
        }
      }
    }

    /// <summary>
    /// Minimum frames required to hold <see cref="Player.controlDown"/> before Stomp can start.
    /// </summary>
    private static int HoldDownDelay => (int)(OriMod.ConfigClient.stompHoldDownDelay * 30);

    private int _currentHoldDown;

    private readonly RandomChar _randStart = new RandomChar();
    private readonly RandomChar _randActive = new RandomChar();
    private readonly RandomChar _randEnd = new RandomChar();

    protected override void UpdateStarting() {
      if (CurrentTime == 0) {
        oPlayer.PlaySound("Ori/Stomp/seinStompStart" + _randStart.NextNoRepeat(3), 0.8f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }

    protected override void UpdateActive() {
      if (CurrentTime == 0) {
        oPlayer.PlaySound("Ori/Stomp/seinStompFall" + _randActive.NextNoRepeat(3), 0.8f);
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
    }

    internal void EndStomp() {
      oPlayer.PlaySound("Ori/Stomp/seinStompImpact" + _randEnd.NextNoRepeat(3), 0.9f);
      abilities.airJump.currentCount = 0;
      player.velocity = Vector2.Zero;
      Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) {
        Dust dust = Dust.NewDustDirect(position, 30, 15, DustID.Clentaminator_Cyan, 0f, 0f, 0, Color.White);
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.velocity *= new Vector2(6, 1.5f);
        dust.velocity.Y = -Math.Abs(dust.velocity.Y);
      }
      PutOnCooldown();
      NewAbilityProjectile<StompEnd>(damage: Damage);
      SetState(State.Inactive);
    }

    protected override void UpdateUsing() {
      player.controlUp = false;
      player.controlDown = false;
      if (Starting) {
        player.controlLeft = false;
        player.controlRight = false;
      }
      player.controlHook = false;
      player.controlMount = false;
      player.controlThrow = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
      oPlayer.KillGrapples();
    }

    internal override void Tick() {
      if (Inactive) {
        if (CanUse) {
          if (input.stomp.JustPressed) {
            _currentHoldDown = 1;
          }
          if (_currentHoldDown >= 1 && input.stomp.Current) {
            _currentHoldDown++;
            if (_currentHoldDown > HoldDownDelay) {
              SetState(State.Starting);
              _currentHoldDown = 0;
            }
          }
        }
        if (Starting) {
          _currentHoldDown = 0;
        }
        else {
          TickCooldown();
        }
      }
      else if (Starting) {
        if (CurrentTime > StartDuration) {
          SetState(State.Active);
        }
      }
      else if (Active) {
        if (CurrentTime > MinDuration && !player.controlDown || abilities.airJump) {
          SetState(State.Inactive);
        }
        if (oPlayer.IsGrounded) {
          EndStomp();
        }
      }
    }
  }
}
