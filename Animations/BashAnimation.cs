using System;
using System.Diagnostics.CodeAnalysis;
using AnimLib;
using AnimLib.Animations;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using OriMod.Skins.Slots;

namespace OriMod.Animations;

/// <summary>
///
/// </summary>
/// <seealso cref="LaunchAnimation"/>
public sealed class BashAnimation : SkinAnimation {
  [field: AllowNull, MaybeNull]
  private Bash Bash => field ??= Character.GetState<Bash>();

  internal Vector2 LastActiveBashPosition;
  internal float LastBashRotation;

  protected override AnimationOptions? UpdateAnimation() {
    if (Bash.Active) {
      float bashStartDuration = Bash.Stats.MaxTime / 30f;
      float tagStartDuration = SpriteSheet.GetTag("Start").TotalDuration;
      float speed = tagStartDuration / bashStartDuration;
      return new AnimationOptions("Start") { Speed = speed };
    }

    if (!SpriteSheet.TryGetTag("End", out AnimTag? endTag)) {
      return null;
    }

    float tagEndDuration = endTag.TotalDuration;
    if (Bash.InactiveTime < tagEndDuration * 30) {
      return new AnimationOptions("End");
    }

    return null;
  }

  public override void UpdateUIAnimation(AnimUiInfo uiInfo) {
    if (Character.UiInfo.CurrentSlot is not BashArrowSlot) {
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
