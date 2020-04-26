# ClassicItems Changelog

**2.3.0**

Commits: 2ff06a17e1995c50140feb38bd74fc9f01a8eaa3, 689d048bc5f0119837696985052dcf230af47bc0, f86c6f15dd7cbaad22287b410182af0b4715bb07, 03831442336f7bfae43af5d2c2e2e198c720f229, 0ff4d7c410abc9e3ec08ba0ed71e90f89a47e984, eb1562a68babf6283699bee3d46c6c4ed8d52f75, 4c3e649be0eceeb3f7d29f64f3180ce40bf31dc4, ee96f5219243edab28530375acf27f869b66901c, ec38b73d58b1f5467a3e104cc719e7da7263b075, a9d005cbf3c022775f0a21e7ae9f2fb8901c38ed, 731163f588a35a226bc3d4c49a44296cdace5143, c9928ffd10143b7c19b8554a3c16b3b742837a16, 98ddd54c2c29d7072f5b34ec02ccfaef932cfbfe, fa44469949a62dd8b1857c47b5034dbca2a52f82, aa65f46109e92cc8f993e5818f29c8cbe4a7d49d, a4a43df342b86e3ad05ca39b7d5887c3c5758dc7, d9c238e2b347736191d8a20422dbb127993626b0, b7e965cd5c643a2ad67d45364dbffe0198210a3f, 052c4564f747faf71c5f63ac6901977d2a0e2557, 3aa5902200cd6f541e9b5ee41889c4183395eb9d, 267c814e2d8b31208d8dbdda4072b6c0daebc5bb, b2ac78f4fa9fc101fda198591671bde815d076a1, 76296a3e9b998322d8ab7eda189ac9e4ce6e9a05, 40ba17aeae643879575545253b5207c856d39fbb, bc9ad4f7033b81c53aca73484ce0627655015db0, 849732aded4bb80d189eb043eb0919c004eaaf7e, 017fb8cf9857761313483a80ed85286841ab9e3b

- ADDED ITEMS: Skeleton Key, Lost Doll, Telescopic Sight, Barbed Wire!
- Golden Gun now applies a percentile buff displaying damage boost given as a fraction of maximum.
- Fixed Beating Embryo having a duplicate config named SubEnableSaw instead of one named SubEnableBrooch.
- Made some event hooks more stable in case of failure (original event is called first where possible).
- Updated R2API dependency to v2.4.21. ClassicItems now uses the BuffAPI and LanguageAPI submodules.

**2.2.0**

Commits: 7af7622245f39d2bf0aa74c66ad18a9e83ea1515, e1658029596fc64eb715f40554c0f9f34698de32, 96ac28834133e6b4a1c48b022adfc28c477a9d67, e27621e5b1f527ce231f2fc2184bb950eb2678ac, e729049a1148bd4626823f09c0d2e787a493e66e, 3905b8ebbe6bea92e197c43238149c966208f7ac, ef177c5f7ce95a65e74c535c099eeaa597fce34a, 41406ef96a528f267b460d5b579aeef144bc2247, 3d09724b02986511dcf0d05b56ce4f9e0d8184f3, fa3ffb2e107ca65ff4070d0dc998078554075385, 9254fc39f7ced185cb356e3fd45d2fc67e214e61

- All items can now be added to the AI blacklist from config. By default, this is enabled for: Life Savings, 56 Leaf Clover, Golden Gun, Rusty Jetpack, Smart Shopper, Photon Jetpack.
- Headstompers and Life Savings are now networked properly and should no longer act wonky if you're not the host.
- Life Savings (disabled by default), Snake Eyes, and Golden Gun can now work on deployables (e.g. Engineer turrets).
- Several other small internal bugfixes and optimizations.

**2.1.0**

