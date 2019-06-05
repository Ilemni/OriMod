using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace OriMod.Tiles {
  public class SpiritSapling : ModTile {
    public override void SetDefaults() {
      Main.tileFrameImportant[Type] = true;
      Main.tileNoAttach[Type] = true;
      Main.tileLavaDeath[Type] = true;
      TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
      TileObjectData.newTile.Origin = new Point16(0, 1);
      TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
      TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
      TileObjectData.newTile.StyleHorizontal = true;
      TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
      TileObjectData.addTile(Type);
      ModTranslation name = CreateMapEntryName();
      AddMapEntry(new Color(200, 200, 200), name);
      disableSmartCursor = true;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
      Item.NewItem(i * 16, j * 16, 32, 32, mod.ItemType("SpiritSapling"));
    }

    public override void RightClick(int i, int j) {
      Player player = Main.LocalPlayer;
      Tile tile = Main.tile[i, j];
      Main.mouseRightRelease = false;
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      if (!oPlayer.OriSet && !oPlayer.Transforming) {
        oPlayer.TransformBlockLocation = new Vector2(i, j) * 16;
        oPlayer.DoTransformation();
        oPlayer.PlayNewSound("AbilityPedestal/abilityPedestalMusic");
        oPlayer.PlayNewSound("AbilityPedestal/abilityPedestalStart");
      }
      else if (!oPlayer.Transforming) {
        oPlayer.OriSet = false;
        for (int m = 0; m < 100; m++) { //does particles
          Vector2 pos = player.position;
          pos.Y += 4;
          pos.X -= 2;
          Dust dust = Main.dust[Terraria.Dust.NewDust(pos, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        }
      }
      else {
        oPlayer.OriSet = !oPlayer.OriSet;
        for (int m = 0; m < 100; m++) { //does particles
          Vector2 pos = player.position;
          pos.Y += 4;
          pos.X -= 2;
          Dust dust = Main.dust[Terraria.Dust.NewDust(pos, 30, 30, 111, 0f, 0f, 0, new Color(255, 255, 255), 1f)];
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        }
        oPlayer.PlayNewSound("SavePoints/checkpointSpawnSound");
      }
    }
  }
}