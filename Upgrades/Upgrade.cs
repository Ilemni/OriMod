using Terraria;
using Terraria.Localization;

namespace OriMod.Upgrades {
  public class Upgrade : IUnlockable {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">Name of the upgrade. Used as a localization key to Mods.OriMod.Upgrade.{Name}</param>
    /// <param name="cost">Spirit Light required to acquire this upgrade</param>
    /// <param name="requiredUpgrades">Previous upgrades required to unlock this Upgrade.</param>
    /// <param name="worldRequirement">Additional delegate requirement to unlock this Upgrade</param>
    public Upgrade(string name, int cost, Upgrade[] requiredUpgrades, ConditionResult worldRequirement = null) {
      string baseKey = $"Mods.OriMod.Upgrade.{name}";
      string nKey = $"{baseKey}.Name";
      string dKey = $"{baseKey}.Description";
      string fKey = $"{baseKey}.Flavor";

      Name = Language.Exists(nKey) ? Language.GetTextValue(nKey) : name;
      Description = Language.Exists(dKey) ? Language.GetTextValue(dKey) : string.Empty;
      FlavorText = Language.Exists(fKey) ? Language.GetTextValue(fKey) : string.Empty;

      actualCost = cost;
      WorldRequirement = worldRequirement;
      RequiredUpgrades = requiredUpgrades;
      //WorldRequirement = (out string reason) => { reason = default; return false; };
    }

    #region Properties
    /// <summary>
    /// Display Name of this Upgrade.
    /// </summary>
    public string DisplayName => Visible ? Name : "???";

    /// <summary>
    /// Display Description of this Upgrade.
    /// </summary>
    public string DisplayDescription => Visible ? Description : string.Empty;

    /// <summary>
    /// Display Flavor text of this Upgrade.
    /// </summary>
    public string DisplayFlavorText => Purchased ? FlavorText : string.Empty;

    /// <summary>
    /// Display Cost of this upgrade
    /// </summary>
    public string DisplayCost => Visible ? $"{actualCost} Spirit Light" : string.Empty;

    /// <summary>
    /// Amount of currency required to unlock this Upgrade.
    /// </summary>
    public int Cost => Visible ? actualCost : 0;
    
    /// <summary>
    /// Unlock state of the Upgrade. This simply means that all prerequisites *except purchase* have been met. Unlocked upgrades can be Purchased
    /// </summary>
    public bool Unlocked { get => flags[0]; set => flags[0] = value; }
    
    /// <summary>
    /// Purchased state of this upgrade. Purchased upgrades can be Enabled.
    /// </summary>
    public bool Purchased { get => flags[1]; private set => flags[1] = value; }
    
    /// <summary>
    /// Enabled state of the Upgrade. Enabled upgrades represent the player's intent to use the upgrade.
    /// </summary>
    public bool Enabled { get => flags[2]; private set => flags[2] = value; }
    
    /// <summary>
    /// Active state of the Upgrade. Active upgrades have their stats applied correctly.
    /// </summary>
    public bool Active { get => flags[3]; private set => flags[3] = value; }
    
    /// <summary>
    /// Visibility state of the Upgrade. Non-visible upgrades are not Unlocked. Non-visible upgrades hides the Display-related strings of this Upgrade
    /// </summary>
    public bool Visible { get => flags[4]; private set => flags[4] = value; }
    
    public Upgrade[] RequiredUpgrades { get; private set; }
    #endregion

    public readonly ConditionResult WorldRequirement;
    
    internal readonly string Name;
    internal readonly string Description;
    internal readonly string FlavorText;
    internal readonly int actualCost;
    private BitsByte flags;

    #region Methods
    internal void MakeVisible() => Visible = true;

    #region Unlocking
    public bool CanUnlock() {
      if (RequiredUpgrades == null) {
        return true;
      }

      for (int i = 0, len = RequiredUpgrades.Length; i < len; i++) {
        if (!RequiredUpgrades[i].Purchased) {
          return false;
        }
      }
      return true;
    }

    public bool TryUnlock() {
      if (!CanUnlock()) {
        return false;
      }
      Unlocked = true;
      return true;
    }
    #endregion

