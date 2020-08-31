using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Networking {
  /// <summary>
  /// Base class for sending and handling received <see cref="ModPacket"/>s.
  /// </summary>
  internal abstract class PacketHandler {
    protected PacketHandler(byte handlerType) => this.handlerType = handlerType;

    /// <summary>
    /// Identifies which <see cref="PacketHandler"/> created the <see cref="ModPacket"/>.
    /// </summary>
    internal readonly byte handlerType;

    /// <summary>
    /// Handle the received <see cref="ModPacket"/> using <paramref name="reader"/>. Packet is from <paramref name="fromWho"/>.
    /// </summary>
    /// <param name="reader"><see cref="BinaryReader"/> for the <see cref="ModPacket"/>.</param>
    /// <param name="fromWho">Client this was from.</param>
    internal abstract void HandlePacket(BinaryReader reader, int fromWho);

    /// <summary>
    /// Gets a <see cref="ModPacket"/> with <see cref="handlerType"/> and <paramref name="fromWho"/> written to it.
    /// </summary>
    /// <param name="fromWho"></param>
    protected ModPacket GetPacket(int fromWho) {
      ModPacket p = OriMod.Instance.GetPacket();
      if (Main.netMode == NetmodeID.Server) {
        p.Write((ushort)fromWho);
      }
      p.Write(handlerType);
      return p;
    }
  }
}
