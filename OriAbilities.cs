using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using OriMod.Abilities;

namespace OriMod {
  
  // This partial class is for Ability-specific implementations    
  public sealed partial class OriAbilities {
    public OriPlayer oPlayer { get; private set; }
    public Player player { get; private set; }
    public List<Ability> Abilities { get; private set; }

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
        { soulLink = new SoulLink(o, this) },
        { wJump = new WallJump(o, this) },
        { stomp = new Stomp(o, this) },
        { airJump = new AirJump(o, this) },
        { bash = new Bash(o, this) },
        { glide = new Glide(o, this) },
        { climb = new Climb(o, this) },
        { cJump = new ChargeJump(o, this) },
        { wCJump = new WallChargeJump(o, this) },
        { dash = new Dash(o, this) },
        { cDash = new ChargeDash(o, this) },
        { crouch = new Crouch(o, this) },
        { lookUp = new LookUp(o, this) },
        { burrow = new Burrow(o, this) },
      };
      for (int i = 0; i < Abilities.Count; i++) {
        Abilities[i].id = (byte)i;
      }
    }
    public Ability this[int index] => Abilities[index];
    internal void Tick() {
      if (oPlayer == null) return;

      if (player.whoAmI != Main.myPlayer) {
        TickOtherClient();
        return;
      }

      byte[] prevStates = new byte[Abilities.Count];
      for (int a = 0; a < Abilities.Count; a++) {
        prevStates[a] = ((byte)Abilities[a].State);
      }

      Abilities.ForEach(ability => {
        ability.Tick();
      });

      List<byte> changes = new List<byte>();
      for (int a = 0; a < Abilities.Count; a++) {
        if ((byte)Abilities[a].State != prevStates[a]) {
          changes.Add((byte)a);
        }
      }

      if (changes.Count > 0) {
        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
          ModNetHandler.abilityPacketHandler.SendAbilityState(255, player.whoAmI, changes);
        }
      }
    }
    internal void Update() {
      Abilities.ForEach(ability => {
        if (ability.DoUpdate) ability.Update();
      });
    }
    internal void TickOtherClient() {
      Abilities.ForEach(ability => {
        if (ability.InUse) {
          ability.Tick();
        }
      });
    }
  }
}