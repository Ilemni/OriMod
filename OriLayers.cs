using AnimLib.Abilities;
using AnimLib.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
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
    internal sealed class OriPlayerSprite : PlayerDrawLayer {
      public override bool IsHeadLayer => true;
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;
      public override void SetStaticDefaults() {
        playerSprite = ModContent.GetInstance<OriPlayerSprite>();
      }
      public override string Name => "OriPlayer";
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
        bool dyeEn = OriMod.ConfigClient.dyeEnabled &&
          (OriMod.ConfigClient.dyeEnabledAll || oPlayer.IsLocal);
        bool isTransformStart = !oPlayer.IsOri && oPlayer.Transforming;

        DrawData data = oPlayer.Animations.playerAnim.GetDrawData(drawInfo);
        bool doFlash = player.immune && oPlayer.immuneTimer == 0;
        if (oPlayer.armor_dye != player.dye[1].netID) {
          oPlayer.dye_shader = GameShaders.Armor.GetShaderFromItemId(player.dye[1].netID);
          oPlayer.armor_dye = player.dye[1].netID;
        }
        Color shColor = oPlayer.dye_shader?.GetColor() ?? Color.White;
        Color sprCol = Color.Lerp(oPlayer.SpriteColorPrimary, shColor,
          (!dyeEn || shColor == Color.White) ? 0 : oPlayer.DyeColorBlend);
        data.color = doFlash
            ? Color.Lerp(sprCol, Color.Red, player.immuneAlpha / 255f)
            : isTransformStart ? Color.White : sprCol;
        data.shader = dyeEn ? player.dye[1].dye : 0;
        data.origin.Y += 5 * player.gravDir;
        if (player.portableStoolInfo.IsInUse)
          data.origin.Y -= 12;
        drawInfo.DrawDataCache.Add(data);

        // Secondary color layer, only used when IsOri is true (i.e. not during transform start)
        if (oPlayer.IsOri) {
          data.color = doFlash
              ? Color.Lerp(oPlayer.SpriteColorSecondary, Color.Red, player.immuneAlpha / 255f)
              : oPlayer.SpriteColorSecondary;

          data.texture = OriTextures.Instance.playerSecondary;
          data.shader = player.dye[1].dye;
          drawInfo.DrawDataCache.Add(data);
        }

        if (oPlayer.IsLocal && oPlayer.abilities.burrow.Unlocked) {
          oPlayer.abilities.burrow.DrawEffects(ref drawInfo);
        }
      }
      public override Position GetDefaultPosition() =>
        new Between(ModContent.GetInstance<OriBashArrowLayer>(), PlayerDrawLayers.MountFront);
    }
    internal static PlayerDrawLayer playerSprite { get; private set; }

    /// <summary>
    /// Draws the Ori trails.
    /// </summary>
    internal sealed class OriTrailLayer : PlayerDrawLayer {
      public override string Name => "OriTrail";
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;
      public override void SetStaticDefaults() {
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
      public override Position GetDefaultPosition() =>
        new Between(PlayerDrawLayers.FaceAcc, ModContent.GetInstance<OriPlayerSprite>());
    }
    internal static PlayerDrawLayer trailLayer { get; private set; }

    /// <summary>
    /// Draws the <see cref="Glide"/> feather when the player glides.
    /// </summary>
    internal sealed class OriFeatherLayer : PlayerDrawLayer {
      public override string Name => "Feather";
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;
      public override void SetStaticDefaults() {
        featherSprite = ModContent.GetInstance<OriFeatherLayer>();
      }
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
        drawInfo.DrawDataCache.Add(oPlayer.Animations.glideAnim.GetDrawData(drawInfo));
      }
      public override Position GetDefaultPosition() =>
        new Between(ModContent.GetInstance<OriTrailLayer>(), ModContent.GetInstance<OriBashArrowLayer>());
    }
    internal static PlayerDrawLayer featherSprite { get; private set; }

    /// <summary>
    /// Draws the <see cref="Bash"/> arrow when the player Bashes or Launches.
    /// </summary>
    internal sealed class OriBashArrowLayer : PlayerDrawLayer {
      public override string Name => "BashArrow";
      public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
        drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;
      public override void SetStaticDefaults() {
        bashArrow = ModContent.GetInstance<OriBashArrowLayer>();
      }
      protected override void Draw(ref PlayerDrawSet drawInfo) {
        OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
        Animation anim = oPlayer.Animations.bashAnim;
        OriAbilityManager abilities = oPlayer.abilities;

        Vector2 pos;
        float rotation;
        int frame;
        Ability ab = abilities.bash ? abilities.bash : abilities.launch;
        if (abilities.bash) {
          pos = abilities.bash.BashEntity.Center;
          rotation = abilities.bash.BashAngle;
          frame = ab.stateTime < 40 ? 0 : ab.stateTime < 50 ? 1 : 2;
        }
        else {
          pos = oPlayer.Player.Center;
          rotation = abilities.launch.LaunchAngle;
          frame = ab.stateTime < 25 ? 0 : ab.stateTime < 35 ? 1 : 2;
        }
        pos -= Main.screenPosition;
        Rectangle rect = anim.TileAt(anim.source["Bash"], frame);
        Vector2 orig = rect.Size() / 2;
        DrawData data = new(anim.CurrentTexture, pos, rect, Color.White, rotation, orig, 1, SpriteEffects.None, 0);
        data.ignorePlayerRotation = true;
        drawInfo.DrawDataCache.Add(data);
      }
      public override Position GetDefaultPosition() =>
        new Between(ModContent.GetInstance<OriFeatherLayer>(), ModContent.GetInstance<OriPlayerSprite>());
    }
    internal static PlayerDrawLayer bashArrow { get; private set; }
  }
}
