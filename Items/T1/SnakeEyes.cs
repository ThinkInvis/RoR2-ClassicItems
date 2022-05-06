using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class SnakeEyes : Item<SnakeEyes> {
        public override string displayName => "Snake Eyes";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Direct additive to percent crit chance per proc per stack of Snake Eyes.", AutoConfigFlags.PreventNetMismatch, 0f, 100f)]
        public float critAdd {get;private set;} = 8f;

        [AutoConfigRoOIntSlider("{0:N0}", 1, 1000)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum number of successive failed shrines to count towards increasing Snake Eyes buff.", AutoConfigFlags.None, 1, int.MaxValue)]
        public int stackCap {get;private set;} = 6;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, any chance shrine activation will trigger Snake Eyes on all living players (matches behavior from RoR1). If false, only the purchaser will be affected.")]
        public bool affectAll {get;private set;} = true;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, deployables (e.g. Engineer turrets) with Snake Eyes will gain/lose buff stacks whenever their master does. If false, Snake Eyes will not work on deployables at all.")]
        public bool inclDeploys {get;private set;} = true;

        public BuffDef snakeEyesBuff {get;private set;}

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Gain increased crit chance on failing a shrine. Removed on succeeding a shrine.";
        protected override string GetDescString(string langid = null) => "Increases <style=cIsDamage>crit chance</style> by <style=cIsDamage>" + Pct(critAdd, 0, 1) + "</style> <style=cStack>(+" + Pct(critAdd, 0, 1) + " per stack, linear)</style> for up to <style=cIsUtility>" + stackCap + "</style> consecutive <style=cIsUtility>chance shrine failures</style>. <style=cIsDamage>Resets to 0</style> on any <style=cIsUtility>chance shrine success</style>.";
        protected override string GetLoreString(string langid = null) => "Order: Snake Eyes\n\nTracking Number: 723***********\nEstimated Delivery: 1/1/2056\nShipping Method: Standard\nShipping Address: 1843, GMG Services, Venus\nShipping Details:\n\nYou dirty ---------er. You KNEW I had to win to pay off my debts. Are you in with the casinos? Of course you are; a snake like you would. A dice that's loaded for SNAKE EYES. CUTE MOVE, ---------er.\n\nI'm comin' for you, ----.";

        public SnakeEyes() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/snakeeyes_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/SnakeEyes.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            snakeEyesBuff = ScriptableObject.CreateInstance<BuffDef>();
            snakeEyesBuff.buffColor = Color.red;
            snakeEyesBuff.canStack = true;
            snakeEyesBuff.isDebuff = false;
            snakeEyesBuff.name = $"{modInfo.shortIdentifier}SnakeEyes";
            snakeEyesBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/SnakeEyes_icon.png");
            ContentAddition.AddBuffDef(snakeEyesBuff);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
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
                tgtBody.SetBuffCount(snakeEyesBuff.buffIndex, 0);
                if(!inclDeploys) return;
                var dplist = tgtBody.master?.deployablesList;
                if(dplist != null) foreach(DeployableInfo d in dplist) {
                    d.deployable.gameObject.GetComponent<CharacterMaster>()?.GetBody()?.SetBuffCount(snakeEyesBuff.buffIndex, 0);
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
