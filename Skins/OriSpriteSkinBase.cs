using System.Collections.Generic;
using AnimLib.Skins;
using OriMod.Skins.Slots;
using Terraria.ModLoader;

namespace OriMod.Skins;

/// <summary>
/// Base class for <see cref="OriCharacter"/> <see cref="Skin"/>s
/// that can be equipped to the character's <see cref="OriSpriteSlot"/>.
/// </summary>
public abstract class OriSpriteSkinBase : Skin<OriCharacter> {
  protected sealed override IEnumerable<SkinSlot> GetValidSlots() => [
    ModContent.GetInstance<OriSpriteSlot>()
  ];
}
