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
        
        [AutoItemConfig("Multiplier for knockback force vs. grounded targets.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float procForceGrounded {get;private set;} = 90f;

        [AutoItemConfig("Multiplier for knockback force vs. flying targets.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float procForceFlying {get;private set;} = 30f;
        
        [AutoItemConfig("If false, Boxing Gloves will not proc on bosses.", AutoItemConfigFlags.None)]
        public bool affectBosses {get;private set;} = false;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Hitting enemies have a " + Pct(procChance,0,1) + " chance to knock them back.";
        protected override string NewLangDesc(string langid = null) => "<style=cIsUtility>" + Pct(procChance,0,1) + "</style> <style=cStack>(+"+Pct(procChance,0,1)+" per stack, mult.)</style> chance to <style=cIsUtility>knock back</style> an enemy <style=cIsDamage>based on attack damage</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public BoxingGloves() {
            onBehav += () => {
                if(Compat_ItemStats.enabled) {
				    Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
					    ((count,inv,master)=>{return (1f-Mathf.Pow(1-procChance/100f,count))*100f;},
					    (value,inv,master)=>{return $"Knockback Chance: {Pct(value, 1, 1)}";}));
			    }
                if(Compat_BetterUI.enabled)
                    Compat_BetterUI.AddEffect(regIndex, procChance, procChance, Compat_BetterUI.ChanceFormatter, Compat_BetterUI.ExponentialStacking);
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }
        protected override void UnloadBehavior() {
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
            if(di?.attacker && (affectBosses || (!self.body?.isBoss ?? false))) {
                var cb = di.attacker.GetComponent<CharacterBody>();
                if(cb) {
                    var pChance = (1f-Mathf.Pow(1-procChance/100f,GetCount(cb)))*100f;
                    var proc = cb.master ? Util.CheckRoll(pChance,cb.master) : Util.CheckRoll(pChance);
                    if(proc) {
                        var mass = self.body.characterMotor?.mass ?? (self.body.rigidbody?.mass ?? 1f);
                        //var prcf = di.damage * procForce;
                        var prcf = (di.damage / self.fullCombinedHealth) * (self.body.isFlying ? procForceFlying : procForceGrounded);
                        if(di.force == Vector3.zero)
                            di.force += Vector3.Normalize(di.position - cb.corePosition) * prcf * mass;
                        else di.force += Vector3.Normalize(di.force) * prcf * mass;
                    }
                }
            }

            orig(self,di);
        }
    }
}
