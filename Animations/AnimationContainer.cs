namespace OriMod.Animations {
  /// <summary>
  /// Container for various <see cref="Animation"/>s to be attached to an <see cref="OriPlayer"/>.
  /// </summary>
  public class AnimationContainer {
    internal AnimationContainer(OriPlayer oPlayer) {
      PlayerAnim = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.PlayerSprite);
      SecondaryLayer = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.SecondaryLayer);
      TrailAnim = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.Trail);
      BashAnim = new Animation(oPlayer, AnimationHandler.Instance.BashAnim, OriLayers.Instance.BashArrow);
      GlideAnim = new Animation(oPlayer, AnimationHandler.Instance.GlideAnim, OriLayers.Instance.FeatherSprite);
    }

    /// <summary>
    /// Animation for the player sprite.
    /// </summary>
    public readonly Animation PlayerAnim;
    public readonly Animation SecondaryLayer; // TODO: remove
    public readonly Animation TrailAnim; // TODO: remove
    
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
