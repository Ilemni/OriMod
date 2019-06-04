using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  internal abstract class PacketHandler {
    internal byte HandlerType { get; set; }
		
		internal const byte OriStatus = 1;
    internal const byte AbilityPacket = 2;
		
		internal abstract void HandlePacket(BinaryReader reader, int fromWho);

		protected PacketHandler(byte handlerType)
		{
			HandlerType = handlerType;
		}

		protected ModPacket GetPacket(byte packetType, int fromWho)
		{
			ModPacket p = OriMod.Instance.GetPacket();
			p.Write(HandlerType);
			p.Write(packetType);
			if (Main.netMode == NetmodeID.Server) {
				p.Write((UInt16)fromWho);
			}
			return p;
		}
  }
}