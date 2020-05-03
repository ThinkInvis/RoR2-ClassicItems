using RoR2;
using System;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Clover : ItemBoilerplate {
        public override string itemCodeName {get;} = "Clover";

        private ConfigEntry<float> cfgBaseChance;
        private ConfigEntry<float> cfgStackChance;
        private ConfigEntry<float> cfgCapChance;

        private ConfigEntry<float> cfgBaseUnc;
        private ConfigEntry<float> cfgStackUnc;
        private ConfigEntry<float> cfgCapUnc;

        private ConfigEntry<float> cfgBaseRare;
        private ConfigEntry<float> cfgStackRare;
        private ConfigEntry<float> cfgCapRare;

        private ConfigEntry<float> cfgEqpChance;

        private ConfigEntry<bool> cfgGlobalStack;

        public float baseChance {get;private set;}
        public float stackChance {get;private set;}
        public float capChance {get;private set;}
        
        public float baseUnc {get;private set;}
        public float stackUnc {get;private set;}
        public float capUnc {get;private set;}
        
        public float baseRare {get;private set;}
        public float stackRare {get;private set;}
        public float capRare {get;private set;}

        public float baseEqp {get;private set;}

        public bool globalStack {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            itemAIBDefault = true;

            cfgBaseChance = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "BaseChance"), 4f, new ConfigDescription(
                "Percent chance for a Clover drop to happen at first stack -- as such, multiplicative with Rare/Uncommon chances.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgStackChance = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "StackChance"), 1.5f, new ConfigDescription(
                "Percent chance for a Clover drop to happen per extra stack.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgCapChance = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "CapChance"), 100f, new ConfigDescription(
                "Maximum percent chance for a Clover drop on elite kill.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgBaseUnc = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "BaseUnc"), 1f, new ConfigDescription(
                "Percent chance for a Clover drop to become Tier 2 at first stack (if it hasn't already become Tier 3).",
                new AcceptableValueRange<float>(0f,100f)));
            cfgStackUnc = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "StackUnc"), 0.1f, new ConfigDescription(
                "Percent chance for a Clover drop to become Tier 2 per extra stack.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgCapUnc = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "CapUnc"), 25f, new ConfigDescription(
                "Maximum percent chance for a Clover drop to become Tier 2.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgBaseRare = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "BaseRare"), 0.01f, new ConfigDescription(
                "Percent chance for a Clover drop to become Tier 3 at first stack.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgStackRare = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "StackRare"), 0.001f, new ConfigDescription(
                "Percent chance for a Clover drop to become Tier 3 per extra stack.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgCapRare = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "CapRare"), 1f, new ConfigDescription(
                "Maximum percent chance for a Clover drop to become Tier 3.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgEqpChance = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "EqpChance"), 5f, new ConfigDescription(
                "Percent chance for a Tier 1 Clover drop to become Equipment instead.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgGlobalStack = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "GlobalStack"), true, new ConfigDescription(
                "If true, all clovers across all living players are counted towards item drops. If false, only the killer's items count."));

            baseChance = cfgBaseChance.Value;
            stackChance = cfgStackChance.Value;
            capChance = cfgCapChance.Value;
            
            baseUnc = cfgBaseUnc.Value;
            stackUnc = cfgStackUnc.Value;
            capUnc = cfgCapUnc.Value;

            baseRare = cfgBaseRare.Value;
            stackRare = cfgStackRare.Value;
            capRare = cfgCapRare.Value;

            baseEqp = cfgEqpChance.Value;

            globalStack = cfgGlobalStack.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "clover_model.prefab";
            iconPathName = "clover_icon.png";
            RegLang("56 Leaf Clover",
            	"Elite mobs have a chance to drop items.",
            	"Elites have a <style=cIsUtility>" + pct(baseChance, 1, 1) + " chance</style> <style=cStack>(+" + pct(stackChance, 1, 1) + " per stack COMBINED FOR ALL PLAYERS, up to " + pct(capChance, 1, 1) + ")</style> to <style=cIsUtility>drop items</style> when <style=cIsDamage>killed</style>. <style=cStack>(Further stacks increase uncommon/rare chance up to " +pct(capUnc,2,1) +" and "+pct(capRare,3,1)+", respectively.)</style>",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier2;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        private void On_DROnKilledServer(On.RoR2.DeathRewards.orig_OnKilledServer orig, DeathRewards self, DamageReport damageReport) {
            orig(self, damageReport);

            if(damageReport == null) return;
            CharacterBody victimBody = damageReport.victimBody;
            if(victimBody == null || victimBody.teamComponent.teamIndex != TeamIndex.Monster || !victimBody.isElite) return;
            int numberOfClovers = 0;
            if(globalStack)
                foreach(CharacterMaster chrm in aliveList()) {
                    numberOfClovers += chrm?.inventory?.GetItemCount(regIndex) ?? 0;
                }
            else
                numberOfClovers += damageReport.attackerMaster?.inventory?.GetItemCount(regIndex) ?? 0;

            if(numberOfClovers == 0) return;

            float rareChance = Math.Min(baseRare + numberOfClovers * stackRare, capRare);
            float uncommonChance = Math.Min(baseUnc + numberOfClovers * stackUnc, capUnc);
            float anyDropChance = Math.Min(baseChance + numberOfClovers * stackChance, capChance);
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
                spawnItemFromBody(victimBody, tier);
            }

        }
    }
}
