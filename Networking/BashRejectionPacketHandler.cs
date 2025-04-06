using System;
using System.IO;
using AnimLib;
using OriMod.Abilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Networking;

/// <summary>
/// Sends from the server only, to a player that has <see cref="Bash"/>ed an NPC,
/// that the bash conditions are invalid and <see cref="Bash"/> must be cancelled.
/// For example, a Bash attempt may cancel if two players attempt to bash the same NPC at the same time.
/// </summary>
/// <param name="handlerType"></param>
/// <remarks>
/// The packet doesn't contain any data specific to the handler, only its type.
/// </remarks>
internal sealed class BashRejectionPacketHandler(byte handlerType) : PacketHandler(handlerType) {
  internal override void HandlePacket(BinaryReader reader, int fromWho) {
    // Packet is always sent from server, telling the local player that its bash attempt is invalid.
    OriCharacter ori = Main.LocalPlayer.GetCharacter<OriCharacter>();
    if (ori.ActiveState is Bash) {
      ori.TriggerState<NoAbility>();
    }
  }

  internal void SendPacket(int toWho) {
    if (!Main.dedServ) {
      throw new InvalidOperationException("Cannot send \"Bash Rejection\" packet from client.");
    }

    ModPacket packet = GetPacket(fromWho: 255);
    packet.Send(toWho);
  }
}
