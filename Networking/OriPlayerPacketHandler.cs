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

    internal override void HandlePacket(BinaryReader reader, ushort fromWho) {
      OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
      BitsByte flags = reader.ReadByte();
      bool oriSet = flags[0];
      bool flashing = flags[1];
      bool transforming = flags[2];
      bool unrestrictedMovement = flags[3];
      bool seinMinionActive = flags[4];
      bool mpcPlayerLight = flags[5];
      ushort transformTimer = flags[2] ? reader.ReadUInt16() : (ushort)0;
      byte seinMinionType = flags[4] ? reader.ReadByte() : (byte)0;
      Color spriteColorPrimary = reader.ReadRGB();
      Color spriteColorSecondary = reader.ReadRGBA();

      fromPlayer.IsOri = oriSet;
      fromPlayer.flashing = flashing;
      fromPlayer.Transforming = transforming;
      fromPlayer.UnrestrictedMovement = unrestrictedMovement;
      fromPlayer.transformTimer = transformTimer;
      fromPlayer.SeinMinionType = seinMinionType;
      fromPlayer.SeinMinionActive = seinMinionActive;
      fromPlayer.multiplayerPlayerLight = mpcPlayerLight;
      fromPlayer.SpriteColorPrimary = spriteColorPrimary;
      fromPlayer.SpriteColorSecondary = spriteColorSecondary;

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
      flags[0] = fromPlayer.IsOri;
      flags[1] = fromPlayer.flashing;
      flags[2] = fromPlayer.Transforming;
      flags[3] = fromPlayer.UnrestrictedMovement;
      flags[4] = fromPlayer.SeinMinionActive;
      flags[5] = fromPlayer.multiplayerPlayerLight;
      packet.Write(flags);
      if (flags[2]) {
        packet.Write((ushort)fromPlayer.transformTimer);
      }

      if (flags[4]) {
        packet.Write((byte)fromPlayer.SeinMinionType);
      }

      packet.WriteRGB(fromPlayer.SpriteColorPrimary);
      packet.WriteRGBA(fromPlayer.SpriteColorSecondary);

      packet.Send(toWho, fromWho);
    }
  }
}
