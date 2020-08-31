using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using OriMod.Networking;
using Terraria.ModLoader.IO;

namespace OriMod {
  /// <summary>
  /// IDs for each <see cref="Abilities.Ability"/>.
  /// </summary>
  public static class AbilityID {
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
    /// ID count for iterating through a loop.
    /// </summary>
    public const int Count = 14;
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
    }

    /// <summary>
    /// The <see cref="OriPlayer"/> this <see cref="AbilityManager"/> belongs to.
    /// </summary>
    public readonly OriPlayer oPlayer;

    [System.Obsolete] public readonly SoulLink soulLink;

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

    public IEnumerator<Ability> GetEnumerator() {
      //yield return soulLink;
      yield return wallJump;
      yield return airJump;
      yield return bash;
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
    /// Get the <see cref="Ability"/> with the matching <see cref="AbilityID"/>.
    /// </summary>
    /// <param name="idx"></param>
    /// <exception cref="ArgumentOutOfRangeException">The value does not match any <see cref="AbilityID"/>.</exception>
    /// <returns></returns>
    public Ability this[int idx] {
      get {
        switch (idx) {
          //case AbilityID.SoulLink: return soulLink;
          case AbilityID.WallJump: return wallJump;
          case AbilityID.AirJump: return airJump;
          case AbilityID.Bash: return bash;
          case AbilityID.Stomp: return stomp;
          case AbilityID.Glide: return glide;
          case AbilityID.Climb: return climb;
          case AbilityID.ChargeJump: return chargeJump;
          case AbilityID.WallChargeJump: return wallChargeJump;
          case AbilityID.Dash: return dash;
          case AbilityID.ChargeDash: return chargeDash;
          case AbilityID.LookUp: return lookUp;
          case AbilityID.Crouch: return crouch;
          case AbilityID.Burrow: return burrow;
          default: throw new System.ArgumentOutOfRangeException(nameof(idx));
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
      foreach (var ability in this) {
        if (ability.InUse || oPlayer.IsLocal) {
          ability.CurrentTime++;
          ability.Tick();
        }
      }

      // Update
      foreach (var ability in this) {
        ability.Update();
      }
    }

    /// <summary>
    /// Condition for if the player can use any abilities.
    /// <para>Used to disable all abilities.</para>
    /// </summary>
    /// <returns></returns>
    public bool CanUseAnyAbilities() {
      var player = oPlayer.player;
      if (player.dead) {
        return false;
      }
      if (player.mount?.Active ?? false) {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Deactivates all abilities.
    /// </summary>
    public void DisableAllAbilities() {
      foreach (var ability in this) {
        ability.SetState(Ability.State.Inactive);
      }
    }

    public void UnlockAllAbilities() {
      foreach (var ability in this) {
        if (ability is ILevelable levelable) {
          levelable.Level = levelable.MaxLevel;
        }
      }
    }

    /// <summary>
    /// Save all abilities to the given <see cref="TagCompound"/>.
    /// </summary>
    /// <param name="tag"><see cref="TagCompound"/> to save abilities to.</param>
    public void Save(TagCompound tag) {
      var arr = new byte[AbilityID.Count];
      foreach (var ability in this) {
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
      var arr = tag.GetByteArray(AbilityTagKey);
      foreach (var ability in this) {
        if (ability is ILevelable levelable) {
          levelable.Level = arr[ability.Id];
        }
      }
    }

    internal void SendClientChanges() {
      var changes = new List<byte>();
      foreach (var ability in this) {
        if (ability.netUpdate) {
          ability.netUpdate = false;
          changes.Add((byte)ability.Id);
        }
      }
      if (changes.Count > 0) {
        ModNetHandler.Instance.abilityPacketHandler.SendAbilityState(255, oPlayer.player.whoAmI, changes);
      }
    }

    private const string AbilityTagKey = "AbilityLevels";
  }
}
