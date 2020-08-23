using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;
using OriMod.Utilities;
using Terraria.ModLoader;
using OriMod.Projectiles.Abilities;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for an air-to-ground Area of Effect attack.
  /// </summary>
  public sealed class Stomp : Ability {
    internal Stomp(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Stomp;

    internal override bool UpdateCondition => InUse || oPlayer.Input(PlayerInput.Triggers.JustPressed.Down);
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !InUse && !Manager.dash.InUse && !Manager.chargeDash.InUse && !Manager.glide.Active && !Manager.climb.InUse && !Manager.stomp.Active && !player.mount.Active && player.grapCount == 0;
    protected override int Cooldown => (int)(Config.StompCooldown * 30);
    protected override Color RefreshColor => Color.Orange;

    private float Gravity => 8f;
    private float MaxFallSpeed => Config.StompFallSpeed;
    private int StartDuration => 24;
    private int MinDuration => 30;

    /// <summary>
    /// Minimum frames required to hold <see cref="TriggersSet.Down"/> before Stomp can start.
    /// </summary>
    private int HoldDownDelay => (int)(OriMod.ConfigClient.StompHoldDownDelay * 30);

    private int currentHoldDown;

    public Projectile PlayerHitboxProjectile { get; private set; }

    private readonly RandomChar randStart = new RandomChar();
    private readonly RandomChar randActive = new RandomChar();
    private readonly RandomChar randEnd = new RandomChar();

    protected override void UpdateStarting() {
      if (currentTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + randStart.NextNoRepeat(3), 1f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }

    protected override void UpdateActive() {
      if (currentTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + randActive.NextNoRepeat(3));
        PlayerHitboxProjectile = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<StompProjectile>(), 30, 0f, player.whoAmI, 0, 1)];
      }
      if (Manager.airJump.Active) {
        return;
      }

      player.maxRunSpeed = 1f;
      player.runSlowdown = 8;
      player.gravity = Gravity;
      player.maxFallSpeed = MaxFallSpeed;
      oPlayer.immuneTimer = 12;
    }

    internal void EndStomp() {
      oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + randEnd.NextNoRepeat(3));
      Manager.airJump.CurrentCount = 0;
      player.velocity = Vector2.Zero;
      var position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) {
        Dust dust = Main.dust[Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, Color.White, 1f)];
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.velocity *= new Vector2(6, 1.5f);
        dust.velocity.Y = -Math.Abs(dust.velocity.Y);
      }
      PutOnCooldown();
      Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<StompEnd>(), PlayerHitboxProjectile.damage, 0, player.whoAmI);
      PlayerHitboxProjectile = null;
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
      if (PlayerInput.Triggers.Current.Down && CanUse) {
        if (PlayerInput.Triggers.JustPressed.Down || currentHoldDown > 0) {
          currentHoldDown++;
          if (HoldDownDelay == 0 || currentHoldDown > HoldDownDelay) {
            SetState(State.Starting);
            currentTime = 0;
            currentHoldDown = 0;
          }
        }
      }
      else if (Starting) {
        currentTime++;
        if (currentTime > StartDuration) {
          currentTime = 0;
          SetState(State.Active);
        }
      }
      else if (Active) {
        currentTime++;
        if (currentTime > MinDuration && !PlayerInput.Triggers.Current.Down) {
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
