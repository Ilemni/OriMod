using System;
using AnimLib.Animations;
using AnimLib.Extensions;
using AnimLib.States;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Contains all <see cref="PlayerDrawLayer"/>s this mod creates.
/// </summary>
internal static class OriLayers {
  // PlayerAnim layers

  // Best practices would use a BodyOnly and ArmsOnly, and combine the two
  // Rather than this, where BodyOnly is a modified version of Full
  private const string FullPrimary = "Full/Primary";
  private const string FullSecondary = "Full/Secondary";
  private const string BodyPrimary = "BodyOnly/Primary";
  private const string BodySecondary = "BodyOnly/Secondary";
  private const string Transform = "Transform";
  private const string Feather = "Feather";

  // BashArrow layers
  private const string BashArrowLayer = "Arrow";


  private static bool GetAllDefaultVisibility(PlayerDrawSet drawInfo, out OriPlayer oriPlayer) {
    oriPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
    return oriPlayer.IsOri && oriPlayer.Character.GraphicsEnabledCompat;
  }

  /// <summary>
  /// Draws the Ori sprite.
  /// </summary>
  [UsedImplicitly]
  private sealed class OriPlayerSprite : PlayerDrawLayer {
    public override bool IsHeadLayer => true;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
      GetAllDefaultVisibility(drawInfo, out _) &&
      drawInfo.drawPlayer is { dead: false, invis: false };

    public override Position GetDefaultPosition() =>
      new Between(ModContent.GetInstance<OriBashArrow>(), PlayerDrawLayers.MountFront);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
      Player player = drawInfo.drawPlayer;
      if (player.outOfRange) {
        return;
      }

      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();

      if (!oPlayer.Character.GraphicsEnabledCompat) {
        return;
      }

      State? activeState = oPlayer.ActiveState;

      if (activeState is Transform { Starting: true }) {
        DrawTransform(oPlayer, ref drawInfo);
        return;
      }

      bool drawSeparateHand = player.ItemAnimationActive || player.HeldItem.holdStyle > 0;
      string layerName = drawSeparateHand ? BodyPrimary : FullPrimary;

      MovementStates playerAnim = oPlayer.Character.Move;
      DrawData data = playerAnim.GetDrawData(drawInfo, layerName);

      data.origin.Y += 10 * player.gravDir;
      if (player.portableStoolInfo.IsInUse) {
        data.origin.Y -= 12;
      }

      SetColorAndShader(ref data, oPlayer, oPlayer.SpriteColorPrimary, oPlayer.PrimaryDyeShader, 1);
      drawInfo.DrawDataCache.Add(data);

      playerAnim.SetLayer(ref data, drawSeparateHand ? BodySecondary : FullSecondary);
      bool primaryShaderDoesNotShowOnTransparentSecondaryLayer = true; // TODO: Config option
      if (primaryShaderDoesNotShowOnTransparentSecondaryLayer && oPlayer.SpriteColorSecondary.A > 0) {
        // Prevent the effects of shader (from primary layer) from affecting secondary color by drawing it as primary
        SetColorAndShader(ref data, oPlayer, oPlayer.SpriteColorPrimary, null, 0);
        drawInfo.DrawDataCache.Add(data);
      }

      SetColorAndShader(ref data, oPlayer, oPlayer.SpriteColorSecondary, oPlayer.SecondaryDyeShader, 0);
      drawInfo.DrawDataCache.Add(data);

      if (activeState is Glide) {
        playerAnim.SetLayer(ref data, Feather);
        data.color = Color.White;
        data.shader = 0;
        drawInfo.DrawDataCache.Add(data);
      }

      // Draw separate arm for holding an item
      if (drawSeparateHand &&
          activeState is not (ChargeJump or Launch or Burrow or Stomp or AirJump or Glide
            or NoAbility { ActiveChild: NoAbility.IdleAgainst or NoAbility.WallSlide })) {
        DrawSeparateHand(ref drawInfo, data);
      }


      Burrow burrow = oPlayer.Character.Move.GetChild<Burrow>();
      if (oPlayer.IsLocal && burrow.Unlocked) {
        burrow.DrawEffects(ref drawInfo);
      }
    }

    private static void DrawSeparateHand(ref PlayerDrawSet drawInfo, DrawData data) {
      Player player = drawInfo.drawPlayer;
      OriPlayer oPlayer = player.GetModPlayer<OriPlayer>();
      MovementStates playerAnim = oPlayer.Character.Move;

      playerAnim.SetLayer(ref data, "Arm/Primary", "ArmRotate", 0);
      SetColorAndShader(ref data, oPlayer, oPlayer.SpriteColorPrimary, oPlayer.PrimaryDyeShader, 1);
      data.origin = data.sourceRect!.Value.Size() / 2 + new Vector2(0, 10);

      // Offset arm and held item by position defined in Aseprite file for where the arm should be
      Vector2 armOrigin = playerAnim.SpriteSheet.Points["ArmOrigin"][playerAnim.CurrentFrame.AtlasFrameIndex];
      Vector2 armOffset = armOrigin - data.origin;
      armOffset.X *= player.direction;
      armOffset.Y *= player.gravDir;
      if (player.gravDir < 0) {
        data.origin.Y -= 20;
      }

      data.position += armOffset;
      drawInfo.ItemLocation += armOffset;

      if (player.ItemAnimationActive) {
        data.rotation = player.HeldItem.useStyle switch {
          ItemUseStyleID.Swing or ItemUseStyleID.Thrust or ItemUseStyleID.HoldUp or ItemUseStyleID.GolfPlay
            => // Requires custom animation
            player.itemRotation - (float)Math.PI * 0.25f * player.direction * player.gravDir,
          _ => player.itemRotation
        };
      }
      else {
        int holdStyle = player.HeldItem.holdStyle;
        if (holdStyle <= 0) {
          return;
        }

        data.rotation = holdStyle switch {
          // Use and held, check if held
          ItemHoldStyleID.HoldGolfClub => player.itemRotation - (float)Math.PI * 0.25f * player.direction,
          _ => player.itemRotation
        };
      }

      drawInfo.DrawDataCache.Add(data);
      playerAnim.SetLayer(ref data, "Arm/Secondary", "ArmRotate", 0);
      SetColorAndShader(ref data, oPlayer, oPlayer.SpriteColorSecondary, oPlayer.SecondaryDyeShader, 0);
      drawInfo.DrawDataCache.Add(data);
    }

