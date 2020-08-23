using Microsoft.Xna.Framework;
using System.Reflection;
using System.Collections.Generic;

namespace OriMod {
  public class SeinUpgrade {
    /// <summary>
    /// Damage of Spirit Flame.
    /// </summary>
    public int damage = 12;

    /// <summary>
    /// Number of NPCs that can be targeted at once.
    /// </summary>
    public int targets = 1;

    /// <summary>
    /// Maximum times the minion can fire with a delay of <see cref="minCooldown"/> before having a delay of <see cref="longCooldown"/>.
    /// </summary>
    public int shotsPerBurst = 2;

    /// <summary>
    /// Maximum number of shots that can be fired at each target.
    /// </summary>
    public int shotsPerTarget = 1;

    /// <summary>
    /// Maximum number of shots that can be fired at the primary target at once.
    /// </summary>
    public int shotsToPrimaryTarget = 1;

    /// <summary>
    /// Maximum number of shots that can be fired at once.
    /// </summary>
    public int maxShotsPerVolley = 1;

    /// <summary>
    /// Delay between each shot in <see cref="shotsPerBurst"/>.
    /// /summary>
    public float minCooldown = 12f;

    /// <summary>
    /// Shortest time to wait during <see cref="shotsPerBurst"/> to reset burst count.
    /// </summary>
    public float shortCooldown = 24f;

    /// <summary>
    /// Delay between each series of shots, incurred when shots reaches <see cref="shotsPerBurst"/>.
    /// </summary>
    public float longCooldown = 40f;

    /// <summary>
    /// Maximum angle that fired Spirit Flames will be away from the target.
    /// </summary>
    internal int randDegrees = 40;

    /// <summary>
    /// NPCs within this distance from the player can be targeted by the minion, if there is line of sight between it and the player.
    /// </summary>
    public float targetMaxDist = 240f;

    /// <summary>
    /// NPCs within this distance from the player can be targeted by the minion, regardless of line of sight.
    /// </summary>
    public float targetThroughWallDist = 80f;

    /// <summary>
    /// The knockback of Spirit Flame.
    /// </summary>
    public float knockback = 0f;

    /// <summary>
    /// Starting homing strength of Spirit Flame.
    /// </summary>
    internal float homingStrengthStart = 0.07f;

    /// <summary>
    /// Rate to increase homing strength every frame after <see cref="homingIncreaseDelay"/>.
    /// </summary>
    internal float homingIncreaseRate = 0.04f;

    /// <summary>
    /// Ticks to wait before increasing homing strength by <see cref="homingIncreaseRate"/>.
    /// </summary>
    internal int homingIncreaseDelay = 16;

    /// <summary>
    /// Speed of Spirit Flame when it is fired.
    /// </summary>
    internal float projectileSpeedStart = 5f;

    /// <summary>
    /// Acceleration of Spirit Flame after waiting for <see cref="projectileSpeedIncreaseDelay"/>.
    /// </summary>
    internal float projectileSpeedIncreaseRate = 0.5f;

    /// <summary>
    /// Time to wait before increasing Spirit Flame speed by <see cref="projectileSpeedIncreaseRate"/>.
    /// </summary>
    internal int projectileSpeedIncreaseDelay = 10;

    internal int minionWidth = 10;
    internal int minionHeight = 11;
    internal int flameWidth = 12;
    internal int flameHeight = 12;

    /// <summary>
    /// The size of the dust trail emitted from Spirit Flame.
    /// </summary>
    public float dustScale = 0.8f;

    /// <summary>
    /// Rarity of the Spirit Orb.
    /// </summary>
    internal int rarity = 1;

    /// <summary>
    /// Buy value of the Spirit Orb.
    /// </summary>
    internal int value = 1000;

    /// <summary>
    /// Color of the Spirit Orb, Sein, Spirit Flame, and emitted lights.
    /// /summary>
    internal Color color;

    /// <summary>
    /// Strength of the light emitted from Sein and Spirit Flame.
    /// </summary>
    internal float lightStrength;
  }

  public partial class OriMod {
    /// <summary>
    /// All <see cref="SeinUpgrade"/>s in the mod.
    /// </summary>
    internal readonly List<SeinUpgrade> SeinUpgrades = new List<SeinUpgrade>();

