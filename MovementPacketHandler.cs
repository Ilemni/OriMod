using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  partial class MovementPacketHandler : PacketHandler {
    public MovementPacketHandler(byte handlerType) : base(handlerType)
		{
			HandlerType = handlerType;
		}
    public const byte MovementState = 1;
    public override void HandlePacket(System.IO.BinaryReader reader, int fromWho) {
      int packetType = reader.ReadByte();
      if (Main.netMode == NetmodeID.MultiplayerClient) {
		    fromWho = reader.ReadUInt16();
	    }
      switch (packetType) {
        case MovementState:
          ReceiveMovementState(reader, fromWho);
          break;
        default:
          Main.NewText("Unknown MovementPacket type" + packetType, Color.Red);
          break;
      }
    }
    public void SendMovementState(int toWho, int fromWho, List<string> changes) {
      ModPacket packet = GetPacket(MovementState, fromWho);
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      packet.Write((byte)changes.Count);
      foreach(string move in changes) {
        packet.Write((byte)(int)Enum.Parse(typeof(MovementHandler.MoveType), move));
        packet.Write((byte)fromPlayer.movementHandler.GetState(move));
        switch (move) {
          case "Bash":
            packet.Write(fromPlayer.movementHandler.bashCurrNPC);
            break;
          case "Grenade":
            packet.Write((int)fromPlayer.movementHandler.grenadePos.X);
            packet.Write((int)fromPlayer.movementHandler.grenadePos.Y);
            break;
          case "ChargeDash":
            packet.Write(fromPlayer.movementHandler.chargeDashCurrNPC);
            break;
        }
      }
      Main.NewText("Sending Movement State Packet from " + fromWho + " [" + Main.player[fromWho].name + "]");
      packet.Send(toWho, fromWho);
    }
    public void ReceiveMovementState(BinaryReader r, int fromWho) {
      Main.NewText("Receiving Movement State Packet from " + fromWho + " [" + Main.player[fromWho].name + "]");
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      int length = r.ReadByte();
      List<string> changes = new List<string>();
      for (int m = 0; m < length; m++) {
        string move = Enum.GetName(typeof(MovementHandler.MoveType), r.ReadByte());
        changes.Add(move);
        int state = r.ReadByte();
        fromPlayer.movementHandler.SetState(move, state);
        switch (move) {
          case "Bash":
            fromPlayer.movementHandler.bashCurrNPC = r.ReadByte();
            break;
          case "Grenade":
            fromPlayer.movementHandler.grenadePos.X = r.ReadInt32();
            fromPlayer.movementHandler.grenadePos.Y = r.ReadInt32();
            break;
          case "ChargeDash":
            fromPlayer.movementHandler.chargeDashCurrNPC = r.ReadByte();
            break;
        }
        if (Main.netMode == NetmodeID.MultiplayerClient) fromPlayer.movementHandler.UseMovement(move);
      }
      if (Main.netMode == NetmodeID.Server) {
        SendMovementState(-1, fromWho, changes);
      }
    }
  }
}