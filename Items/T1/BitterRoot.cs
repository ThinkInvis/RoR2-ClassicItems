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

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Linearly-stacking multiplier for health gained from Bitter Root.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float healthMult {get; private set;} = 0.08f;

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
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();

            if(Compat_ItemStats.enabled) {
                Compat_ItemStats.CreateItemStatDef(itemDef,
                    ((count, inv, master) => {
                        return Math.Min(count * healthMult, healthCap);
                    },
                    (value, inv, master) => { return $"Bonus Health: {Pct(value)}"; }
                ));
            }
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
