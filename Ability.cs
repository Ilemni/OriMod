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
    public bool Active {
      get { return State == States.Active; }
      internal set { State = value ? States.Active : State; }
    }
    public bool Starting {
      get { return State == States.Starting; }
      internal set { State = value ? States.Starting : State; }
    }
    public bool Ending {
      get { return State == States.Ending; }
      internal set { State = value ? States.Ending : State; }
    }
    public bool Inactive {
      get { return State == States.Inactive; }
      internal set { State = value ? States.Inactive : State; }
    }
    public bool Failed {
      get { return State == States.Failed; }
      internal set { State = value ? States.Failed : State; }
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
      get { return _refreshed; }
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
      State = (States)r.ReadByte();
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
    protected virtual Color RefreshColor => Color.White;
    private void OnRefreshed() {
      for(int i = 0; i < 10; i++) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("AbilityRefreshedDust"), newColor:RefreshColor)];
      }
    }
    internal virtual void PutOnCooldown(bool force=false) {
      if (OriMod.ConfigClient.AbilityCooldowns && (!CooldownOnlyOnBoss || CooldownOnlyOnBoss && OriModUtils.IsAnyBossAlive(check:true)) || force) {
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
}