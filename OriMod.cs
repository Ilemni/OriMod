using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod
{
  public class SeinUpgrade {
    public int damage = 8;
    public int targets = 1;
    public int shotsPerBurst = 1;
    public float minCooldown = 12f;
    public float shortCooldown = 18f;
    public float longCooldown = 60f;
    public bool tileCollide = true;
    public int randDegrees = 40;
    public float targetMaxDist = 230f;
    public float targetThroughWallDist = 0f;

    public int pierce = 1;
    public float knockback = 0f;
    public float homingStrengthStart = 0.07f;
    public float homingIncreaseRate = 0.03f;
    public float homingIncreaseDelay = 20f;
    public float projectileSpeedStart = 5f;
    public float projectileSpeedIncreaseRate = 0.5f;
    public int projectileSpeedIncreaseDelay = 20;
    public int minionWidth = 10;
    public int minionHeight = 11;
    public int flameWidth = 12;
    public int flameHeight = 12;
    public float dustScale = 0.8f;
    public int rarity = 1;
    public int value = 1000;
    public Color color;
    public float lightStrength;
  }
	class OriMod : Mod
	{
    public static ModHotKey BashKey;
    public static ModHotKey DashKey;
    public static ModHotKey ClimbAndFeather;

    public static List<SeinUpgrade> SeinUpgrades = new List<SeinUpgrade>();

    public OriMod() {
			Properties = new ModProperties() {
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}
    public override void AddRecipeGroups() {
      // Creates a new recipe group
      RecipeGroup group1 = new RecipeGroup(() => "Any Enchanted Items", new int[] {
        ItemID.EnchantedSword,
        ItemID.EnchantedBoomerang,
        ItemID.Arkhalis
      });
      RecipeGroup group2 = new RecipeGroup(() => "Any Basic Movement Accessories", new int[] {
        ItemID.HermesBoots,
        ItemID.CloudinaBottle,
        ItemID.FlurryBoots,
        ItemID.SailfishBoots,
        ItemID.SandstorminaBottle,
        ItemID.FartinaJar,
        ItemID.ShinyRedBalloon,
        ItemID.ShoeSpikes,
        ItemID.ClimbingClaws
      });
      // Registers the new recipe group with the specified name
      RecipeGroup.RegisterGroup("OriMod:EnchantedItems", group1);
      RecipeGroup.RegisterGroup("OriMod:MovementAccessories", group2);
    }
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
    public override void Load() {
      BashKey = RegisterHotKey("Bash", "Mouse2");
      DashKey = RegisterHotKey("Dash", "LeftControl");
      ClimbAndFeather = RegisterHotKey("Climbing + Feather", "LeftShift");

      if (!Main.dedServ) {
        // Add certain equip textures
        AddEquipTexture(null, EquipType.Head, "OriHead", "OriMod/PlayerEffects/OriHead");
      }

      AddNewSein(new SeinUpgrade()); // Tier 1 (Silver)
      AddNewSein(new SeinUpgrade{    // Tier 2 (Demonite/Crimsane)
        rarity = 2,
        value = 3000,
        color = new Color(108, 92, 172),
        damage = 17,
        shotsPerBurst = 2,
        projectileSpeedStart = 7f,
        homingIncreaseRate = 0.045f,
        dustScale = 1.1f,
        lightStrength = 1.6f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 3 (Hellstone)
        rarity = 3,
        value = 10000,
        color = new Color(240, 0, 0, 194),
        damage = 28, 
        shotsPerBurst = 3, 
        tileCollide = false, 
        randDegrees = 100,
        projectileSpeedStart = 10.5f, 
        projectileSpeedIncreaseRate = 0.65f, 
        projectileSpeedIncreaseDelay = 19, 
        targetMaxDist = 320f,
        dustScale = 1.4f,
        lightStrength = 1.275f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 4 (Mythral/Orichalcum)
        rarity = 4,
        value = 25000,
        color = new Color(185, 248, 248),
        damage = 39,
        targets = 2, 
        projectileSpeedStart = 12.5f, 
        homingIncreaseRate = 0.05f, 
        homingIncreaseDelay = 22.5f,
        dustScale = 1.6f,
        lightStrength = 1.2f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 5 (Hallow)
        rarity = 5,
        value = 50000,
        color = new Color(255, 228, 160),
        damage = 52, 
        pierce = 2, 
        targetMaxDist = 370f,
        dustScale = 1.9f,
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
        homingIncreaseDelay = 20f, 
        projectileSpeedStart = 14.5f, 
        projectileSpeedIncreaseRate = 0.825f, 
        projectileSpeedIncreaseDelay = 17, 
        randDegrees = 70,
        dustScale = 2.2f,
        lightStrength = 2.25f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 7 (Lunar)
        rarity = 9,
        value = 250000,
        color = new Color(78, 38, 102),
        damage = 84, 
        shotsPerBurst = 4,
        projectileSpeedStart = 16f, 
        targetMaxDist = 550f,
        dustScale = 2.65f,
        lightStrength = 4.5f,
      });
      AddNewSein(new SeinUpgrade{    // Tier 8 (Lunar Bars)
        rarity = 10,
        value = 500000,
        color = new Color(220, 220, 220),
        damage = 114, 
        pierce = 3,
        shotsPerBurst = 5, 
        targets = 6, 
        minCooldown = 6.5f, 
        shortCooldown = 18f, 
        longCooldown = 35f, 
        homingStrengthStart = 0.05f, 
        homingIncreaseRate = 0.025f, 
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
    public override void Unload() {
      BashKey = null;
      DashKey = null;
      ClimbAndFeather = null;
    }
    internal enum OriModPacket : byte {
      OriSet
    }
    
    public override void HandlePacket(BinaryReader reader, int fromWho) {
      if (Main.netMode == NetmodeID.MultiplayerClient) {
        fromWho = reader.ReadInt32();
      }
      OriPlayer oPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      oPlayer.OriSet = reader.ReadBoolean();
      oPlayer.OriSetPrevious = reader.ReadBoolean();
      Vector2 oFrame = reader.ReadVector2();
      oPlayer.SetFrame(oFrame);
      oPlayer.OriFlashing = reader.ReadBoolean();

      if (Main.netMode == NetmodeID.Server) {
        OriPlayer.Send(-1, fromWho, oPlayer, this);
      }
    }
  }
}
