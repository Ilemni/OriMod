using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace OriMod.Abilities {
  public class SoulLink : Ability {
    public SoulLink(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    public override int id => AbilityID.SoulLink;
    internal override bool DoUpdate => OriMod.SoulLinkKey.Current || InUse;
    internal override bool CanUse => base.CanUse && !AnyBossAlive && player.CheckMana(ManaCost, blockQuickMana:true) && oPlayer.IsGrounded;
    protected override int Cooldown => (int)(Config.SoulLinkCooldown * 30);

    private static Point p(int x, int y) => new Point(x, y);
    private static readonly Point[] Template = new Point[] {
      p(-1, -1), p(0, -1), p(1, -1),
      p(0, -1),  p(0, 0),  p(0, 1),
      p(1, -1),  p(1, 0),  p(1, 1)
    };
    internal Point[] Box = new Point[Template.Length];
    internal Point Center => PlacedSoulLink && Box[4] != Point.Zero ? Box[4] : player.Center.ToTileCoordinates();
    internal Point SoulLinkLocation { get; private set; }
    
    private float ChargeRate => 1 / (Config.SoulLinkChargeRate * 60);
    private float UnchargeRate => ChargeRate * 1.75f;
    private int RespawnTime => (int)(Config.SoulLinkRespawnTime * 30);
    private int ManaCost => 20;

    private float CurrCharge;
    internal bool PlacedSoulLink;
    internal bool Obstructed;
    private bool WasObstructed;
    private bool AnyBossAlive;
    private bool WasDead;
    
    private void CheckValidPlacement(Point? check, out bool obstructed, bool force=false) {
      obstructed = false;
      if (!force && Main.time % 20 != 0) return;

      Point[] newBox = (Point[])Box.Clone();
      newBox.UpdateHitbox(Template, check ?? Center);
      for (int i = 0; i < newBox.Length; i++) {
        Tile t = Main.tile[newBox[i].X, newBox[i].Y];
        if (Burrow.IsSolid(t)) {
          obstructed = true;
          return;
        }
      }
      return;
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
        player.Center = SoulLinkLocation.ToWorldCoordinates();
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
        CheckValidPlacement(Center, out Obstructed);
        if (Obstructed) {
          PlacedSoulLink = false;
          Main.NewText("Your Soul Link has been blocked and despawned...");
        }
      }
      if (CanUse && OriMod.SoulLinkKey.Current) {
        if (OriMod.SoulLinkKey.JustPressed) {
          AnyBossAlive = OriModUtils.IsAnyBossAlive();
          if (AnyBossAlive) {
            Main.NewText("Cannot place a Soul Link while powerful enemies are around");
            return;
          }
        }
        CheckValidPlacement(player.Center.ToTileCoordinates(), out bool tempObstructed, force:true);
        if (tempObstructed) {
          if (!WasObstructed) {
            Main.NewText("Cannot place a Soul Link here...");
          }
          CurrCharge -= UnchargeRate;
          if (CurrCharge < 0) CurrCharge = 0;
        }
        else {
          CurrCharge += ChargeRate;
          if (CurrCharge > 1) {
            player.statMana -= ManaCost;
            CurrCharge = 0;
            Active = true;
            PlacedSoulLink = true;
            Box.UpdateHitbox(Template, player.Center.ToTileCoordinates());
            SoulLinkLocation = Center;
            oPlayer.Debug("Placed a Soul Link!");
            PutOnCooldown(force:true);
          }
        }
        WasObstructed = tempObstructed;
      }
      else if (CurrCharge > 0) {
        CurrCharge -= UnchargeRate;
        if (CurrCharge < 0) {
          CurrCharge = 0;
        }
      }
      if (CurrCharge > 0 && CurrCharge < 1) {
        Dust dust = Main.dust[Dust.NewDust(player.Center, 12, 12, oPlayer.mod.DustType("SoulLinkChargeDust"), newColor:Color.DeepSkyBlue)];
        dust.customData = player;
        dust.position += -Vector2.UnitY.RotatedBy(CurrCharge * 2 * Math.PI) * 56;
      }
      TickCooldown();
    }
  }
}