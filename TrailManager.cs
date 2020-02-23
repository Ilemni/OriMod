using Terraria;

namespace OriMod {
  internal class TrailManager {
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
    
    internal void UpdateTrails() {
      for (int i = 0, len = trails.Length; i < len; i++) {
        trails[i].Tick();
      }
    }

    internal void AddTrailDrawData() {
      for (int i = 0, len = trails.Length; i < len; i++) {
        Main.playerDrawData.Add(trails[i].GetDrawData());
      }
    }
    
    internal void ResetNext() => trails[Next].Reset();

    private int Next {
      get {
        index++;
        if (index > trails.Length) {
          index = 0;
        }
        return index;
      }
    }

    private int index = 0;
    internal Trail[] trails;
  }
}
