using RoR2;
using System;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Clover : Item<Clover> {
        public override string displayName => "56 Leaf Clover";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for a Clover drop to happen at first stack -- as such, multiplicative with Rare/Uncommon chances.", AutoConfigFlags.None, 0f, 100f)]
        public float baseChance {get;private set;} = 4f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for a Clover drop to happen per extra stack.", AutoConfigFlags.None, 0f, 100f)]
        public float stackChance {get;private set;} = 1.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum percent chance for a Clover drop on elite kill.", AutoConfigFlags.None, 0f, 100f)]
        public float capChance {get;private set;} = 100f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfig("Percent chance for a Clover drop to become Tier 2 at first stack (if it hasn't already become Tier 3).", AutoConfigFlags.None, 0f, 100f)]
        public float baseUnc {get;private set;} = 1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfig("Percent chance for a Clover drop to become Tier 2 per extra stack.", AutoConfigFlags.None, 0f, 100f)]
        public float stackUnc {get;private set;} = 0.1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum percent chance for a Clover drop to become Tier 2.", AutoConfigFlags.None, 0f, 100f)]
        public float capUnc {get;private set;} = 25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfig("Percent chance for a Clover drop to become Tier 3 at first stack.", AutoConfigFlags.None, 0f, 100f)]
        public float baseRare {get;private set;} = 0.01f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfig("Percent chance for a Clover drop to become Tier 3 per extra stack.", AutoConfigFlags.None, 0f, 100f)]
        public float stackRare {get;private set;} = 0.001f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum percent chance for a Clover drop to become Tier 3.", AutoConfigFlags.None, 0f, 100f)]
        public float capRare {get;private set;} = 1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfig("Percent chance for a Tier 1 Clover drop to become Equipment instead.", AutoConfigFlags.None, 0f, 100f)]
        public float baseEqp {get;private set;} = 5f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, all clovers across all living players are counted towards item drops. If false, only the killer's items count.")]
        public bool globalStack {get;private set;} = true;
        
        [AutoConfigRoOCheckbox()]
		[AutoConfig("If true, deployables (e.g. Engineer turrets) with 56 Leaf Clover will count towards globalStack.")]
        public bool inclDeploys {get;private set;} = false;

        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "Elite mobs have a chance to drop items.";
        protected override string GetDescString(string langid = null) {
            string desc = "Elites have a <style=cIsUtility>" + Pct(baseChance, 1, 1) + " chance</style> <style=cStack>(";
            if(stackChance > 0f) desc += $"+{Pct(stackChance, 1, 1)} per stack, ";
            desc += "COMBINED FOR ALL PLAYERS, up to " + Pct(capChance, 1, 1) + ")</style> to <style=cIsUtility>drop items</style> when <style=cIsDamage>killed</style>. <style=cStack>(Further stacks increase uncommon/rare chance up to " + Pct(capUnc, 2, 1) + " and " + Pct(capRare, 3, 1) + ", respectively.)</style>";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "Order: 56 Leaf Clover\n\nTracking Number: 600***********\nEstimated Delivery: 12/24/2056\nShipping Method: Priority\nShipping Address: 777, Lucky Drop, Earth\nShipping Details:\n\nA FIFTY-FREAKIN-SIX leaf clover! It was sent to me from a fan, picked from the top of Mount Drummond. It lived through the mountain fire; imagine that! I'm keeping it in observation to make sure it doesn't wilt; this is a once in a lifetime specimen! Think of all the elixirs we could make.. I may use a few of them myself and go hit the lottery!";

        public Clover() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/clover_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Clover.prefab");
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

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport) {
            orig(self, damageReport);

            if(damageReport == null) return;
            CharacterBody victimBody = damageReport.victimBody;
            if(victimBody == null || victimBody.teamComponent.teamIndex != TeamIndex.Monster || !victimBody.isElite) return;
            int numberOfClovers = 0;
            if(globalStack)
                foreach(CharacterMaster chrm in AliveList()) {
			        if(!inclDeploys && chrm.GetComponent<Deployable>()) continue;
                    numberOfClovers += chrm?.inventory?.GetItemCount(catalogIndex) ?? 0;
                }
            else
                numberOfClovers += damageReport.attackerMaster?.inventory?.GetItemCount(catalogIndex) ?? 0;

            if(numberOfClovers == 0) return;

            float rareChance = Math.Min(baseRare + (numberOfClovers - 1) * stackRare, capRare);
            float uncommonChance = Math.Min(baseUnc + (numberOfClovers - 1) * stackUnc, capUnc);
            float anyDropChance = Math.Min(baseChance + (numberOfClovers - 1) * stackChance, capChance);
            //Base drop chance is multiplicative with tier chances -- tier chances are applied to upgrade the dropped item

            if(Util.CheckRoll(anyDropChance)) {
                int tier;
                if(Util.CheckRoll(rareChance)) {
                    tier = 2;
                } else if(Util.CheckRoll(uncommonChance)) {
                    tier = 1;
                } else {
                    if(Util.CheckRoll(baseEqp))
                        tier = 4;
                    else
                        tier = 0;
                }
                SpawnItemFromBody(victimBody, tier, rng);
            }
        }
    }
}
