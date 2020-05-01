using BepInEx.Configuration;
using RoR2;
using RoR2.Orbs;
using System.Collections.ObjectModel;
using UnityEngine;
using static ThinkInvisible.ClassicItems.ClassicItemsPlugin.MasterItemList;

namespace ThinkInvisible.ClassicItems {
    public class LostDoll : ItemBoilerplate {
        public override string itemCodeName {get;} = "LostDoll";

        private ConfigEntry<float> cfgDamageTaken;
        private ConfigEntry<float> cfgDamageGiven;

        public float damageTaken {get;private set;}
        public float damageGiven {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgDamageTaken = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "DamageTaken"), 0.25f, new ConfigDescription(
                "Fraction of CURRENT health to take from the user when Lost Doll is activated.",
                new AcceptableValueRange<float>(0f, 1f)));
            cfgDamageGiven = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "DamageGiven"), 5f, new ConfigDescription(
                "Fraction of MAXIMUM health to deal in damage to the closest enemy when Lost Doll is activated.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));

            damageTaken = cfgDamageTaken.Value;
            damageGiven = cfgDamageGiven.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemIsEquipment = true;
            eqpIsLunar = true;

            modelPathName = "lostdollcard.prefab";
            iconPathName = "lostdoll_icon.png";
            eqpEnigmable = true;
            eqpCooldown = 45;

            RegLang("Lost Doll",
                "Harm yourself to instantly kill an enemy.",
                "Sacrifices <style=cIsDamage>25%</style> of your <style=cIsDamage>current health</style> to damage the nearest enemy for <style=cIsDamage>500%</style> of your <style=cIsDamage>maximum health</style>.",
                "A relic of times long past (ClassicItems mod)");
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }
        
        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
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
                            procChainMask = default(ProcChainMask),
                            scale = 10f
                        });
                        didHit = true;
                    }
                    if(result2 && embryo.subEnableLostDoll && Util.CheckRoll(embryo.GetCount(slot.characterBody)*embryo.procChance)) {
                        OrbManager.instance.AddOrb(new LostDollOrb {
                            attacker = slot.characterBody.gameObject,
                            damageColorIndex = DamageColorIndex.Default,
                            damageValue = myHcpt.fullCombinedHealth * damageGiven,
                            isCrit = false,
                            origin = slot.characterBody.corePosition,
                            target = result2,
                            procCoefficient = 0f,
                            procChainMask = default(ProcChainMask),
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
				            procChainMask = default(ProcChainMask)
                        });
                    }
                }
                return didHit;
            } else return orig(slot, eqpid);
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
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.damage = this.damageValue;
					damageInfo.attacker = this.attacker;
					damageInfo.inflictor = null;
					damageInfo.force = Vector3.zero;
					damageInfo.crit = this.isCrit;
					damageInfo.procChainMask = this.procChainMask;
					damageInfo.procCoefficient = this.procCoefficient;
					damageInfo.position = this.target.transform.position;
					damageInfo.damageColorIndex = this.damageColorIndex;
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