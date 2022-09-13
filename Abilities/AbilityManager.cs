using System;
using System.Collections.Generic;
using OriMod.Networking;
using Terraria;
using Terraria.ModLoader.IO;

namespace OriMod {
  /// <summary>
  /// IDs for each <see cref="Abilities.Ability"/>.
  /// </summary>
  public static class AbilityId {
    /// <summary>
    /// ID for <see cref="Abilities.SoulLink"/>.
    /// </summary>
    public const byte SoulLink = 0;
    /// <summary>
    /// ID for <see cref="Abilities.WallJump"/>.
    /// </summary>
    public const byte WallJump = 1;
    /// <summary>
    /// ID for <see cref="Abilities.AirJump"/>.
    /// </summary>
    public const byte AirJump = 2;
    /// <summary>
    /// ID for <see cref="Abilities.Bash"/>.
    /// </summary>
    public const byte Bash = 3;
    /// <summary>
    /// ID for <see cref="Abilities.Stomp"/>.
    /// </summary>
    public const byte Stomp = 4;
    /// <summary>
    /// ID for <see cref="Abilities.Glide"/>.
    /// </summary>
    public const byte Glide = 5;
    /// <summary>
    /// ID for <see cref="Abilities.Climb"/>.
    /// </summary>
    public const byte Climb = 6;
    /// <summary>
    /// ID for <see cref="Abilities.ChargeJump"/>.
    /// </summary>
    public const byte ChargeJump = 7;
    /// <summary>
    /// ID for <see cref="Abilities.WallChargeJump"/>.
    /// </summary>
    public const byte WallChargeJump = 8;
    /// <summary>
    /// ID for <see cref="Abilities.Dash"/>.
    /// </summary>
    public const byte Dash = 9;
    /// <summary>
    /// ID for <see cref="Abilities.ChargeDash"/>.
    /// </summary>
    public const byte ChargeDash = 10;
    /// <summary>
    /// ID for <see cref="Abilities.LookUp"/>.
    /// </summary>
    public const byte LookUp = 11;
    /// <summary>
    /// ID for <see cref="Abilities.Crouch"/>.
    /// </summary>
    public const byte Crouch = 12;
    /// <summary>
    /// ID for <see cref="Abilities.Burrow"/>.
    /// </summary>
    public const byte Burrow = 13;
    /// <summary>
    /// ID for <see cref="Abilities.Launch"/>
    /// </summary>
    public const byte Launch = 14;
    /// <summary>
    /// ID count for iterating through a loop.
    /// </summary>
    public static readonly int Count = 15;
  }
}

namespace OriMod.Abilities {
  /// <summary>
  /// Class for containing and updating all <see cref="Ability"/>s on an <see cref="OriPlayer"/>.
  /// </summary>
  public sealed class AbilityManager {
    internal AbilityManager(OriPlayer oPlayer) {
      this.oPlayer = oPlayer;

      //soulLink = new SoulLink(this);
      wallJump = new WallJump(this);
      stomp = new Stomp(this);
      airJump = new AirJump(this);
      bash = new Bash(this);
      glide = new Glide(this);
      climb = new Climb(this);
      chargeJump = new ChargeJump(this);
      wallChargeJump = new WallChargeJump(this);
      dash = new Dash(this);
      chargeDash = new ChargeDash(this);
      lookUp = new LookUp(this);
      crouch = new Crouch(this);
      burrow = new Burrow(this);
      launch = new Launch(this);
    }

    /// <summary>
    /// The <see cref="OriPlayer"/> this <see cref="AbilityManager"/> belongs to.
    /// </summary>
    public readonly OriPlayer oPlayer;

    //[Obsolete] public readonly SoulLink soulLink;

    public readonly WallJump wallJump;
    public readonly AirJump airJump;
    public readonly Bash bash;
    public readonly Stomp stomp;
    public readonly Glide glide;
    public readonly Climb climb;
    public readonly ChargeJump chargeJump;
    public readonly WallChargeJump wallChargeJump;
    public readonly Dash dash;
    public readonly ChargeDash chargeDash;
    public readonly LookUp lookUp;
    public readonly Crouch crouch;
    public readonly Burrow burrow;
    public readonly Launch launch;

    public IEnumerator<Ability> GetEnumerator() {
      //yield return soulLink;
      yield return wallJump;
      yield return airJump;
      yield return bash;
      yield return launch; // Run Launch directly after Bash
      yield return stomp;
      yield return glide;
      yield return climb;
      yield return chargeJump;
      yield return wallChargeJump;
      yield return dash;
      yield return chargeDash;
      yield return lookUp;
      yield return crouch;
      yield return burrow;
    }

