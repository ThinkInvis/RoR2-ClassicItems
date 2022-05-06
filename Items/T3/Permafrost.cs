using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Permafrost : Item<Permafrost> {
        public override string displayName => "Permafrost";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance of triggering Permafrost on hit. Affected by proc coefficient; stacks hyperbolically.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 6f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of freeze applied by Permafrost.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float freezeTime {get;private set;} = 1.5f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of slow applied by Permafrost.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float slowTime {get;private set;} = 3.0f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, Permafrost will slow targets even if they can't be frozen.")]
        public bool slowUnfreezable {get;private set;} = true;
        
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to freeze enemies on hit.";
        protected override string GetDescString(string langid = null) => "<style=cIsUtility>" + Pct(procChance,1,1) + "</style> <style=cStack>(+" + Pct(procChance,1,1) + " per stack, hyperbolic)</style> chance to <style=cIsUtility>freeze and slow</style> an enemy (" + freezeTime.ToString("N1") + "s and " + slowTime.ToString("N1") + "s respectively). Affected by proc coefficient.";
        protected override string GetLoreString(string langid = null) => "Order: Permafrost\n\nTracking Number: 581***********\nEstimated Delivery: 4/17/2056\nShipping Method: High Priority/Fragile\nShipping Address: 10681, Frozen Valley, Pluto\nShipping Details:\n\nThe core of this ice cube is VERY VERY close to absolute zero, much closer than anything we made in the lab before. Test it! TEST! This will lead to HUGE breakthroughs in superconductor technology.";

        public Permafrost() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/permafrost_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Permafrost.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += On_SSOHOnTakeDamageServer;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer -= On_SSOHOnTakeDamageServer;
        }

        private void On_SSOHOnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, DamageReport damageReport) {
            orig(self, damageReport);
            int icnt = GetCount(damageReport.attackerMaster);
            if(icnt < 1) return;
            var ch = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance * icnt * damageReport.damageInfo.procCoefficient);
            if(!Util.CheckRoll(ch)) return;
            if(self.canBeFrozen) {
                self.SetFrozen(freezeTime);
                damageReport.victim?.body.AddTimedBuff(ClassicItemsPlugin.freezeBuff, freezeTime);
            }
            if((self.canBeFrozen || slowUnfreezable) && damageReport.victim) damageReport.victim.body.AddTimedBuff(RoR2Content.Buffs.Slow60, slowTime);
        }
    }
}
