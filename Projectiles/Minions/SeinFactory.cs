using OriMod.Buffs;
using Terraria.ModLoader;

namespace OriMod.Projectiles.Minions {
  public sealed class Sein1 : Sein {
    protected override int Init => 1;
    protected override int BuffType() => ModContent.BuffType<SeinBuff1>();
  }

  public sealed class Sein2 : Sein {
    protected override int Init => 2;
    protected override int BuffType() => ModContent.BuffType<SeinBuff2>();
  }

  public sealed class Sein3 : Sein {
    protected override int Init => 3;
    protected override int BuffType() => ModContent.BuffType<SeinBuff3>();
  }

  public sealed class Sein4 : Sein {
    protected override int Init => 4;
    protected override int BuffType() => ModContent.BuffType<SeinBuff4>();
  }

  public sealed class Sein5 : Sein {
    protected override int Init => 5;
    protected override int BuffType() => ModContent.BuffType<SeinBuff5>();
  }

  public sealed class Sein6 : Sein {
    protected override int Init => 6;
    protected override int BuffType() => ModContent.BuffType<SeinBuff6>();
  }

  public sealed class Sein7 : Sein {
    protected override int Init => 7;
    protected override int BuffType() => ModContent.BuffType<SeinBuff7>();
  }

  public sealed class Sein8 : Sein {
    protected override int Init => 8;
    protected override int BuffType() => ModContent.BuffType<SeinBuff8>();
  }
}
