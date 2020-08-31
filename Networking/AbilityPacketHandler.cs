using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Networking {
  /// <summary>
  /// Sends and receives <see cref="ModPacket"/>s that handle the <see cref="Abilities.AbilityManager"/> state.
  /// </summary>
  internal class AbilityPacketHandler : PacketHandler {
    internal AbilityPacketHandler(byte handlerType) : base(handlerType) { }

    internal override void HandlePacket(BinaryReader reader, int fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      int len = reader.ReadByte();
      var changes = new List<byte>();
      for (int m = 0; m < len; m++) {
        byte id = reader.ReadByte();
        changes.Add(id);
        fromPlayer.abilities[id].PreReadPacket(reader);
      }
      if (Main.netMode == NetmodeID.Server) {
        SendAbilityState(-1, fromWho, changes);
      }
    }

    /// <summary>
    /// <para>Sends a <see cref="ModPacket"/> with ability data.</para>
    /// <inheritdoc cref="ModPacket.Send(int, int)"/>
    /// </summary>
    /// <param name="toWho">Who to send to. 255 for server, -1 for all players.</param>
    /// <param name="fromWho">Sender, client to ignore.</param>
    /// <param name="changes">List of <see cref="AbilityID"/>s for abilities that have changed.</param>
    internal void SendAbilityState(int toWho, int fromWho, List<byte> changes) {
      ModPacket packet = GetPacket(fromWho);
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      packet.Write((byte)changes.Count);
      foreach (byte id in changes) {
        packet.Write(id);
        fromPlayer.abilities[id].PreWritePacket(packet);
      }
      packet.Send(toWho, fromWho);
    }
  }
}
