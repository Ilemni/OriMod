using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace OriMod {
  /// <summary>
  /// Class for containing and updating all <see cref="TrailSegment"/>s on an <see cref="OriPlayer"/>.
  /// </summary>
  public class Trail {
    /// <summary>
    /// Create an instance of <see cref="Trail"/> that will belong to <paramref name="oPlayer"/>.
    /// </summary>
    /// <param name="oPlayer">The <see cref="OriPlayer"/> this <see cref="Trail"/> will belong to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="oPlayer"/> is <see langword="null"/>.</exception>
    internal Trail(OriPlayer oPlayer) {
      if (oPlayer is null) {
        throw new ArgumentNullException(nameof(oPlayer));
      }

      _segments = new TrailSegment[Count];
      int i = 0;
      while (i < Count) {
        _segments[i++] = new TrailSegment(oPlayer);
      }
    }

    /// <summary>
    /// Sets <see cref="_index"/> to the next index in <see cref="_segments"/>, and returns the value.
    /// </summary>
    /// <returns>The updated value of <see cref="_index"/>.</returns>
    private int NextIndex() {
      _index = (_index + 1) % _segments.Length;
      return _index;
    }

    /// <summary>
    /// All <see cref="TrailSegment"/>s in this <see cref="Trail"/>.
    /// </summary>
    private readonly TrailSegment[] _segments;

    /// <summary>
    /// Current <see cref="_segments"/> index. Used for <see cref="TrailSegment.Reset"/>.
    /// </summary>
    private int _index;

    internal bool hasDrawnThisFrame;

    /// <summary>
    /// Number of segments in a trail.
    /// </summary>
    public static int Count => 26;

    /// <summary>
    /// Call <see cref="TrailSegment.Tick"/> on each <see cref="TrailSegment"/>.
    /// <para>This keeps the opacity of the trail appearing consistent.</para>
    /// </summary>
    public void UpdateSegments() {
      foreach (TrailSegment segment in _segments) {
        segment.Tick();
      }
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/> of all <see cref="DrawData"/>s from this trail.
    /// </summary>
    public IEnumerable<DrawData> TrailDrawDatas => _segments.Select(segment => segment.GetDrawData());

    /// <summary>
    /// Calls <see cref="TrailSegment.Reset"/> on the next segment.
    /// </summary>
    internal void ResetNextSegment() {
      _segments[NextIndex()].Reset();
    }
  }
}
