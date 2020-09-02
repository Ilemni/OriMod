using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Networking {
  /// <summary>
  /// Sends and receives <see cref="ModPacket"/>s that handle the <see cref="OriPlayer"/> state.
  /// </summary>
  internal class OriPlayerPacketHandler : PacketHandler {
    internal OriPlayerPacketHandler(byte handlerType) : base(handlerType) { }

    internal override void HandlePacket(BinaryReader reader, int fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      BitsByte flags = reader.ReadByte();
      BitsByte ctrl = reader.ReadByte();
      bool oriSet = flags[0];
      bool transforming = flags[1];
      bool unrestrictedMovement = flags[2];
      bool seinMinionActive = flags[3];
      bool mpcPlayerLight = flags[4];
      ushort transformTimer = transforming ? reader.ReadUInt16() : (ushort)0;
      byte seinMinionType = seinMinionActive ? reader.ReadByte() : (byte)0;
      Color spriteColorPrimary = reader.ReadRGB();
      Color spriteColorSecondary = reader.ReadRGBA();

      bool jump = ctrl[0];
      bool feather = ctrl[1];

      fromPlayer.IsOri = oriSet;
      fromPlayer.Transforming = transforming;
      fromPlayer.UnrestrictedMovement = unrestrictedMovement;
      fromPlayer.transformTimer = transformTimer;
      fromPlayer.SeinMinionType = seinMinionType;
      fromPlayer.SeinMinionActive = seinMinionActive;
      fromPlayer.multiplayerPlayerLight = mpcPlayerLight;
      fromPlayer.SpriteColorPrimary = spriteColorPrimary;
      fromPlayer.SpriteColorSecondary = spriteColorSecondary;

      fromPlayer.justPressedJumped = jump;
      fromPlayer.featherKeyDown = feather;

      if (Main.netMode == NetmodeID.Server) {
        SendOriState(-1, fromWho);
      }
    }

    /// <summary>
    /// <para>Sends a <see cref="ModPacket"/> with <see cref="OriPlayer"/> data.</para>
    /// <inheritdoc cref="ModPacket.Send(int, int)"/>
    /// </summary>
    /// <param name="toWho">Who to send to. 255 for server, -1 for all players.</param>
    /// <param name="fromWho">Sender, client to ignore.</param>
    internal void SendOriState(int toWho, int fromWho) {
      ModPacket packet = GetPacket(fromWho);
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();

      var flags = new BitsByte();
      var ctrl = new BitsByte();
      flags[0] = fromPlayer.IsOri;
      flags[1] = fromPlayer.Transforming;
      flags[2] = fromPlayer.UnrestrictedMovement;
      flags[3] = fromPlayer.SeinMinionActive;
      flags[4] = fromPlayer.multiplayerPlayerLight;
      
      ctrl[0] = fromPlayer.justPressedJumped;
      ctrl[1] = fromPlayer.featherKeyDown;

      packet.Write(flags);
      packet.Write(ctrl);
      if (fromPlayer.Transforming) {
        packet.Write((ushort)fromPlayer.transformTimer);
      }

      if (fromPlayer.SeinMinionActive) {
        packet.Write((byte)fromPlayer.SeinMinionType);
      }

      packet.WriteRGB(fromPlayer.SpriteColorPrimary);
      packet.WriteRGBA(fromPlayer.SpriteColorSecondary);

      packet.Send(toWho, fromWho);
    }
  }
}
