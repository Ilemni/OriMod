using System;
using Microsoft.Xna.Framework;
using OriMod.Utilities;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  /// <summary>
  /// Ability to allow respawning wherever a player wants. No longer being developed.
  /// </summary>
  [Obsolete]
  public sealed class SoulLink : Ability {
    internal SoulLink(AbilityManager manager) : base(manager) { }
    public override int Id => AbilityID.SoulLink;
    public override byte Level => 0;

    internal override bool CanUse => base.CanUse && oPlayer.IsGrounded && player.CheckMana(ManaCost, blockQuickMana: true);
    protected override int Cooldown => 900;

    private static Point P(int x, int y) => new Point(x, y);
    private static TileHitbox Box => _b ?? (_b = new TileHitbox(
      P(-1, -1), P(0, -1), P(1, -1),
      P(0, -1), P(0, 0), P(0, 1),
      P(1, -1), P(1, 0), P(1, 1)
    ));
    private static TileHitbox _b;

    internal Point Center => placedSoulLink && Box.Points[4] != Point.Zero ? Box.Points[4] : player.Center.ToTileCoordinates();
    internal Point SoulLinkLocation { get; private set; }

    private static float ChargeRate => 0.2f;
    private static float UnchargeRate => ChargeRate * 1.75f;
    private static int RespawnTime => 60;
    private static int ManaCost => 20;

    private float currentCharge;
    internal bool placedSoulLink;
    internal bool obstructed;
    private bool wasObstructed;
    private bool anyBossAlive;
    private bool wasDead;

    private void CheckValidPlacement(Point? check, out bool obstructed, bool force = false) {
      obstructed = false;
      if (!force && Main.time % 20 != 0) {
        return;
      }

      Box.UpdateHitbox(check ?? Center);
      var points = Box.Points;
      for (int i = 0, len = points.Length; i < len; i++) {
        Tile t = Main.tile[points[i].X, points[i].Y];
        if (Burrow.IsSolid(t)) {
          obstructed = true;
          return;
        }
      }
      return;
    }

    internal void UpdateDead() {
      if (!placedSoulLink) {
        return;
      }

      if (player.respawnTimer > RespawnTime) {
        player.respawnTimer = RespawnTime;
      }
      wasDead = true;
    }

    internal void OnRespawn() {
      if (placedSoulLink && !obstructed) {
        player.Center = SoulLinkLocation.ToWorldCoordinates();
        placedSoulLink = false;
      }
    }

    internal override void Tick() {
      if (placedSoulLink && wasDead && !player.dead) {
        wasDead = false;
        OnRespawn();
      }

      if (placedSoulLink) {
        CheckValidPlacement(Center, out obstructed);
        if (obstructed) {
          placedSoulLink = false;
          OriMod.Error("SoulLinkObstructed", log: false);
        }
      }
      if (CanUse && OriMod.SoulLinkKey.Current) {
        if (OriMod.SoulLinkKey.JustPressed) {
          anyBossAlive = OriUtils.AnyBossAlive();
          if (anyBossAlive) {
            OriMod.Error("SoulLinkBossActive", log: false);
            return;
          }
        }
        CheckValidPlacement(player.Center.ToTileCoordinates(), out bool tempObstructed, force: true);
        if (tempObstructed) {
          if (!wasObstructed) {
            OriMod.Error("SoulLinkCannotPlace", log: false);
          }
          currentCharge -= UnchargeRate;
          if (currentCharge < 0) {
            currentCharge = 0;
          }
        }
        else {
          currentCharge += ChargeRate;
          if (currentCharge > 1) {
            player.statMana -= ManaCost;
            currentCharge = 0;
            SetState(State.Active);
            placedSoulLink = true;
            Box.UpdateHitbox(player.Center);
            SoulLinkLocation = Center;
            oPlayer.Debug("Placed a Soul Link!");
            PutOnCooldown(force: true);
          }
        }
        wasObstructed = tempObstructed;
      }
      else if (currentCharge > 0) {
        currentCharge -= UnchargeRate;
        if (currentCharge < 0) {
          currentCharge = 0;
        }
      }
      if (currentCharge > 0 && currentCharge < 1) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, ModContent.DustType<Dusts.SoulLinkChargeDust>(), newColor: Color.DeepSkyBlue)];
        dust.customData = player;
        dust.position += -Vector2.UnitY.RotatedBy(currentCharge * 2 * Math.PI) * 56;
      }
      TickCooldown();
    }

    internal static void Unload() {
      _b = null;
    }
  }
}
