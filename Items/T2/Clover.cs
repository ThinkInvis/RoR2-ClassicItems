using RoR2;
using System;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Clover : Item<Clover> {
        public override string displayName => "56 Leaf Clover";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemAIB {get; protected set;} = true;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Percent chance for a Clover drop to happen at first stack -- as such, multiplicative with Rare/Uncommon chances.", AutoItemConfigFlags.None, 0f, 100f)]
        public float baseChance {get;private set;} = 4f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Percent chance for a Clover drop to happen per extra stack.", AutoItemConfigFlags.None, 0f, 100f)]
        public float stackChance {get;private set;} = 1.5f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Maximum percent chance for a Clover drop on elite kill.", AutoItemConfigFlags.None, 0f, 100f)]
        public float capChance {get;private set;} = 100f;
        
        [AutoItemConfig("Percent chance for a Clover drop to become Tier 2 at first stack (if it hasn't already become Tier 3).", AutoItemConfigFlags.None, 0f, 100f)]
        public float baseUnc {get;private set;} = 1f;
        [AutoItemConfig("Percent chance for a Clover drop to become Tier 2 per extra stack.", AutoItemConfigFlags.None, 0f, 100f)]
        public float stackUnc {get;private set;} = 0.1f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Maximum percent chance for a Clover drop to become Tier 2.", AutoItemConfigFlags.None, 0f, 100f)]
        public float capUnc {get;private set;} = 25f;
        
        [AutoItemConfig("Percent chance for a Clover drop to become Tier 3 at first stack.", AutoItemConfigFlags.None, 0f, 100f)]
        public float baseRare {get;private set;} = 0.01f;
        [AutoItemConfig("Percent chance for a Clover drop to become Tier 3 per extra stack.", AutoItemConfigFlags.None, 0f, 100f)]
        public float stackRare {get;private set;} = 0.001f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Maximum percent chance for a Clover drop to become Tier 3.", AutoItemConfigFlags.None, 0f, 100f)]
        public float capRare {get;private set;} = 1f;

        [AutoItemConfig("Percent chance for a Tier 1 Clover drop to become Equipment instead.", AutoItemConfigFlags.None, 0f, 100f)]
        public float baseEqp {get;private set;} = 5f;

        [AutoItemConfig("If true, all clovers across all living players are counted towards item drops. If false, only the killer's items count.")]
        public bool globalStack {get;private set;} = true;
        
		[AutoItemConfig("If true, deployables (e.g. Engineer turrets) with 56 Leaf Clover will count towards globalStack.")]
        public bool inclDeploys {get;private set;} = false;

        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Elite mobs have a chance to drop items.";        
        protected override string NewLangDesc(string langid = null) => "Elites have a <style=cIsUtility>" + Pct(baseChance, 1, 1) + " chance</style> <style=cStack>(+" + Pct(stackChance, 1, 1) + " per stack COMBINED FOR ALL PLAYERS, up to " + Pct(capChance, 1, 1) + ")</style> to <style=cIsUtility>drop items</style> when <style=cIsDamage>killed</style>. <style=cStack>(Further stacks increase uncommon/rare chance up to " +Pct(capUnc,2,1) +" and "+Pct(capRare,3,1)+", respectively.)</style>";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Clover() {
            onBehav += () => {
			    if(Compat_ItemStats.enabled) {
				    Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
					    ((count,inv,master)=>{
                            float numberOfClovers = 0;
                            if(globalStack)
                                foreach(CharacterMaster chrm in AliveList()) {
			                        if(!inclDeploys && chrm.GetComponent<Deployable>()) continue;
                                    numberOfClovers += chrm?.inventory?.GetItemCount(regIndex) ?? 0;
                                }
                            else
                                numberOfClovers += count;
                            return Math.Min(baseChance + (numberOfClovers-1) * stackChance, capChance);
                        },
					    (value,inv,master)=>{return $"Drop Chance: {Pct(value,1,1f)}";}),
                    
					    ((count,inv,master)=>{
                            float numberOfClovers = 0;
                            if(globalStack)
                                foreach(CharacterMaster chrm in AliveList()) {
			                        if(!inclDeploys && chrm.GetComponent<Deployable>()) continue;
                                    numberOfClovers += chrm?.inventory?.GetItemCount(regIndex) ?? 0;
                                }
                            else
                                numberOfClovers += count;
                            return Math.Min(baseUnc + (numberOfClovers-1) * stackUnc, capUnc);
                        },
					    (value,inv,master)=>{return $"T2 Upgrade Chance: {Pct(value,2,1f)}";}),
                    
					    ((count,inv,master)=>{
                            float numberOfClovers = 0;
                            if(globalStack)
                                foreach(CharacterMaster chrm in AliveList()) {
			                        if(!inclDeploys && chrm.GetComponent<Deployable>()) continue;
                                    numberOfClovers += chrm?.inventory?.GetItemCount(regIndex) ?? 0;
                                }
                            else
                                numberOfClovers += count;
                            return Math.Min(baseRare + (numberOfClovers-1) * stackRare, capRare);
                        },
					    (value,inv,master)=>{return $"T3 Upgrade Chance: {Pct(value,3,1f)}";})
                        );
			    }
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.DeathRewards.OnKilledServer += On_DROnKilledServer;
        }

        protected override void UnloadBehavior() {
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
                    numberOfClovers += chrm?.inventory?.GetItemCount(regIndex) ?? 0;
                }
            else
                numberOfClovers += damageReport.attackerMaster?.inventory?.GetItemCount(regIndex) ?? 0;

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
                SpawnItemFromBody(victimBody, tier, itemRng);
            }
        }
    }
}
