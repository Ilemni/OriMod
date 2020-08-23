using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria.GameInput;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a quick horizontal dash. May be used in the air.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraira already allowing dashing with the Shield of Cthuhlu.
  /// </remarks>
  public sealed class Dash : Ability {
    static Dash() => OriMod.OnUnload += Unload;
    internal Dash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Dash;

    internal override bool UpdateCondition => InUse || oPlayer.Input(OriMod.DashKey.JustPressed) && !Manager.chargeDash.InUse;
    internal override bool CanUse => base.CanUse && !InUse && Refreshed && !oPlayer.OnWall && !Manager.stomp.InUse && !Manager.bash.InUse && !player.mount.Active;
    protected override int Cooldown => (int)(Config.DashCooldown * 30);
    protected override bool CooldownOnlyOnBoss => true;
    protected override Color RefreshColor => Color.White;

    private static float[] Speeds => _speeds ?? (_speeds = new float[25] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    });
    private static float[] _speeds;
    private static float SpeedMultiplier => Config.DashSpeedMultiplier * 0.65f;
    private int Duration => Speeds.Length;

    private sbyte Direction;

    private readonly RandomChar rand = new RandomChar();

    internal void StartDash() {
      currentTime = 0;
      Direction = (sbyte)(PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction);
      oPlayer.PlayNewSound("Ori/Dash/seinDash" + rand.NextNoRepeat(3), 0.2f);
      player.pulley = false;
    }

    protected override void ReadPacket(System.IO.BinaryReader r) {
      Direction = r.ReadSByte();
    }

    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      packet.Write(Direction);
    }

    protected override void UpdateActive() {
      if (PlayerInput.Triggers.JustPressed.Jump && (player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm)) {
        SetState(State.Inactive);
        PutOnCooldown();
        return;
      }
      player.velocity.X = Speeds[currentTime] * SpeedMultiplier * Direction;
      player.velocity.Y = 0.25f * (currentTime + 1) * player.gravDir;
      if (currentTime > 20) {
        player.runSlowdown = 26f;
      }
    }

    internal override void PutOnCooldown(bool force = false) {
      base.PutOnCooldown(force);
      Refreshed = false;
    }

    protected override void TickCooldown() {
      if (currentCooldown > 0 || !Refreshed) {
        currentCooldown--;
        if (currentCooldown < 0 && (Manager.bash.InUse || oPlayer.OnWall || oPlayer.IsGrounded || player.mount.Active)) {
          Refreshed = true;
        }
      }
    }

    internal override void Tick() {
      if (CanUse && OriMod.DashKey.JustPressed) {
        SetState(State.Active);
        StartDash();
        return;
      }
      TickCooldown();
      if (InUse) {
        currentTime++;
        if (currentTime > Duration - 1 || oPlayer.OnWall || Manager.bash.InUse) {
          SetState(State.Inactive);
          PutOnCooldown();
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
