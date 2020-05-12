# Classic Items

## SUPPORT DISCLAIMER

### Use of a mod manager is STRONGLY RECOMMENDED.

Seriously, use a mod manager.

If the versions of ClassicItems or TILER2 (or possibly any other mods) are different between your game and other players' in multiplayer, things WILL break. If TILER2 is causing kicks for "unspecified reason", it's likely due to a mod version mismatch. Ensure that all players in a server, including the host and/or dedicated server, are using the same mod versions before reporting a bug.

**While reporting a bug, make sure to post a console log** (`path/to/RoR2/BepInEx/LogOutput.log`) from a run of the game where the bug happened; this often provides important information about why the bug is happening. If the bug is multiplayer-only, please try to include logs from both server and client.

## Description

Adds some items from Risk of Rain 1 which RoR2 thought it was too good for.

For nostalgic purposes only. Here Be Dragons who hoard balance issues, because the numbers are closer to RoR1's than 2's... and, y'know, Brooch+Clover. Will absolutely lead to silly, broken runs, unless you feel like changing the config up -- most aspects of each item are configurable!

### Current Additions
#### Tier 1
- Barbed Wire: "Hurt nearby enemies."
- Bitter Root: "Gain 8% max hp."
- Fire Shield: "Retaliate on taking heavy damage."
- Headstompers: "Hurt enemies by falling."
- Life Savings: "Gain gold over time."
- Snake Eyes: "Gain increased crit chance on failing a shrine. Removed on succeeding a shrine."
- Mysterious Vial: "Increased health regeneration."
- Spikestrip: "Drop spikestrips on being hit, slowing enemies."
    - Diff. from RoR1: provides 50% slow instead of 20% to compensate for enemy ability to path around spikestrips.
#### Tier 2
- 56 Leaf Clover: "Elite mobs have a chance to drop items."
- Boxing Gloves: "Hitting enemies have a 6% chance to knock them back."
- Golden Gun: "More gold, more damage."
- Rusty Jetpack: "Increase jump height and reduce gravity."
- Smart Shopper: "Enemies drop more gold."
#### Tier 3
- Beating Embryo: "Equipment has a 30% chance to deal double the effect."
    - Doubles *duration* on: Ocular HUD, Jade Elephant, Milky Chrysalis, Radar Scanner, Snowglobe, Pillaged Gold, Prescriptions, Safeguard Lantern.
    - Doubles *range* on: Primordial Cube, Blast Shower, Skeleton Key.
    - Doubles *count* on: The Back-up, Captain's Brooch, Sawmerang, Royal Capacitor, Recycler, Lost Doll, Gigantic Amethyst.
    - Doubles *fire rate and count* on: Disposable Missile Launcher.
    - Doubles *fire rate* on: The Crowdfunder.
    - Doubles *damage* on: Preon Accumulator.
    - Doubles *burst heal* on: Foreign Fruit, Gnarled Woodsprite.
    - Doubles *speed* on: Eccentric Vase.
    - Doubles *speed and damage* on: Volcanic Egg.
    - *Lunar* equipment will not work with Beating Embryo by default, but effects are still implemented as listed above.
- Permafrost: "Chance to freeze enemies on hit."
- Photon Jetpack: "No hands."
    - Provides flight while holding jump, using limited recharging fuel.
    - Diff. from RoR1: only provides flight after using all double jumps.
- Telescopic Sight: "Chance to instantly kill an enemy."
#### Lunar
- Old Box: "Chance to fear enemies when attacked."
#### Equipment
- Captain's Brooch: "One man's wreckage is another man's treasure."
    - Calls down an expensive first-tier item chest.
- Gigantic Amethyst: "Resets all your cooldowns."
- Pillaged Gold: "For 14 seconds, hitting enemies cause them to drop gold."
- Prescriptions: "Increase damage and attack speed for 8 seconds."
- Skeleton Key: "Open all chests in view."
    - Diff. from RoR1: limited to a 50-meter radius instead of line-of-sight.
- Snowglobe: "Randomly freeze enemies for 8 seconds."
#### Lunar Equipment
- Lost Doll: "Harm yourself to instantly kill an enemy."
    - Takes 25% of your current health to damage the closest enemy for 500% of your maximum health.
