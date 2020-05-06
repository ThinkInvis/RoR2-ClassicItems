using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using System.Collections.ObjectModel;

namespace ThinkInvisible.ClassicItems {
    public class Spikestrip : Item<Spikestrip> {
        public override string displayName => "Spikestrip";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AutoItemCfg("AoE radius for Spikestrip.", default, 0f, float.MaxValue)]
        public float baseRadius {get; private set;} = 5f;
        [AutoItemCfg("AoE duration for the first stack of Spikestrip, in seconds.", default, 0f, float.MaxValue)]
        public float baseDuration {get; private set;} = 2f;
        [AutoItemCfg("AoE duration per additional stack of Spikestrip, in seconds.", default, 0f, float.MaxValue)]
        public float stackDuration {get; private set;} = 1f;

		internal static GameObject spikeWardPrefab;

        public override void SetupAttributesInner() {
            RegLang(
            	"Drop spikestrips on being hit, slowing enemies.",
            	"<style=cIsDamage>When hit</style>, drop a <style=cIsUtility>" + baseRadius.ToString("N0") + " m AoE</style> which <style=cIsUtility>slows enemies by 50%</style> and lasts <style=cIsUtility>" + baseDuration.ToString("N1") + " s</style> <style=cStack>(+" + stackDuration.ToString("N1") + " s per stack)</style>.",
            	"A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
			var mshPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/MushroomWard");

			var bwPrefabPrefab = new GameObject("SpikestripAuraPrefabPrefab");
            bwPrefabPrefab.AddComponent<NetworkIdentity>();
			bwPrefabPrefab.AddComponent<TeamFilter>();
            var bwPfb2ModelAdjust = new GameObject("SpikestripAuraModelAdjust");
            bwPfb2ModelAdjust.transform.parent = bwPrefabPrefab.transform;
			bwPfb2ModelAdjust.AddComponent<MeshFilter>().mesh = mshPrefab.GetComponentInChildren<MeshFilter>().mesh;
			bwPfb2ModelAdjust.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(mshPrefab.GetComponentInChildren<MeshRenderer>().material);
			bwPfb2ModelAdjust.GetComponent<MeshRenderer>().material.SetVector("_TintColor",new Vector4(0.6f,0.06f,0.5f,0.3f));
            bwPrefabPrefab.transform.localScale *= 2f;
			var bw = bwPrefabPrefab.AddComponent<BuffWard>();
            bw.invertTeamFilter = true;
			bw.rangeIndicator = bwPfb2ModelAdjust.GetComponent<MeshRenderer>().transform;
            bw.expires = false;
            bw.Networkradius = baseRadius;
            bw.buffDuration = 0.5f;
            bw.interval = 0.5f;
            bw.buffType = BuffIndex.Slow50;
			spikeWardPrefab = bwPrefabPrefab.InstantiateClone("SpikestripAuraPrefab");
			UnityEngine.Object.Destroy(bwPrefabPrefab);
			
			On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
			orig(self, di);
			
			int icnt = GetCount(self.body);
			if(icnt < 1) return;
			var cpt = UnityEngine.Object.Instantiate(spikeWardPrefab, self.body.footPosition, Quaternion.identity);
			cpt.GetComponent<TeamFilter>().teamIndex = self.body.teamComponent.teamIndex;
            cpt.AddComponent<DestroyOnTimer>().duration = baseDuration + stackDuration * (icnt - 1);
            NetworkServer.Spawn(cpt);
        }
	}
}
