using RoR2;
using RoR2.Orbs;
using System.Collections.ObjectModel;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;


namespace ThinkInvisible.ClassicItems {
    public class LostDoll : Equipment_V2<LostDoll> {
        public override string displayName => "Lost Doll";

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Fraction of the user's CURRENT health to take from the user when Lost Doll is activated.", AutoConfigFlags.None, 0f, 1f)]
        public float damageTaken {get;private set;} = 0.25f;

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Fraction of the user's MAXIMUM health to deal in damage to the closest enemy when Lost Doll is activated.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float damageGiven {get;private set;} = 5f;
        
		public override bool isLunar => true;
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) {
            string desc = "";
            if(damageTaken > 0f) desc += "Harm yourself to";
            else desc += "Use to";
            desc += " damage an enemy.";
            return desc;
        }
        protected override string NewLangDesc(string langid = null) {
            string desc = "";
            if(damageTaken > 0f) desc += $"Sacrifice <style=cIsDamage>{Pct(damageTaken)}</style> of your <style=cIsDamage>current health</style>";
            else desc += "Use";
            desc += $" to damage the nearest enemy for <style=cIsDamage>{Pct(damageGiven)}</style> of your <style=cIsDamage>maximum health</style>.";

            return desc;
        }
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public LostDoll() { }

        protected override bool OnEquipUseInner(EquipmentSlot slot) {
            if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var tpos = slot.characterBody.transform.position;
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers((TeamIndex.Player | TeamIndex.Neutral | TeamIndex.Monster) & ~slot.characterBody.teamComponent.teamIndex);
			float lowestDist = float.MaxValue;
			HurtBox result = null;
            float secondLowestDist = float.MaxValue;
            HurtBox result2 = null;
            foreach(TeamComponent tcpt in teamMembers) {
                if(!tcpt.body || !tcpt.body.isActiveAndEnabled || !tcpt.body.mainHurtBox) continue;
				float currDist = Vector3.SqrMagnitude(tcpt.transform.position - tpos);
				if(currDist < lowestDist) {
                    secondLowestDist = lowestDist;
                    result2 = result;

					lowestDist = currDist;
					result = tcpt.body.mainHurtBox;
				}
                if(currDist < secondLowestDist && currDist > lowestDist) {
                    secondLowestDist = currDist;
                    result2 = tcpt.body.mainHurtBox;
                }
			}
            var myHcpt = slot.characterBody?.healthComponent ?? null;
            bool didHit = false;
            if(myHcpt) {
                if(result) {
                    OrbManager.instance.AddOrb(new LostDollOrb {
                        attacker = slot.characterBody.gameObject,
                        damageColorIndex = DamageColorIndex.Default,
                        damageValue = myHcpt.fullCombinedHealth * damageGiven,
                        isCrit = false,
                        origin = slot.characterBody.corePosition,
                        target = result,
                        procCoefficient = 0f,
                        procChainMask = default,
                        scale = 10f
                    });
                    didHit = true;
                }
                if(result2 && instance.CheckEmbryoProc(slot.characterBody)) {
                    OrbManager.instance.AddOrb(new LostDollOrb {
                        attacker = slot.characterBody.gameObject,
                        damageColorIndex = DamageColorIndex.Default,
                        damageValue = myHcpt.fullCombinedHealth * damageGiven,
                        isCrit = false,
                        origin = slot.characterBody.corePosition,
                        target = result2,
                        procCoefficient = 0f,
                        procChainMask = default,
                        scale = 10f
                    });
                    didHit = true;
                }
                if(didHit) {                        
                    myHcpt.TakeDamage(new DamageInfo {
				        damage = myHcpt.combinedHealth * damageTaken,
				        position = slot.characterBody.corePosition,
                        force = Vector3.zero,
				        damageColorIndex = DamageColorIndex.Default,
				        crit = false,
				        attacker = null,
				        inflictor = null,
				        damageType = (DamageType.NonLethal | DamageType.BypassArmor | DamageType.BypassOneShotProtection),
				        procCoefficient = 0f,
				        procChainMask = default
                    });
                }
            }
            return didHit;
        }
    }

	public class LostDollOrb : Orb {
		public override void Begin() {
			base.duration = 1f;
			EffectData effectData = new EffectData {
				scale = this.scale,
				origin = this.origin,
				genericFloat = base.duration
			};
			effectData.SetHurtBoxReference(this.target);
			GameObject effectPrefab = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/InfusionOrbEffect");
			EffectManager.SpawnEffect(effectPrefab, effectData, true);
		}

		public override void OnArrival() {
			if (this.target) {
				HealthComponent healthComponent = this.target.healthComponent;
				if (healthComponent) {
                    DamageInfo damageInfo = new DamageInfo {
                        damage = this.damageValue,
                        attacker = this.attacker,
                        inflictor = null,
                        force = Vector3.zero,
                        crit = this.isCrit,
                        procChainMask = this.procChainMask,
                        procCoefficient = this.procCoefficient,
                        position = this.target.transform.position,
                        damageColorIndex = this.damageColorIndex
                    };
                    healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
				}
			}
		}

		public float damageValue;
		public GameObject attacker;
		public TeamIndex teamIndex;
		public bool isCrit;
		public float scale;
		public ProcChainMask procChainMask;
		public float procCoefficient = 0f;
		public DamageColorIndex damageColorIndex;
	}
}