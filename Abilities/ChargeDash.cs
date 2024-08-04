using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using OriMod.Utilities;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities;

/// <summary>
/// Ability for a quick and fast horizontal dash. May be used in the air.
/// </summary>
public sealed class ChargeDash : OriAbility {
  public override int Id => AbilityId.ChargeDash;
  public override bool Unlocked => LevelableDependency.Level >= 2;
  public override ILevelable LevelableDependency => Abilities.Dash;

  public override bool CanUse => base.CanUse && !IsOnCooldown && !InUse && !OnWall && !Player.mount.Active &&
    !Abilities.Bash && !Abilities.Burrow && !Abilities.Launch && !Abilities.Stomp && !Abilities.ChargeJump && !Abilities.WallChargeJump;
  public override int Cooldown => LevelableDependency.Level >= 3 ? 60 : 90;
  public override void OnRefreshed() => Abilities.RefreshParticles(Color.LightBlue);

  private static int ManaCost => 25;
  private static float MaxRange => 480f;
  private static int Duration => Speeds.Length - 1;
  private static float[] Speeds => _speeds ??= Unloadable.New(new float[15] {
    100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
  }, () => _speeds = null);
  private static float[] _speeds;

  private ushort _npcId = ushort.MaxValue;
  private sbyte _direction;

  private Vector2 _startDirection;

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

  private RandomChar _rand;

  public override void ReadPacket(BinaryReader r) {
    _npcId = r.ReadUInt16();
    Player.position = r.ReadVector2();
    Player.velocity = r.ReadVector2();
  }

  public override void WritePacket(ModPacket packet) {
    packet.Write(_npcId);
    packet.WriteVector2(Player.position);
    packet.WriteVector2(Player.velocity);
  }

  //internal override void PutOnCooldown(bool force = false) {
  //  Refreshed = false;
  //  base.PutOnCooldown(force);
  //}

  public override bool RefreshCondition() => !Input.Charge.Current;

  private void Start() {
    if(IsLocal && OriMod.ConfigClient.eChargeDashHoming) {
      Player.manaRegenDelay = (int)Player.maxRegenDelay;
      float tempDist = MaxRange*MaxRange*4;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC npc = Main.npc[n];
        if (!npc.active || npc.friendly ||
          (Player.Center - npc.Center).LengthSquared() > MaxRange*MaxRange ||
          !Collision.CanHitLine(Player.Center, Player.width, Player.height, npc.Center, 16, 16)
        ) continue;

        float dist = (Main.MouseWorld - npc.Center).LengthSquared();
        if (dist >= tempDist) continue;
        tempDist = dist;
        Target = npc;
      }
    }
    _direction = !(IsLocal && OriMod.ConfigClient.eChargeDashHoming) || Target is null
      ? (sbyte)(Player.controlLeft ? -1 : Player.controlRight ? 1 : Player.direction)
      : (sbyte)(Player.direction = Player.position.X - Target.position.X < 0 ? 1 : -1);
    if (Target is not null) {
      Vector2 dir = Target.Center - Player.Center;
      dir.Y -= 32f;
      dir.Normalize();
      _startDirection = dir;
    }

    PlaySound("Ori/ChargeDash/seinChargeDash" + _rand.NextNoRepeat(3), 0.5f);
    NewAbilityProjectile<ChargeDashProjectile>(damage: 50);
  }

  /// <summary>
  /// End the Charge Dash. Ending behavior depends on <paramref name="byNpcContact"/>.
  /// </summary>
  /// <param name="byNpcContact">If the cause for ending is by player contact with <see cref="Target"/> (true), or for any other reason (false).</param>
  internal void End(bool byNpcContact = false) {
    SetState(AbilityState.Inactive);
    StartCooldown();
    NPC target = Target;
    if (byNpcContact) {
      // Force player position to same as target's, and reduce speed.
      Player.position = target.position;
      Player.position.Y -= 32f;
      Player.velocity = _startDirection * Speeds[^1];
      RestoreAirJumps();
    }
    else if (Math.Abs(Player.velocity.Y) < Math.Abs(Player.velocity.X)) {
      // Reducing velocity. If intended direction is mostly flat (not moving upwards, not jumping), make it flat.
      Vector2 newVel = target is null && !Abilities.AirJump ? new Vector2(_direction, 0) : Player.velocity;
      newVel = newVel.Normalized() * Speeds[^1];
      Player.velocity = newVel;
    }
    Target = null;
  }

  public override void UpdateActive() {
    if (IsLocal) NetUpdate = true;
  }

  public override void UpdateUsing() {
    float speed = Speeds[StateTime];
    Player.gravity = 0;
    NPC target = Target;

    if (target?.active ?? false) {
      Player.maxFallSpeed = speed;
      Vector2 dir = target.Center - Player.Center;
      dir.Y -= 32f;
      dir.Normalize();
      Player.velocity = dir * speed;
    }
    else {
      Player.velocity.X = speed * _direction * 0.8f;
      Player.velocity.Y = (IsGrounded ? -0.1f : 0.15f * (StateTime + 1)) * Player.gravDir;
    }

    Player.runSlowdown = 26f;
    oPlayer.ImmuneTimer = 12;
  }

  public override void PreUpdate() {
    if (IsLocal && CanUse && Input.Dash.JustPressed && Input.Charge.Current) {
      if (Player.CheckMana(ManaCost, true, true)) {
        SetState(AbilityState.Active);
        Start();
      }
      else if (!Abilities.Dash) {
        Abilities.Dash.StartDash();
      }
      return;
    }
    if (!InUse) return;
    UpdateCooldown();
    Abilities.Dash.StartCooldown();
    if (StateTime > Duration || OnWall || Abilities.Bash || Abilities.Launch || Player.controlJump) {
      End();
    }
  }
}
