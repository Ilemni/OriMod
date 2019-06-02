using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace OriMod {
  
  // This partial class is for Movement-specific implementations
  
  public sealed partial class OriAbilities {
    #region Variables
    private const float defaultFallSpeed = 16f;
    
    private const int soulLinkActivateTime = 48;
    private const int soulLinkRechargeTime = 60 * 60;
    public int soulLinkCurrTime = 0;
    public int soulLinkCurrRecharge = 0;

    private const int bashMinTime = 40;
    private const int bashMaxTime = 150;
    private const int doubleBashWindow = 2;
    public byte bashCurrNPC = 255;
    public int bashCurrTime = 0;
    public float bashCurrAngle = 0;

    private const int chargeJumpStartTime = 60;
    public int chargeJumpCurrTime = 0;

    public Vector2 grenadePos = Vector2.Zero;
    #endregion
    internal void Tick() {
      if (oPlayer == null || player.whoAmI != Main.myPlayer) {
        return;
      }
      List<int> prevStates = new List<int>();
      // string[] eNames = Enum.GetNames(typeof(MoveType)); // TODO: Replace with Movements
      // int[] eValues = (int[])Enum.GetValues(typeof(MoveType));
      // foreach(string str in eNames) {
      //   prevStates.Add((int)GetState(str));
      // }

      // soulLink.Tick();
      wJump.Tick();
      // cFlame.Tick();
      airJump.Tick();
      // bash.Tick();
      stomp.Tick();
      glide.Tick();
      climb.Tick();
      // cJump.Tick();
      dash.Tick();
      cDash.Tick();
      // grenade.Tick();
      
      // List of things Minecart should disable
      if (player.mount.Cart) {
        // soulLink.CanUse = false;
        wJump.CanUse = false;
        airJump.CanUse = false;
        stomp.CanUse = false;
        glide.CanUse = false;
        climb.CanUse = false;
        // cJump.CanUse = false;
        dash.CanUse = false;
        cDash.CanUse = false;
      }
      List<string> changes = new List<string>();
      if (changes.Count > 0) {
        if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer) {
          ModNetHandler.movementPacketHandler.SendMovementState(255, player.whoAmI, changes);
        }
      }
    }
    internal void TickOtherClient() {
      if (glide.InUse) {
        glide.Update();
      }
    }
  }
}