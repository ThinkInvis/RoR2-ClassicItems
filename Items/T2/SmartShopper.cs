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
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Linear multiplier for money-on-kill increase per stack of Smart Shopper.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float moneyMult {get;private set;} = 0.25f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Enemies drop extra gold.";
        protected override string GetDescString(string langid = null) => "Gain <style=cIsUtility>+" + Pct(moneyMult) + "</style> <style=cStack>(+" + Pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public SmartShopper() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/smartshopper_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/SmartShopper.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
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
