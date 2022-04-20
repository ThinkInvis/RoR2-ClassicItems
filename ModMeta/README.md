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
- Taser: "Chance to snare on hit."
#### Tier 2
- 56 Leaf Clover: "Elite mobs have a chance to drop items."
- Boxing Gloves: "Hitting enemies have a 6% chance to knock them back."
- Filial Imprinting: "Hatch a strange creature who drops buffs periodically."
    - Diff. from RoR1: now provides buffs instantly, instead of on-ground drops from pets.
- Golden Gun: "More gold, more damage."
- Rusty Jetpack: "Increase jump height and reduce gravity."
- Smart Shopper: "Enemies drop more gold."
#### Tier 3
- Ancient Scepter: "Upgrades your 4th skill."
    <details><summary>Specific skill details...</summary>

    - Commando: Suppressive Fire > Death Blossom (2x shots, fire rate, and accuracy)  -OR-  Grenade > Carpet Bomb (0.5x damage, throw a spread of 8 at once)

    - Huntress: Arrow Rain > Burning Rain (1.5x duration and radius, burns)  -OR-  Ballista > Rabauld (5 extra weaker projectiles per shot, for 2.5x TOTAL damage)

    - MUL-T: Transport Mode > Breach Mode (0.5x incoming damage, 2x duration; after stopping, retaliate with a stunning explosion for 100% of unmodified damage taken)

    - Engineer: TR12 Gauss Auto-Turret > TR12-C Gauss Compact (+1 stock, +1 placed turret cap)  -OR-  TR58 Carbonizer Turret > TR58-C Carbonizer Mini (+2 stock, +2 placed turret cap)

    - Artificer: Flamethrower > Dragon's Breath (hits leave lingering fire clouds)  -OR-  Ion Surge > Antimatter Surge (2x damage, 4x radius)

    - Mercenary: Eviscerate > Massacre (2x duration, kills refresh duration)  -OR-  Slicing Winds > Gale-Force (4x stock and recharge speed, fires all charges at once)

    - REX: Tangling Growth > Chaotic Growth (2x radius, pulses additional random debuffs)

    - Loader: Charged Gauntlet > Megaton Gauntlet (2x damage and lunge speed, 7x knockback)  -OR-  Thunder Gauntlet > Thundercrash (3x lightning bolts fired, cone AoE becomes sphere)

    - Acrid: Epidemic > Plague (victims become walking sources of Plague, chains infinitely)

    - Captain: Orbital Probe > 21-Probe Salute (1/3 damage, 7x shots, hold primary to fire continuously)
    </details>
- Beating Embryo: "Equipment has a 30% chance to deal double the effect."
    - Has a config option to enable multiple stacks for x3, x4, etc.
    - Supports mod-added equipment, but will not do anything by default; item mods have to specify what should happen.
- Permafrost: "Chance to freeze enemies on hit."
- Photon Jetpack: "No hands."
    - Provides flight while holding jump, using limited recharging fuel.
    - Diff. from RoR1: only provides flight after using all double jumps.
- Telescopic Sight: "Chance to instantly kill an enemy."
- The Hit List: "Killing marked enemies permanently increases damage."
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

## Issues/TODO

- Item models are a quick first pass -- improvements may be made.
    - Item displays may also happen at some point.
- Stats are set close to RoR1's whenever possible. May eventually set up a config preset which balances items a little more carefully with respect to RoR2's existing content.
- Beating Embryo has no effect on Lunar equipments (other than those added by mods). This is a design decision, but disabled-by-default effects are planned.
- As of SotV, new characters, and some new skills for old characters, are missing Ancient Scepter skills.
- See the GitHub repo for more!

## Modder Resources

ClassicItems exposes some members as public for use in compatibility patches in other mods, including:

- Tools to implement Beating Embryo and Ancient Scepter behavior for other mods' equipment items and characters

For details and instructions on applying these, see: https://github.com/ThinkInvis/RoR2-ClassicItems/blob/master/modding.md

## Changelog

The 5 latest updates are listed below. For a full changelog, see: https://github.com/ThinkInvis/RoR2-ClassicItems/blob/master/changelog.md
(🌧︎: Involves an accepted GitHub Pull Request from the community. Thanks for your help!)

**6.2.0**

- Cross-mod support for the new version of Beating Embryo has been finalized! Check modding.md for instructions for developers.
- Pillaged Gold amount per hit is now configurable.
- Reworked Life Savings internal mechanics to hopefully attrite some very evasive bugs out of existence.
- Implemented Risk Of Options support on ALL items/equipments, via new AutoConfig attributes in TILER2 7.
- Removed some deprecated ItemStats and BetterUI support.
- Updated for latest Risk of Rain 2 version.
- Updated TILER2 dependency to 7.0.0.

**6.1.2**

- Fixed missing icon on Ancient Scepter. Again. For real this time I promise.
- Beating Embryo:
	- Fixed guaranteed proc count calculation being 100x the intended value.
	- Fixed unclosed style tag in every single tooltip append. To any HTML developers affected by this: I'm so sorry.
	- Fixed tooltip appends causing language tokens to display instead of the language they point to.
	- Tooltip appends are now applied dynamically when tooltip text is retrieved, instead of overriding language tokens.
		- This extra tooltip text describing how Beating Embryo works on a specific equipment will now only appear if you have a Beating Embryo.
		- Should improve mod compatibility.
- Updated R2API dependency to 4.3.5.
- Updated TILER2 dependency to 6.2.0.

**6.1.1**

- Fixed Embryo LifestealOnHit hook causing rampant NRE spam.

**6.1.0**

- Reimplemented Beating Embryo for internal items. Beating Embryo is no longer force-disabled.
    - API for adding functionality to other mods' items is not yet restored.
- Fixed missing icon on Ancient Scepter.
- Ancient Scepter now has a config option to allow other mods to override its default skill replacers (enabled by default).
- Internal AssetBundle is now exposed for other mods to reference.
- Migrated GatherEnemies util method to TILER2.
- Updated TILER2 dependency to 6.1.2.

**6.0.0**

- Item models!
	- We got 'em!
	- They're not very good, but we got 'em!
- Removed Headstompers.
	- These were too close in functionality to the H3AD-5T v2.
	- Also, I really didn't want to model a boot.
- Removed the trading card item model override feature.
	- This will be migrated to a standalone mod in the near future.
- Fixed Barbed Wire, Snowglobe, et. al. not working on Void/Lunar enemies.
- Updated TILER2 dependency to 6.0.2.