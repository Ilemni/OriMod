using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace OriMod.Animations {
  public class Animation {
    public Animation(OriPlayer oPlayer, AnimationSource source, PlayerLayer playerLayer) {
      this.oPlayer = oPlayer;
      this.source = source;
      this.playerLayer = playerLayer;
    }

    public Texture2D Texture => ActiveTrack.Header.Texture ?? source.texture;
    public Track ActiveTrack => Valid ? source.tracks[oPlayer.AnimName] : source.tracks.First().Value;
    public Frame ActiveFrame => ActiveTrack.Frames[oPlayer.AnimIndex < ActiveTrack.Frames.Length ? oPlayer.AnimIndex : 0];

    public Rectangle ActiveTile {
      get {
        var size = source.spriteSize;
        var tile = ActiveFrame.Tile;
        return new Rectangle(tile.X * size.X, tile.Y * size.Y, size.X, size.Y);
      }
    }

    public bool Valid { get; private set; }
    public readonly PlayerLayer playerLayer;
    public readonly OriPlayer oPlayer;
    public readonly AnimationSource source;

    public void InsertInLayers(List<PlayerLayer> layers, int idx = 0, bool force = false) {
      if (Valid || force) {
        layers.Insert(idx, playerLayer);
      }
    }

    /// <param name="layers">The PlayerLayer list to add to</param>
    public void AddToLayers(List<PlayerLayer> layers, bool force = false) {
      if (Valid || force) {
        layers.Add(playerLayer);
      }
    }

    public void CheckIfValid(string name) => Valid = source.tracks.ContainsKey(name);
  }
}
