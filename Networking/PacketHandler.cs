using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Networking {
  internal abstract class PacketHandler {
    internal readonly byte HandlerType;

    internal byte OriStatus => 1;
    internal byte AbilityPacket => 2;

    internal abstract void HandlePacket(BinaryReader reader, int fromWho);

    protected PacketHandler(byte handlerType) => HandlerType = handlerType;

    protected ModPacket GetPacket(byte packetType, int fromWho) {
      ModPacket p = OriMod.Instance.GetPacket();
      p.Write(HandlerType);
      p.Write(packetType);
      if (Main.netMode == NetmodeID.Server) {
        p.Write((ushort)fromWho);
      }
      return p;
    }
  }
}
