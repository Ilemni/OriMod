using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace OriMod.Animations {
  /// <summary>
  /// Animation for a single player.
  /// </summary>
  public class Animation {
    public Animation(OriPlayer oPlayer, AnimationSource source, PlayerLayer playerLayer) {
      this.oPlayer = oPlayer;
      this.source = source;
      this.playerLayer = playerLayer;
    }

    /// <summary>
    /// Current texture to use. Is the current <see cref="Header"/>'s texture if it is not null.
    /// </summary>
    public Texture2D Texture => ActiveTrack.Header.Texture ?? source.texture;

    /// <summary>
    /// Current track that is being played.
    /// </summary>
    public Track ActiveTrack => Valid ? source.tracks[oPlayer.AnimName] : source.tracks.First().Value;
    
    /// <summary>
    /// Current frame to play.
    /// </summary>
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

    /// <summary>
    /// Inserts the <see cref="playerLayer"/> of this animation to the given <paramref name="layers"/>
    /// </summary>
    /// <param name="layers">The <see cref="List{T}"/> of <see cref="Terraria.ModLoader.PlayerLayer"/> to insert in</param>
    /// <param name="idx">Position to insert into</param>
    /// <param name="force">Add even if <see cref="Valid"/> is <c>false</c></param>
    public void InsertInLayers(List<PlayerLayer> layers, int idx = 0, bool force = false) {
      if (Valid || force) {
        layers.Insert(idx, playerLayer);
      }
    }

    /// <summary>
    /// Add the <see cref="playerLayer"/> of this animation to the given <paramref name="layers"/>
    /// <para>This will not run if Valid is false, unless force is true</para>
    /// </summary>
    /// <param name="layers">The PlayerLayer list to add to</param>
    /// <param name="force">Add even if <see cref="Valid"/> is <c>false</c></param>
    public void AddToLayers(List<PlayerLayer> layers, bool force = false) {
      if (Valid || force) {
        layers.Add(playerLayer);
      }
    }

    public void CheckIfValid(string name) => Valid = source.tracks.ContainsKey(name);
  }
}
