using AnimLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OriMod.Animations;
using Terraria;
using Terraria.DataStructures;

namespace OriMod;

/// <summary>
/// For drawing a trail behind the player.
/// </summary>
public class TrailSegment {
  /// <summary>
  /// Creates a <see cref="TrailSegment"/> that belongs to <paramref name="oPlayer"/>.
  /// </summary>
  /// <param name="oPlayer"><see cref="OriPlayer"/> this trail will belong to.</param>
  internal TrailSegment(OriPlayer oPlayer) => _oPlayer = oPlayer;

  private readonly OriPlayer _oPlayer;
  private Vector2 _position;
  private PointByte _tile;
  private byte _timeLeft;
  private float _startAlpha = 1;
  private float _rotation;
  private SpriteEffects _effect;

  /// <summary>
  /// Resets various attributes to be based on the player's current attributes.
  /// </summary>
  public void Reset() {
    Player player = _oPlayer.Player;
    OriAnimationController anim = _oPlayer.Animations;

    _position = player.Center;
    _tile = anim.playerAnim.CurrentFrame.tile;
    _rotation = anim.SpriteRotation;

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
  /// Gets the Trail <see cref="DrawData"/> for this <see cref="OriPlayer"/>.
  /// </summary>
  public DrawData GetDrawData() {
    Vector2 pos = _position - Main.screenPosition;
    PointByte spriteSize = PlayerAnim.Instance.spriteSize;
    Rectangle rect = new(_tile.x * spriteSize.x, _tile.y * spriteSize.y, spriteSize.x, spriteSize.y);
    float alpha = _startAlpha * _timeLeft / Trail.Count;
    Color color = _oPlayer.SpriteColorPrimary * alpha;
    Vector2 origin = new(rect.Width / 2f, rect.Height / 2f + 5 * _oPlayer.Player.gravDir);

    DrawData data = new(OriTextures.Instance.trail, pos, rect, color, _rotation, origin, 1, _effect, 0);
    data.ignorePlayerRotation = true;
    return data;
  }

  public override string ToString() => $"tile:{_tile}, rotation:{_rotation}, effect:{_effect}";
}
