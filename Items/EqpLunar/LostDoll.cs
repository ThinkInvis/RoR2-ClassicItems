﻿using RoR2;
using RoR2.Orbs;
using System.Collections.ObjectModel;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;


namespace ThinkInvisible.ClassicItems {
    public class LostDoll : Equipment<LostDoll> {
        public override string displayName => "Lost Doll";

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of the user's CURRENT health to take from the user when Lost Doll is activated.", AutoConfigFlags.None, 0f, 1f)]
        public float damageTaken {get;private set;} = 0.25f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of the user's MAXIMUM health to deal in damage to the closest enemy when Lost Doll is activated.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float damageGiven {get;private set;} = 5f;
        
		public override bool isLunar => true;
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) {
            string desc = "";
            if(damageTaken > 0f) desc += "Harm yourself to";
            else desc += "Use to";
            desc += " damage an enemy.";
            return desc;
        }
        protected override string GetDescString(string langid = null) {
            string desc = "";
            if(damageTaken > 0f) desc += $"Sacrifice <style=cIsDamage>{Pct(damageTaken)}</style> of your <style=cIsDamage>current health</style>";
            else desc += "Use";
            desc += $" to damage the nearest enemy for <style=cIsDamage>{Pct(damageGiven)}</style> of your <style=cIsDamage>maximum health</style>.";

            return desc;
        }
        protected override string GetLoreString(string langid = null) => "Order: Lost Doll\n\nTracking Number: 812***********\nEstimated Delivery: 2/12/2056\nShipping Method: Volatile/Military\nShipping Address: Tibb Station, Box Unknown, Venus\nShipping Details:\n\nGet this out of my house. Please. Just take it.\n\nSince I've recieved this god-forsaken thing, my husband has fallen down the stairs and broke his neck, my son got hit by a bus, and my daughter has drowned in the bathtub.\nAnd.. oh god.. I swear it moves around the house. I've tried leaving it locked in a safe, and it will be out the next day. It won't burn. Cutting it has resulted in the amputation of both my arms.\n\nPlease..";

        public LostDoll() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/lostdoll_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/LostDoll.prefab");
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var tpos = slot.characterBody.transform.position;
            var teamMembers = GatherEnemies(slot.characterBody.teamComponent.teamIndex);
			float lowestDist = float.MaxValue;
			HurtBox result = null;
            foreach(TeamComponent tcpt in teamMembers) {
                if(!tcpt.body || !tcpt.body.isActiveAndEnabled || !tcpt.body.mainHurtBox) continue;
				float currDist = Vector3.SqrMagnitude(tcpt.transform.position - tpos);
				if(currDist < lowestDist) {
					lowestDist = currDist;
					result = tcpt.body.mainHurtBox;
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
			GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/InfusionOrbEffect");
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