using BepInEx.Configuration;
using RoR2;
using R2API;
using System;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using static ThinkInvisible.ClassicItems.ClassicItemsPlugin.MasterItemList;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace ThinkInvisible.ClassicItems
{
    public class SkeletonKey : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "SkeletonKey";

        private ConfigEntry<float> cfgRadius;

        public float radius {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgRadius = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Radius"), 25f, new ConfigDescription(
                "Radius around the user to search for chests to open when using Skeleton Key.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));

            radius = cfgRadius.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemIsEquipment = true;

            modelPathName = "skeletonkeycard.prefab";
            iconPathName = "skeletonkey_icon.png";
            itemEnigmable = true;
            itemCooldown = 90;

            RegLang("Skeleton Key",
                "Open all nearby chests.",
                "Opens all <style=cIsUtility>chests</style> within <style=cIsUtility>" + radius.ToString("N0") + " m</style> for <style=cIsUtility>no cost</style>.",
                "A relic of times long past (ClassicItems mod)");
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }
        
        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
                if(!slot.characterBody) return false;
                var sphpos = slot.characterBody.transform.position;
                var sphrad = radius;
                if(slot.characterBody && Util.CheckRoll(embryo.GetCount(slot.characterBody)*embryo.procChance)) sphrad *= 2;
			    Collider[] sphits = Physics.OverlapSphere(sphpos, sphrad, LayerIndex.defaultLayer.mask, QueryTriggerInteraction.Collide);
                bool foundAny = false;
                foreach(Collider c in sphits) {
                    var ent = EntityLocator.GetEntity(c.gameObject);
                    if(!ent) continue;
				    var cptChest = ent.GetComponent<ChestBehavior>();
                    if(!cptChest) continue;
                    var cptPurch = ent.GetComponent<PurchaseInteraction>();
                    if(cptPurch && cptPurch.costType == CostTypeIndex.Money) {
                        cptPurch.SetAvailable(false);
                        cptChest.Open();
                        foundAny = true;
                    }
                }
                return foundAny;
            } else return orig(slot, eqpid);
        }
    }
}