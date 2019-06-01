using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections.Generic;
using Terraria;

namespace OriMod {
  public class SeinUpgrade {
    public int damage = 8;
    public float primaryDamageMultiplier = 1;
    public int targets = 1;
    public int shotsPerBurst = 1;
    public int shotsPerTarget = 1;
    public int shotsToPrimaryTarget = 1;
    public int maxShotsPerVolley = 1;
    public float minCooldown = 12f;
    public float shortCooldown = 18f;
    public float longCooldown = 60f;
    public bool tileCollide = true;
    public int randDegrees = 40;
    public float targetMaxDist = 240f;
    public float targetThroughWallDist = 0f;

    public int pierce = 1;
    public float knockback = 0f;
    public float homingStrengthStart = 0.07f;
    public float homingIncreaseRate = 0.03f;
    public float homingIncreaseDelay = 20f;
    public float projectileSpeedStart = 5f;
    public float projectileSpeedIncreaseRate = 0.5f;
    public int projectileSpeedIncreaseDelay = 20;
    public int minionWidth = 10;
    public int minionHeight = 11;
    public int flameWidth = 12;
    public int flameHeight = 12;
    public float dustScale = 0.8f;
    public int rarity = 1;
    public int value = 1000;
    public Color color;
    public float lightStrength;
  }
  partial class OriMod
  {
    internal static List<SeinUpgrade> SeinUpgrades = new List<SeinUpgrade>();
    internal void AddNewSein(SeinUpgrade newSein) {
      SeinUpgrade defaultSein = new SeinUpgrade();
      SeinUpgrade lastSein = new SeinUpgrade();
      if (SeinUpgrades.Count != 0) {
        lastSein = SeinUpgrades[SeinUpgrades.Count - 1];
      }
      foreach(FieldInfo field in ((newSein.GetType()).GetFields())) {
        var defVal = field.GetValue(defaultSein);
        var oldVal = field.GetValue(lastSein);
        var newVal = field.GetValue(newSein);
        // If value is unspecified, use the one from the previous Sein upgrade
        if (newVal.ToString() == defVal.ToString()) {
          newVal = oldVal;
        }
        field.SetValue(newSein, newVal);
      }
      SeinUpgrades.Add(newSein);
    }
    private void LoadSeinUpgrades() {
      AddNewSein(new SeinUpgrade()); // Tier 1 (Silver)
      AddNewSein(new SeinUpgrade{    // Tier 2 (Demonite/Crimsane)
        rarity = 2,
        value = 3000,
        color = new Color(108, 92, 172),
        damage = 17,
        shotsPerBurst = 2,
        projectileSpeedStart = 7f,
        homingIncreaseRate = 0.045f,
        dustScale = 1.3f,
        lightStrength = 1.6f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 3 (Hellstone)
        rarity = 3,
        value = 10000,
        color = new Color(240, 0, 0, 194),
        damage = 28,
        targets = 2,
        shotsPerBurst = 3, 
        tileCollide = false, 
        randDegrees = 100,
        projectileSpeedStart = 10.5f, 
        projectileSpeedIncreaseRate = 0.65f, 
        projectileSpeedIncreaseDelay = 19, 
        targetMaxDist = 370f,
        dustScale = 1.55f,
        lightStrength = 1.275f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 4 (Mythral/Orichalcum)
        rarity = 4,
        value = 25000,
        color = new Color(185, 248, 248),
        damage = 39,
        shotsToPrimaryTarget = 2,
        maxShotsPerVolley = 2,
        randDegrees = 60,
        projectileSpeedStart = 12.5f, 
        homingIncreaseRate = 0.05f, 
        homingIncreaseDelay = 19.5f,
        dustScale = 1.8f,
        lightStrength = 1.2f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 5 (Hallow)
        rarity = 5,
        value = 50000,
        color = new Color(255, 228, 160),
        damage = 52, 
        shotsToPrimaryTarget = 2,
        pierce = 2, 
        homingIncreaseDelay = 17f,
        targetMaxDist = 440f,
        dustScale = 2.2f,
        lightStrength = 1.4f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 6 (Spectral)
        rarity = 8,
        value = 100000,
        color = new Color(0, 180, 174, 210),
        damage = 68, 
        targets = 3,
        minCooldown = 10f, 
        shortCooldown = 14f, 
        longCooldown = 52f, 
        targetThroughWallDist = 224f, 
        homingIncreaseRate = 0.0625f,
        projectileSpeedStart = 14.5f, 
        projectileSpeedIncreaseRate = 0.825f, 
        projectileSpeedIncreaseDelay = 17, 
        randDegrees = 70,
        dustScale = 2.6f,
        lightStrength = 2.25f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 7 (Lunar)
        rarity = 9,
        value = 250000,
        color = new Color(78, 38, 102),
        damage = 84,
        targets = 4,
        shotsToPrimaryTarget = 3,
        shotsPerTarget = 2,
        maxShotsPerVolley = 8,
        homingIncreaseRate = 0.025f, 
        projectileSpeedStart = 16f, 
        targetMaxDist = 670f,
        randDegrees = 120,
        dustScale = 3f,
        lightStrength = 4.5f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 8 (Lunar Bars)
        rarity = 10,
        value = 500000,
        color = new Color(220, 220, 220),
        damage = 114, 
        pierce = 3,
        shotsPerBurst = 6, 
        targets = 6, 
        shotsToPrimaryTarget = 4,
        shotsPerTarget = 2,
        maxShotsPerVolley = 8,
        minCooldown = 8f, 
        shortCooldown = 18f, 
        longCooldown = 45f, 
        homingStrengthStart = 0.05f, 
        homingIncreaseDelay = 15f, 
        projectileSpeedStart = 20f, 
        projectileSpeedIncreaseRate = 1f, 
        projectileSpeedIncreaseDelay = 35, 
        randDegrees = 180, 
        targetMaxDist = 800f, 
        targetThroughWallDist = 370f,
        dustScale = 3.35f,
        lightStrength = 2.5f,
      });
    }
  }
}