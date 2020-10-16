using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class SnakeEyes : Item<SnakeEyes> {
        public override string displayName => "Snake Eyes";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateLanguage | AutoUpdateEventFlags.InvalidateStats)]
        [AutoConfig("Direct additive to percent crit chance per proc per stack of Snake Eyes.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float critAdd {get;private set;} = 8f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Maximum number of successive failed shrines to count towards increasing Snake Eyes buff.", AutoConfigFlags.None, 1, int.MaxValue)]
        public int stackCap {get;private set;} = 6;

        [AutoConfig("If true, any chance shrine activation will trigger Snake Eyes on all living players (matches behavior from RoR1). If false, only the purchaser will be affected.")]
        public bool affectAll {get;private set;} = true;

        [AutoConfig("If true, deployables (e.g. Engineer turrets) with Snake Eyes will gain/lose buff stacks whenever their master does. If false, Snake Eyes will not work on deployables at all.")]
        public bool inclDeploys {get;private set;} = true;

        public BuffIndex snakeEyesBuff {get;private set;}

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Gain increased crit chance on failing a shrine. Removed on succeeding a shrine.";
        protected override string NewLangDesc(string langid = null) => "Increases <style=cIsDamage>crit chance</style> by <style=cIsDamage>" + Pct(critAdd, 0, 1) + "</style> <style=cStack>(+" + Pct(critAdd, 0, 1) + " per stack, linear)</style> for up to <style=cIsUtility>" + stackCap + "</style> consecutive <style=cIsUtility>chance shrine failures</style>. <style=cIsDamage>Resets to 0</style> on any <style=cIsUtility>chance shrine success</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupAttributes() {
            base.SetupAttributes();

            var snakeEyesBuffDef = new R2API.CustomBuff(new BuffDef {
                buffColor = Color.red,
                canStack = true,
                isDebuff = false,
                name = $"{modInfo.shortIdentifier}SnakeEyes",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/SnakeEyes_icon.png"
            });
            snakeEyesBuff = R2API.BuffAPI.Add(snakeEyesBuffDef);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            if(Compat_ItemStats.enabled) {
                Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
                    ((count, inv, master) => { return critAdd * count; },
                    (value, inv, master) => { return $"Crit Chance Per Shrine Fail: {Pct(value, 1, 1)}"; }
                ));
            }
        }

        public override void Install() {
            base.Install();
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
            ShrineChanceBehavior.onShrineChancePurchaseGlobal += Evt_SCBOnShrineChancePurchaseGlobal;
        }

        public override void Uninstall() {
            base.Uninstall();
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
            ShrineChanceBehavior.onShrineChancePurchaseGlobal -= Evt_SCBOnShrineChancePurchaseGlobal;
        }

        private void cbApplyBuff(bool failed, CharacterBody tgtBody) {
            if(tgtBody == null) return;
            if(failed) {
                if(GetCount(tgtBody) > 0 && tgtBody.GetBuffCount(snakeEyesBuff) < stackCap) tgtBody.AddBuff(snakeEyesBuff);
                if(!inclDeploys) return;
                var dplist = tgtBody.master?.deployablesList;
                if(dplist != null) foreach(DeployableInfo d in dplist) {
                    var dplBody = d.deployable.gameObject.GetComponent<CharacterMaster>()?.GetBody();
                    if(dplBody && GetCount(dplBody) > 0 && dplBody.GetBuffCount(snakeEyesBuff) < stackCap) {
                        dplBody.AddBuff(snakeEyesBuff);
                    }
                }
            } else {
                tgtBody.SetBuffCount(snakeEyesBuff, 0);
                if(!inclDeploys) return;
                var dplist = tgtBody.master?.deployablesList;
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
        
        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            args.critAdd += sender.GetBuffCount(snakeEyesBuff) * GetCount(sender) * critAdd;
        }
    }
}
