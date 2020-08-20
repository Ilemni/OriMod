using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using OriMod.Networking;

namespace OriMod {
  public static class AbilityID {
    public const byte SoulLink = 0;
    public const byte WallJump = 1;
    public const byte AirJump = 2;
    public const byte Bash = 3;
    public const byte Stomp = 4;
    public const byte Glide = 5;
    public const byte Climb = 6;
    public const byte ChargeJump = 7;
    public const byte WallChargeJump = 8;
    public const byte Dash = 9;
    public const byte ChargeDash = 10;
    public const byte LookUp = 11;
    public const byte Crouch = 12;
    public const byte Burrow = 13;
    public const int Count = 14;
  }
}

namespace OriMod.Abilities {
  public sealed class AbilityManager {
    internal AbilityManager(OriPlayer oPlayer) {
      this.oPlayer = oPlayer;

      soulLink = new SoulLink(this);
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

    public static AbilityManager Local { get; private set; }

    public readonly OriPlayer oPlayer;

    public readonly SoulLink soulLink;
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
      yield return soulLink;
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

    public Ability this[int idx] {
      get {
        switch (idx) {
          case AbilityID.SoulLink: return soulLink;
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

    internal void Update() {
      if (!CanUseAnyAbilities()) {
        DisableAllAbilities();
        return;
    }

      // Tick
      if (oPlayer.player.whoAmI != Main.myPlayer) {
        foreach (var ability in this) {
          if (ability.InUse) {
            ability.Tick();
          }
        }
        return;
      }
      foreach (var ability in this) {
        ability.Tick();
      }

      // Update
      foreach (var ability in this) {
        if (ability.UpdateCondition) {
          ability.Update();
        }
      }

      // Net sync
      if (oPlayer.player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient) {
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
    }

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

    public void DisableAllAbilities() {
      foreach (var ability in this) {
        ability.SetState(Ability.State.Inactive);
      }
    }

    internal static void Unload() {
      Burrow.Unload();
      SoulLink.Unload();
    }
  }
}
