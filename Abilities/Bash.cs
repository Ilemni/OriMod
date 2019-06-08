using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;

namespace OriMod.Abilities {
  public class Bash : Ability {
    internal Bash(OriPlayer oriPlayer, OriAbilities handler) : base(oriPlayer, handler) { }
    
    public static List<int> CannotBash = new List<int> {
      NPCID.BlazingWheel, NPCID.SpikeBall
    };
    private const int BashDamage = 15;
    private const float BashPlayerStrength = 12f;
    private const float BashNpcStrength = 10f;
    private const float BashRange = 120f;
    private const int MinBashDuration = 30;
    private const int MaxBashDuration = 85;
    private const int Cooldown = 5;
    internal int CurrDuration { get; private set; }
    private Vector2 playerStartPos;
    private Vector2 npcStartPos;
    public byte NpcID { get; internal set; }
    public byte WormID { get; internal set; }
    public NPC Npc => NpcID < Main.maxNPCs ? Main.npc[NpcID] : null;
    public NPC WormNpc => WormID < Main.maxNPCs ? Main.npc[WormID] : null;
    public bool IsBashingWorm => WormID < Main.maxNPCs;
    internal override bool CanUse => Refreshed && State == States.Inactive && !Handler.stomp.InUse /*&& Handler.cJump.InUse*/;

    protected override void ReadPacket(System.IO.BinaryReader r) {
      if (InUse) {
        NpcID = r.ReadByte();
        if (State == States.Starting) {
          npcStartPos = r.ReadVector2();
          Npc.GetGlobalNPC<OriNPC>().BashPos = Npc.Center = npcStartPos;
          playerStartPos = r.ReadVector2();
        }
      }
    }
    protected override void WritePacket(Terraria.ModLoader.ModPacket packet) {
      if (InUse) {
        packet.Write((byte)NpcID);
        if (State == States.Starting) {
          packet.WriteVector2(npcStartPos);
          packet.WriteVector2(playerStartPos);
        }
      }
    }
    
    private bool BashStart() {
      byte tempNpcID = 255;
      float currDist = BashRange;
      for (int n = 0; n < Main.maxNPCs; n++) {
        NPC localNpc = Main.npc[n];
        if (localNpc == null || !localNpc.active || localNpc.friendly || CannotBash.Contains(localNpc.type) || localNpc.boss || localNpc.immortal) continue;
        if (localNpc.aiStyle == 37) continue; // A Destroyer segment is not considered a boss
        float newDist = (player.position - localNpc.position).Length();
        if (newDist < currDist) {
          tempNpcID = (byte)n;
          currDist = newDist;
        }
      }
      if (tempNpcID == 255) {
        NpcID = 255;
        return false;
      }
      NpcID = (byte)tempNpcID;
      WormID = Npc.aiStyle == 6 ? (byte)Npc.ai[3] : (byte)255; 
      OriNPC bashNpc = (IsBashingWorm ? WormNpc : Npc).GetGlobalNPC<OriNPC>();
      bashNpc.IsBashed = true;
      bashNpc.BashPos = Npc.Center;

      playerStartPos = player.Center;
      npcStartPos = Npc.Center;
      oPlayer.PlayNewSound("Ori/Bash/seinBashStartA", 0.7f);
      return true;
    }
    protected override void UpdateActive() {
      if (CurrDuration == MinBashDuration + 2) {
        oPlayer.PlayNewSound("Ori/Bash/seinBashLoopA", 0.7f);
      }
      oPlayer.ImmuneTimer = 30;
    }
    protected override void UpdateEnding() {
      player.pulley = false;
      float bashAngle = player.AngleFrom(Main.MouseWorld);
      oPlayer.PlayNewSound("Ori/Bash/seinBashEnd" + OriPlayer.RandomChar(3, ref currRand), /*0.7f*/ Main.soundVolume);
      oPlayer.UnrestrictedMovement = true;
      Vector2 bashVector = new Vector2((float)(0 - (Math.Cos(bashAngle))), (float)(0 - (Math.Sin(bashAngle))));
      player.velocity = bashVector * BashPlayerStrength;
      Npc.velocity = -bashVector * BashNpcStrength;
      OriNPC bashNpc = (IsBashingWorm ? WormNpc : Npc).GetGlobalNPC<OriNPC>();
      bashNpc.IsBashed = false;
      if (oPlayer.IsGrounded) {
        player.position.Y -= 1f;
      }
      int damage = BashDamage + oPlayer.SeinMinionUpgrade * 9;
      player.ApplyDamageToNPC(Npc, damage, 0, 1, false);
    }
    protected override void UpdateUsing() {
      if (State != States.Ending) {
        if (Npc != null) Npc.Center = npcStartPos;
        player.velocity = Vector2.Zero;
        player.gravity = 0;
      }
      Npc.netUpdate2 = true;
      // // Allow only quick heal and quick mana
      player.controlJump = false;
      player.controlUp = false;
      player.controlDown = false;
      player.controlLeft = false;
      player.controlRight = false;
      player.controlHook = false;
      player.controlInv = false;
      player.controlMount = false;
      player.controlSmart = false;
      player.controlThrow = false;
      player.controlTorch = false;
      player.controlUseItem = false;
      player.controlUseTile = false;
      player.immune = true;
      player.buffImmune[BuffID.CursedInferno] = true;
      player.buffImmune[BuffID.Dazed] = true;
      player.buffImmune[BuffID.Frozen] = true;
      player.buffImmune[BuffID.Frostburn] = true;
      player.buffImmune[BuffID.MoonLeech] = true;
      player.buffImmune[BuffID.Obstructed] = true;
      player.buffImmune[BuffID.OnFire] = true;
      player.buffImmune[BuffID.Poisoned] = true;
      player.buffImmune[BuffID.ShadowFlame] = true;
      player.buffImmune[BuffID.Silenced] = true;
      player.buffImmune[BuffID.Slow] = true;
      player.buffImmune[BuffID.Stoned] = true;
      player.buffImmune[BuffID.Suffocation] = true;
      player.buffImmune[BuffID.Venom] = true;
      player.buffImmune[BuffID.Weak] = true;
      player.buffImmune[BuffID.WitheredArmor] = true;
      player.buffImmune[BuffID.WitheredWeapon] = true;
      player.buffImmune[BuffID.WindPushed] = true;
    }
    protected override void UpdateFailed() {
      oPlayer.PlayNewSound("Ori/Bash/bashNoTargetB", Main.soundVolume);
    }

    internal override void Tick() {
      if (InUse) {
        CurrDuration++;
        switch (State) {
          case States.Starting:
            if (CurrDuration > MinBashDuration) {
              State = States.Active;
            }
            return;
          case States.Active:
            if (CurrDuration > MaxBashDuration || !OriMod.BashKey.Current || Npc == null || !Npc.active) {
              State = States.Ending;
            }
            return;
          case States.Ending:
            State = States.Inactive;
            return;
        }
      }
      else {
        if (!Refreshed) {
          CurrDuration++;
          if (CurrDuration > Cooldown) {
            Refreshed = true;
            State = States.Inactive;
          }
        }
        if (CanUse && OriMod.BashKey.JustPressed) {
          Refreshed = false;
          CurrDuration = 0;
          bool didBash = BashStart();
          if (!didBash) {
            State = States.Failed;
            return;
          }
          State = States.Starting;
        }
      }
    }
  }
}