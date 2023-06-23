using AnimLib.Abilities;
using OriMod.Utilities;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping in the air.
  /// </summary>
  public sealed class AirJump : Ability<OriAbilityManager>, ILevelable {
    public override int Id => AbilityId.AirJump;
    public override int Level => ((ILevelable)this).Level;
    public override bool Unlocked => Level > 0;
    int ILevelable.Level { get; set; }
    public int MaxLevel => 4;

    public override bool CanUse => base.CanUse && !abilities.oPlayer.IsGrounded && !abilities.oPlayer.OnWall &&
      currentCount < MaxJumps && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.climb && !abilities.chargeJump && !abilities.launch &&
      !abilities.wallChargeJump;

    private static float JumpVelocity => 8.8f;
    private static int EndDuration => 32;
    private int MaxJumps => Level;

    internal ushort currentCount;
    private sbyte _gravityDirection;

    public override void ReadPacket(BinaryReader r) {
      cooldownLeft = r.ReadInt32();
      _gravityDirection = r.ReadSByte();
      player.position = r.ReadVector2();
      player.velocity = r.ReadVector2();
    }

    public override void WritePacket(ModPacket packet) {
      packet.Write(cooldownLeft);
      packet.Write(_gravityDirection);
      packet.WriteVector2(player.position);
      packet.WriteVector2(player.velocity);
    }

    private readonly RandomChar _rand = new RandomChar();

    public override void UpdateActive() {
      float newVel = -JumpVelocity * ((float)(EndDuration - stateTime) / EndDuration) * _gravityDirection;
      player.velocity.Y = newVel;
    }

    public override void PreUpdate() {
      if (CanUse && abilities.oPlayer.input.jump.JustPressed && IsLocal) {
        if (player.canJumpAgain_Blizzard || player.canJumpAgain_Cloud || player.canJumpAgain_Fart || 
          player.canJumpAgain_Sail || player.canJumpAgain_Sandstorm || player.mount.Active) return;
        SetState(AbilityState.Active);
        currentCount++;
        _gravityDirection = (sbyte)player.gravDir;

        if (MaxJumps != 1 && currentCount == MaxJumps) {
          abilities.oPlayer.PlaySound("Ori/TripleJump/seinTripleJumps" + _rand.NextNoRepeat(5), 0.6f);
        }
        else {
          abilities.oPlayer.PlaySound("Ori/DoubleJump/seinDoubleJumps" + _rand.NextNoRepeat(4), 0.5f);
        }
        return;
      }
      if (abilities.oPlayer.IsGrounded || abilities.bash || abilities.launch || abilities.oPlayer.OnWall) {
        currentCount = 0;
        if (abilities.oPlayer.IsGrounded || abilities.bash || abilities.launch || abilities.climb) {
          SetState(AbilityState.Inactive);
        }
      }
      if (Active) {
        SetState(AbilityState.Ending);
      }
      else if (Ending) {
        if (stateTime > EndDuration || player.velocity.Y * player.gravDir > 0) {
          SetState(AbilityState.Inactive);
        }
      }
      // Other than activation, Air Jump is deterministic and requires no additional syncing
      netUpdate = false;
    }
  }
}
