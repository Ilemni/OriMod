using System.IO;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Networking;

/// <summary>
/// Sends and receives <see cref="ModPacket"/>s that handle the <see cref="OriPlayer"/> state.
/// </summary>
internal class OriPlayerPacketHandler(byte handlerType) : PacketHandler(handlerType) {
  internal override void HandlePacket(BinaryReader reader, int fromWho) {
    OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();

    BitsByte flags = reader.ReadByte();
    fromPlayer.MultiplayerPlayerLight = flags[0];
    fromPlayer.SpriteColorPrimary = reader.ReadRGB();
    fromPlayer.SpriteColorSecondary = reader.ReadRGBA();
    fromPlayer.DyeColorBlend = reader.ReadSingle();

    if (Main.dedServ) {
      SendOriState(-1, fromWho);
    }
  }

  /// <summary>
  /// <para>Sends a <see cref="ModPacket"/> with <see cref="OriPlayer"/> data.</para>
  /// <inheritdoc cref="ModPacket.Send(int, int)"/>
  /// </summary>
  /// <param name="toWho">Who to send to. <see langword="255"/> for server, <see langword="-1"/> for all players.</param>
  /// <param name="fromWho">Sender, client to ignore.</param>
  internal void SendOriState(int toWho, int fromWho) {
    ModPacket packet = GetPacket(fromWho);
    OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();

    BitsByte flags = new() {
      [0] = fromPlayer.MultiplayerPlayerLight,
    };

    packet.Write(flags);

    packet.WriteRGB(fromPlayer.SpriteColorPrimary);
    packet.WriteRGBA(fromPlayer.SpriteColorSecondary);
    packet.Write(fromPlayer.DyeColorBlend);

    packet.Send(toWho, fromWho);
  }
}
