using System;
using AnimLib;
using Microsoft.Xna.Framework;
using OriMod.Abilities;
using Terraria.ModLoader.IO;

namespace OriMod;

public static class BackCompatability {
  private const string AbilitiesTagName = "AnimLibAbilities";

  public static void LoadSave_From_3_2_2_0(OriCharacter ori, TagCompound tag) {
    if (!tag.TryGet(AbilitiesTagName, out TagCompound abilities)) {
      return;
    }

    LoadOriActive_From_3_2_2_0(ori, tag);
    LoadAbilities_From_3_2_2_0(ori, abilities);
    LoadColors_From_3_2_2_0(ori, tag);
  }

  private static void LoadOriActive_From_3_2_2_0(OriCharacter ori, TagCompound tag) {
    if (tag.TryGet("OriSet", out bool set) && set) {
      ori.Enable();
    }
  }

  private static void LoadAbilities_From_3_2_2_0(OriCharacter ori, TagCompound abilities) {
    QuickLoad<AirJump>();
    QuickLoad<Burrow>();
    QuickLoad<Climb>();
    QuickLoad<Glide>();
    QuickLoad<Stomp>();
    QuickLoad<WallJump>();
    QuickLoad<Bash>();
    if (TryQuickLoad<Dash>(out int dashLevel)) {
      ori.GetState<ChargeDash>().Level = Math.Max(dashLevel - 2, 0);
    }

    if (TryQuickLoad<ChargeJump>(out int chargeJumpLevel)) {
      ori.GetState<WallChargeJump>().Level = chargeJumpLevel >= 2 ? 1 : 0;
      ori.GetState<Launch>().Level = Math.Max(chargeJumpLevel - 2, 0);
    }

    return;

    void QuickLoad<T>() where T : OriAbility, new() {
      if (abilities.TryGet(typeof(T).Name, out TagCompound ability)) {
        ori.GetState<T>().Level = ability.Get<int>("Level");
      }
    }

    bool TryQuickLoad<T>(out int level) where T : OriAbility, new() {
      level = 0;
      if (!abilities.TryGet(typeof(T).Name, out TagCompound ability)) {
        return false;
      }

      ori.GetState<T>().Level = level = ability.Get<int>("Level");
      return true;
    }
  }

  private static void LoadColors_From_3_2_2_0(OriCharacter ori, TagCompound tag) {
    AnimCharacterStyle style = ori.Style;
    if (tag.TryGet("Color1", out Color color)) {
      style.SkinColor = color;
    }

    if (tag.TryGet("Color2", out color)) {
      style.HairColor = color;
    }

    if (tag.TryGet("DyeColLerp", out float dyeColorBlend)) {
      ori.DyeColorBlend = dyeColorBlend;
    }
  }
}
