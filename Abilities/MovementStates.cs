using AnimLib.Animations;
using AnimLib.States;
using ReLogic.Content;
using Terraria;

namespace OriMod.Abilities;

/// <summary>
/// Root <see cref="AnimatedStateMachine"/> state which contains all main Ori states as children states.
/// </summary>
public sealed class MovementStates(Player player) : AnimatedStateMachine(player) {
  private Player Player => (Player)Entity;
  private OriPlayer OriPlayer => _oriPlayer ??= Player.GetModPlayer<OriPlayer>();
  private OriPlayer? _oriPlayer;

  private int _lastFrameIndex = -1;

  protected override void OnInitialize() {
    AddChild(new NoAbility(Player));
    AddChild(new Transform(Player));
    AddChild(new AirJump(Player));
    AddChild(new Bash(Player));
    AddChild(new Burrow(Player));
    AddChild(new ChargeDash(Player));
    AddChild(new ChargeJump(Player));
    AddChild(new Climb(Player));
    AddChild(new Crouch(Player));
    AddChild(new Dash(Player));
    AddChild(new Glide(Player));
    AddChild(new Launch(Player));
    AddChild(new LookUp(Player));
    AddChild(new Stomp(Player));
    AddChild(new WallChargeJump(Player));
    AddChild(new WallJump(Player));
  }

  protected override void OnUpdate() {
    if (ActiveChild is NoAbility.Running && FrameIndex is 4 or 9 && FrameIndex != _lastFrameIndex) {
      OriPlayer.Footstep();
    }

    _lastFrameIndex = FrameIndex;
  }

  protected override Asset<AnimSpriteSheet> SpriteSheetAsset => OriPlayer.SpriteSheet;
}
