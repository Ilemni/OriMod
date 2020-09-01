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
    public override int Id => AbilityID.AirJump;
    public override byte Level => (this as ILevelable).Level;
    byte ILevelable.Level { get; set; }
    public byte MaxLevel => 3;

    internal override bool CanUse => base.CanUse && !oPlayer.IsGrounded && !oPlayer.OnWall && currentCount < MaxJumps && !player.mount.Active && !abilities.bash.InUse && !abilities.wallChargeJump.InUse;

    private static float JumpVelocity => 8.8f;
    private static int EndDuration => 32;
    private int MaxJumps => MaxLevel;

    internal ushort currentCount;
    private sbyte gravityDirection;

    protected override void ReadPacket(BinaryReader r) {
      currentCooldown = r.ReadUInt16();
      gravityDirection = r.ReadSByte();
      player.position = r.ReadVector2();
      player.velocity = r.ReadVector2();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(currentCount);
      packet.Write(gravityDirection);
      packet.WriteVector2(player.position);
      packet.WriteVector2(player.velocity);
    }

    private readonly RandomChar rand = new RandomChar();

    protected override void UpdateActive() {
      if (currentCount == MaxJumps) {
        oPlayer.PlayNewSound("Ori/TripleJump/seinTripleJumps" + rand.NextNoRepeat(5), 0.7f);
      }
      else {
        oPlayer.PlayNewSound("Ori/DoubleJump/seinDoubleJumps" + rand.NextNoRepeat(4), 0.75f);
      }
      float newVel = -JumpVelocity * ((EndDuration - CurrentTime) / EndDuration);
      if (player.velocity.Y > newVel) {
        player.velocity.Y = newVel;
      }

      player.velocity.Y *= gravityDirection;
    }

    internal override void Tick() {
      if (CanUse && oPlayer.justPressedJumped) {
        if (!(player.jumpAgainBlizzard || player.jumpAgainCloud || player.jumpAgainFart || player.jumpAgainSail || player.jumpAgainSandstorm || player.mount.Active)) {
          SetState(State.Active);
          if (abilities.dash.Active) {
            abilities.dash.SetState(State.Inactive);
            abilities.dash.PutOnCooldown();
          }
          currentCount++;
          gravityDirection = (sbyte)player.gravDir;
        }
        return;
      }
      if (oPlayer.IsGrounded || abilities.bash.InUse || oPlayer.OnWall) {
        currentCount = 0;
        SetState(State.Inactive);
      }
      else if (Active) {
        abilities.stomp.SetState(State.Inactive);
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
