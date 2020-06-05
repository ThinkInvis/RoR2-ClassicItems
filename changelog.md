# ClassicItems Changelog

**4.2.3**

- Main plugin now uses TILER2.AutoItemConfig for global configs. Note that **these config entries will reset to defaults once after updating ClassicItems** (names/categories have changed).
- Moved some Captain's Brooch code (nodegraph cleanup) into TILER2.
- (From TILER2 update to v1.4.0) Fixed some items applying incorrect types of damage bonus.

**4.2.2**

- Boxing Gloves now has an option to disable affecting bosses.
- Made Golden Gun's second IL patch target slightly more lenient (fixes compatibility with GeneralFixes mod).
- Now uses plugin-specific console logger.

**4.2.1**

- Logbook setup stage no longer completely breaks if an item (in ANY mod) has no model.
- Mysterious Vial: fixed buff being applied during the wrong part of calculations (multiplier instead of base value).
- Index dump during game startup is now much prettier.
- Internal buff names are now consistent with item names and with each other.
- Migrated most RecalculateStats IL patches to TILER2, as well as some extension methods. Most UseIL config settings have been removed as a consequence, possibly temporarily.
- GitHub repo is now licensed (GNU GPL3).

**4.2.0**

- ADDED ITEMS: Taser, Filial Imprinting, The Hit List!
- Fixed an issue where Barbed Wire, Snowglobe, and Safeguard Lantern were not performing team filtering correctly. This caused these items to fail to work properly when used by non-players, or by anyone if Artifact of Chaos was enabled.
- (From TILER2 update to v1.2.1) All relevant items now use run-seeded RNG instead of always using the same seed (0).

**4.1.3**

- Barbed Wire: Fixed aura being removed after stage changes.
- 56 Leaf Clover: Now has a disabled-by-default option to allow deployables to count towards global Clover stacks.
- Ancient Scepter/Massacre: Now filters kills by maximum Eviscerate range. Kills farther than this range will not refresh Massacre duration.
- Ancient Scepter/Chaotic Growth: Fixed range indicator's radius not being changed.
- Ancient Scepter/both Engi skills: Now has a disabled-by-default option to decrease cooldown to match the additional stock (such that total recharge remains unchanged).
- (From TILER2 update to v1.1.1) All equipments now have configurable cooldown.

**4.1.2**

- Ancient Scepter: Non-upgraded Eviscerate no longer resets duration on kill.
- Ancient Scepter: Fixed several other cases where having negative item count could cause skills to act like their scepter replacements.
- Gigantic Amethyst: restored cooldown to 8s from incorrect 45s.

**4.1.1**

- Ancient Scepter: Skill lookup is now more stable. It should no longer replace the wrong skill if another mod switches slots around, e.g. by adding a new passive variant.
- Ancient Scepter/Gale-Force: now works as described, instead of only firing one charge at a time.
- Ancient Scepter/Rabauld: now fires bursts of 6 shots with three-tenths damage on all but the first, instead of having 4x manual fire rate and count but half damage.

Late notes as of next version:

- Public API change: Parameter `skillSlot` in method `Scepter.RegisterScepterSkill` now takes a RoR2.SkillSlot instead of an int.

**4.1.0**

- ADDED ITEM: Ancient Scepter! Has skill overrides for ALL playable characters, and for each variant per chosen slot, but likely needs a balance pass or two.

**4.0.0**

- Loads and loads and loads of behind-the-scenes changes, most of which were moved to a new mod (TILER2).
- Added Old Box to the default AI blacklist.
- Several cosmetic fixes.
- Config mismatches are now automatically resolved (TILER2 NetConfig module).

**3.1.0**

