using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using UnityEngine;
using static R2API.RecalculateStatsAPI;

namespace ThinkInvisible.ClassicItems {
    public class Vial : Item<Vial> {
        public override string displayName => "Mysterious Vial";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Healing});

        [AutoConfigRoOSlider("{0:N1}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Direct additive to natural health regen per stack of Mysterious Vial.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addRegen {get;private set;} = 1.4f;

        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "Increased health regeneration.";        
        protected override string GetDescString(string langid = null) => "Increases <style=cIsHealing>health regen by +" + addRegen.ToString("N1") + "/s</style> <style=cStack>(+" + addRegen.ToString("N1") + "/s per stack)</style>.";        
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Vial() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/vial_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/MysteriousVial.prefab");
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
            args.baseRegenAdd += GetCount(sender) * addRegen;
        }
    }
}
