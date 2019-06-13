using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Projectiles.SpiritLight {
  public class SpiritLight : ModProjectile {
    public readonly int[] toGet = new int[] { 1, 5, 25, 100, 250, 500, 1000, 2500, 5000, 10000, 25000, 50000, 100000 };
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => false;
    public override bool CanHitPlayer(Player target) => false;
    public override bool CanHitPvp(Player target) => false;
    public int Type => (int)projectile.ai[0];
    public bool IsBoss => projectile.ai[1] == 1;
    public float Gravity => 9f;
    public float Bounce => 0.8f;
    public Color LightColor = Color.Black;

    public override void SetDefaults() {
      projectile.Name = "SpiritLight";
      projectile.damage = (int)Math.Pow(10, Type);
      projectile.friendly = false;
      projectile.maxPenetrate = 999;
      projectile.width = 16;
      projectile.height = 16;
      projectile.tileCollide = true;
      projectile.timeLeft = 1200;
    }
    public override void AI() {
      if (LightColor == Color.Black) {
        switch (Type) {
          case 0:
            LightColor = Color.White;
            break;
          case 1:
            LightColor = Color.LightBlue;
            break;
          case 2:
            LightColor = Color.Green;
            break;
          case 3:
            LightColor = Color.Yellow;
            break;
          case 4:
            LightColor = Color.Red;
            break;
          default:
            LightColor = Color.Purple;
            break;
        }
      }
      Lighting.AddLight(projectile.Center, LightColor.ToVector3() * (float)(Type + 3) * 0.2f);
      float dist = Vector2.Distance(projectile.Center, Main.LocalPlayer.Center);
      if (dist < 48) { // Pickup
        // Main.NewText("Type: " + Type);
        OriPlayer oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
        int giveSpiritLight = toGet[Type] * (IsBoss ? 2 : 1);
        try {
          int x = checked(oPlayer.SpiritLight + giveSpiritLight);
          oPlayer.SpiritLight = x;
          Main.NewText("Added " + giveSpiritLight + " Spirit Light! You now have " + oPlayer.SpiritLight + ".");
        }
        catch (OverflowException) {
          oPlayer.SpiritLight = int.MaxValue;
          Main.NewText("Reached maximum Spirit Light!");
        }
        projectile.Kill();
        return;
      }
      // if (dist < 80) { // Go to player, TODO: set to magnet strength when it exists

      // }
      else if (projectile.velocity != Vector2.Zero) { // Normal bounce
        projectile.velocity.Y += (Gravity / 60);
      }
    }
    public override bool OnTileCollide(Vector2 oldVelocity) {
      projectile.velocity = oldVelocity * Bounce;
      projectile.velocity.Y = -projectile.velocity.Y;
      if (Math.Abs(projectile.velocity.Y) < 1f) {
        projectile.velocity = Vector2.Zero;
        projectile.position.Y -= 0.5f;
      }
      return false;
    }
  }
}