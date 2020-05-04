using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class BoxingGloves : ItemBoilerplate<BoxingGloves> {
        public override string itemCodeName {get;} = "BoxingGloves";

        private ConfigEntry<float> cfgProcChance;
        private ConfigEntry<float> cfgProcForce;

        public float procChance {get;private set;}
        public float procForce {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgProcChance = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "ProcChance"), 6f, new ConfigDescription(
                "Percent chance for Boxing Gloves to proc; stacks multiplicatively.",
                new AcceptableValueRange<float>(0f,100f)));
            cfgProcForce = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "ProcForce"), 50f, new ConfigDescription(
                "Multiplier for knockback force.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));

            procChance = cfgProcChance.Value;
            procForce = cfgProcForce.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "boxinggloves_model.prefab";
            iconPathName = "boxinggloves_icon.png";
            RegLang("Boxing Gloves",
            	"Hitting enemies have a " + pct(procChance,0,1) + " chance to knock them back.",
            	"<style=cIsUtility>" + pct(procChance,0,1) + "</style> <style=cStack>(+"+pct(procChance,0,1)+" per stack, mult.)</style> chance to <style=cIsUtility>knock back</style> an enemy <style=cIsDamage>based on attack damage</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier2;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
            if(di.attacker) {
                var cb = di.attacker.GetComponent<CharacterBody>();
                if(cb) {
                    var pChance = (1f-Mathf.Pow(1-procChance/100f,GetCount(cb)))*100f;
                    var proc = cb.master ? Util.CheckRoll(pChance,cb.master) : Util.CheckRoll(pChance);
                    if(proc) {
                        var prcf = di.damage * procForce;
                        if(di.force == Vector3.zero)
                            di.force += Vector3.Normalize(di.position - cb.corePosition) * prcf;
                        else di.force += Vector3.Normalize(di.force) * prcf;
                    }
                }
            }

            orig(self,di);
        }
    }
}
