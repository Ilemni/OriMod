//using AnimLib.Abilities;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability for a quick and fast horizontal dash. May be used in the air.
  /// </summary>
  public sealed class ChargeDash : Ability {
    static ChargeDash() => OriMod.OnUnload += Unload;
    internal ChargeDash(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityId.ChargeDash;
    public override byte Level => (byte)(levelableDependency.Level >= 2 ? 1 : 0);
    protected override ILevelable levelableDependency => abilities.dash;

    internal override bool CanUse => base.CanUse && Refreshed && !InUse && !oPlayer.OnWall && !player.mount.Active &&
      !abilities.bash && !abilities.burrow && !abilities.launch && !abilities.lookUp && !abilities.stomp;
    protected override int Cooldown => Level >= 3 ? 0 : 90;
    protected override Color RefreshColor => Color.LightBlue;

    private static int ManaCost => 20;
    private static int Duration => Speeds.Length - 1;
    private static float[] Speeds => _speeds ?? (_speeds = new float[15] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
    });
    private static float[] _speeds;

    private ushort _npcId = ushort.MaxValue;
    private sbyte _direction;

    /// <summary>
    /// Check if <paramref name="npc"/> is <see cref="Target"/>.
    /// </summary>
    /// <param name="npc"><see cref="NPC"/> to check.</param>
    /// <returns><see langword="true"/> if <paramref name="npc"/> is <see cref="Target"/>, otherwise <see langword="false"/>.</returns>
    public bool NpcIsTarget(NPC npc) => npc.whoAmI == _npcId;

    /// <summary>
    /// Target of this Charge Dash. May be <see langword="null"/>.
    /// </summary>
    public NPC Target {
      get => _npcId < Main.npc.Length ? Main.npc[_npcId] : null;
      set => _npcId = (ushort)(value?.whoAmI ?? ushort.MaxValue);
    }

    private readonly RandomChar _rand = new RandomChar();

    protected override void ReadPacket(BinaryReader r) {
      _npcId = r.ReadUInt16();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(_npcId);
    }

    internal override void PutOnCooldown(bool force = false) {
      Refreshed = false;
      base.PutOnCooldown(force);
    }

    protected override void TickCooldown() {
      if (Refreshed && currentCooldown <= 0) return;
      currentCooldown--;
      if (currentCooldown < 0 && !input.charge.Current) {
        Refreshed = true;
      }
    }

    private void Start() {
      float tempDist = 720f * 720f;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC npc = Main.npc[n];
        if (!npc.active || npc.friendly || !Collision.CanHitLine(player.Center, player.width, player.height, npc.Center, 16, 16)) {
          continue;
        }

        float dist = (player.position - npc.position).LengthSquared();
        if (dist >= tempDist) continue;
        tempDist = dist;
        Target = npc;
      }
      _direction = Target is null
        ? (sbyte)(player.controlLeft ? -1 : player.controlRight ? 1 : player.direction)
        : (sbyte)(player.direction = player.position.X - Target.position.X < 0 ? 1 : -1);
      oPlayer.PlaySound("Ori/ChargeDash/seinChargeDash" + _rand.NextNoRepeat(3), 0.5f);
      NewAbilityProjectile<ChargeDashProjectile>(damage: 50);
    }

    /// <summary>
    /// End the Charge Dash. Ending behavior depends on <paramref name="byNpcContact"/>.
    /// </summary>
    /// <param name="byNpcContact">If the cause for ending is by player contact with <see cref="Target"/> (true), or for any other reason (false).</param>
    internal void End(bool byNpcContact = false) {
      SetState(State.Inactive);
      PutOnCooldown();
      NPC target = Target;
      if (byNpcContact) {
        // Force player position to same as target's, and reduce speed.
        player.position = target.position;
        player.position.Y -= 32f;
        player.velocity = player.velocity.Normalized() * Speeds[Speeds.Length - 1];
      }
      else if (Math.Abs(player.velocity.Y) < Math.Abs(player.velocity.X)) {
        // Reducing velocity. If intended direction is mostly flat (not moving upwards, not jumping), make it flat.
        Vector2 newVel = target is null && !abilities.airJump ? new Vector2(_direction, 0) : player.velocity;
        newVel = newVel.Normalized() * Speeds[Speeds.Length - 1];
        player.velocity = newVel;
      }
      Target = null;
    }

    protected override void UpdateUsing() {
      float speed = Speeds[CurrentTime];
      player.gravity = 0;
      NPC target = Target;

      if (target?.active ?? false) {
        player.maxFallSpeed = speed;
        Vector2 dir = target.Center - player.Center;
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed;
      }
      else {
        player.velocity.X = speed * _direction * 0.8f;
        player.velocity.Y = (oPlayer.IsGrounded ? -0.1f : 0.15f * (CurrentTime + 1)) * player.gravDir;
      }

      player.runSlowdown = 26f;
      oPlayer.immuneTimer = 12;
    }

    internal override void Tick() {
      if (IsLocal && CanUse && input.dash.JustPressed && input.charge.Current) {
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
      if (!InUse) return;
      abilities.dash.Refreshed = false;
      if (CurrentTime > Duration || oPlayer.OnWall || abilities.bash || abilities.launch || player.controlJump) {
        End();
      }
    }

    private static void Unload() {
      _speeds = null;
    }
  }
}
