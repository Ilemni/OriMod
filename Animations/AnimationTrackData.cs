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

    public static byte SpriteWidth => 64;
    public static byte SpriteHeight = 68;

    private AnimationSource _pa;
    public AnimationSource PlayerAnim => _pa ?? (_pa = new AnimationSource("PlayerEffects/OriPlayer", SpriteWidth, SpriteHeight,
      new Dictionary<string, Track> {
        ["Default"] = Track.Single(
          F(0, 0)
        ),
        ["Idle"] = Track.Range(
          F(0, 1, 9), F(0, 8, 9)
        ),
        ["IdleAgainst"] = Track.Range(
          F(0, 9, 7), F(0, 14, 7)
        ),
        ["LookUpStart"] = Track.Single(
          F(1, 0)
        ),
        ["LookUp"] = Track.Range(
          F(1, 1, 8), F(1, 7, 8)
        ),
        ["CrouchStart"] = Track.Single(
          F(1, 8)
        ),
        ["Crouch"] = Track.Single(
          F(1, 9)
        ),
        ["Running"] = Track.Range(
          F(2, 0, 4), F(2, 10, 4)
        ),
        ["Dash"] = Track.Range(
          F(2, 11, 36), F(2, 12, 12)
        ),
        ["Bash"] = Track.Range(
          F(2, 13, 40), F(2, 12)
        ),
        ["AirJump"] = Track.Single(
          F(3, 0, 32)
        ),
        ["Jump"] = new Track(new IFrame[] {
          F(3, 2, 14), F(3, 1)
        }),
        ["IntoJumpBall"] = Track.Range(LoopMode.None,
          F(3, 3, 6), F(3, 4, 4)
        ),
        ["ChargeJump"] = Track.Range(LoopMode.None, Direction.PingPong,
          F(3, 5, 4), F(3, 8, 4)
        ),
        ["Falling"] = Track.Range(
          F(3, 9, 4), F(3, 12, 4)
        ),
        ["FallNoAnim"] = Track.Single(
          F(3, 13)
        ),
        ["GlideStart"] = Track.Range(LoopMode.None,
          F(4, 0, 5), F(4, 2, 5)
        ),
        ["GlideIdle"] = Track.Single(
          F(4, 3)
        ),
        ["Glide"] = Track.Range(
          F(4, 4, 5), F(4, 9, 5)
        ),
        ["ClimbIdle"] = Track.Single(
          F(5, 0)
        ),
        ["Climb"] = Track.Range(
          F(5, 1, 4), F(5, 8, 4)
        ),
        ["WallSlide"] = Track.Range(
          F(5, 9, 5), F(5, 12, 5)
        ),
        ["WallJump"] = Track.Single(
          F(5, 13, 12)
        ),
        ["WallChargeJumpCharge"] = new Track(new IFrame[] {
          F(6, 0, 16), F(6, 1, 10), F(6, 2)
        }),
        ["WallChargeJumpAim"] = Track.Range(
          F(6, 2), F(6, 6) // No duration, frames are selected by code, i.e. player mouse position
        ),
        ["Burrow"] = Track.Range(
          F(7, 0, 3), F(7, 7, 3)
        ),
        ["TransformStart"] = new Track(LoopMode.Transfer, new IFrame[] { // TODO: Migrate TransformStart textures to OriPlayer
          F(0, 0, 2), F(0, 1, 60), F(0, 2, 60), F(0, 3, 120),
          F(0, 4, 40), F(0, 5, 40), F(0, 6, 40), F(0, 7, 30)
        }).WithTexture(OriTextures.Instance.Transform),
        ["TransformEnd"] = new Track(new IFrame[] {
          F(6, 7, 6), F(6, 8, 50), F(6, 9, 6), F(6, 10, 60),
          F(6, 11, 10), F(6, 12, 40), F(6, 13, 3), F(6, 14, 60)
        }),
      })
    );

    private AnimationSource _ba;
    public AnimationSource BashAnim => _ba ?? (_ba = new AnimationSource("PlayerEffects/BashArrow", 152, 20,
      new Dictionary<string, Track> {
        ["Bash"] = Track.Single(
          F(0, 0)
        )
      }
    ));

    private AnimationSource _ga;
    public AnimationSource GlideAnim => _ga ?? (_ga = new AnimationSource("PlayerEffects/Feather", 128, 128,
      new Dictionary<string, Track> {
        ["GlideStart"] = Track.Range(LoopMode.None,
          F(0, 0, 5), F(0, 2, 5)
        ),
        ["GlideIdle"] = Track.Single(
          F(0, 3)
        ),
        ["Glide"] = Track.Range(
          F(0, 4, 5), F(0, 9, 5)
        ),
      }
    ));
  }
}
