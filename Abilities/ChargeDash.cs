using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a quick and fast horizontal dash. May be used in the air.
  /// </summary>
  public sealed class ChargeDash : Ability {
    static ChargeDash() => OriMod.OnUnload += Unload;
    internal ChargeDash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.ChargeDash;
    public override byte Level => (byte)(abilities.dash.Level >= 3 ? 1 : 0);

    internal override bool CanUse => base.CanUse && Refreshed && !InUse && !oPlayer.OnWall && !abilities.stomp && !abilities.bash && !abilities.launch && !player.mount.Active;
    protected override int Cooldown => (int)(Config.CDashCooldown * 30);
    protected override Color RefreshColor => Color.LightBlue;

    private static int ManaCost => 20;
    private static int Duration => Speeds.Length - 1;
    private static float[] Speeds => _speeds ?? (_speeds = new float[15] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
    });
    private static float[] _speeds;

    private static float SpeedMultiplier => Config.CDashSpeedMultiplier * 0.8f;

    private ushort npcID = ushort.MaxValue;
    private sbyte direction;

    /// <summary>
    /// Check if <paramref name="npc"/> is <see cref="Target"/>.
    /// </summary>
    /// <param name="npc"><see cref="NPC"/> to check.</param>
    /// <returns><see langword="true"/> if <paramref name="npc"/> is <see cref="Target"/>, otherwise <see langword="false"/>.</returns>
    public bool NpcIsTarget(NPC npc) => npc.whoAmI == npcID;

    /// <summary>
    /// Target of this Charge Dash. May be <see langword="null"/>.
    /// </summary>
    public NPC Target {
      get => npcID < Main.npc.Length ? Main.npc[npcID] : null;
      set => npcID = (ushort)(value?.whoAmI ?? ushort.MaxValue);
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
      npcID = r.ReadUInt16();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(npcID);
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

    private void Start() {
      float tempDist = 720f * 720f;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC localNpc = Main.npc[n];
        if (!localNpc.active || localNpc.friendly) {
          continue;
        }

        float dist = (player.position - localNpc.position).LengthSquared();
        if (dist < tempDist) {
          tempDist = dist;
          Target = localNpc;
        }
      }
      direction = Target is null
        ? (sbyte)(player.controlLeft ? -1 : player.controlRight ? 1 : player.direction)
        : (sbyte)(player.direction = player.position.X - Target.position.X < 0 ? 1 : -1);
      oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + rand.NextNoRepeat(3), 0.5f);
      NewAbilityProjectile<ChargeDashProjectile>(damage: 30);
    }

    /// <summary>
    /// End the Charge Dash. Ending behavior depends on <paramref name="byNpcContact"/>.
    /// </summary>
    /// <param name="byNpcContact">If the cause for ending is by player contact with <see cref="Target"/> (true), or for any other reason (false).</param>
    internal void End(bool byNpcContact = false) {
      SetState(State.Inactive);
      PutOnCooldown();
      var target = Target;
      if (byNpcContact) {
        // Force player position to same as target's, and reduce speed.
        player.position = target.position;
        player.position.Y -= 32f;
        player.velocity *= Speeds[CurrentTime] * SpeedMultiplier < 50 ? 0.5f : 0.25f;
      }
      else if ((target is null || CurrentTime > 4) && Math.Abs(player.velocity.Y) < Math.Abs(player.velocity.X)) {
        // Reducing velocity. If intended direction is mostly flat (not moving upwards, not jumping), make it flat.
        Vector2 newVel = target is null && !abilities.airJump ? new Vector2(direction, 0) : player.velocity;
        newVel.Normalize();
        newVel *= Speeds[Speeds.Length - 1] * SpeedMultiplier;
        player.velocity = newVel;
      }
      Target = null;
    }

    protected override void UpdateUsing() {
      float speed = Speeds[CurrentTime] * SpeedMultiplier;
      player.gravity = 0;
      var target = Target;

      if (target?.active ?? false) {
        player.maxFallSpeed = speed;
        Vector2 dir = target.position - player.position;
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed;
        if (CurrentTime < Duration && (player.position - target.position).LengthSquared() < speed * speed) {
          End(byNpcContact: true);
        }
      }
      else {
        player.velocity.X = speed * direction * 0.8f;
        player.velocity.Y = (oPlayer.IsGrounded ? -0.1f : 0.15f * (CurrentTime + 1)) * player.gravDir;
      }

      player.runSlowdown = 26f;
      oPlayer.immuneTimer = 12;
    }

    internal override void Tick() {
      if (IsLocal && CanUse && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
        if (player.CheckMana(ManaCost, true, true)) {
          SetState(State.Active);
          Start();
        }
        else if (!abilities.dash) {
          abilities.dash.StartDash();
        }
        return;
      }
      TickCooldown();
      if (InUse) {
        abilities.dash.Refreshed = false;
        if (CurrentTime > Duration || oPlayer.OnWall || abilities.bash || abilities.launch || player.controlJump) {
          End();
        }
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
