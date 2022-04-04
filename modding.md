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

- `public bool ThinkInvisible.ClassicItems.Embryo.instance.CheckLastEmbryoProc(CharacterBody body -OR- EquipmentSlot slot)`: At the start of `EquipmentSlot.PerformEquipmentAction`, a single roll for Beating Embryo procs will be performed and its value stored on the EquipmentSlot. This method returns whatever the value of that roll was. Equal to 0 if no proc, 1+ if a proc occurred and the equipment effect should be doubled. Values above 1 may occur when multi-stacking is enabled in the config; it is recommended to implement linear stacking in this case (activate or increase effects by 2x, 3x, 4x, etc.).
- `public bool ThinkInvisible.ClassicItems.Scepter.instance.RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant)`: Add a SkillDef to the library of skill replacers for when an Ancient Scepter is obtained. Returns true on success.
