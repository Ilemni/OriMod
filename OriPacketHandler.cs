using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  public class OriPlayerPacketHandler : PacketHandler
  {
    public OriPlayerPacketHandler(byte handlerType) : base(handlerType)
		{
			HandlerType = handlerType;
		}
    public const byte OriState = 1;
    public override void HandlePacket(BinaryReader reader, int fromWho) {
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
    public void SendOriState(int toWho, int fromWho) {
      ModPacket packet = GetPacket(OriState, fromWho);
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();

      BitsByte flags = new BitsByte();
      flags[0] = fromPlayer.OriSet;
      flags[1] = fromPlayer.OriSet;
      flags[2] = fromPlayer.flashing;
      packet.Write((byte)flags);
      Vector2 animFrame = fromPlayer.AnimTile;
      packet.Write((byte)animFrame.X);
      packet.Write((byte)animFrame.Y);

      packet.Send(toWho, fromWho);
    }

    public void ReceiveOriState(BinaryReader r, int fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      BitsByte flags = r.ReadByte();
      bool oriSet = flags[0];
      bool oriSetPrevious = flags[1];
      bool flashing = flags[2];
      byte animFrameX = r.ReadByte();
      byte animFrameY = r.ReadByte();

      fromPlayer.OriSet = oriSet;
      fromPlayer.OriSet = oriSetPrevious;
      fromPlayer.flashing = flashing;
      
      fromPlayer.AnimFrame = OriPlayer.TileToPixel(animFrameX, animFrameY);
      if (Main.netMode == NetmodeID.Server) {
				SendOriState(-1, fromWho);
			}
    }
  }
}