using AnimLib.Skins;
using OriMod.Animations;
using Terraria.ModLoader;

namespace OriMod.Skins.Slots;

public sealed class LaunchArrowSlot : SkinSlot<OriCharacter, LaunchAnimation> {
  public override Skin DefaultSkin => ModContent.GetInstance<DefaultArrowSkin>();
  public override int SortOrder => 2;
}
