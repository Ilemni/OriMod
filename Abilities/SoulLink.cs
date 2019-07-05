using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Abilities {
  public class SoulLink : Ability {
    public SoulLink(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    internal override bool DoUpdate => OriMod.SoulLinkKey.Current || InUse;
    internal override bool CanUse => base.CanUse && !AnyBossAlive && player.CheckMana(ManaCost, blockQuickMana:true) && oPlayer.IsGrounded;
    protected override int Cooldown => 1800;

    private static Point p(int x, int y) => new Point(x, y);
    private static readonly Point[] Template = new Point[] {
      p(-1, -1), p(0, -1), p(1, -1),
      p(0, -1),  p(0, 0),  p(0, 1),
      p(1, -1),  p(1, 0),  p(1, 1)
    };
    internal Point[] Box = new Point[Template.Length];
    internal Point Center => PlacedSoulLink && Box[4] != Point.Zero ? Box[4] : player.Center.ToTileCoordinates();
    
    private float ChargeMax => 1;
    private float ChargeRate => 0.018f;
    private float UnchargeRate => 0.032f;
    private int RespawnTime => 80;
    private int ManaCost => 20;

    private float CurrCharge = 0;
    internal bool PlacedSoulLink = false;
    internal bool Obstructed = false;
    private bool AnyBossAlive = false;
    private bool WasDead = false;

    private void CheckValidPlacement(bool force=false) {
      if (force || PlacedSoulLink) {
        if (!force && Main.time % 20 != 0) return;
        for (int i = 0; i < Box.Length; i++) {
          Tile t = Main.tile[Box[i].X, Box[i].Y];
          if (Burrow.IsSolid(t)) {
            Obstructed = true;
            return;
          }
        }
        Obstructed = false;
        return;
      }
    }
    private void UpdateBox() {
      Box.UpdateHitbox(Template, player.Center.ToTileCoordinates());
    }
    

    internal void UpdateDead() {
      if (!PlacedSoulLink) return;
      if (player.respawnTimer > RespawnTime) {
        player.respawnTimer = RespawnTime;
      }
      WasDead = true;
    }
    internal void OnRespawn() {
      if (PlacedSoulLink && !Obstructed) {
        oPlayer.Debug("Soul link respawn!");
        player.Center = Center.ToWorldCoordinates();
        PlacedSoulLink = false;
      }
    }
    protected override bool PreUpdate() {
      if (PlacedSoulLink && WasDead && !player.dead) {
        WasDead = false;
        OnRespawn();
      }
      return base.PreUpdate();
    }

    internal override void Tick() {
      if (PlacedSoulLink) {
        CheckValidPlacement();
        if (Obstructed) {
          PlacedSoulLink = false;
          Main.NewText("Your Soul Link has been blocked and despawned...");
        }
      }
      if (CanUse && OriMod.SoulLinkKey.Current) {
        if (OriMod.SoulLinkKey.JustPressed) {
          AnyBossAlive = OriModUtils.IsAnyBossAlive;
          if (AnyBossAlive) {
          Main.NewText("Cannot place a Soul Link while powerful enemies are around");
            return;
          }
        }
        bool WasObstructed = Obstructed;
        UpdateBox();
        CheckValidPlacement(force:true);
        if (Obstructed) {
          if (!WasObstructed) {
          Main.NewText("Cannot place a Soul Link here...");
          }
          CurrCharge -= UnchargeRate;
          if (CurrCharge < 0) CurrCharge = 0;
        }
        else {
          CurrCharge += ChargeRate;
          if (CurrCharge > ChargeMax) {
            player.statMana -= ManaCost;
            CurrCharge = 0;
            Active = true;
            PlacedSoulLink = true;
            oPlayer.Debug("Placed a Soul Link!");
            PutOnCooldown(force:true);
          }
        }
      }
      else if (CurrCharge > 0) {
        CurrCharge -= UnchargeRate;
        if (CurrCharge < 0) {
          CurrCharge = 0;
        }
      }
      if (CurrCharge > 0 && CurrCharge < ChargeMax) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("SoulLinkChargeDust"), newColor:Color.DeepSkyBlue)];
        dust.position += -Vector2.UnitY.RotatedBy((CurrCharge / ChargeMax) * 2 * Math.PI) * 56;
      }
      TickCooldown();
    }
  }
}