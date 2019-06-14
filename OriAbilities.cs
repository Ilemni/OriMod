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

    public WallJump wJump { get; private set; }
    public AirJump airJump { get; private set; }
    public Bash bash { get; private set; }
    public Stomp stomp { get; private set; }
    public Glide glide { get; private set; }
    public Climb climb { get; private set; }
    public Dash dash { get; private set; }
    public ChargeDash cDash { get; private set; }
    public LookUp lookUp { get; private set; }
    public Crouch crouch { get; private set; }
    internal OriAbilities(OriPlayer o) {
      oPlayer = o;
      player = o.player;
      Abilities = new List<Ability> {
        { wJump = new WallJump(o, this) },
        { stomp = new Stomp(o, this) },
        { airJump = new AirJump(o, this) },
        { bash = new Bash(o, this) },
        { glide = new Glide(o, this) },
        { climb = new Climb(o, this) },
        { dash = new Dash(o, this) },
        { cDash = new ChargeDash(o, this) },
        { crouch = new Crouch(o, this) },
        { lookUp = new LookUp(o, this) },
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
    internal void TickOtherClient() {
      Abilities.ForEach(ability => {
        if (ability.InUse) {
          ability.Tick();
        }
      });
    }
  }
}