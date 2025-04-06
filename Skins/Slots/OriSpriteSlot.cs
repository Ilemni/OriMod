using AnimLib;
using AnimLib.Extensions;
using AnimLib.Skins;
using AnimLib.States;
using Microsoft.Xna.Framework;
using OriMod.Animations;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace OriMod.Skins.Slots;

/// <summary>
/// <list type="table">
/// <listheader>
///   <term>Layers</term><description>Usage</description>
/// </listheader>
/// <item>
///   <term>Primary/Body</term>
///   <description>Main layer</description>
/// </item>
/// <item>
///   <term>Primary/Arm</term>
///   <description>This layer is drawn when the vanilla arm would NOT be drawn.</description>
/// </item>
/// <item>
///   <term>Secondary/N/Body</term>
///   <description>
///     Where N ranges from 1 to the number of secondary options.
///     <br/> Pixel colors do not matter, only pixel alpha matters.
///     <br/> Layer properties should have <c>copyColor:Primary/Body,ignoreOpacity</c>
///   </description>
/// </item>
/// <item>
///   <term>Secondary/N/Arm</term>
///   <description>Same as above, with layer props <c>copyColor:Primary/Arm,ignoreOpacity</c></description>
/// </item>
/// <item>
///   <term>Eye Pupil</term>
///   <description>Draws the eye pupil.</description>
/// </item>
/// <item>
///   <term>Eye Sclera</term>
///   <description>Draws the eye sclera.</description>
/// </item>
/// <item>
///   <term>Feather</term>
///   <description>Draws the feather when the player is using the Glide ability.</description>
/// </item>
/// <item>
///   <term>Transform</term>
///   <description>Draws the player transforming from human to Ori, without applying player color.</description>
/// </item>
/// <item>
///   <term>AfterImage</term>
///   <description>Draws the Ori sprite's trail.</description>
/// </item>
/// </list>
/// <para/> <b>See <see cref="OriAnimation"/> for the list of required and optional animations.</b>
/// </summary>
public sealed class OriSpriteSlot : SkinSlot<OriCharacter, OriAnimation> {
  public override Skin DefaultSkin => ModContent.GetInstance<DefaultOriSkin>();
  public override int SortOrder => 0;

  public OriSpriteSlot() {
    LayerColors.Add("Primary/Body", PrimaryBodyColor);
    LayerColors.Add("Primary/Arm", PrimaryArmColor);
    LayerColors.Add("Secondary/Body", SecondaryBodyColor);
    LayerColors.Add("Secondary/Arm", SecondaryArmColor);
    LayerColors.Add("Eye Pupil", EyePupilColor);
    LayerColors.Add("Eye Sclera", EyeScleraColor);
    LayerColors.Add("Feather", FeatherColor);

    RequiredLayers.AddRange([
      "Primary/Body",
      "Secondary/Body",
      "Eye Pupil",
      "Eye Sclera",
      "Feather"
    ]);

    RequiredAnimations.AddRange([
      "Idle",
      "Run",
      "Jump",
      "Fall",
      "Glide"
    ]);
  }

  private static (Color color, int shader) PrimaryBodyColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorHead, player.skinColor, 0.4f);
    return GetColorAndShader(layer, in drawInfo, anim, color, player.dye[0]);
  }

  private static (Color color, int shader) PrimaryArmColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorBodySkin, player.skinColor, 0.6f);
    return GetColorAndShader(layer, in drawInfo, anim, color, player.dye[0]);
  }

  private static (Color color, int shader) SecondaryBodyColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorShirt, player.shirtColor, 0.4f);
    return GetColorAndShader(layer, in drawInfo, anim, color, player.dye[1]);
  }

  private static (Color color, int shader) SecondaryArmColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorShirt, player.shirtColor, 0.6f);
    return GetColorAndShader(layer, in drawInfo, anim, color, player.dye[1]);
  }

  private static (Color color, int shader) EyePupilColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorEyes, player.eyeColor, 0.6f);
    return GetColorAndShader(layer, in drawInfo, anim, color);
  }

  private static (Color color, int shader) EyeScleraColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorUnderShirt, player.underShirtColor, 0.2f);
    return GetColorAndShader(layer, in drawInfo, anim, color);
  }

  private static (Color color, int shader) FeatherColor
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim) {
    Player player = drawInfo.drawPlayer;
    Color color = Color.Lerp(drawInfo.colorPants, player.pantsColor, 0.3f);
    return GetColorAndShader(layer, in drawInfo, anim, color);
  }

  private static (Color color, int shader) GetColorAndShader
    (string layer, ref readonly PlayerDrawSet drawInfo, SkinAnimation anim, Color color, Item? dyeItem = null) {
    Player player = drawInfo.drawPlayer;
    OriCharacter ori = player.GetCharacter<OriCharacter>();
    OriConfigClient1 config = OriMod.ConfigClient;
    bool useDye = ori.IsLocal ? config.dyeEnabled : config.dyeEnabledAll;

    bool ignoreColor = anim.SpriteSheet.IgnoreColor(layer, anim.CurrentFrame);
    if (ignoreColor) {
      color = Color.White;
    }

    int shaderType = 0;

    ArmorShaderData? shader = !ignoreColor && useDye && dyeItem is { dye: > 0 }
      ? GameShaders.Armor.GetShaderFromItemId(dyeItem.netID)
      : null;
    if (shader is not null) {
      color = Color.Lerp(color, shader.GetColor(), ori.DyeColorBlend);
      shaderType = dyeItem!.dye;
    }

    // todo: consider player.GetImmuneAlpha
    bool doFlash = player is { immune: true, immuneNoBlink: false } && !config.FlashOff;
    Color tempColor = color;
    if (doFlash) {
      float playerImmuneAlpha = player.immuneAlpha / 255f;
      tempColor = Color.Lerp(color, config.FlashColor, playerImmuneAlpha);
    }

    color = player.GetImmuneAlphaPure(tempColor, 0);

    return (color, shaderType);
  }
}
