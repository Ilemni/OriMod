using AnimLib.Abilities;
using AnimLib.Animations;
using AnimLib.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Animation = AnimLib.Animations.Animation;

namespace OriMod;

/// <summary>
/// Contains all <see cref="PlayerDrawLayer"/>s this mod creates.
/// </summary>
internal static class OriLayers {
  /// <summary>
  /// Draws the Ori sprite.
  /// </summary>
  private sealed class OriPlayerSprite : PlayerDrawLayer {
    public override bool IsHeadLayer => true;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
      drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;

    public override void SetStaticDefaults() {
      OriSprite = ModContent.GetInstance<OriPlayerSprite>();
    }

    public override string Name => nameof(OriSprite);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
      Player player = drawInfo.drawPlayer;
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      Animation playerAnim = oPlayer.Animations.PlayerAnim;

      bool dyeEn = OriMod.ConfigClient.dyeEnabled &&
        (OriMod.ConfigClient.dyeEnabledAll || oPlayer.IsLocal);
      bool isTransformStart = !oPlayer.IsOri && oPlayer.Transforming;

      DrawData data = playerAnim.GetDrawData(drawInfo, "Primary");
      bool doFlash = player.immune && !player.immuneNoBlink && OriMod.ConfigClient.flashMode != "Disabled";
      Color flashColor = Color.Transparent;
      if (OriMod.ConfigClient.flashMode == "Red") flashColor = Color.Red;
      if (oPlayer.armor_dye != player.dye[1].netID) {
        oPlayer.dye_shader = GameShaders.Armor.GetShaderFromItemId(player.dye[1].netID);
        oPlayer.armor_dye = player.dye[1].netID;
      }

      Color shaderColor = oPlayer.dye_shader?.GetColor() ?? Color.White;
      Color spriteColor = Color.Lerp(oPlayer.SpriteColorPrimary, shaderColor,
        !dyeEn || shaderColor == Color.White ? 0 : oPlayer.DyeColorBlend);
      Color dataColor = doFlash
        ? Color.Lerp(spriteColor, flashColor, player.immuneAlpha / 255f)
        : isTransformStart
          ? Color.White
          : spriteColor;

      data.color = dataColor;
      data.shader = dyeEn ? player.dye[1].dye : 0;
      data.origin.Y += 10 * player.gravDir;
      if (player.portableStoolInfo.IsInUse)
        data.origin.Y -= 12;
      drawInfo.DrawDataCache.Add(data);


      // Secondary color layer, only used when IsOri is true (i.e. not during transform start)
      if (oPlayer.IsOri) {
        data.texture = playerAnim.GetTexture("Secondary");
        data.sourceRect = playerAnim.GetRect("Secondary");
        data.color = doFlash
          ? Color.Lerp(oPlayer.SpriteColorSecondary, flashColor, player.immuneAlpha / 255f)
          : oPlayer.SpriteColorSecondary;
        data.shader = dyeEn ? player.dye[1].dye : 0;
        drawInfo.DrawDataCache.Add(data);
      }

      if (oPlayer.abilities.glide) {
        data.texture = playerAnim.GetTexture("Feather");
        data.sourceRect = playerAnim.GetRect("Feather");
        data.color = Color.White;
        drawInfo.DrawDataCache.Add(data);
      }

      if (oPlayer.IsLocal && oPlayer.abilities.burrow.Unlocked) {
        oPlayer.abilities.burrow.DrawEffects(ref drawInfo);
      }
    }

    public override Position GetDefaultPosition() =>
      new Between(ModContent.GetInstance<OriBashArrowLayer>(), PlayerDrawLayers.MountFront);
  }

  internal static PlayerDrawLayer OriSprite { get; private set; }

  /// <summary>
  /// Draws the Ori trails.
  /// </summary>
  private sealed class OriTrailLayer : PlayerDrawLayer {
    public override string Name => nameof(OriTrail);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
      drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;

    public override void SetStaticDefaults() {
      OriTrail = ModContent.GetInstance<OriTrailLayer>();
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

  internal static PlayerDrawLayer OriTrail { get; private set; }

  /// <summary>
  /// Draws the <see cref="Bash"/> arrow when the player Bashes or Launches.
  /// </summary>
  private sealed class OriBashArrowLayer : PlayerDrawLayer {
    public override string Name => nameof(BashArrow);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
      drawInfo.drawPlayer.GetModPlayer<OriPlayer>().Animations?.GraphicsEnabledCompat ?? false;

    public override void SetStaticDefaults() {
      BashArrow = ModContent.GetInstance<OriBashArrowLayer>();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      OriAbilityManager abilities = oPlayer.abilities;
      AnimSpriteSheet arrowSpriteSheet = OriTextures.Instance.Arrow;
      const string layer = "Arrow";
      AnimTextureAtlas atlas = arrowSpriteSheet.Atlases[layer];

      Vector2 pos;
      float rotation;
      Rectangle rect;
      Ability ab = abilities.bash ? abilities.bash : abilities.launch;
      if (abilities.bash) {
        pos = abilities.bash.BashEntity.Center;
        rotation = abilities.bash.BashAngle;
        rect = arrowSpriteSheet.GetRectFromTimer(layer, nameof(Bash), ab.stateTime);
      }
      else {
        pos = oPlayer.Player.Center;
        rotation = abilities.launch.LaunchAngle;
        rect = arrowSpriteSheet.GetRectFromTimer(layer, nameof(Launch), ab.stateTime);
      }

      pos -= Main.screenPosition;
      Vector2 orig = rect.Size() / 2;
      DrawData data = new(atlas.Texture, pos, rect, Color.White, rotation, orig, 1, SpriteEffects.None)
        {
          ignorePlayerRotation = true
        };
      drawInfo.DrawDataCache.Add(data);
    }

    public override Position GetDefaultPosition() =>
      new Between(ModContent.GetInstance<OriTrailLayer>(), ModContent.GetInstance<OriPlayerSprite>());
  }

  internal static PlayerDrawLayer BashArrow { get; private set; }
}
