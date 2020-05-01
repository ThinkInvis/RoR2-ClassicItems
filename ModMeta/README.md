# Classic Items

## Description

Adds some items from Risk of Rain 1 which RoR2 thought it was too good for.

For nostalgic purposes only. Here Be Dragons who hoard balance issues, because the numbers are closer to RoR1's than 2's... and, y'know, Brooch+Clover. Will absolutely lead to silly, broken runs, unless you feel like changing the config up -- most aspects of each item are configurable!



### WARNING: MAKE SURE YOUR CONFIG FILE MATCHES THE HOST'S IN MULTIPLAYER!

By extension, the other clients' configs need to match too. This mod has some settings that are too difficult to change after the game has launched, so it will not automatically sync config with servers.



### Current Additions
#### Tier 1
- Barbed Wire: "Hurt nearby enemies."
- Bitter Root: "Gain 8% max hp."
- Headstompers: "Hurt enemies by falling."
- Life Savings: "Gain gold over time."
- Snake Eyes: "Gain increased crit chance on failing a shrine. Removed on succeeding a shrine."
- Mysterious Vial: "Increased health regeneration."
#### Tier 2
- 56 Leaf Clover: "Elite mobs have a chance to drop items."
- Boxing Gloves: "Hitting enemies have a 6% chance to knock them back."
- Golden Gun: "More gold, more damage."
- Rusty Jetpack: "Increase jump height and reduce gravity."
- Smart Shopper: "Enemies drop more gold."
#### Tier 3
- Beating Embryo: "Equipment has a 30% chance to deal double the effect."
    - Doubles *duration* on: Ocular HUD, Jade Elephant.
    - Doubles *range* on: Primordial Cube, Blast Shower, Skeleton Key.
    - Doubles *count* on: The Back-up, Captain's Brooch, Sawmerang, Royal Capacitor, Lost Doll.
    - Doubles *fire rate and count* on: Disposable Missile Launcher.
    - Doubles *fire rate* on: The Crowdfunder.
    - Doubles *damage* on: Preon Accumulator.
    - Doubles *burst heal* on: Foreign Fruit, Gnarled Woodsprite.
    - Doubles *speed and damage* on: Volcanic Egg.
    - *Lunar* equipment will not work with Beating Embryo by default.
- Permafrost: "Chance to freeze enemies on hit."
- Photon Jetpack: "No hands."
    - Provides flight while holding jump, using limited recharging fuel.
    - Diff. from RoR1: only provides flight after using all double jumps.
- Telescopic Sight: "Chance to instantly kill an enemy."
#### Equipment
- Captain's Brooch: "One man's wreckage is another man's treasure."
    - Calls down an expensive first-tier item chest.
- Skeleton Key: "Open all chests in view."
    - Diff. from RoR1: limited to a 50-meter radius instead of line-of-sight.
#### Lunar Equipment
- Lost Doll: "Harm yourself to instantly kill an enemy."
    - Takes 25% of your current health to damage the closest enemy for 500% of your maximum health.

### Other Features
- Every item added by Classic Items can be individually disabled in the mod's config file.
- More config options for various aspects of item effects (how much regen a Mysterious Vial provides, which equipments Beating Embryo effects...).
- Descriptions in the logbook match config values.
- Some vanilla tweaks, also with config options:
    - Disables the H3AD5T V2 stomp move, which is normally triggered by holding jump in midair; this is replaced by the Headstompers item.

## Issues/TODO

- More items are on the way! There's a lot to work with.
- Minor incompatibility with BiggerBazaar: Skeleton Key will open chests added by BiggerBazaar.
- Item models are bigger than they should be, and are also just placeholder cards. The style's growing on me, but actual models may happen someday.
- Stats are set close to RoR1's whenever possible. May eventually set up a config preset which balances items a little more carefully with respect to RoR2's existing content.
- Beating Embryo has no effect on some equipments: Eccentric Vase (NYI), Milky Chrysalis (nonfunctional), Radar Scanner (NYI), Recycler (NYI).
- Beating Embryo does not update the visual effects on some other equipments despite mechanical effects being properly doubled (e.g. Ocular HUD model deactivates too early).
- Beating Embryo has no effect on Lunar equipments (other than Lost Doll). This is a design decision, but disabled-by-default effects are planned.
- See the GitHub repo for more!

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-ClassicItems/blob/master/changelog.md

**2.4.0**

- ADDED ITEM: Permafrost!
- Fixed Barbed Wire VFX having half the intended radius.
- Fixed Barbed Wire applying damage to the Artifact Reliquary.
- Lost Doll now has proper team targeting, and should theoretically work for enemies if one manages to pick it up.
- Headstompers no longer deals self damage while Artifact of Chaos is enabled.
- Added an option (enabled by default) to pause Life Savings while the run timer is paused, e.g. in the bazaar.
- Greatly improved stability and performance while setting buff count (Golden Gun, Photon Jetpack, Snake Eyes).

**2.3.0**

- ADDED ITEMS: Skeleton Key, Lost Doll, Telescopic Sight, Barbed Wire!
- Golden Gun now applies a percentile buff displaying damage boost given as a fraction of maximum.
- Fixed Beating Embryo having a duplicate config named SubEnableSaw instead of one named SubEnableBrooch.
- Made some event hooks more stable in case of failure (original event is called first where possible).
- Updated R2API dependency to v2.4.21. ClassicItems now uses the BuffAPI and LanguageAPI submodules.

**2.2.0**

- All items can now be added to the AI blacklist from config. By default, this is enabled for: Life Savings, 56 Leaf Clover, Golden Gun, Rusty Jetpack, Smart Shopper, Photon Jetpack.
- Headstompers and Life Savings are now networked properly and should no longer act wonky if you're not the host.
- Life Savings (disabled by default), Snake Eyes, and Golden Gun can now work on deployables (e.g. Engineer turrets).
- Several other small internal bugfixes and optimizations.

**2.1.0**

- ADDED ITEM: Golden Gun!
- Beating Embryo now works on Blast Shower and The Crowdfunder.
- Beating Embryo IL patches are now slightly more stable.
- Captain's Brooch is now networked and should animate smoothly and play sound for non-host players.
- Added a missing SubmoduleDependency which could cause Captain's Brooch to break if no other mod loaded the Submodule.
- Finished incomplete IL failure fallback for Captain's Brooch, which should no longer potentially cause errors if another mod interferes with its IL patch.
- Added inverse behavior at low stacks to Life Savings. Default config options now provide (per second, by stack count): $1/3, $1/2, $1, $2, $3....
- Updated libraries for RoR2 patch #4892828.

**2.0.1**

- Fixed item disables in config not being checked.