using System.IO;
using JetBrains.Annotations;
using Terraria.ModLoader;

namespace OriMod.Networking;

/// <summary>
/// Receives all <see cref="ModPacket"/>s and distributes them to the desired <see cref="PacketHandler"/>.
/// </summary>
[UsedImplicitly]
internal sealed class ModNetHandler : ModSystem {
  public override void SetStaticDefaults() {
    OriPlayerHandler = new OriPlayerPacketHandler(OriState);
    BashRejection = new BashRejectionPacketHandler(BashRejectType);
  }

  public override void Unload() {
    OriPlayerHandler = null!;
    BashRejection = null!;
  }

  /// <summary>
  /// Enum value for <see cref="OriPlayerPacketHandler"/>.
  /// </summary>
  private const byte OriState = 1;

  /// <summary>
  /// Enum value for <see cref="BashRejectionPacketHandler"/>
  /// </summary>
  private const byte BashRejectType = 2;

  /// <inheritdoc cref="OriPlayerPacketHandler"/>
  internal static OriPlayerPacketHandler OriPlayerHandler { get; private set; } = null!; // SetStaticDefaults()

  /// <inheritdoc cref="BashRejectionPacketHandler"/>
  internal static BashRejectionPacketHandler BashRejection { get; private set; } = null!; // SetStaticDefaults()

  /// <summary>
  /// Sends the received <see cref="ModPacket"/> to the desired <see cref="PacketHandler"/> based on data read from <paramref name="reader"/>.
  /// </summary>
  /// <param name="reader">The <see cref="BinaryReader"/> that reads the received <see cref="ModPacket"/>.</param>
  /// <param name="fromWho">The player that this packet is from.</param>
  internal static void HandlePacket(BinaryReader reader, int fromWho) {
    byte packetClass = reader.ReadByte();
    PacketHandler? handler = packetClass switch {
      OriState => OriPlayerHandler,
      BashRejectType => BashRejection,
      _ => null,
    };

    if (handler is null) {
        OriMod.Error("UnknownPacket", args: packetClass);
        return;
    }

    handler.HandlePacket(reader, fromWho);
  }
}
