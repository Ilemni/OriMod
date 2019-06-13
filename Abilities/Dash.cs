using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Dash : Ability {
    internal Dash(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    private static readonly float[] Speeds = new float[] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    };
    private const int Duration = 24;
    private int CurrTime = 0;
    internal int Direction = 1;
    
    internal override bool CanUse => base.CanUse && !InUse && Refreshed && !oPlayer.OnWall && !Handler.stomp.InUse && !Handler.bash.InUse && !player.mount.Active;
    protected override void ReadPacket(System.IO.BinaryReader r) {
      Direction = r.ReadByte();
    }
    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      packet.Write((byte)Direction);
    }
    internal void StartDash() {
      State = States.Active;
      CurrTime = 0;
      Refreshed = false;
      Direction = PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction;
      oPlayer.PlayNewSound("Ori/Dash/seinDash" + OriPlayer.RandomChar(3, ref currRand), 0.2f);
      player.pulley = false;
    }

    protected override void UpdateActive() {
      player.velocity.X = Speeds[CurrTime] * Direction * 0.65f;
      player.velocity.Y = 0.25f * CurrTime;
      if (CurrTime > 20) player.runSlowdown = 26f;
    }
    internal override void Tick() {
      if (!Refreshed && (oPlayer.IsGrounded || oPlayer.OnWall || Handler.bash.InUse)) {
        Refreshed = true;
      }
      if (InUse) {
        CurrTime++;
        if (CurrTime > Duration || oPlayer.OnWall || Handler.bash.InUse) {
          State = States.Inactive;
        }
      }
      else {
        if (CanUse && OriMod.DashKey.JustPressed) {
          StartDash();
        }
      }
    }
  }
}