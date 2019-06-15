# OriMod
Terraria mod that introduces the Ori franchise. Created by Fro Zen and TwiliChaos

The Ori franchise is the property of Microsoft. This mod and its creators are in no way affiliated with Microsoft or Moon Studios.

If you enjoy this mod, we strongly urge you purchase Ori and the Blind Forest, as well as Ori and the Will of the Wisps when it releases.

# Changelog
## v2.2.5.1
### Bug Fixes
- Fix Bash no longer working
## v2.2.5
### Additions
- Add Config file for lighting, color, and camera movement.
    - GlobalPlayerLight: All other players' lighting looks the same as yours if set to true.
    - DoPlayerLight: If your player lights up.
    - OriColor: The color of your sprite.
    - SmoothCamera: If camera movement is smooth similar to Blind Forest or retains vanilla behavior.
### Changes
- Reduce speed of Sein's auto fire to 75%.
- Renamed `/oricolor` to `/color`.
- Trail color is now based on sprite color.
### Bug Fixes
- Fix Dash resetting flight timer.
- Fix Charge Jump not activating while water walking.
- Fix wrong animmation playing while water walking.
- Transform animation now plays properly.
---
## Known Issues
- Multiplayer syncing for movement is a work in progress. Netcode is hard.
- Charge Dash sometimes causes clipping into the ceiling.
- Spirit Flame is not considered summon damage in some mods.
- Movement is not properly affected by Mighty Winds.
---
## v2.2.4
### Changes
- Sein is set to auto fire rather than manual fire while holding non-damage tiles.
    - To manual fire, you must now either be holding the summon item or nothing.
- Reduced Spirit Flame manual damage multiplier from 2x to 1.4x.
- Increase Bash cooldown to 2.5 seconds.
### Bug Fixes
- Fix some vanilla bosses and all mod bosses not dropping loot (sorry about that!)
- Fix Sein firing while performing menu actions.
## v2.2.3
### Changes
- Spirit Flame can now be used manually
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
### Bug Fixes
- Fixed Charge Dash hitbox being too small.
- Fix mount interaction.
- Cleaned up some sounds and animations.
## v2.2.2
### Changes
- Greatly reduced filesize.
- Damage of Stomp, Bash, and Charge Dash now scaled to which Sein is summoned.
- Stomp, Bash, and Charge Dash now grant temporary invulnerability after ending.
- Increased volume of certain sounds.
### Bug Fixes
- Fixed Bash activating on the Destroyer.
- Fixed certain armors greatly increasing the speed of animations.
- Actually fixed Sein being able to be spawned endlessly.
- Remove unintended debug messages.
## v2.2.1
### Bug Fixes
- Fixed some texts not displaying properly.
- Fixed Sein no longer spawnable after mod reload.
- Fixed Sein being able to be spawned endlessly.
- Fixed Bash and Charge Dash targeting friendly NPCs.
- Fixed Charge Dash not dealing damage to enemies.
## v2.2
### Additions
- Added the Spirit Orb
    - The Spirit Orb is used to summon Sein.
    - This item can be upgraded throughout your playthrough.
- Added the minion Sein
    - Moves like Sein
    - Shoots like Sein... sorta
    - Uses 0 Minion slots
    - Maximum 1 active per player
    - Upgraded versions of the Spirit Orb summon a stronger variant of Sein.
- You can now change your Spirit Guardian color with /oricolor
    - Usage: /oricolor \<r> \<g> \<b>
    - Values are between 0 and 255
### Changes
- Massive overhaul of the mod's backend
### Bug Fixes
- The Spirit Guardian state is now properly synced among clients.
- Lunar armors no longer caused Spirit Guardian animations to animate at unintended speeds.
- Double Jump is now usable after Bashing or Wall Jumping.
- Many other bug fixes.
## v2.1
### Additions
- Proper Water Walking mechanics
- Spirit potion was added
-- Can be used to remotely transform into Ori
- Reduced .tmod size
## v2.0
### Changes
- Charge Dash was tweaked
-- Distance is two blocks further
### Bug Fixes
- More bugfixes
## v1.4
### Bug Fixes
-Multiplayer fixes pertaining to people not looking like Ori
(it apparently doesnt actually work, ill be investigating this)
## v1.3
#### Mod browser release
## v1.2
### Bug Fixes
- Version number actually updates ingame
## v1.1
### Bug Fixes
- Fixed issues pertaining to minecarts
- Ori now dies properly
## v1.0
### Release