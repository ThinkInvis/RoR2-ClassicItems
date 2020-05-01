# ClassicItems Changelog

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