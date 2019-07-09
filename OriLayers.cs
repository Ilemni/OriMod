using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod {
  public static class OriLayers {
    private static bool? SecondaryTextureExists = null;
    private static Texture2D SecondaryTexture = null;
    internal static readonly PlayerLayer PlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      Player player = drawInfo.drawPlayer;
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>(mod);
      
      DrawData data = DefaultDrawData(oPlayer, oPlayer.Animations.PlayerAnim);
      // Current check for if in player select screen
      if (player.position == Vector2.Zero) {
        data.position = drawInfo.position - Main.screenPosition + player.Size / 2;
      }
      data.color = oPlayer.Flashing ? Color.Red : oPlayer.Transforming && oPlayer.AnimName == "TransformStart" ? Color.White : oPlayer.SpriteColor;
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer SecondaryLayer = new PlayerLayer("OriMod", "SecondaryColor", delegate (PlayerDrawInfo drawinfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      if ((SecondaryTextureExists ?? (SecondaryTextureExists = mod.TextureExists("PlayerEffects/OriPlayerSecondary"))) == false) return;
      OriPlayer oPlayer = drawinfo.drawPlayer.GetModPlayer<OriPlayer>(mod);
      
      DrawData data = DefaultDrawData(oPlayer, oPlayer.Animations.SecondaryLayer);
      data.color = oPlayer.Flashing ? Color.Red : oPlayer.Transforming && oPlayer.AnimName == "TrasformStart" ? Color.White : oPlayer.SpriteColorSecondary;
      data.texture = SecondaryTexture ?? (SecondaryTexture = mod.GetTexture("PlayerEffects/OriPlayerSecondary"));
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer Trail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      Player player = drawInfo.drawPlayer;
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>(mod);
      if (!player.dead && !player.invis) {
        oPlayer.TrailIndex++;
        if (oPlayer.TrailIndex > 25) {
          oPlayer.TrailIndex = 0;
        }
        oPlayer.Trails[oPlayer.TrailIndex].Reset(oPlayer);
      }
      for (int i = 0; i < 26; i++) {
        Trail trail = oPlayer.Trails[i];
        trail.Tick();
        Main.playerDrawData.Add(trail.GetDrawData(oPlayer));
      }
    });
    internal static readonly PlayerLayer BashArrow = new PlayerLayer("OriMod", "BashArrow", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>(mod);
      Animation anim = oPlayer.Animations.BashAnim;
      DrawData data = DefaultDrawData(oPlayer, anim);
      data.position = oPlayer.bash.BashEntity.Center - Main.screenPosition;
      int frame = oPlayer.bash.CurrDuration < 40 ? 0 : oPlayer.bash.CurrDuration < 50 ? 1 : 2;
      data.sourceRect = new Rectangle(0, frame * anim.Source.TileSize.Y, anim.Source.TileSize.X, anim.Source.TileSize.Y);
      data.rotation = oPlayer.bash.BashEntity.AngleTo(Main.MouseWorld);
      data.effect = SpriteEffects.None;
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer FeatherSprite = new PlayerLayer("OriMod", "Feather", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>(mod);

      DrawData data = DefaultDrawData(oPlayer, oPlayer.Animations.GlideAnim);
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer SoulLinkLayer = new PlayerLayer("OriMod", "SoulLink", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>(mod);
      Texture2D tex = mod.GetTexture("Dusts/SoulLinkPlaced");
      Vector2 pos = oPlayer.soulLink.Center.ToWorldCoordinates() - Main.screenPosition;
      int frame = (int)(Main.time % 45 / 15) * 32;
      Rectangle rect = new Rectangle(0, /*frame*/ 0, 24, 32);
      Vector2 orig = new Vector2(12, 20);
      SpriteEffects effect = SpriteEffects.None;

      DrawData data = new DrawData(tex, pos, rect, Color.White, 0, orig, 2, effect, 0);
      Main.playerDrawData.Add(data);
    });
    private static DrawData DefaultDrawData(OriPlayer oPlayer, Animation anim) {
      Player player = oPlayer.player;
      Texture2D texture = anim.Texture;
      Vector2 pos = player.Center - Main.screenPosition;
      Rectangle rect = anim.ActiveTile;
      Vector2 orig = new Vector2(rect.Width / 2, rect.Height / 2 + 5 * player.gravDir);
      SpriteEffects effect = SpriteEffects.None;
      if (player.direction == -1) effect = effect | SpriteEffects.FlipHorizontally;
      if (player.gravDir == -1) effect = effect | SpriteEffects.FlipVertically;
      return new DrawData(texture, pos, rect, oPlayer.SpriteColor, player.direction * oPlayer.AnimRads, orig, 1, effect, 0);
    }
  }
}