using System;
using AnimLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using OriMod.Animations;
using OriMod.Skins.Slots;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace OriMod.DrawLayers;

public sealed class OriFrontArmLayer : AnimPlayerDrawLayer<OriCharacter, OriAnimation> {
  private const string ArmPrimary = "Primary/Arm";
  private const string ArmSecondary = "Secondary/Arm";

  // Draw with head layer in specific cases, hairstyle button
  public override bool IsHeadLayer => true;

  public override Position GetDefaultPosition() =>
    new Between(PlayerDrawLayers.SolarShield, PlayerDrawLayers.ArmOverItem);

  public override bool GetDefaultVisibility(PlayerDrawSet drawInfo, OriCharacter ori, OriAnimation anim) {
    if (!base.GetDefaultVisibility(drawInfo, ori, anim)) {
      return false;
    }

    if (drawInfo.headOnlyRender) {
      return ori.UiInfo is { IsDrawingInUI: true, CategoryIndex: 2 };
    }

    if (ori.UiInfo is { IsDrawingInUI: true, CurrentSlot: { } slot }) {
      return slot is OriSpriteSlot;
    }

    // Drawing hairstyle
    return ori is {
      Player: { outOfRange: false, dead: false, invis: false },
      ActiveState: not Transform { Starting: true }
    };
  }

  protected override void Draw(ref PlayerDrawSet drawInfo, OriCharacter ori, OriAnimation anim) {
    Player player = drawInfo.drawPlayer;

    // avoid offset in hairstyle menu
    Vector2 headOffset = drawInfo.headOnlyRender && ori.UiInfo.CategoryIndex == 2
      ? Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height]
      : Vector2.Zero;

    if (!anim.SpriteSheet.HasLayer(ArmPrimary)) {
      return;
    }

    DrawData data = anim.GetDrawData(ref drawInfo, ArmPrimary);
    data.position += headOffset;


    if (ShouldDrawHoldingItem(ref drawInfo)) {
      // Note: most of the code below this method is in early testing,
      // as I try to figure out best practice for drawing arms from aseprite sprite
      // and get a decent-looking position representing where the shoulder would be per-frame.
      DrawHoldingItem(ref drawInfo, anim);
      DebugDrawVanillaArms(ref drawInfo, data.position);
      return;
    }

    // Draw both primary and secondary layers for the arm
    data = anim.GetDrawData(ref drawInfo, ArmPrimary);
    data.position += headOffset;
    drawInfo.DrawDataCache.Add(data);

