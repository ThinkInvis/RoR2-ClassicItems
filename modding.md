## Tools for Modders

### Tutorial: Refererencing Other Mods

To use public members of another mod in your own, you must perform ALL of these steps:

- Add the other mod's DLL as a dependency in your VS project.
- Add `[BepInExDependency("full.mod.id", BepInDependency.DependencyFlags.SoftDependency)]` to your main plugin to ensure that it always loads after the other mod, if the other mod is present.
- Put any code referencing any part of the other mod in a standalone method with the following attribute: `[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]` (requres `using System.Runtime.CompilerServices;`). This will prevent the compiler from trying to access the other mod's code when it might not exist.
- **DO NOT** use any of these standalone methods without checking for `BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("full.mod.id")` first.

See [this source file](Compat/ShareSuite.cs) for a partial example.

### Documentation

The following public members of ClassicItems are explicitly intended to help increase compatibility with other mods.

- `public void ThinkInvisible.ClassicItems.Embryo.instance.Compat_Register(EquipmentIndex ind)`: Calling this on your mod's equipment will stop Beating Embryo from naively triggering the equipment twice when it procs with SubEnableModded on.
- `public bool ThinkInvisible.ClassicItems.Embryo.instance.CheckProc(CharacterBody body)`: Returns true if Embryo is enabled and a roll against proc chance * CharacterBody's item count succeeds. Use in combination with Compat_Register to set up your own proc behavior.
- `public bool ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, int targetSlot, int targetVariant)`: Add a SkillDef to the library of skill replacers for when an Ancient Scepter is obtained. Returns true on success.