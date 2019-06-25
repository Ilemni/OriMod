# OriMod
Terraria mod that introduces the Ori franchise. Created by Fro Zen and TwiliChaos

The Ori franchise is the property of Microsoft. This mod and its creators are in no way affiliated with Microsoft or Moon Studios.

If you enjoy this mod, we strongly urge you purchase Ori and the Blind Forest, as well as Ori and the Will of the Wisps when it releases.

# Changelog
## v2.3.0.2
### Changes
- Burrowable tiles is now based on Pickaxe power.
    - The config defaults to 0, which makes most tiles burrowable.
    - Read bullet point 4 in [this wiki page](https://terraria.gamepedia.com/Pickaxe_power#Mechanics) for more information.
    - Due to this change, loading the mod will take longer.
- Tweaked Burrow movement.
    - Smaller gaps should no longer cause Burrow to get stuck.
- Add config option to hold down for x frames to stomp.
- Rename config option "BlindForestMovement" to "AbilityCooldowns" for better clarity.
### Bug Fixes
- *Actually* fixed water walking.
- Fix Dash refreshing when it shouldn't be.
- Fix uncharging Wall Charge Jump not properly resetting the charge.
## v2.3.0.1
### Bug Fixes
- Fix first transformation lasting forever
## v2.3
### Additions
- Added Burrow
    - Burrow can travel through all forms of Sand, Silt, and Slush.
    - Bound to Left Shift by default.
- Added Wall Charge Jump (It's about time)
    - Activate by climbing and attempting to move in the opposite direction.
    - Aim by facing the mouse in the desired direction
    - Cooldown of 6 seconds
- Added additional Config options
    - RestrictiveCrouch: If true, crouching prevents moving left or right. Default: true
    - BlindForestMovement: If true, disables ability cooldowns. Default: false
    - BurrowToMouse: Determines if Burrow is controlled by mouse or arrow keys.
    - AutoBurrow: Automatically re-enter Burrow if the Burrow key is still held (note this feature is buggy). Default: false
    - BurrowTier: Determines which tiles can be burrowed through.
        - 0 (Default): Sands, Slush, Silt, and Leaves
        - 1: Dirt and Grass
        - 2: Wood, Stone, and Sandstone
        - -1: Everything
- Abilities now indicate when they are refreshed
- Added support for Mod Helpers.
### Changes
- Modified cooldowns to various abilities
    - Stomp now has a cooldown of 8 seconds.
    - Charge Jump now has a cooldown of 6 seconds.
    - Charge Dash now has a cooldown of 5 seconds.
    - Dash now has a cooldown of 1 second.
    - Bash cooldown reduced from 2 seconds to 1.5 seconds.
- Changed invincibility timer for various abilities
    - Note: Immunity is always active when performing one of the below ability, and the timer impacts immunity after the ability is finished.
    - Stomp immunity timer reduced from 1/3 seconds to 1/5 seconds.
    - Charge Dash immunity timer reduced from 1/3 seconds to 1/5 seconds.
    - Bash no longer grants immunity to damage. Instead, bashed NPCs cannot deal damage.
- Improve trail color
- Rewrote Charge Jump
- Changed Sein's texture
### Bug Fixes
- Fix Dash interaction with Cloud-based jumps
- Fix all skills not working properly with flipped gravity.
- Fix wrong animation playing while water walking and on rope.
- Fix various actions still able to be performed while using Stomp.
---
To view previous version changes, refer to the [changelog](CHANGELOG.md).