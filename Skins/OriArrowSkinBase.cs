using System.Collections.Generic;
using AnimLib.Skins;
using OriMod.Skins.Slots;
using Terraria.ModLoader;

namespace OriMod.Skins;

/// <summary>
/// Base class for <see cref="OriCharacter"/> <see cref="Skin"/>s
/// that can be equipped to either the character's <see cref="BashArrowSlot"/> or <see cref="LaunchArrowSlot"/>.
/// </summary>
public abstract class OriArrowSkinBase : Skin<OriCharacter> {
  protected sealed override IEnumerable<SkinSlot> GetValidSlots() => [
    ModContent.GetInstance<BashArrowSlot>(),
    ModContent.GetInstance<LaunchArrowSlot>()
  ];
}
