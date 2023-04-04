# v3.2.0.0

## Feature update 

Added Ori dye support, place dye to chestplate slot!
Added camera movement on looking up and crouching
Refactored code, now uses AnimLib's abilities code
Got rid of obsolete features in recipe groups
Corrected dash ability lv2 and lv3 descriptions
Reduced charge dash cooldown on lv3
Disabled active cursor on sappling
Reduced stomp randomness (fixed invalid float to int cast)

---

# v3.1.0.0

## Updated to terraria 1.4

Terraria 1.4 port attempt.

---

# v3.0.0.3

## Bug Fixes

Fixed server errors that prevented some mod bosses from spawning.

---

# v3.0.0.2

## Bug Fixes

Fixed invalid Sein recipe.

---

# v3.0.0.1

### Reupload to Mod Browser

Some issues involving an outdated github repo used for mod browser uploads

---

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

# v2.3.2

## Update to tML v0.11.3

## Additions
- Uses the Mod Configurations menu.
    - This will make configs (especially player color) much easier to work with.
- Added most ability variables to config.

## Removals
- Removed the old config file.
- Removed `/config`, `/color`, `/color2` commands.
    - All of this is much easier in the Config menu.

## Bug Fixes
- Fixed server crash due to attempting to load textures.

---

# v2.3.1.1

## Bug Fixes
- Fixed a possible issue with Mac and Linux.

---

# v2.3.1

## Additions
- Added Secondary Colors
    - Use `/color2` to change this from in-game.

## Changes
- Burrow strength is now based on pickaxe power.
- Bash now works on projectiles.
- Stomp now stops the player mid-air if a stomped enemy does not die.
- Reduced the number of targets Stomp can damage to 8.
    - For each additional NPC hit, the damage of Stomp is slightly reduced until reaching the 8th NPC.
- Dash now only goes on cooldown (1s) if a boss is alive.
- Wings are hidden while using certain abilities.
- Reduced camera smoothing during boss fights.

## Bug Fixes
- Ori now displays in the character select screen.
- Fixed how some abilities interact with each other.
- Fixed wings displaying in front of the player.

---

# v2.3.0.2

## Changes
- Burrowable tiles is now based on Pickaxe Power.
    - The config defaults to 0, which makes most tiles burrowable.
    - Read the Terraria Wiki on Pickaxe Power for more information.
    - Due to this change, loading the mod will take longer.
- Tweaked Burrow movement.
    - Smaller gaps should no longer cause Burrow to get stuck.
- Added config option to hold down for x frames to stomp.
- Renamed config option "BlindForestMovement" to "AbilityCooldowns" for better clarity.

## Bug Fixes
- *Actually* fixed water walking.
- Fixed Dash refreshing when it shouldn't be.
- Fixed uncharging Wall Charge Jump not properly resetting the charge.

---

# v2.3.0.1

## Bug Fixes
- Fixed first transformation lasting forever.

---

# v2.3

## Additions
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
- Abilities now indicate when they are refreshed.
- Added support for Mod Helpers.

## Changes
- Modified cooldowns to various abilities.
    - Stomp now has a cooldown of 8 seconds.
    - Charge Jump now has a cooldown of 6 seconds.
    - Charge Dash now has a cooldown of 5 seconds.
    - Dash now has a cooldown of 1 second.
    - Bash cooldown reduced from 2 seconds to 1.5 seconds.
- Changed invincibility timer for various abilities.
    - Note: Immunity is always active when performing one of the below ability, and the timer impacts immunity after the ability is finished.
    - Stomp immunity timer reduced from 1/3 seconds to 1/5 seconds.
    - Charge Dash immunity timer reduced from 1/3 seconds to 1/5 seconds.
    - Bash no longer grants immunity to damage. Instead, bashed NPCs cannot deal damage.
- Improve trail color.
- Rewrote Charge Jump.
- Changed Sein's texture.

## Bug Fixes
- Fix Dash interaction with Cloud-based jumps.
- Fix all skills not working properly with flipped gravity.
- Fix wrong animation playing while water walking and on rope.
- Fix various actions still able to be performed while using Stomp.

---

# v2.2.5.2

## Bug Fixes
- Fix Bash no longer working.

---

# v2.2.5

