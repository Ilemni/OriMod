using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AnimLib;
using AnimLib.States;
using AnimLib.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Abilities;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace OriMod.Menus;

using AbilityTextureDict = (
  Dictionary<AbilityMenu.DrawOptions, TextureEntry?> before,
  Dictionary<AbilityMenu.DrawOptions, TextureEntry?> after,
  TextureEntry? litOnMax,
  TextureEntry? rootLighting);

public sealed class AbilityMenu : UIState {
  internal enum DrawOptions {
    /// <summary> Do not draw textures, ability is both locked and out of progression </summary>
    DoNotShow = 0,

    /// <summary> Draw textures for when ability access is in progression </summary>
    Hint = 1,

    /// <summary> Draw textures for when ability is unlocked, but not max level </summary>
    Unlocked = 2,

    /// <summary> Draw textures for when ability reaches max level </summary>
    Maxed = 3
  }

  /// Set this to true to enable this prototype ability view. It's not yet a menu and lacks any functionality.
  public static bool LoadingEnabled => false;

  /// <summary> Used to draw the ability labels during <see cref="PostDraw"/>. </summary>
  private readonly Dictionary<int, UIText> _abilityTexts = new();

  /// <summary> Contains all textures which represent progress for a specified ability. </summary>
  private readonly Dictionary<int, AbilityTextureDict> _abilityMenuTextures = new();

  /// <summary> Ability IDs sorted by the order in which we want to draw them. </summary>
  private readonly int[] _sortedIds = new int[12];

  public static OriAbility Get(int index) => (OriAbility)Main.LocalPlayer.GetState(index);

  internal static DrawOptions ProgressFor(int index) {
    return Get(index) switch {
      { IsMaxLevel: true } => DrawOptions.Maxed,
      { Unlocked: true } => DrawOptions.Unlocked,
      { ShowHintInUI: true } => DrawOptions.Hint,
      _ => DrawOptions.DoNotShow
    };
  }

  private DraggablePanel _panel = null!;

  private LayeredTexture2D _menuTextures = null!;
  private TextureEntry _abilityIconBg = null!;
  private TextureEntry[][] _filledTextures = null!;
  private TextureEntry[] _notchesTextures = null!;

  public override void OnInitialize() {
    InitMenuTextures();
    InitAbilityIconTextures();

    _panel = new DraggablePanel {
      HAlign = 0.5f,
      VAlign = 1,
      Width = StyleDimension.FromPixels(320),
      Height = StyleDimension.FromPixels(480),
      Top = StyleDimension.FromPixels(-100),
      BackgroundColor = Color.Transparent,
      BorderColor = Color.Transparent
    };
    Append(_panel);

    UIText text = new("Abilities") {
      HAlign = 0.5f,
      VAlign = 0f
    };
    _panel.Append(text);

    ReadOnlySpan<(int x, int y)> positions = [
      GetCoordinates(0, 220), // Draw Launch under everything
      GetCoordinates(0, 80),
      GetCoordinates(1, 80),
      GetCoordinates(2, 80),
      GetCoordinates(3, 80),
      GetCoordinates(4, 80),
      GetCoordinates(5, 80),
      GetCoordinates(6, 80),
      GetCoordinates(7, 80),

      GetCoordinates(0, 150),
      GetCoordinates(7, 150),
      GetCoordinates(1, 150),
    ];

    foreach (OriAbility ability in ModContent.GetInstance<OriCharacter>().AbilityStates.OfType<OriAbility>()) {
      int idx = ability switch {
        AirJump => 1,
        Climb => 2,
        WallJump => 3,
        Stomp => 4,
        Burrow => 5,
        Bash => 6,
        Glide => 7,
        Dash => 8,
        ChargeJump => 9,
        ChargeDash => 10,
        WallChargeJump => 11,
        Launch => 0,
        _ => throw new ArgumentException($"Unhandled ability {ability.Name}")
      };

      _sortedIds[idx] = ability.Index;

      var notchedTexture = _notchesTextures[ability.MaxLevel - 1];
      var filledTextures = _filledTextures[ability.MaxLevel - 1];

      (int x, int y) = positions[idx];
      AbilityElement abilityElement = new(ability.Index, _abilityIconBg, notchedTexture,
        filledTextures) {
        HAlign = 0.5f,
        VAlign = 1f,
        Left = StyleDimension.FromPixels(x),
        Top = StyleDimension.FromPixels(y)
      };
      _panel.Append(abilityElement);
      UIText name = new(ability.Name, 0.4f) {
        HAlign = 0.5f,
        VAlign = 1f,
        Left = StyleDimension.FromPixels(x),
        Top = StyleDimension.FromPixels(y + 8)
      };
      _panel.Append(name);
      _abilityTexts[ability.Index] = name;
    }

    return;

    static (int x, int y) GetCoordinates(int rotation, int radius) {
      float angle = (rotation - 2) * MathHelper.PiOver4; // Convert rotation to radians
      int x = (int)(radius * Math.Cos(angle));
      int y = (int)(radius * Math.Sin(angle)) - 140;
      return (x, y);
    }
  }