    private static void SetColorAndShader(ref DrawData data, OriPlayer oPlayer, Color color, ArmorShaderData? shader,
      int dyeSlot) {
      OriConfigClient1 config = OriMod.ConfigClient;
      Player player = oPlayer.Player;

      bool useDye = oPlayer.IsLocal ? config.dyeEnabled : config.dyeEnabledAll;
      if (useDye && shader is not null) {
        color = Color.Lerp(color, shader.GetColor(), oPlayer.DyeColorBlend);
        data.shader = player.dye[dyeSlot].dye;
      }
      else {
        data.shader = 0;
      }

      bool doFlash = player is { immune: true, immuneNoBlink: false } && config.FlashOff;
      if (doFlash) {
        float playerImmuneAlpha = player.immuneAlpha / 255f;
        color = Color.Lerp(color, config.FlashColor, playerImmuneAlpha);
      }

      data.color = color;
    }

    private static void DrawTransform(OriPlayer oPlayer, ref PlayerDrawSet drawInfo) {
      MovementStates playerAnim = oPlayer.Character.Move;
      DrawData data = playerAnim.GetDrawData(drawInfo, OriLayers.Transform);
      data.origin.Y += 10 * oPlayer.Player.gravDir;
      if (oPlayer.Player.portableStoolInfo.IsInUse) {
        data.origin.Y -= 12;
      }

      drawInfo.DrawDataCache.Add(data);
    }
  }

  /// <summary>
  /// Draws the Ori trails.
  /// </summary>
  [UsedImplicitly]
  private sealed class OriPlayerTrail : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
      GetAllDefaultVisibility(drawInfo, out OriPlayer oriPlayer) &&
      !drawInfo.drawPlayer.mount.Active &&
      oriPlayer.ActiveState is not (Abilities.Transform or Burrow);

    public override Position GetDefaultPosition() =>
      new Between(PlayerDrawLayers.FaceAcc, ModContent.GetInstance<OriPlayerSprite>());

    protected override void Draw(ref PlayerDrawSet drawInfo) {
      if (drawInfo.shadow > 0) {
        return;
      }

      Player player = drawInfo.drawPlayer;
      Trail trail = player.GetModPlayer<OriPlayer>().Trail;
      trail.UpdateSegments();
      if (player is { dead: false, invis: false }) {
        trail.ResetNextSegment();
      }

      drawInfo.DrawDataCache.AddRange(trail.TrailDrawDatas);
    }
  }

  /// <summary>
  /// Draws the <see cref="Bash"/> arrow when the player Bashes or Launches.
  /// </summary>
  [UsedImplicitly]
  private sealed class OriBashArrow : PlayerDrawLayer {
    public override void SetStaticDefaults() {
      _spriteSheet = ModContent.Request<AnimSpriteSheet>("OriMod/Animations/BashAnim");
    }

    private static Asset<AnimSpriteSheet> _spriteSheet = null!;

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) =>
      GetAllDefaultVisibility(drawInfo, out OriPlayer oriPlayer) &&
      oriPlayer.ActiveState is Bash or Launch { Starting: true };

    public override Position GetDefaultPosition() =>
      new Between(ModContent.GetInstance<OriPlayerTrail>(), ModContent.GetInstance<OriPlayerSprite>());

    protected override void Draw(ref PlayerDrawSet drawInfo) {
      OriPlayer oPlayer = drawInfo.drawPlayer.GetModPlayer<OriPlayer>();
      AnimSpriteSheet arrowSpriteSheet = _spriteSheet.Value;
      AnimTextureAtlas atlas = arrowSpriteSheet.Atlases[BashArrowLayer];

      Vector2 pos;
      float rotation;
      Rectangle rect;
      switch (oPlayer.ActiveState) {
        case Bash bash:
          bash.GetDrawFields(arrowSpriteSheet, out pos, out rotation, out rect);
          break;
        case Launch launch:
          launch.GetDrawFields(arrowSpriteSheet, out pos, out rotation, out rect);
          break;
        default:
          return;
      }

      pos -= Main.screenPosition;
      Vector2 orig = rect.Size() / 2;
      DrawData data = new(atlas.GetTexture(), pos, rect, Color.White, rotation, orig, 1, SpriteEffects.None) {
        ignorePlayerRotation = true
      };
      drawInfo.DrawDataCache.Add(data);
    }
  }
}
