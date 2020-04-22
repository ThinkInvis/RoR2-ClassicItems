using R2API.Utils;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems
{
    public class SnakeEyes : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "SnakeEyes";

        private ConfigEntry<float> cfgAdd;
        private ConfigEntry<int> cfgCap;
        private ConfigEntry<bool> cfgAffectAll;
        private ConfigEntry<bool> cfgUseIL;

        public float critAdd {get;private set;}
        public int stackCap {get;private set;}
        public bool affectAll {get;private set;}
        public bool useIL {get;private set;}

        public BuffIndex snakeEyesBuff {get;private set;}

        private bool ilFailed = false;

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgAdd = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Add"), 8f, new ConfigDescription(
                "Direct additive to percent crit chance per proc * stack of Snake Eyes.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgCap = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Cap"), 6, new ConfigDescription(
                "Number of successive failed shrines that count towards increasing Snake Eyes buff.",
                new AcceptableValueRange<int>(1,int.MaxValue)));
            cfgAffectAll = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "AffectAll"), true, new ConfigDescription(
                "If true, any chance shrine activation will trigger Snake Eyes on all living players (matches behavior from RoR1). If false, only the purchaser will be affected."));
            cfgUseIL = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "UseIL"), true, new ConfigDescription(
                "Set to false to change Snake Eyes' effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Snake Eyes interacts with other effects."));

            critAdd = cfgAdd.Value;
            stackCap = cfgCap.Value;
            affectAll = cfgAffectAll.Value;
            useIL = cfgUseIL.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "snakeeyescard.prefab";
            iconPathName = "snakeeyes_icon.png";
            RegLang("Snake Eyes",
            	"Gain increased crit chance on failing a shrine. Removed on succeeding a shrine.",
            	"Increases <style=cIsDamage>crit chance</style> by <style=cIsDamage>" + pct(critAdd, 0, 1) + "</style> <style=cStack>(+" + pct(critAdd, 0, 1) + " per stack, linear)</style> for up to <style=cIsUtility>" + stackCap + "</style> consecutive <style=cIsUtility>chance shrine failures</style>. <style=cIsDamage>Resets to 0</style> on any <style=cIsUtility>chance shrine success</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Damage};
            itemTier = ItemTier.Tier1;
        }

        protected override void SetupBehaviorInner() {
            var snakeEyesBuffDef = new R2API.CustomBuff("SnakeEyes", new BuffDef {
                buffColor = Color.red,
                canStack = true,
                isDebuff = false,
                name = "SnakeEyes",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName
            });
            snakeEyesBuff = R2API.ItemAPI.Add(snakeEyesBuffDef);

            if(useIL) {
                IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
                if(ilFailed) {
                    IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
                    On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
                }
            } else
                On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;

            ShrineChanceBehavior.onShrineChancePurchaseGlobal += Evt_SCBOnShrineChancePurchaseGlobal;
        }

        private void Evt_SCBOnShrineChancePurchaseGlobal(bool failed, Interactor tgt) {
            if(affectAll) {
                aliveList().ForEach(x=>{
                    CharacterBody tgtBody = x.GetBody();
                    if(GetCount(tgtBody) < 1) return;
                    if(failed)
                        if(tgtBody.GetBuffCount(snakeEyesBuff) < stackCap) tgtBody.AddBuff(snakeEyesBuff);
                    else
                        Reflection.InvokeMethod(tgtBody, "SetBuffCount", snakeEyesBuff, 0);
                });
            } else {
                CharacterBody tgtBody = tgt.GetComponentInParent<CharacterBody>();
                if(GetCount(tgtBody) < 1) return;
                if(failed)
                    if(tgtBody.GetBuffCount(snakeEyesBuff) < stackCap) tgtBody.AddBuff(snakeEyesBuff);
                else
                    Reflection.InvokeMethod(tgtBody, "SetBuffCount", snakeEyesBuff, 0);
            }
        }

        private void On_CBRecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);

            float CritIncrement = self.GetBuffCount(snakeEyesBuff) * GetCount(self) * critAdd;
            Reflection.SetPropertyValue(self, "crit", self.crit + CritIncrement);
        }

        private void IL_CBRecalcStats(ILContext il) {
            var c = new ILCursor(il);
            //Add another local variable to store Snake Eyes itemcount
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
                Debug.LogError("ClassicItems: failed to apply Snake Eyes IL patch (inventory load), falling back to event hook");
                return;
            }

            //Find: num53 += (float)num8 * 10f


            int locOrigCrit = -1;
            ILFound = c.TryGotoNext(
                x=>x.MatchLdarg(0),
                x=>x.MatchLdloc(out locOrigCrit),
                x=>x.MatchCallOrCallvirt<CharacterBody>("set_crit"));

            if(ILFound) {
                c.Emit(OpCodes.Ldloc, locOrigCrit);
                c.Emit(OpCodes.Ldloc, locItemCount);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float,int,CharacterBody,float>>((crit, icnt, body) => {
                    return crit + icnt * critAdd * body.GetBuffCount(snakeEyesBuff);
                });
                c.Emit(OpCodes.Stloc, locOrigCrit);
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Snake Eyes IL patch (crit modifier), falling back to event hook");
                return;
            }
        }
    }
}
