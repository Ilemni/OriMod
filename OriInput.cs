using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// Net-synced player input, specific to this mod's controls.
  /// </summary>
  public sealed class OriInput : IEnumerable<Input> {
    public readonly Input jump = new Input(() => PlayerInput.Triggers.Current.Jump && !OriPlayer.Local.controls_blocked);
    public readonly Input bash = new Input(() => OriMod.bashKey.Current && !OriPlayer.Local.controls_blocked);
    public readonly Input dash = new Input(() => OriMod.dashKey.Current && !OriPlayer.Local.controls_blocked);
    public readonly Input climb = new Input(() => OriMod.climbKey.Current && !OriPlayer.Local.controls_blocked);
    public readonly Input glide = new Input(() => OriMod.featherKey.Current && !OriPlayer.Local.controls_blocked);
    public readonly Input stomp = new Input(() => PlayerInput.Triggers.Current.Down && !OriPlayer.Local.controls_blocked);
    public readonly Input charge = new Input(() => OriMod.chargeKey.Current && !OriPlayer.Local.controls_blocked);
    public readonly Input burrow = new Input(() => OriMod.burrowKey.Current && !OriPlayer.Local.controls_blocked);
    public readonly Input leftClick = new Input(() => PlayerInput.Triggers.Current.MouseLeft && !OriPlayer.Local.controls_blocked);

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
      BitVector32 value = new BitVector32(reader.ReadUInt16());
      int i = 0;
      foreach (Input input in this) {
        input.SetInputValue(value[(1 << i++)]);
      }
    }

    public void WritePacket(ModPacket packet) {
      BitVector32 arr = new BitVector32();
      int i = 0;
      foreach (Input input in this) {
        arr[(1 << i++)] = input.GetInputValue();
      }
      packet.Write((ushort)arr.Data);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<Input> GetEnumerator() {
      yield return jump;
      yield return bash;
      yield return dash;
      yield return climb;
      yield return glide;
      yield return stomp;
      yield return charge;
      yield return burrow;
      yield return leftClick;
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
    /// Whether or not the key is currently pressed.
    /// </summary>
    public bool Current { get; private set; }

    /// <summary>
    /// Whether or not this is the first frame this key was pressed down.
    /// </summary>
    public bool JustPressed => Current && _changed;

    /// <summary>
    /// Whether or not this is the first frame this kew was not pressed down.
    /// </summary>
    public bool JustReleased => !Current && _changed;

    /// <summary>
    /// Whether or not the value of <see cref="Current"/> during this frame is different from the previous frame.
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
}