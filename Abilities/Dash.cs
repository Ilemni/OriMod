using Microsoft.Xna.Framework;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Dash : Ability {
    internal Dash(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool DoUpdate => InUse || (oPlayer.Input(OriMod.DashKey.JustPressed) && !Handler.cDash.InUse);
    internal override bool CanUse => base.CanUse && !InUse && Refreshed && !oPlayer.OnWall && !Handler.stomp.InUse && !Handler.bash.InUse && !player.mount.Active;
    protected override int Cooldown => 60;
    protected override Color RefreshColor => Color.White;
    
    private static readonly float[] Speeds = new float[] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    };
    private int Duration => 24;
    
    private int CurrTime = 0;
    private int Direction = 1;
    
    internal void StartDash() {
      CurrTime = 0;
      Direction = PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction;
      oPlayer.PlayNewSound("Ori/Dash/seinDash" + OriPlayer.RandomChar(3, ref currRand), 0.2f);
      player.pulley = false;
    }
    
    protected override void ReadPacket(System.IO.BinaryReader r) {
      Direction = r.ReadByte();
    }
    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      packet.Write((byte)Direction);
    }
    
    protected override void UpdateActive() {
      if (PlayerInput.Triggers.JustPressed.Jump && (player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm)) {
        Inactive = true;
        PutOnCooldown();
        return;
      }
      player.velocity.X = Speeds[CurrTime] * Direction * 0.65f;
      player.velocity.Y = 0.25f * (CurrTime + 1) * player.gravDir;
      if (CurrTime > 20) player.runSlowdown = 26f;
    }
    protected override void TickCooldown() {
      if (CurrCooldown > 0 || !Refreshed) {
        CurrCooldown--;
        if (CurrCooldown < 0 && (Handler.bash.InUse || oPlayer.OnWall || oPlayer.IsGrounded || player.mount.Active)) {
          Refreshed = true;
        }
      }
    }
    internal override void Tick() {
      if (CanUse && OriMod.DashKey.JustPressed) {
        Active = true;
        StartDash();
        return;
      }
      TickCooldown();
      if (InUse) {
        CurrTime++;
        if (CurrTime > Duration || oPlayer.OnWall || Handler.bash.InUse) {
          Inactive = true;
          PutOnCooldown();
        }
      }
    }
  }
}