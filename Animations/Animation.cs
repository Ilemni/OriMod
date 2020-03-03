using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace OriMod.Animations {
  internal class Animation {
    internal Texture2D Texture => ActiveTrack.Header.Texture ?? Source.Texture;
    internal readonly PlayerLayer PlayerLayer;
    internal bool Valid { get; private set; }
    internal Track ActiveTrack => Valid ? Source.Tracks[Handler.oPlayer.AnimName] : Source.Tracks.First().Value;
    internal Frame ActiveFrame => ActiveTrack.Frames[Handler.oPlayer.AnimIndex < ActiveTrack.Frames.Length ? Handler.oPlayer.AnimIndex : 0];
    internal Rectangle ActiveTile => new Rectangle(ActiveFrame.Tile.X * Source.TileSize.X, ActiveFrame.Tile.Y * Source.TileSize.Y, Source.TileSize.X, Source.TileSize.Y);
    internal readonly AnimationContainer Handler;
    internal readonly AnimationSource Source;

    internal Animation(AnimationContainer handler, AnimationSource source, PlayerLayer playerLayer) {
      Handler = handler;
      Source = source;
      PlayerLayer = playerLayer;
    }

    internal void InsertInLayers(List<PlayerLayer> layers, int idx = 0, bool force = false) {
      if (Valid || force) {
        layers.Insert(idx, PlayerLayer);
      }
    }
    /// <summary> Add the PlayerLayer of this animation to the given `layers`
    /// 
    /// This will not run if Valid is false, unless force is true </summary>
    /// <param name="layers">The PlayerLayer list to add to</param>
    /// <param name="force">Add this Player even if Valid is false </param>
    internal void AddToLayers(List<PlayerLayer> layers, bool force = false) {
      if (Valid || force) {
        layers.Add(PlayerLayer);
      }
    }

    internal void OnAnimNameChange(string name) => Valid = Source.Tracks.ContainsKey(name);
  }
}
