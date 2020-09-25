using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Animation = AnimLib.Animations.Animation;

namespace OriMod {
  /// <summary>
  /// Contains all <see cref="PlayerLayer"/>s this mod creates.
  /// </summary>
  internal sealed class OriLayers : SingleInstance<OriLayers> {
    private OriLayers() { }

    /// <summary>
    /// Draws the Ori sprite.
    /// </summary>
    internal readonly PlayerLayer PlayerSprite = new PlayerLayer("OriMod", "OriPlayer", delegate (PlayerDrawInfo drawInfo) {
      var player = drawInfo.drawPlayer;
      var oPlayer = player.GetModPlayer<OriPlayer>();
      bool isTransformStart = !oPlayer.IsOri && oPlayer.Transforming;

      DrawData data = oPlayer.animations.playerAnim.GetDrawData(drawInfo);
      data.color = player.immune && oPlayer.immuneTimer == 0
          ? Color.Lerp(oPlayer.SpriteColorPrimary, Color.Red, player.immuneAlpha / 255f)
          : isTransformStart ? Color.White : oPlayer.SpriteColorPrimary;
      data.origin.Y += 5 * player.gravDir;
      Main.playerDrawData.Add(data);

      if (oPlayer.IsOri) {
        data.color = player.immune && oPlayer.immuneTimer == 0
            ? Color.Lerp(oPlayer.SpriteColorSecondary, Color.Red, player.immuneAlpha / 255f)
            : oPlayer.SpriteColorSecondary;

        data.texture = OriTextures.Instance.PlayerSecondary;
        Main.playerDrawData.Add(data);
      }

      if (oPlayer.IsLocal && oPlayer.abilities.burrow.Unlocked) {
        oPlayer.abilities.burrow.DrawEffects();
      }
    });

    /// <summary>
    /// Draws the Ori trails.
    /// </summary>
    internal readonly PlayerLayer Trail = new PlayerLayer("OriMod", "OriTrail", delegate (PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Trail trail = player.GetModPlayer<OriPlayer>().trail;
      if (trail.hasDrawnThisFrame) {
        return;
      }
      trail.hasDrawnThisFrame = true;

      if (!player.dead && !player.invis) {
        trail.ResetNextSegment();
      }
      Main.playerDrawData.AddRange(trail.TrailDrawDatas);
    });

    /// <summary>
    /// Draws the <see cref="Bash"/> arrow when the player Bashes or Launches.
    /// </summary>
    internal readonly PlayerLayer BashArrow = new PlayerLayer("OriMod", "BashArrow", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      Animation anim = oPlayer.animations.bashAnim;
      var abilities = oPlayer.abilities;

      Vector2 pos;
      float rotation;
      int frame;
      var ab = abilities.bash ? (Ability)abilities.bash : abilities.launch;
      if (abilities.bash) {
        pos = abilities.bash.BashEntity.Center;
        rotation = abilities.bash.bashAngle;
        frame = ab.CurrentTime < 40 ? 0 : ab.CurrentTime < 50 ? 1 : 2;
      }
      else {
        pos = oPlayer.player.Center;
        rotation = abilities.launch.launchAngle;
        frame = ab.CurrentTime < 25 ? 0 : ab.CurrentTime < 35 ? 1 : 2;
      }
      pos -= Main.screenPosition;
      var rect = anim.TileAt(anim.source["Bash"], frame);
      var orig = rect.Size() / 2;
      var effect = SpriteEffects.None;
      var data = new DrawData(anim.CurrentTexture, pos, rect, Color.White, rotation, orig, 1, effect, 0);
      Main.playerDrawData.Add(data);
    });

    /// <summary>
    /// Draws the <see cref="Glide"/> feather when the player glides.
    /// </summary>
    internal readonly PlayerLayer FeatherSprite = new PlayerLayer("OriMod", "Feather", delegate (PlayerDrawInfo drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();

      Main.playerDrawData.Add(oPlayer.animations.glideAnim.GetDrawData(drawInfo));
    });

    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the <see cref="SoulLink"/> a player can place within the world.
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
