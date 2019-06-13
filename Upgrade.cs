using System.Collections.Generic;
using System.Linq;

namespace OriMod {
  public abstract class Upgrade {
    internal UpgradeHandler Handler { get; }
    internal Upgrade(UpgradeHandler handler, string name, string desc, int id, string flavor="") {
      Handler = handler;
      Name = name;
      Description = desc;
      FlavorText = flavor;
      ID = id;
    }
    public string Name { get; }
    public string Description { get; }
    public string FlavorText { get; }
    internal int ID { get; }
    internal int Price { get; }
    internal virtual bool WorldRequirement => true; // Usually something like NPC.downedBoss1
    internal bool Unlocked;
    internal bool Bought;
    internal bool Active;
    internal bool CanBuy => Unlocked && !Bought;
    internal bool CanActivate => Unlocked && Bought;
    internal bool CheckUnlockState() {
      if (!WorldRequirement) Unlocked = false;
      if (!AllRequired.All(id => Handler.Upgrades[id].CanActivate)) return false;
      if (!AnyRequired.Any(id => Handler.Upgrades[id].CanActivate)) return false;
      return true;
    }
    internal abstract void OnApply();
    /// <summary>
    /// Upgrades in which any of them is required to set this to CanBuy
    /// </summary>
    /// <value></value>
    /// <seealso cref="AllRequired" />
    internal int[] AnyRequired;
    /// <summary>
    /// Upgrades in which all of them is required to set this to CanBuy
    /// </summary>
    /// <value></value>
    /// <seealso cref="AnyRequired" />
    internal int[] AllRequired;
    /// <summary>
    /// Upgrades that have `CheckUnlockState()` called when this upgrade is bought.
    /// 
    /// This list is automatically generated based on `AllRequired` and `AnyRequired`
    /// </summary>
    internal List<int> ThisUnlocks;
  }
}