using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;

namespace ThinkInvisible.ClassicItems {
    public class RustyJetpack : Item<RustyJetpack> {
        public override string displayName => "Rusty Jetpack";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AutoConfigRoOSlider("{0:P0}", 0f, 0.999f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplier for gravity reduction (0.0 = no effect, 1.0 = full anti-grav).", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float gravMod {get;private set;} = 0.5f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Amount added to jump power per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float jumpMult {get;private set;} = 0.1f;

        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "Increase jump height and reduce gravity.";        
        protected override string GetDescString(string langid = null) => "<style=cIsUtility>Reduces gravity</style> by <style=cIsUtility>" + Pct(gravMod) + "</style> while <style=cIsUtility>holding jump</style>. Increases <style=cIsUtility>jump power</style> by <style=cIsUtility>" + Pct(jumpMult) + "</style> <style=cStack>(+" + Pct(jumpMult)  + " per stack, linear)</style>.";        
        protected override string GetLoreString(string langid = null) => "Order: Rusty Jetpack\n\nTracking Number: 761***********\nEstimated Delivery: 01/01/2056\nShipping Method: Priority\nShipping Address: Fun Center, 2105, NE Taurus\nShipping Details:\n\nSorry, it seems to be broken. It only works for a split second; maybe I'll send you a fully working jetpack in a few months? Should work well enough for the carnival!\n\nMake sure to keep the kiddos away from the bottom; it shoots out quite a jet. Can make for fun obstacle challenges.";

        public RustyJetpack() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/rustyjetpack_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/RustyJetpack.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }
        
        public override void Install() {
            base.Install();
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
            On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;
        }

        public override void Uninstall() {
            base.Uninstall();
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
            On.RoR2.CharacterBody.FixedUpdate -= On_CBFixedUpdate;
        }
        
        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            args.jumpPowerMultAdd += GetCount(sender) * jumpMult;
        }

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            if(!self.characterMotor) return;
            if(GetCount(self) > 0 && self.inputBank.jump.down && (
                    !PhotonJetpack.instance.enabled
                    || !ClassicItemsPlugin.globalConfig.coolYourJets
                    || (self.GetComponent<PhotonJetpackComponent>()?.fuel ?? 0f) <= 0f))
                self.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * gravMod;
        }
    }
}
