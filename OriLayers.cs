using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod {
  public static class OriLayers {
    internal static readonly PlayerLayer PlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      Player drawPlayer = drawInfo.drawPlayer;
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Texture2D spriteTexture = mod.GetTexture("PlayerEffects/OriPlayer");
      Vector2 pos;
      SpriteEffects effect;
      Rectangle rect;
      Vector2 orig;
      GetSpriteInfo(drawPlayer, oPlayer, out pos, out effect, out rect, out orig);
      
      DrawData data = new DrawData(spriteTexture, pos, rect, oPlayer.SpriteColor, drawPlayer.direction * oPlayer.AnimRads, orig, 1, effect, 0);
      data.position += Offset();
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer Trail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      for (int i = 0; i < 26; i++) {
        Trail trail = oPlayer.Trails[i];
        trail.Alpha -= 0.00002f;
        if (trail.Alpha < 0) {
          trail.Alpha = 0;
        }
      }
      if (!drawPlayer.dead && !drawPlayer.invis) {
        oPlayer.TrailIndex++;
        if (oPlayer.TrailIndex > 25) {
          oPlayer.TrailIndex = 0;
        }
        Trail trail = oPlayer.Trails[oPlayer.TrailIndex];
        trail.Position = drawPlayer.Center;
        trail.Frame = oPlayer.AnimFrame;
        trail.Direction = drawPlayer.direction;
        float alpha = drawPlayer.velocity.Length() * 0.002f;
        if (alpha > 0.005f) alpha = 0.005f;
        trail.Alpha = alpha;
        trail.Rotation = oPlayer.AnimRads;
        if (trail.Alpha > 104) {
          trail.Alpha = 104;
        }
      }
      for (int i = 0; i < 26; i++) {
        Trail trail = oPlayer.Trails[i];
        SpriteEffects effect = trail.Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Color color = oPlayer.SpriteColor * trail.Alpha;
        color.A = 0;
        DrawData data = new DrawData(
          mod.GetTexture("PlayerEffects/OriGlow"),
          new Vector2(trail.Position.X - Main.screenPosition.X, trail.Position.Y - Main.screenPosition.Y),
          new Rectangle(trail.X, trail.Y, OriPlayer.SpriteWidth, OriPlayer.SpriteHeight), color, trail.Rotation,
          new Vector2(OriPlayer.SpriteWidth / 2, OriPlayer.SpriteHeight / 2 + 6), 1, effect, 0
        );
        data.position += Offset();
        Main.playerDrawData.Add(data);
      }
    });
    internal static readonly PlayerLayer TransformSprite = new PlayerLayer("OriMod", "OriTransform", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Vector2 position = drawPlayer.position;
      Texture2D texture = mod.GetTexture("PlayerEffects/transform");
      SpriteEffects effect = drawPlayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
      float t = oPlayer.TransformTimer - 235;
      int y = t > 390 ? 0 : t > 330 ? 1 : t > 270 ? 2 : t > 150 ? 3 : t > 110 ? 4 : t > 70 ? 5 : t > 30 ? 6 : 7;
      
      DrawData data = new DrawData(texture,
        new Vector2(drawPlayer.position.X - Main.screenPosition.X + 10, drawPlayer.position.Y - Main.screenPosition.Y + 8),
        new Rectangle(0, y * 76, 104, 76),
        Color.White, drawPlayer.direction * oPlayer.AnimRads,
        new Vector2(52, 38), 1, effect, 0);
      data.position += Offset();
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer BashArrow = new PlayerLayer("OriMod", "bashArrow", delegate (PlayerDrawInfo drawInfo) {
      Player drawPlayer = drawInfo.drawPlayer;
      Mod mod = ModLoader.GetMod("OriMod");
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Texture2D texture = mod.GetTexture("PlayerEffects/bashArrow");
      SpriteEffects effect = SpriteEffects.None;
      int y = oPlayer.bash.CurrDuration < 40 ? 0 : oPlayer.bash.CurrDuration < 50 ? 1 : 2;

      DrawData data = new DrawData(texture,
        new Vector2(oPlayer.bash.Npc.Center.X - Main.screenPosition.X, oPlayer.bash.Npc.Center.Y - Main.screenPosition.Y),
        new Rectangle(0, y * 20, 152, 20),
        Color.White, oPlayer.bash.Npc.AngleTo(Main.MouseWorld),
        new Vector2(76, 10), 1, effect, 0);
      Main.playerDrawData.Add(data);
    });
    internal static readonly PlayerLayer FeatherSprite = new PlayerLayer("OriMod", "Feather", delegate (PlayerDrawInfo drawInfo) {
      Mod mod = ModLoader.GetMod("OriMod");
      Player drawPlayer = drawInfo.drawPlayer;
      OriPlayer oPlayer = drawPlayer.GetModPlayer<OriPlayer>(mod);
      Texture2D spriteTexture = mod.GetTexture("PlayerEffects/Feather");
      Vector2 pos;
      SpriteEffects effect;
      Rectangle rect;
      Vector2 orig;
      GetSpriteInfo(drawPlayer, oPlayer, out pos, out effect, out rect, out orig);
      rect.X = 0;

      DrawData data = new DrawData(spriteTexture, pos, rect, oPlayer.SpriteColor, drawPlayer.direction * oPlayer.AnimRads, orig, 1, effect, 0);
      data.position += Offset();
      Main.playerDrawData.Add(data);
    });

    // Used with frames that are not aligned to center
    private static Vector2 Offset(int x=0, int y=0) {
      Vector2 offset = new Vector2(x, y);
      return offset;
    }
    private static void GetSpriteInfo(Player player, OriPlayer oPlayer, out Vector2 pos, out SpriteEffects effect, out Rectangle rect, out Vector2 orig) {
      pos = new Vector2(player.Center.X - Main.screenPosition.X, player.Center.Y - Main.screenPosition.Y);
      effect = player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
      rect = new Rectangle((int)oPlayer.AnimFrame.X, (int)oPlayer.AnimFrame.Y, OriPlayer.SpriteWidth, OriPlayer.SpriteHeight);
      orig = new Vector2(OriPlayer.SpriteWidth / 2, OriPlayer.SpriteHeight / 2 + 6);
    }
  }
}