    /// <summary>
    /// Get the <see cref="Ability"/> with the matching <see cref="AbilityId"/>.
    /// </summary>
    /// <param name="index">Index that corresponds to an <see cref="AbilityId"/>.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">The value does not match any <see cref="AbilityId"/>.</exception>
    /// <returns>An <see cref="Ability"/> with the matching <see cref="AbilityId"/>.</returns>
    public Ability this[int index] {
      get {
        switch (index) {
          //case AbilityID.SoulLink: return soulLink;
          case AbilityId.WallJump: return wallJump;
          case AbilityId.AirJump: return airJump;
          case AbilityId.Bash: return bash;
          case AbilityId.Stomp: return stomp;
          case AbilityId.Glide: return glide;
          case AbilityId.Climb: return climb;
          case AbilityId.ChargeJump: return chargeJump;
          case AbilityId.WallChargeJump: return wallChargeJump;
          case AbilityId.Dash: return dash;
          case AbilityId.ChargeDash: return chargeDash;
          case AbilityId.LookUp: return lookUp;
          case AbilityId.Crouch: return crouch;
          case AbilityId.Burrow: return burrow;
          case AbilityId.Launch: return launch;
          default: throw new ArgumentOutOfRangeException(nameof(index), $"The value {index} does not match any AbilityID.");
        }
      }
    }

    /// <summary>
    /// Update loop for abilities.
    /// <para>If unable to use abilities: disables all abilities.</para>
    /// <para>Conditionally calls <see cref="Ability.Tick"/> and <see cref="Ability.Update"/> on all abilities.</para>
    /// </summary>
    internal void Update() {
      if (!CanUseAnyAbilities()) {
        DisableAllAbilities();
        return;
      }

      // Tick
      foreach (Ability ability in this) {
        if (!ability.Unlocked) continue;
        ability.CurrentTime++;
        ability.Tick();
      }

      // Update
      foreach (Ability ability in this) {
        if (ability.Unlocked) {
          ability.Update();
        }
      }

      foreach (Ability ability in this) {
        if (ability.Unlocked) {
          ability.PostUpdateAbilities();
        }
      }
    }

    internal void PostUpdate() {
      if (!CanUseAnyAbilities()) {
        return;
      }

      foreach (Ability ability in this) {
        if (ability.Unlocked) {
          ability.PostUpdate();
        }
      }
    }

    /// <summary>
    /// Condition for if the player can use any abilities.
    /// <para>Used to disable all abilities.</para>
    /// </summary>
    /// <returns><see langword="true"/> if any ability can be used; otherwise, <see langword="false"/>.</returns>
    private bool CanUseAnyAbilities() {
      Player player = oPlayer.Player;
      if (player.dead) {
        return false;
      }
      var _k = player.mount?.Active;
      return !(_k ?? false);
    }

    /// <summary>
    /// Deactivates all abilities.
    /// </summary>
    public void DisableAllAbilities() {
      foreach (Ability ability in this) {
        ability.SetState(Ability.State.Inactive);
      }
    }

    /// <summary>
    /// Sets the level of all Levelable Abilities to their max level.
    /// </summary>
    public void UnlockAllAbilities() {
      foreach (Ability ability in this) {
        if (ability is ILevelable levelable) {
          levelable.Level = levelable.MaxLevel;
        }
      }
    }


    /// <summary>
    /// Sets the level of all Levelable Abilities to 0.
    /// </summary>
    public void ResetAllAbilities() {
      foreach (Ability ability in this) {
        if (ability is ILevelable levelable) {
          levelable.Level = 0;
        }
      }
    }

    /// <summary>
    /// Save all abilities to the given <see cref="TagCompound"/>.
    /// </summary>
    /// <param name="tag"><see cref="TagCompound"/> to save abilities to.</param>
    public void Save(TagCompound tag) {
      byte[] arr = new byte[AbilityId.Count];
      foreach (Ability ability in this) {
        // Non-ILevelable abilities saved anyways
        arr[ability.Id] = ability.Level;
      }
      tag.Add(AbilityTagKey, arr);
    }

    /// <summary>
    /// Load all abilities from the given <see cref="TagCompound"/>.
    /// </summary>
    /// <param name="tag"><see cref="TagCompound"/> to load abilities from.</param>
    public void Load(TagCompound tag) {
      if (!tag.ContainsKey(AbilityTagKey)) {
        return;
      }
      byte[] arr = tag.GetByteArray(AbilityTagKey);
      foreach (Ability ability in this) {
        if (ability is ILevelable levelable) {
          levelable.Level = arr[ability.Id];
        }
      }
    }

    internal void SendClientChanges() {
      var changes = new List<byte>();
      foreach (Ability ability in this) {
        if (!ability.netUpdate) continue;
        ability.netUpdate = false;
        changes.Add((byte)ability.Id);
      }
      if (changes.Count > 0) {
        ModNetHandler.Instance.abilityPacketHandler.SendAbilityState(255, oPlayer.Player.whoAmI, changes);
      }
    }

    private const string AbilityTagKey = "AbilityLevels";
  }
}
