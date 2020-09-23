using System;
using System.Collections.Generic;
using System.IO;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod {
  /// <summary>
  /// Net-synced player input.
  /// </summary>
  public sealed class OriInput {
    public readonly Input jump = new Input(() => PlayerInput.Triggers.Current.Jump);
    public readonly Input bash = new Input(() => OriMod.BashKey.Current);
    public readonly Input dash = new Input(() => OriMod.DashKey.Current);
    public readonly Input climb = new Input(() => OriMod.ClimbKey.Current);
    public readonly Input glide = new Input(() => OriMod.FeatherKey.Current);
    public readonly Input stomp = new Input(() => PlayerInput.Triggers.Current.Down);
    public readonly Input charge = new Input(() => OriMod.ChargeKey.Current);
    public readonly Input burrow = new Input(() => OriMod.BurrowKey.Current);
    public readonly Input leftClick = new Input(() => PlayerInput.Triggers.Current.MouseLeft);

    public bool netUpdate;

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

    public void Update() {
      foreach (var input in this) {
        netUpdate |= input.Update();
      }
    }

    public void ReadPacket(BinaryReader reader) {
      foreach (var input in this) {
        input.Write(reader.ReadByte());
      }
    }

    public void WritePacket(ModPacket packet) {
      foreach (var input in this) {
        packet.Write(input.Read());
      }
    }
  }

  /// <summary>
  /// Simpistic OriMod input for syncing between clients
  /// </summary>
  public class Input {
    /// <summary>
    /// Create a new <see cref="Input"/> where the read input value is <paramref name="func"/>.
    /// </summary>
    /// <param name="func"></param>
    public Input(Func<bool> func) => this.func = func ?? throw new ArgumentNullException(nameof(func));

    /// <summary>
    /// Whether or not the key is currently pressed.
    /// </summary>
    public bool Current { get; private set; }

    /// <summary>
    /// Whether or not this is the first frame this key was pressed down.
    /// </summary>
    public bool JustPressed => Current && changed;

    /// <summary>
    /// Whether or not this is the first frame this kew was not pressed down.
    /// </summary>
    public bool JustReleased => !Current && changed;

    internal bool changed;

    /// <summary>
    /// Update the values of the give
    /// </summary>
    public bool Update() {
      bool old = Current;
      bool oldChanged = changed;
      Current = func();
      changed = old != Current;
      return changed || oldChanged;
    }

    private readonly Func<bool> func;

    internal byte Read() => (byte)((Current ? 1 : 0) | (changed ? 2 : 0));

    internal void Write(byte value) {
      Current = (value & 1) == 1;
      changed = (value & 2) == 2;
    }
  }
}
