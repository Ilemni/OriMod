using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Stomp : Ability {
    internal Stomp(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }

    private const int StartDuration = 24;
    private const int MinDuration = 30;
    protected override int Cooldown => 480;
    private const float Gravity = 4f;
    private const float MaxFallSpeed = 28f;
    private int CurrDur = 0;
    /// <summary>
    /// Projectile used by Stomp to damage enemies during and at the end of Stomp
    /// </summary>
    /// <value>Valid Projectile if stomping, null if no longer stomping</value>
    public Projectile Proj { get; private set; }

    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !InUse && !Handler.dash.InUse && !Handler.cDash.InUse && !Handler.glide.Active && !Handler.climb.InUse && !Handler.stomp.Active && !player.mount.Active && player.grapCount == 0;

    protected override void UpdateStarting() {
      if (CurrDur == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompStart" + OriPlayer.RandomChar(3), 1f, 0.2f);
      }
      player.velocity.X = 0;
      player.velocity.Y *= 0.9f;
      player.gravity = -0.1f;
    }

    protected override void UpdateActive() {
      if (CurrDur == 0) {
        oPlayer.PlayNewSound("Ori/Stomp/seinStompFall" + OriPlayer.RandomChar(3));
        Proj = Main.projectile[Projectile.NewProjectile(player.Center, new Vector2(0, 0), oPlayer.mod.ProjectileType("StompHitbox"), 30, 0f, player.whoAmI, 0, 1)];
        Proj.damage = 9 + OriWorld.GlobalSeinUpgrade * 9;
      }
      if (Handler.airJump.Active) return;
      player.velocity.X = 0;
      player.gravity = Gravity;
      player.maxFallSpeed = MaxFallSpeed;
      oPlayer.ImmuneTimer = 12;
    }

    protected override void UpdateEnding() {
      oPlayer.PlayNewSound("Ori/Stomp/seinStompImpact" + OriPlayer.RandomChar(3));
      Vector2 position = new Vector2(player.position.X, player.position.Y + 32);
      for (int i = 0; i < 25; i++) { // does particles
        Dust dust = Main.dust[Terraria.Dust.NewDust(position, 30, 15, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
        dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        dust.velocity *= new Vector2(6, 1.5f);
        if (dust.velocity.Y > 0) {
          dust.velocity.Y = -dust.velocity.Y;
        }
      }
      Proj.width = 600;
      Proj.height = 320;
      Proj.damage = (int)(Proj.damage * 1.6f);
      Proj = null;
      CurrCooldown = Cooldown;
      Refreshed = false;
    }
    protected override void UpdateUsing() {
      player.controlUp = false;
      player.controlDown = false;
      player.controlLeft = false;
      player.controlRight = false;
      player.controlHook = false;
      player.controlMount = false;
      player.controlThrow = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
    }
    internal override void Tick() {
      if (PlayerInput.Triggers.JustPressed.Down && CanUse) {
        Starting = true;
        CurrDur = 0;
      }
      else if (Starting) {
        CurrDur++;
        if (CurrDur > StartDuration) {
          CurrDur = 0;
          Active = true;
        }
      }
      else if (Active) {
        CurrDur++;
        if (CurrDur > MinDuration && !PlayerInput.Triggers.Current.Down) {
          Inactive = true;
        }
        if (oPlayer.IsGrounded) {
          Ending = true;
        }
      }
      else if (Ending) {
        Inactive = true;
      }
      else {
        if (CurrCooldown > 0 || !Refreshed) {
          CurrCooldown--;
          if (CurrCooldown < 0) {
            Refreshed = true;
          }
        }
      }
    }
  }
}