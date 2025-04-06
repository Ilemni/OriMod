using System;
using System.Collections.Generic;
using AnimLib;
using AnimLib.Animations;
using AnimLib.States;
using OriMod.Skins.Slots;

namespace OriMod.Animations;

/// <summary>
/// <list type="table">
/// <listheader>
/// <term>Animations</term><description>[Fallback if missing] Condition for animation to play.</description>
/// </listheader>
/// <item>
///   <term>Idle</term>
///   <description>When the player is grounded and not moving.</description>
/// </item>
/// <item>
///   <term>IdleAgainst</term>
///   <description>[Idle] When the player is grounded, not moving, and standing next to and towards a wall.</description>
/// </item>
/// <item>
///   <term>LookUp</term>
///   <description>When the player is grounded, not moving, and looking up via W or charge key.</description>
/// </item>
/// <item>
///   <term>LookUpStart</term>
///   <description>[LookUp] Plays at the start of LookUp animation above.</description>
/// </item>
/// <item>
///   <term>LookUpEnd</term>
///   <description>[LookUpStart, Reversed] Plays at the end of LookUp animation above.</description>
/// </item>
/// <item>
///   <term>Crouch</term>
///   <description>When the player is grounded, not moving, and looking down via S key.</description>
/// </item>
/// <item>
///   <term>CrouchStart</term>
///   <description>[Crouch] Plays at the start of Crouch animation.</description>
/// </item>
/// <item>
///   <term>CrouchEnd</term>
///   <description>[CrouchStart, Reversed] Plays at the start of Crouch animation.</description>
/// </item>
/// <item>
///   <term>Running</term>
///   <description>When the player is grounded and moving.</description>
/// </item>
/// <item>
///   <term>Walking</term>
///   <description>[Running] When Running and the player's horizontal speed is relatively low.</description>
/// </item>
/// </list>
/// </summary>
public sealed class OriAnimation : SkinAnimation {
  public override OriCharacter Character => (OriCharacter)base.Character;

  protected override IReadOnlyCollection<(string optional, string fallback)> RegisterFallbackTags() => [
    ("Default", "Idle"),
    ("IdleAgainst", "Idle"),

    ("LookUpStart", "LookUp"),
    ("LookUpEnd", "LookUpStart"),

    ("CrouchStart", "Crouch"),
    ("CrouchEnd", "CrouchStart"),

    ("GlideStart", "Glide"),
    ("GlideIdle", "Glide"),
    ("GlideEnd", "GlideStart"),

    ("AirJumpStart", "AirJump"),
    ("AirJumpEnd", "AirJumpStart"),
  ];

  protected override AnimationOptions? UpdateAnimation() => Character.MoveStates.GetAnimationOptions();

  public override void UpdateUIAnimation(AnimUiInfo uiInfo) {
    const int glideCategory = 8;
    // if (drawInfo.headOnlyRender && categoryIndex == 2) {
    //   // UIHairStyleButton
    //   UIAnimation(new AnimationOptions("Idle", frameIndex: 0), counter);
    //   return;
    // }

    if (uiInfo.CurrentSlot is OriSpriteSlot) {
      UIAnimation(new AnimationOptions("Running"), uiInfo.AnimationCounter);
    }

    bool isUiGlide = uiInfo.CategoryIndex is glideCategory;
    bool wasUiGlide = uiInfo.LastCategoryIndex is glideCategory;
    if (isUiGlide || wasUiGlide) {
      GlideUIAnimation(isUiGlide, uiInfo.Animated, uiInfo.CategoryAnimationCounter);
      return;
    }

    if (!uiInfo.Animated) {
      UIAnimation(new AnimationOptions("Idle"), uiInfo.AnimationCounter);
    }
  }

  private void GlideUIAnimation(bool isUiGlide, bool animated, int counter) {
    if (isUiGlide) {
      if (!animated) {
        UIAnimation(new AnimationOptions("GlideIdle"), counter);
        return;
      }
      ReadOnlySpan<AnimationOptions> glideOptions = [
        new("GlideStart"),
        new("GlideIdle")
      ];
      UIAnimationSequence(glideOptions, counter, syncLastTag: true);
      return;
    }

    if (!animated) {
      UIAnimation(new AnimationOptions("Idle"), counter);
      return;
    }

    AnimationOptions exitTag = HasTag("GlideEnd")
      ? new AnimationOptions("GlideEnd")
      : new AnimationOptions("GlideStart") { IsReversed = true };

    ReadOnlySpan<AnimationOptions> wasGlideOptions = [
      exitTag,
      new("Idle")
    ];

    UIAnimationSequence(wasGlideOptions, counter, syncLastTag: true);
  }
}
