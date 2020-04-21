# ClassicItems Changelog

**2.1.0**

Commits: 4257d252ce5340f564b4b76b023ea5314f4483a9, 186184d64c73456663350566177007bfc58286f3, 5d334c6339ceeb25c3c33d3bc18be4db4f705f47, 9b1c85fb82063cc1685518dbeb908ec51475b4b2, 1b083be1d0d696b1ea71671bcc1aaaf1623a60bc, 7616cb703bac738cba228f8cfaaec2eba07a1fcc, bed8e9fec414688a8898b8b3a40b23fcca3f4ab1, 06612c023a1204a577725155ba9959e482f82c3c, 7b2f65a07f1d3c9d4933cb08022f733e2436f952, 57fd052271841907f5c1e5b591f4e344f82e618c, 4cf5efa93a230255ddee7d30289a57e86f48b236, 63f3fbc61ab3d1e2a1d752f4b76ee99a1fdad705, 8ccb20d2fa9e19d38a86073187216827d9e0386c, 6bf51c38420a093d87e0e478543e25c7c5618869, 8e4e7062478c653b725a7d9338b0879292d63713

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