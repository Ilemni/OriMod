using Terraria;
using Terraria.ModLoader;

namespace OriMod;

public static class OriModCall {
  private const string ResetPlayerCmd = "ResetPlayerModData";
  private const string IsOriCmd = "IsOri";

  /// <summary>
  /// Interact with <see cref="OriMod"/> using various inputs.<br />
  /// If any arguments are invalid, the call will warn and return <see langword="null"/>.
  /// <list type="table">
  /// <listheader>
  /// <term>Command/Parameters</term>
  /// <description>Description</description>
  /// </listheader>
  /// <item>
  /// <term>
  /// "ResetPlayerModData"<br />
  /// <see cref="Player"/>, <see cref="ModPlayer"/>, or <see cref="int"/>
  /// </term>
  /// <description>
  /// (Method call)<br />
  /// Resets the <see cref="OriPlayer"/> data on the given <see cref="Player"/>/<see cref="ModPlayer"/><br />
  /// Returns <see langword="null"/>
  /// </description>
  /// </item>
  /// <item>
  /// <term>
  /// "IsOri"<br />
  /// <see cref="Player"/>, <see cref="ModPlayer"/>, or <see cref="int"/><br />
  /// </term>
  /// <description>
  /// (Get)<br />
  /// Checks if the given <see cref="OriPlayer"/> is in Ori state.<br />
  /// Returns <see langword="bool"/>:<br />
  /// <see langword="true"/> if the player is Ori, or transforming into Ori;<br />
  /// <see langword="false"/> if neither;<br />
  /// </description>
  /// </item>
  /// <item>
  /// <term>
  /// "IsOri"<br />
  /// <see cref="Player"/>, <see cref="ModPlayer"/>, or <see cref="int"/><br />
  /// <see cref="bool"/>
  /// </term>
  /// <description>
  /// (Set)<br />
  /// Sets the player's Ori state to the given value.<br />
  /// Returns <see langword="null"/>
  /// </description>
  /// </item>
  /// </list>
  /// </summary>
  public static object Call(params object[] args) {
    int len = args.Length;
    if (len <= 0 || args[0] is not string cmd) return null;
    
    OriPlayer oPlayer = len >= 2 ? GetOriPlayer(args[1]) : null;

    switch (cmd) {
      case ResetPlayerCmd when oPlayer is not null:
        // ("ResetPlayerModData", player)
        oPlayer.ResetData();
        break;
      case ResetPlayerCmd:
        // ("ResetPlayerModData", ???)
        WarnArgType(ResetPlayerCmd, 1, args[1], typeof(Player));
        break;
      case IsOriCmd when oPlayer is not null && len < 3:
        // ("IsOri", player)
        return oPlayer.IsOri || oPlayer.Transforming;
      case IsOriCmd when oPlayer is not null && args[2] is bool isOri:
        // ("IsOri", player, isOri)
        oPlayer.IsOri = isOri;
        break;
      case IsOriCmd when oPlayer is not null:
        // ("IsOri", player, ???)
        WarnArgType(IsOriCmd, 2, args[2], typeof(bool));
        break;
      case IsOriCmd:
        // ("IsOri", ???)
        WarnArgType(IsOriCmd, 1, args[1], typeof(Player));
        break;
    }
    return null;
  }

  private static OriPlayer GetOriPlayer(object obj) {
    Player player = obj switch {
      Player p => p,
      ModPlayer modPlayer => modPlayer.Player,
      int index => Main.player[index],
      _ => null
    };
    return player?.GetModPlayer<OriPlayer>();
  }

  private static void WarnArgType(string cmd, int argIndex, object arg, System.Type expectedType) {
    OriMod.Log.Warn($"{OriMod.instance.Name}.Call() - {cmd} - Arg {argIndex}: Expected type {expectedType}, got {arg.GetType()}");
  }
}
