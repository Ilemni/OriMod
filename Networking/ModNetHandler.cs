using System.IO;
using Terraria.ModLoader;

namespace OriMod.Networking {
  /// <summary>
  /// Receives all <see cref="ModPacket"/>s and distributes them to the desired <see cref="PacketHandler"/>.
  /// </summary>
  internal class ModNetHandler : SingleInstance<ModNetHandler> {
    private ModNetHandler() { }

    /// <summary>
    /// Type for <see cref="OriPlayerPacketHandler"/>.
    /// </summary>
    internal const byte OriState = 1;

    /// <summary>
    /// Type for <see cref="AbilityPacketHandler"/>.
    /// </summary>
    internal const byte AbilityState = 2;

    /// <inheritdoc cref="OriPlayerPacketHandler"/>
    internal readonly OriPlayerPacketHandler oriPlayerHandler = new OriPlayerPacketHandler(OriState);

    /// <inheritdoc cref="AbilityPacketHandler"/>
    internal readonly AbilityPacketHandler abilityPacketHandler = new AbilityPacketHandler(AbilityState);

    /// <summary>
    /// Sends the received <see cref="ModPacket"/> to the desired <see cref="PacketHandler"/> based on data read from <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> that reads the received <see cref="ModPacket"/>.</param>
    /// <param name="fromWho">The player that this packet is from.</param>
    internal void HandlePacket(BinaryReader reader, int fromWho) {
      byte packetClass = reader.ReadByte();
      switch (packetClass) {
        case OriState:
          oriPlayerHandler.HandlePacket(reader, fromWho);
          break;
        case AbilityState:
          abilityPacketHandler.HandlePacket(reader, fromWho);
          break;
        default:
          OriMod.Error("UnknownPacket", args: packetClass);
          break;
      }
    }
  }
}
