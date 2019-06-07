using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Items {
  public class SpiritOrbBase : ModItem {
    public int upgrade;
    public override void SetStaticDefaults() { }
    public override void SetDefaults() {
      item.summon = true;
      item.mana = 10;
      item.width = 18;
      item.height = 18;
      item.useTime = 21;
      item.useAnimation = 21;
      item.useStyle = 1;
      item.noMelee = true;
      item.UseSound = SoundID.Item44;
      item.buffTime = 3600;
    }
    internal void Init(int upgradeID) {
      SeinUpgrade u = OriMod.SeinUpgrades[upgradeID - 1];
      item.damage = u.damage;
      item.rare = u.rarity;
      item.value = u.value;
      item.shoot = mod.ProjectileType("Sein" + upgradeID);
      item.buffType = mod.BuffType("SeinBuff" + upgradeID);
      item.color = u.color;
      upgrade = upgradeID;
    }
    public override bool AltFunctionUse(Player player) => true;
    public int GetUpgrade() {
      string name = this.GetType().ToString();
      string sub = name.Substring(22);
      int u;
      bool success = int.TryParse(sub, out u);
      if (!success) {
        Main.NewText("Error with Sein summon: Couldn't get number from " + sub, Color.Red);
        return 0;
      }
      return u;
    }
    public override bool CanUseItem(Player player) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>(mod);
      if (player.altFunctionUse == 2) {
        return false;
      }
      if (oPlayer.SeinMinionActive && oPlayer.SeinMinionUpgrade == GetUpgrade()) return false;
      return true;
    }
    public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      int u = GetUpgrade();
      oPlayer.RemoveSeinBuffs(exclude:u);
      oPlayer.SeinMinionUpgrade = u;
      oPlayer.SeinMinionActive = true;
      if(player.altFunctionUse == 2) {
        player.MinionNPCTargetAim();
      }
      return true;
    }
  }
}
