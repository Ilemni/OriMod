using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items;

/// <summary>
/// Summoning item used to summon <see cref="Projectiles.Minions.Sein"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class SpiritOrb(int type) : ModItem {
  public override string Texture => "OriMod/Items/SpiritOrb";

  /// <summary>
  /// Type used for <see cref="Projectiles.Minions.Sein"/>.
  /// Values are indices for <see cref="SeinLoader"/>.
  /// </summary>
  private int SeinType { get; } = type;

  protected Recipe GetRecipe<T>() where T : ModItem =>
    GetRecipe()
      .AddIngredient(ModContent.ItemType<T>());

  protected Recipe GetRecipe() =>
    CreateRecipe()
      .AddTile(ModContent.TileType<Tiles.SpiritSapling>());

  public override void SetDefaults() {
    Item.buffType = SeinLoader.BuffType(SeinType);
    Item.shoot = SeinLoader.MinionType(SeinType);
    Item.DamageType = DamageClass.Summon;
    Item.mana = 10;
    Item.width = 18;
    Item.height = 18;
    Item.useTime = 21;
    Item.useAnimation = 21;
    Item.useStyle = ItemUseStyleID.Swing;
    Item.noMelee = true;
    Item.UseSound = SoundID.Item44;

    ref readonly SeinData data = ref SeinLoader.Get(SeinType);
    Item.damage = data.Damage;
    Item.rare = data.Rarity;
    Item.value = data.Value;
    Item.color = data.Color;
  }

  public override bool AltFunctionUse(Player player) => true;

  public override bool CanUseItem(Player player) {
    return player.altFunctionUse != 2 && !player.HasBuff(Item.buffType);
  }

  public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
    int type, int damage, float knockBack) {
    foreach (int buffType in SeinLoader.BuffTypes) {
      player.ClearBuff(buffType);
    }

    player.AddBuff(Item.buffType, 2);

    if (player.altFunctionUse == 2) {
      player.MinionNPCTargetAim(true);
    }

    Projectile.NewProjectile(source, position, -Vector2.UnitY, type, damage, knockBack, player.whoAmI);
    return false;
  }
}
