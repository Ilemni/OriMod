using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  public abstract class PacketHandler {
    internal byte HandlerType { get; set; }
		
		public const byte OriStatus = 1;
    public const byte MovementPacket = 2;
		
		public abstract void HandlePacket(BinaryReader reader, int fromWho);

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