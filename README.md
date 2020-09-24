# OriMod
Terraria mod that introduces the Ori franchise. Created by Fro Zen and TwiliChaos

The Ori franchise is the property of Microsoft. This mod and its creators are in no way affiliated with Microsoft or Moon Studios.

If you enjoy this mod, we strongly urge you purchase Ori and the Blind Forest, as well as Ori and the Will of the Wisps when it releases.

# Changelog

# v3.0 (In Beta)

## Updated to tML v0.11.7.5

## Additions
- Proper multiplayer support.
- Added Spirit Medallions.
    - These medallions will either unlock or upgrade various abilities, depending on the ones you craft.
- Added time limit to Burrow.
    - Behaves like underwater breath.
- Added config options for Burrow.
    - Duration: How long Burrow lasts before suffocating.
    - Recovery: How fast Burrow recovers when not in use.

## Changes
- Abilities are no longer unlocked upon start.
- Greatly nerfed Sein.
    - Removed piercing.
    - Removed damage increase against first/selected NPC.
    - Reduced damage, number of targets, number of shots fired at once, on all Sein levels.
- Burrow now requires being crouched to enter.
- Charging for Charge Jump is no longer always bound to Up.
- Config tooltips now show default values.
- Optimize textures for better memory usage.

## Bug Fixes
- Fixed crashing when trying to use a disposed texture after reloading mods.
- Fixed abilities being active when they shouldn't be, such as when respawning and when under The Tongue debuff.
- Charging for Charge Jump is no longer caused by looking up.
- Fixed texture glitch while burrowing with an armor effect.
- Fixed Bash not properly detecting NPCs that are in-range.
    - Distance now properly based on how far the player hitbox is from a bashable NPC.

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