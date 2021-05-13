using System;
using Microsoft.Xna.Framework;
using OriMod.Dusts;
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
    public override int Id => AbilityId.SoulLink;
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

    private Point Center => _placedSoulLink && Box.Points[4] != Point.Zero ? Box.Points[4] : player.Center.ToTileCoordinates();
    private Point SoulLinkLocation { get; set; }

    private static float ChargeRate => 0.2f;
    private static float UnchargeRate => ChargeRate * 1.75f;
    private static int RespawnTime => 60;
    private static int ManaCost => 20;

    private float _currentCharge;
    private bool _placedSoulLink;
    private bool _obstructed;
    private bool _wasObstructed;
    private bool _anyBossAlive;
    private bool _wasDead;

    private void CheckValidPlacement(Point? check, out bool obstructed, bool force = false) {
      obstructed = false;
      if (!force && Main.time % 20 != 0) {
        return;
      }

      Box.UpdateHitbox(check ?? Center);
      var points = Box.Points;
      for (int i = 0, len = points.Length; i < len; i++) {
        Tile t = Main.tile[points[i].X, points[i].Y];
        if (!Burrow.IsSolid(t)) continue;
        obstructed = true;
        return;
      }
    }

    internal void UpdateDead() {
      if (!_placedSoulLink) {
        return;
      }

      if (player.respawnTimer > RespawnTime) {
        player.respawnTimer = RespawnTime;
      }
      _wasDead = true;
    }

    internal void OnRespawn() {
      if (!_placedSoulLink || _obstructed) return;
      player.Center = SoulLinkLocation.ToWorldCoordinates();
      _placedSoulLink = false;
    }

    internal override void Tick() {
      if (_placedSoulLink && _wasDead && !player.dead) {
        _wasDead = false;
        OnRespawn();
      }

      if (_placedSoulLink) {
        CheckValidPlacement(Center, out _obstructed);
        if (_obstructed) {
          _placedSoulLink = false;
          OriMod.Error("SoulLinkObstructed", log: false);
        }
      }
      if (CanUse && OriMod.soulLinkKey.Current) {
        if (OriMod.soulLinkKey.JustPressed) {
          _anyBossAlive = OriUtils.IsAnyBossAlive();
          if (_anyBossAlive) {
            OriMod.Error("SoulLinkBossActive", log: false);
            return;
          }
        }
        CheckValidPlacement(player.Center.ToTileCoordinates(), out bool tempObstructed, force: true);
        if (tempObstructed) {
          if (!_wasObstructed) {
            OriMod.Error("SoulLinkCannotPlace", log: false);
          }
          _currentCharge -= UnchargeRate;
          if (_currentCharge < 0) {
            _currentCharge = 0;
          }
        }
        else {
          _currentCharge += ChargeRate;
          if (_currentCharge > 1) {
            player.statMana -= ManaCost;
            _currentCharge = 0;
            SetState(State.Active);
            _placedSoulLink = true;
            Box.UpdateHitbox(player.Center);
            SoulLinkLocation = Center;
            oPlayer.Debug("Placed a Soul Link!");
            PutOnCooldown(force: true);
          }
        }
        _wasObstructed = tempObstructed;
      }
      else if (_currentCharge > 0) {
        _currentCharge -= UnchargeRate;
        if (_currentCharge < 0) {
          _currentCharge = 0;
        }
      }
      if (_currentCharge > 0 && _currentCharge < 1) {
        Dust dust = Dust.NewDustDirect(player.Center, 12, 12, ModContent.DustType<SoulLinkChargeDust>(), newColor: Color.DeepSkyBlue);
        dust.customData = player;
        dust.position += -Vector2.UnitY.RotatedBy(_currentCharge * 2 * Math.PI) * 56;
      }
      TickCooldown();
    }

    internal static void Unload() {
      _b = null;
    }
  }
}
