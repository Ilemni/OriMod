using Microsoft.Xna.Framework;
using OriMod.Utilities;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a quick horizontal dash. May be used in the air.
  /// </summary>
  /// <remarks>
  /// This ability is derived from the Ori games, despite Terraira already allowing dashing with the Shield of Cthuhlu.
  /// </remarks>
  public sealed class Dash : Ability, ILevelable {
    static Dash() => OriMod.OnUnload += Unload;
    internal Dash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Dash;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    byte ILevelable.MaxLevel => 3;

    internal override bool CanUse => base.CanUse && !InUse && Refreshed && !oPlayer.OnWall && !abilities.stomp && !abilities.bash && !abilities.launch && !player.mount.Active && (Level >= 2 || oPlayer.IsGrounded);
    protected override int Cooldown => Level >= 3 ? 0 : 60;
    protected override Color RefreshColor => Color.White;

    private static float[] Speeds => _speeds ?? (_speeds = new float[25] {
      50f, 50f, 50f, 49.9f, 49.6f, 49f, 48f, 46.7f, 44.9f, 42.4f, 39.3f, 35.4f, 28.6f, 20f,
      19.6f, 19.1f, 18.7f, 18.3f, 17.9f, 17.4f, 17f, 16.5f, 16.1f, 15.7f, 15.2f
    });
    private static float[] _speeds;
    private static int Duration => Speeds.Length - 1;

    private sbyte direction;

    private readonly RandomChar rand = new RandomChar();

    internal void StartDash() {
      direction = (sbyte)(player.controlLeft ? -1 : player.controlRight ? 1 : player.direction);
      oPlayer.PlayNewSound("Ori/Dash/seinDash" + rand.NextNoRepeat(3), 0.2f);
      player.pulley = false;
    }

    protected override void ReadPacket(System.IO.BinaryReader r) {
      direction = r.ReadSByte();
    }

    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      packet.Write(direction);
    }

    protected override void UpdateActive() {
      if (player.controlJump && (player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm)) {
        SetState(State.Inactive);
        PutOnCooldown();
        return;
      }
      player.velocity.X = Speeds[CurrentTime] * 0.5f * direction;
      player.velocity.Y = 0.25f * (CurrentTime + 1) * player.gravDir;
      if (CurrentTime > 20) {
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
        if (currentCooldown < 0 && (abilities.bash || oPlayer.OnWall || oPlayer.IsGrounded || player.mount.Active)) {
          Refreshed = true;
        }
      }
    }

    internal override void Tick() {
      if (abilities.chargeDash) {
        SetState(State.Inactive);
        return;
      }
      if (CanUse && input.dash.JustPressed) {
        SetState(State.Active);
        StartDash();
        return;
      }
      TickCooldown();
      if (InUse) {
        if (abilities.airJump) {
          SetState(State.Inactive);
          player.velocity.X = Speeds[24] * direction; // Rip hyperspeed dash-jump
          PutOnCooldown(true);
        }
        else if (CurrentTime > Duration || oPlayer.OnWall || abilities.bash) {
          SetState(State.Inactive);
          PutOnCooldown(true);
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
