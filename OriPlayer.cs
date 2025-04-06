using System;
using AnimLib;
using JetBrains.Annotations;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod;

/// <summary>
/// Obsolete <see cref="ModPlayer"/> class for <see cref="OriMod"/>.
/// <para/> All functionality has moved to the <see cref="AnimCharacter"/> class <see cref="OriCharacter"/>.
/// This class remains for back-compatibility with older versions of <see cref="OriMod"/>.
/// </summary>
[UsedImplicitly]
[Obsolete("Functionality has moved to OriCharacter")]
public sealed class OriPlayer : ModPlayer {
  protected override void ValidateType() {
    // Prevent requiring both Save/Load,
    // since loading in this class is a back-compat feature that loads data onto OriCharacter
  }

  public override void LoadData(TagCompound tag) {
    OriCharacter ori = Player.GetCharacter<OriCharacter>();
    BackCompatability.LoadSave_From_3_2_2_0(ori, tag);
  }
}
