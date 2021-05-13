using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public sealed partial class OriMod {
    /// <summary>
    /// Interact with <see cref="OriMod"/> using various inputs.
    /// <list type="table">
    /// <listheader>
    /// <term>Command/Parameters</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>"ResetPlayerModData"</term>
    /// <description>
    /// Arguments <see cref="Player"/> -or- <see cref="ModPlayer"/>, resets the <see cref="OriPlayer"/> data on the given <see cref="Player"/>/<see cref="ModPlayer"/> (set-only) —
    /// Returns <see langword="true"/> if arguments are valid; otherwise, <see langword="false"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <term>"IsOri"</term>
    /// <description>
    /// Arguments <see cref="Player"/> -or- <see cref="ModPlayer"/>, checks if the player is in Ori state (readonly) —
    /// Returns <see langword="true"/> if the player is Ori or transforming into Ori; <see langword="false"/> if neither; or <see langword="null"/> if arguments are invalid.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public override object Call(params object[] args) {
      int len = args.Length;
      if (len <= 0 || !(args[0] is string cmd)) return null;
      
      OriPlayer oPlayer = null;
      if (len >= 2) {
        oPlayer = GetOriPlayer(args[1]);
      }

      switch (cmd) {
        case "ResetPlayerModData":
          if (!(oPlayer is null)) {
            oPlayer.ResetData();
            return true;
          }
          Log.Warn($"{Name}.Call() - ResetPlayerModData - Expected type {typeof(Player)}, got {args[1].GetType()}");
          return false;
        case "IsOri":
          if (!(oPlayer is null)) {
            return oPlayer.IsOri || oPlayer.Transforming;
          }
          Log.Warn($"{Name}.Call() - Transforming - Expected type {typeof(Player)}, got {args[1].GetType()}");
          return null;
      }
      return null;
    }

    private static OriPlayer GetOriPlayer(object obj) {
      Player player = obj is Player p ? p : obj is ModPlayer modPlayer ? modPlayer.player : null;
      return player?.GetModPlayer<OriPlayer>();
    }
  }
}
