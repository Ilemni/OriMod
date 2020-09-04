using System;
using System.Collections.Generic;
using Terraria;
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
    /// <param name="segmentCount">Number of sprites to use for the trail.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException">Expected value of <see langword="1"/> or greater.</exception>
    internal Trail(OriPlayer oPlayer, int segmentCount) {
      if (oPlayer is null) {
        throw new ArgumentNullException(nameof(oPlayer));
      }
      if (segmentCount < 1) {
        throw new ArgumentOutOfRangeException(nameof(segmentCount), "Expected value of 1 or greater.");
      }

      segments = new TrailSegment[segmentCount];
      int i = 0;
      while (i < segmentCount) {
        segments[i++] = new TrailSegment(oPlayer);
      }
    }

    /// <summary>
    /// Sets <see cref="index"/> to the next index in <see cref="segments"/>, and returns the value.
    /// </summary>
    /// <returns>The updated value of <see cref="index"/>.</returns>
    private int NextIndex() {
      index = (index + 1) % segments.Length;
      return index;
    }

    /// <summary>
    /// All <see cref="TrailSegment"/>s in this <see cref="Trail"/>.
    /// </summary>
    private readonly TrailSegment[] segments;

    /// <summary>
    /// Current <see cref="segments"/> index. Used for <see cref="TrailSegment.Reset"/>.
    /// </summary>
    private int index = 0;

    private double lastTrailResetTime;

    internal double lastTrailDrawTime;

    /// <summary>
    /// Call <see cref="TrailSegment.Tick"/> on each <see cref="TrailSegment"/>.
    /// <para>This keeps the opacity of the trail appearing consistent.</para>
    /// </summary>
    public void UpdateSegments() {
      foreach (var segment in segments) {
        segment.Tick();
      }
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/> of all <see cref="DrawData"/>s from this trail.
    /// </summary>
    public IEnumerable<DrawData> TrailDrawDatas {
      get {
        foreach (var segment in segments) {
          yield return segment.GetDrawData();
        }
      }
    }

    /// <summary>
    /// Calls <see cref="TrailSegment.Reset"/> on the next segment.
    /// </summary>
    internal void ResetNextSegment() {
      if (lastTrailResetTime < Main.time - segments.Length) {
        lastTrailResetTime = Main.time - segments.Length;
      }
      while (lastTrailResetTime < Main.time) {
        lastTrailResetTime++;
        segments[NextIndex()].Reset();
      }
    }
  }
}
