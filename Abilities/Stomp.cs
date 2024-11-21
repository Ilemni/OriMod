using Microsoft.Xna.Framework;
using OriMod.Projectiles.Abilities;
using System;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace OriMod.Abilities;

/// <summary>
/// Ability for an air-to-ground Area of Effect attack.
/// </summary>
public sealed class Stomp(Player player) : OriAbility(player) {
  private static float Gravity => 8f;

  private static int MinDuration => 30;

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

  private bool JustPressedStomp => Input.Stomp.JustPressed || (Player.controlDown && !_wasControlDownLastFrame);


  public override int MaxLevel => 3;
  public override int MaxCooldown => Stats.MaxCooldown;
  private ref StompStats Stats => ref IStats<StompStats>.Get(Level);

  protected override void OnInitialize() {
    base.OnInitialize();
    MovementStates parent = GetParent<MovementStates>();

    parent.AddInterruptible<NoAbility>(to: this);
  }

  public override bool CanEnter() => base.CanEnter() && !IsGrounded && Player.grapCount == 0;

  protected override void OnEnter(State? fromState) {
    base.OnEnter(fromState);
    _startSound.Play(Player);
  }

  protected override void OnExit() {
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

  protected override bool OnPreUpdateInterruptible(State activeState) {
    if (!IsLocal || IsGrounded || activeState is Climb or NoAbility { ActiveChild: NoAbility.WallSlide }) {
      return false;
    }

    if (!Input.Stomp.Current || !Player.controlDown) {
      _controlDownDuration = 0;
      return false;
    }

    bool justPressedStomp = JustPressedStomp;
    _wasControlDownLastFrame = Player.controlDown;

    if (_controlDownDuration == 0 && !justPressedStomp) {
      return false;
    }


    _controlDownDuration++;
    return _controlDownDuration > HoldDownDelay;
  }

  protected override void OnPreUpdate() {
    if (Starting && Input.Jump.JustPressed) {
      if (TriggerState<AirJump>()) {
        return;
      }
    }

    if (ActiveTime > MinDuration && !Player.controlDown) {
      // Allow cancelling if player doesn't want to commit to the stomp
      CancelState();
    }

    if (IsGrounded) {
      CancelState();
    }
  }

  protected override void OnUpdate() {
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlJump = false;
    Player.controlHook = false;
    Player.controlMount = false;
    Player.controlThrow = false;
    Player.controlUseItem = false;
    Player.controlUseTile = false;
    Player.RemoveAllGrapplingHooks();

    if (Starting) {
      Player.controlLeft = false;
      Player.controlRight = false;
      Player.velocity.X = 0;
      Player.velocity.Y *= 0.9f;
      Player.gravity = -0.1f;
    }
    else {
      ref StompStats stats = ref Stats;
      if (ActiveTime == stats.StartDuration) {
        _activeSound.Play(Player);
        NewAbilityProjectile<StompProjectile>(damage: stats.Damage * 2);
      }

      Player.maxRunSpeed = 1f;
      Player.runSlowdown = 8;
      Player.gravity = Gravity;
      Player.maxFallSpeed = stats.MaxFallSpeed;
      OriPlayer.SetImmune(12);
    }
  }

  protected override void NetSync(ISync sync) {
    sync.SyncPositionAndVelocity(Player);
  }

  protected override void OnEndCooldown() => RefreshParticles(Color.Orange);

  protected override AnimationOptions? GetAnimationOptions() {
    return Starting
      ? new AnimationOptions("AirJump", rotation: ActiveTime * 0.8f)
      : new AnimationOptions("ChargeJump", speed: 2, rotation: (float)Math.PI, loopCount: 0, isPingPong: true);
  }

  protected override void DebugText(DebugUIState ui) {
    base.DebugText(ui);
    ui.DrawAppendBoolean(Starting);
  }

  private readonly record struct StompStats(
    int Damage,
    int StartDuration,
    int MaxFallSpeed,
    int MaxCooldown) : IStats<StompStats> {
    public static ref StompStats[] Values => ref _values;

    private static StompStats[] _values = [
      default,
      new StompStats(Damage: 50, StartDuration: 24, MaxFallSpeed: 28, MaxCooldown: 60),
      new StompStats(Damage: 70, StartDuration: 20, MaxFallSpeed: 36, MaxCooldown: 90),
    ];

    public static StompStats CreateFromLevel(int level) => new(
      Damage: 30 + level * 20,
      StartDuration: 16,
      MaxFallSpeed: 25 + level * 5,
      MaxCooldown: Math.Min(30 + level * 30, 600));
  }
}
