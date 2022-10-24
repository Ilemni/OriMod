using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
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

    protected Recipe GetRecipe<T>() where T : ModItem =>
      GetRecipe()
        .AddIngredient(ModContent.ItemType<T>());

    protected Recipe GetRecipe() =>
      CreateRecipe()
        .AddTile(ModContent.TileType<Tiles.SpiritSapling>());

    public override void SetDefaults() {
      Item.buffType = GetBuffType();
      Item.shoot = GetShootType();
      Item.DamageType = DamageClass.Summon;
      Item.mana = 10;
      Item.width = 18;
      Item.height = 18;
      Item.useTime = 21;
      Item.useAnimation = 21;
      Item.useStyle = ItemUseStyleID.Swing;
      Item.noMelee = true;
      Item.UseSound = SoundID.Item44;

      SeinData data = SeinData.All[SeinType - 1];
      Item.damage = data.damage;
      Item.rare = data.rarity;
      Item.value = data.value;
      Item.color = data.color;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      return player.altFunctionUse != 2 && (!oPlayer.SeinMinionActive || oPlayer.SeinMinionType != Item.shoot);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      oPlayer.RemoveSeinBuffs();
      player.AddBuff(Item.buffType, 2);
      oPlayer.SeinMinionType = Item.shoot;
      oPlayer.SeinMinionActive = true;
      if (player.altFunctionUse == 2) {
        player.MinionNPCTargetAim(true);
      }
      oPlayer.SeinMinionId = Projectile.NewProjectile(source, position, -Vector2.UnitY, type, damage, knockBack, player.whoAmI);
      return false;
    }
  }
}
