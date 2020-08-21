using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod {
  internal class OriLayers : SingleInstance<OriLayers> {
    private OriLayers() { }

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the base sprite for the <see cref="OriPlayer"/> sprite.
    /// </summary>
    internal readonly PlayerLayer PlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();

      DrawData data = DefaultDrawData(drawInfo, oPlayer, oPlayer.Animations.PlayerAnim);
      data.color = oPlayer.Flashing ? Color.Red : oPlayer.Transforming && oPlayer.AnimName == "TransformStart" ? Color.White : oPlayer.SpriteColorPrimary;
      Main.playerDrawData.Add(data);

      oPlayer.Abilities.burrow.DrawEffects();
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents secondary colors for the <see cref="OriPlayer"/> sprite.
    /// </summary>
    internal readonly PlayerLayer SecondaryLayer = new PlayerLayer("OriMod", "OriPlayer_SecondaryColor", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();

      DrawData data = DefaultDrawData(drawInfo, oPlayer, oPlayer.Animations.SecondaryLayer);
      data.color = oPlayer.Flashing ? Color.Red : oPlayer.Transforming && oPlayer.AnimName == "TrasformStart" ? Color.White : oPlayer.SpriteColorSecondary;
      data.texture = OriTextures.Instance.PlayerSecondary;
      Main.playerDrawData.Add(data);
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the trail for the <see cref="OriPlayer"/>.
    /// </summary>
    internal readonly PlayerLayer Trail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      if (!player.dead && !player.invis) {
        oPlayer.Trails.ResetNextTrail();
      }
      oPlayer.Trails.AddTrailDrawDataToMain();
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the <see cref="Abilities.Bash"/> arrow when a player Bashes.
    /// </summary>
    internal readonly PlayerLayer BashArrow = new PlayerLayer("OriMod", "BashArrow", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      Animations.Animation anim = oPlayer.Animations.BashAnim;
      var bash = oPlayer.Abilities.bash;

      var pos = bash.BashEntity.Center - Main.screenPosition;
      var orig = anim.ActiveTile.Size() / 2;
      int frame = bash.CurrDuration < 40 ? 0 : bash.CurrDuration < 50 ? 1 : 2;
      var rect = new Rectangle(0, frame * anim.source.spriteSize.Y, anim.source.spriteSize.X, anim.source.spriteSize.Y);
      var rotation = oPlayer.Abilities.bash.BashEntity.AngleTo(Main.MouseWorld);
      var effect = SpriteEffects.None;
      var data = new DrawData(anim.Texture, pos, rect, Color.White, rotation, orig, 1, effect, 0);
      Main.playerDrawData.Add(data);
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the <see cref="Abilities.Glide"/> feather when a player Glides.
    /// </summary>
    internal readonly PlayerLayer FeatherSprite = new PlayerLayer("OriMod", "Feather", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();

      Main.playerDrawData.Add(DefaultDrawData(drawInfo, oPlayer, oPlayer.Animations.GlideAnim));
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the <see cref="Abilities.SoulLink"/> a player can place within the world.
    /// <para>(Consider using <see cref="Dust"/>s instead of <see cref="PlayerLayer"/>s</para>
    /// </summary>
    [System.Obsolete]
    internal readonly PlayerLayer SoulLinkLayer = new PlayerLayer("OriMod", "SoulLink", delegate (PlayerDrawInfo drawInfo) {
      var mod = OriMod.Instance;
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      Vector2 pos = oPlayer.Abilities.soulLink.SoulLinkLocation.ToWorldCoordinates() - Main.screenPosition;
      int frame = (int)(Main.time % 48 / 8) * 64;
      var rect = new Rectangle(0, frame, 48, 64);
      Vector2 orig = rect.Size() / 2;
      orig.Y += 8;
      SpriteEffects effect = SpriteEffects.None;

      var data = new DrawData(OriTextures.Instance.SoulLink, pos, rect, Color.White, 0, orig, 1, effect, 0);
      Main.playerDrawData.Add(data);
    });

    private static DrawData DefaultDrawData(PlayerDrawInfo drawInfo, OriPlayer oPlayer, Animations.Animation anim) {
      Player player = oPlayer.player;
      Texture2D texture = anim.Texture;
      Vector2 pos = drawInfo.position - Main.screenPosition + player.Size / 2;
      Rectangle rect = anim.ActiveTile;
      var orig = new Vector2(rect.Width / 2, rect.Height / 2 + 5 * player.gravDir);
      SpriteEffects effect = SpriteEffects.None;
      if (player.direction == -1) {
        effect |= SpriteEffects.FlipHorizontally;
      }

      if (player.gravDir == -1) {
        effect |= SpriteEffects.FlipVertically;
      }

      return new DrawData(texture, pos, rect, oPlayer.SpriteColorPrimary, player.direction * oPlayer.AnimRads, orig, 1, effect, 0);
    }
  }
}
