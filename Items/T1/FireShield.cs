using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class FireShield : Item_V2<FireShield> {
        public override string displayName => "Fire Shield";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of max health required as damage taken to trigger Fire Shield.", AutoConfigFlags.None, 0f, 1f)]
        public float healthThreshold {get; private set;} = 0.1f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE radius for Fire Shield.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseRadius {get; private set;} = 15f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE damage, based on player base damage, for the first stack of Fire Shield.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDmg {get; private set;} = 2f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE damage, based on player base damage, per additional stack of Fire Shield.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float stackDmg {get; private set;} = 0.5f;

        [AutoConfig("If true, damage to shield and barrier (from e.g. Personal Shield Generator, Topaz Brooch) will not count towards triggering Fire Shield.")]
        public bool requireHealth {get; private set;} = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Retaliate on taking heavy damage.";
        protected override string GetDescString(string langid = null) {
            string desc = $"<style=cDeath>When hit";
            if(healthThreshold > 0f) desc += $" for more than {Pct(healthThreshold)} of max health";
            desc += $"</style>, <style=cIsDamage>explode</style> for up to <style=cIsDamage>{Pct(baseDmg)}</style>";
            if(stackDmg > 0f) desc += $" <style=cStack>(+{Pct(stackDmg)} per stack)</style>";
            desc += $" damage to enemies within <style=cIsDamage>{baseRadius:N0} m</style>.";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();

            if(Compat_ItemStats.enabled) {
                Compat_ItemStats.CreateItemStatDef(itemDef,
                    ((count, inv, master) => {
                        return baseDmg + (count - 1) * stackDmg;
                    },
                    (value, inv, master) => { return $"Fire Blast Damage: {Pct(value, 1)}"; }
                ));
            }
        }

        public override void Install() {
			base.Install();
			On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }
        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
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
