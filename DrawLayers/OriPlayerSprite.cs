using AnimLib;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Skins.Slots;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.DrawLayers;

/// <summary>
/// Draws the Ori sprite.
/// </summary>
public sealed class OriPlayerSprite : AnimPlayerDrawLayer<OriCharacter, OriAnimation> {
  public override bool IsHeadLayer => true;

  public override bool GetDefaultVisibility(PlayerDrawSet drawInfo, OriCharacter ori, OriAnimation anim) {
    if (!base.GetDefaultVisibility(drawInfo, ori, anim)) {
      return false;
    }

    if (ori.UiInfo is { IsDrawingInUI: true, CurrentSlot: { } slot }) {
      return slot is OriSpriteSlot;
    }

    return drawInfo.drawPlayer is { outOfRange: false, dead: false, invis: false };
  }

  public override Position GetDefaultPosition() =>
    new Between(ModContent.GetInstance<OriBashArrowLayer>(), PlayerDrawLayers.MountFront);

  protected override void Draw(ref PlayerDrawSet drawInfo, OriCharacter ori, OriAnimation anim) {
    if (drawInfo.shadow != 0f) {
      return;
    }

    Player player = drawInfo.drawPlayer;

    // avoid offset in hairstyle menu
    Vector2 headOffset = drawInfo.headOnlyRender && ori.UiInfo.CategoryIndex == 2
      ? Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height]
      : Vector2.Zero;

    // Draw the primary layer.
    // Primary color uses vanilla skinColor
    if (anim.TryGetDrawData(in drawInfo, "Primary/Body", out DrawData data)) {
      data.position += headOffset;
      drawInfo.DrawDataCache.Add(data);
    }

    // Draw the secondary layer.
    // Secondary color uses vanilla shirtColor
    if (anim.TryGetDrawData(in drawInfo, "Secondary/Body", out data)) {
      data.position += headOffset;
      drawInfo.DrawDataCache.Add(data);
    }

    // Draw pupil
    if (anim.TryGetDrawData(in drawInfo, "Eye Pupil", out data)) {
      data.position += headOffset;
      drawInfo.DrawDataCache.Add(data);
    }

    // Draw sclera.
    // Sclera color uses vanilla underShirtColor
    if (anim.TryGetDrawData(in drawInfo, "Eye Sclera", out data)) {
      data.position += headOffset;
      drawInfo.DrawDataCache.Add(data);
    }

    // Draw feather.
    // Feather color uses vanilla pantsColor
    if (anim.TryGetDrawData(in drawInfo, "Feather", out data)) {
      data.position += headOffset;
      drawInfo.DrawDataCache.Add(data);
    }

    ori.GetState<Burrow>().DrawEffects(in drawInfo);
  }
}
