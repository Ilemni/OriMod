using System.Diagnostics.CodeAnalysis;
using AnimLib.Abilities;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace OriMod {
  /// <summary>
  /// IDs for each <see cref="Ability"/>.
  /// </summary>
  public static class AbilityId {
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

    public WallJump WallJump { get; private set; }
    public AirJump AirJump { get; private set; }
    public Bash Bash { get; private set; }
    public Stomp Stomp { get; private set; }
    public Glide Glide { get; private set; }
    public Climb Climb { get; private set; }
    public ChargeJump ChargeJump { get; private set; }
    public WallChargeJump WallChargeJump { get; private set; }
    public Dash Dash { get; private set; }
    public ChargeDash ChargeDash { get; private set; }
    public LookUp LookUp { get; private set; }
    public Crouch Crouch { get; private set; }
    public Burrow Burrow { get; private set; }
    public Launch Launch { get; private set; }


    public override bool CanUseAnyAbilities() {
      if (Player.dead || oPlayer.Transforming ||
        Player.frozen || Player.stoned || Player.webbed || Player.shimmering) {
        return false;
      }
      bool mountActive = Player.mount?.Active ?? false;
      return !mountActive;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public OriPlayer oPlayer { get; private set; }

    public override void PhysicsPreUpdate() => oPlayer.PostUpdatePhysics();

    public override void Initialize() {
      WallJump = (WallJump)this[AbilityId.WallJump];
      AirJump = (AirJump)this[AbilityId.AirJump];
      Bash = (Bash)this[AbilityId.Bash];
      Stomp = (Stomp)this[AbilityId.Stomp];
      Glide = (Glide)this[AbilityId.Glide];
      Climb = (Climb)this[AbilityId.Climb];
      ChargeJump = (ChargeJump)this[AbilityId.ChargeJump];
      WallChargeJump = (WallChargeJump)this[AbilityId.WallChargeJump];
      Dash = (Dash)this[AbilityId.Dash];
      ChargeDash = (ChargeDash)this[AbilityId.ChargeDash];
      LookUp = (LookUp)this[AbilityId.LookUp];
      Crouch = (Crouch)this[AbilityId.Crouch];
      Burrow = (Burrow)this[AbilityId.Burrow];
      Launch = (Launch)this[AbilityId.Launch];
      oPlayer = Player.GetModPlayer<OriPlayer>();
    }

    internal void RefreshParticles(Color col) {
      int dustType = ModContent.DustType<AbilityRefreshedDust>();
      for (int i = 0; i < 10; i++) {
        Dust.NewDust(Player.Center, 12, 12, dustType, newColor: col);
      }
    }

    //Backward compatibility don't pay attention
    public void OldSave(TagCompound tag) {
      byte[] arr = new byte[AbilityId.Count];
      foreach (Ability ability in this) {
        // Non-ILevelable abilities saved anyways
        arr[ability.Id] = (byte)ability.Level;
      }
      tag.Add("AbilityLevels", arr);
    }

    //Backward compatibility don't pay attention
    public void OldLoad(TagCompound tag) {
      if (!tag.ContainsKey("AbilityLevels")) {
        OldAbility = null;
        return;
      }
      OldAbility = tag.GetByteArray("AbilityLevels");
    }

    //Backward compatibility don't pay attention
    internal byte[] OldAbility;
  }

}
