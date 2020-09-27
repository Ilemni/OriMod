using System;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for looking up. Pairs with the ability <see cref="ChargeJump"/>.
  /// <para>This ability on its own is entirely visual, and is always unlocked.</para>
  /// </summary>
  public sealed class LookUp : Ability {
    internal LookUp(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.LookUp;
    public override byte Level => 1;

    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && Math.Abs(player.velocity.X) < 0.8f && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.chargeDash && !abilities.climb && !abilities.crouch && !abilities.dash;

    private static int StartDuration => 12;
    private static int EndDuration => 8;

    internal override void Tick() {
      if (!InUse) {
        if (CanUse && (player.controlUp || input.charge.Current)) {
          SetState(State.Starting);
        }
      }
      else if (!CanUse) {
        SetState(State.Inactive);
      }
      else if (!(player.controlUp || input.charge.Current) && !Ending) {
        SetState(Active ? State.Ending : State.Inactive);
        return;
      }
      else if (Starting) {
        if (CurrentTime > StartDuration) {
          SetState(State.Active);
        }
      }
      else if (Ending) {
        if (CurrentTime > EndDuration) {
          SetState(State.Inactive);
        }
      }
    }
  }
}
