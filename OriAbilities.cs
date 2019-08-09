using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using OriMod.Abilities;

namespace OriMod {
  public static class AbilityID {
    public static byte SoulLink => 1;
    public static byte WallJump => 2;
    public static byte AirJump => 3;
    public static byte Bash => 4;
    public static byte Stomp => 5;
    public static byte Glide => 6;
    public static byte Climb => 7;
    public static byte ChargeJump => 8;
    public static byte WallChargeJump => 9;
    public static byte Dash => 10;
    public static byte ChargeDash => 11;
    public static byte Grenade => 12; // Unused
    public static byte LookUp => 13;
    public static byte Crouch => 14;
    public static byte Burrow => 15;
  }
  
  public sealed class OriAbilities {
    public OriPlayer oPlayer { get; }
    public Player player { get; }
    public List<Ability> Abilities { get; }

    private Ability[] _a { get; }
    public Ability this[int index] => _a[index];

    public SoulLink soulLink { get; }
    public WallJump wJump { get; }
    public AirJump airJump { get; }
    public Bash bash { get; }
    public Stomp stomp { get; }
    public Glide glide { get; }
    public Climb climb { get; }
    public ChargeJump cJump { get; }
    public WallChargeJump wCJump { get; }
    public Dash dash { get; }
    public ChargeDash cDash { get; }
    public LookUp lookUp { get; }
    public Crouch crouch { get; }
    public Burrow burrow { get; }
    
    internal OriAbilities(OriPlayer o) {
      oPlayer = o;
      player = o.player;
      Abilities = new List<Ability> {
        { soulLink = new SoulLink(this) },
        { wJump = new WallJump(this) },
        { stomp = new Stomp(this) },
        { airJump = new AirJump(this) },
        { bash = new Bash(this) },
        { glide = new Glide(this) },
        { climb = new Climb(this) },
        { cJump = new ChargeJump(this) },
        { wCJump = new WallChargeJump(this) },
        { dash = new Dash(this) },
        { cDash = new ChargeDash(this) },
        { crouch = new Crouch(this) },
        { lookUp = new LookUp(this) },
        { burrow = new Burrow(this) },
      };
      _a = new Ability[Abilities.Count];
      Abilities.ForEach(a => _a[a.id] = a);
    }
    
    
    internal void Tick() {
      if (player.whoAmI != Main.myPlayer) {
        TickOtherClient();
        return;
      }

      Abilities.ForEach(a => a.Tick());
    }

    internal void Sync() {
      if (player.whoAmI != Main.myPlayer || Main.netMode != NetmodeID.MultiplayerClient) return;
        
      List<byte> changes = new List<byte>();
      for (int a = 0; a < Abilities.Count; a++) {
        if (Abilities[a].netUpdate) {
          changes.Add((byte)a);
        }
      }
      if (changes.Count > 0) {
        ModNetHandler.abilityPacketHandler.SendAbilityState(255, player.whoAmI, changes);
      }
    }

    internal void Update() {
      Abilities.ForEach(a => {
        if (a.DoUpdate) a.Update();
      });
    }
    
    internal void TickOtherClient() {
      Abilities.ForEach(a => {
        if (a.InUse) a.Tick();
      });
    }
    
    internal void DisableAllAbilities() => Abilities.ForEach(a => a.Inactive = true);
  }
}