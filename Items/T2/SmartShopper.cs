using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;

namespace ThinkInvisible.ClassicItems
{
    public class SmartShopper : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "SmartShopper";

        private ConfigEntry<float> cfgMult;

        public float moneyMult {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgMult = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Mult"), 0.25f, new ConfigDescription(
                "Linear multiplier for money-on-kill increase per stack of Smart Shopper.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));

            moneyMult = cfgMult.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "shoppercard.prefab";
            iconPathName = "smartshopper_icon.png";
            itemName = "Smart Shopper";
            itemShortText = "Enemies drop extra gold.";
            itemLongText = "Gain <style=cIsUtility>+" + pct(moneyMult) + "</style> <style=cStack>(+" + pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.";
            itemLoreText = "A relic of times long past (ClassicItems mod)";
            _itemTags = new[]{ItemTag.Utility};
            itemTier = ItemTier.Tier2;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, RoR2.DeathRewards self, RoR2.DamageReport rep) {
            int icnt = GetCount(rep.attackerBody);
            self.goldReward = (uint)Mathf.FloorToInt(self.goldReward * (1f + icnt * moneyMult));
            orig(self,rep);
        }
    }
}
