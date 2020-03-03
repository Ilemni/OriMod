namespace OriMod.Animations {
  internal class AnimationContainer {
    internal AnimationContainer(OriPlayer oPlayer) {
      this.oPlayer = oPlayer;
      PlayerAnim = new Animation(this, AnimationHandler.PlayerAnim, OriLayers.Instance.PlayerSprite);
      SecondaryLayer = new Animation(this, AnimationHandler.PlayerAnim, OriLayers.Instance.SecondaryLayer);
      TrailAnim = new Animation(this, AnimationHandler.PlayerAnim, OriLayers.Instance.Trail);
      BashAnim = new Animation(this, AnimationHandler.BashAnim, OriLayers.Instance.BashArrow);
      GlideAnim = new Animation(this, AnimationHandler.GlideAnim, OriLayers.Instance.FeatherSprite);
    }

    internal readonly OriPlayer oPlayer;
    
    internal readonly Animation PlayerAnim;
    internal readonly Animation SecondaryLayer;
    internal readonly Animation TrailAnim;
    internal readonly Animation BashAnim;
    internal readonly Animation GlideAnim;
  }
}
