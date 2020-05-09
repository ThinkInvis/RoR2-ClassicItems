using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class BitterRoot : Item<BitterRoot> {
        public override string displayName => "Bitter Root";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Healing});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidatePickupToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Linearly-stacking multiplier for health gained from Bitter Root.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float healthMult {get; private set;} = 0.08f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Cap for health multiplier gained from Bitter Root.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float healthCap {get; private set;} = 3f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Set to false to change Bitter Root's effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Bitter Root interacts with other effects.")]
        public bool useIL {get; private set;} = true;

        private bool ilFailed = false;        
        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Gain " + Pct(healthMult) + " max hp.";        
        protected override string NewLangDesc(string langid = null) => "Increases <style=cIsHealing>health</style> by <style=cIsHealing>" + Pct(healthMult) + "</style> <style=cStack>(+" +Pct(healthMult)+ " per stack, linear)</style>, up to a <style=cIsHealing>maximum</style> of <style=cIsHealing>+"+Pct(healthCap)+"</style>.";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public BitterRoot() {}

        protected override void LoadBehavior() {
            if(useIL) {
                ilFailed = false;
                IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
                if(ilFailed) {
                    IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
                    On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
                }
            } else
                On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
        }
        protected override void UnloadBehavior() {
            IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
            On.RoR2.CharacterBody.RecalculateStats -= On_CBRecalcStats;
        }

        private void IL_CBRecalcStats(ILContext il) {
            var c = new ILCursor(il);

            //Add another local variable to store Bitter Root itemcount
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
                Debug.LogError("ClassicItems: failed to apply Bitter Root IL patch (inventory load), falling back to event hook");
                return;
            }

            //Find: num34 += (float)(num26 + num27) * 0.1f, after get_maxHealth()
            int locOrigHPMult = -1;
            ILFound = c.TryGotoNext(x=>x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth"))
            && c.TryGotoNext(MoveType.After,
                x=>x.MatchLdloc(out locOrigHPMult),
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.MatchAdd(),
                x=>x.MatchConvR4(),
                x=>x.OpCode == OpCodes.Ldc_R4,
                x=>x.MatchMul(),
                x=>x.MatchAdd(),
                x=>x.MatchStloc(locOrigHPMult));

            if(ILFound) {
                c.Index--;
                c.Emit(OpCodes.Ldloc, locItemCount);
                c.EmitDelegate<Func<float,int,float>>((maxhp, icnt) => {
                    return maxhp + Math.Min((float)icnt * healthMult, healthCap);
                });
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Bitter Root IL patch (health modifier), falling back to event hook");
                return;
            }
        }
        private void On_CBRecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);
                
            //this stat boost takes effect after every single vanilla item, so it might not play nice with some of them
            float oldHealth = self.maxHealth;
            float HealIncrement = 1.0f + Math.Min(healthMult * GetCount(self), healthCap);
            Reflection.SetPropertyValue(self, "maxHealth", self.maxHealth * HealIncrement);

            //redo the healback/capping since we've modified maxHealth again
            float healthDiff = self.maxHealth-oldHealth;
            if(healthDiff>0)
                self.healthComponent.Heal(healthDiff, default, false);
            else if(self.healthComponent.health > self.maxHealth)
                self.healthComponent.Networkhealth = Mathf.Max(self.healthComponent.health + healthDiff, self.maxHealth);
        }
    }
}
