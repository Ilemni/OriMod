using Terraria.ModLoader;

namespace OriMod.Skins;

public sealed class BashTestSkin : OriArrowSkinBase {
  /// Set to true to enable this test skin.
  public override bool IsLoadingEnabled(Mod mod) => false;

  public override string SpriteSheetPath => "OriMod/Animations/BashAnim2";
}
