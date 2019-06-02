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
      flags[1] = fromPlayer.Flashing;
      flags[2] = fromPlayer.Transforming;
      packet.Write((byte)flags);
      if (fromPlayer.Transforming) {
        packet.Write((short)fromPlayer.TransformTimer);
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
      bool flashing = flags[1];
      bool transforming = flags[2];
      short transformTimer = 0;
      if (transforming) {
        transformTimer = r.ReadInt16();
      }
      Vector2 animTile = new Vector2(r.ReadByte(), r.ReadByte());

      fromPlayer.OriSet = oriSet;
      fromPlayer.Flashing = flashing;
      fromPlayer.Transforming = transforming;
      fromPlayer.TransformTimer = transformTimer;
      fromPlayer.AnimTile = animTile;
      
      if (Main.netMode == NetmodeID.Server) {
				SendOriState(-1, fromWho);
			}
    }
  }
}