using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class SmartShopper : ItemBoilerplate<SmartShopper> {
        public override string itemCodeName {get;} = "SmartShopper";

        private ConfigEntry<float> cfgMult;

        public float moneyMult {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            itemAIBDefault = true;

            cfgMult = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Mult"), 0.25f, new ConfigDescription(
                "Linear multiplier for money-on-kill increase per stack of Smart Shopper.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));

            moneyMult = cfgMult.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "smartshopper_model.prefab";
            iconPathName = "smartshopper_icon.png";
            RegLang("Smart Shopper",
            	"Enemies drop extra gold.",
            	"Gain <style=cIsUtility>+" + Pct(moneyMult) + "</style> <style=cStack>(+" + Pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier2;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep) {
            int icnt = GetCount(rep.attackerBody);
            self.goldReward = (uint)Mathf.FloorToInt(self.goldReward * (1f + icnt * moneyMult));
            orig(self,rep);
        }
    }
}