Commits: 4257d252ce5340f564b4b76b023ea5314f4483a9, 186184d64c73456663350566177007bfc58286f3, 5d334c6339ceeb25c3c33d3bc18be4db4f705f47, 9b1c85fb82063cc1685518dbeb908ec51475b4b2, 1b083be1d0d696b1ea71671bcc1aaaf1623a60bc, 7616cb703bac738cba228f8cfaaec2eba07a1fcc, bed8e9fec414688a8898b8b3a40b23fcca3f4ab1, 06612c023a1204a577725155ba9959e482f82c3c, 7b2f65a07f1d3c9d4933cb08022f733e2436f952, 57fd052271841907f5c1e5b591f4e344f82e618c, 4cf5efa93a230255ddee7d30289a57e86f48b236, 63f3fbc61ab3d1e2a1d752f4b76ee99a1fdad705, 8ccb20d2fa9e19d38a86073187216827d9e0386c, 6bf51c38420a093d87e0e478543e25c7c5618869, 8e4e7062478c653b725a7d9338b0879292d63713, c879fa0b77e6ed696afb1052b607371928de7101, c156a4703b6ddbe523800e545bc3e1f2dee06dca

- ADDED ITEM: Golden Gun!
- Beating Embryo now works on Blast Shower and The Crowdfunder.
- Beating Embryo IL patches are now slightly more stable.
- Captain's Brooch is now networked and should animate smoothly and play sound for non-host players.
- Added a missing SubmoduleDependency which could cause Captain's Brooch to break if no other mod loaded the Submodule.
- Finished incomplete IL failure fallback for Captain's Brooch, which should no longer potentially cause errors if another mod interferes with its IL patch.
- Added inverse behavior at low stacks to Life Savings. Default config options now provide (per second, by stack count): $1/3, $1/2, $1, $2, $3....
- Updated libraries for RoR2 patch #4892828.

**2.0.1**

Commits: 31421da0b3ece448e1f8498a3d229822c9c35acb

- Fixed item disables in config not being checked.

**2.0.0**

Commits: bdac899fb03052ac3cbdb344307df75a681a3971, 50bc2c6b5a41e61a89f7ac4b3b42cfdf85ac1166, 159e43e48b2c4614d174632bb2bcf572f33ce518, 8d2a1a68eb6d465877eee78cc8806a25fbe9ace9

- Life Savings will now work if the ShareSuite mod is installed.
- Captain's Brooch should now display the correct cost to clients in multiplayer, and has much better ground targeting!
- Started using language tokens instead of direct string loading. May fix an unconfirmed issue with logbook entries resetting on mod uninstall; will definitely reset existing logbook entries for CI items, as their internal names have changed.

**1.0.3**

Commits: f41e092e8454ca1d343f8ed52b17123bbd893649, 9c519e1aadf99195711e31e3b46e955bbbf1b627, 362c900ae688c2d03700d86f18e04cc1b00592e4, 6583cbcbd7880aeb7dd08273ee0cc3a9c41e80a5

- Fixed incompatibility with BrokenMagnet (and with any other mod that includes an AssetBundle with an internal name of "exampleitemmod")

**1.0.2**

Commits: 56eaa6ba4df45b5f2e635c2ea571891f419ed1df, 85d2059787d4943566489951542963a97c880efa, 19c03ca701ed814cd1dc72860ab11bfc11677e5e, 73c177115b339a172c7a92dca54b4939820c2550

- Fixed Life Savings blocking teleporter completion and not working after Stage 1.
- Fixed Captain's Brooch not triggering with Artifact of Sacrifice enabled.

**1.0.1**

Commits: 0a3fd34aa8e94522374defd202e8f5a633d81236

- Fixed Snake Eyes giving health instead of crit chance.

**1.0.0**

Commits: 9bd2c4ab6ebcc9c9b1589babb9f9cb2ae6fd5197, cae7ea2c79c4dd5b7e849f1a309143452828e7b0

- Initial version. Adds the following items to the game: Bitter Root, Headstompers, Life Savings, Snake Eyes, Mysterious Vial, 56 Leaf Clover, Boxing Gloves, Rusty Jetpack, Smart Shopper, Beating Embryo, Photon Jetpack, Captain's Brooch.