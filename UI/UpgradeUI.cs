using Microsoft.Xna.Framework;
using OriMod.Upgrades;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace OriMod.UI {
  internal class UpgradeUI : UIState {
    public override void OnInitialize() {
      mainPanel = new UIPanel() {
        HAlign = 0.5f,
        VAlign = 0.2f
      };
      mainPanel.Width.Set(1000, 80);
      mainPanel.Height.Set(600, 80);
      Append(mainPanel);

      AppendHeader();
      AppendInfoPanel();
      RecalculateChildren();
    }

    public override void OnActivate() {
      AppendUpgrades();
    }

    private void AppendHeader() {
      var header = new UIText("OriMod Upgrades") {
        HAlign = 0.5f
      };
      header.Top.Set(10, 0);
      mainPanel.Append(header);
    }
    private void AppendInfoPanel() {
      var infoPanel = new UIPanel {
        HAlign = 1
      };
      infoPanel.Left.Set(0, 40);
      infoPanel.Top.Set(0, 100);
      mainPanel.Append(infoPanel);

      infoPanel.Append(upgradeName = new UIText(string.Empty));
      infoPanel.Append(upgradeDesc = new UIText(string.Empty) { PaddingTop = 30 });
      infoPanel.Append(upgradeFlav = new UIText(string.Empty) { PaddingTop = 60 });
      infoPanel.Append(upgradeCost = new UIText(string.Empty) { PaddingTop = 90 });
    }
    private void AppendUpgrades() {
      if (hasAppendedUpgrades) {
        return;
      }
      try {
        var upPanel = new UIPanel { HAlign = 0, VAlign = 0 };
        var pos = 50f;
        foreach(var u in UpgradeManager.Local.Upgrades.Values) {
          var text = new UIText(u.Name);
          text.Top.Set(pos, 0);
          text.Width.Set(300, 0);
          text.Height.Set(20, 0);
          pos += 25f;
          upPanel.Append(text);
        }
        hasAppendedUpgrades = true;
        mainPanel.Append(upPanel);
      }
      catch { }
    }

    private UIPanel mainPanel;

    private UIText upgradeName;
    private UIText upgradeDesc;
    private UIText upgradeFlav;
    private UIText upgradeCost;

    private bool hasAppendedUpgrades;

    public void SetUpgradeText(Upgrade upgrade) {
      if (upgrade is null) {
        throw new ArgumentNullException(nameof(upgrade));
      }

      upgradeName.SetText(upgrade.Name);
      upgradeDesc.SetText(upgrade.Description);
      upgradeFlav.SetText(upgrade.FlavorText);
      upgradeCost.SetText(upgrade.actualCost.ToString());
    }

    Upgrade currUp;

    public override void Update(GameTime gameTime) {
      try {
        var up = Main.LocalPlayer.GetModPlayer<OriPlayer>().Upgrades.Upgrades;
        bool b = false;
        if (currUp is null) {
          b = true;
          currUp = up["WallJump"];
        }
        var set = Terraria.GameInput.PlayerInput.Triggers.JustPressed;
        if (set.Left) {
          b = true;
          currUp = up["AirJump"];
        }
        else if (set.Right) {
          b = true;
          currUp = up["Bash"];
        }
        else if (set.Up) {
          b = true;
          currUp = up["Stomp"];
        }
        else if (set.Down) {
          b = true;
          currUp = up["Climb"];
        }
        if (b) {
          SetUpgradeText(currUp);
        }
      }
      catch (Exception) { }
    }
  }
}
