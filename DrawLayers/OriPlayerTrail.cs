using AnimLib;
using OriMod.Abilities;
using OriMod.Animations;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.DrawLayers;

/// <summary>
/// Draws the Ori trails.
/// </summary>
public sealed class OriPlayerTrail : AnimPlayerDrawLayer<OriCharacter, OriAnimation> {
  public override bool GetDefaultVisibility(PlayerDrawSet drawInfo, OriCharacter ori, OriAnimation anim) =>
    base.GetDefaultVisibility(drawInfo, ori, anim) && !drawInfo.drawPlayer.mount.Active &&
    ori.ActiveState is not (Abilities.Transform or Burrow);

  public override Position GetDefaultPosition() =>
    new Between(PlayerDrawLayers.FaceAcc, ModContent.GetInstance<OriPlayerSprite>());

  protected override void Draw(ref PlayerDrawSet drawInfo, OriCharacter ori, OriAnimation anim) {
    if (drawInfo.shadow > 0) {
      return;
    }

    if (anim.SpriteSheet.HasLayer("AfterImage")) {
      drawInfo.DrawDataCache.AddRange(ori.Trail!.TrailDrawDatas);
    }
  }
}
