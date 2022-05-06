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

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Linear multiplier for money-on-kill increase per stack of Smart Shopper.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float moneyMult {get;private set;} = 0.25f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Enemies drop extra gold.";
        protected override string GetDescString(string langid = null) => "Gain <style=cIsUtility>+" + Pct(moneyMult) + "</style> <style=cStack>(+" + Pct(moneyMult) + " per stack, linear)</style> <style=cIsUtility>money</style> from <style=cIsDamage>killing enemies</style>.";
        protected override string GetLoreString(string langid = null) => "Order: Smart Shopper\n\nTracking Number: 769***********\nEstimated Delivery: 12/24/1962\nShipping Method: Priority\nShipping Address: 200 West Street, City of Gold, Earth\nShipping Details:\n\nHoney! HONEY! Look! I /told/ you I still had enough left over! Yeah, weird huh? I swear the last $5 I used was the last one I had in my purse, but then when I looked again, I still had a bit left over. Strange, huh?\n\nMaybe it's black magic, haha! Honey?";

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
