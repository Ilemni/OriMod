using Microsoft.Xna.Framework;
using Terraria.GameInput;

namespace OriMod.Abilities {
  public class Dash : Ability {
    internal Dash(OriAbilities handler) : base(handler) { }
    public override int id => AbilityID.Dash;
    internal override bool DoUpdate => InUse || (oPlayer.Input(OriMod.DashKey.JustPressed) && !Handler.cDash.InUse);
    internal override bool CanUse => base.CanUse && !InUse && Refreshed && !oPlayer.OnWall && !Handler.stomp.InUse && !Handler.bash.InUse && !player.mount.Active;
    protected override int Cooldown => (int)(Config.DashCooldown * 30);
    protected override bool CooldownOnlyOnBoss => true;
    protected override Color RefreshColor => Color.White;
    
    private static readonly float[] Speeds = new float[25] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    };
    private static float SpeedMultiplier => Config.DashSpeedMultiplier * 0.65f;
    private int Duration => Speeds.Length;
    
    private int Direction;
    
    internal void StartDash() {
      CurrTime = 0;
      Direction = PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction;
      oPlayer.PlayNewSound("Ori/Dash/seinDash" + OriPlayer.RandomChar(3, ref CurrSoundRand), 0.2f);
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
      player.velocity.X = Speeds[CurrTime] * SpeedMultiplier * Direction;
      player.velocity.Y = 0.25f * (CurrTime + 1) * player.gravDir;
      if (CurrTime > 20) player.runSlowdown = 26f;
    }
    internal override void PutOnCooldown(bool force=false) {
      base.PutOnCooldown(force);
      Refreshed = false;
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
        if (CurrTime > Duration - 1 || oPlayer.OnWall || Handler.bash.InUse) {
          Inactive = true;
          PutOnCooldown();
        }
      }
    }
  }
}