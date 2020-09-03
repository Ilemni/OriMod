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
    /// <summary>
    /// Creates a new instance of <see cref="Animation"/> for the given <see cref="OriPlayer"/>, using the given <see cref="AnimationSource"/> and rendering with <see cref="PlayerLayer"/>.
    /// </summary>
    /// <param name="container"><see cref="PlayerAnimationData"/> instance this will belong to.</param>
    /// <param name="source"><see cref="AnimationSource"/> to determine which sprite is drawn.</param>
    /// <param name="playerLayer"><see cref="PlayerLayer"/></param>
    /// <exception cref="System.InvalidOperationException">Animation classes are not allowed to be constructed on a server.</exception>
    public Animation(PlayerAnimationData container, AnimationSource source, PlayerLayer playerLayer) {
      if (Terraria.Main.netMode == Terraria.ID.NetmodeID.Server) {
        throw new System.InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
      this.container = container;
      this.source = source;
      this.playerLayer = playerLayer;
      CheckIfValid(container.TrackName);
    }

    /// <summary>
    /// Current texture that is being used. Uses <see cref="Header.texture"/> if it is not null, otherwise <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Texture2D Texture => ActiveTrack.header.texture ?? source.texture;

    /// <summary>
    /// Current track that is being played.
    /// </summary>
    public Track ActiveTrack => Valid ? source.tracks[container.TrackName] : source.tracks.First().Value;
    
    /// <summary>
    /// Current frame that is being played.
    /// </summary>
    public Frame ActiveFrame => ActiveTrack.frames[container.FrameIndex < ActiveTrack.frames.Length ? container.FrameIndex : 0];

    /// <summary>
    /// Current tile's sprite position and size on the spritesheet.
    /// </summary>
    public Rectangle ActiveTile {
      get {
        var size = source.spriteSize;
        var tile = ActiveFrame.tile;
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
    public readonly PlayerAnimationData container;

    /// <summary>
    /// <see cref="AnimationSource"/> used for this <see cref="Animation"/>.
    /// </summary>
    public readonly AnimationSource source;

    /// <summary>
    /// Attempts to insert the <see cref="playerLayer"/> of this animation to the given <paramref name="layers"/>. This will fail and return false if the current track is not valid.
    /// </summary>
    /// <param name="layers">The <see cref="List{T}"/> of <see cref="PlayerLayer"/> to insert in.</param>
    /// <param name="idx">Position to insert into.</param>
    /// <returns><c>true</c> if <see cref="playerLayer"/> could be inserted, otherwise false.</returns>
    public bool TryInsertInLayers(List<PlayerLayer> layers, int idx = 0) {
      if (Valid) {
        layers.Insert(idx, playerLayer);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Checks if <paramref name="name"/> is a valid track.
    /// </summary>
    /// <param name="name">Track name to check.</param>
    public void CheckIfValid(string name) => Valid = source.tracks.ContainsKey(name);
  }
}
