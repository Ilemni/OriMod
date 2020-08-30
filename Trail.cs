using Terraria;

namespace OriMod {
  /// <summary>
  /// Class for containing and updating all <see cref="TrailSegment"/>s on an <see cref="OriPlayer"/>.
  /// </summary>
  public class Trail {
    /// <summary>
    /// Create an instance of <see cref="Trail"/> that will belong to <paramref name="oPlayer"/>.
    /// </summary>
    /// <param name="oPlayer">The <see cref="OriPlayer"/> this <see cref="Trail"/> will belong to.</param>
    /// <param name="trailCount">Number of sprites to use for the trail.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Expected value of 1 or greater.</exception>
    internal Trail(OriPlayer oPlayer, int trailCount) {
      if (trailCount < 1) {
        throw new System.ArgumentOutOfRangeException(nameof(trailCount), "Expected value of 1 or greater.");
      }
      trails = new TrailSegment[trailCount];
      int i = 0;
      while (i < trailCount) {
        trails[i++] = new TrailSegment(oPlayer);
      }
    }

    private int Next {
      get {
        index = (index + 1) % trails.Length;
        return index;
      }
    }

    /// <summary>
    /// All <see cref="TrailSegment"/>s in this <see cref="Trail"/>.
    /// </summary>
    internal readonly TrailSegment[] trails;

    /// <summary>
    /// Current <see cref="trails"/> index. Used for <see cref="TrailSegment.Reset"/>.
    /// </summary>
    private int index = 0;

    /// <summary>
    /// Call <see cref="TrailSegment.Tick"/> on each <see cref="TrailSegment"/>.
    /// <para>This keeps the opacity of the trail appearing consistent.</para>
    /// </summary>
    public void UpdateTrails() {
      foreach (var trail in trails) {
        trail.Tick();
      }
    }

    /// <summary>
    /// Adds all <see cref="Terraria.DataStructures.DrawData"/> from this trail to <see cref="Main.playerDrawData"/>.
    /// </summary>
    public void AddTrailDrawDataToMain() {
      foreach (var trail in trails) {
        Main.playerDrawData.Add(trail.GetDrawData());
      }
    }

    /// <summary>
    /// Calls <see cref="TrailSegment.Reset"/> on the next trail.
    /// </summary>
    internal void ResetNextTrail() => trails[Next].Reset();
  }
}
