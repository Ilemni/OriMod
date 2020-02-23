using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  public abstract class Ability : IUnlockable {
    /// <summary>
    /// Only construct this in AbilityManager.
    /// </summary>
    internal Ability(AbilityManager manager) {
      Manager = manager;
      oPlayer = manager.oPlayer;
      player = oPlayer.player;
    }

    /// <summary>
    /// Abilities Config
    /// </summary>
    internal static OriConfigClient2 Config => OriMod.ConfigAbilities;

    /// <summary>
    /// The Player this ability is attached to.
    /// </summary>
    public readonly Player player;

    /// <summary>
    /// The OriPlayer this ability is attached to.
    /// </summary>
    public readonly OriPlayer oPlayer;

    /// <summary>
    /// The OriAbilities this ability is attached to.
    /// </summary>
    public readonly AbilityManager Manager;

    /// <summary>
    /// Determines if the ability has been unlocked by the player. Currently unimplemented.
    /// </summary>
    public bool Unlocked { get; set; } = true;

    /// <summary>
    /// Previous sound index used to prevent the same sound from playing consectutively.
    /// </summary>
    protected int CurrSoundRand = 0;

    #region General Properties
    /// <summary>
    /// Unique ID of this ability.
    /// </summary>
    public abstract int Id { get; }

    /// <summary>
    /// Condition required to call Update()
    /// </summary>
    internal abstract bool DoUpdate { get; }

    /// <summary>
    /// Condition required for the player to use this ability.
    /// </summary>
    internal virtual bool CanUse => Unlocked && Refreshed;

    /// <summary>
    /// Cooldown of the ability. This should point to a Config option.
    /// </summary>
    protected virtual int Cooldown { get; }

    /// <summary>
    /// Determines if the ability only goes on cooldown if a boss is active.
    /// </summary>
    protected virtual bool CooldownOnlyOnBoss => false;

    /// <summary>
    /// Color of dust spawned in OnRefreshed()
    /// </summary>
    protected virtual Color RefreshColor => Color.White;
    #endregion

    #region States
    public State AbilityState { get; private set; }

    public void SetState(State state) {
      if (state != AbilityState) {
        netUpdate = true;
        AbilityState = state;
      }
    }

    /// <summary>
    /// If this AbilityState is Inactive
    /// </summary>
    public bool Inactive => AbilityState == State.Inactive;

    /// <summary>
    /// If this AbilityState is Starting
    /// </summary>
    public bool Starting => AbilityState == State.Starting;

    /// <summary>
    /// If this AbilityState is Active
    /// </summary>
    public bool Active => AbilityState == State.Active;

    /// <summary>
    /// If this AbilityState is Ending
    /// </summary>
    public bool Ending => AbilityState == State.Ending;

    /// <summary>
    /// If this AbilityState is Failed
    /// </summary>
    public bool Failed => AbilityState == State.Failed;

    /// <summary> Checks if the ability is active </summary>
    /// <value>True if the state is neither Inactive nor Failed</value>
    public bool InUse => Starting || Active || Ending;
    #endregion

    #region Time and Cooldown Management
    /// <summary>
    /// Time left until the ability is no longer on cooldown.
    /// </summary>
    protected int CurrCooldown;

    /// <summary>
    /// Time the ability was in the given State.
    /// </summary>
    protected int CurrTime;

    /// <summary>
    /// True if ready to use, false if on cooldown.
    /// </summary>
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

    /// <summary>
    /// Creates dust when refreshed.
    /// </summary>
    protected virtual void OnRefreshed() {
      for (int i = 0; i < 10; i++) {
        Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("AbilityRefreshedDust"), newColor: RefreshColor);
      }
    }

    /// <summary>
    /// Set this ability on cooldown.
    /// </summary>
    /// <param name="force">Force ignores config options</param>
    internal virtual void PutOnCooldown(bool force = false) {
      if (force || OriMod.ConfigClient.AbilityCooldowns && (CooldownOnlyOnBoss ? OriUtils.IsAnyBossAlive(check: true) : true)) {
                CurrCooldown = Cooldown;
                Refreshed = false;
      }
    }

    /// <summary>
    /// Simple cooldown ticking. Can be overridden.
    /// </summary>
    protected virtual void TickCooldown() {
      if (CurrCooldown > 0 || !Refreshed) {
        CurrCooldown--;
        if (CurrCooldown < 0) {
          Refreshed = true;
        }
      }
    }
    #endregion

    #region Networking
    internal bool netUpdate = false;

    /// <summary>
    /// Do not use this outside of AbilityPacketHandler.
    /// </summary>
    internal void PreReadPacket(BinaryReader r) {
      AbilityState = (State)r.ReadByte();
      ReadPacket(r);
    }

    /// <summary>
    /// Do not use this outside of AbilityPacketHandler.
    /// </summary>
    internal void PreWritePacket(ModPacket packet) {
      packet.Write((byte)AbilityState);
      WritePacket(packet);
    }

    /// <summary>
    /// Ability-specific data to read from packet. Use in conjunction with WritePacket()
    /// </summary>
    protected virtual void ReadPacket(BinaryReader r) { }

    /// <summary>
    /// Ability-specific data to write to packet. Use in conjunction with ReadPacket()
    /// </summary>
    protected virtual void WritePacket(ModPacket packet) { }
    #endregion

    #region Ticking and Updating
    /// <summary>
    /// Called in OriPlayer.PostUpdateRunSpeeds(), directly after Ability.Update().
    /// 
    /// Always called, used for managing States
    /// </summary>
    internal abstract void Tick();

    /// <summary>
    /// Called in OriPlayer.PostUpdateRunSpeeds(), directly before Ability.Tick().
    /// 
    /// Only called if AbilityState is Starting, Active, or Ending
    /// </summary>
    internal void Update() {
      if (!InUse) {
        return;
      }

      switch (AbilityState) {
        case State.Active:
          UpdateActive();
          UpdateUsing();
          return;
        case State.Starting:
          UpdateStarting();
          UpdateUsing();
          return;
        case State.Ending:
          UpdateEnding();
          UpdateUsing();
          return;
        case State.Failed:
          UpdateFailed();
          return;
      }
    }

    /// <summary>
    /// Called before Update() when this update is in the Starting state.
    /// </summary>
    protected virtual void UpdateStarting() { }

    /// <summary>
    /// Called before Update() when this update is in the Active state.
    /// </summary>
    protected virtual void UpdateActive() { }

    /// <summary>
    /// Called before Update() when this update is in the Ending state.
    /// </summary>
    protected virtual void UpdateEnding() { }

    /// <summary>
    /// Called when this update is in the Failed state.
    /// </summary>
    protected virtual void UpdateFailed() { }

    /// <summary>
    /// Called directly after UpdateStarting(), UpdateActive(), and UpdateEnding()
    /// </summary>
    protected virtual void UpdateUsing() { }

    /// <summary>
    /// Rudimentary implementation, for now manually call in PlayerLayer
    /// </summary>
    internal virtual void DrawEffects() { }
    #endregion

    public override string ToString() => $"Ability ID:{Id} Player:{player.whoAmI} State:{AbilityState} Unlocked:{Unlocked} Cooldown:{CurrCooldown}/{Cooldown}";

    public override int GetHashCode() => Id * Main.player.Length + player.whoAmI;

    /// <summary>
    /// States that the Ability can be in. Determines update logic.
    /// </summary>
    public enum State {
      /// <summary>
      /// The ability will not perform any Update logic.
      /// </summary>
      Inactive = 0,
      /// <summary>
      /// The ability will use UpdateStarting() and UpdateUsing().
      /// </summary>
      Starting = 1,
      /// <summary>
      /// The ability will use UpdateActive() and UpdateUsing().
      /// </summary>
      Active = 2,
      /// <summary>
      /// The ability will use UpdateEnding() and UpdateUsing().
      /// </summary>
      Ending = 3,
      /// <summary>
      /// The ability will use UpdateFailed() only.
      /// </summary>
      Failed = 4
    }
  }
}
