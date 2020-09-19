using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public sealed partial class OriMod : Mod {
    /// <summary>
    /// Interact with <see cref="OriMod"/> using various inputs.
    /// <list type="table">
    /// <listheader>
    /// <term>Command/Parameters</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>"ResetPlayerModData", <see cref="Player"/> -or- <see cref="ModPlayer"/></term>
    /// <description>
    /// Resets the <see cref="OriPlayer"/> data on the given <see cref="Player"/>/<see cref="ModPlayer"/> (setonly) —
    /// Returns <see langword="true"/> if arguments are valid; otherwise, <see langword="false"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    public override object Call(params object[] args) {
      int len = args.Length;
      if (len > 0 && args[0] is string cmd) {
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
            Log.Warn($"{this.Name}.Call() - ResetPlayerModData - Expected type {typeof(Player)}, got {args[1].GetType()}");
            return false;
        }
      }
      return null;
    }

    private OriPlayer GetOriPlayer(object obj) {
      var player = obj is Player p ? p : obj is ModPlayer modPlayer ? modPlayer.player : null;
      return player?.GetModPlayer<OriPlayer>();
    }
  }
}
