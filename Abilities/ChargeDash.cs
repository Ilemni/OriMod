using Microsoft.Xna.Framework;
using OriMod.Utilities;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace OriMod.Abilities {
  public class ChargeDash : Ability {
    internal ChargeDash(AbilityManager handler) : base(handler) { }
    public override int Id => AbilityID.ChargeDash;

    internal override bool DoUpdate => !Manager.dash.DoUpdate && (InUse || oPlayer.Input(OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) && Manager.cDash.Refreshed);
    internal override bool CanUse => base.CanUse && Refreshed && !InUse && !oPlayer.OnWall && !Manager.stomp.InUse && !Manager.bash.InUse && !player.mount.Active;
    protected override int Cooldown => (int)(Config.CDashCooldown * 30);
    protected override Color RefreshColor => Color.LightBlue;

    private int ManaCost => 20;
    private int Duration => Speeds.Length - 1;
    private static readonly float[] Speeds = new float[15] {
      100f, 99.5f, 99, 98.5f, 97.5f, 96.3f, 94.7f, 92.6f, 89.9f, 86.6f, 78.8f, 56f, 26f, 15f, 15f
    };
    private static float SpeedMultiplier => Config.CDashSpeedMultiplier * 0.8f;

    public byte NpcID { get; internal set; } = 255;
    internal int Direction;

    public Projectile Proj { get; private set; }

    private readonly RandomChar randChar = new RandomChar();

    protected override void ReadPacket(System.IO.BinaryReader r) {
      NpcID = r.ReadByte();
    }

    protected override void WritePacket(ModPacket packet) {
      packet.Write(NpcID);
    }

    internal override void PutOnCooldown(bool force = false) {
      Refreshed = false;
      base.PutOnCooldown(force);
    }

    protected override void TickCooldown() {
      if (!Refreshed || CurrCooldown > 0) {
        CurrCooldown--;
        if (CurrCooldown < 0 && !OriMod.ChargeKey.Current) {
          Refreshed = true;
        }
      }
    }

    internal void End(bool byNpcContact = false) {
      SetState(State.Inactive);
      PutOnCooldown();
      if (byNpcContact) {
        player.position = Main.npc[NpcID].position;
        player.position.Y -= 32f;
        player.velocity *= Speeds[CurrTime] * SpeedMultiplier < 50 ? 0.5f : 0.25f;
      }
      else if ((NpcID == 255 || CurrTime > 4) && Math.Abs(player.velocity.Y) < Math.Abs(player.velocity.X)) {
        Vector2 newVel = NpcID == 255 && !Manager.airJump.InUse ? new Vector2(Direction, 0) : player.velocity;
        newVel.Normalize();
        newVel *= Speeds[Speeds.Length - 1] * SpeedMultiplier;
        player.velocity = newVel;
      }
      Proj = null;
      NpcID = 255;
    }

    protected override void UpdateStarting() {
      float tempDist = 720f;
      int tempNPC = -1;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC localNpc = Main.npc[n];
        if (!localNpc.active || localNpc.friendly) {
          continue;
        }

        float dist = (player.position - localNpc.position).Length();
        if (dist < tempDist) {
          tempDist = dist;
          tempNPC = localNpc.whoAmI;
        }
      }
      if (tempNPC != -1) {
        NpcID = (byte)tempNPC;
        Direction = player.direction = player.position.X - Main.npc[NpcID].position.X < 0 ? 1 : -1;
      }
      else {
        Direction = PlayerInput.Triggers.Current.Left ? -1 : PlayerInput.Triggers.Current.Right ? 1 : player.direction;
      }
      oPlayer.PlayNewSound("Ori/ChargeDash/seinChargeDash" + randChar.NextNoRepeat(3), .5f);
      Proj = Main.projectile[Projectile.NewProjectile(player.Center, Vector2.Zero, oPlayer.mod.ProjectileType("ChargeDashProjectile"), 30, 0f, player.whoAmI, 0, 1)];
      Proj.damage = 12 + OriWorld.GlobalSeinUpgrade * 9;
    }

    protected override void UpdateUsing() {
      float speed = Speeds[CurrTime] * SpeedMultiplier;
      player.gravity = 0;
      if (NpcID < Main.maxNPCs && Main.npc[NpcID].active) {
        player.maxFallSpeed = speed;
        Vector2 dir = Main.npc[NpcID].position - player.position;
        dir.Y -= 32f;
        dir.Normalize();
        player.velocity = dir * speed;
        if (CurrTime < Duration && (player.position - Main.npc[NpcID].position).Length() < speed) {
          End(byNpcContact: true);
        }
      }
      else {
        player.velocity.X = speed * Direction * 0.8f;
        player.velocity.Y = (oPlayer.IsGrounded ? -0.1f : 0.15f * (CurrTime + 1)) * player.gravDir;
      }

      player.runSlowdown = 26f;
      oPlayer.ImmuneTimer = 12;
    }

    internal override void Tick() {
      if (CanUse && OriMod.DashKey.JustPressed && OriMod.ChargeKey.Current) {
        if (player.CheckMana(ManaCost, true, true)) {
          SetState(State.Active);
          CurrTime = 0;
          UpdateStarting();
        }
        else if (!Manager.dash.InUse) {
          Manager.dash.StartDash();
        }
        return;
      }
      TickCooldown();
      if (InUse) {
        SetState(State.Inactive);
        Manager.dash.Refreshed = false;
        CurrTime++;
        if (CurrTime > Duration || oPlayer.OnWall || Manager.bash.InUse || PlayerInput.Triggers.JustPressed.Jump) {
          End();
        }
      }
    }
  }
}
