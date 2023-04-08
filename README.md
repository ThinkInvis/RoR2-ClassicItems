### DEPRECATED: Development on this project has been halted in favor of more original projects (e.g. Tinker's Satchel). Recommended alternative: [Lost In Transit](https://thunderstore.io/package/LostInTransitTeam/LostInTransit/)

# ClassicItems

A mod for Risk of Rain 2. Built with BepInEx and R2API.

Adds a bunch of custom items, all based on ones which didn't make the cut from RoR1.

## Installation

Release builds are published to Thunderstore: https://thunderstore.io/package/ThinkInvis/ClassicItems/

**Use of a mod manager is recommended**. If not using a mod manager: extract ThinkInvis-ClassicItems-[version].zip into your BepInEx plugins folder such that the following path exists: `[RoR2 game folder]/BepInEx/Plugins/ThinkInvis-ClassicItems-[version]/ClassicItems.dll`.
Installation of TILER2 is also required: https://thunderstore.io/package/ThinkInvis/TILER2/

## Building

Building ClassicItems locally will require setup of the postbuild event:
- The middle 3 xcopy calls need to either be updated with the path to your copy of RoR2, or removed entirely if you don't want copies of the mod moved for testing.
- Installation of Weaver (postbuild variant) is left as an exercise for the user. https://github.com/risk-of-thunder/R2Wiki/wiki/Networking-with-Weaver:-The-Unity-Way

You may also need to change the reference path to TILER2, which is expected to be built in a parallel solution folder.
