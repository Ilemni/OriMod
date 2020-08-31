using System.IO;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Base class for all player abilities.
  /// </summary>
  public abstract class Ability {
    /// <summary>
    /// Only construct this in AbilityManager.
    /// </summary>
    internal Ability(AbilityManager abilities) {
      oPlayer = abilities.oPlayer;
      player = oPlayer.player;
    }

    /// <summary>
    /// Configuration for abilities.
    /// </summary>
    internal static OriConfigClient2 Config => OriMod.ConfigAbilities;

    /// <summary>
    /// The <see cref="OriPlayer"/> this ability belongs to.
    /// </summary>
    public readonly OriPlayer oPlayer;

    /// <summary>
    /// The <see cref="Player"/> this ability belongs to.
    /// </summary>
    public readonly Player player;

    /// <summary>
    /// The <see cref="AbilityManager"/> this ability belongs to.
    /// </summary>
    public AbilityManager abilities => oPlayer.abilities;


    public bool IsLocal => oPlayer.IsLocal;

    /// <summary>
    /// Determines if the ability has been unlocked by the player. Currently unimplemented.
    /// </summary>
    public virtual bool Unlocked => Level > 0;

    /// <summary>
    /// Current level of the ability. Unless an ability is always unlocked, if this is 0, the ability is not unlocked.
    /// </summary>
    public virtual byte Level { get; set; } = 1;

    #region General Properties
    /// <summary>
    /// Unique ID of this ability. Corresponds with an <see cref="AbilityID"/>.
    /// </summary>
    public abstract int Id { get; }

    /// <summary>
    /// Condition required for the player to activate this ability.
    /// </summary>
    internal virtual bool CanUse => Unlocked && Refreshed;

    /// <summary>
    /// Cooldown of the ability. This should point to an <see cref="OriConfigClient2"/> field.
    /// </summary>
    protected virtual int Cooldown { get; }

    /// <summary>
    /// Determines if the ability only goes on cooldown if a boss is active.
    /// <para>If true, cooldown penalties only ever occur if a boss is alive.</para>
    /// <para>If false, cooldown penalties occur regardless of boss status.</para>
    /// </summary>
    protected virtual bool CooldownOnlyOnBoss => false;

    /// <summary>
    /// Color of dust spawned in <see cref="OnRefreshed"/>.
    /// <para>This property is only ever used if <see cref="Cooldown"/> is not <c>0</c>.</para>
    /// </summary>
    protected virtual Color RefreshColor => Color.White;
    #endregion

    #region States
    /// <summary>
    /// Current <see cref="State"/> of the ability.
    /// <para>Setting this value is done with <see cref="SetState(State, bool)"/>. This should only be done within <see cref="Tick"/>.</para>
    /// </summary>
    public State AbilityState { get; private set; }

    /// <summary>
    /// Sets the <see cref="State"/> of the ability to <paramref name="state"/>.
    /// <para>This should only be used within the <see cref="Tick"/> method.</para>
    /// </summary>
    /// <param name="state">State to set <see cref="AbilityState"/> to.</param>
    /// <param name="preserveCurrentTime">Whether to preserve or reset <see cref="CurrentTime"/>. Resets by default.</param>
    public void SetState(State state, bool preserveCurrentTime = false) {
      if (state != AbilityState) {
        //Main.NewText($"{GetType().Name}: {state}");
        netUpdate = true;
        AbilityState = state;
        if (!preserveCurrentTime) {
          CurrentTime = 0;
        }
      }
    }

    /// <summary>
    /// If <see cref="AbilityState"/> is <see cref="State.Inactive"/>.
    /// </summary>
    public bool Inactive => AbilityState == State.Inactive;

    /// <summary>
    /// If <see cref="AbilityState"/> is <see cref="State.Starting"/>.
    /// </summary>
    public bool Starting => AbilityState == State.Starting;

    /// <summary>
    /// If <see cref="AbilityState"/> is <see cref="State.Active"/>.
    /// </summary>
    public bool Active => AbilityState == State.Active;

    /// <summary>
    /// If <see cref="AbilityState"/> is <see cref="State.Ending"/>.
    /// </summary>
    public bool Ending => AbilityState == State.Ending;

    /// <summary>
    /// If <see cref="AbilityState"/> is either <see cref="State.Starting"/>, <see cref="State.Active"/>, or <see cref="State.Ending"/>.
    /// </summary>
    /// <value>True if the state is either <see cref="State.Starting"/>, <see cref="State.Active"/>, or <see cref="State.Ending"/>.</value>
    public bool InUse => Starting || Active || Ending;
    #endregion

    #region Time and Cooldown Management
    /// <summary>
    /// Time left until the ability is no longer on cooldown.
    /// </summary>
    protected int currentCooldown;

    /// <summary>
    /// Time the ability was in the current State.
    /// <para>This is automatically incremented every frame, and is reset to 0 from <see cref="SetState(State, bool)"/></para>
    /// </summary>
    public int CurrentTime { get; internal set; }

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
    /// Creates dust when <see cref="Refreshed"/> becomes true.
    /// </summary>
    protected virtual void OnRefreshed() {
      for (int i = 0; i < 10; i++) {
        Dust.NewDust(player.Center, 12, 12, ModContent.DustType<Dusts.AbilityRefreshedDust>(), newColor: RefreshColor);
      }
    }

    /// <summary>
    /// Set this ability on cooldown.
    /// </summary>
    /// <param name="force">If true, ignore config options that may otherwise prevent cooldown.</param>
    internal virtual void PutOnCooldown(bool force = false) {
      if (force || OriMod.ConfigClient.AbilityCooldowns && (!CooldownOnlyOnBoss || OriUtils.AnyBossAlive())) {
        currentCooldown = Cooldown;
        Refreshed = false;
      }
    }

    /// <summary>
    /// Simple cooldown ticking. Can be overridden.
    /// </summary>
    protected virtual void TickCooldown() {
      if (currentCooldown > 0 || !Refreshed) {
        currentCooldown--;
        if (currentCooldown < 0) {
          Refreshed = true;
        }
      }
    }
    #endregion

    #region Networking
    /// <summary>
    /// If true, the ability will be put into the next ability packet.
    /// </summary>
    internal bool netUpdate = false;

    /// <summary>
    /// For <see cref="Networking.AbilityPacketHandler"/>.
    /// </summary>
    internal void PreReadPacket(BinaryReader r) {
      AbilityState = (State)r.ReadByte();
      ReadPacket(r);
    }

    /// <summary>
    /// For <see cref="Networking.AbilityPacketHandler"/>.
    /// </summary>
    internal void PreWritePacket(ModPacket packet) {
      packet.Write((byte)AbilityState);
      WritePacket(packet);
    }

    /// <summary>
    /// Ability-specific data to read from packet. Use in conjunction with <see cref="WritePacket(ModPacket)"/>.
    /// </summary>
    protected virtual void ReadPacket(BinaryReader r) { }

    /// <summary>
    /// Ability-specific data to write to packet. Use in conjunction with <see cref="ReadPacket(BinaryReader)"/>.
    /// </summary>
    protected virtual void WritePacket(ModPacket packet) { }
    #endregion

    #region Ticking and Updating
    /// <summary>
    /// Called in <see cref="OriPlayer.PostUpdateRunSpeeds"/>, directly after <see cref="Update"/>.
    /// Always called, this should <strong>only</strong> be used for managing <see cref="AbilityState"/>.
    /// </summary>
    /// <remarks>
    /// As some changes are only possible to make on the local client (i.e. Glide due to dependence on <see cref="OriMod.FeatherKey"/>), the only changes should be to state.
    /// <para>If some other changes must be made here and not in any Update methods (i.e. Bash targeting), they must be synced in <see cref="ReadPacket(BinaryReader)"/> and <see cref="WritePacket(ModPacket)"/>.</para>
    /// </remarks>
    internal abstract void Tick();

    /// <summary>
    /// Calls all Update methods in this class.
    /// <para>Called in <see cref="OriPlayer.PostUpdateRunSpeeds"/>, directly after <see cref="Tick"/>.</para>
    /// <para>Only called if <see cref="AbilityState"/> is <see cref="State.Starting"/>, <see cref="State.Active"/>, or <see cref="State.Ending"/>.</para>
    /// </summary>
    internal void Update() {
      switch (AbilityState) {
        case State.Active:
          UpdateActive();
          break;
        case State.Starting:
          UpdateStarting();
          break;
        case State.Ending:
          UpdateEnding();
          break;
        case State.Inactive:
          return;
      }
      UpdateUsing();
    }

    /// <summary>
    /// Called before <see cref="Update"/> when this <see cref="AbilityState"/> is <see cref="State.Starting"/>.
    /// </summary>
    protected virtual void UpdateStarting() { }

    /// <summary>
    /// Called before <see cref="Update"/> when this <see cref="AbilityState"/> is <see cref="State.Active"/>.
    /// <para>It is recommended to make any changes to <c>player.control*</c> in <see cref="UpdateUsing"/>, if it is overridden.</para>
    /// </summary>
    protected virtual void UpdateActive() { }

    /// <summary>
    /// Called before <see cref="Update"/> when this <see cref="AbilityState"/> is <see cref="State.Ending"/>.
    /// </summary>
    protected virtual void UpdateEnding() { }

    /// <summary>
    /// Called directly after <see cref="UpdateStarting"/>, <see cref="UpdateActive"/>, and <see cref="UpdateEnding"/>.
    /// <para>Any modifications to <c>player.control*</c> should be at the end of this method.</para>
    /// </summary>
    protected virtual void UpdateUsing() { }

    /// <summary>
    /// Rudimentary implementation, for now manually called in <see cref="OriLayers"/>.
    /// </summary>
    internal virtual void DrawEffects() { }
    #endregion

    public override string ToString() => $"Ability ID:{Id} Player:{player.whoAmI} State:{AbilityState} Level:{Level} Cooldown:{currentCooldown}/{Cooldown}";

    /// <summary>
    /// States that the <see cref="Ability"/> can be in. Determines update logic.
    /// </summary>
    public enum State : byte {
      /// <summary>
      /// The ability will not perform any Update logic.
      /// </summary>
      Inactive = 0,
      /// <summary>
      /// The ability will use <see cref="UpdateStarting"/> and <see cref="UpdateUsing"/>.
      /// </summary>
      Starting = 1,
      /// <summary>
      /// The ability will use <see cref="UpdateActive"/> and <see cref="UpdateUsing"/>.
      /// </summary>
      Active = 2,
      /// <summary>
      /// The ability will use <see cref="UpdateEnding"/> and <see cref="UpdateUsing"/>.
      /// </summary>
      Ending = 3,
    }
  }
}
