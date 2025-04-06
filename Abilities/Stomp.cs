using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using System;
using System.Collections.Generic;
using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace OriMod.Abilities;

/// <summary>
/// Ability for an air-to-ground Area of Effect attack.
/// </summary>
public sealed class Stomp : OriAbility {
  private static float Gravity => 6f;

  private static int MinDuration => 30;

  private static int EndDuration => 50;
  private static int EndEarlyDuration => 20;

  /// <summary>
  /// Minimum frames required to hold <see cref="Player.controlDown"/> before Stomp can start.
  /// </summary>
  private static int HoldDownDelay => (int)(OriMod.ConfigClient.stompHoldDownDelay * 30);


  private int _controlDownDuration;
  private bool _wasControlDownLastFrame;

  private SoundInfo _startSound = new("Ori/Stomp/seinStompStart", 3, 0.8f, 0.2f);
  private SoundInfo _endSound = new("Ori/Stomp/seinStompImpact", 3, 0.9f);
  private SoundInfo _activeSound = new("Ori/Stomp/seinStompFall", 3, 0.8f);


  private bool Starting => ActiveTime < Stats.StartDuration;
  public bool Ending { get; private set; }
  private int _endingTime;


  public override int MaxLevel => 3;
  public override int MaxCooldown => Stats.MaxCooldown;
  private ref StompStats Stats => ref IStats<StompStats>.Get(Level);

  public override bool ShowHintInUI => Main.hardMode;

  private void ImpactEffects() {
    if (!IsGrounded) {
      return;
    }

    StartCooldown();
    _endSound.Play(Player);
    RestoreAirJumps();
    Player.velocity = Vector2.Zero;
    Vector2 position = new(Player.position.X, Player.position.Y + 32);
    for (int i = 0; i < 25; i++) {
      Dust dust = Dust.NewDustDirect(position, 30, 15, DustID.Clentaminator_Cyan, 0f, 0f, 0, Color.White);
      dust.shader = GameShaders.Armor.GetSecondaryShader(19, Main.LocalPlayer);
      dust.velocity *= new Vector2(6, 1.5f);
      dust.velocity.Y = -Math.Abs(dust.velocity.Y);
    }

    NewAbilityProjectile<StompEnd>(damage: Stats.Damage);
  }

  public override void RegisterInterruptibles(List<State> interruptibles) {
    interruptibles.AddRange([
      GetState<NoAbility.Jumping>(),
      GetState<NoAbility.Falling>(),
    ]);
  }

  public override bool CanEnter() => base.CanEnter() && !IsGrounded && Player.grapCount == 0;

  protected override void OnEnter(State? fromState) {
    _startSound.Play(Player);
  }

  protected override void OnExit(State? toState) {
    Ending = false;
    _endingTime = 0;
  }

  public override void SetControls() {
    Player.controlUp = false;
    Player.controlJump = false;
    Player.controlHook = false;
    Player.controlMount = false;
    Player.controlThrow = false;
    Player.controlUseItem = false;
    Player.controlUseTile = false;
    if (Starting) {
      Player.controlLeft = false;
      Player.controlRight = false;
    }
  }

  protected override bool UpdateInterrupt(State activeState) {
    if (!IsLocal) {
      return false;
    }

    if (!Input.Stomp.Current) {
      _controlDownDuration = 0;
      return false;
    }

    if (_controlDownDuration == 0 && !Input.Stomp.JustPressed) {
      return false;
    }

    if (Character.InAirTime < 5) {
      return false;
    }


    _controlDownDuration++;
    return _controlDownDuration > HoldDownDelay;
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) =>
    Active && !Starting;

  public override void PostUpdateMiscEffects() {
    if (Starting && Input.Jump.JustPressed) {
      if (TriggerState<AirJump>()) {
        return;
      }
    }

    if (ActiveTime > MinDuration && !Input.Stomp.Current && !Ending) {
      // Allow cancelling if player doesn't want to commit to the stomp
      CancelState();
    }

    if (Ending) {
      _endingTime++;
      if (_endingTime >= EndDuration || (_endingTime >= EndEarlyDuration && !Player.controlDown)) {
        CancelState();
      }
    }

    if (!IsGrounded) {
      return;
    }

    if (!Ending) {
      Ending = true;
      ImpactEffects();
    }
  }

  public override void PostUpdateRunSpeeds() {
    Player.RemoveAllGrapplingHooks();

    if (Starting) {
      Player.velocity.X = 0;
      Player.velocity.Y *= 0.8f;
      Player.gravity = -0.2f;
      return;
    }

    ref StompStats stats = ref Stats;
    if (ActiveTime == stats.StartDuration) {
      _activeSound.Play(Player);
      NewAbilityProjectile<StompProjectile>(damage: stats.Damage * 2);
    }

    Player.maxRunSpeed = 1f;
    Player.runSlowdown = 8;
    Player.gravity = Gravity;
    Player.maxFallSpeed = stats.MaxFallSpeed;
  }

  protected override void NetSync(NetSyncer sync) {
    sync.SyncPositionAndVelocity(Player);
  }

  protected override void OnEndCooldown(bool justCooledDown) {
    if (justCooledDown) {
      RefreshParticles(Color.Orange);
    }
  }

  public override AnimationOptions? GetAnimationOptions() {
    if (Starting) {
      return Anim.HasTag("StompStart")
        ? new AnimationOptions("StompStart")
        : new AnimationOptions("AirJump") { Rotation = ActiveTime * 0.8f };
    }

    if (Ending) {
      return Anim.HasTag("StompEnd")
        ? new AnimationOptions("StompEnd")
        : new AnimationOptions("Crouch");
    }

    return Anim.HasTag("Stomp")
      ? new AnimationOptions("Stomp")
      : new AnimationOptions("ChargeJump") {
        Speed = 2,
        Rotation = MathF.PI / 2f + Player.velocity.ToRotation(),
        LoopCount = 0,
        IsPingPong = true
      };
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendBoolean(Starting);
    ui.DrawAppendBoolean(Ending);
    ui.DrawAppendLabelValue(_endingTime);
    ui.DrawAppendLabelProgressBar("Duration", ActiveTime, [Stats.StartDuration, MinDuration], Color.Green);
  }

  private readonly record struct StompStats(
    int Damage,
    int StartDuration,
    int MaxFallSpeed,
    int MaxCooldown) : IStats<StompStats> {
    public static StompStats[] Values { get; } = [
      new(Damage: 50, StartDuration: 24, MaxFallSpeed: 28, MaxCooldown: 60),
      new(Damage: 70, StartDuration: 20, MaxFallSpeed: 36, MaxCooldown: 90),
      new(Damage: 90, StartDuration: 16, MaxFallSpeed: 40, MaxCooldown: 120),
    ];
  }
}