    #region Purchasing
    /// <summary>
    /// Check if this Upgrade can be purchased. Returns false if this upgrade is already purchased, or cannot be afforded.
    /// </summary>
    /// <returns></returns>
    public bool CanPurchase() {
      ValidateFlags();
      var oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
      if (Purchased || actualCost > oPlayer.SpiritLight) {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Attempt to purchase this Upgrade. If it cannot be purchase, the method returns false.
    /// </summary>
    /// <returns></returns>
    public bool TryPurchase() {
      if (!CanPurchase()) {
        return false;
      }
      var oPlayer = Main.LocalPlayer.GetModPlayer<OriPlayer>();
      oPlayer.SpiritLight -= actualCost;
      Purchased = true;
      return true;
    }
    #endregion

    #region Enabling
    /// <summary>
    /// Check if this Upgrade can be enabled. Returns false if this upgrade is already enabled, or not purchased.
    /// </summary>
    /// <returns></returns>
    public bool CanEnable() {
      ValidateFlags();
      if (!Purchased) {
        return false;
      }
      return !Enabled;
    }

    /// <summary>
    /// Attempt to enable this upgrade. If it cannot be enabled, the method returns false.
    /// </summary>
    /// <returns></returns>
    public bool TryEnable() {
      if (!CanEnable()) {
        return false;
      }
      Enabled = true;
      return true;
    }
    #endregion

    #region Disabling
    /// <summary>
    /// Check if this Upgrade can be disabled. Returns false if the upgrade is already disabled.
    /// </summary>
    /// <param name="warn">Warn if other upgrades may be disabled as a result of disabling this upgrade.</param>
    /// <returns></returns>
    public bool CanDisable(out bool warn) {
      warn = false;
      ValidateFlags();
      if (!Enabled) {
        return false;
      }

      // TODO: Warn checking
      warn = false;
      return true;
    }

    /// <summary>
    /// Attempt to disable this upgrade. If it cannot be disabled, this method returns false.
    /// </summary>
    /// <returns></returns>
    public bool TryDisable() {
      if (!CanDisable(out bool warn)) {
        return false;
      }
      Enabled = false;
      return true;
    }
    #endregion

    #region Activating
    /// <summary>
    /// Check if this Upgrade can be activated. Returns false if already active or is not enabled. 
    /// </summary>
    /// <returns></returns>
    public bool CanActivate() {
      ValidateFlags();
      return Enabled && !Active;
    }

    /// <summary>
    /// Attempt to active this upgrade. If it cannot be disabled, this method returns false.
    /// </summary>
    /// <returns></returns>
    public bool TryActivate() {
      if (!CanActivate()) {
        return false;
      }

      Active = true;
      return true;
    }
    #endregion

    /// <summary>
    /// Validates that current flags are valid.
    /// </summary>
    /// <remarks>
    /// How some flags affect others are as followed:
    /// Unlock [False] => Purchased [False]
    /// Unlock [True] => Visible [False]
    /// Purchased [False] => Enabled [False]
    /// Enabled [False] => Active [False]
    /// </remarks>
    public void ValidateFlags() {
      if (Unlocked) {
        Visible = false;
      }
      if (!Unlocked) {
        Purchased = false;
      }
      if (!Purchased) {
        Enabled = false;
      }
      if (!Enabled) {
        Active = false;
      }
    }

    /// <summary>
    /// Gets the byte representation of this class's mutable fields
    /// </summary>
    /// <returns></returns>
    public byte ToByte() => flags;
    internal byte SetByte(byte value) => flags = value;

    internal void SetRequiredUpgrades(Upgrade[] upgrades) => RequiredUpgrades = upgrades;

    /// <summary>
    /// Adds an Upgrade to the given UpgradeManager where the required upgrade is this Upgrade
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="upgrade"></param>
    /// <returns></returns>
    internal Upgrade ChainUpgrade(UpgradeManager manager, Upgrade upgrade) {
      upgrade.SetRequiredUpgrades(new[] { this });
      manager.AddUpgrade(upgrade);
      return upgrade;
    }
    #endregion

    public static implicit operator bool(Upgrade u) => u.Active;
  }
  
  public delegate bool ConditionResult(out string failReason);
}
