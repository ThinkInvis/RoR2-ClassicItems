﻿using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Spikestrip : Item<Spikestrip> {
        public override string displayName => "Spikestrip";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AutoConfigRoOSlider("{0:N1} m", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE radius for Spikestrip.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseRadius {get; private set;} = 5f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE duration for the first stack of Spikestrip, in seconds.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDuration {get; private set;} = 2f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("AoE duration per additional stack of Spikestrip, in seconds.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float stackDuration {get; private set;} = 1f;

		internal static GameObject spikeWardPrefab;
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Drop spikestrips on being hit, slowing enemies.";
        protected override string GetDescString(string langid = null) {
            string desc = $"<style=cIsDamage>When hit</style>, drop a <style=cIsUtility>{baseRadius:N0}-meter AoE</style> which <style=cIsUtility>slows enemies by 50%</style> and lasts <style=cIsUtility>{baseDuration:N1} seconds</style>";
            if(stackDuration > 0f) desc += $" <style=cStack>(+{stackDuration:N1} per stack)</style>";
            desc += ".";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "Order: Spikestrip\n\nTracking Number: 599***********\nEstimated Delivery: 1/18/2056\nShipping Method: Standard\nShipping Address: D300, Enf. Station, Jupiter\nShipping Details:\n\nHey bud! Do you remember... summer of 2032, was it? The night with Alicia? Well, um, remember when we got waay too drunk and broke into the police station? Well, I still have those spikestrips from then. Haha, good times, right?\n\nThe doctors say I don't have much time left. Since you're in the force now and all, I felt obligated to return it to you! Haha. So.. yeah. Hope you're doing well. It's been lonely here without you.";

        public Spikestrip() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/spikestrip_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/SpikeStrip.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            var mshPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MushroomWard");

            var bwPrefabPrefab = new GameObject("SpikestripAuraPrefabPrefab");
            bwPrefabPrefab.AddComponent<NetworkIdentity>();
            bwPrefabPrefab.AddComponent<TeamFilter>();
            var bwPfb2ModelAdjust = new GameObject("SpikestripAuraModelAdjust");
            bwPfb2ModelAdjust.transform.parent = bwPrefabPrefab.transform;
            bwPfb2ModelAdjust.AddComponent<MeshFilter>().mesh = mshPrefab.GetComponentInChildren<MeshFilter>().mesh;
            bwPfb2ModelAdjust.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(mshPrefab.GetComponentInChildren<MeshRenderer>().material);
            bwPfb2ModelAdjust.GetComponent<MeshRenderer>().material.SetVector("_TintColor", new Vector4(0.6f, 0.06f, 0.5f, 0.3f));
            bwPrefabPrefab.transform.localScale *= 2f;
            var bw = bwPrefabPrefab.AddComponent<BuffWard>();
            bw.invertTeamFilter = true;
            bw.rangeIndicator = bwPfb2ModelAdjust.GetComponent<MeshRenderer>().transform;
            bw.expires = false;
            bw.Networkradius = baseRadius;
            bw.buffDuration = 0.5f;
            bw.interval = 0.5f;
            bw.buffDef = RoR2Content.Buffs.Slow50;
            spikeWardPrefab = bwPrefabPrefab.InstantiateClone("SpikestripAuraPrefab", true);
            UnityEngine.Object.Destroy(bwPrefabPrefab);
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
			orig(self, di);
			
			int icnt = GetCount(self.body);
			if(icnt < 1) return;
			var cpt = UnityEngine.Object.Instantiate(spikeWardPrefab, self.body.footPosition, Quaternion.identity);
            cpt.GetComponent<BuffWard>().Networkradius = baseRadius;
			cpt.GetComponent<TeamFilter>().teamIndex = self.body.teamComponent.teamIndex;
            cpt.AddComponent<DestroyOnTimer>().duration = baseDuration + stackDuration * (icnt - 1);
            NetworkServer.Spawn(cpt);
        }
	}
}
