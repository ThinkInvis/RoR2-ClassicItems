using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.ObjectModel;

namespace ThinkInvisible.ClassicItems {
    public class SmartShopper : Item<SmartShopper> {
        public override string displayName => "Smart Shopper";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemAIBDefault => true;

        [AutoItemCfg("Linear multiplier for money-on-kill increase per stack of Smart Shopper.", default, 0f, float.MaxValue)]
        public float moneyMult {get;private set;} = 0.25f;

        public override void SetupAttributesInner() {
            RegLang(
            	"Enemies drop extra gold.",
            	"Gain <style=cIsUtility>+" + Pct(moneyMult) + "</style> <style=cStack>(+" + Pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.",
            	"A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep) {
            int icnt = GetCount(rep.attackerBody);
            self.goldReward = (uint)Mathf.FloorToInt(self.goldReward * (1f + icnt * moneyMult));
            orig(self,rep);
        }
    }
}