- Safeguard Lantern: "Drop a lantern that fears and damages enemies for 10 seconds."

### Other Features

- Every item added by Classic Items can be individually disabled in the mod's config file.
- More config options for various aspects of item effects (how much regen a Mysterious Vial provides, which equipments Beating Embryo affects...).
- Most config options can be changed mid-run by using TILER2's AIC console commands.
- Descriptions in the logbook match config values.
- Some vanilla tweaks, also with config options:
    - Disables the H3AD5T V2 stomp move, which is normally triggered by holding jump in midair; this is replaced by the Headstompers item.
    - Converts most pickup models for base game items and equipment into trading cards (disabled by default).

## Issues/TODO

- More items are on the way! There's a lot to work with.
- Stats are set close to RoR1's whenever possible. May eventually set up a config preset which balances items a little more carefully with respect to RoR2's existing content.
- Beating Embryo has no effect on Lunar equipments (other than those added by mods). This is a design decision, but disabled-by-default effects are planned.
- Color tags on pickup model text are too bright.
- See the GitHub repo for more!

## Modder Resources

ClassicItems exposes some members as public for use in compatibility patches in other mods, including:

- Tools to implement Beating Embryo behavior for other mods' equipment items

For details and instructions on applying these, see: https://github.com/ThinkInvis/RoR2-ClassicItems/blob/master/modding.md

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-ClassicItems/blob/master/changelog.md

**4.0.0**

- Loads and loads and loads of behind-the-scenes changes, most of which were moved to a new mod (TILER2).
- NOTE: some config entries may have different names now (mostly Beating Embryo subenable flags), which will reset them to their default values once. Check to make sure things are still how you want them!
- Added Old Box to the default AI blacklist.
- Several cosmetic fixes.
- Config mismatches are now automatically resolved (TILER2 NetConfig module).

**3.1.0**

- ADDED ITEMS: Pillaged Gold, Prescriptions, Safeguard Lantern, Old Box!
- Pickup models now have dynamically generated name/description text instead of an unreadable blur. This can be disabled for performance on low-end systems.
- Pickup models now also have a modified spin animation (so the new text stays still long enough to read).
- Improved appearance of pickup models in general.
- Lunar Equipment cards now look different from normal Equipment.
- Fixed Snake Eyes not working in multiplayer, and not applying to enemies if in AffectAll mode.
- Added a few failsafes to Boxing Gloves and Snowglobe, which were possibly conflicting with other mods.
- Beating Embryo now exposes a Compat_Register method for stopping default proc behavior on modded equipments.

**3.0.1**

- Fixed Fire Shield not having a pickup model.
- Beating Embryo now doubles the uptime of the display model of Ocular HUD.
- Behind-the-scenes: item instantiation is MUCH easier to keep track of now.
- Fixed internal item names not being set up correctly. This will reset logbook entries again (hopefully for the last time).

**3.0.0**

- ADDED ITEMS: Snowglobe, Spikestrip, Gigantic Amethyst, Fire Shield!
- Fancy new textures for item pickup models! These also use meshes instead of sprites and should look better in general.
- New disabled-by-default option to replace vanilla item pickup models with trading cards too.
- Beating Embryo now works on Milky Chrysalis (2x duration), Eccentric Vase (2x speed), Radar Scanner (2x duration), and Recycler (2x count).
- Skeleton Key and Captain's Brooch will no longer work in the Bazaar (fixes compatibility with BiggerBazaar).
- Fixed freeze debuff not having an icon.
- Added a console command for debugging: evo_setitem adds/removes items in the Artifact of Evolution item pool.

**2.4.0**

- ADDED ITEM: Permafrost!
- Fixed Barbed Wire VFX having half the intended radius.
- Fixed Barbed Wire applying damage to the Artifact Reliquary.
- Lost Doll now has proper team targeting, and should theoretically work for enemies if one manages to pick it up.
- Headstompers no longer deals self damage while Artifact of Chaos is enabled.
- Added an option (enabled by default) to pause Life Savings while the run timer is paused, e.g. in the bazaar.
- Greatly improved stability and performance while setting buff count (Golden Gun, Photon Jetpack, Snake Eyes).