using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class SmartShopper : Item_V2<SmartShopper> {
        public override string displayName => "Smart Shopper";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Linear multiplier for money-on-kill increase per stack of Smart Shopper.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float moneyMult {get;private set;} = 0.25f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Enemies drop extra gold.";
        protected override string GetDescString(string langid = null) => "Gain <style=cIsUtility>+" + Pct(moneyMult) + "</style> <style=cStack>(+" + Pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();

            if(Compat_ItemStats.enabled) {
				Compat_ItemStats.CreateItemStatDef(itemDef,
					((count,inv,master)=>{return moneyMult*count;},
					(value,inv,master)=>{return $"Money Bonus: {Pct(value, 1)}";}));
			}
        }

        public override void Install() {
            base.Install();
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.DeathRewards.OnKilledServer -= On_DROnKilledServer;
        }

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport rep) {
            int icnt = GetCount(rep.attackerBody);
            self.goldReward = (uint)Mathf.FloorToInt(self.goldReward * (1f + icnt * moneyMult));
            orig(self,rep);
        }
    }
}
