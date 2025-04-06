using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod;

/// <summary>
/// Net-synced player input, specific to this mod's controls.
/// </summary>
public sealed class OriInput : IEnumerable<Input> {
  /// <summary> Non-modded Jump bind. </summary>
  public Input Jump { get; } = new OriTriggerSetInput(nameof(PlayerInput.Triggers.Current.Jump));
  public Input Bash { get; } = new OriModKeyInput(OriMod.BashKey);
  public Input Dash { get; } = new OriModKeyInput(OriMod.DashKey);
  public Input Climb { get; } = new OriModKeyInput(OriMod.ClimbKey);
  public Input Glide { get; } = new OriModKeyInput(OriMod.FeatherKey);
  public Input Stomp { get; } = new OriModKeyInput(OriMod.StompKey);
  public Input Charge { get; } = new OriModKeyInput(OriMod.ChargeKey);
  public Input Burrow { get; } = new OriModKeyInput(OriMod.BurrowKey);
  /// <summary> Non-modded Left Click bind. </summary>
  public Input LeftClick { get; } = new OriTriggerSetInput(nameof(PlayerInput.Triggers.Current.MouseLeft));

  /// <summary>
  /// Read and updates the player's inputs.
  /// Returns <see langword="true"/> if any net-synced control was changed; otherwise, <see langword="false"/>
  /// </summary>
  public void Update() {
    if (OriMod.ConfigClient.blockControlsInMenu) {
      Player player = Main.LocalPlayer;
      bool inMenu = Main.ingameOptionsWindow || Main.inFancyUI || player.talkNPC >= 0 || player.sign >= 0 ||
        Main.clothesWindow || Main.playerInventory;
      if (inMenu) {
        DisableAll();
        return;
      }
    }

    foreach (Input input in this) {
      input.UpdateInputValue();
    }
  }

  public void DisableAll() {
    foreach (Input input in this) {
      input.ClearInputValue();
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
  /// Whether this is the first tick this key was pressed down.
  /// </summary>
  public bool JustPressed { get; private set; }

  /// <summary>
  /// Whether this is the first tick this key was not pressed down.
  /// </summary>
  public bool JustReleased { get; private set; }

  protected abstract bool ReadCurrent();
  protected abstract bool ReadJustPressed();
  protected abstract bool ReadJustReleased();

  internal void UpdateInputValue() {
    if (ShouldBlockInput()) {
      ClearInputValue();
      return;
    }

    Current = ReadCurrent();
    JustPressed = ReadJustPressed();
    JustReleased = ReadJustReleased();
  }

  internal void ClearInputValue() {
    Current = false;
    JustPressed = false;
    JustReleased = false;
  }

  protected abstract bool ShouldBlockInput();
}

/// <summary>
/// Key based on <see cref="ModKeybind"/>.
/// </summary>
public sealed class OriModKeyInput(ModKeybind key) : Input {
  private ModKeybind Key { get; } = key;

  protected override bool ReadCurrent() => Key.Current;
  protected override bool ReadJustPressed() => Key.JustPressed;
  protected override bool ReadJustReleased() => Key.JustReleased;

  protected override bool ShouldBlockInput() {
    if (!Main.LocalPlayer.mouseInterface) {
      return false;
    }

    var assignedKeys = Key.GetAssignedKeys();
    if (Main.playerInventory && assignedKeys.Any(
          s => s is "LeftControl" or "RightControl" or "LeftAlt" or "RightAlt" or "LeftShift" or "RightShift"
        )) {
      return true;
    }

    return assignedKeys.Any(
      s => s is "Mouse1" or "Mouse2" or "Mouse3" or "Mouse4" or "Mouse5"
    );
  }
}

/// <summary>
/// Key based on <see cref="PlayerInput"/>.
/// </summary>
public sealed class OriTriggerSetInput(string key) : Input {
  private string Key { get; } = key;

  protected override bool ReadCurrent() => PlayerInput.Triggers.Current.KeyStatus[Key];
  protected override bool ReadJustPressed() => PlayerInput.Triggers.JustPressed.KeyStatus[Key];
  protected override bool ReadJustReleased() => PlayerInput.Triggers.JustReleased.KeyStatus[Key];

  protected override bool ShouldBlockInput() {
    return Main.LocalPlayer.mouseInterface && Key
      is nameof(PlayerInput.Triggers.Current.MouseLeft)
      or nameof(PlayerInput.Triggers.Current.MouseRight)
      or nameof(PlayerInput.Triggers.Current.MouseMiddle)
      or nameof(PlayerInput.Triggers.Current.MouseXButton1)
      or nameof(PlayerInput.Triggers.Current.MouseXButton2);
  }
}
