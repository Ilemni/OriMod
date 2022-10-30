using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using Terraria;
using Terraria.ModLoader;
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
  public sealed class OriAbilityManager : AbilityManager {

    public WallJump wallJump { get; private set; }
    public AirJump airJump { get; private set; }
    public Bash bash { get; private set; }
    public Stomp stomp { get; private set; }
    public Glide glide { get; private set; }
    public Climb climb { get; private set; }
    public ChargeJump chargeJump { get; private set; }
    public WallChargeJump wallChargeJump { get; private set; }
    public Dash dash { get; private set; }
    public ChargeDash chargeDash { get; private set; }
    public LookUp lookUp { get; private set; }
    public Crouch crouch { get; private set; }
    public Burrow burrow { get; private set; }
    public Launch launch { get; private set; }


    public override bool CanUseAnyAbilities() {
      if (player.dead || oPlayer.Transforming) {
        return false;
      }
      var _k = player.mount?.Active;
      return !(_k ?? false);
    }

    public OriPlayer oPlayer { get; private set; }

    public override void PhysicsPreUpdate() => oPlayer.PostUpdatePhysics();

    public override void Initialize() {
      wallJump = (WallJump)this[AbilityId.WallJump];
      airJump = (AirJump)this[AbilityId.AirJump];
      bash = (Bash)this[AbilityId.Bash];
      stomp = (Stomp)this[AbilityId.Stomp];
      glide = (Glide)this[AbilityId.Glide];
      climb = (Climb)this[AbilityId.Climb];
      chargeJump = (ChargeJump)this[AbilityId.ChargeJump];
      wallChargeJump = (WallChargeJump)this[AbilityId.WallChargeJump];
      dash = (Dash)this[AbilityId.Dash];
      chargeDash = (ChargeDash)this[AbilityId.ChargeDash];
      lookUp = (LookUp)this[AbilityId.LookUp];
      crouch = (Crouch)this[AbilityId.Crouch];
      burrow = (Burrow)this[AbilityId.Burrow];
      launch = (Launch)this[AbilityId.Launch];
      oPlayer = player.GetModPlayer<OriPlayer>();
    }

    internal void RefreshParticles(Color col) {
      int dust_type = ModContent.DustType<AbilityRefreshedDust>();
      for (int i = 0; i < 10; i++) {
        Dust.NewDust(player.Center, 12, 12, dust_type, newColor: col);
      }
    }

    public void OldSave(TagCompound tag) {
      byte[] arr = new byte[AbilityId.Count];
      foreach (Ability ability in this) {
        // Non-ILevelable abilities saved anyways
        arr[ability.Id] = (byte)ability.Level;
      }
      tag.Add("AbilityLevels", arr);
    }

    public void OldLoad(TagCompound tag) {
      if (!tag.ContainsKey("AbilityLevels")) {
        return;
      }
      oldAbility = tag.GetByteArray("AbilityLevels");
    }

    internal byte[] oldAbility; 
  }

}
