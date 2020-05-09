using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class BoxingGloves : Item<BoxingGloves> {
        public override string displayName => "Boxing Gloves";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidatePickupToken)]
        [AutoItemConfig("Percent chance for Boxing Gloves to proc; stacks multiplicatively.", AutoItemConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 6f;
        
        [AutoItemConfig("Multiplier for knockback force.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float procForce {get;private set;} = 50f;
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Hitting enemies have a " + Pct(procChance,0,1) + " chance to knock them back.";
        protected override string NewLangDesc(string langid = null) => "<style=cIsUtility>" + Pct(procChance,0,1) + "</style> <style=cStack>(+"+Pct(procChance,0,1)+" per stack, mult.)</style> chance to <style=cIsUtility>knock back</style> an enemy <style=cIsDamage>based on attack damage</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public BoxingGloves() { }

        protected override void LoadBehavior() {
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }
        protected override void UnloadBehavior() {
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
            if(di?.attacker) {
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
