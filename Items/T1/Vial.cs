using R2API.Utils;
using RoR2;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Vial : ItemBoilerplate {
        public override string itemCodeName {get;} = "Vial";

        private ConfigEntry<float> cfgAdd;
        private ConfigEntry<bool> cfgUseIL;

        public float addRegen {get;private set;}
        public bool useIL {get;private set;}

        private bool ilFailed = false;

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgAdd = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Add"), 1.4f, new ConfigDescription(
                "Direct additive to natural health regen per stack of Mysterious Vial.",
                new AcceptableValueRange<float>(0f,int.MaxValue)));
            cfgUseIL = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "UseIL"), true, new ConfigDescription(
                "Set to false to change Mysterious Vial's effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Mysterious Vial interacts with other effects."));

            addRegen = cfgAdd.Value;
            useIL = cfgUseIL.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "vial_model.prefab";
            iconPathName = "vial_icon.png";
            RegLang("Mysterious Vial",
            	"Increased health regeneration.",
            	"Increases <style=cIsHealing>health regen by +" + addRegen.ToString("N1") + "/sec</style> <style=cStack>(+" + addRegen.ToString("N1") + "/sec per stack)</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Healing};
            itemTier = ItemTier.Tier1;
        }

        protected override void SetupBehaviorInner() {
            if(useIL) {
                IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
                if(ilFailed) {
                    IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
                    On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
                }
            } else
                On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
        }

        private void On_CBRecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);

            float RegenIncrement = addRegen * GetCount(self);
            Reflection.SetPropertyValue(self, "regen", self.regen + RegenIncrement);
        }

        private void IL_CBRecalcStats(ILContext il) {
            var c = new ILCursor(il);

            //Add another local variable to store Mysterious Vial itemcount
            c.IL.Body.Variables.Add(new VariableDefinition(c.IL.Body.Method.Module.TypeSystem.Int32));
            int locItemCount = c.IL.Body.Variables.Count-1;
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Stloc, locItemCount);

            bool ILFound;
                    
            ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchCallOrCallvirt<CharacterBody>("get_inventory"),
                x=>x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit"),
                x=>x.OpCode==OpCodes.Brfalse);

            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Call,typeof(CharacterBody).GetMethod("get_inventory"));
                c.Emit(OpCodes.Ldc_I4, (int)regIndex);
                c.Emit(OpCodes.Callvirt,typeof(Inventory).GetMethod("GetItemCount"));
                c.Emit(OpCodes.Stloc, locItemCount);
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Mysterious Vial IL patch (inventory load), falling back to event hook");
                return;
            }

            //Find (parts of): num44 = (num37 + num39 + num40 + num41 + num42) * num43, after reading baseRegen
            ILFound = c.TryGotoNext(
                x=>x.MatchLdfld<CharacterBody>("baseRegen"))
            && c.TryGotoNext(MoveType.After,
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.MatchAdd(),
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.MatchAdd(),
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.MatchAdd(),
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.MatchAdd());

            if(ILFound) {
                c.Emit(OpCodes.Ldloc, locItemCount);
                c.EmitDelegate<Func<int,float>>((icnt) => {
                    return (float)icnt * addRegen;
                });
                c.Emit(OpCodes.Add);
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Mysterious Vial IL patch (health modifier), falling back to event hook");
                return;
            }
        }
    }
}