## Additions
- Add Config file for lighting, color, and camera movement.
    - GlobalPlayerLight: All other players' lighting looks the same as yours if set to true.
    - DoPlayerLight: If your player lights up.
    - OriColor: The color of your sprite.
    - SmoothCamera: If camera movement is smooth similar to Blind Forest or retains vanilla behavior.

## Changes
- Reduce speed of Sein's auto fire to 75%.
- Renamed `/oricolor` to `/color`.
- Trail color is now based on sprite color.

## Bug Fixes
- Fix Dash resetting flight timer.
- Fix Charge Jump not activating while water walking.
- Transform animation now plays properly.

---

# v2.2.4

## Changes
- Sein is set to auto fire rather than manual fire while holding non-damage tiles.
    - To manual fire, you must now either be holding the summon item or nothing.
- Reduced Spirit Flame manual damage multiplier from 2x to 1.4x.
- Increase Bash cooldown to 2.5 seconds.

## Bug Fixes
- Fix some vanilla bosses and all mod bosses not dropping loot (sorry about that!)
- Fix Sein firing while performing menu actions.

---

# v2.2.3

## Changes
- Spirit Flame can now be used manually.
    - Use Left Mouse button while either holding nothing, the summon item, or an item that doesn't deal damage.
    - Deals 2x damage compared to auto fire.
- Balanced Sein to be more viable early game.
    - Sein and Spirit Flame no longer collide with tiles.
    - Sein can see enemies a short distance through walls.
    - Now targets enemies closest to the player, rather than to Sein.
- Charge Dash now requires 20 mana to use.
- Ability to toggle player light
    - Use `/light` to turn on or off.
- Temporarily disable transformation animation.
- Slightly reduced filesize further.

## Bug Fixes
- Fixed Charge Dash hitbox being too small.
- Fix mount interaction.
- Cleaned up some sounds and animations.

---

# v2.2.2

## Changes
- Greatly reduced filesize.
- Damage of Stomp, Bash, and Charge Dash now scaled to which Sein is summoned.
- Stomp, Bash, and Charge Dash now grant temporary invulnerability after ending.
- Increased volume of certain sounds.

## Bug Fixes
- Fixed Bash activating on the Destroyer.
- Fixed certain armors greatly increasing the speed of animations.
- Actually fixed Sein being able to be spawned endlessly.
- Remove unintended debug messages.

---

# v2.2.1

## Bug Fixes
- Fixed some texts not displaying properly.
- Fixed Sein no longer spawnable after mod reload.
- Fixed Sein being able to be spawned endlessly.
- Fixed Bash and Charge Dash targeting friendly NPCs.
- Fixed Charge Dash not dealing damage to enemies.

---

# v2.2

## Additions
- Added the Spirit Orb.
    - The Spirit Orb is used to summon Sein.
    - This item can be upgraded throughout your playthrough.
- Added the minion Sein.
    - Moves like Sein.
    - Shoots like Sein... sorta.
    - Uses 0 Minion slots.
    - Maximum 1 active per player.
    - Upgraded versions of the Spirit Orb summon a stronger variant of Sein.
- You can now change your Spirit color with /oricolor.
    - Usage: /oricolor \<r> \<g> \<b>
    - Values are between 0 and 255.

## Changes
- Massive overhaul of the mod's backend.

## Bug Fixes
- The Spirit state is now properly synced among clients.
- Lunar armors no longer caused Spirit animations to animate at unintended speeds.
- Double Jump is now usable after Bashing or Wall Jumping.
- Many other bug fixes.

---

# v2.1

## Additions
- Proper Water Walking mechanics.
- Spirit potion was added.
    - Can be used to remotely transform to and from Ori.
- Reduced .tmod size.

---

# v2.0

## Changes
- Charge Dash was tweaked.
    - Distance is two blocks further.

## Bug Fixes
- More bugfixes.

---

# v1.4

## Bug Fixes
- Multiplayer fixes pertaining to people not looking like Ori.

---

# v1.3

### Mod browser release

---

# v1.2

## Bug Fixes
- Version number actually updates in-game.

---

# v1.1

## Bug Fixes
- Fixed issues pertaining to minecarts.
- Ori now dies properly.

---

# v1.0

## Release