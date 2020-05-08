using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class SmartShopper : Item<SmartShopper> {
        public override string displayName => "Smart Shopper";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemAIB {get; protected set;} = true;

        [AICAUEventInfo(AICAUEventFlags.InvalidateDescToken)]
        [AutoItemCfg("Linear multiplier for money-on-kill increase per stack of Smart Shopper.", AICFlags.None, 0f, float.MaxValue)]
        public float moneyMult {get;private set;} = 0.25f;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Enemies drop extra gold.";
        protected override string NewLangDesc(string langid = null) => "Gain <style=cIsUtility>+" + Pct(moneyMult) + "</style> <style=cStack>(+" + Pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public SmartShopper() {}

        protected override void LoadBehavior() {
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        protected override void UnloadBehavior() {
            On.RoR2.DeathRewards.OnKilledServer -= On_DROnKilledServer;
        }

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep) {
            int icnt = GetCount(rep.attackerBody);
            self.goldReward = (uint)Mathf.FloorToInt(self.goldReward * (1f + icnt * moneyMult));
            orig(self,rep);
        }
    }
}
