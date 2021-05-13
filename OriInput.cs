using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// Net-synced player input.
  /// </summary>
  public sealed class OriInput : IEnumerable<Input> {
    public readonly Input jump = new Input(() => PlayerInput.Triggers.Current.Jump);
    public readonly Input bash = new Input(() => OriMod.bashKey.Current);
    public readonly Input dash = new Input(() => OriMod.dashKey.Current);
    public readonly Input climb = new Input(() => OriMod.climbKey.Current);
    public readonly Input glide = new Input(() => OriMod.featherKey.Current);
    public readonly Input stomp = new Input(() => PlayerInput.Triggers.Current.Down);
    public readonly Input charge = new Input(() => OriMod.chargeKey.Current);
    public readonly Input burrow = new Input(() => OriMod.burrowKey.Current);
    public readonly Input leftClick = new Input(() => PlayerInput.Triggers.Current.MouseLeft);

    public bool netUpdate;

    public void Update() {
      foreach (Input input in this) {
        netUpdate |= input.Update();
      }
    }

    public void ReadPacket(BinaryReader reader) {
      foreach (Input input in this) {
        input.Write(reader.ReadByte());
      }
    }

    public void WritePacket(ModPacket packet) {
      foreach (Input input in this) {
        packet.Write(input.Read());
      }
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

    private bool _changed;

    /// <summary>
    /// Update the values of the give
    /// </summary>
    public bool Update() {
      bool old = Current;
      bool oldChanged = _changed;
      Current = _func();
      _changed = old != Current;
      return _changed || oldChanged;
    }

    private readonly Func<bool> _func;

    internal byte Read() => (byte)((Current ? 1 : 0) | (_changed ? 2 : 0));

    internal void Write(byte value) {
      Current = (value & 1) == 1;
      _changed = (value & 2) == 2;
    }
  }
}
