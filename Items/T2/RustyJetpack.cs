using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class RustyJetpack : Item_V2<RustyJetpack> {
        public override string displayName => "Rusty Jetpack";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Multiplier for gravity reduction (0.0 = no effect, 1.0 = full anti-grav).", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float gravMod {get;private set;} = 0.5f;
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage | AutoUpdateEventFlags_V2.InvalidateStats)]
        [AutoConfig("Amount added to jump power per stack.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float jumpMult {get;private set;} = 0.1f;

        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "Increase jump height and reduce gravity.";        
        protected override string GetDescString(string langid = null) => "<style=cIsUtility>Reduces gravity</style> by <style=cIsUtility>" + Pct(gravMod) + "</style> while <style=cIsUtility>holding jump</style>. Increases <style=cIsUtility>jump power</style> by <style=cIsUtility>" + Pct(jumpMult) + "</style> <style=cStack>(+" + Pct(jumpMult)  + " per stack, linear)</style>.";        
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();

			if(Compat_ItemStats.enabled) {
				Compat_ItemStats.CreateItemStatDef(itemDef,
					((count,inv,master)=>{return jumpMult*count;},
					(value,inv,master)=>{return $"Jump Power: +{value.ToString("N1")}";}));
			}
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
