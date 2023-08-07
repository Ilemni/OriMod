using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Networking; 

/// <summary>
/// Sends and receives <see cref="ModPacket"/>s that handle the <see cref="OriPlayer"/> state.
/// </summary>
internal class OriPlayerPacketHandler : PacketHandler {
  internal OriPlayerPacketHandler(byte handlerType) : base(handlerType) { }

  internal override void HandlePacket(BinaryReader reader, int fromWho) {
    OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();
    BitsByte flags = reader.ReadByte();
    bool oriSet = flags[0];
    bool transforming = flags[1];
    bool seinMinionActive = flags[3];
    bool mpcPlayerLight = flags[4];
    bool controls_blocked = flags[5];
    ushort transformTimer = transforming ? reader.ReadUInt16() : (ushort)0;
    byte seinMinionType = seinMinionActive ? reader.ReadByte() : (byte)0;
    Color spriteColorPrimary = reader.ReadRGB();
    Color spriteColorSecondary = reader.ReadRGBA();
    float dyeLerp = reader.ReadSingle();

    fromPlayer.IsOri = oriSet;
    fromPlayer.Transforming = transforming;
    fromPlayer.transformTimer = transformTimer;
    fromPlayer.SeinMinionType = seinMinionType;
    fromPlayer.SeinMinionActive = seinMinionActive;
    fromPlayer.multiplayerPlayerLight = mpcPlayerLight;
    fromPlayer.SpriteColorPrimary = spriteColorPrimary;
    fromPlayer.SpriteColorSecondary = spriteColorSecondary;
    fromPlayer.DyeColorBlend = dyeLerp;
    fromPlayer.controls_blocked = controls_blocked;

    fromPlayer.input.ReadPacket(reader);

    if (Main.netMode == NetmodeID.Server) {
      SendOriState(-1, fromWho);
    }
  }

  /// <summary>
  /// <para>Sends a <see cref="ModPacket"/> with <see cref="OriPlayer"/> data.</para>
  /// <inheritdoc cref="ModPacket.Send(int, int)"/>
  /// </summary>
  /// <param name="toWho">Who to send to. <see langword="255"/> for server, <see langword="-1"/> for all players.</param>
  /// <param name="fromWho">Sender, client to ignore.</param>
  internal void SendOriState(int toWho, int fromWho) {
    ModPacket packet = GetPacket(fromWho);
    OriPlayer fromPlayer = Main.player[fromWho].GetModPlayer<OriPlayer>();

    BitsByte flags = new() {
      [0] = fromPlayer.IsOri,
      [1] = fromPlayer.Transforming,
      [3] = fromPlayer.SeinMinionActive,
      [4] = fromPlayer.multiplayerPlayerLight,
      [5] = fromPlayer.controls_blocked
    };

    packet.Write(flags);
    if (fromPlayer.Transforming) {
      packet.Write((ushort)fromPlayer.transformTimer);
    }

    if (fromPlayer.SeinMinionActive) {
      packet.Write((byte)fromPlayer.SeinMinionType);
    }

    packet.WriteRGB(fromPlayer.SpriteColorPrimary);
    packet.WriteRGBA(fromPlayer.SpriteColorSecondary);
    packet.Write(fromPlayer.DyeColorBlend);

    fromPlayer.input.WritePacket(packet);

    packet.Send(toWho, fromWho);
  }
}
