using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  partial class AbilityPacketHandler : PacketHandler {
    internal AbilityPacketHandler(byte handlerType) : base(handlerType) { }

    internal const byte AbilityState = 1;

    internal override void HandlePacket(BinaryReader reader, int fromWho) {
      byte packetType = reader.ReadByte();
      if (Main.netMode == NetmodeID.MultiplayerClient) {
        fromWho = reader.ReadUInt16();
      }
      switch (packetType) {
        case AbilityState:
          ReceiveAbilityState(reader, fromWho);
          break;
        default:
          OriMod.ErrorFormat("UnknownAbilityPacket", args: packetType);
          break;
      }
    }

    internal void SendAbilityState(int toWho, int fromWho, List<byte> changes) {
      ModPacket packet = GetPacket(AbilityState, fromWho);
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      packet.Write((byte)changes.Count);
      foreach (byte id in changes) {
        packet.Write(id);
        fromPlayer.Abilities[id].PreWritePacket(packet);
      }
      packet.Send(toWho, fromWho);
    }

    internal void ReceiveAbilityState(BinaryReader r, int fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      int len = r.ReadByte();
      var changes = new List<byte>();
      for (int m = 0; m < len; m++) {
        byte id = r.ReadByte();
        changes.Add(id);
        fromPlayer.Abilities[id].PreReadPacket(r);
        fromPlayer.Abilities[id].Update();
      }
      if (Main.netMode == NetmodeID.Server) {
        SendAbilityState(-1, fromWho, changes);
      }
    }
  }
}
