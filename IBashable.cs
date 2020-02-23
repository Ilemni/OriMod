using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod {
  public interface IBashable {
    OriPlayer BashPlayer { get; set; }
    Entity BashEntity { get; set; } 
    Vector2 BashPosition { get; set; }
    bool IsBashed { get; set; }

    void OnBashed(OriPlayer oPlayer);
  }
}
