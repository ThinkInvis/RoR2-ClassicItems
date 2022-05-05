using RoR2;
using System;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using UnityEngine;

namespace ThinkInvisible.ClassicItems {
    public class BitterRoot : Item<BitterRoot> {
        public override string displayName => "Bitter Root";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Healing});

        [AutoConfigRoOSlider("{0:P1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Linearly-stacking multiplier for health gained from Bitter Root.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healthMult {get; private set;} = 0.08f;

        [AutoConfigRoOSlider("{0:P1}", 0f, 10000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Cap for health multiplier gained from Bitter Root.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healthCap {get; private set;} = 3f;

        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "Gain " + Pct(healthMult) + " max hp.";
        protected override string GetDescString(string langid = null) {
            string desc = $"Increases <style=cIsHealing>health</style> by <style=cIsHealing>{Pct(healthMult)}</style>";
            if(healthMult > 0f) desc += $" <style=cStack>(+{Pct(healthMult)} per stack, linear)</style>";
            desc += $", up to a <style=cIsHealing>maximum</style> of <style=cIsHealing>+{Pct(healthCap)}</style>.";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "Order: Bitter Root\n\nTracking Number: 552***********\nEstimated Delivery: 6/4/2056\nShipping Method: Standard\nShipping Address: 103 110th, Northlake, Mars\nShipping Details:\n\nBiggest. Ginseng. Root. Ever.\n\nNot really, but it's pretty freakin' huge. You can boil it or sun dry it, whatever you want. I'll be curious to see if your cancer research actually pans out; sounds like voodoo magic to me, herbal medicine, but if the natives say it works, it works.";

        public BitterRoot() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/bitterroot_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/BitterRoot.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }
        public override void Uninstall() {
            base.Uninstall();
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            args.healthMultAdd += Math.Min(GetCount(sender) * healthMult, healthCap);
        }
    }
}
