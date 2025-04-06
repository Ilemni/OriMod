using AnimLib.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Animations;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace OriMod;

/// <summary>
/// For drawing a trail behind the player.
/// </summary>
public sealed class TrailSegment(OriCharacter ori) {
  private const string LayerName = "AfterImage";
  private float Alpha => _startAlpha * _timeLeft / Trail.Count;
  private OriAnimation Anim => ori.GetAnimation<OriAnimation>();
  private Texture2D Texture => Anim.SpriteSheet.GetAtlas(LayerName).Texture;
  private Vector2 _position;
  private Rectangle _tile;
  private byte _timeLeft;
  private float _startAlpha = 1;
  private float _rotation;
  private SpriteEffects _effect;
  private Color _color;
  private int _dye;

  /// <summary>
  /// Resets various attributes to be based on the player's current attributes.
  /// </summary>
  public void Reset() {
    if (!Anim.SpriteSheet.HasLayer("AfterImage")) {
      return;
    }

    Player player = Anim.Player;

    _position = player.Center;
    _tile = Anim.GetSourceRect(LayerName);
    _rotation = Anim.SpriteRotation;

    _startAlpha = player.velocity.LengthSquared() * 0.005f;
    if (_startAlpha > 0.16f) {
      _startAlpha = 0.16f;
    }

    _timeLeft = (byte)Trail.Count;

    _effect = SpriteEffects.None;
    if (player.direction == -1) {
      _effect |= SpriteEffects.FlipHorizontally;
    }

    if (player.gravDir < 0) {
      _effect |= SpriteEffects.FlipVertically;
    }

    Item dyeItem = Anim.Player.dye[1];
    var shaderColor = GameShaders.Armor.GetShaderFromItemId(dyeItem.netID)?.GetColor();
    Color skinColor = Anim.Player.skinColor;
    Color baseColor = shaderColor is { } sc
      ? Color.Lerp(skinColor, sc, Anim.Character.DyeColorBlend)
      : skinColor;
    _color = Anim.Player.GetImmuneAlphaPure(baseColor, 0);
    _dye = dyeItem.dye;
  }

  /// <summary>
  /// Decreases Alpha by a fixed amount.
  /// </summary>
  public void Tick() {
    if (_timeLeft > 0) {
      _timeLeft--;
    }
  }

  /// <summary>
  /// Decreases Alpha to zero.
  /// </summary>
  public void Decay() {
    _timeLeft = 0;
  }

  /// <summary>
  /// Gets the Trail <see cref="DrawData"/> for this <see cref="OriCharacter"/>.
  /// </summary>
  public DrawData GetDrawData() {
    Vector2 pos = _position - Main.screenPosition;
    Color color = _color * Alpha;
    Rectangle rect = _tile;
    Vector2 origin = new(rect.Width / 2f, rect.Height / 2f + 5 * Anim.Player.gravDir);

    DrawData data = new(Texture, pos, rect, color, _rotation, origin, 1, _effect) {
      ignorePlayerRotation = true,
      shader = _dye
    };
    return data;
  }

  public override string ToString() => $"tile:{_tile}, rotation:{_rotation}, effect:{_effect}";
}
