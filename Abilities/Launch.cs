using System;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for launching the player in the desired direction. Used in the air.
  /// </summary>
  /// <remarks>
  /// Seems a common consensus that as an ability that is actually a leveled version of another ability,
  /// it would be more fitting as a Charge Jump Lv3, rather than Bash Lv3.
  /// This must be a separate class rather than built into Bash, as this would otherwise require
  /// Bash to be unlocked as well to be usable.
  /// </remarks>
  public sealed class Launch : Ability {
    internal Launch(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.Launch;
    public override byte Level => (byte)(abilities.chargeJump.Level >= 3 ? 1 : 0);

    /// <summary>
    /// Bash restrictions, plus in air and bash failed
    /// </summary>
    internal override bool CanUse => base.CanUse && Inactive && !oPlayer.IsGrounded && !abilities.stomp && !abilities.chargeJump && !abilities.bash && !abilities.climb;
    protected override int Cooldown => (int)(Config.BashCooldown * 30);

    private int MinLaunchDuration => 15;
    private int MaxLaunchDuration => 45;
    private int EndDuration => 16;
    private float LaunchSpeed => 15;

    private Vector2 startPos;
    public float launchAngle { get; private set; }
    public Vector2 launchDirection => new Vector2((float)Math.Cos(launchAngle), (float)Math.Sin(launchAngle));
    private readonly RandomChar rand = new RandomChar();

    protected override void ReadPacket(System.IO.BinaryReader r) {
      if (InUse) {
        if (Starting) {
          startPos = r.ReadVector2();
        }
        launchAngle = r.ReadSingle();
      }
    }

    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      if (InUse) {
        if (Starting) {
          packet.WriteVector2(startPos);
        }
        packet.Write(launchAngle);
      }
    }

    protected override void UpdateUsing() {
      if (!Ending) {
        launchAngle = player.AngleTo(Main.MouseWorld);
        player.velocity = Vector2.Zero;
        player.gravity = 0;
      }
      if (IsLocal) {
        netUpdate = true;
      }
      // Allow only quick heal and quick mana
      player.controlJump = false;
      player.controlUp = false;
      player.controlDown = false;
      player.controlLeft = false;
      player.controlRight = false;
      player.controlHook = false;
      player.controlInv = false;
      player.controlMount = false;
      player.controlSmart = false;
      player.controlThrow = false;
      player.controlTorch = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
      player.immune = true;
      player.buffImmune[BuffID.CursedInferno] = true;
      player.buffImmune[BuffID.Dazed] = true;
      player.buffImmune[BuffID.Frozen] = true;
      player.buffImmune[BuffID.Frostburn] = true;
      player.buffImmune[BuffID.MoonLeech] = true;
      player.buffImmune[BuffID.Obstructed] = true;
      player.buffImmune[BuffID.OnFire] = true;
      player.buffImmune[BuffID.Poisoned] = true;
      player.buffImmune[BuffID.ShadowFlame] = true;
      player.buffImmune[BuffID.Silenced] = true;
      player.buffImmune[BuffID.Slow] = true;
      player.buffImmune[BuffID.Stoned] = true;
      player.buffImmune[BuffID.Suffocation] = true;
      player.buffImmune[BuffID.Venom] = true;
      player.buffImmune[BuffID.Weak] = true;
      player.buffImmune[BuffID.WitheredArmor] = true;
      player.buffImmune[BuffID.WitheredWeapon] = true;
      player.buffImmune[BuffID.WindPushed] = true;
    }

    protected override void UpdateEnding() {
      player.velocity = launchDirection * LaunchSpeed;
    }

    private void End() {
      player.pulley = false;
      player.velocity = launchDirection * 15;
      player.immuneTime = 5;
      oPlayer.UnrestrictedMovement = true;
      oPlayer.PlayNewSound("Ori/Bash/seinBashEnd" + rand.NextNoRepeat(3), 0.5f);
      Refreshed = false;
    }

    internal override void Tick() {
      if (CanUse && IsLocal && OriMod.BashKey.JustPressed) {
        startPos = player.Center;
        oPlayer.PlayNewSound("Ori/Bash/seinBashStartA", 0.5f, localOnly: true);
        SetState(State.Starting);
        return;
      }
      else if (InUse) {
        if (Starting) {
          if (CurrentTime > MinLaunchDuration) {
            SetState(State.Active, preserveCurrentTime: true);
          }
          return;
        }
        if (Active) {
          if (CurrentTime == MinLaunchDuration + 4) {
            oPlayer.PlayNewSound("Ori/Bash/seinBashLoopA", 0.5f, localOnly: true);
          }
          if (CurrentTime > MaxLaunchDuration || IsLocal && !OriMod.BashKey.Current) {
            End();
            SetState(State.Ending);
          }
          return;
        }
        if (Ending) {
          if (CurrentTime > EndDuration) {
            SetState(State.Inactive);
          }
        }
      }
      else {
        TickCooldown();
      }
    }

    protected override void TickCooldown() {
      if (oPlayer.IsGrounded || oPlayer.OnWall || abilities.bash) {
        Refreshed = true;
      }
    }
  }
}
