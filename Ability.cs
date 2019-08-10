using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod {
  public abstract class Ability {
    /// <summary> Only construct this in OriAbilities. </summary>
    internal Ability(OriAbilities handler) {
      Handler = handler;
      oPlayer = Handler.oPlayer;
      player = oPlayer.player;
      isLocalPlayer = player.whoAmI == Main.myPlayer;
    }
    
    
    /// <summary> Abilities Config </summary>
    protected static OriConfigClient2 Config => OriMod.ConfigAbilities;
    
    
    /// <summary> Unique ID of this ability. </summary>
    public abstract int id { get; }

    /// <summary> The `Player` this ability is attached to. </summary>
    protected Player player { get; }

    /// <summary> The `OriPlayer` this ability is attached to. </summary>
    protected OriPlayer oPlayer { get; }

    /// <summary> Short for player.whoAmI == Main.myPlayer </summary>
    protected bool isLocalPlayer { get; }

    /// <summary> Handler this ability is attached to. Same as oPlayer.Abilities </summary>
    protected OriAbilities Handler { get; }


    /// <summary> Condition required to call `Update()`. </summary>
    internal abstract bool DoUpdate { get; }
    
    /// <summary> Condition required for the player to use this ability. </summary>
    internal virtual bool CanUse => Unlocked && Refreshed;
    
    /// <summary> Cooldown of the ability. This should point to a Config option. </summary>
    protected virtual int Cooldown { get; }
    
    /// <summary> Determines if the ability only goes on cooldown if a boss is active. </summary>
    protected virtual bool CooldownOnlyOnBoss => false;

    /// <summary> Color of dust spawned in `OnRefreshed()`</summary>
    protected virtual Color RefreshColor => Color.White;

    
    /// <summary> Current state of the ability </summary>
    public AbilityState State {
      get => _state;
      private set {
        if (value != _state) {
          netUpdate = true;
          _state = value;
        }
      }
    }
    private AbilityState _state;
    /// <summary> If State is Inactive. Setting only works when value is `true`</summary>
    public bool Inactive {
      get => State == AbilityState.Inactive;
      internal set => State = value ? AbilityState.Inactive : State;
    }
    /// <summary> If State is Starting. Setting only works when value is `true`</summary>
    public bool Starting {
      get => State == AbilityState.Starting;
      internal set => State = value ? AbilityState.Starting : State;
    }
    /// <summary> If State is Active. Setting only works when value is `true`</summary>
    public bool Active {
      get => State == AbilityState.Active;
      internal set => State = value ? AbilityState.Active : State;
    }
    /// <summary> If State is Ending. Setting only works when value is `true`</summary>
    public bool Ending {
      get => State == AbilityState.Ending;
      internal set => State = value ? AbilityState.Ending : State;
    }
    /// <summary> If State is Failed. Setting only works when value is `true`</summary>
    public bool Failed {
      get => State == AbilityState.Failed;
      internal set => State = value ? AbilityState.Failed : State;
    }
    
    /// <summary> Checks if the ability is active </summary>
    /// <value>True if the state is neither Inactive nor Failed</value>
    public bool InUse => !Inactive && !Failed;

    
    /// <summary> Determines if the ability has been unlocked by the player. Currently unimplemented. </summary>
    internal bool Unlocked = true;
    
    /// <summary> Time left until the ability is no longer on cooldown. </summary>
    protected int CurrCooldown;

    /// <summary> Time the ability was in the given State. </summary>
    protected int CurrTime;

    /// <summary> Previous sound index used to prevent the same sound from playing consectutively. </summary>
    protected int CurrSoundRand = 0;
    
    /// <summary> True if ready to use, false if on cooldown. </summary>
    internal bool Refreshed {
      get => _refreshed;
      set {
        if (!_refreshed && value) {
          OnRefreshed();
        }
        _refreshed = value;
      }
    }
    private bool _refreshed = true;


    /// <summary> Creates dust when refreshed. </summary>
    private void OnRefreshed() {
      for(int i = 0; i < 10; i++) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("AbilityRefreshedDust"), newColor:RefreshColor)];
      }
    }
    
    internal bool netUpdate = false;

    /// <summary> Do not use this. </summary>
    internal void PreReadPacket(BinaryReader r) {
      State = (AbilityState)r.ReadByte();
      ReadPacket(r);
    }
    /// <summary> Do not use this. </summary>
    internal void PreWritePacket(ModPacket packet) {
      packet.Write((byte)State);
      WritePacket(packet);
    }
    
    /// <summary> Ability-specific data to read from packet. Use in conjunction with `WritePacket()` </summary>
    protected virtual void ReadPacket(BinaryReader r) { }
    /// <summary> Ability-specific data to write to packet. Use in conjunction with `ReadPacket()` </summary>
    protected virtual void WritePacket(ModPacket packet) { }
    

    /// <summary> Called in `OriPlayer.PostUpdateRunSpeeds()`, directly before `this.Update()`
    /// 
    /// Always called, used for managing States </summary>
    internal abstract void Tick();
    
    /// <summary> Simple cooldown ticking. Can be overwridden. </summary>
    protected virtual void TickCooldown() {
      if (CurrCooldown > 0 || !Refreshed) {
        CurrCooldown--;
        if (CurrCooldown < 0) {
          Refreshed = true;
        }
      }
    }
    
    /// <summary> Put this ability on cooldown. </summary>
    /// <param name="force">Put ability on cooldown regardless of config options. </param>
    internal virtual void PutOnCooldown(bool force=false) {
      if (force || OriMod.ConfigClient.AbilityCooldowns && (CooldownOnlyOnBoss ? OriModUtils.IsAnyBossAlive(check:true) : true)) {
        CurrCooldown = Cooldown;
        Refreshed = false;
      }
    }
    
    
    /// <summary> Called before `Update()`. If this returns false, `Update()` will not run. Check source for default value. </summary>
    protected bool PreUpdate() {
      if (!isLocalPlayer) return true;
      if (!CanUse && !InUse) return false;
      return true;
    }

    /// <summary> Called in `OriPlayer.PostUpdateRunSpeeds()`, directly after `this.Tick()`, if `PreUpdate()` returns true. </summary>
    internal void Update() {
      if (!PreUpdate()) return;
      switch (State) {
        case AbilityState.Active:
          UpdateActive();
          UpdateUsing();
          return;
        case AbilityState.Starting:
          UpdateStarting();
          UpdateUsing();
          return;
        case AbilityState.Ending:
          UpdateEnding();
          UpdateUsing();
          return;
        case AbilityState.Failed:
          UpdateFailed();
          return;
        default:
          return;
      }
    }
    
    /// <summary> Called when this update is in the Starting state. </summary>
    protected virtual void UpdateStarting() { }
    /// <summary> Called when this update is in the Active state. </summary>
    protected virtual void UpdateActive() { }
    /// <summary> Called when this update is in the Ending state. </summary>
    protected virtual void UpdateEnding() { }
    /// <summary> Called when this update is in the Failed state. </summary>
    protected virtual void UpdateFailed() { }
    /// <summary> Called directly after `UpdateStarting()`, `UpdateActive()`, and `UpdateEnding()`</summary>
    protected virtual void UpdateUsing() { }
    
    /// <summary> Rudimentary implementation, for now manually call in PlayerLayer </summary>
    internal virtual void DrawEffects() { }


    public override string ToString() => $"Ability ID:{id} Player:{player.whoAmI} State:{State} Unlocked:{Unlocked} Cooldown:{CurrCooldown}/{Cooldown}";
    
    public override int GetHashCode() => (player.whoAmI * Main.player.Length) + id;
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