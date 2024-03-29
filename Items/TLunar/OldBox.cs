﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class OldBox : Item<OldBox> {
        public override string displayName => "Old Box";
		public override ItemTier itemTier => ItemTier.Lunar;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemIsAIBlacklisted {get; protected set;} = true; //TODO: find a way to make fear work on players... random movement and forced sprint? halt movement (root)?

        [AutoConfigRoOSlider("{0:P0}", 0f, 1f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Fraction of max health required as damage taken to trigger Old Box (halved per additional stack).", AutoConfigFlags.None, 0f, 1f)]
        public float healthThreshold {get; private set;} = 0.5f;

        [AutoConfigRoOSlider("{0:N1} m", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE radius for Old Box.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float radius {get; private set;} = 25f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of fear debuff applied by Old Box.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get; private set;} = 2f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, damage to shield and barrier (from e.g. Personal Shield Generator, Topaz Brooch) will not count towards triggering Old Box.")]
        public bool requireHealth {get; private set;} = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to fear enemies when attacked.";
        protected override string GetDescString(string langid = null) => "<style=cDeath>When hit for more than " + Pct(healthThreshold) + " max health</style> <style=cStack>(/2 per stack)</style>, <style=cIsUtility>fear enemies</style> within <style=cIsUtility>" + radius.ToString("N0") + " m</style> for <style=cIsUtility>" + duration.ToString("N1") + " seconds</style>. <style=cIsUtility>Feared enemies will run out of melee</style>, <style=cDeath>but that won't stop them from performing ranged attacks</style>.";
        protected override string GetLoreString(string langid = null) => "Order: Old Box\n\nTracking Number: 361***********\nEstimated Delivery: 5/4/2056\nShipping Method: High Priority/Fragile\nShipping Address: Breezy Drive, Middle-land, Mars\nShipping Details:\n\nMusty, saggy, loose box. You can see it was used a lot already. Its bad shape honestly makes it scarier; you never know when the stupid thing pops out again.\nSometimes it sits in there for days!";

        public OldBox() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/oldbox_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/OldBox.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
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

            var teamMembers = GatherEnemies(self.body.teamComponent.teamIndex);
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
