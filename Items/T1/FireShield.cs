using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using static ThinkInvisible.ClassicItems.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class FireShield : ItemBoilerplate<FireShield> {
        public override string displayName {get;} = "Fire Shield";
        
        [AutoItemCfg("Fraction of max health required as damage taken to trigger Fire Shield.", default, 0f, 1f)]
        public float healthThreshold {get; private set;} = 0.1f;
        [AutoItemCfg("AoE radius for Fire Shield.", default, 0f, float.MaxValue)]
        public float baseRadius {get; private set;} = 15f;
        [AutoItemCfg("AoE damage, based on player base damage, for the first stack of Fire Shield.", default, 0f, float.MaxValue)]
        public float baseDmg {get; private set;} = 2f;
        [AutoItemCfg("AoE damage, based on player base damage, per additional stack of Fire Shield.", default, 0f, float.MaxValue)]
        public float stackDmg {get; private set;} = 0.5f;
        [AutoItemCfg("If true, damage to shield and barrier (from e.g. Personal Shield Generator, Topaz Brooch) will not count towards triggering Fire Shield.")]
        public bool requireHealth {get; private set;} = true;

        public override void SetupAttributesInner() {
            RegLang(
            	"Retaliate on taking heavy damage.",
            	"<style=cDeath>When hit for more than " + Pct(healthThreshold) + " max health</style>, <style=cIsDamage>explode</style> for up to <style=cIsDamage>" + Pct(baseDmg) + "</style> <style=cStack>(+" + Pct(stackDmg) + " per stack)</style> damage to enemies within <style=cIsDamage>" + baseRadius.ToString("N0") + " m</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Damage};
            itemTier = ItemTier.Tier1;
        }

        public override void SetupBehaviorInner() {
			On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
            var oldHealth = self.health;
            var oldCH = self.combinedHealth;

			orig(self, di);

			int icnt = GetCount(self.body);
			if(icnt < 1
                || (requireHealth && (oldHealth - self.health)/self.fullHealth < healthThreshold)
                || (!requireHealth && (oldCH - self.combinedHealth)/self.fullCombinedHealth < healthThreshold))
                return;

            Vector3 corePos = Util.GetCorePosition(self.body);
			var thisThingsGonnaX = GlobalEventManager.instance.explodeOnDeathPrefab;
			var x = thisThingsGonnaX.GetComponent<DelayBlast>();
			EffectManager.SpawnEffect(x.explosionEffect, new EffectData {
				origin = corePos,
				rotation = Quaternion.identity,
				scale = baseRadius
			}, true);
			new BlastAttack {
				position = corePos,
		        baseDamage = self.body.damage * (baseDmg + (icnt-1) * stackDmg),
		        baseForce = 2000f,
		        bonusForce = Vector3.up * 1000f,
		        radius = baseRadius,
		        attacker = self.gameObject,
		        inflictor = null,
                crit = Util.CheckRoll(self.body.crit, self.body.master),
                damageColorIndex = DamageColorIndex.Item,
                falloffModel = BlastAttack.FalloffModel.Linear,
                attackerFiltering = AttackerFiltering.NeverHit,
				teamIndex = self.body.teamComponent?.teamIndex ?? default,
				damageType = DamageType.AOE,
				procCoefficient = 1.0f
			}.Fire();
        }
	}
}
