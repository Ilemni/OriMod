using AnimLib;
using Microsoft.Xna.Framework;
using AnimLib.States;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities;

/// <summary>
/// Base class for items that unlocks or upgrades an <see cref="AbilityState"/>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class AbilityMedallionBase<T>(int level) : ModItem where T : AbilityState, new() {
  /// <summary>
  /// Level that the <see cref="AbilityState"/> of type <see cref="T"/> will be set to when this item is used.
  /// </summary>
  private int Level { get; } = level;

  public override void SetDefaults() {
    Item.useStyle = ItemUseStyleID.HoldUp;
    Item.useTime = 45;
    Item.useAnimation = 45;
  }

  /// <summary>
  /// Increases the level of <paramref name="player"/>'s <see cref="AbilityState"/> this Item represents to by 1.
  /// <para>By increasing by 1, the player can level it multiple times if they skip one, rather than having their level skip.</para>
  /// </summary>
  /// <param name="player">The player using the item.</param>
  /// <returns><see langword="true"/> if the ability can be leveled. If this returns <see langword="false"/>, this <see cref="AbilityMedallionBase{T}"/> or the <see cref="AbilityState"/> must be fixed.</returns>
  public override bool? UseItem(Player player) {
    AbilityState ability = player.GetState<T>();
    ability.Level = ability.Level < Level ? ability.Level + 1 : 0;
    if (player.whoAmI == Main.myPlayer) {
      string key = $"Mods.OriMod.Lore.{ability.GetType().Name}.{ability.Level}";
      if (Language.Exists(key)) {
        Main.NewText(Language.GetText(key), Color.LightCyan);
      }
    }

    if (Main.dedServ) {
      return true;
    }

    string str = (player.whoAmI == Main.myPlayer ? "You" : $"{player.name} has ") +
      ability.Level switch {
        1 => $" unlocked {ability.Name}!",
        0 => $" forgot {ability.Name}!",
        _ => $" upgraded {ability.Name} to Level {ability.Level}!"
      };
    Main.NewText(str, Color.LightGreen);

    return true;
  }

  public abstract override void AddRecipes();

  /// <summary>
  /// Gets a <see cref="Recipe"/> that uses the ingredient <see cref="AbilityMedallionEmpty"/>, crafting station <see cref="Tiles.SpiritSapling"/>, and sets the result.
  /// <para>These are standard recipes to all <see cref="AbilityMedallionBase{T}"/> types.</para>
  /// </summary>
  /// <returns>A <see cref="Recipe"/> set with ingredients and tiles common across all <see cref="AbilityMedallionBase{T}"/> items.</returns>
  protected Recipe GetAbilityRecipe() => GetAbilityRecipe<AbilityMedallionEmpty>();

  /// <summary>
  /// Gets a <see cref="Recipe"/> that uses the ingredient <typeparamref name="TItem"/>, crafting station <see cref="Tiles.SpiritSapling"/>, and sets the result.
  /// <para>This is intended for leveled Medallions, where <typeparamref name="TItem"/> is the previous level's Medallion.</para>
  /// </summary>
  /// <returns>A <see cref="Recipe"/> set with ingredients and tiles common across all <see cref="AbilityMedallionBase{T}"/> items.</returns>
  ///
  protected Recipe GetAbilityRecipe<TItem>() where TItem : ModItem =>
    CreateRecipe()
      .AddIngredient(ModContent.ItemType<TItem>())
      .AddTile(ModContent.TileType<Tiles.SpiritSapling>());
}
