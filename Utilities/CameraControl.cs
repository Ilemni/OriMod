using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Utilities {
  internal class CameraControl : ModSystem {
    internal float camera_v_offset;
    private static CameraControl _inst;
    public static CameraControl instance => 
      _inst ??= ModContent.GetInstance<CameraControl>();
    public override void Load() {
      camera_v_offset = 0.0f;
    }
    public override void ModifyScreenPosition() {
      Main.screenPosition += new Vector2(0.0f, camera_v_offset);
    }
  }
}
