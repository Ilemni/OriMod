using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod;

// ReSharper disable IdentifierTypo
/// <summary>
/// Primarily used for <see cref="Abilities.Burrow"/>, stores an array of points as a template, and retrieves tiles of that template when updated.
/// </summary>
public sealed class TileHitbox {
  /// <summary>
  /// Instantiate a <see cref="TileHitbox"/> with local-space <paramref name="template"/>.
  /// </summary>
  /// <param name="template"><see cref="Template"/>. Must have at least 1 item.</param>
  private TileHitbox(params Point[] template) {
    ArgumentNullException.ThrowIfNull(template);
    if (template.Length == 0) {
      throw new ArgumentException("Template must have at least one item.", nameof(template));
    }

    Template = template;
    Points = new Point[Template.Length];
    UpdateHitbox(Point.Zero);
  }

  public TileHitbox(params ReadOnlySpan<(int x, int y)> template) : this(ToPoints(template)) {
  }

  private static Point[] ToPoints(ReadOnlySpan<(int x, int y)> span) {
    var result = new Point[span.Length];
    for (int i = 0; i < span.Length; i++) {
      (int x, int y) = span[i];
      result[i] = new Point(x, y);
    }

    return result;
  }

  /// <summary>
  /// Current world position of points in the hitbox, in tile coordinates.
  /// </summary>
  public Point[] Points { get; }

  /// <summary>
  /// Local position of points in the hitbox, in tile coordinates.
  /// </summary>
  public Point[] Template { get; }

  public bool Contains(Point point) => Points.Contains(point);

  public bool Any(Func<Tile, bool> func) => Points.Any(p => func(Main.tile[p.X, p.Y]));

  public void GetCollisions(Func<Tile, bool> checkFunc, out bool x, out bool y) {
    x = y = false;
    for (int i = 0; i < Points.Length; i++) {
      Point point = Points[i];
      if (!checkFunc(Main.tile[point.X, point.Y])) {
        continue;
      }

      Point templatePoint = Template[i];
      x |= templatePoint.X != 0;
      y |= templatePoint.Y != 0;
      if (x && y) {
        return;
      }
    }
  }

  /// <summary>
  /// Updates the position of the hitbox based on the given world position.
  /// </summary>
  /// <param name="origin">World-space position to use.</param>
  public void UpdateHitbox(Vector2 origin) => UpdateHitbox(origin.ToTileCoordinates());

  /// <summary>
  /// Updates the world position of the hitbox based on the given tile position.
  /// </summary>
  /// <param name="origin">Tile-space position to use.</param>
  public void UpdateHitbox(Point origin) {
    for (int i = 0; i < Points.Length; i++) {
      Points[i] = Template[i] + origin;
    }
  }
}
