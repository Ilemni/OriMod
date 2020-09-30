using System;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;

namespace OriMod {
  /// <summary>
  /// Primarily used for <see cref="Abilities.Burrow"/>, stores an array of points as a template, and retrieves tiles of that template when updated.
  /// </summary>
  public class TileHitbox {
    /// <summary>
    /// Instantiate a <see cref="TileHitbox"/> with local-space <paramref name="template"/>.
    /// </summary>
    /// <param name="template"><see cref="Template"/>. Must have at least 1 item.</param>
    public TileHitbox(params Point[] template) {
      if (template is null) {
        throw new ArgumentNullException(nameof(template));
      }
      if (template.Length == 0) {
        throw new ArgumentException("Template must have at least one item.", nameof(template));
      }

      Template = template;
      Points = new Point[Template.Length];
      UpdateHitbox(Point.Zero);
    }

    /// <summary>
    /// Current world position of points in the hitbox, in tile coordinates.
    /// </summary>
    public Point[] Points { get; }

    /// <summary>
    /// Local position of points in the hitbox, in tile coordinates.
    /// </summary>
    public Point[] Template { get; }

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
        Points[i] = Template[i].Add(origin);
      }
    }
  }
}