- ADDED ITEMS: Pillaged Gold, Prescriptions, Safeguard Lantern, Old Box!
	- Pillage: [#11](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/ab18becc265c16b0de50e6408373d7b548aa96b6), [#19](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/dc0393d0163ce659c0e96740f3eb052d99f9ac58), [#20](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/874d80c40784cc9ae21a5bf6e530ee342708efe0)
	- Prescriptions: [#12](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/b3cd17c851b08f21cd4f4200a9aed6e7db76d71a), [#25](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/cc11c3db7df2abb5b28e68d1be7477aa9d0a2809)
	- Lantern: [#14](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/4e5b7e1a14ffea60aab3efa27ce4f9b11640fb98)
	- OldBox: [#16](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3133342755a18c8becb05ea9a6b5a68e1f86dbf7)
	- All assets added in [#17](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/52611d7944423f5ff3de0e1d8c0cb86f992e2dda)
- Pickup models now have dynamically generated description text instead of an unreadable blur. This can be disabled for performance on low-end systems. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/9d0f1fef5ce2f665c2fdbc2f2ca14bf67c0e4b67), [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/5bb42f899fd1bbbe15a41c4c2da3a27bb7ff778b), [#9](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/03d714ceccd11d58119c86dac97dd1c8a13f74b3), [#21](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3d6eba8645f80a4344e4e5d3d88f17742e44f045)
- Pickup models now also have a modified spin animation (so the new text stays still long enough to read). [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7f594d259f7cf5407a45973622c7a508790670e1)
- Improved appearance of pickup models. [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/5bb42f899fd1bbbe15a41c4c2da3a27bb7ff778b)
- Lunar Equipment cards now look different from normal Equipment. [#18](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/f60365625bb565e5c5585d434d3bbd819bf0b725)
- Fixed Snake Eyes not working in multiplayer, and not applying to enemies if in AffectAll mode. [#23](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/8377c8d51f75c8d9d4e2f07ca5b451e353faba25)
- Added a few failsafes to Boxing Gloves and Snowglobe, which were possibly conflicting with other mods. [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/cf75bfca89b2035f9f414cd6283379b84aef51cf)
- Fixed typo in Spikestrip description. [#7](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3df242060f1a3a2df99def28c05107249d0fb90d)
- Beating Embryo now exposes a Compat_Register method for stopping default proc behavior on modded equipments. [#13](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/65af16eacf3cd97bd6662e909fedf635f5a03a72)
- Other minor commits: [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/706b2e7ae2e92a964acec44e3cd03242866eb26f), [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/2ebadb6c25b18aeb5453ef2022d1a18da1141d1c), [#8](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7674f18036dd777e7699c8249d4f95b536ee9c14), [#10](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/181585fcb56d065734011c51dbe0536d207f2cd5), [#15](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/a96d429e4aec4e9be47d759abca98cf4bf15ff20), [#22](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/de542721d8c3d6cd7f376e981045ae8e57ec077c), [#24](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/097fb9e693ed53670b8df6d475efbecce0bd73db).

**3.0.1**

- Fixed Fire Shield not having a pickup model. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/987b53224662cd571335b404ba6a867f48b559d1)
- Beating Embryo now doubles the 'active' time of the display model of Ocular HUD. [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/cc0672bd03baa0f289f4c6d61e2fe6210a5d3e42)
- Behind-the-scenes: item instantiation is MUCH easier to keep track of now. [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/1d4b8d6173ac1902d04546b8641df05527e380b5)
- Fixed internal item names not being set up correctly. This will reset logbook entries again (hopefully for the last time). [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e7f932fd401e770931a3deeefec0f2e48b6731db), [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/721330042710c4e6912cf6b72da8baca42c0073d)
- Other minor commits: [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7283174731ef5a6105f6a4c0ddbcf29cac4aef4b)

**3.0.0**

- ADDED ITEMS: Snowglobe, Spikestrip, Gigantic Amethyst, Fire Shield!
	- Snowglobe: [#11](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/4d0b6b4eb88c1f92ced8cd8545e4030c870ef570), [#13](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/65cc276c195570940c451caa617c467c41b67556), [#14](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/c29a8096719bacf6fb7effc728b7887c1c17c4ef), [#16](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3d7f0aba3c6ed634162645b8dcb8cedb78451795), [#19](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/ade1fefff8116c9771f2b83e1572257cbd446348)
	- Spikestrip: [#18](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e2972ec4ff504cb34bc0db82fa873419d228ff51)
	- Amethyst: [#20](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/23c64deb9be06594996ab55f6c49dd493dfbb964)
	- FireShield: [#21](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/23b15f2e87cf798927fd877453e3eeec79023b05)
- Fancy new textures for item pickup models! These also use meshes instead of sprites and should look better in general. [#12](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7a7f56c897a4f46b3987497b71d67cbb9227e926)
- New disabled-by-default option to replace vanilla item pickup models with trading cards too. [#22](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e4ed57b5377a9103f4031fe52a5a7b687d1162c1)
- Beating Embryo now works on Milky Chrysalis (2x duration), Eccentric Vase (2x speed), Radar Scanner (2x duration), and Recycler (2x count).
	- Chyrsalis/Vase: [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/124f3256a93f6f1d2eb9c8fd496823d645ff0b38), [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/a23b22a8c70b7678acdc926ddb043e32b6fc506b), [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/be973a515d8e66f572b7f76b209ae8cdd0a7da7b)
	- Scanner: [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/fc8ec43c375f67cd6c3eb759fb41a8511be4ad40), [#7](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e2838f540ccfe2568bb29d03f7a8a02297eaabce)
	- Recycler: [#8](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/d344347d189f8006d3080d9ccaab350e3234474a)
- Skeleton Key and Captain's Brooch will no longer work in the Bazaar (fixes compatibility with BiggerBazaar). [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/644ca908193acfa195e20dc6b80656f486f678a1)
- Fixed freeze debuff not having an icon. [#9](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/a0e4eae4a0cf44f6280b078365ac6ee4bd0d2185), [#17](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/89a93cfdf4dd0d2c7a4e113fd08fd2b135e7d6d2)
- Added a console command for debugging: evo_setitem adds/removes items in the Artifact of Evolution item pool. [#10](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/8b064addc3912a9740a40c345eb31094205d264f)
- Other minor commits: [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/0d9c4b2fe76f6c496e7c23f9de97bb3218e9de19), [#15](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7f53fd5dcdcb2b06909e536d39a5ba22415dd090).

**2.4.0**

- ADDED ITEM: Permafrost! [#8](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/41e2bf864f0367af1c2d5bc5cc96a6c679eb6e43)
- Fixed Barbed Wire VFX having half the intended radius. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/ad124b5507390ba727dcf4a4500ccb8f9979bdd9)
- Fixed Barbed Wire applying damage to the Artifact Reliquary. [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/de6a1d8959c846340baedca813528d867a06eebe)
- Lost Doll now has proper team targeting, and should theoretically work for enemies if one manages to pick it up. [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/b3a44af078b988491e4d090c772b61f21dbcd2ca)
- Headstompers no longer deals self damage while Artifact of Chaos is enabled. [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/48938d13a36a2cd1cce9a260cb523e2fefd60fb5)
- Added an option (enabled by default) to pause Life Savings while the run timer is paused, e.g. in the bazaar. [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/21f0fca735b676d9f51a6b40048e3ce300c0eca9)
- Greatly improved stability and performance while setting buff count (Golden Gun, Photon Jetpack, Snake Eyes). [#7](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/6a4a1e0efad5c3bebcd053898cd3483730eac38a)
- Other minor commits: [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/be7c0881a0173bacd35c4c26360aa2034efcded6), [#9](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/a6cd7150bb36805fad64cf5039d5106d9d40e939).

**2.3.0**

- ADDED ITEMS: Skeleton Key, Lost Doll, Telescopic Sight, Barbed Wire!
    - SkelKey: [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/2ff06a17e1995c50140feb38bd74fc9f01a8eaa3), [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/0ff4d7c410abc9e3ec08ba0ed71e90f89a47e984), [#7](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/4c3e649be0eceeb3f7d29f64f3180ce40bf31dc4), [#8](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/ee96f5219243edab28530375acf27f869b66901c), [#9](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/ec38b73d58b1f5467a3e104cc719e7da7263b075), [#10](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/a9d005cbf3c022775f0a21e7ae9f2fb8901c38ed), [#18](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/b7e965cd5c643a2ad67d45364dbffe0198210a3f)
	- LostDoll: [#19](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/052c4564f747faf71c5f63ac6901977d2a0e2557), [#20](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3aa5902200cd6f541e9b5ee41889c4183395eb9d), [#26](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/849732aded4bb80d189eb043eb0919c004eaaf7e)
	- TeleSight: [#22](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/b2ac78f4fa9fc101fda198591671bde815d076a1), [#23](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/76296a3e9b998322d8ab7eda189ac9e4ce6e9a05)
	- BarbedWire: [#24](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/40ba17aeae643879575545253b5207c856d39fbb), [#25](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/bc9ad4f7033b81c53aca73484ce0627655015db0), [#28](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/017fb8cf9857761313483a80ed85286841ab9e3b)
- Golden Gun now applies a percentile buff displaying damage boost given as a fraction of maximum. [#16](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/a4a43df342b86e3ad05ca39b7d5887c3c5758dc7), [#27](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/53874ee861af369fd17523fc4b91a6165a2f2e57)
- Fixed Beating Embryo having a duplicate config named SubEnableSaw instead of one named SubEnableBrooch. [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/f86c6f15dd7cbaad22287b410182af0b4715bb07)
- Made some event hooks more stable in case of failure (original event is called first where possible). [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/689d048bc5f0119837696985052dcf230af47bc0)
- Updated R2API dependency to v2.4.21. ClassicItems now uses the BuffAPI and LanguageAPI submodules. [#11](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/731163f588a35a226bc3d4c49a44296cdace5143), [#13](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/98ddd54c2c29d7072f5b34ec02ccfaef932cfbfe), [#14](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/fa44469949a62dd8b1857c47b5034dbca2a52f82), [#15](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/aa65f46109e92cc8f993e5818f29c8cbe4a7d49d)
- Other minor commits: [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/03831442336f7bfae43af5d2c2e2e198c720f229), [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/eb1562a68babf6283699bee3d46c6c4ed8d52f75), [#12](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/c9928ffd10143b7c19b8554a3c16b3b742837a16), [#17](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/d9c238e2b347736191d8a20422dbb127993626b0), [#21](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/267c814e2d8b31208d8dbdda4072b6c0daebc5bb).

**2.2.0**

- All items can now be added to the AI blacklist from config. By default, this is enabled for: Life Savings, 56 Leaf Clover, Golden Gun, Rusty Jetpack, Smart Shopper, Photon Jetpack. [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e1658029596fc64eb715f40554c0f9f34698de32)
- Headstompers and Life Savings are now networked properly and should no longer act wonky if you're not the host. [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e27621e5b1f527ce231f2fc2184bb950eb2678ac), [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3905b8ebbe6bea92e197c43238149c966208f7ac)
- Life Savings (disabled by default), Snake Eyes, and Golden Gun can now work on deployables (e.g. Engineer turrets). [#8](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/41406ef96a528f267b460d5b579aeef144bc2247), [#10](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/fa3ffb2e107ca65ff4070d0dc998078554075385), [#11](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/9254fc39f7ced185cb356e3fd45d2fc67e214e61)
- Other minor commits: [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7af7622245f39d2bf0aa74c66ad18a9e83ea1515), [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/96ac28834133e6b4a1c48b022adfc28c477a9d67), [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/e729049a1148bd4626823f09c0d2e787a493e66e), [#7](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/ef177c5f7ce95a65e74c535c099eeaa597fce34a), [#9](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/3d09724b02986511dcf0d05b56ce4f9e0d8184f3).

**2.1.0**

- ADDED ITEM: Golden Gun! [#10](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/57fd052271841907f5c1e5b591f4e344f82e618c), [#11](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/4cf5efa93a230255ddee7d30289a57e86f48b236), [#12](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/63f3fbc61ab3d1e2a1d752f4b76ee99a1fdad705)
- Beating Embryo now works on Blast Shower and The Crowdfunder. [#5](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/1b083be1d0d696b1ea71671bcc1aaaf1623a60bc), [#9](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7b2f65a07f1d3c9d4933cb08022f733e2436f952)
- Beating Embryo IL patches are now slightly more stable. [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/9b1c85fb82063cc1685518dbeb908ec51475b4b2), [#6](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/7616cb703bac738cba228f8cfaaec2eba07a1fcc), [#7](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/bed8e9fec414688a8898b8b3a40b23fcca3f4ab1)
- Captain's Brooch is now networked and should animate smoothly and play sound for non-host players. [#14](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/6bf51c38420a093d87e0e478543e25c7c5618869), [#16](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/c879fa0b77e6ed696afb1052b607371928de7101), [#17](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/c156a4703b6ddbe523800e545bc3e1f2dee06dca)
- Added a missing SubmoduleDependency which could cause Captain's Brooch to break if no other mod loaded the Submodule. [#15](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/8e4e7062478c653b725a7d9338b0879292d63713)
- Finished incomplete IL failure fallback for Captain's Brooch, which should no longer potentially cause errors if another mod interferes with its IL patch. [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/186184d64c73456663350566177007bfc58286f3)
- Added inverse behavior at low stacks to Life Savings. Default config options now provide (per second, by stack count): $1/3, $1/2, $1, $2, $3.... [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/4257d252ce5340f564b4b76b023ea5314f4483a9), [#8](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/06612c023a1204a577725155ba9959e482f82c3c)
- Updated libraries for RoR2 patch #4892828. [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/5d334c6339ceeb25c3c33d3bc18be4db4f705f47)
- Other minor commits: [#13](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/8ccb20d2fa9e19d38a86073187216827d9e0386c).

**2.0.1**

- Fixed item disables in config not being checked. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/31421da0b3ece448e1f8498a3d229822c9c35acb)

**2.0.0**

- Life Savings will now work if the ShareSuite mod is installed. [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/8d2a1a68eb6d465877eee78cc8806a25fbe9ace9)
- Captain's Brooch should now display the correct cost to clients in multiplayer, and has much better ground targeting! [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/159e43e48b2c4614d174632bb2bcf572f33ce518)
- Started using language tokens instead of direct string loading. May fix an unconfirmed issue with logbook entries resetting on mod uninstall; will definitely reset existing logbook entries for CI items, as their internal names have changed. [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/50bc2c6b5a41e61a89f7ac4b3b42cfdf85ac1166)
- Other minor commits: [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/bdac899fb03052ac3cbdb344307df75a681a3971).

**1.0.3**

- Fixed incompatibility with BrokenMagnet (and with any other mod that includes an AssetBundle with an internal name of "exampleitemmod"). [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/f41e092e8454ca1d343f8ed52b17123bbd893649), [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/362c900ae688c2d03700d86f18e04cc1b00592e4)
- Other minor commits: [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/9c519e1aadf99195711e31e3b46e955bbbf1b627), [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/6583cbcbd7880aeb7dd08273ee0cc3a9c41e80a5).

**1.0.2**

- Fixed Life Savings blocking teleporter completion and not working after Stage 1. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/56eaa6ba4df45b5f2e635c2ea571891f419ed1df)
- Fixed Captain's Brooch not triggering with Artifact of Sacrifice enabled. [#3](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/19c03ca701ed814cd1dc72860ab11bfc11677e5e)
- Other minor commits: [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/85d2059787d4943566489951542963a97c880efa), [#4](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/73c177115b339a172c7a92dca54b4939820c2550).

**1.0.1**

- Fixed Snake Eyes giving health instead of crit chance. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/0a3fd34aa8e94522374defd202e8f5a633d81236)

**1.0.0**

- Initial version. Adds the following items to the game: Bitter Root, Headstompers, Life Savings, Snake Eyes, Mysterious Vial, 56 Leaf Clover, Boxing Gloves, Rusty Jetpack, Smart Shopper, Beating Embryo, Photon Jetpack, Captain's Brooch. [#1](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/9bd2c4ab6ebcc9c9b1589babb9f9cb2ae6fd5197), [#2](https://github.com/ThinkInvis/RoR2-ClassicItems/commit/cae7ea2c79c4dd5b7e849f1a309143452828e7b0)