  private void InitMenuTextures() {
    // Aseprite support goes brrrr
    // Sprite userdata has "upscale" and as such, AnimLib will upscale it to 2x2 when creating the Texture2Ds
    // Layers and group layers with green color are imported as their own Texture2D, indexed as a path
    _menuTextures = OriMod.Instance.Assets
      .Request<LayeredTexture2D>("Menus/AbilityMenu", AssetRequestMode.ImmediateLoad).Value;

    var abilities = ModContent.GetInstance<OriCharacter>().AbilityStates;
    foreach (AbilityState ability in abilities) {
      string name = ability.Name;
      _abilityMenuTextures[ability.Index] = (
        before: new Dictionary<DrawOptions, TextureEntry?> {
          [DrawOptions.Maxed] = _menuTextures.GetValueOrDefault($"{name}/Max/B"),
          [DrawOptions.Unlocked] = _menuTextures.GetValueOrDefault($"{name}/Unlock/B"),
          [DrawOptions.Hint] = _menuTextures.GetValueOrDefault($"{name}/Hint/B")
        },
        after: new Dictionary<DrawOptions, TextureEntry?> {
          [DrawOptions.Maxed] = _menuTextures.GetValueOrDefault($"{name}/Max/A"),
          [DrawOptions.Unlocked] = _menuTextures.GetValueOrDefault($"{name}/Unlock/A"),
          [DrawOptions.Hint] = _menuTextures.GetValueOrDefault($"{name}/Hint/A")
        },
        litOnMax: _menuTextures.GetValueOrDefault($"{name}/Lit"),
        rootLighting: _menuTextures.GetValueOrDefault($"{name}/RootLighting")
      );
    }
  }

  private void InitAbilityIconTextures() {
    LayeredTexture2D dict = OriMod.Instance.Assets
      .Request<LayeredTexture2D>("Menus/AbilityIcons", AssetRequestMode.ImmediateLoad).Value;

    _abilityIconBg = dict["Base"];
    _notchesTextures = [
      dict["Notches 1"],
      dict["Notches 2"],
      dict["Notches 3"],
      dict["Notches 4"],
      dict["Notches 5"]
    ];
    _filledTextures = [
      [dict["Filled"]],
      [dict["Filled 1_2"], dict["Filled"]],
      [dict["Filled 1_3"], dict["Filled 2_3"], dict["Filled"]],
      [dict["Filled 1_4"], dict["Filled 1_2"], dict["Filled 3_4"], dict["Filled"]],
      [dict["Filled 1_5"], dict["Filled 2_5"], dict["Filled 3_5"], dict["Filled 4_5"], dict["Filled"]]
    ];
  }

  private static bool GetHighestProgressTexture(OriAbility ability, [NotNullWhen(true)] out TextureEntry? texture,
    Dictionary<DrawOptions, TextureEntry?> textures) {
    DrawOptions progress = ProgressFor(ability.Index);
    while (progress >= 0) {
      if (textures.TryGetValue(progress, out texture) && texture is not null) {
        return true;
      }

      progress--;
    }

    texture = null;
    return false;
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);
    Vector2 panelPos = _panel.GetDimensions().Position();
    Player player = Main.LocalPlayer;

    // spriteBatch.Draw(_menuTextures["Background"].Value, panelPos, Color.White);
    TextureEntry startB = _menuTextures["Start/B"];
    spriteBatch.Draw(startB, panelPos, Color.White);
    TextureEntry startOrb = _menuTextures["Start/Orb"];
    spriteBatch.Draw(startOrb, panelPos, GetPulseColor(strength: 1.6f, minPulse: 0.5f));

