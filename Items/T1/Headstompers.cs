using RoR2;
using System;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Headstompers : Item<Headstompers> {
        public override string displayName => "Headstompers";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Multiplier for player base damage applied by explosion.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float baseDamage {get;private set;} = 5f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Added to BaseDamage per extra stack.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float stackDamage {get;private set;} = 0.5f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Minimum vertical velocity required to trigger Headstompers.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float velThreshold {get;private set;} = 20f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Additional vertical velocity required for max damage (scales linearly from 0 @ VelThreshold).", AutoItemConfigFlags.None, 0f, float.MaxValue)]

        public float velMax {get;private set;} = 40f;
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Hurt enemies by falling.";
        protected override string NewLangDesc(string langid = null) {
            string desc = $"Hitting the ground faster than <style=cIsDamage>{velThreshold:N1} meters per second</style> vertically causes a <style=cIsDamage>10-meter</style> radius <style=cIsDamage>kinetic explosion</style>, dealing up to <style=cIsDamage>{Pct(baseDamage)} base damage</style>";
            if(stackDamage > 0f) desc += $"<style=cStack>(+{Pct(stackDamage)} per stack, linear)</style>";
            desc += $". <style=cIsDamage>Max damage</style> requires <style=cIsDamage>{(velMax + velThreshold):N1} meters per second falling speed</style>.";
            return desc;
        }
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Headstompers() {
            onBehav += () => {
			    if(Compat_ItemStats.enabled) {
				    Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
					    ((count,inv,master)=>{return baseDamage + stackDamage * (count - 1);},
					    (value,inv,master)=>{return $"Stomp Damage: {Pct(value, 1)}";}));
			    }
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.CharacterMotor.OnHitGround += On_CMOnHitGround;
        }
        protected override void UnloadBehavior() {
            On.RoR2.CharacterMotor.OnHitGround -= On_CMOnHitGround;
        }

        private void On_CMOnHitGround(On.RoR2.CharacterMotor.orig_OnHitGround orig, CharacterMotor self, CharacterMotor.HitGroundInfo ghi) {
            orig(self,ghi);
            if(!self.body) return;
            if(GetCount(self.body) > 0 && Math.Abs(ghi.velocity.y) > velThreshold) {
                float scalefac = Mathf.Lerp(0f, baseDamage + (GetCount(self.body) - 1f) * stackDamage,
                    Mathf.InverseLerp(velThreshold, velMax+velThreshold, Math.Abs(ghi.velocity.y)));
                //most properties borrowed from H3AD-5T v2
				BlastAttack blastAttack = new BlastAttack {
                    attacker = self.body.gameObject,
					inflictor = self.body.gameObject,
					teamIndex = TeamComponent.GetObjectTeam(self.body.gameObject),
					position = ghi.position,
					procCoefficient = 0.5f,
					radius = 10f,
					baseForce = 2000f,
					bonusForce = Vector3.up * 2000f,
					baseDamage = self.body.damage * scalefac,
					falloffModel = BlastAttack.FalloffModel.SweetSpot,
					crit = Util.CheckRoll(self.body.crit, self.body.master),
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
