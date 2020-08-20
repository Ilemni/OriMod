using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// Summoning item used to summon <see cref="Projectiles.Minions.Sein"/>
  /// </summary>
  public abstract class SpiritOrb : ModItem {
    public override string Texture => "OriMod/Items/SpiritOrb";

    #region Stats
    /// <summary>
    /// Damage of Spirit Flame
    /// </summary>
    public int damage = 12;

    /// <summary>
    /// Multiplier of damage dealt to the primary target
    /// </summary>
    public float primaryDamageMultiplier = 1;

    /// <summary>
    /// Number of NPCs that can be targeted at once
    /// </summary>
    public int targets = 1;

    /// <summary>
    /// Maximum times the minion can fire with a delay of `minCooldown` before having a delay of `longCooldown`
    /// </summary>
    public int shotsPerBurst = 2;

    /// <summary>
    /// Maximum number of shots that can be fired at each target
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
    /// Delay between each shot in `shotsPerBurst`
    /// </summary>
    public float minCooldown = 12f;

    /// <summary>
    /// Shortest time to wait during <c>shotsPerBurst</c> to reset burst count
    /// </summary>
    public float shortCooldown = 24f;

    /// <summary>
    /// Delay between each series of shots, incurred when shots reaches `shotsPerBurst` </summary>
    public float longCooldown = 40f;

    /// <summary>
    /// Maximum angle that fired Spirit Flames will be away from the target
    /// </summary>
    internal int randDegrees = 40;

    /// <summary>
    /// NPCs this distance from the player can be targeted by the minion, if there is line of sight between it and the player
    /// </summary>
    public float targetMaxDist = 240f;

    /// <summary>
    /// NPCs this distance from the player can be targeted by the minion, regardless of line of sight
    /// </summary>
    public float targetThroughWallDist = 80f;

    /// <summary>
    /// The amount of times Spirit Flame can hit enemies before disappearing
    /// </summary>
    public int pierce = 1;

    /// <summary>
    /// The knockback of Spirit Flame
    /// </summary>
    public float knockback = 0f;

    /// <summary>
    /// Starting homing strength of Spirit Flame
    /// </summary>
    internal float homingStrengthStart = 0.07f;

    /// <summary>
    /// Rate to increase homing strength every frame after `homingIncreaseDelay`
    /// </summary>
    internal float homingIncreaseRate = 0.04f;

    /// <summary>
    /// Ticks to wait before increasing homing strength by `homingIncreaseRate`
    /// </summary>
    internal int homingIncreaseDelay = 16;

    /// <summary>
    /// Speed of Spirit Flame when it is fired
    /// </summary>
    internal float projectileSpeedStart = 5f;

    /// <summary>
    /// Acceleration of Spirit Flame after waiting for `projectileSpeedIncreaseDelay`
    /// </summary>
    internal float projectileSpeedIncreaseRate = 0.5f;

    /// <summary>
    /// Time to wait before increasing Spirit Flame speed by `projectileSpeedIncreaseRate`
    /// </summary>
    internal int projectileSpeedIncreaseDelay = 10;

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
    #endregion

    internal abstract SpiritOrb SetOrbDefaults();

    protected abstract int GetBuffType();
    protected abstract int GetShootType();

    internal void CopyFrom(SpiritOrb old) {
      damage = old.damage;
      primaryDamageMultiplier = old.primaryDamageMultiplier;
      targets = old.targets;
      shotsPerBurst = old.shotsPerBurst;
      shotsPerTarget = old.shotsPerTarget;
      shotsToPrimaryTarget = old.shotsToPrimaryTarget;
      maxShotsPerVolley = old.maxShotsPerVolley;
      minCooldown = old.minCooldown;
      shortCooldown = old.shortCooldown;
      longCooldown = old.longCooldown;
      randDegrees = old.randDegrees;
      targetMaxDist = old.targetMaxDist;
      targetThroughWallDist = old.targetThroughWallDist;
      pierce = old.pierce;
      knockback = old.knockback;
      homingStrengthStart = old.homingStrengthStart;
      homingIncreaseRate = old.homingIncreaseRate;
      homingIncreaseDelay = old.homingIncreaseDelay;
      projectileSpeedStart = old.projectileSpeedStart;
      projectileSpeedIncreaseRate = old.projectileSpeedIncreaseRate;
      projectileSpeedIncreaseDelay = old.projectileSpeedIncreaseDelay;
      minionWidth = old.minionWidth;
      minionHeight = old.minionHeight;
      flameWidth = old.flameWidth;
      flameHeight = old.flameHeight;
      dustScale = old.dustScale;
      rarity = old.rarity;
      value = old.value;
      color = old.color;
      lightStrength = old.lightStrength;
    }

    public override void SetDefaults() {
      SetOrbDefaults();
      item.buffType = GetBuffType();
      item.shoot = GetShootType();
      item.summon = true;
      item.mana = 10;
      item.width = 18;
      item.height = 18;
      item.useTime = 21;
      item.useAnimation = 21;
      item.useStyle = ItemUseStyleID.SwingThrow;
      item.noMelee = true;
      item.UseSound = SoundID.Item44;
      item.damage = damage;
      item.rare = rarity;
      item.value = value;
      item.color = color;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      if (player.altFunctionUse == 2 || oPlayer.SeinMinionActive && oPlayer.SeinMinionType == item.shoot) {
        return false;
      }
      return true;
    }

    public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.RemoveSeinBuffs();
      Main.NewText($"Shoot: Item.buffType: {item.buffType}");
      player.AddBuff(item.buffType, 2, true);
      oPlayer.SeinMinionType = item.shoot;
      oPlayer.SeinMinionActive = true;
      if (player.altFunctionUse == 2) {
        player.MinionNPCTargetAim();
      }
      oPlayer.SeinMinionID = Projectile.NewProjectile(player.position, Vector2.Zero, item.shoot, item.damage, item.knockBack, player.whoAmI, 0, 0);
      return false;
    }
  }
}
