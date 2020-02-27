using System;
using System.Linq;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;

namespace OriMod {
  public class TileHitbox {
    public TileHitbox(params Point[] template) {
      if (template is null) {
        throw new ArgumentNullException(nameof(template));
      }
      if (template.Length == 0) {
        throw new ArgumentException("Template must not have a length of 0", nameof(template));
      }

      Template = template;
      points = new Point[Template.Length];
    }

    /// <summary>
    /// Tile positions of the hitbox. This is a newly allocated array
    /// </summary>
    public Point[] Points => points.ToArray();
    private readonly Point[] points;

    /// <summary>
    /// Shape of the hitbox, used for calculating Points
    /// </summary>
    public Point[] Template { get; }

    /// <summary>
    /// Updates the position of the hitbox based on the given world position.
    /// </summary>
    /// <param name="origin">Position to use</param>
    public void UpdateHitbox(Vector2 origin) => UpdateHitbox(origin.ToTileCoordinates());

    /// <summary>
    /// Updates the position of the hitbox based on the given tile position.
    /// </summary>
    /// <param name="origin">Position to use</param>
    public void UpdateHitbox(Point origin) {
      for (int i = 0; i < points.Length; i++) {
        points[i] = Template[i].Add(origin);
      }
    }
  }
}

