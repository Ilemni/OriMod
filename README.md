# OriMod
Terraria mod that introduces the Ori franchise. Created by Fro Zen and TwiliChaos

The Ori franchise is the property of Microsoft. This mod and its creators are in no way affiliated with Microsoft or Moon Studios.

If you enjoy this mod, we strongly urge you purchase Ori and the Blind Forest, as well as Ori and the Will of the Wisps when it releases.

# Changelog

# v3.0

## Updated to tML v0.11.7.7

Animation backend ported to library mod AnimLib

### Abilities
Added Ability Levels
- These are upgraded with the new craftable Spirit Medalion items.

Abilities are no longer unlocked upon start. Unlock or upgrade them by crafting Spirit Medallions.

#### Launch (added)
- From WotW, this ability launches you through the air.
- 2 Levels, unlocks as Charge Jump Lv3 and Lv4

#### Air Jump
- 4 Levels
    - Each level adds an extra jump

#### Bash
- 3 Levels
    - Lv1: Bash NPCs
    - Lv2: Also bash most Projectiles
    - Lv3: Bash is more forceful
- Increased range.
- Fixed Bash not properly detecting NPCs that are in-range.
  - Distance now properly based on how far the player hitbox is from a bashable NPC.

#### Burrow
- 3 Levels
    - Each level allows more tile types to be burrowed through.
- Now has a time limit. This behaves similarly to underwater breath.
- Now requires being crouched to enter.
- Hold down left click to move faster through the ground.

#### Charge Dash
- Unlocks as Dash Lv3
- Fixed Charge Dash attempting to target NPCs through walls.

#### Charge Jump
- 4 Levels
    - Lv1: Charge Jump
    - Lv2: Charge Jump off walls
    - Lv3: Launch from the air
    - Lv4: Launch multiple times
- Fixed Charging Charge Jump always bound to Up key.

#### Climb
- Climbing over ledges no longer requires jumping.

#### Dash
- 3 Levels
    - Lv1: Dash
    - Lv2: Dash without cooldown
    - Lv3: Charge Dash

#### Stomp
- 3 Levels
    - Each level increases damage, range, speed, and knockback.

### Sein
- Changed movement to be simpler and more consistent.
- Removed piercing.
- Removed damage increase against first/selected NPC.
- Lowered damage, number of targets, number of shots fired
  at once, across all Sein levels.

### Changes
- Config tooltips now show default values.
- Optimize textures for better memory usage.

### Removals
- Removed Ability configs
  - This is replaced with leveling abilities through
    in-game progression.

### Bug Fixes
- Fixed multiplayer syncing.
- Fixed crashing when trying to use a disposed texture after
  reloading mods.
- Fixed abilities being active when they shouldn't be, such as
  when respawning and when under The Tongue debuff.
- Fixed texture glitch while burrowing with an armor effect.
- Fixed incorrect gravity at the top of the map.

### Misc
- The animation system used for this mod was ported to the
  library mod AnimLib.

---

## v2.3.2

### Update to tML v0.11.3

### Additions
- Uses the Mod Configurations menu.
    - This will make configs (especially player color) much easier to work with.
- Added most ability variables to config.

### Removals
- Removed the old config file
- Removed `/config`, `/color`, `/color2` commands.
    - All of this is much easier in the Config menu.

### Bug Fixes
- Fixed server crash due to attempting to load textures.

---

To view previous version changes, refer to the [changelog](CHANGELOG.md).