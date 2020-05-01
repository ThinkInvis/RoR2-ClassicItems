using RoR2;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Permafrost : ItemBoilerplate {
        public override string itemCodeName {get;} = "Permafrost";

        private ConfigEntry<float> cfgProcChance;
        private ConfigEntry<float> cfgFreezeTime;
        private ConfigEntry<float> cfgSlowTime;
        private ConfigEntry<bool> cfgSlowUnfreezable;

        public float procChance {get;private set;}
        public float freezeTime {get;private set;}
        public float slowTime {get;private set;}
        public bool slowUnfreezable {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgProcChance = cfl.Bind<float>(new ConfigDefinition("Items." + itemCodeName, "ProcChance"), 6f, new ConfigDescription(
                "Percent chance of triggering Permafrost on hit. Affected by proc coefficient; stacks inverse-multiplicatively.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgFreezeTime = cfl.Bind<float>(new ConfigDefinition("Items." + itemCodeName, "FreezeTime"), 1.5f, new ConfigDescription(
                "Duration of freeze applied by Permafrost.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgSlowTime = cfl.Bind<float>(new ConfigDefinition("Items." + itemCodeName, "SlowTime"), 3.0f, new ConfigDescription(
                "Duration of slow applied by Permafrost.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgSlowUnfreezable = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "SlowUnfreezable"), true, new ConfigDescription(
                "If true, Permafrost will slow targets even if they can't be frozen."));

            procChance = cfgProcChance.Value;
            freezeTime = cfgFreezeTime.Value;
            slowTime = cfgSlowTime.Value;
            slowUnfreezable = cfgSlowUnfreezable.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemAIBDefault = true;

            modelPathName = "permafrostcard.prefab";
            iconPathName = "permafrost_icon.png";
            RegLang("Permafrost",
            	"Chance to freeze enemies on hit.",
            	"<style=cIsUtility>" + pct(procChance,1,1) + "</style> <style=cStack>(+" + pct(procChance,1,1) + " per stack, inverse-mult.)</style> chance to <style=cIsUtility>freeze and slow</style> an enemy (" + freezeTime.ToString("N1") + " sec, " + slowTime.ToString("N1") + " sec). Affected by proc coefficient.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier3;
        }

        protected override void SetupBehaviorInner() {
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
