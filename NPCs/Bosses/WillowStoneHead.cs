using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace OriMod.NPCs.Bosses {
  class WillowStoneHead : ModNPC {
    #region AI properties
    public WillowState state {
      get => (WillowState)npc.ai[0];
      set => npc.ai[0] = (int)value;
    }

    public WillowStateExtra extra {
      get => (WillowStateExtra)npc.ai[1];
      set => npc.ai[1] = (int)value;
    }

    public int Target {
      get => (int)npc.ai[2];
      set => npc.ai[2] = value;
    }

    public float Rotation {
      get => npc.ai[3];
      set => npc.ai[3] = value;
    }

    // LocalAI
    public int CurrentTime {
      get => (int)npc.localAI[0];
      set => npc.localAI[0] = value;
    }

    public int MaxTime {
      get => (int)npc.localAI[1];
      set => npc.localAI[1] = value;
    }

    private int CurrentRegen {
      get => (int)npc.localAI[2];
      set => npc.localAI[2] = value;
    }
    #endregion

    public int Stage {
      get {
        float percent = npc.life / npc.lifeMax;
        // 100%-75%: 1 | 75%-50%: 2 | 50%-25%: 3 | 25%-0%: 4
        return 5 - (int)(percent * 4);
      }
    }

    public static int MaxSegmentCount => 48;

    public static int SegmentRegenDuration => 20;

    private readonly bool[] currentActiveSegments = new bool[MaxSegmentCount];

    public override void SetDefaults() {
      npc.boss = true;
      npc.life = npc.lifeMax = 100000;
      npc.noGravity = true;
      npc.knockBackResist = 0;
      npc.width = npc.height = 80;
    }


    public override void AI() {
      if (true) { // Replace with check for changing state
        var state = this.state;
        var extra = this.extra;

        NextState(ref state);
        NextExtraState(state, ref extra);
        Main.NewText(state);
        this.state = state;
        this.extra = extra;
      }
    }

    #region States
    private void State_Starting() {
      npc.immortal = true;
      if (CurrentRegen < SegmentRegenDuration) {
        CurrentRegen++;
        if (CurrentRegen >= SegmentRegenDuration) {
          CurrentRegen = 0;
          for (int i = 0; i < MaxSegmentCount; i++) {
            if (!currentActiveSegments[i]) {
              var pos = WillowStoneHelper.GetWorldPosition(npc, i);
            }
          }
        }
      }
    }

    private void State_Idle() {
      npc.immortal = false;
    }

    private void State_Regenerate() {

    }
    #endregion

    public int GetCurrentSegmentCount() {
      int result = 0;
      foreach (var npc in Main.npc) {
        if (npc.active && npc.type == ModContent.NPCType<WillowStoneSegment>() && npc.ai[0] == npc.whoAmI) {
          result++;
        }
      }
      return result;
    }

    public bool? GetActiveSegmentIndices(ref bool[] arr) {
      // Result is false if zero active
      // Result is true if all are active
      // Result is null if some are active/missing
      for (int i = 0; i < MaxSegmentCount; ++i) {
        currentActiveSegments[i] = false;
      }
      int count = 0;
      foreach (var npc in Main.npc) {
        if (npc.active && npc.type == ModContent.NPCType<WillowStoneSegment>() && npc.ai[0] == npc.whoAmI) {
          var id = (int)npc.ai[1];
          if (arr[id]) {
            Main.NewText($"Warning, extra {nameof(WillowStoneSegment)} with id {id} found.");
          }

          arr[id] = true;
          count++;
        }
      }

      return count == 0 ? false : count == MaxSegmentCount ? (bool?)true : null;
    }

    private void NextState(ref WillowState state) {
      int rand = Main.rand.Next(100);
      if (state != WillowState.Idle) {
        state = WillowState.Idle;
      }
      else if (DoRegen(rand)) {
        state = WillowState.Regenerate;
      }
      else {
        switch (Stage) {
          case 1:
            if (rand < 50) {
              state = WillowState.Starting;
            }
            else {
              state = WillowState.Laser2;
            }
            break;
          case 2:
          case 3:
          case 4:
            if (rand < 10) {
              state = WillowState.Laser2;
            }
            else if (rand < 25) {
              state = WillowState.Laser3;
            }
            else if (rand < 50) {
              state = WillowState.Laser4;
            }
            else if (rand < 80) {
              state = WillowState.ShootingSeeking;
            }
            else {
              state = WillowState.Shooting;
            }
            break;
          default:
            state = WillowState.Idle;
            break;
        }
      }
    }

    private bool DoRegen(int chance) {
      float missing = (float)GetCurrentSegmentCount() / MaxSegmentCount;
      if (missing < 0.5f) {
        return false;
      }
      if (missing < 0.15f) {
        return chance < 5;
      }
      if (missing < 0.25f) {
        return chance < 15;
      }
      if (missing < 0.5f) {
        return chance < 30;
      }
      if (missing < 0.75f) {
        return chance < 45;
      }
      if (missing < 0.9f) {
        return chance < 60;
      }
      return chance < 90;
    }

    public static void NextExtraState(WillowState state, ref WillowStateExtra extra) {
      bool useExtra = state == WillowState.Shooting || state == WillowState.Laser2 || state == WillowState.Laser3 || state == WillowState.Laser4;
      if (useExtra) {
        bool flip = Main.rand.NextBool();
        if (flip) {
          extra = extra == WillowStateExtra.None ? WillowStateExtra.Right : extra == WillowStateExtra.Right ? WillowStateExtra.Left : WillowStateExtra.Left;
        }
        else {
          extra = extra == WillowStateExtra.None ? WillowStateExtra.Left : WillowStateExtra.None;
        }
      }
      else {
        extra = WillowStateExtra.None;
      }
    }

  }

  public enum WillowState : byte {
    /// <summary>
    /// No state, should be first frame of initialization
    /// </summary>
    None = 0,
    /// <summary>
    /// Boss spawn, create segments, immune.
    /// </summary>
    Starting = 1,
    /// <summary>
    /// Idle state between all action states.
    /// </summary>
    Idle = 2,
    /// <summary>
    /// Regain segments.
    /// </summary>
    Regenerate = 3,

    /// <summary>
    /// Shoots several straight projectiles at the player.
    /// </summary>
    Shooting = 4,
    /// <summary>
    /// Aims upwards, shoots seeking projectiles.
    /// </summary>
    ShootingSeeking = 5,
    /// <summary>
    /// Two lasers at once.
    /// </summary>
    Laser2 = 6,
    /// <summary>
    /// Three lasers at once.
    /// </summary>
    Laser3 = 7,
    /// <summary>
    /// Four lasers at once.
    /// </summary>
    Laser4 = 8,
    /// <summary>
    /// Push all segments outwards until they collide with something, or max distance
    /// </summary>
    Explode = 9,
  }

  public enum WillowStateExtra : byte {
    /// <summary>
    /// For when a state does not use any directional extras
    /// </summary>
    None = 0,
    Left = 1,
    Right = 2,
  }
}
