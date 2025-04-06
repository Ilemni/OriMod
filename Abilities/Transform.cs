using AnimLib.Animations;
using AnimLib.Menus.Debug;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace OriMod.Abilities;

public sealed class Transform : OriState {
  private const string HasTransformedOnceKey = "HasTransformedOnce";
  private static float RepeatedTransformSpeed => 1.8f;

  private static int StartDuration => 182;
  private static int MidDuration => StartDuration + 10;
  private static int LateEndDuration => MidDuration + 225;
  private static int EarlyEndDuration => MidDuration + 68;


  private int _startDirection;
  private float _transformTime;
  private SoundInfo _startSound = new("AbilityPedestal/abilityPedestalMusic", 0, 0.25f);


  public bool Starting => _transformTime < MidDuration;
  private int EndDuration => _hasTransformedOnce ? EarlyEndDuration : LateEndDuration;
  private bool _hasTransformedOnce;

  protected override void OnEnter(State? fromState) {
    _startDirection = Player.direction;
    _transformTime = 0;
    if (!_hasTransformedOnce) {
      _startSound.Play(Player);
    }
  }

  protected override void OnExit(State? toState) {
    _hasTransformedOnce = true;
  }

  protected override void NetSync(NetSyncer sync) {
    sync.Sync(ref _hasTransformedOnce);
  }

  public override bool ImmuneTo(PlayerDeathReason damageSource, int cooldownCounter, bool dodgeable) => Active;

  public override void SetControls() {
    Player.controlLeft = false;
    Player.controlRight = false;
    Player.controlUp = false;
    Player.controlDown = false;
    Player.controlJump = false;
    Player.controlMount = false;
    Player.controlHook = false;
    Player.controlUseItem = false;
  }

  public override void ResetEffects() {
    _transformTime += _hasTransformedOnce ? RepeatedTransformSpeed : 1;
  }

  public override void PostUpdateRunSpeeds() {
    Player.ChangeDir(_startDirection);
    Player.runAcceleration = 0;
    Player.maxRunSpeed = 0;

    if (_transformTime < StartDuration) {
      // Starting
      Player.velocity = new Vector2(0, -0.0003f * (StartDuration * 1.5f - _transformTime));
      Player.gravity = 0;
      Character.CreatePlayerDust();
    }
    else if (_transformTime < MidDuration) {
      // Near end of start
      Player.gravity = 9f;
    }
    else if (_transformTime >= EndDuration) {
      CancelState();
    }
  }

  public override AnimationOptions? GetAnimationOptions() {
    return Anim.HasTag("Transform")
      ? new AnimationOptions("Transform") { Speed = _hasTransformedOnce ? RepeatedTransformSpeed : 1 }
      : new AnimationOptions("Idle");
  }

  protected override void DebugText(UIStateInfo ui) {
    base.DebugText(ui);
    ui.DrawAppendLabelProgressBar("Duration", _transformTime, [MidDuration, EndDuration]);

    ui.DrawAppendBoolean(_hasTransformedOnce);
  }

  public override void LoadData(TagCompound tag) {
    if (tag.TryGet(HasTransformedOnceKey, out bool hasTransformedOnce)) {
      _hasTransformedOnce = hasTransformedOnce;
    }
  }

  public override void SaveData(TagCompound tag) {
    if (_hasTransformedOnce) {
      tag.Set(HasTransformedOnceKey, true);
    }
  }
}
