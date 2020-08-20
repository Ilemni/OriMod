namespace OriMod.Animations {
  public class AnimationContainer {
    internal AnimationContainer(OriPlayer oPlayer) {
      PlayerAnim = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.PlayerSprite);
      SecondaryLayer = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.SecondaryLayer);
      TrailAnim = new Animation(oPlayer, AnimationHandler.Instance.PlayerAnim, OriLayers.Instance.Trail);
      BashAnim = new Animation(oPlayer, AnimationHandler.Instance.BashAnim, OriLayers.Instance.BashArrow);
      GlideAnim = new Animation(oPlayer, AnimationHandler.Instance.GlideAnim, OriLayers.Instance.FeatherSprite);
    }

    public readonly Animation PlayerAnim;
    public readonly Animation SecondaryLayer;
    public readonly Animation TrailAnim;
    public readonly Animation BashAnim;
    public readonly Animation GlideAnim;
  }
}