    data = anim.GetDrawData(ref drawInfo, ArmSecondary);
    data.position += headOffset;
    drawInfo.DrawDataCache.Add(data);
  }

  private static void DrawHoldingItem(ref PlayerDrawSet drawInfo, OriAnimation playerAnim) {
    Player player = drawInfo.drawPlayer;

    // Offset arm and held item by position defined in Aseprite file for where the arm should be
    Vector2 armOffset = playerAnim.GetPoint("ArmOrigin") - player.Size;
    armOffset.X *= player.direction;
    armOffset.Y *= player.gravDir;
    if (player.gravDir < 0) {
      armOffset.Y -= 20;
    }

    drawInfo.ItemLocation += armOffset;

    // Decide data.rotation
    Item heldItem = player.HeldItem;
    float rotation = player.ItemAnimationActive
      ? heldItem.useStyle switch {
        ItemUseStyleID.Swing or ItemUseStyleID.Thrust or ItemUseStyleID.HoldUp
          => player.itemRotation - MathF.PI * 0.25f * player.direction * player.gravDir,
        // Requires custom animation
        ItemUseStyleID.GolfPlay
          => MathHelper.PiOver2 * player.direction * player.gravDir + drawInfo.compositeFrontArmRotation,
        _ => player.itemRotation
      }
      : heldItem.holdStyle switch {
        // Use and held, check if held
        ItemHoldStyleID.HoldGolfClub
          => MathHelper.PiOver2 * player.direction * player.gravDir + drawInfo.compositeFrontArmRotation,
        _ => player.itemRotation
      };

    Rectangle frontArmFrame = drawInfo.compFrontArmFrame;
    int armFrameX = frontArmFrame.X / frontArmFrame.Width;
    int armFrameY = frontArmFrame.Y / frontArmFrame.Height;
    int frameIndex = (armFrameX, armFrameY) switch {
      (armFrameX: 0, armFrameY: 3 or 4 or 5) => 1,
      (armFrameX: 7, armFrameY: _) => armFrameY + 2,
      _ => 0
    };

    // Draw both primary and secondary layers for the arm
    if (playerAnim.TryGetDrawData(ref drawInfo, ArmPrimary, "ArmRotate", frameIndex, out DrawData data)) {
      // data.position -= armOffset;
      // data.rotation = rotation;
      // data.origin -= armOffset;
      drawInfo.DrawDataCache.Add(data);
    }

    if (playerAnim.TryGetDrawData(ref drawInfo, ArmSecondary, "ArmRotate", frameIndex, out data)) {
      // data.position -= armOffset;
      // data.rotation = rotation;
      // data.origin -= armOffset;
      drawInfo.DrawDataCache.Add(data);
    }
  }

  private static void DebugDrawVanillaArms(ref PlayerDrawSet drawInfo, Vector2 position) {
    // Debug test vars, get vanilla rect for arm
    var textures = TextureAssets.Players;
    float frontRotation = drawInfo.drawPlayer.bodyRotation + drawInfo.compositeFrontArmRotation;
    float backRotation = drawInfo.drawPlayer.bodyRotation + drawInfo.compositeBackArmRotation;
    Texture2D tex = textures[drawInfo.skinVar, 7].Value;

    Rectangle frontArmFrame = drawInfo.compFrontArmFrame;
    Rectangle backArmFrame = drawInfo.compBackArmFrame;

    // int frontArmFrameX = frontArmFrame.X / frontArmFrame.Width;
    // int frontArmFrameY = frontArmFrame.Y / frontArmFrame.Height;
    // Main.NewText($"[{frontArmFrameX}, {frontArmFrameY}]");

    DrawData data = new(tex, position + new Vector2(0, -80),
      frontArmFrame, new Color(192, 255, 192), frontRotation, drawInfo.bodyVect, 1,
      drawInfo.playerEffect);

    drawInfo.DrawDataCache.Add(data);
    data.sourceRect = backArmFrame;
    data.rotation = backRotation;
    drawInfo.DrawDataCache.Add(data);

    data.sourceRect = frontArmFrame;
    data.rotation = frontRotation;
    data.position = position + new Vector2(0, -112);
    data.texture = textures[drawInfo.skinVar, 8].Value;
    data.color = new Color(255, 192, 192);
    drawInfo.DrawDataCache.Add(data);

    data.sourceRect = backArmFrame;
    data.rotation = backRotation;
    drawInfo.DrawDataCache.Add(data);

    data.sourceRect = frontArmFrame;
    data.rotation = frontRotation;
    data.texture = textures[drawInfo.skinVar, 9].Value;
    data.color = new Color(192, 192, 255);
    drawInfo.DrawDataCache.Add(data);

    data.sourceRect = backArmFrame;
    data.rotation = backRotation;
    drawInfo.DrawDataCache.Add(data);
  }

  private static void DrawArm(ref PlayerDrawSet drawInfo) {
    float rotation = drawInfo.drawPlayer.bodyRotation + drawInfo.compositeFrontArmRotation;
    Vector2 drawPos = drawInfo.Position - Main.screenPosition;
    Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
    Vector2 vector =
      new Vector2(
        // ReSharper disable PossibleLossOfFraction
        (int)(drawPos.X - bodyFrame.Width / 2 + drawInfo.drawPlayer.width / 2),
        // ReSharper restore PossibleLossOfFraction
        (int)(drawPos.Y + drawInfo.drawPlayer.height - bodyFrame.Height + 4f)) +
      drawInfo.drawPlayer.bodyPosition +
      bodyFrame.Size() / 2;
    Vector2 vector2 = Main.OffsetsPlayerHeadgear[bodyFrame.Y / bodyFrame.Height];
    vector2.Y -= 2f;
    vector += vector2 * -drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically).ToDirectionInt();
    Texture2D tex = OriMod.Instance.Assets.Request<Texture2D>("Animations/Arms", AssetRequestMode.ImmediateLoad)
      .Value;
    DrawData data =
      new(tex, vector,
        drawInfo.compFrontArmFrame, drawInfo.drawPlayer.skinColor, rotation, drawInfo.bodyVect, 1f,
        drawInfo.playerEffect) {
        shader = drawInfo.skinDyePacked
      };


    Player player = drawInfo.drawPlayer;
    OriCharacter ori = player.GetCharacter<OriCharacter>();
    OriAnimation anim = ori.GetAnimation<OriAnimation>();

    // Offset arm and held item by position defined in Aseprite file for where the arm should be
    Vector2 armOrigin = anim.GetPoint("ArmOrigin");
    Vector2 armOffset = armOrigin - new Vector2(30, 50);
    armOffset.X *= player.direction;

    // data.origin = data.sourceRect!.Value.Size() / 2 + new Vector2(0, 10);
    data.position += armOffset;
    drawInfo.ItemLocation += armOffset;

    drawInfo.DrawDataCache.Add(data);
  }

  private static bool ShouldDrawHoldingItem(ref readonly PlayerDrawSet drawInfo) {
    Player player = drawInfo.drawPlayer;
    return
      (player.ItemAnimationActive || player.HeldItem.holdStyle > 0) &&
      PlayerDrawLayers.HeldItem.Visible &&
      player.CanVisuallyHoldItem(player.HeldItem);
  }
}
