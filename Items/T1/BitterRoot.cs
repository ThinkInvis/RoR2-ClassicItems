using RoR2;
using System;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class BitterRoot : Item<BitterRoot> {
        public override string displayName => "Bitter Root";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Healing});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidatePickupToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Linearly-stacking multiplier for health gained from Bitter Root.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healthMult {get; private set;} = 0.08f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Cap for health multiplier gained from Bitter Root.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healthCap {get; private set;} = 3f;

        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Gain " + Pct(healthMult) + " max hp.";        
        protected override string NewLangDesc(string langid = null) => "Increases <style=cIsHealing>health</style> by <style=cIsHealing>" + Pct(healthMult) + "</style> <style=cStack>(+" +Pct(healthMult)+ " per stack, linear)</style>, up to a <style=cIsHealing>maximum</style> of <style=cIsHealing>+"+Pct(healthCap)+"</style>.";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public BitterRoot() {}

        protected override void LoadBehavior() {
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }
        protected override void UnloadBehavior() {
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            args.healthMultAdd += Math.Min(GetCount(sender) * healthMult, healthCap);
        }
    }
}
