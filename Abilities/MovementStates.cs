using System;
using System.Collections.Generic;
using AnimLib.Animations;
using AnimLib.Networking;
using AnimLib.States;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace OriMod.Abilities;

/// <summary>
/// Root <see cref="SkinAnimation"/> state which contains all main Ori states as children states.
/// </summary>
public sealed class MovementStates : StateMachine {
  public override OriCharacter Character => (OriCharacter)base.Character!;

  private int _timeSinceLastHurt = 1000; // arbitrary large initial number
  private float _lastDamagePercentAmount;
  private bool _wasHurtInAir;
  private bool _movedAnyDir;

  public override void RegisterChildren(List<State> statesToAdd) {
    statesToAdd.AddRange([
      GetState<NoAbility>(),
      GetState<Transform>(),
      GetState<AirJump>(),
      GetState<Bash>(),
      GetState<Burrow>(),
      GetState<ChargeDash>(),
      GetState<ChargeJump>(),
      GetState<Climb>(),
      GetState<Crouch>(),
      GetState<Dash>(),
      GetState<Glide>(),
      GetState<Launch>(),
      GetState<LookUp>(),
      GetState<Stomp>(),
      GetState<WallChargeJump>(),
      GetState<WallJump>()
    ]);
  }

  protected override void OnEnter(State? fromState) {
    base.OnEnter(fromState);
    TrySetActiveChild<NoAbility>();
  }

  public override void ResetEffects() {
    _timeSinceLastHurt++;
  }

  // TODO: Do these three methods need to be separate?
  public override void PostUpdateMiscEffects() {
    if (Player.HasBuff(BuffID.TheTongue) || Player.grappling[0] > -1 || Player.pulley) {
      TriggerState<NoAbility>();
    }
  }

  public override void PostUpdate() {
  }

  public override AnimationOptions? GetAnimationOptions() {
    int hurtAnimationTime = _lastDamagePercentAmount > 0.2f ? 50 : 30;
    if (_timeSinceLastHurt >= hurtAnimationTime) {
      return base.GetAnimationOptions();
    }

    return TryGetHurtAnimation() ?? base.GetAnimationOptions();
  }

  /// <summary>
  /// Attempts to play a hurt animation, if the player was recently hurt.
  /// <br/> This will return <see langword="null"/> if the current spritesheet does not support hurt animations.
  /// </summary>
  private AnimationOptions? TryGetHurtAnimation() {
    AnimSpriteSheet sheet = Character.Anim.SpriteSheet;
    if (!_movedAnyDir) {
      _movedAnyDir = Player is not { controlLeft: false, controlRight: false, controlUp: false, controlDown: false, controlJump: false };
    }

    if (ActiveChild is AirJump && sheet.TryGetTag("Hurt_Spin", out AnimTag? tag)) {
      return new AnimationOptions(tag.Name);
    }

    if (Character.IsGrounded) {
      if (_wasHurtInAir && sheet.TryGetTag("Hurt_Bounce", out tag)) {
        return new AnimationOptions(tag.Name);
      }

      if (sheet.TryGetTag("Hurt_Ground", out tag)) {
        return new AnimationOptions(tag.Name);
      }
    }

    if (Math.Abs(Player.velocity.Y) > Math.Abs(Player.velocity.X)) {
      switch (Player.velocity.Y) {
        case < 0 when sheet.TryGetTag("Hurt_Up", out tag):
        case > 0 when sheet.TryGetTag("Hurt_Down", out tag) && !_movedAnyDir:
          return new AnimationOptions(tag.Name);
      }
    }

    if (_lastDamagePercentAmount > 0.15f && sheet.TryGetTag("Hurt_Big", out tag)) {
      return new AnimationOptions(tag.Name);
    }

    return sheet.TryGetTag("Hurt", out tag) ? new AnimationOptions(tag.Name) : null;
  }

  public override void OnHurt(Player.HurtInfo info) {
    _timeSinceLastHurt = 0;
    _lastDamagePercentAmount = info.Damage / (float)Player.statLifeMax2;
    _wasHurtInAir = !Character.IsGrounded;
    _movedAnyDir = false;
  }
}
