using Terraria;

namespace OriMod {
  public class TrailManager {
    internal TrailManager(OriPlayer oPlayer, int trailCount) {
      if (trailCount < 1) {
        throw new System.ArgumentOutOfRangeException(nameof(trailCount), "Expected value of 1 or greater");
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

    internal readonly Trail[] trails;
    private int index = 0;
    public void UpdateTrails() {
      foreach (var trail in trails) {
        trail.Tick();
      }
    }

    public void AddTrailDrawDataToMain() {
      foreach (var trail in trails) {
        Main.playerDrawData.Add(trail.GetDrawData());
      }
    }
    
    internal void ResetNextTrail() => trails[Next].Reset();
  }
}
