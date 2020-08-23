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
    /// Current texture that is being used. Uses <see cref="Header.texture"/> if it is not null, otherwise <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Texture2D Texture => ActiveTrack.header.texture ?? source.texture;

    /// <summary>
    /// Current track that is being played.
    /// </summary>
    public Track ActiveTrack => Valid ? source.tracks[oPlayer.AnimationName] : source.tracks.First().Value;
    
    /// <summary>
    /// Current frame that is being played.
    /// </summary>
    public Frame ActiveFrame => ActiveTrack.frames[oPlayer.AnimationIndex < ActiveTrack.frames.Length ? oPlayer.AnimationIndex : 0];

    /// <summary>
    /// Current tile's sprite position and size on the spritesheet.
    /// </summary>
    public Rectangle ActiveTile {
      get {
        var size = source.spriteSize;
        var tile = ActiveFrame.Tile;
        return new Rectangle(tile.X * size.X, tile.Y * size.Y, size.X, size.Y);
      }
    }

    /// <summary>
    /// Whether or not the current animation name is valid.
    /// </summary>
    public bool Valid { get; private set; }

    /// <summary>
    /// <see cref="PlayerLayer"/> used for this <see cref="Animation"/>.
    /// </summary>
    public readonly PlayerLayer playerLayer;

    /// <summary>
    /// <see cref="OriPlayer"/> this <see cref="Animation"/> belongs to.
    /// </summary>
    public readonly OriPlayer oPlayer;

    /// <summary>
    /// <see cref="AnimationSource"/> used for this <see cref="Animation"/>.
    /// </summary>
    public readonly AnimationSource source;

    /// <summary>
    /// Inserts the <see cref="playerLayer"/> of this animation to the given <paramref name="layers"/>.
    /// </summary>
    /// <param name="layers">The <see cref="List{T}"/> of <see cref="PlayerLayer"/> to insert in.</param>
    /// <param name="idx">Position to insert into.</param>
    /// <param name="force">Add even if <see cref="Valid"/> is <c>false</c>.</param>
    public void InsertInLayers(List<PlayerLayer> layers, int idx = 0, bool force = false) {
      if (Valid || force) {
        layers.Insert(idx, playerLayer);
      }
    }

    /// <summary>
    /// Add the <see cref="playerLayer"/> of this animation to the given <paramref name="layers"/>.
    /// <para>This will not run if Valid is false, unless force is true.</para>
    /// </summary>
    /// <param name="layers">The PlayerLayer list to add to.</param>
    /// <param name="force">Add even if <see cref="Valid"/> is <c>false</c>.</param>
    public void AddToLayers(List<PlayerLayer> layers, bool force = false) {
      if (Valid || force) {
        layers.Add(playerLayer);
      }
    }

    /// <summary>
    /// Checks if <paramref name="name"/> is a valid track.
    /// </summary>
    /// <param name="name">Track name to check.</param>
    public void CheckIfValid(string name) => Valid = source.tracks.ContainsKey(name);
  }
}
