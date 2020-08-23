using Terraria;

namespace OriMod {
  /// <summary>
  /// Class for containing and updating all <see cref="Trail"/>s on an <see cref="OriPlayer"/>.
  /// </summary>
  public class TrailManager {
    /// <summary>
    /// Create an instance of <see cref="TrailManager"/> that will belong to <paramref name="oPlayer"/>.
    /// </summary>
    /// <param name="oPlayer">The <see cref="OriPlayer"/> this <see cref="TrailManager"/> will belong to.</param>
    /// <param name="trailCount">Number of sprites to use for the trail.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Expected value of 1 or greater.</exception>
    internal TrailManager(OriPlayer oPlayer, int trailCount) {
      if (trailCount < 1) {
        throw new System.ArgumentOutOfRangeException(nameof(trailCount), "Expected value of 1 or greater.");
      }
      trails = new Trail[trailCount];
      int i = 0;
      while (i < trailCount) {
        trails[i++] = new Trail(oPlayer);
      }
    }
    
    private int Next {
      get {
        index = (index + 1) % trails.Length;
        return index;
      }
    }

    /// <summary>
    /// All <see cref="Trail"/>s in this <see cref="TrailManager"/>.
    /// </summary>
    internal readonly Trail[] trails;

    /// <summary>
    /// Current <see cref="trails"/> index. Used for <see cref="Trail.Reset"/>.
    /// </summary>
    private int index = 0;

    /// <summary>
    /// Call <see cref="Trail.Tick"/> on each <see cref="Trail"/>.
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
    /// Calls <see cref="Trail.Reset"/> on the next trail.
    /// </summary>
    internal void ResetNextTrail() => trails[Next].Reset();
  }
}
