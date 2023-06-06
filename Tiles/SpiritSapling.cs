using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.ID;

namespace OriMod.Tiles {
  /// <summary>
  /// Tile used to transform the player from and to Ori state.
  /// </summary>
  public class SpiritSapling : ModTile {
    public override void SetStaticDefaults() {
      Main.tileFrameImportant[Type] = true;
      Main.tileNoAttach[Type] = true;
      Main.tileLavaDeath[Type] = true;
      TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
      TileObjectData.newTile.Origin = new Point16(0, 1);
      TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
      TileObjectData.newTile.StyleHorizontal = true;
      TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
      TileObjectData.addTile(Type);
      ModTranslation name = CreateMapEntryName();
      AddMapEntry(new Color(200, 200, 200), name);
      TileID.Sets.DisableSmartCursor[Type] = true;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
      Item.NewItem(null, i * 16, j * 16, 32, 32, ModContent.ItemType<Items.SpiritSapling>());
    }

    public override bool RightClick(int i, int j) {
      OriPlayer oPlayer = OriPlayer.Local;
      Player player = oPlayer.Player;

      Main.mouseRightRelease = false;
      if (oPlayer.Transforming) {
        return false;
      }
      if (!oPlayer.IsOri) {
        oPlayer.BeginTransformation();
        if (!oPlayer.HasTransformedOnce) {
          oPlayer.PlaySound("AbilityPedestal/abilityPedestalMusic", 0.25f);
        }
      }
      else {
        oPlayer.IsOri = false;
        oPlayer.PlaySound("SavePoints/checkpointSpawnSound");
      
        Vector2 pos = player.position;
        pos.Y += 4;
        pos.X -= 2;
        for (int m = 0; m < 100; m++) {
          Dust dust = Dust.NewDustDirect(pos, 30, 30, DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255));
          dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
        }
      }
      return true;
    }
  }
}
