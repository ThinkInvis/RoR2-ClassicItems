using RoR2;
using System;
using UnityEngine;
using R2API.Utils;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Headstompers : Item<Headstompers> {
        public override string displayName => "Headstompers";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Multiplier for player base damage applied by explosion.", AICFlags.None, 0f, float.MaxValue)]
        public float baseDamage {get;private set;} = 5f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Added to BaseDamage per extra stack.", AICFlags.None, 0f, float.MaxValue)]
        public float stackDamage {get;private set;} = 0.5f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Minimum vertical velocity required to trigger Headstompers.", AICFlags.None, 0f, float.MaxValue)]
        public float velThreshold {get;private set;} = 20f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Additional vertical velocity required for max damage (scales linearly from 0 @ VelThreshold).", AICFlags.None, 0f, float.MaxValue)]

        public float velMax {get;private set;} = 40f;
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Hurt enemies by falling.";
        protected override string NewLangDesc(string langid = null) => "Hitting the ground faster than <style=cIsDamage>" + velThreshold.ToString("N1") + " m/s</style> (vertical component only) causes a <style=cIsDamage>10 m</style> radius <style=cIsDamage>kinetic explosion</style>, dealing up to <style=cIsDamage>" + Pct(baseDamage) + " base damage</style> <style=cStack>(+" + Pct(stackDamage) + " per stack, linear)</style>. <style=cIsDamage>Max damage</style> requires <style=cIsDamage>" + (velMax+velThreshold).ToString("N1") + " m/s falling speed</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Headstompers() {}

        protected override void LoadBehavior() {
            On.RoR2.CharacterMotor.OnHitGround += On_CMOnHitGround;
        }
        protected override void UnloadBehavior() {
            On.RoR2.CharacterMotor.OnHitGround -= On_CMOnHitGround;
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
