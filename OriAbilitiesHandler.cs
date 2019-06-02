using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using OriMod.Movements;

namespace OriMod {
  // This partial class is for how abilities are handled.
  public sealed partial class OriAbilities {
    public OriPlayer oPlayer { get; private set; }
    public Player player { get; private set; }
    public List<Ability> Abilities { get; private set; }
    public WallJump wJump { get; private set; }
    public AirJump airJump { get; private set; }
    public Glide glide { get; private set; }
    public Stomp stomp { get; private set; }
    public Climb climb { get; private set; }
    public Dash dash { get; private set; }
    public ChargeDash cDash { get; private set; }
    internal OriAbilities(OriPlayer o) {
      oPlayer = o;
      player = o.player;
      Abilities = new List<Ability> {
        { wJump = new WallJump(o, this) },
        { airJump = new AirJump(o, this) },
        { glide = new Glide(o, this) },
        { stomp = new Stomp(o, this) },
        { climb = new Climb(o, this) },
        { dash = new Dash(o, this) },
        { cDash = new ChargeDash(o, this) }
      };
      for (int i = 0; i < Abilities.Count; i++) {
        Abilities[i].id = (byte)i;
      }
    }
    internal void UseAbility(byte id) { // To be used my players that are not Main.myPlayer
      if (player.whoAmI == Main.myPlayer) {
        return;
      }
      if (id > Abilities.Count - 1) {
        Main.NewText("OriAbilities.UseAbility(): Invalid id " + id, Color.Red);
        return;
      }
      Abilities[id].Update();
    }
  }
}