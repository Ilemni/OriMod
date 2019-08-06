// Dust no longer in use but may be brought back

/* using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Dusts {

  internal class SoulLinkDustCustomObject {
    internal float shrinkRate;
    internal float rotateRate;
    internal Vector2 velocity;
    internal int alphaDir = -1;
  }
  public class SoulLinkDust : ModDust {
    private static float velocityMin => 0.6f;
    private static float velocityMax => 1f;
    private static float rotateMin => -(float)Math.PI;
    private static float rotateMax => 0;
    private static float rotateRateMax => 0.02f;
    private static float sizeMin => 1.2f;
    private static float sizeMax => 1.7f;
    private static float shrinkRateMin => 0.008f;
    private static float shrinkRateMax => 0.01f;


    public override bool Autoload(ref string name, ref string texture) {
      texture = "OriMod/PlayerEffects/soulLinkForeground";
      return base.Autoload(ref name, ref texture);
    }
    public override void OnSpawn(Dust dust) {
      dust.alpha = 255;
      dust.noGravity = true;
      dust.velocity = Vector2.Zero;
      dust.rotation = Main.rand.NextFloat(rotateMin, rotateMax);
      dust.scale = Main.rand.NextFloat(sizeMin, sizeMax);
      SoulLinkDustCustomObject customData  = new SoulLinkDustCustomObject();
      customData.rotateRate = Main.rand.NextFloat(-rotateRateMax,rotateRateMax);
      customData.shrinkRate = Main.rand.NextFloat(shrinkRateMin, shrinkRateMax);
      customData.velocity = (-Vector2.UnitY * Main.rand.NextFloat(velocityMin, velocityMax)).RotateRandom(Math.PI / 3);
      dust.customData = customData;
    }
    public override bool Update(Dust dust) {
      if (dust.customData != null && dust.customData is SoulLinkDustCustomObject) {
        SoulLinkDustCustomObject customData = dust.customData as SoulLinkDustCustomObject;
        dust.rotation += customData.rotateRate;
        dust.scale -= customData.shrinkRate;
        dust.position += customData.velocity;
        customData.velocity.X *= 0.99f;
        customData.velocity.Y *= 0.995f;
        if (dust.scale < 0.01f) {
          dust.active = false;
        }

        if (customData.alphaDir == -1) {
          dust.alpha -= 10;
          if (dust.alpha < 0) {
            dust.alpha = 0;
            customData.alphaDir = 1;
          }
        }
        else {
          dust.alpha += 4;
          if (dust.alpha > 255) {
            dust.alpha = 255;
            dust.active = false;
          }
        }
      }
      return false;
    }
  }
} */