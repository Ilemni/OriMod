using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;
using OriMod.Utilities;

namespace OriMod.Abilities {
  public class Stomp : Ability {
    internal Stomp(AbilityManager handler) : base(handler) { }
    public override int Id => AbilityID.Stomp;

    internal override bool DoUpdate => InUse || oPlayer.Input(PlayerInput.Triggers.JustPressed.Down);
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !InUse && !Manager.dash.InUse && !Manager.cDash.InUse && !Manager.glide.Active && !Manager.climb.InUse && !Manager.stomp.Active && !player.mount.Active && player.grapCount == 0;
    protected override int Cooldown => (int)(Config.StompCooldown * 30);
    protected override Color RefreshColor => Color.Orange;

    private float Gravity => 8f;
    private float MaxFallSpeed => Config.StompFallSpeed;
    private int StartDuration => 24;
    private int MinDuration => 30;
    private int HoldDownDelay => (int)(OriMod.ConfigClient.StompHoldDownDelay * 30);

    private int CurrHoldDown;

    public Projectile Proj { get; private set; }

    private readonly RandomChar randCharStart = new RandomChar();
    private readonly RandomChar randCharActive = new RandomChar();
    private readonly RandomChar randCharEnd = new RandomChar();

    protected override void UpdateStarting() {
      if (CurrTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + randCharStart.NextNoRepeat(3), 1f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }

    protected override void UpdateActive() {
      if (CurrTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + randCharActive.NextNoRepeat(3));
        Proj = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("StompProjectile"), 30, 0f, player.whoAmI, 0, 1)];
        Proj.damage = 9 + OriWorld.GlobalSeinUpgrade * 9;
      }
      if (Manager.airJump.Active) {
        return;
      }

      player.maxRunSpeed = 1f;
      player.runSlowdown = 8;
      player.gravity = Gravity;
      player.maxFallSpeed = MaxFallSpeed;
      oPlayer.ImmuneTimer = 12;
    }

    internal void EndStomp() {
      oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + randCharEnd.NextNoRepeat(3));
      Manager.airJump.CurrCount = 0;
      player.velocity = Vector2.Zero;
      var position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) {
        Dust dust = Main.dust[Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, Color.White, 1f)];
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.velocity *= new Vector2(6, 1.5f);
        dust.velocity.Y = -Math.Abs(dust.velocity.Y);
      }
      PutOnCooldown();
      Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("StompEnd"), Proj.damage, 0, player.whoAmI);
      Proj = null;
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
      oPlayer.KillGrapples();
      player.controlMount = false;
      player.controlThrow = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
    }

    internal override void Tick() {
      if (PlayerInput.Triggers.Current.Down && CanUse) {
        if (PlayerInput.Triggers.JustPressed.Down || CurrHoldDown > 0) {
          CurrHoldDown++;
          if (HoldDownDelay == 0 || CurrHoldDown > HoldDownDelay) {
            SetState(State.Starting);
            CurrTime = 0;
            CurrHoldDown = 0;
          }
        }
      }
      else if (Starting) {
        CurrTime++;
        if (CurrTime > StartDuration) {
          CurrTime = 0;
          SetState(State.Active);
        }
      }
      else if (Active) {
        CurrTime++;
        if (CurrTime > MinDuration && !PlayerInput.Triggers.Current.Down) {
          SetState(State.Inactive);
        }
        if (oPlayer.IsGrounded) {
          EndStomp();
        }
      }
      else {
        CurrHoldDown = 0;
        TickCooldown();
      }
    }
  }
}
