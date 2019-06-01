using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  internal class OriPlayerPacketHandler : PacketHandler
  {
    internal OriPlayerPacketHandler(byte handlerType) : base(handlerType)
		{
			HandlerType = handlerType;
		}
    internal const byte OriState = 1;
    internal override void HandlePacket(BinaryReader reader, int fromWho) {
      byte packetType = reader.ReadByte();
      if (Main.netMode == NetmodeID.MultiplayerClient) {
		    fromWho = reader.ReadUInt16();
	    }
      switch(packetType) {
        case OriState:
          ReceiveOriState(reader, fromWho);
          break;
        default:
          Main.NewText("Unknown OriStateHandler byte " + packetType);
          break;
      }
    }
    internal void SendOriState(int toWho, int fromWho) {
      ModPacket packet = GetPacket(OriState, fromWho);
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();

      BitsByte flags = new BitsByte();
      flags[0] = fromPlayer.OriSet;
      flags[1] = fromPlayer.OriSet;
      flags[2] = fromPlayer.flashing;
      flags[3] = fromPlayer.transforming;
      packet.Write((byte)flags);
      if (fromPlayer.transforming) {
        packet.Write((short)fromPlayer.transformTimer);
      }
      Vector2 animTile = fromPlayer.AnimTile;
      packet.Write((byte)animTile.X);
      packet.Write((byte)animTile.Y);

      packet.Send(toWho, fromWho);
    }

    internal void ReceiveOriState(BinaryReader r, int fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      BitsByte flags = r.ReadByte();
      bool oriSet = flags[0];
      bool oriSetPrevious = flags[1];
      bool flashing = flags[2];
      bool transforming = flags[3];
      short transformTimer = 0;
      if (transforming) {
        transformTimer = r.ReadInt16();
      }
      Vector2 animTile = new Vector2(r.ReadByte(), r.ReadByte());

      fromPlayer.OriSet = oriSet;
      fromPlayer.OriSet = oriSetPrevious;
      fromPlayer.flashing = flashing;
      fromPlayer.transforming = transforming;
      fromPlayer.transformTimer = transformTimer;
      fromPlayer.AnimTile = animTile;
      
      if (Main.netMode == NetmodeID.Server) {
				SendOriState(-1, fromWho);
			}
    }
  }
}