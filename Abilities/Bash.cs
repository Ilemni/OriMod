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
    private const float BashPlayerStrength = 10f;
    private const float BashNpcStrength = 8f;
    private const float BashRange = 175f;
    private const int MinBashDuration = 20;
    private const int MaxBashDuration = 85;
    private const int Cooldown = 20;
    internal int CurrDuration { get; private set; }
    private Vector2 playerStartPos;
    private Vector2 npcStartPos;
    public byte BashNpcID { get; internal set; }
    public NPC BashNpc {
      get {
        return Main.npc[BashNpcID];
      }
      internal set {
        BashNpcID = (byte)value.whoAmI;
      }
    }
    internal override bool CanUse {
      get {
        return Refreshed && State == States.Inactive && !Handler.stomp.InUse /*&& Handler.cJump.InUse*/;
      }
    }
    
    private bool BashStart() {
      NPC tempNPC = null;
      float currDist = BashRange;
      for (int n = 0; n < Main.maxNPCs; n++) {
        if (!Main.npc[n].active) continue;
        float newDist = (player.position - Main.npc[n].position).Length();
        if (newDist < currDist) {
          tempNPC = Main.npc[n];
          currDist = newDist;
        }
      }
      if (tempNPC == null || CannotBash.Contains(tempNPC.type) || tempNPC.boss == true || tempNPC.immortal) {
        BashNpcID = 255;
        return false;
      }
      BashNpc = tempNPC;
      playerStartPos = player.Center;
      npcStartPos = BashNpc.Center;
      oPlayer.PlayNewSound("Ori/Bash/seinBashStartA", /*0.7f*/ Main.soundVolume);
      return true;
    }
    protected override void UpdateActive() {
      oPlayer.PlayNewSound("Ori/Bash/seinBashLoopA", /*0.7f*/ Main.soundVolume);
    }
    protected override void UpdateEnding() {
      player.pulley = false;
      float bashAngle = player.AngleFrom(Main.MouseWorld);
      oPlayer.PlayNewSound("Ori/Bash/seinBashEnd" + OriPlayer.RandomChar(3, ref currRand), /*0.7f*/ Main.soundVolume);
      oPlayer.UnrestrictedMovement = true;
      Vector2 bashVector = new Vector2((float)(0 - (Math.Cos(bashAngle))), (float)(0 - (Math.Sin(bashAngle))));
      player.velocity = bashVector * BashPlayerStrength;
      player.velocity.Y *= 0.8f;
      BashNpc.velocity = -bashVector * BashNpcStrength;
      if (oPlayer.IsGrounded) {
        player.position.Y -= 1f;
      }
      player.ApplyDamageToNPC(BashNpc, BashDamage, 0, 1, false);
      BashNpcID = 255;
    }
    protected override void UpdateUsing() {
      player.Center = playerStartPos;
      BashNpc.Center = npcStartPos;

      if (State != States.Ending) {
        player.velocity.X = 0;
        player.velocity.Y = 0 - player.gravity;
      }

      // Allow only quick heal and quick mana
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
            if (CurrDuration > MaxBashDuration || !OriMod.BashKey.Current || !BashNpc.active) {
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