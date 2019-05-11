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
			item.buffTime = 3600;				//The duration of the buff, here is 60 seconds
		}
		public void Init(int upgradeID) {
			SeinUpgrade u = OriMod.SeinUpgrades[upgradeID - 1];
			item.damage = u.damage;
			item.rare = u.rarity;
			item.value = u.value;
			item.shoot = mod.ProjectileType("Sein" + upgradeID);
			item.buffType = mod.BuffType("SeinBuff" + upgradeID);
			item.color = u.color;
			upgrade = upgradeID;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			if (player.altFunctionUse != 2) {
				OriPlayer modPlayer = player.GetModPlayer<OriPlayer>(mod);
				if (!modPlayer.seinMinionActive) {
					modPlayer.seinMinionActive = true;
					modPlayer.seinMinionUpgrade = upgrade;
				}
				else {
					// Identical Sein upgrade: do nothing
					if (modPlayer.seinMinionUpgrade == upgrade) {
						return false;
					}
					// Replace different Sein
					else {
						modPlayer.RemoveSeinBuffs(exclude:upgrade);
						modPlayer.seinMinionUpgrade = upgrade;
						return true;
					}
				}
			}
			return true;
		}
		public override bool UseItem(Player player) {
			OriPlayer modPlayer = player.GetModPlayer<OriPlayer>(mod);
			if (modPlayer.seinMinionActive) {
				return true;
			}
			if(player.altFunctionUse == 2) {
				player.MinionNPCTargetAim();
			}
			return base.UseItem(player);
		}
	}
}
