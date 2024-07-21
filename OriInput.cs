using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Net-synced player input, specific to this mod's controls.
/// </summary>
public sealed class OriInput : IEnumerable<Input> {
  public readonly Input Jump = new(() => PlayerInput.Triggers.Current.Jump && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Bash = new(() => OriMod.bashKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Dash = new(() => OriMod.dashKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Climb = new(() => OriMod.climbKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Glide = new(() => OriMod.featherKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Stomp = new(() => OriMod.stompKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Charge = new(() => OriMod.chargeKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input Burrow = new(() => OriMod.burrowKey.Current && !OriPlayer.Local.ControlsBlocked);
  public readonly Input LeftClick = new(() => PlayerInput.Triggers.Current.MouseLeft && !OriPlayer.Local.ControlsBlocked);

  /// <summary>
  /// Read and updates the player's inputs.
  /// </summary>
  /// <param name="netUpdate"><see langword="true"/> if this would invoke a net update; otherwise, <see langword="false"/></param>
  public void Update(out bool netUpdate) {
    netUpdate = false;
    foreach (Input input in this) {
      input.Update(out bool hasChanged);
      netUpdate |= hasChanged;
    }
  }

  public void ReadPacket(BinaryReader reader) {
    BitVector32 value = new(reader.ReadUInt16());
    int i = 0;
    foreach (Input input in this) {
      input.SetInputValue(value[1 << i++]);
    }
  }

  public void WritePacket(ModPacket packet) {
    BitVector32 arr = new();
    int i = 0;
    foreach (Input input in this) {
      arr[1 << i++] = input.GetInputValue();
    }
    packet.Write((ushort)arr.Data);
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
      input.ResetChanged();
    }
  }
}

/// <summary>
/// Simplistic OriMod input for syncing between clients
/// </summary>
public class Input {
  /// <summary>
  /// Create a new <see cref="Input"/> where the read input value is <paramref name="func"/>.
  /// </summary>
  /// <param name="func">Function to detect a button press as pressed or not</param>
  public Input(Func<bool> func) => _func = func ?? throw new ArgumentNullException(nameof(func));

  /// <summary>
  /// Whether the key is currently pressed.
  /// </summary>
  public bool Current { get; private set; }

  /// <summary>
  /// Whether this is the first frame this key was pressed down.
  /// </summary>
  public bool JustPressed => Current && _changed;

  /// <summary>
  /// Whether this is the first frame this kew was not pressed down.
  /// </summary>
  public bool JustReleased => !Current && _changed;

  /// <summary>
  /// Whether the value of <see cref="Current"/> during this frame is different from the previous frame.
  /// </summary>
  private bool _changed;

  /// <summary>
  /// Update the values of this input. Returns <see langword="true"/> if the values have changed.
  /// </summary>
  public void Update(out bool hasChanged) {
    SetInputValue(_func());
    hasChanged = _changed;
  }

  private readonly Func<bool> _func;

  internal void SetInputValue(bool value) {
    bool oldCurrent = Current;
    Current = value;
    _changed = Current != oldCurrent;
  }

  internal bool GetInputValue() => Current;

  internal bool ResetChanged() => _changed = false;
}
