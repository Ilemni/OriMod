using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod.Animations {
  /// <summary>
  /// Animation for a single player. This class uses runtime data from a <see cref="PlayerAnimationData"/> to retrieve values from an <see cref="AnimationSource"/>.
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
      if (Main.netMode == NetmodeID.Server) {
        throw new System.InvalidOperationException($"Animation classes are not allowed to be constructed on servers.");
      }
      this.container = container;
      this.source = source;
      this.playerLayer = playerLayer;
      CheckIfValid(container.TrackName);
    }

    /// <summary>
    /// Current texture that is being used. Uses the result of <see cref="Track.GetTexture(int)"/> if it is not <see langword="null"/>, otherwise <see cref="AnimationSource.texture"/>.
    /// </summary>
    public Texture2D Texture => ActiveTrack.GetTexture(container.FrameIndex) ?? source.texture;

    /// <summary>
    /// Current track that is being played.
    /// </summary>
    public Track ActiveTrack => Valid ? source.tracks[container.TrackName] : source.tracks.First().Value;
    
    /// <summary>
    /// Current frame that is being played.
    /// </summary>
    public IFrame ActiveFrame => ActiveTrack.frames[container.FrameIndex < ActiveTrack.frames.Length ? container.FrameIndex : 0];

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
    /// <returns><see langword="true"/> if <see cref="playerLayer"/> was inserted, otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Gets a <see cref="DrawData"/> that is based on this <see cref="Animation"/>.
    /// <list type="bullet">
    /// <item><see cref="DrawData.texture"/> is <see cref="Texture"/> (recommended)</item>
    /// <item><see cref="DrawData.position"/> is the center of the <see cref="PlayerDrawInfo.drawPlayer"/>, in screen-space. (recommended)</item>
    /// <item><see cref="DrawData.sourceRect"/> is <see cref="ActiveTile"/> (recommended)</item>
    /// <item><see cref="DrawData.rotation"/> is <see cref="Entity.direction"/> <see langword="*"/> <see cref="PlayerAnimationData.SpriteRotation"/> (recommended)</item>
    /// <item><see cref="DrawData.origin"/> is half of <see cref="ActiveTile"/>'s size, plus (5 * <see cref="Player.gravDir"/>) on the Y axis. Feel free to modify this.</item>
    /// <item><see cref="DrawData.effect"/> is based on <see cref="Entity.direction"/> and <see cref="Player.gravDir"/>. (recommended)</item>
    /// </list>
    /// </summary>
    /// <param name="drawInfo">Parameter of <see cref="PlayerLayer(string, string, System.Action{PlayerDrawInfo})"/>.</param>
    /// <returns></returns>
    public DrawData GetDrawData(PlayerDrawInfo drawInfo) {
      Player player = drawInfo.drawPlayer;
      Texture2D texture = Texture;
      Vector2 pos = drawInfo.position - Main.screenPosition + player.Size / 2;
      Rectangle rect = ActiveTile;
      var orig = new Vector2(rect.Width / 2, rect.Height / 2 + 5 * player.gravDir);
      SpriteEffects effect = SpriteEffects.None;
      if (player.direction == -1) {
        effect |= SpriteEffects.FlipHorizontally;
      }

      if (player.gravDir == -1) {
        effect |= SpriteEffects.FlipVertically;
      }

      return new DrawData(texture, pos, rect, Color.White, player.direction * container.SpriteRotation, orig, 1, effect, 0);
    }
  }
}
