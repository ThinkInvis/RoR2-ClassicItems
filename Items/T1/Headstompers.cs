using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;
using R2API.Utils;

namespace ThinkInvisible.ClassicItems {
    public class Headstompers : ItemBoilerplate<Headstompers> {
        public override string itemCodeName {get;} = "Headstompers";

        private ConfigEntry<float> cfgBaseDamage;
        private ConfigEntry<float> cfgStackDamage;
        private ConfigEntry<float> cfgVelThreshold;
        private ConfigEntry<float> cfgVelMax;

        public float baseDamage {get;private set;}
        public float stackDamage {get;private set;}
        public float velThreshold {get;private set;}
        public float velMax {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgBaseDamage = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "BaseDamage"), 5f, new ConfigDescription(
                "Multiplier for player base damage applied by explosion.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgStackDamage = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "StackDamage"), 0.5f, new ConfigDescription(
                "Added to BaseDamage per extra stack.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgVelThreshold = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "VelThreshold"), 20f, new ConfigDescription(
                "Minimum vertical velocity required to trigger Headstompers.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgVelMax = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "VelMax"), 40f, new ConfigDescription(
                "Additional vertical velocity required for max damage (scales linearly from 0 @ VelThreshold).",
                new AcceptableValueRange<float>(0f,float.MaxValue)));

            baseDamage = cfgBaseDamage.Value;
            stackDamage = cfgStackDamage.Value;
            velThreshold = cfgVelThreshold.Value;
            velMax = cfgVelMax.Value;
        }

        protected override void SetupAttributesInner() {
            modelPathName = "headstompers_model.prefab";
            iconPathName = "headstompers_icon.png";
            RegLang("Headstompers",
            	"Hurt enemies by falling.",
            	"Hitting the ground faster than <style=cIsDamage>" + velThreshold.ToString("N1") + " m/s</style> (vertical component only) causes a <style=cIsDamage>10 m</style> radius <style=cIsDamage>kinetic explosion</style>, dealing up to <style=cIsDamage>" + pct(baseDamage) + " base damage</style> <style=cStack>(+" + pct(stackDamage) + " per stack, linear)</style>. <style=cIsDamage>Max damage</style> requires <style=cIsDamage>" + (velMax+velThreshold).ToString("N1") + " m/s falling speed</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Damage};
            itemTier = ItemTier.Tier1;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.CharacterMotor.OnHitGround += On_CMOnHitGround;
        }

        private void On_CMOnHitGround(On.RoR2.CharacterMotor.orig_OnHitGround orig, CharacterMotor self, CharacterMotor.HitGroundInfo ghi) {
            orig(self,ghi);
            CharacterBody body = self.GetFieldValue<CharacterBody>("body");
            if(!body) return;
            if(GetCount(body) > 0 && Math.Abs(ghi.velocity.y) > velThreshold) {
                float scalefac = Mathf.Lerp(0f, baseDamage + (GetCount(body) - 1f) * stackDamage,
                    Mathf.InverseLerp(velThreshold, velMax+velThreshold, Math.Abs(ghi.velocity.y)));
                //most properties borrowed from H3AD-5T v2
				BlastAttack blastAttack = new BlastAttack {
                    attacker = body.gameObject,
					inflictor = body.gameObject,
					teamIndex = TeamComponent.GetObjectTeam(body.gameObject),
					position = ghi.position,
					procCoefficient = 0.5f,
					radius = 10f,
					baseForce = 2000f,
					bonusForce = Vector3.up * 2000f,
					baseDamage = body.damage * scalefac,
					falloffModel = BlastAttack.FalloffModel.SweetSpot,
					crit = Util.CheckRoll(body.crit, body.master),
					damageColorIndex = DamageColorIndex.Item,
                    attackerFiltering = AttackerFiltering.NeverHit
                };
				blastAttack.Fire();
				EffectData effectData = new EffectData {
					origin = ghi.position,
					scale = 10f
                };
				EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/BootShockwave"), effectData, true);
            }
        }
    }
}
