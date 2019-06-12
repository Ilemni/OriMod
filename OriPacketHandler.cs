using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  internal class OriPlayerPacketHandler : PacketHandler {
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
          Main.NewText("Unknown OriStateHandler byte " + packetType, Color.Red);
          ErrorLogger.Log("Unknown OriStateHandler byte " + packetType);
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
      flags[3] = fromPlayer.UnrestrictedMovement;
      flags[4] = fromPlayer.SeinMinionActive;
      flags[5] = fromPlayer.MpcPlayerLight;
      packet.Write((byte)flags);
      if (flags[2]) packet.Write((short)fromPlayer.TransformTimer);
      if (flags[4]) packet.Write((byte)fromPlayer.SeinMinionUpgrade);
      packet.WriteRGB(fromPlayer.SpriteColor);

      packet.Send(toWho, fromWho);
    }

    internal void ReceiveOriState(BinaryReader r, int fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      BitsByte flags = r.ReadByte();
      bool oriSet = flags[0];
      bool flashing = flags[1];
      bool transforming = flags[2];
      bool unrestrictedMovement = flags[3];
      bool seinMinionActive = flags[4];
      bool mpcPlayerLight = flags[5];
      short transformTimer = flags[2] ? r.ReadInt16() : (short)0;
      byte seinMinionUpgrade = flags[4] ? r.ReadByte() : (byte)0;
      Color spriteColor = r.ReadRGB();

      fromPlayer.OriSet = oriSet;
      fromPlayer.Flashing = flashing;
      fromPlayer.Transforming = transforming;
      fromPlayer.UnrestrictedMovement = unrestrictedMovement;
      fromPlayer.TransformTimer = transformTimer;
      fromPlayer.SeinMinionUpgrade = seinMinionUpgrade;
      fromPlayer.SeinMinionActive = seinMinionActive;
      fromPlayer.MpcPlayerLight = mpcPlayerLight;
      fromPlayer.SpriteColor = spriteColor;
      
      if (Main.netMode == NetmodeID.Server) {
        SendOriState(-1, fromWho);
      }
    }
  }
}