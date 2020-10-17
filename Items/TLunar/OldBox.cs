using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class OldBox : Item_V2<OldBox> {
        public override string displayName => "Old Box";
		public override ItemTier itemTier => ItemTier.Lunar;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemIsAIBlacklisted {get; protected set;} = true; //TODO: find a way to make fear work on players... random movement and forced sprint? halt movement (root)?

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of max health required as damage taken to trigger Old Box (halved per additional stack).", AutoConfigFlags.None, 0f, 1f)]
        public float healthThreshold {get; private set;} = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE radius for Old Box.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float radius {get; private set;} = 25f;
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of fear debuff applied by Old Box.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get; private set;} = 2f;

        [AutoConfig("If true, damage to shield and barrier (from e.g. Personal Shield Generator, Topaz Brooch) will not count towards triggering Old Box.")]
        public bool requireHealth {get; private set;} = true;
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to fear enemies when attacked.";
        protected override string GetDescString(string langid = null) => "<style=cDeath>When hit for more than " + Pct(healthThreshold) + " max health</style> <style=cStack>(/2 per stack)</style>, <style=cIsUtility>fear enemies</style> within <style=cIsUtility>" + radius.ToString("N0") + " m</style> for <style=cIsUtility>" + duration.ToString("N1") + " seconds</style>. <style=cIsUtility>Feared enemies will run out of melee</style>, <style=cDeath>but that won't stop them from performing ranged attacks</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();
            if(Compat_ItemStats.enabled) {
				Compat_ItemStats.CreateItemStatDef(itemDef,
					((count,inv,master)=>{return healthThreshold * Mathf.Pow(2, 1-count);},
					(value,inv,master)=>{return $"Health Threshold: {Pct(value, 1)}";}));
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
            float adjThreshold = healthThreshold * Mathf.Pow(2, 1-icnt);
			if(icnt < 1
                || (requireHealth && (oldHealth - self.health)/self.fullHealth < adjThreshold)
                || (!requireHealth && (oldCH - self.combinedHealth)/self.fullCombinedHealth < adjThreshold))
                return;

            /*Vector3 corePos = Util.GetCorePosition(self.body);
			var thisThingsGonnaX = GlobalEventManager.instance.explodeOnDeathPrefab;
			var x = thisThingsGonnaX.GetComponent<DelayBlast>();
			EffectManager.SpawnEffect(x.explosionEffect, new EffectData {
				origin = corePos,
				rotation = Quaternion.identity,
                color = Color.blue,
				scale = radius
			}, true);*/

            var tind = TeamIndex.Monster | TeamIndex.Neutral | TeamIndex.Player;
			tind &= ~self.body.teamComponent.teamIndex;
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(tind);
			float sqrad = radius * radius;
			foreach(TeamComponent tcpt in teamMembers) {
				if ((tcpt.transform.position - self.body.corePosition).sqrMagnitude <= sqrad) {
					if (tcpt.body && tcpt.body.mainHurtBox && tcpt.body.isActiveAndEnabled) {
                        tcpt.body.AddTimedBuff(ClassicItemsPlugin.fearBuff, duration);
					}
				}
			}
        }
	}
}
