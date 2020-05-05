using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine.Networking;
using R2API;
using static ThinkInvisible.ClassicItems.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class FireShield : ItemBoilerplate<FireShield> {
        public override string itemCodeName {get;} = "FireShield";

        private ConfigEntry<float> cfgHealthThreshold;
        private ConfigEntry<float> cfgBaseRadius;
        private ConfigEntry<float> cfgBaseDmg;
        private ConfigEntry<float> cfgStackDmg;
        private ConfigEntry<bool> cfgRequireHealth;
        
        public float healthThreshold {get; private set;}
        public float baseRadius {get; private set;}
        public float baseDmg {get; private set;}
        public float stackDmg {get; private set;}
        public bool requireHealth {get; private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgHealthThreshold = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "HealthThreshold"), 0.1f, new ConfigDescription(
                "Fraction of max health required as damage taken to trigger Fire Shield.",
                new AcceptableValueRange<float>(0f, 1f)));
            cfgBaseRadius = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "BaseRadius"), 15f, new ConfigDescription(
                "AoE radius for Fire Shield.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));
            cfgBaseDmg = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "BaseDmg"), 2f, new ConfigDescription(
                "AoE damage, based on player base damage, for the first stack of Fire Shield.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));
            cfgStackDmg = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "StackDmg"), 0.5f, new ConfigDescription(
                "AoE damage, based on player base damage, per additional stack of Fire Shield.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));
            cfgRequireHealth = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "RequireHealth"), true, new ConfigDescription(
                "If true, damage to shield and barrier (from e.g. Personal Shield Generator, Topaz Brooch) will not count towards triggering Fire Shield."));

            healthThreshold = cfgHealthThreshold.Value;
            baseRadius = cfgBaseRadius.Value;
            baseDmg = cfgBaseDmg.Value;
            stackDmg = cfgStackDmg.Value;
            requireHealth = cfgRequireHealth.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "fireshield_model.prefab";
            iconPathName = "fireshield_icon.png";
            RegLang("Fire Shield",
            	"Retaliate on taking heavy damage.",
            	"<style=cDeath>When hit for more than " + pct(healthThreshold,1,1f) + " max health</style>, <style=cIsDamage>explode</style> for up to <style=cIsDamage>" + pct(baseDmg) + "</style> <style=cStack>(+" + pct(stackDmg) + " per stack)</style> damage to enemies within <style=cIsDamage>" + baseRadius.ToString("N0") + " m</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Damage};
            itemTier = ItemTier.Tier1;
        }

        protected override void SetupBehaviorInner() {
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
