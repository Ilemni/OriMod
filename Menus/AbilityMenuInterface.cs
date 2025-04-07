using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace OriMod.Menus;

[UsedImplicitly]
public sealed class AbilityMenuInterface : ModSystem {
  private UserInterface? _uiInterface;
  private AbilityMenu? _ui;
  private LegacyGameInterfaceLayer? _oriAbilityLayer;
  private GameTime? _gameTime;

  public override bool IsLoadingEnabled(Mod mod) => AbilityMenu.LoadingEnabled;

  public override void PostSetupContent() {
    _ui = new AbilityMenu();
    _ui.Activate();
    _uiInterface = new UserInterface();
    _uiInterface.SetState(_ui);
    _oriAbilityLayer = new LegacyGameInterfaceLayer("OriMod: Ability Menu", DrawMethod, InterfaceScaleType.UI);
  }

  private bool DrawMethod() {
    if (_gameTime is not null && _uiInterface?.CurrentState is not null) {
      _uiInterface.Draw(Main.spriteBatch, _gameTime);
    }

    return true;
  }

  public override void UpdateUI(GameTime gameTime) {
    _gameTime = gameTime;
    _uiInterface?.Update(gameTime);
  }

  public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
    if (_oriAbilityLayer is null) {
      return;
    }

    int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
    if (mouseTextIndex != -1) {
      layers.Insert(mouseTextIndex, _oriAbilityLayer);
    }
  }
}
