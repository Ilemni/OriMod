using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// Contains all <see cref="PlayerLayer"/>s this mod creates.
  /// </summary>
  internal class OriLayers : SingleInstance<OriLayers> {
    private OriLayers() { }

    /// <summary>
    /// Draws the Ori sprite.
    /// </summary>
    internal readonly PlayerLayer PlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      bool isTransformStart = oPlayer.animations.TrackName == "TransformStart";

      DrawData data = oPlayer.animations.playerAnim.GetDrawData(drawInfo);
      data.color = oPlayer.flashing ? Color.Red : isTransformStart ? Color.White : oPlayer.SpriteColorPrimary;
      Main.playerDrawData.Add(data);

      if (oPlayer.IsOri && !isTransformStart) {
        data = oPlayer.animations.playerAnim.GetDrawData(drawInfo);
        data.color = oPlayer.flashing ? Color.Red : oPlayer.SpriteColorSecondary;
        data.texture = OriTextures.Instance.PlayerSecondary;
        Main.playerDrawData.Add(data);
      }

      if (oPlayer.IsLocal) {
        oPlayer.abilities.burrow.DrawEffects();
      }
    });

    /// <summary>
    /// Draws the Ori trails.
    /// </summary>
    internal readonly PlayerLayer Trail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Trail trail = player.GetModPlayer<OriPlayer>().trail;
      if (trail.lastTrailDrawTime >= Main.time) {
        return;
      }
      trail.lastTrailDrawTime = Main.time;

      if (!player.dead && !player.invis) {
        trail.ResetNextSegment();
      }
      Main.playerDrawData.AddRange(trail.TrailDrawDatas);
    });

    /// <summary>
    /// Draws the <see cref="Abilities.Bash"/> arrow when the player Bashes.
    /// </summary>
    internal readonly PlayerLayer BashArrow = new PlayerLayer("OriMod", "BashArrow", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      Animations.Animation anim = oPlayer.animations.bashAnim;
      var bash = oPlayer.abilities.bash;

      var pos = bash.BashEntity.Center - Main.screenPosition;
      var orig = anim.ActiveTile.Size() / 2;
      int frame = bash.CurrentTime < 40 ? 0 : bash.CurrentTime < 50 ? 1 : 2;
      var rect = new Rectangle(0, frame * anim.source.spriteSize.Y, anim.source.spriteSize.X, anim.source.spriteSize.Y);
      var rotation = oPlayer.abilities.bash.bashAngle;
      var effect = SpriteEffects.None;
      var data = new DrawData(anim.Texture, pos, rect, Color.White, rotation, orig, 1, effect, 0);
      Main.playerDrawData.Add(data);
    });

    /// <summary>
    /// Draws the <see cref="Abilities.Glide"/> feather when the player glides.
    /// </summary>
    internal readonly PlayerLayer FeatherSprite = new PlayerLayer("OriMod", "Feather", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();

      Main.playerDrawData.Add(oPlayer.animations.glideAnim.GetDrawData(drawInfo));
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the <see cref="Abilities.SoulLink"/> a player can place within the world.
    /// <para>(Consider using <see cref="Dust"/> or <see cref="Projectile"/> instead of <see cref="PlayerLayer"/>).</para>
    /// </summary>
    [System.Obsolete]
    internal readonly PlayerLayer SoulLinkLayer = new PlayerLayer("OriMod", "SoulLink", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      Vector2 pos = oPlayer.abilities.soulLink.SoulLinkLocation.ToWorldCoordinates() - Main.screenPosition;
      int frame = (int)(Main.time % 48 / 8) * 64;
      var rect = new Rectangle(0, frame, 48, 64);
      Vector2 orig = rect.Size() / 2;
      orig.Y += 8;
      SpriteEffects effect = SpriteEffects.None;

      var data = new DrawData(OriTextures.Instance.SoulLink, pos, rect, Color.White, 0, orig, 1, effect, 0);
      Main.playerDrawData.Add(data);
    });
  }
}
