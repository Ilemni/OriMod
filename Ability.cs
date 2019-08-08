using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public abstract class Ability {
    public virtual int id => -1;
    protected Player player { get; }
    protected OriPlayer oPlayer { get; }
    protected bool isLocalPlayer { get; }
    protected OriAbilities Handler { get; }
    protected static OriConfigClient2 Config => OriMod.ConfigAbilities;
    internal Ability(OriPlayer oriPlayer, OriAbilities handler) {
      player = oriPlayer.player;
      oPlayer = oriPlayer;
      isLocalPlayer = player.whoAmI == Main.myPlayer;
      Handler = handler;
    }
    /// <summary>
    /// Current state of the ability
    /// 
    /// <seealso cref="State" />
    /// </summary>
    /// <value></value>
    public AbilityState State { get; internal set; }
    public bool Active {
      get => State == AbilityState.Active;
      internal set => State = value ? AbilityState.Active : State;
    }
    public bool Starting {
      get => State == AbilityState.Starting;
      internal set => State = value ? AbilityState.Starting : State;
    }
    public bool Ending {
      get => State == AbilityState.Ending;
      internal set => State = value ? AbilityState.Ending : State;
    }
    public bool Inactive {
      get => State == AbilityState.Inactive;
      internal set => State = value ? AbilityState.Inactive : State;
    }
    public bool Failed {
      get => State == AbilityState.Failed;
      internal set => State = value ? AbilityState.Failed : State;
    }
    /// <summary>
    /// Determines if the ability has been unlocked by the player.
    /// 
    /// Currently unimplemented
    /// </summary>
    /// <value></value>
    internal bool Unlocked = true;
    protected virtual int Cooldown { get; }
    protected int CurrCooldown;
    protected int CurrTime;
    protected virtual bool CooldownOnlyOnBoss => false;
    internal virtual bool CanUse => Unlocked && Refreshed;
    internal abstract bool DoUpdate { get; }
    private bool _refreshed = true;
    internal bool Refreshed {
      get => _refreshed;
      set {
        if (!_refreshed && value) {
          OnRefreshed();
        }
        _refreshed = value;
      }
    }
    protected int currRand = 0; // Used for random sounds that don't repeat
    /// <summary>
    /// Checks if the ability is active
    /// </summary>
    /// <value>True if the state is neither Inactive nor Failed</value>
    public bool InUse => !Inactive && !Failed;
    
    internal void PreReadPacket(BinaryReader r) {
      State = (AbilityState)r.ReadByte();
      ReadPacket(r);
      if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient) {
        Update();
      }
    }
    internal void PreWritePacket(ModPacket packet) {
      packet.Write((byte)State);
      WritePacket(packet);
    }
    protected virtual void ReadPacket(BinaryReader r) { }
    protected virtual void WritePacket(ModPacket packet) { }
    
    protected virtual bool PreUpdate() {
      if (!isLocalPlayer) return true;
      if (!CanUse && !InUse) return false;
      return true;
    }
    internal virtual bool Update() {
      if (!PreUpdate()) return false;
      switch (State) {
        case AbilityState.Active:
          UpdateActive();
          UpdateUsing();
          return true;
        case AbilityState.Starting:
          UpdateStarting();
          UpdateUsing();
          return true;
        case AbilityState.Ending:
          UpdateEnding();
          UpdateUsing();
          return true;
        case AbilityState.Failed:
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
    protected virtual Color RefreshColor => Color.White;
    private void OnRefreshed() {
      for(int i = 0; i < 10; i++) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("AbilityRefreshedDust"), newColor:RefreshColor)];
      }
    }
    internal virtual void PutOnCooldown(bool force=false) {
      if (force || OriMod.ConfigClient.AbilityCooldowns && (CooldownOnlyOnBoss ? OriModUtils.IsAnyBossAlive(check:true) : true)) {
        CurrCooldown = Cooldown;
        Refreshed = false;
      }
    }
    protected virtual void TickCooldown() {
      if (CurrCooldown > 0 || !Refreshed) {
        CurrCooldown--;
        if (CurrCooldown < 0) {
          Refreshed = true;
        }
      }
    }
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
  public enum AbilityState {
    Inactive = 0,
    Starting = 1,
    Active = 2,
    Ending = 3,
    Failed = 4
  }
}