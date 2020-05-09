using R2API.Utils;
using RoR2;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class SnakeEyes : Item<SnakeEyes> {
        public override string displayName => "Snake Eyes";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Direct additive to percent crit chance per proc per stack of Snake Eyes.", AutoItemConfigFlags.None, 0f, 100f)]
        public float critAdd {get;private set;} = 8f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Maximum number of successive failed shrines to count towards increasing Snake Eyes buff.", AutoItemConfigFlags.None, 1, int.MaxValue)]
        public int stackCap {get;private set;} = 6;

        [AutoItemConfig("If true, any chance shrine activation will trigger Snake Eyes on all living players (matches behavior from RoR1). If false, only the purchaser will be affected.")]
        public bool affectAll {get;private set;} = true;

        [AutoItemConfig("If true, deployables (e.g. Engineer turrets) with Snake Eyes will gain/lose buff stacks whenever their master does. If false, Snake Eyes will not work on deployables at all.")]
        public bool inclDeploys {get;private set;} = true;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Set to false to change Snake Eyes' effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Snake Eyes interacts with other effects.")]
        public bool useIL {get;private set;} = true;

        public BuffIndex snakeEyesBuff {get;private set;}

        private bool ilFailed = false;
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Gain increased crit chance on failing a shrine. Removed on succeeding a shrine.";
        protected override string NewLangDesc(string langid = null) => "Increases <style=cIsDamage>crit chance</style> by <style=cIsDamage>" + Pct(critAdd, 0, 1) + "</style> <style=cStack>(+" + Pct(critAdd, 0, 1) + " per stack, linear)</style> for up to <style=cIsUtility>" + stackCap + "</style> consecutive <style=cIsUtility>chance shrine failures</style>. <style=cIsDamage>Resets to 0</style> on any <style=cIsUtility>chance shrine success</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public SnakeEyes() {
            onAttrib += (tokenIdent, namePrefix) => {
                var snakeEyesBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = Color.red,
                    canStack = true,
                    isDebuff = false,
                    name = "SnakeEyes",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/SnakeEyes_icon.png"
                });
                snakeEyesBuff = R2API.BuffAPI.Add(snakeEyesBuffDef);
            };
        }

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

            ShrineChanceBehavior.onShrineChancePurchaseGlobal += Evt_SCBOnShrineChancePurchaseGlobal;
        }

        protected override void UnloadBehavior() {
            IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
            On.RoR2.CharacterBody.RecalculateStats -= On_CBRecalcStats;
        }

        private void cbApplyBuff(bool failed, CharacterBody tgtBody) {
            if(tgtBody == null) return;
            if(failed) {
                if(GetCount(tgtBody) > 0 && tgtBody.GetBuffCount(snakeEyesBuff) < stackCap) tgtBody.AddBuff(snakeEyesBuff);
                if(!inclDeploys) return;
                var dplist = tgtBody.master?.GetFieldValue<List<DeployableInfo>>("deployablesList");
                if(dplist != null) foreach(DeployableInfo d in dplist) {
                    var dplBody = d.deployable.gameObject.GetComponent<CharacterMaster>()?.GetBody();
                    if(dplBody && GetCount(dplBody) > 0 && dplBody.GetBuffCount(snakeEyesBuff) < stackCap) {
                        dplBody.AddBuff(snakeEyesBuff);
                    }
                }
            } else {
                tgtBody.SetBuffCount(snakeEyesBuff, 0);
                if(!inclDeploys) return;
                var dplist = tgtBody.master?.GetFieldValue<List<DeployableInfo>>("deployablesList");
                if(dplist != null) foreach(DeployableInfo d in dplist) {
                    d.deployable.gameObject.GetComponent<CharacterMaster>()?.GetBody()?.SetBuffCount(snakeEyesBuff, 0);
                }
            }
        }

        private void Evt_SCBOnShrineChancePurchaseGlobal(bool failed, Interactor tgt) {
            if(affectAll) {
                AliveList().ForEach(x=>{
                    cbApplyBuff(failed, x.GetBody());
                });
            } else {
                cbApplyBuff(failed, tgt.GetComponent<CharacterBody>());
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