    /// <summary>
    /// Loads all Sein variants. Sein stats are hardcoded into this method.
    /// <para>The only benefit to doing it this way over overrides is that the previous Sein level is automatically used when values are not provided.</para>
    /// </summary>
    private void LoadSeinUpgrades() {
      var defaultSein = new SeinUpgrade();
      var fields = typeof(SeinUpgrade).GetFields();

      void AddNewSein(SeinUpgrade newSein) {
        var lastSein = SeinUpgrades.Count == 0 ?
          new SeinUpgrade() :
          SeinUpgrades[SeinUpgrades.Count - 1];

        foreach (FieldInfo field in fields) {
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

      SeinUpgrades.Clear();

      // Tier 1 (Silver)
      AddNewSein(new SeinUpgrade());

      // Tier 2 (Demonite/Crimsane)
      // Increased shots per burst
      // Max damage per burst: 15
      AddNewSein(new SeinUpgrade {
        rarity = 2,
        value = 3000,
        color = new Color(108, 92, 172),
        damage = 15,
        shotsPerBurst = 3,
        projectileSpeedStart = 7f,
        homingIncreaseRate = 0.045f,
        dustScale = 1.3f,
        lightStrength = 1.6f,
      });

      // Tier 3 (Hellstone)
      // 2 targets
      // Max damage per burst: 42
      AddNewSein(new SeinUpgrade {
        rarity = 3,
        value = 10000,
        color = new Color(240, 0, 0, 194),
        damage = 21,
        targets = 2,
        maxShotsPerVolley = 2,
        randDegrees = 100,
        projectileSpeedStart = 10.5f,
        projectileSpeedIncreaseRate = 0.65f,
        projectileSpeedIncreaseDelay = 19,
        targetMaxDist = 370f,
        dustScale = 1.55f,
        lightStrength = 1.275f,
      });

      // Tier 4 (Mythral/Orichalcum)
      // 2 targets, 2 shots to primary, 3 shots max (rather than 4)
      // Max damage per burst: 81
      AddNewSein(new SeinUpgrade {
        rarity = 4,
        value = 25000,
        color = new Color(185, 248, 248),
        damage = 27,
        shotsToPrimaryTarget = 2,
        maxShotsPerVolley = 3,
        randDegrees = 60,
        projectileSpeedStart = 12.5f,
        homingIncreaseRate = 0.05f,
        homingIncreaseDelay = 20,
        dustScale = 1.8f,
        lightStrength = 1.2f,
      });

      // Tier 5 (Hallow)
      // 3 targets, 2 shots to primary, 4 shots max (rather than 5)
      // Max damage per burst: 132
      AddNewSein(new SeinUpgrade {
        rarity = 5,
        value = 50000,
        color = new Color(255, 228, 160),
        damage = 33,
        targets = 3,
        maxShotsPerVolley = 4,
        homingIncreaseDelay = 17,
        targetMaxDist = 440f,
        dustScale = 2.2f,
        lightStrength = 1.4f,
      });

      // Tier 6 (Spectral)
      // 3 targets, 3 shots to primary, 5 shots max
      // Max damage per burst: 195
      AddNewSein(new SeinUpgrade {
        rarity = 8,
        value = 100000,
        color = new Color(0, 180, 174, 210),
        damage = 39,
        shotsToPrimaryTarget = 3,
        maxShotsPerVolley = 5,
        minCooldown = 10f,
        shortCooldown = 34f,
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

      // Tier 7 (Lunar)
      // 4 targets, 3 shots to primary, 2 to others, 6 shots max (rather than 9)
      // Max damage per burst: 282
      AddNewSein(new SeinUpgrade {
        rarity = 9,
        value = 250000,
        color = new Color(78, 38, 102),
        damage = 47,
        targets = 4,
        shotsToPrimaryTarget = 3,
        shotsPerTarget = 2,
        maxShotsPerVolley = 9,
        homingIncreaseRate = 0.025f,
        projectileSpeedStart = 16f,
        targetMaxDist = 510f,
        randDegrees = 120,
        dustScale = 3f,
        lightStrength = 4.5f,
      });

      // Tier 8 (Lunar Bars)
      // 5 targets, 4 shots to primary, 2 shots to others, 10 shots max (rather than 12)
      // Max damage per burst: 530 (too high?)
      AddNewSein(new SeinUpgrade {
        rarity = 10,
        value = 500000,
        color = new Color(220, 220, 220),
        damage = 53,
        shotsPerBurst = 4,
        targets = 6,
        shotsToPrimaryTarget = 4,
        maxShotsPerVolley = 10,
        longCooldown = 55f,
        homingStrengthStart = 0.05f,
        homingIncreaseDelay = 15,
        projectileSpeedStart = 20f,
        projectileSpeedIncreaseRate = 1f,
        projectileSpeedIncreaseDelay = 35,
        randDegrees = 180,
        targetMaxDist = 650f,
        targetThroughWallDist = 370f,
        dustScale = 3.35f,
        lightStrength = 2.5f,
      });
    }
  }
}
