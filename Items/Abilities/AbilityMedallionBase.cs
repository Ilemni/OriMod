//using AnimLib.Abilities;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod.Items.Abilities {
  /// <summary>
  /// Base class for items that unlocks or upgrades an <see cref="Ability"/>.
  /// </summary>
  public abstract class AbilityMedallionBase : ModItem {
    /// <summary>
    /// <see cref="AbilityId"/> of the <see cref="Ability"/> to unlock.
    /// </summary>
    public abstract byte Id { get; }

    /// <summary>
    /// Level that the <see cref="Ability"/> with <see cref="Id"/> will be set to when this item is used.
    /// </summary>
    public virtual byte Level => 1;

    public override void SetDefaults() {
      Item.useStyle = ItemUseStyleID.HoldUp;
      Item.useTime = 45;
      Item.useAnimation = 45;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the player does not have the <see cref="Ability"/> this Item represents upgraded to the <see cref="Ability.Level"/> this Item upgrades to.
    /// </summary>
    /// <param name="player">The <see cref="Player"/> using the item.</param>
    /// <returns><see langword="true"/> if the player does not have the <see cref="Ability"/> at this <see cref="Level"/>; otherwise, <see langword="false"/>.</returns>
    public override bool CanUseItem(Player player) {
      // Can only use the item if the ability to be unlocked has not been unlocked
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      return oPlayer.abilities[Id].Level < Level;
    }

    /// <summary>
    /// Increases the level of <paramref name="player"/>'s <see cref="Ability"/> this Item represents to by 1.
    /// <para>By increasing by 1, the player can level it multiple times if they skip one, rather than having their level skip.</para>
    /// </summary>
    /// <param name="player">The player using the item.</param>
    /// <returns><see langword="true"/> if the ability can be leveled. If this returns <see langword="false"/>, this <see cref="AbilityMedallionBase"/> or the <see cref="Ability"/> must be fixed.</returns>
    public override bool? UseItem(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      Ability ability = oPlayer.abilities[Id];
      if (ability is ILevelable levelable) {
        levelable.Level++;
        if (player.whoAmI == Main.myPlayer) {
          string key = $"Mods.OriMod.Lore.{ability.GetType().Name}.{levelable.Level}";
          if (Language.Exists(key)) {
            Main.NewText(Language.GetText(key), Color.LightCyan);
          }
        }
        string strStart = player.whoAmI == Main.myPlayer ? "You" : $"{player.name} has";
        Main.NewText(
          levelable.Level == 1
            ? $"{strStart} unlocked {NiceName(ability)}!"
            : $"{strStart} upgraded {NiceName(ability)} to Level {levelable.Level}!", Color.LightGreen);
        return true;
      }

      Main.NewText($"OriMod dev bug: Ability {ability.GetType().Name} cannot be leveled.", Color.Red);
      return false;
    }

    private static string NiceName(Ability ability) => Regex.Replace(ability.GetType().Name, "(\\B[A-Z])", " $1");

    public abstract override void AddRecipes();

    /// <summary>
    /// Gets a <see cref="ModRecipe"/> that uses the ingredient <see cref="AbilityMedallionEmpty"/>, crafting station <see cref="Tiles.SpiritSapling"/>, and sets the result.
    /// <para>These are standard recipes to all <see cref="AbilityMedallionBase"/> types.</para>
    /// </summary>
    /// <returns>A <see cref="ModRecipe"/> set with ingredients and tiles common across all <see cref="AbilityMedallionBase"/> items.</returns>
    protected Recipe GetAbilityRecipe() => GetAbilityRecipe<AbilityMedallionEmpty>();

    /// <summary>
    /// Gets a <see cref="ModRecipe"/> that uses the ingredient <typeparamref name="T"/>, crafting station <see cref="Tiles.SpiritSapling"/>, and sets the result.
    /// <para>This is intended for leveled Medallions, where <typeparamref name="T"/> is the previous level's Medallion.</para>
    /// </summary>
    /// <returns>A <see cref="ModRecipe"/> set with ingredients and tiles common across all <see cref="AbilityMedallionBase"/> items.</returns>
    /// 

    protected Recipe GetAbilityRecipe<T>() where T : ModItem => 
        CreateRecipe()
        .AddIngredient(ModContent.ItemType<T>())
        .AddTile(ModContent.TileType<Tiles.SpiritSapling>());
  }
}
