using System;
using System.Diagnostics.CodeAnalysis;
using AnimLib;
using AnimLib.Animations;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Skins.Slots;

namespace OriMod.Animations;

public sealed class LaunchAnimation : SkinAnimation {
  [field: AllowNull, MaybeNull]
  private Launch Launch => field ??= Character.GetState<Launch>();

  internal Vector2 LastActiveLaunchPosition;

  protected override AnimationOptions? UpdateAnimation() {
    if (Launch.Starting) {
      float launchStartDuration = Launch.Stats.MaxDuration / 30f;
      float tagStartDuration = SpriteSheet.GetTag("Start").TotalDuration;
      float speed = tagStartDuration / launchStartDuration;
      return new AnimationOptions("Start") { Speed = speed };
    }

    if (!Launch.Active || !SpriteSheet.TryGetTag("End", out AnimTag? endTag)) {
      return null;
    }

    float tagEndDuration = endTag.TotalDuration;
    float launchEndDuration = Launch.Stats.GetMovDuration(Launch.CurrentChain) / 30f;
    float endSpeed = tagEndDuration / launchEndDuration;
    return new AnimationOptions("End") { Speed = endSpeed };
  }

  public override void UpdateUIAnimation(AnimUiInfo uiInfo) {
    if (Character.UiInfo.CurrentSlot is not LaunchArrowSlot) {
      return;
    }

    if (!HasTag("End")) {
      UIAnimation(new AnimationOptions("Start"), uiInfo.SlotCounter);
      return;
    }

    ReadOnlySpan<AnimationOptions> opts = [
      new("Start") { LoopCount = 1 },
      new("End") { LoopCount = 1 }
    ];
    UILoopedAnimationSequence(opts, uiInfo.SlotCounter);
  }
}
