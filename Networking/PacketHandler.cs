using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Networking;

/// <summary>
/// Base class for sending and handling received <see cref="ModPacket"/>s.
/// </summary>
internal abstract class PacketHandler(byte handlerType) {
  /// <summary>
  /// Identifies which <see cref="PacketHandler"/> created the <see cref="ModPacket"/>.
  /// </summary>
  private readonly byte _handlerType = handlerType;

  /// <summary>
  /// Handle the received <see cref="ModPacket"/> using <paramref name="reader"/>. Packet is from <paramref name="fromWho"/>.
  /// </summary>
  /// <param name="reader"><see cref="BinaryReader"/> for the <see cref="ModPacket"/>.</param>
  /// <param name="fromWho">Client this was from.</param>
  internal abstract void HandlePacket(BinaryReader reader, int fromWho);

  /// <summary>
  /// Gets a <see cref="ModPacket"/> with <see cref="_handlerType"/> and <paramref name="fromWho"/> written to it.
  /// </summary>
  /// <param name="fromWho">The whoAmI of the player whose data will be in this packet.</param>
  protected ModPacket GetPacket(int fromWho) {
    ModPacket packet = OriMod.Instance.GetPacket();

    if (Main.dedServ) {
      packet.Write((ushort)fromWho);
    }

    packet.Write(_handlerType);
    return packet;
  }
}
