using System.IO;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for jumping in the air.
  /// </summary>
  public sealed class AirJump : Ability, ILevelable {
    internal AirJump(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityId.AirJump;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    public byte MaxLevel => 4;

    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && currentCount < MaxJumps && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.climb && !abilities.chargeJump && !abilities.launch &&
      !abilities.wallChargeJump;

    private static float JumpVelocity => 8.8f;
    private static int EndDuration => 32;
    private int MaxJumps => Level;

    internal ushort currentCount;
    private sbyte _gravityDirection;

    protected override void ReadPacket(BinaryReader r) {
      currentCooldown = r.ReadUInt16();
      _gravityDirection = r.ReadSByte();
      player.position = r.ReadVector2();
      player.velocity = r.ReadVector2();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(currentCount);
      packet.Write(_gravityDirection);
      packet.WriteVector2(player.position);
      packet.WriteVector2(player.velocity);
    }

    private readonly RandomChar _rand = new RandomChar();

    protected override void UpdateActive() {
      float newVel = -JumpVelocity * ((float)(EndDuration - CurrentTime) / EndDuration) * _gravityDirection;
      player.velocity.Y = newVel;
    }

    internal override void Tick() {
      if (CanUse && input.jump.JustPressed) {
        if (player.canJumpAgain_Blizzard || player.canJumpAgain_Cloud || player.canJumpAgain_Fart || player.canJumpAgain_Sail ||
            player.canJumpAgain_Sandstorm || player.canCarpet || (player.rocketBoots!=0 && player.rocketTime>0) || player.mount.Active) return;
        SetState(State.Active);
        currentCount++;
        _gravityDirection = (sbyte)player.gravDir;

        if (MaxJumps != 1 && currentCount == MaxJumps) {
          oPlayer.PlaySound("Ori/TripleJump/seinTripleJumps" + _rand.NextNoRepeat(5), 0.6f);
        }
        else {
          oPlayer.PlaySound("Ori/DoubleJump/seinDoubleJumps" + _rand.NextNoRepeat(4), 0.5f);
        }
        return;
      }
      if (oPlayer.IsGrounded || abilities.bash || abilities.launch || oPlayer.OnWall) {
        currentCount = 0;
        if (oPlayer.IsGrounded || abilities.bash || abilities.launch || abilities.climb) {
          SetState(State.Inactive);
        }
      }
      if (Active) {
        SetState(State.Ending);
      }
      else if (Ending) {
        if (CurrentTime > EndDuration || player.velocity.Y * player.gravDir > 0) {
          SetState(State.Inactive);
        }
      }
      // Other than activation, Air Jump is deterministic and requires no additional syncing
      netUpdate = false;
    }
  }
}
