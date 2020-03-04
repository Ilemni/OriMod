using System.IO;

namespace OriMod.Networking {
  internal class ModNetHandler : SingleInstance<ModNetHandler> {
    private ModNetHandler() { }

    internal const byte OriState = 1;
    internal const byte Ability = 2;

    internal readonly OriPlayerPacketHandler oriPlayerHandler = new OriPlayerPacketHandler(OriState);
    internal readonly AbilityPacketHandler abilityPacketHandler = new AbilityPacketHandler(Ability);

    internal void HandlePacket(BinaryReader r, int fromWho) {
      byte packetClass = r.ReadByte();
      switch (packetClass) {
        case OriState:
          oriPlayerHandler.HandlePacket(r, fromWho);
          break;
        case Ability:
          abilityPacketHandler.HandlePacket(r, fromWho);
          break;
        default:
          OriMod.ErrorFormat("UnknownPacket", args: packetClass);
          break;
      }
    }
  }
}
