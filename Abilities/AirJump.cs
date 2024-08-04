using AnimLib.Abilities;
using OriMod.Utilities;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for jumping in the air.
/// </summary>
public sealed class AirJump : OriAbility, ILevelable {
  public override int Id => AbilityId.AirJump;
  public override int Level => ((ILevelable)this).Level;
  public override bool Unlocked => Level > 0;
  int ILevelable.Level { get; set; }
  public int MaxLevel => 4;

  public override bool CanUse => base.CanUse && !IsGrounded && !OnWall &&
    CurrentCount < MaxJumps && !Player.mount.Active && !(Abilities.ChargeJump.Charged && Abilities.ChargeJump.Grace) &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.Climb && !Abilities.ChargeJump && !Abilities.Launch &&
    !Abilities.WallChargeJump && !(OriMod.ConfigClient.airJumpCondition == "Not Down" && Player.controlDown) &&
    !(OriMod.ConfigClient.airJumpCondition == "Only Up" && !Player.controlUp);

  private static float JumpVelocity => 8.8f;
  private static int EndDuration => 32;
  private int MaxJumps => Level;

  internal ushort CurrentCount;
  private sbyte _gravityDirection;

  public override void ReadPacket(BinaryReader r) {
    CooldownLeft = r.ReadInt32();
    _gravityDirection = r.ReadSByte();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
    CurrentCount = r.ReadUInt16();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(CooldownLeft);
    packet.Write(_gravityDirection);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
    packet.Write(CurrentCount);
  }

  private RandomChar _rand;

  public override void UpdateActive() {
    float newVel = -JumpVelocity * ((float)(EndDuration - StateTime) / EndDuration) * _gravityDirection;
    Player.velocity.Y = newVel;
  }

  public override void PreUpdate() {
    if (CanUse && Input.Jump.JustPressed && IsLocal) {
      if (Player.AnyExtraJumpUsable() || Player.mount.Active) return;
      SetState(AbilityState.Active);
      CurrentCount++;
      _gravityDirection = (sbyte)Player.gravDir;

      if (Abilities.Glide) {
        PlaySound("Ori/Glide/seinGlideStart" + _rand.NextNoRepeat(3), 0.8f);
      }
      else if (MaxJumps != 1 && CurrentCount == MaxJumps) {
        PlaySound("Ori/TripleJump/seinTripleJumps" + _rand.NextNoRepeat(5), 0.6f);
      }
      else {
        PlaySound("Ori/DoubleJump/seinDoubleJumps" + _rand.NextNoRepeat(4), 0.5f);
      }
      return;
    }
    if (IsGrounded || Abilities.Bash || Abilities.Launch || Abilities.Climb) {
      SetState(AbilityState.Inactive);
    }
    if (Active) {
      SetState(AbilityState.Ending);
    }
    else if (Ending) {
      if (StateTime > EndDuration || Player.velocity.Y * Player.gravDir > 0) {
        SetState(AbilityState.Inactive);
        if (CurrentCount == MaxJumps) return;
      }
    }
    // Other than activation, Air Jump is deterministic and requires no additional syncing
    NetUpdate = false;
  }
}
