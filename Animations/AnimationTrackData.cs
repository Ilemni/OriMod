using System;
using System.Collections.Generic;

namespace OriMod.Animations {
  /// <summary>
  /// Stores all animations in the mod. Animation data is hardcoded here.
  /// </summary>
  internal sealed class AnimationTrackData : SingleInstance<AnimationTrackData> {
    /// <summary>
    /// Creates the singleton instance of <see cref="AnimationTrackData"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    private AnimationTrackData() {
      if (Terraria.Main.netMode == Terraria.ID.NetmodeID.Server) {
        throw new InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
    }

    private static Frame F(int x, int y, int duration = -1) => new Frame(x, y, duration);
    private static Header H(LoopMode l = LoopMode.Always, Direction d = Direction.Forward, ReferencedTexture2D texture = null) => new Header(loop: l, direction: d, rtx: texture);

    private AnimationSource _pa;
    public AnimationSource PlayerAnim => _pa ?? (_pa = new AnimationSource("PlayerEffects/OriPlayer", 64, 68,
      new Dictionary<string, Track> {
        ["Default"] = new Track(
          F(0, 0)
        ),
        ["Idle"] = new Track(asRange: true,
          F(0, 1, 9), F(0, 8, 9)
        ),
        ["IdleAgainst"] = new Track(asRange: true,
          F(0, 9, 7), F(0, 14, 7)
        ),
        ["LookUpStart"] = new Track(
          F(1, 0)
        ),
        ["LookUp"] = new Track(asRange: true,
          F(1, 1, 8), F(1, 7, 8)
        ),
        ["CrouchStart"] = new Track(
          F(1, 8)
        ),
        ["Crouch"] = new Track(
          F(1, 9)
        ),
        ["Running"] = new Track(asRange: true,
          F(2, 0, 4), F(2, 10, 4)
        ),
        ["Dash"] = new Track(asRange: false,
          F(2, 11, 36), F(2, 12, 12)
        ),
        ["Bash"] = new Track(asRange: false,
          F(2, 13, 40), F(2, 12)
        ),
        ["AirJump"] = new Track(
          F(3, 0, 32)
        ),
        ["Jump"] = new Track(H(d: Direction.Reverse), asRange: false,
          F(3, 1), F(3, 2, 14)
        ),
        ["IntoJumpBall"] = new Track(Header.None, asRange: false,
          F(3, 3, 6), F(3, 4, 4)
        ),
        ["ChargeJump"] = new Track(H(l: LoopMode.None, d: Direction.PingPong), asRange: true,
          F(3, 5, 4), F(3, 8, 4)
        ),
        ["Falling"] = new Track(asRange: true,
          F(3, 9, 4), F(3, 12, 4)
        ),
        ["FallNoAnim"] = new Track(
          F(3, 13)
        ),
        ["GlideStart"] = new Track(Header.None, asRange: true,
          F(4, 0, 5), F(4, 2, 5)
        ),
        ["GlideIdle"] = new Track(
          F(4, 3)
        ),
        ["Glide"] = new Track(asRange: true,
          F(4, 4, 5), F(4, 9, 5)
        ),
        ["ClimbIdle"] = new Track(
          F(5, 0)
        ),
        ["Climb"] = new Track(asRange: true,
          F(5, 1, 4), F(5, 8, 4)
        ),
        ["WallSlide"] = new Track(asRange: true,
          F(5, 9, 5), F(5, 12, 5)
        ),
        ["WallJump"] = new Track(
          F(5, 13, 12)
        ),
        ["WallChargeJumpCharge"] = new Track(asRange: false,
          F(6, 0, 16), F(6, 1, 10), F(6, 2)
        ),
        ["WallChargeJumpAim"] = new Track(asRange: true,
          F(6, 2), F(6, 6) // No duration, frames are selected by code, i.e. player mouse position
        ),
        ["Burrow"] = new Track(asRange: true,
          F(7, 0, 3), F(7, 7, 3)
        ),
        ["TransformStart"] = new Track(H(l: LoopMode.Transfer, texture: OriTextures.Instance.Transform), asRange: false, // TODO: Migrate TransformStart textures to OriPlayer
          F(0, 0, 2), F(0, 1, 60), F(0, 2, 60), F(0, 3, 120),
          F(0, 4, 40), F(0, 5, 40), F(0, 6, 40), F(0, 7, 30)
        ),
        ["TransformEnd"] = new Track(asRange: true,
          F(6, 7, 6), F(6, 8, 50), F(6, 9, 6), F(6, 10, 60),
          F(6, 11, 10), F(6, 12, 40), F(6, 13, 3), F(6, 14, 60)
        ),
      })
    );

    private AnimationSource _ba;
    public AnimationSource BashAnim => _ba ?? (_ba = new AnimationSource("PlayerEffects/BashArrow", 152, 20,
      new Dictionary<string, Track> {
        ["Bash"] = new Track(
          F(0, 0)
        )
      }
    ));

    private AnimationSource _ga;
    public AnimationSource GlideAnim => _ga ?? (_ga = new AnimationSource("PlayerEffects/Feather", 128, 128,
      new Dictionary<string, Track> {
        ["GlideStart"] = new Track(Header.None, asRange: true,
          F(0, 0, 5), F(0, 2, 5)
        ),
        ["GlideIdle"] = new Track(
          F(0, 3)
        ),
        ["Glide"] = new Track(asRange: true,
          F(0, 4, 5), F(0, 9, 5)
        ),
      }
    ));
  }
}
