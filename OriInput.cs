using System.Collections;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Net-synced player input, specific to this mod's controls.
/// </summary>
public sealed class OriInput : IEnumerable<Input> {
  public Input Jump { get; } = new OriTriggerSetInput(nameof(PlayerInput.Triggers.Current.Jump));
  public Input Bash { get; } = new OriModKeyInput(OriMod.bashKey);
  public Input Dash { get; } = new OriModKeyInput(OriMod.dashKey);
  public Input Climb { get; } = new OriModKeyInput(OriMod.climbKey);
  public Input Glide { get; } = new OriModKeyInput(OriMod.featherKey);
  public Input Stomp { get; } = new OriModKeyInput(OriMod.stompKey);
  public Input Charge { get; } = new OriModKeyInput(OriMod.chargeKey);
  public Input Burrow { get; } = new OriModKeyInput(OriMod.burrowKey);
  public Input LeftClick { get; } = new OriTriggerSetInput(nameof(PlayerInput.Triggers.Current.MouseLeft));

  /// <summary>
  /// Read and updates the player's inputs.
  /// Returns <see langword="true"/> if any net-synced control was changed; otherwise, <see langword="false"/>
  /// </summary>
  public void Update() {
    foreach (Input input in this) {
      input.UpdateInputValue();
    }
  }

  public void DisableAll() {
    foreach (Input input in this) {
      input.SetInputValue(false);
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public IEnumerator<Input> GetEnumerator() {
    yield return Jump;
    yield return Bash;
    yield return Dash;
    yield return Climb;
    yield return Glide;
    yield return Stomp;
    yield return Charge;
    yield return Burrow;
    yield return LeftClick;
  }

  public void ResetInputChangedState() {
    foreach (Input input in this) {
      input.Changed = false;
    }
  }
}

/// <summary>
/// Abstract class for OriMod input, for syncing between clients
/// </summary>
public abstract class Input {
  /// <summary>
  /// Whether the key is currently pressed.
  /// </summary>
  public bool Current { get; private set; }

  /// <summary>
  /// Whether this is the first frame this key was pressed down.
  /// </summary>
  public bool JustPressed => Current && Changed;

  /// <summary>
  /// Whether this is the first frame this key was not pressed down.
  /// </summary>
  public bool JustReleased => !Current && Changed;

  /// <summary>
  /// Whether the value of <see cref="Current"/> during this frame is different from the previous frame.
  /// </summary>
  internal bool Changed;

  protected abstract bool ReadCurrent();

  internal void UpdateInputValue() => SetInputValue(ReadCurrent());

  internal void SetInputValue(bool value) {
    Changed = Current != value;
    Current = value;
  }
}

/// <summary>
/// Key based on <see cref="ModKeybind"/>.
/// </summary>
/// <param name="key"></param>
public sealed class OriModKeyInput(ModKeybind key) : Input {
  private ModKeybind Key { get; } = key;
  protected override bool ReadCurrent() => Key.Current;
}

/// <summary>
/// Key based on <see cref="PlayerInput"/>.
/// </summary>
/// <param name="key"></param>
public sealed class OriTriggerSetInput(string key) : Input {
  private string Key { get; } = key;
  protected override bool ReadCurrent() => PlayerInput.Triggers.Current.KeyStatus[Key];
}