    // Draw portion of tree textures which are under the icons
    foreach (int id in _sortedIds) {
      OriAbility ability = (OriAbility)player.GetState(id);
      if (GetHighestProgressTexture(ability, out TextureEntry? textureEntry, _abilityMenuTextures[ability.Index].before)) {
        spriteBatch.Draw(textureEntry, panelPos, Color.White);
      }
    }
  }

  protected override void DrawChildren(SpriteBatch spriteBatch) {
    base.DrawChildren(spriteBatch);
    PostDraw(spriteBatch);
  }

  private void PostDraw(SpriteBatch spriteBatch) {
    Vector2 panelPos = _panel.GetDimensions().Position();
    Player player = Main.LocalPlayer;
    Color white = Color.White;

    TextureEntry startA = _menuTextures["Start/A"];
    spriteBatch.Draw(startA, panelPos, white);

    // Draw portion of tree textures which are over the icons
    foreach (int id in _sortedIds) {
      OriAbility ability = (OriAbility)player.GetState(id);
      if (GetHighestProgressTexture(ability, out TextureEntry? textureEntry, _abilityMenuTextures[ability.Index].after)) {
        spriteBatch.Draw(textureEntry, panelPos, white);
      }
    }

    // Draw lit textures for maxed abilities
    Color litColor = GetPulseColor(strength: 0.6f, minPulse: 0.5f);
    Color rootLightColor = GetPulseColor(strength: 2.2f, minPulse: 0f, white * 0.8f, floorColor: Color.Transparent);
    foreach (int id in _sortedIds) {
      OriAbility ability = (OriAbility)player.GetState(id);
      if (!ability.IsMaxLevel) {
        continue;
      }

      // Glowing leaves, orbs
      if (_abilityMenuTextures[ability.Index].litOnMax is { } litTex) {
        spriteBatch.Draw(litTex, panelPos, litColor);
      }

      // lighting "cast" by the glowing leaves
      if (_abilityMenuTextures[ability.Index].rootLighting is { } rootLighting) {
        spriteBatch.Draw(rootLighting, panelPos, rootLightColor);
      }
    }

    // Draw text on top of everything
    foreach ((int abilityId, UIText? text) in _abilityTexts) {
      if (Get(abilityId).Unlocked) {
        if (text.Parent is null) {
          _panel.Append(text);
        }

        text.Draw(spriteBatch);
      }
      else {
        text.Remove();
      }
    }
  }

  /// <summary>
  /// Modification of <see cref="Main.MouseTextColorReal"/> pulse effect.
  /// </summary>
  /// <param name="strength">
  /// Strength of the pulse, as a multiplier onto <see cref="Main.mouseTextColor"/>.
  /// <br/> A lower value will have the color stay closer to <paramref name="startColor"/>.
  /// <br/> A higher value will have the color pulse more strongly to <paramref name="floorColor"/>.
  /// <br/> The default behaviour is a value of 1.
  /// </param>
  /// <param name="minPulse">
  /// Minimum pulse value to use, between 0 and 1.
  /// <br/> In some cases <see cref="Main.mouseTextColor"/> may be much lower than desired.
  /// </param>
  /// <param name="startColor">
  /// Color to start the pulse from. Defaults to <see cref="Color.White"/>.
  /// </param>
  /// <param name="floorColor">
  /// Color to pulse towards. Defaults to <see cref="Color.Black"/>.
  /// <br/> <see cref="Color.Transparent"/> may be preferred here.
  /// </param>
  /// <returns></returns>
  internal static Color GetPulseColor(float strength, float minPulse, Color? startColor = null,
    Color? floorColor = null) {
    float pulse = Math.Max(minPulse, float.Lerp(1, Main.mouseTextColor / 255f, strength));
    return Color.Lerp(floorColor ?? Color.Black, startColor ?? Color.White, pulse);
  }
}

public sealed class AbilityElement : UIElement {
  private readonly int _index;
  private readonly TextureEntry _baseTexture;
  private readonly TextureEntry _notchesTexture;
  private readonly TextureEntry[] _filledTextures;

  public AbilityElement(int abilityId, TextureEntry baseTex, TextureEntry notches, TextureEntry[] filled) {
    _index = abilityId;
    _baseTexture = baseTex;
    _notchesTexture = notches;
    _filledTextures = filled;

    Width = StyleDimension.FromPixels(40);
    Height = StyleDimension.FromPixels(40);
  }

  protected override void DrawSelf(SpriteBatch spriteBatch) {
    base.DrawSelf(spriteBatch);
    Vector2 pos = GetDimensions().Position();
    spriteBatch.Draw(_baseTexture, pos, Color.White);
    spriteBatch.Draw(_notchesTexture, pos, Color.White);

    OriAbility ability = AbilityMenu.Get(_index);
    if (ability.Level > 0) {
      Color baseColor = ability.IsMaxLevel
        ? Color.Lerp(Color.White, Color.Cyan, 0.18f)
        : Color.Lerp(Color.White, Color.Yellow, 0.1f);
      Color pulseColor = AbilityMenu.GetPulseColor(3.6f, 0.1f, baseColor, Color.Transparent);

      spriteBatch.Draw(_filledTextures[ability.Level - 1], pos, pulseColor);
    }

    if (IsMouseHovering) {
      Main.instance.MouseText(ability.Name);
    }
  }

  public override void Draw(SpriteBatch spriteBatch) {
    if (AbilityMenu.ProgressFor(_index) is AbilityMenu.DrawOptions.Unlocked or AbilityMenu.DrawOptions.Maxed) {
      base.Draw(spriteBatch);
    }
  }
}
