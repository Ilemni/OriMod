using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;

namespace OriMod.Movements {
  public class Stomp : Movement {
    public Stomp(OriPlayer oriPlayer, MovementHandler handler) : base(oriPlayer, handler) { }

    public const int stompStartDur = 24;
    public const int stompMinDur = 60;
    public const float stompGrav = 4f;
    public const float stompMaxFallSpeed = 28f;
    public int stompCurrDur = 0;
    public Projectile stompProj;

    public override void Starting() {
      if (stompCurrDur == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + OriPlayer.RandomChar(3), 1f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }

    public override void Active() {
      if (stompCurrDur == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + OriPlayer.RandomChar(3));
        stompProj = Main.projectile[Projectile.NewProjectile(player.Center, new Vector2(0, 0), oPlayer.mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1)];
      }
      player.velocity.X = 0;
      player.gravity = stompGrav;
      player.maxFallSpeed = stompMaxFallSpeed;
      player.immune = true;
    }

    public override void Ending() {
      oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + OriPlayer.RandomChar(3));
      Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) { // does particles
        Dust dust = Main.dust[Terraria.Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.velocity *= new Vector2(2, 0.5f);
        if (dust.velocity.Y > 0) {
          dust.velocity.Y = -dust.velocity.Y;
        }
      }
      stompProj.width = 600;
      stompProj.height = 320;
    }
    public override void Using() {
      player.controlUp = false;
      player.controlDown = false;
      player.controlLeft = false;
      player.controlRight = false;
    }
    public override void Tick() {
      canUse = !oPlayer.isGrounded && !inUse && !Handler.IsInUse("Dash") && !Handler.IsInUse("ChargeDash") && !Handler.glide.inUse;
      if (PlayerInput.Triggers.JustPressed.Down && canUse) {
        state = State.Starting;
        stompCurrDur = 0;
        canUse = false;
      }
      else if (IsState(State.Starting)) {
        stompCurrDur++;
        if (stompCurrDur > stompStartDur) {
          stompCurrDur = 0;
          state = State.Active;
        }
      }
      else if (IsState(State.Active)) {
        stompCurrDur++;
        if (stompCurrDur > stompMinDur && !PlayerInput.Triggers.Current.Down) {
          state = State.Inactive;
          canUse = true;
        }
        if (oPlayer.isGrounded) {
          state = State.Ending;
          canUse = false;
        }
      }
      else if (IsState(State.Ending)) {
        state = State.Inactive;
      }
    }
  }
}