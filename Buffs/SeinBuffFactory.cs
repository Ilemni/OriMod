using OriMod.Projectiles.Minions;
using Terraria.ModLoader;

namespace OriMod.Buffs; 

public sealed class SeinBuff1 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein1>();
}

public sealed class SeinBuff2 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein2>();
}

public sealed class SeinBuff3 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein3>();
}

public sealed class SeinBuff4 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein4>();
}

public sealed class SeinBuff5 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein5>();
}

public sealed class SeinBuff6 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein6>();
}

public sealed class SeinBuff7 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein7>();
}

public sealed class SeinBuff8 : SeinBuff {
  protected override int ProjectileType() => ModContent.ProjectileType<Sein8>();
}
