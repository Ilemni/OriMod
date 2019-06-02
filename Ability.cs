using Terraria;

namespace OriMod {
  public abstract class Ability {
    internal byte id = 255;
    protected Player player;
    protected OriPlayer OPlayer;
    protected bool isLocalPlayer;
    protected OriAbilities Handler;
    internal Ability(OriPlayer oriPlayer, OriAbilities handler) {
      player = oriPlayer.player;
      OPlayer = oriPlayer;
      isLocalPlayer = player.whoAmI == Main.myPlayer;
      Handler = handler;
    }
    /// <summary>
    /// All possible states that the Ability can be in.
    /// 
    /// Inactive: The ability will not perform any actions
    /// 
    /// Starting: The ability will use UpdateStarting() and UpdateUsing()
    /// 
    /// Active: The ability will use UpdateActive() and UpdateUsing()
    /// 
    /// Ending: The ability will use UpdateEnding() and UpdateUsing()
    /// 
    /// Failed: The ability will use UpdateFailed()
    /// </summary>
    public enum States {
      Inactive = 0,
      Starting = 1,
      Active = 2,
      Ending = 3,
      Failed = 4
    }
    /// <summary>
    /// Current state of the ability
    /// 
    /// <seealso cref="States" />
    /// </summary>
    /// <value></value>
    public States State { get; internal set; }
    /// <summary>
    /// Determines if the ability has been unlocked by the player.
    /// 
    /// Currently unimplemented
    /// </summary>
    /// <value></value>
    public bool unlocked { get; internal set; }
    internal virtual bool CanUse { get; set; }
    internal virtual bool Refreshed { get; set; }
    protected int currRand = 0; // Used for random sounds that don't repeat
    /// <summary>
    /// Checks the current state of the ability
    /// </summary>
    /// <param name="states">One or more states to match against</param>
    /// <returns>True if the state matches one of the provided state arguments, otherwise false</returns>
    public bool IsState(params States[] states) {
      foreach(int s in states) {
        States v = (States)s;
        if (State == v) return true;
      }
      return false;
    }
    /// <summary>
    /// Checks if the ability is active
    /// </summary>
    /// <value>True if the state is neither Inactive nor Failed</value>
    public bool InUse {
      get {
        return State != States.Inactive && State != States.Failed;
      }
    }
    protected virtual bool PreUpdate() {
      if (!CanUse && !InUse) return false;
      return true;
    }
    internal virtual bool Update() {
      if (!PreUpdate()) return false;
      switch (State) {
        case States.Active:
          UpdateActive();
          UpdateUsing();
          return true;
        case States.Starting:
          UpdateStarting();
          UpdateUsing();
          return true;
        case States.Ending:
          UpdateEnding();
          UpdateUsing();
          return true;
        case States.Failed:
          UpdateFailed();
          return true;
        default:
          return false;
      }
    }
    protected virtual void UpdateStarting() { }
    protected virtual void UpdateActive() { }
    protected virtual void UpdateEnding() { }
    protected virtual void UpdateFailed() { }
    protected virtual void UpdateUsing() { }
    internal abstract void Tick();
  }
}