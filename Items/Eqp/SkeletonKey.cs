using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class SkeletonKey : Equipment<SkeletonKey> {
        public override string displayName => "Skeleton Key";

        [AutoConfigRoOSlider("{0:N1} m", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Radius around the user to search for chests to open when using Skeleton Key.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float radius {get;private set;} = 50f;

		public override float cooldown {get;protected set;} = 90f;        
        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "Open all nearby chests.";
        protected override string GetDescString(string langid = null) => "Opens all <style=cIsUtility>chests</style> within <style=cIsUtility>" + radius.ToString("N0") + " m</style> for <style=cIsUtility>no cost</style>.";        
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";
        
        public SkeletonKey() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/skeletonkey_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/SkeletonKey.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            LanguageAPI.Add("EMBRYO_DESC_APPEND_SKELETONKEY", "\n<style=cStack>Beating Embryo: Double range.</style>");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            Embryo.RegisterHook(this.equipmentDef, "EMBRYO_DESC_APPEND_SKELETONKEY", () => "SkeletonKey");
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody) return false;
            if(SceneCatalog.mostRecentSceneDef.baseSceneName == "bazaar") return false;
            var sphpos = slot.characterBody.transform.position;
            var sphrad = radius;

            sphrad *= Embryo.CheckLastEmbryoProc(slot.characterBody, equipmentDef) + 1;
			Collider[] sphits = Physics.OverlapSphere(sphpos, sphrad, LayerIndex.defaultLayer.mask, QueryTriggerInteraction.Collide);
            bool foundAny = false;
            foreach(Collider c in sphits) {
                var ent = EntityLocator.GetEntity(c.gameObject);
                if(!ent) continue;
				var cptChest = ent.GetComponent<ChestBehavior>();
                if(!cptChest) continue;
                var cptPurch = ent.GetComponent<PurchaseInteraction>();
                if(cptPurch && cptPurch.available && cptPurch.costType == CostTypeIndex.Money) {
                    cptPurch.SetAvailable(false);
                    cptChest.Open();
                    foundAny = true;
                }
            }
            return foundAny;
        }
    }
}