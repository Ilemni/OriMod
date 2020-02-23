﻿namespace OriMod.Animations {
  internal class AnimationContainer {
    internal OriPlayer oPlayer { get; private set; }
    internal Animation PlayerAnim { get; private set; }
    internal Animation SecondaryLayer { get; private set; }
    internal Animation TrailAnim { get; private set; }
    internal Animation BashAnim { get; private set; }
    internal Animation GlideAnim { get; private set; }
    internal AnimationContainer(OriPlayer oPlayer) {
      this.oPlayer = oPlayer;
      PlayerAnim = new Animation(this, AnimationHandler.PlayerAnim, OriLayers.PlayerSprite);
      SecondaryLayer = new Animation(this, AnimationHandler.PlayerAnim, OriLayers.SecondaryLayer);
      TrailAnim = new Animation(this, AnimationHandler.PlayerAnim, OriLayers.Trail);
      BashAnim = new Animation(this, AnimationHandler.BashAnim, OriLayers.BashArrow);
      GlideAnim = new Animation(this, AnimationHandler.GlideAnim, OriLayers.FeatherSprite);
    }

    internal void Dispose() {
      oPlayer = null;
      PlayerAnim.Dispose();
      SecondaryLayer.Dispose();
      TrailAnim.Dispose();
      BashAnim.Dispose();
      GlideAnim.Dispose();
      PlayerAnim = SecondaryLayer = TrailAnim = BashAnim = GlideAnim = null;
    }
  }
}
