using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Stomp : Ability {
    internal Stomp(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    public override int id => AbilityID.Stomp;
    internal override bool DoUpdate => InUse || oPlayer.Input(OriPlayer.JustPressed.Down);
    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !InUse && !Handler.dash.InUse && !Handler.cDash.InUse && !Handler.glide.Active && !Handler.climb.InUse && !Handler.stomp.Active && !player.mount.Active && player.grapCount == 0;
    protected override int Cooldown => (int)(Config.StompCooldown * 30);
    protected override Color RefreshColor => Color.Orange;

    private float Gravity => 8f;
    private float MaxFallSpeed => Config.StompFallSpeed;
    private int StartDuration => 24;
    private int MinDuration => 30;
    private int HoldDownDelay => (int)(OriMod.ConfigClient.StompHoldDownDelay * 30);

    private int CurrHoldDown;
    
    public Projectile Proj { get; private set; }

    protected override void UpdateStarting() {
      if (CurrTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + OriPlayer.RandomChar(3), 1f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }
    protected override void UpdateActive() {
      if (CurrTime == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + OriPlayer.RandomChar(3));
        Proj = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("StompProjectile"), 30, 0f, player.whoAmI, 0, 1)];
        Proj.damage = 9 + OriWorld.GlobalSeinUpgrade * 9;
      }
      if (Handler.airJump.Active) return;
      player.maxRunSpeed = 1f;
      player.runSlowdown = 8;
      player.gravity = Gravity;
      player.maxFallSpeed = MaxFallSpeed;
      oPlayer.ImmuneTimer = 12;
    }
    internal void EndStomp() {
      oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + OriPlayer.RandomChar(3));
      Handler.airJump.CurrCount = 0;
      player.velocity = Vector2.Zero;
      Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) {
        Dust dust = Main.dust[Terraria.Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, Color.White, 1f)];
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.velocity *= new Vector2(6, 1.5f);
        dust.velocity.Y = -Math.Abs(dust.velocity.Y);
      }
      PutOnCooldown();
      Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("StompEnd"), Proj.damage, 0, player.whoAmI);
      Proj = null;
      Inactive = true;
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
            Starting = true;
            CurrTime = 0;
            CurrHoldDown = 0;
          }
        }
      }
      else if (Starting) {
        CurrTime++;
        if (CurrTime > StartDuration) {
          CurrTime = 0;
          Active = true;
        }
      }
      else if (Active) {
        CurrTime++;
        if (CurrTime > MinDuration && !PlayerInput.Triggers.Current.Down) {
          Inactive = true;
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