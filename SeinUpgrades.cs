using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections.Generic;
using Terraria;

namespace OriMod {
  public class SeinUpgrade {
    /// <summary>
    /// Damage of Spirit Flame
    /// </summary>
    public int damage = 8;
    /// <summary>
    /// Multiplier of damage dealt to the primary target
    /// </summary>
    public float primaryDamageMultiplier = 1;
    /// <summary>
    /// Number of NPCs that can be targeted at once
    /// </summary>
    public int targets = 1;
    /// <summary>
    /// Maximum times the minion can fire with a delay of <c>minCooldown</c> before having a delay of <c>longCooldown</c>
    /// 
    /// </summary>
    /// <seealso cref="minCooldown" />
    /// <seealso cref="shortCooldown" />
    /// <seealso cref="longCooldown" />
    public int shotsPerBurst = 1;
    /// <summary>
    /// Maximum number of shots that can be fired at each target
    /// 
    /// <seealso cref="shotsToPrimaryTarget" />
    /// </summary>
    public int shotsPerTarget = 1;
    /// <summary>
    /// Maximum number of shots that can be fired at the primary target at once
    /// </summary>
    public int shotsToPrimaryTarget = 1;
    /// <summary>
    /// Maximum number of shots that can be fired at once
    /// </summary>
    public int maxShotsPerVolley = 1;
    /// <summary>
    /// Delay between each shot in <c>shotsPerBurst</c>
    /// </summary>
    public float minCooldown = 12f;
    /// <summary>
    /// Shortest time to wait during <c>shotsPerBurst</c> to reset burst count
    /// </summary>
    public float shortCooldown = 18f;
    /// <summary>
    /// Delay between each series of shots, incurred when shots reaches <c>shotsPerBurst</c>
    /// </summary>
    public float longCooldown = 60f;
    /// <summary>
    /// Determines if Spirit Flame shots collide with or ignore tiles
    /// </summary>
    public bool tileCollide = true;
    /// <summary>
    /// Maximum angle that fired Spirit Flames will be away from the target
    /// </summary>
    internal int randDegrees = 40;
    /// <summary>
    /// NPCs this distance from the player can be targeted by the minion, if there is line of sight between it and the player
    /// 
    /// <seealso cref="targetThroughWallDist" />
    /// </summary>
    public float targetMaxDist = 240f;
    /// <summary>
    /// NPCs this distance from the player can be targeted by the minion, regardless of line of sight
    /// </summary>
    public float targetThroughWallDist = 0f;
    /// <summary>
    /// The amount of times Spirit Flame can hit enemies before disappearing
    /// </summary>
    public int pierce = 1;
    /// <summary>
    /// The knockback of Spirit Flame
    /// </summary>
    public float knockback = 0f;
    /// <summary>
    /// Default homing strength of Spirit Flame
    /// </summary>
    internal float homingStrengthStart = 0.07f;
    /// <summary>
    /// Rate to increase homing strength every frame after <c>homingIncreaseDelay</c>
    /// </summary>
    internal float homingIncreaseRate = 0.03f;
    /// <summary>
    /// Time to wait before increasing homing strength by <c>homingIncreaseRate</c>
    /// </summary>
    internal float homingIncreaseDelay = 20f;
    /// <summary>
    /// Speed of Spirit Flame when it is fired
    /// </summary>
    internal float projectileSpeedStart = 5f;
    /// <summary>
    /// Acceleration of Spirit Flame after waiting for <c>projectileSpeedIncreaseDelay</c>
    /// </summary>
    internal float projectileSpeedIncreaseRate = 0.5f;
    /// <summary>
    /// Time to wait before increasing Spirit Flame speed by <c>projectileSpeedIncreaseRate</c>
    /// </summary>
    internal int projectileSpeedIncreaseDelay = 20;
    internal int minionWidth = 10;
    internal int minionHeight = 11;
    internal int flameWidth = 12;
    internal int flameHeight = 12;
    /// <summary>
    /// The size of the dust trail emitted from Spirit Flame
    /// </summary>
    public float dustScale = 0.8f;
    /// <summary>
    /// Rarity of the Spirit Orb
    /// </summary>
    internal int rarity = 1;
    /// <summary>
    /// Buy value of the Spirit Orb
    /// </summary>
    internal int value = 1000;
    /// <summary>
    /// Color of the Spirit Orb, Sein, Spirit Flame, and emitted lights
    /// </summary>
    internal Color color;
    /// <summary>
    /// Strength of the light emitted from Sein and Spirit Flame
    /// </summary>
    internal float lightStrength;
  }
  partial class OriMod {
    internal static List<SeinUpgrade> SeinUpgrades;
    private void AddNewSein(SeinUpgrade newSein) {
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
      SeinUpgrades = new List<SeinUpgrade>();
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