using RoR2;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Permafrost : ItemBoilerplate<Permafrost> {
        public override string displayName {get;} = "Permafrost";

        [AutoItemCfg("Percent chance of triggering Permafrost on hit. Affected by proc coefficient; stacks inverse-multiplicatively.", default, 0f, 100f)]
        public float procChance {get;private set;} = 6f;
        [AutoItemCfg("Duration of freeze applied by Permafrost.", default, 0f, float.MaxValue)]
        public float freezeTime {get;private set;} = 1.5f;
        [AutoItemCfg("Duration of slow applied by Permafrost.", default, 0f, float.MaxValue)]
        public float slowTime {get;private set;} = 3.0f;
        [AutoItemCfg("If true, Permafrost will slow targets even if they can't be frozen.")]
        public bool slowUnfreezable {get;private set;} = true;

        public override void SetupConfigInner(ConfigFile cfl) {
            itemAIBDefault = true;
        }

        public override void SetupAttributesInner() {
            RegLang(
            	"Chance to freeze enemies on hit.",
            	"<style=cIsUtility>" + Pct(procChance,1,1) + "</style> <style=cStack>(+" + Pct(procChance,1,1) + " per stack, inverse-mult.)</style> chance to <style=cIsUtility>freeze and slow</style> an enemy (" + freezeTime.ToString("N1") + " sec, " + slowTime.ToString("N1") + " sec). Affected by proc coefficient.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier3;
        }

        public override void SetupBehaviorInner() {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += On_SSOHOnTakeDamageServer;
        }

        private void On_SSOHOnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, RoR2.DamageReport damageReport) {
            orig(self, damageReport);
            int icnt = GetCount(damageReport.attackerMaster);
            if(icnt < 1) return;
            var ch = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance * icnt * damageReport.damageInfo.procCoefficient);
            if(!Util.CheckRoll(ch)) return;
            if(self.canBeFrozen) {
                self.SetFrozen(freezeTime);
                damageReport.victim?.body.AddTimedBuff(ClassicItemsPlugin.freezeBuff, freezeTime);
            }
            if((self.canBeFrozen || slowUnfreezable) && damageReport.victim) damageReport.victim.body.AddTimedBuff(BuffIndex.Slow60, slowTime);
        }
    }
}
