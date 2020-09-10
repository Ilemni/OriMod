using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  /// <summary>
  /// Summoning item used to summon <see cref="Projectiles.Minions.Sein"/>.
  /// </summary>
  public abstract class SpiritOrb : ModItem {
    public override string Texture => "OriMod/Items/SpiritOrb";

    /// <summary>
    /// Type for <see cref="Buffs.SeinBuff"/>. This value should be from <see cref="ModContent.BuffType{T}"/>.
    /// </summary>
    /// <returns>The type of the <see cref="ModBuff"/>.</returns>
    protected abstract int GetBuffType();

    /// <summary>
    /// Type for <see cref="Projectiles.Minions.Sein"/>. This value should be from <see cref="ModContent.ProjectileType{T}"/>.
    /// </summary>
    /// <returns>The type of the <see cref="Projectiles.Minions.Sein"/>.</returns>
    protected abstract int GetShootType();

    /// <summary>
    /// Type used for <see cref="Projectiles.Minions.Sein"/>. Values are indices to <see cref="SeinData.All"/>.
    /// </summary>
    protected abstract int SeinType { get; }

    public override void SetDefaults() {
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

      var data = SeinData.All[SeinType - 1];
      item.damage = data.damage;
      item.rare = data.rarity;
      item.value = data.value;
      item.color = data.color;
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
