using AnimLib;
using AnimLib.Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Skins.Slots;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace OriMod.DrawLayers;

/// <summary>
/// Draws the <see cref="Bash"/> arrow when the player Bashes or Launches.
/// </summary>
public sealed class OriBashArrowLayer : AnimPlayerDrawLayer<OriCharacter, BashAnimation> {
  public override bool GetDefaultVisibility(PlayerDrawSet drawInfo, OriCharacter ori, BashAnimation anim) {
    if (!base.GetDefaultVisibility(drawInfo, ori, anim)) {
      return false;
    }

    if (ori.UiInfo.CurrentSlot is BashArrowSlot) {
      return true;
    }

    Bash bash = ori.GetState<Bash>();
    if (anim.SpriteSheet.TryGetTag("End", out AnimTag? endTag)) {
      return bash.InactiveTime < endTag.TotalDuration;
    }

    return bash.Active;
  }

  public override Position GetDefaultPosition() =>
    new Between(ModContent.GetInstance<OriPlayerTrail>(), ModContent.GetInstance<OriPlayerSprite>());

  protected override void Draw(ref PlayerDrawSet drawInfo, OriCharacter ori, BashAnimation anim) {
    Bash bash = ori.GetState<Bash>();
    // TODO: instead of indexing by name, should foreach onto layers, and use userdata to determine how to draw
    //  - rotate: layer is rotated by 45 degrees (drawn in file pointing to top right)
    //  - spin: layer rotates with player, otherwise never rotates
    //  - flip: layer uses the player SpriteEffects, otherwise always SpriteEffects.None
    //  - follow: layer follows player, otherwise never moves from starting position
    //  - glow: layer is drawn without lighting

    DrawData data = anim.GetDrawData(in drawInfo, "Arrow") with {
      rotation = bash.GetRotation(in drawInfo),
      effect = SpriteEffects.None
    };

    if (anim.SpriteSheet.UserData.HasFlag("rotate")) {
      // Sprite is aligned such that the arrow points to the top right
      data.rotation += MathHelper.PiOver4;
    }

    if (bash.Active || anim.CurrentTag.Name == "Start") {
      anim.LastActiveBashPosition = data.position;
      anim.LastBashRotation = data.rotation;
    }
    else {
      data.position = anim.LastActiveBashPosition;
      data.rotation = anim.LastBashRotation;
    }

    drawInfo.DrawDataCache.Add(data);
  }
}

public sealed class OriLaunchArrowLayer : AnimPlayerDrawLayer<OriCharacter, LaunchAnimation> {
  public override bool GetDefaultVisibility(PlayerDrawSet drawInfo, OriCharacter ori, LaunchAnimation anim) =>
    base.GetDefaultVisibility(drawInfo, ori, anim) &&
    (ori.ActiveState is Launch || ori.UiInfo.CurrentSlot is LaunchArrowSlot);

  public override Position GetDefaultPosition() =>
    new Between(ModContent.GetInstance<OriPlayerTrail>(), ModContent.GetInstance<OriPlayerSprite>());

  protected override void Draw(ref PlayerDrawSet drawInfo, OriCharacter ori, LaunchAnimation anim) {
    Launch launch = ori.GetState<Launch>();

    DrawData data2 = anim.GetDrawData(in drawInfo, "Arrow") with {
      rotation = launch.GetRotation(in drawInfo),
      effect = SpriteEffects.None
    };

    if (anim.SpriteSheet.UserData.HasFlag("rotate")) {
      // Sprite is aligned such that the arrow points to the top right
      data2.rotation += MathHelper.PiOver4;
    }

    if (launch.Starting || anim.CurrentTag.Name == "Start") {
      anim.LastActiveLaunchPosition = data2.position;
    }
    else {
      data2.position += anim.LastActiveLaunchPosition;
    }

    drawInfo.DrawDataCache.Add(data2);
  }
}
