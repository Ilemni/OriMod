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
  internal static class OriLayers {

    /// <summary>
    /// Draws the Ori sprite.
    /// </summary>
    [Autoload(false)]
    internal sealed class OriPlayerSprite : PlayerDrawLayer {
      private Position _draw_pos;
      public override void SetStaticDefaults() {
        _draw_pos = new AfterParent(PlayerDrawLayers.FaceAcc);
        playerSprite = ModContent.GetInstance<OriPlayerSprite>();
      }
      public override string Name => "OriPlayer";
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
        bool isTransformStart = !oPlayer.IsOri && oPlayer.Transforming;

        DrawData data = oPlayer.Animations.playerAnim.GetDrawData(drawInfo);
        bool doFlash = player.immune && oPlayer.immuneTimer == 0;
        data.color = doFlash
            ? Color.Lerp(oPlayer.SpriteColorPrimary, Color.Red, player.immuneAlpha / 255f)
            : isTransformStart ? Color.White : oPlayer.SpriteColorPrimary;
        data.origin.Y += 5 * player.gravDir;
        drawInfo.DrawDataCache.Add(data);

        // Secondary color layer, only used when IsOri is true (i.e. not during transform start)
        if (oPlayer.IsOri) {
          data.color = doFlash
              ? Color.Lerp(oPlayer.SpriteColorSecondary, Color.Red, player.immuneAlpha / 255f)
              : oPlayer.SpriteColorSecondary;

          data.texture = OriTextures.Instance.playerSecondary;
          drawInfo.DrawDataCache.Add(data);
        }

        if (oPlayer.IsLocal && oPlayer.abilities.burrow.Unlocked) {
          oPlayer.abilities.burrow.DrawEffects(ref drawInfo);
        }
      }
      public override Position GetDefaultPosition() => _draw_pos;
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        OriPlayer.Local.IsOri && !OriPlayer.Local.Transforming;
    }
    internal static PlayerDrawLayer playerSprite { get; private set; }

    /// <summary>
    /// Draws the Ori trails.
    /// </summary>
    [Autoload(false)]
    internal sealed class OriTrailLayer : PlayerDrawLayer {
      private Position _draw_pos;
      public override string Name => "OriTrail";
      public override void SetStaticDefaults() {
        _draw_pos = new AfterParent(playerSprite);
        trailLayer = ModContent.GetInstance<OriTrailLayer>();
      }
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        Trail trail = player.GetModPlayer<OriPlayer>().trail;
        if (trail.hasDrawnThisFrame) {
          return;
        }
        trail.hasDrawnThisFrame = true;
        trail.UpdateSegments();
        if (!player.dead && !player.invis) {
          trail.ResetNextSegment();
        }
        drawInfo.DrawDataCache.AddRange(trail.TrailDrawDatas);
      }
      public override Position GetDefaultPosition() => _draw_pos;
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        OriPlayer.Local.IsOri && !OriPlayer.Local.Transforming;
    }
    internal static PlayerDrawLayer trailLayer { get; private set; }

    /// <summary>
    /// Draws the <see cref="Glide"/> feather when the player glides.
    /// </summary>
    [Autoload(false)]
    internal sealed class OriFeatherLayer : PlayerDrawLayer {
      private Position _draw_pos;
      public override string Name => "Feather";
      public override void SetStaticDefaults() {
        _draw_pos = new AfterParent(trailLayer);
        featherSprite = ModContent.GetInstance<OriFeatherLayer>();
      }
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();

        drawInfo.DrawDataCache.Add(oPlayer.Animations.glideAnim.GetDrawData(drawInfo));
      }
      public override Position GetDefaultPosition() => _draw_pos;
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        OriPlayer.Local.IsOri && !OriPlayer.Local.Transforming;
    }
    internal static PlayerDrawLayer featherSprite { get; private set; }

    /// <summary>
    /// Draws the <see cref="Bash"/> arrow when the player Bashes or Launches.
    /// </summary>
    [Autoload(false)]
    internal sealed class OriBashArrowLayer : PlayerDrawLayer {
      private Position _draw_pos;
      public override string Name => "BashArrow";
      public override void SetStaticDefaults() {
        _draw_pos = new AfterParent(featherSprite);
        bashArrow = ModContent.GetInstance<OriBashArrowLayer>();
      }
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
        Animation anim = oPlayer.Animations.bashAnim;
        AbilityManager abilities = oPlayer.abilities;

        Vector2 pos;
        float rotation;
        int frame;
        Ability ab = abilities.bash ? (Ability)abilities.bash : abilities.launch;
        if (abilities.bash) {
          pos = abilities.bash.BashEntity.Center;
          rotation = abilities.bash.BashAngle;
          frame = ab.CurrentTime < 40 ? 0 : ab.CurrentTime < 50 ? 1 : 2;
        }
        else {
          pos = oPlayer.Player.Center;
          rotation = abilities.launch.LaunchAngle;
          frame = ab.CurrentTime < 25 ? 0 : ab.CurrentTime < 35 ? 1 : 2;
        }
        pos -= Main.screenPosition;
        Rectangle rect = anim.TileAt(anim.source["Bash"], frame);
        Vector2 orig = rect.Size() / 2;
        DrawData data = new(anim.CurrentTexture, pos, rect, Color.White, rotation, orig, 1, SpriteEffects.None, 0);
        drawInfo.DrawDataCache.Add(data);
      }
      public override Position GetDefaultPosition() => _draw_pos;
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        OriPlayer.Local.IsOri && !OriPlayer.Local.Transforming;
    }
    internal static PlayerDrawLayer bashArrow { get; private set; }

    /*
    /// <summary>
    /// <see cref="PlayerLayer"/> that represents the <see cref="SoulLink"/> a player can place within the world.
    /// <para>(Consider using <see cref="Dust"/> or <see cref="Projectile"/> instead of <see cref="PlayerLayer"/>).</para>
    /// </summary>
    [Obsolete]
    internal readonly PlayerLayer soulLinkLayer = new PlayerLayer("OriMod", "SoulLink", delegate (PlayerDrawSet drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      Vector2 pos = oPlayer.Abilities.soulLink.SoulLinkLocation.ToWorldCoordinates() - Main.screenPosition;
      int frame = (int)(Main.time % 48 / 8) * 64;
      var rect = new Rectangle(0, frame, 48, 64);
      Vector2 orig = rect.Size() / 2;
      orig.Y += 8;
      SpriteEffects effect = SpriteEffects.None;

      var data = new DrawData(OriTextures.Instance.soulLink, pos, rect, Color.White, 0, orig, 1, effect, 0);
      drawInfo.DrawDataCache.Add(data);
    });*/
  }
}
