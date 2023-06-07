using OriMod.Networking;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Sends info about players' Ori state to newly joined player
/// </summary>
internal class PlayerSyncOnConnect : ModSystem {
  public override bool HijackSendData(int whoAmI, int msgType,
    int remoteClient, int ignoreClient, NetworkText text,
    int number, float number2, float number3, float number4,
    int number5, int number6, int number7) {
    if (msgType == MessageID.FinishedConnectingToServer && Main.netMode == NetmodeID.Server) {
      foreach (Player pl in Main.player) {
        if (pl.active && remoteClient != pl.whoAmI)
          ModNetHandler.Instance.oriPlayerHandler.SendOriState(remoteClient, pl.whoAmI);
      }
    }
    return false;
  }
}
