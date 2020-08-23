using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a quick and fast horizontal dash. May be used in the air.
  /// </summary>
  public sealed class ChargeDash : Ability {
    static ChargeDash() => OriMod.OnUnload += Unload;
    internal ChargeDash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.ChargeDash;

    internal override bool UpdateCondition => !Manager.dash.UpdateCondition && (InUse || oPlayer.Input(OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) && Manager.chargeDash.Refreshed);
    internal override bool CanUse => base.CanUse && Refreshed && !InUse && !oPlayer.OnWall && !Manager.stomp.InUse && !Manager.bash.InUse && !player.mount.Active;
    protected override int Cooldown => (int)(Config.CDashCooldown * 30);
    protected override Color RefreshColor => Color.LightBlue;

    private int ManaCost => 20;
    private int Duration => Speeds.Length - 1;
    private static float[] Speeds => _speeds ?? (_speeds = new float[15] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
    });
    private static float[] _speeds;

    private static float SpeedMultiplier => Config.CDashSpeedMultiplier * 0.8f;

    private ushort NpcID = ushort.MaxValue;
    private sbyte Direction;

    /// <summary>
    /// Check if <paramref name="npc"/> is <see cref="Target"/>.
    /// </summary>
    /// <param name="npc"><see cref="NPC"/> to check.</param>
    /// <returns>True if <paramref name="npc"/> is <see cref="Target"/>.</returns>
    public bool NpcIsTarget(NPC npc) => npc.whoAmI == NpcID;
    
    /// <summary>
    /// Target of this Charge Dash. May be null.
    /// </summary>
    public NPC Target {
      get {
        if (NpcID >= Main.npc.Length) {
          return null;
        }
        return Main.npc[NpcID];
      }
      set => NpcID = (ushort)(value?.whoAmI ?? ushort.MaxValue);
    }
    
    /// <summary>
    /// Projectile created while Charge Dashing to damage enemies.
    /// </summary>
    /// <remarks>
    /// This damage aspect is derived from the Ori games. May be unbalanced or unnecessary here.
    /// </remarks>
    public Projectile PlayerHitboxProjectile { get; private set; }

    private readonly RandomChar rand = new RandomChar();

    protected override void ReadPacket(System.IO.BinaryReader r) {
      NpcID = r.ReadUInt16();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(NpcID);
    }

    internal override void PutOnCooldown(bool force = false) {
      Refreshed = false;
      base.PutOnCooldown(force);
    }

    protected override void TickCooldown() {
      if (!Refreshed || currentCooldown > 0) {
        currentCooldown--;
        if (currentCooldown < 0 && !OriMod.ChargeKey.Current) {
          Refreshed = true;
        }
      }
    }

    /// <summary>
    /// End the Charge Dash. Ending behavior depends on <paramref name="byNpcContact"/>.
    /// </summary>
    /// <param name="byNpcContact">If the cause for ending is by player contact with <see cref="Target"/> (true), or for any other reason (false)</param>
    internal void End(bool byNpcContact = false) {
      SetState(State.Inactive);
      PutOnCooldown();
      var target = Target;
      Target = null;
      if (byNpcContact) {
        // Force player position to same as target's, and reduce speed.
        player.position = target.position;
        player.position.Y -= 32f;
        player.velocity *= Speeds[currentTime] * SpeedMultiplier < 50 ? 0.5f : 0.25f;
      }
      else if ((target is null || currentTime > 4) && Math.Abs(player.velocity.Y) < Math.Abs(player.velocity.X)) {
        // Reducing velocity. If intended direction is mostly flat (not moving upwards, not jumping), make it flat.
        Vector2 newVel = target is null && !Manager.airJump.InUse ? new Vector2(Direction, 0) : player.velocity;
        newVel.Normalize();
        newVel *= Speeds[Speeds.Length - 1] * SpeedMultiplier;
        player.velocity = newVel;
      }
      PlayerHitboxProjectile = null;
      Target = null;
    }

    protected override void UpdateStarting() {
      float tempDist = 720f;
      int tempNPC = -1;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC localNpc = Main.npc[n];
        if (!localNpc.active || localNpc.friendly) {
          continue;
        }

        float dist = (player.position - localNpc.position).Length();
        if (dist < tempDist) {
          tempDist = dist;
          tempNPC = localNpc.whoAmI;
        }
      }
      if (tempNPC != -1) {
        Target = null;
        Direction = (sbyte)(player.direction = player.position.X - Target.position.X < 0 ? 1 : -1);
      }
      else {
        Direction = (sbyte)(PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction);
      }
      oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + rand.NextNoRepeat(3), 0.5f);
      PlayerHitboxProjectile = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, ModContent.ProjectileType<ChargeDashProjectile>(), 30, 0f, player.whoAmI, 0, 1)];
      PlayerHitboxProjectile.damage = 12 + (int)OriWorld.GlobalUpgrade * 9;
    }

    protected override void UpdateUsing() {
      float speed = Speeds[currentTime] * SpeedMultiplier;
      player.gravity = 0;
      var target = Target;

      if (target?.active ?? false) {
        player.maxFallSpeed = speed;
        Vector2 dir = target.position - player.position;
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed;
        if (currentTime < Duration && (player.position - target.position).Length() < speed) {
          End(byNpcContact: true);
        }
      }
      else {
        player.velocity.X = speed * Direction * 0.8f;
        player.velocity.Y = (oPlayer.IsGrounded ? -0.1f : 0.15f * (currentTime + 1)) * player.gravDir;
      }

      player.runSlowdown = 26f;
      oPlayer.immuneTimer = 12;
    }

    internal override void Tick() {
      if (CanUse && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
        if (player.CheckMana(ManaCost, true, true)) {
          SetState(State.Active);
          currentTime = 0;
          UpdateStarting();
        }
        else if (!Manager.dash.InUse) {
          Manager.dash.StartDash();
        }
        return;
      }
      TickCooldown();
      if (InUse) {
        SetState(State.Inactive);
        Manager.dash.Refreshed = false;
        currentTime++;
        if (currentTime > Duration || oPlayer.OnWall || Manager.bash.InUse || PlayerInput.Triggers.JustPressed.Jump) {
          End();
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
