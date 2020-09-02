namespace OriMod.Animations {
  /// <summary>
  /// Container for various <see cref="Animation"/>s to be attached to an <see cref="OriPlayer"/>.
  /// </summary>
  public class AnimationContainer {
    /// <summary>
    /// Creates a new instance of <see cref="AnimationContainer"/> for the given <see cref="OriPlayer"/>.
    /// </summary>
    /// <param name="oPlayer"><see cref="OriPlayer"/> instance the animations will belong to.</param>
    /// <exception cref="System.InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    internal AnimationContainer(OriPlayer oPlayer) {
      if (Terraria.Main.netMode == Terraria.ID.NetmodeID.Server) {
        throw new System.InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
      PlayerAnim = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.PlayerSprite);
      BashAnim = new Animation(oPlayer, AnimationHandler.Instance.BashAnim, OriLayers.Instance.BashArrow);
      GlideAnim = new Animation(oPlayer, AnimationHandler.Instance.GlideAnim, OriLayers.Instance.FeatherSprite);
    }

    /// <summary>
    /// Animation for the player sprite.
    /// </summary>
    public readonly Animation PlayerAnim;
    
    /// <summary>
    /// Animation for the Bash arrow sprite.
    /// </summary>
    public readonly Animation BashAnim;

    /// <summary>
    /// Animation for the Glide feather sprite.
    /// </summary>
    public readonly Animation GlideAnim;
  }
}
