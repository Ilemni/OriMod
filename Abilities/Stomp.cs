using System;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.Graphics.Shaders;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for an air-to-ground Area of Effect attack.
  /// </summary>
  public sealed class Stomp : Ability, ILevelable {
    internal Stomp(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Stomp;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 3;

    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !InUse && !abilities.dash && !abilities.chargeDash && !abilities.glide.Active && !abilities.climb && !abilities.stomp.Active && !player.mount.Active && player.grapCount == 0;
    protected override int Cooldown => Math.Min(30 + Level * 30, 600);
    protected override Color RefreshColor => Color.Orange;

    public int Damage => 30 + Level * 20;

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
    private static int HoldDownDelay => (int)(OriMod.ConfigClient.StompHoldDownDelay * 30);

    private int currentHoldDown;

    private readonly RandomChar randStart = new RandomChar();
    private readonly RandomChar randActive = new RandomChar();
    private readonly RandomChar randEnd = new RandomChar();

    protected override void UpdateStarting() {
      if (CurrentTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + randStart.NextNoRepeat(3), 0.8f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }

    protected override void UpdateActive() {
      if (CurrentTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + randActive.NextNoRepeat(3), 0.8f);
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
      oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + randEnd.NextNoRepeat(3), 0.9f);
      abilities.airJump.currentCount = 0;
      player.velocity = Vector2.Zero;
      var position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) {
        Dust dust = Main.dust[Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, Color.White, 1f)];
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
      if (player.controlDown && CanUse) {
        currentHoldDown++;
        if (HoldDownDelay == 0 || currentHoldDown > HoldDownDelay) {
          SetState(State.Starting);
          currentHoldDown = 0;
        }
      }
      else if (Starting) {
        if (CurrentTime > StartDuration) {
          SetState(State.Active);
        }
      }
      else if (Active) {
        if (CurrentTime > MinDuration && !player.controlDown) {
          SetState(State.Inactive);
        }
        if (oPlayer.IsGrounded) {
          EndStomp();
        }
      }
      else {
        currentHoldDown = 0;
        TickCooldown();
      }
    }
  }
}
