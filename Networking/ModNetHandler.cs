using System.IO;
using JetBrains.Annotations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Networking;

/// <summary>
/// Receives all <see cref="ModPacket"/>s and distributes them to the desired <see cref="PacketHandler"/>.
/// </summary>
[UsedImplicitly]
internal sealed class ModNetHandler : ModSystem {
  public override void SetStaticDefaults() {
    BashRejection = new BashRejectionPacketHandler(BashRejectType);
  }

  public override void Unload() {
    BashRejection = null!;
  }

  /// <summary>
  /// Enum value for <see cref="BashRejectionPacketHandler"/>
  /// </summary>
  private const byte BashRejectType = 1;

  /// <inheritdoc cref="BashRejectionPacketHandler"/>
  internal static BashRejectionPacketHandler BashRejection { get; private set; } = null!; // SetStaticDefaults()

  /// <summary>
  /// Sends the received <see cref="ModPacket"/> to the desired <see cref="PacketHandler"/> based on data read from <paramref name="reader"/>.
  /// </summary>
  /// <param name="reader">The <see cref="BinaryReader"/> that reads the received <see cref="ModPacket"/>.</param>
  /// <param name="fromWho">The player that this packet is from.</param>
  internal static void HandlePacket(BinaryReader reader, int fromWho) {
    if (Main.netMode == NetmodeID.MultiplayerClient) {
      // If packet is sent TO server, it is FROM player.
      // If packet is sent TO player, it is FROM server (This block) and fromWho is 255.
      // Server-written packet includes the fromWho, the player that created it.
      // Now in either case of this being server or player, the fromWho is the player.
      fromWho = reader.ReadUInt16();
    }

    byte packetClass = reader.ReadByte();
    PacketHandler? handler = packetClass switch {
      BashRejectType => BashRejection,
      _ => null
    };

    if (handler is null) {
      OriMod.Error("UnknownPacket", args: packetClass);
      return;
    }

    handler.HandlePacket(reader, fromWho);
  }
}
