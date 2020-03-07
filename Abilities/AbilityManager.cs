using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using OriMod.Networking;

namespace OriMod {
  public static class AbilityID {
    public static byte SoulLink => 0;
    public static byte WallJump => 1;
    public static byte AirJump => 2;
    public static byte Bash => 3;
    public static byte Stomp => 4;
    public static byte Glide => 5;
    public static byte Climb => 6;
    public static byte ChargeJump => 7;
    public static byte WallChargeJump => 8;
    public static byte Dash => 9;
    public static byte ChargeDash => 10;
    public static byte LookUp => 11;
    public static byte Crouch => 12;
    public static byte Burrow => 13;
    public static int Count => 14;
  }
}

namespace OriMod.Abilities {
  public sealed class AbilityManager {
    internal AbilityManager(OriPlayer oPlayer) {
      this.oPlayer = oPlayer;
      if (oPlayer.player.whoAmI == Main.myPlayer) {
        Local = this;
      }
      soulLink = new SoulLink(this);
      wJump = new WallJump(this);
      stomp = new Stomp(this);
      airJump = new AirJump(this);
      bash = new Bash(this);
      glide = new Glide(this);
      climb = new Climb(this);
      cJump = new ChargeJump(this);
      wCJump = new WallChargeJump(this);
      dash = new Dash(this);
      cDash = new ChargeDash(this);
      crouch = new Crouch(this);
      lookUp = new LookUp(this);
      burrow = new Burrow(this);
    }

    public static AbilityManager Local { get; private set; }

    public readonly OriPlayer oPlayer;

    public readonly SoulLink soulLink;
    public readonly WallJump wJump;
    public readonly AirJump airJump;
    public readonly Bash bash;
    public readonly Stomp stomp;
    public readonly Glide glide;
    public readonly Climb climb;
    public readonly ChargeJump cJump;
    public readonly WallChargeJump wCJump;
    public readonly Dash dash;
    public readonly ChargeDash cDash;
    public readonly LookUp lookUp;
    public readonly Crouch crouch;
    public readonly Burrow burrow;

    public Ability this[int index] {
      get {
        switch (index) {
          case 0:
            return soulLink;
          case 1:
            return wJump;
          case 2:
            return airJump;
          case 3:
            return bash;
          case 4:
            return stomp;
          case 5:
            return glide;
          case 6:
            return climb;
          case 7:
            return cJump;
          case 8:
            return wCJump;
          case 9:
            return dash;
          case 10:
            return cDash;
          case 11:
            return lookUp;
          case 12:
            return crouch;
          case 13:
            return burrow;
          default:
            throw new System.ArgumentOutOfRangeException();
        }
      }
    }

    internal void Tick() {
      if (oPlayer.player.whoAmI != Main.myPlayer) {
        for (int a = 0; a < AbilityID.Count; a++) {
          var ability = this[a];
          if (ability.InUse) {
            ability.Tick();
          }
        }
        return;
      }
      for (int a = 0; a < AbilityID.Count; a++) {
        this[a].Tick();
      }
    }

    internal void Update() {
      for (int a = 0; a < AbilityID.Count; a++) {
        var ability = this[a];
        if (ability.DoUpdate) {
          ability.Update();
        }
      }
    }

    internal void NetSync() {
      if (oPlayer.player.whoAmI != Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient) {
        return;
      }

      var changes = new List<byte>();
      for (int a = 0; a < AbilityID.Count; a++) {
        if (this[a].netUpdate) {
          this[a].netUpdate = false;
          changes.Add((byte)a);
        }
      }
      if (changes.Count > 0) {
        ModNetHandler.Instance.abilityPacketHandler.SendAbilityState(255, oPlayer.player.whoAmI, changes);
      }
    }

    internal void DisableAllAbilities() {
      for (int a = 0; a < AbilityID.Count; a++) {
        this[a].SetState(Ability.State.Inactive);
      }
    }

    internal static void Unload() {
      Local = null;
      Burrow.Unload();
      SoulLink.Unload();
    }
  }
}
