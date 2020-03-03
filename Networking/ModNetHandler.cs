using System.IO;

namespace OriMod.Networking {
  internal class ModNetHandler {
    private ModNetHandler() {
      oriPlayerHandler = new OriPlayerPacketHandler(OriState);
      abilityPacketHandler = new AbilityPacketHandler(Ability);
    }

    internal static ModNetHandler Instance => _i ?? (_i = new ModNetHandler());
    private static ModNetHandler _i;

    internal const byte OriState = 1;
    internal const byte Ability = 2;

    internal readonly OriPlayerPacketHandler oriPlayerHandler;
    internal readonly AbilityPacketHandler abilityPacketHandler;

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

    internal static void Unload() {
      _i = null;
    }
  }
}
