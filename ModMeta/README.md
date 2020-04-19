# Classic Items

## Description

Adds some items from Risk of Rain 1 which RoR2 thought it was too good for.

For nostalgic purposes only. Here Be Dragons who hoard balance issues, because the numbers are closer to RoR1's than 2's... and, y'know, Brooch+Clover. Will absolutely lead to silly, broken runs, unless you feel like changing the config up -- most aspects of each item are configurable!

### Current Additions
#### Tier 1
- Bitter Root: Gain 8% max hp.
- Headstompers: Hurt enemies by falling.
- Life Savings: Gain gold over time.
- Snake Eyes: Gain increased crit chance on failing a shrine. Removed on succeeding a shrine.
- Mysterious Vial: Increased health regeneration.
#### Tier 2
- 56 Leaf Clover: Elite mobs have a chance to drop items.
- Boxing Gloves: Hitting enemies have a 6% chance to knock them back.
- Rusty Jetpack: Increase jump height and reduce gravity.
- Smart Shopper: Enemies drop more gold.
#### Tier 3
- Beating Embryo: Equipment has a 30% chance to deal double the effect.
    - Doubles *duration* on: Ocular HUD, Jade Elephant.
    - Doubles *range* on: Primordial Cube.
    - Doubles *count* on: The Back-up, Captain's Brooch, Sawmerang, Royal Capacitor.
    - Doubles *fire rate and count* on: Disposable Missile Launcher.
    - Doubles *damage* on: Preon Accumulator.
    - Doubles *burst heal* on: Foreign Fruit, Gnarled Woodsprite.
    - Doubles *speed and damage* on: Volcanic Egg.
- Photon Jetpack: No hands.
#### Equipment
- Captain's Brooch: One man's wreckage is another man's treasure.

### Other Features
- Every item added by Classic Items can be individually disabled in the mod's config file.
- More config options for various aspects of item effects (how much regen a Mysterious Vial provides, which equipments Beating Embryo effects...).
- Descriptions in the logbook match config values.
- Some vanilla tweaks, also with config options:
    - Disables the H3AD5T V2 stomp move, which is normally triggered by holding jump in midair; this is replaced by the Headstompers item.

## Issues/TODO

- More items are on the way! There's a lot to work with.
- Item models are bigger than they should be, and are also just placeholder cards. The style's growing on me, but actual models may happen someday.
- Engineer turrets aren't affected by some items when they could/should be (e.g. Life Savings, Snake Eyes?); may also add a config option to blacklist any CI item from working on turrets.
- Life Savings doesn't match the inverse behavior at low stacks that the RoR1 version had.
- Stats are set close to RoR1's whenever possible. May eventually set up a config preset which balances items a little more carefully with respect to RoR2's existing content.
- Beating Embryo has no effect on some equipments: Blast Shower (nonfunctional), Eccentric Vase (NYI), Milky Chrysalis (nonfunctional), Radar Scanner (NYI), The Crowdfunder (NYI), Recycler (NYI).
- Beating Embryo does not update the visual effects on some other equipments despite mechanical effects being properly doubled (e.g. Ocular HUD model deactivates too early).
- Beating Embryo has no effect on Lunar equipments. This is a design decision, but disabled-by-default effects are planned.
- (May be fixed, MP-untested) In multiplayer, Captain's Brooch chests may *display* a cost of $25 to non-host players. They will still cost the right amount if purchase is attempted.
- Captain's Brooch drop point targeting is a little wonky and may put your loot up on a cliff once in a while.
- See the GitHub repo for more!

## Changelog

**1.0.2**

- Fixed Life Savings blocking teleporter completion and not working after Stage 1.
- Fixed Captain's Brooch not triggering with Artifact of Sacrifice enabled.

**1.0.1**

- Fixed Snake Eyes giving health instead of crit chance.
- Updated this document to correct nested list formatting.

**1.0.0**

- Initial version. Adds the following items to the game: Bitter Root, Headstompers, Life Savings, Snake Eyes, Mysterious Vial, 56 Leaf Clover, Boxing Gloves, Rusty Jetpack, Smart Shopper, Beating Embryo, Photon Jetpack, Captain's Brooch.