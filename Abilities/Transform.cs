using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using AnimLib.UI.Debug;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Abilities;

public sealed class Transform(Player player) : OriState(player) {
  private static float RepeatedTransformSpeed => 1.8f;

  private static int StartDuration => 182;
  private static int MidDuration => StartDuration + 10;
  private static int LateEndDuration => MidDuration + 225;
  private static int EarlyEndDuration => MidDuration + 68;


  private int _startDirection;
  private float _transformTime;
  private SoundInfo _startSound = new("AbilityPedestal/abilityPedestalMusic", 0, 0.25f);


  public bool Starting => _transformTime < MidDuration;
  private int EndDuration => HasTransformedOnce ? EarlyEndDuration : LateEndDuration;
  private ref bool HasTransformedOnce => ref OriPlayer.HasTransformedOnce;

  protected override void OnEnter(State? fromState) {
    _startDirection = Player.direction;
    _transformTime = 0;
    if (!HasTransformedOnce) {
      _startSound.Play(Player);
    }
  }

  protected override void OnExit() {
    HasTransformedOnce = true;
  }

  protected override void NetSync(ISync sync) {
    sync.Sync(ref HasTransformedOnce);
  }

  protected override void OnPreUpdate() {
    _transformTime += HasTransformedOnce ? RepeatedTransformSpeed : 1;
  }

  protected override void OnUpdate() {
    Player.ChangeDir(_startDirection);
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlJump = false;
    Player.controlMount = false;
    Player.controlHook = false;
    Player.controlUseItem = false;
    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;
    OriPlayer.SetImmune(2);

    if (_transformTime < StartDuration) {
      // Starting
      Player.velocity = new Vector2(0, -0.0003f * (StartDuration * 1.5f - _transformTime));
      Player.gravity = 0;
      OriPlayer.CreatePlayerDust();
    }
    else if (_transformTime < MidDuration) {
      // Near end of start
      Player.gravity = 9f;
    }
    else if (_transformTime >= EndDuration) {
      CancelState();
    }
  }

  protected override AnimationOptions? GetAnimationOptions() {
    return new AnimationOptions("Transform", speed: HasTransformedOnce ? RepeatedTransformSpeed : 1);
  }

  protected override void DebugText(DebugUIState ui) {
    base.DebugText(ui);
    ui.DrawAppendBoolean(HasTransformedOnce);
  }
}
