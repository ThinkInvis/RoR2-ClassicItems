using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Lantern : Equipment<Lantern> {
        public override string displayName => "Safeguard Lantern";

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the Safeguard Lantern effect.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 10f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Base-player-damage/sec applied by Safeguard Lantern.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float damage {get;private set;} = 0.2f;

		[AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
		[AutoConfig("Radius of the Safeguard Lantern aura.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float range {get;private set;} = 25f;

        private GameObject lanternWardPrefab;

		public override bool isLunar => true;
        protected override string GetNameString(string langid = null) => displayName;
		protected override string GetPickupString(string langid = null) {
			string desc = "Drop a lantern that fears";
			if(damage > 0f) desc += " and damages";
			desc += $" enemies for {duration:N0} seconds.";
			return desc;
		}
		protected override string GetDescString(string langid = null) {
			string desc = $"Sets a {range:N0}-meter, {duration:N0}-second AoE which <style=cIsUtility>fears enemies</style>";
			if(damage > 0f) desc += $" and deals <style=cIsDamage>{Pct(damage)} damage per second</style>";
			desc += ". <style=cIsUtility>Feared enemies will run out of melee</style>, <style=cDeath>but that won't stop them from performing ranged attacks</style>.";
			return desc;
		}
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

		public Lantern() {
			iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/lantern_icon.png");
			modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Lantern.prefab");
		}

		public override void SetupAttributes() {
			base.SetupAttributes();
			var mshPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/WarbannerWard").transform.Find("Indicator");

			var lPrefabPrefab = new GameObject("LanternAuraPrefabPrefab");
			lPrefabPrefab.AddComponent<TeamFilter>();
			lPrefabPrefab.AddComponent<MeshFilter>().mesh = mshPrefab.GetComponentInChildren<MeshFilter>().mesh;
			lPrefabPrefab.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(mshPrefab.GetComponentInChildren<MeshRenderer>().material);
			lPrefabPrefab.GetComponent<MeshRenderer>().material.SetVector("_TintColor", new Vector4(0.3f, 0.6f, 1f, 0.5f));
			var lw = lPrefabPrefab.AddComponent<LanternWard>();
			lw.rangeIndicator = lPrefabPrefab.GetComponent<MeshRenderer>().transform;
			lw.interval = 1f;
			lw.duration = duration;
			lw.radius = range;
			lw.damage = 0f;
			lanternWardPrefab = lPrefabPrefab.InstantiateClone("LanternAuraPrefab", true);
			UnityEngine.Object.Destroy(lPrefabPrefab);
		}

		protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var ctrlInst = UnityEngine.Object.Instantiate(lanternWardPrefab, slot.characterBody.corePosition, Quaternion.identity);
			var lw = ctrlInst.GetComponent<LanternWard>();
			lw.owner = slot.characterBody.gameObject;
			lw.GetComponent<TeamFilter>().teamIndex = slot.characterBody.teamComponent.teamIndex;
            NetworkServer.Spawn(ctrlInst);
			lw.damage = slot.characterBody.damage * damage;
			lw.duration = duration;
			lw.radius = range;
            return true;
        }
	}

    [RequireComponent(typeof(TeamFilter))]
	public class LanternWard : NetworkBehaviour {
		[SyncVar]
		public float duration;
		[SyncVar]
		public float radius;
		[SyncVar]
		public float damage;

		public float interval;
		public Transform rangeIndicator;

		public GameObject owner;
		
		private TeamFilter teamFilter;
		private float rangeIndicatorScaleVelocity;

		private float stopwatch;
		private float lifeStopwatch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void Awake() {
			teamFilter = base.GetComponent<TeamFilter>();
			stopwatch = 0f;
			lifeStopwatch = 0f;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void Update() {
			float num = Mathf.SmoothDamp(rangeIndicator.localScale.x, radius*2f, ref rangeIndicatorScaleVelocity, 0.2f);
			rangeIndicator.localScale = new Vector3(num, num, num);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void FixedUpdate() {
			stopwatch -= Time.fixedDeltaTime;
			lifeStopwatch += Time.fixedDeltaTime;
			if(lifeStopwatch > duration) Destroy(this.gameObject);
			else if(stopwatch <= 0f) {
				stopwatch = interval;
				if(NetworkServer.active) {
					ServerProc();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void OnDestroy() {
			Destroy(rangeIndicator);
		}

		[Server]
		private void ServerProc() {
			var teamMembers = GatherEnemies(teamFilter.teamIndex);
			float sqrad = radius * radius;
			foreach(TeamComponent tcpt in teamMembers) {
				if ((tcpt.transform.position - transform.position).sqrMagnitude <= sqrad) {
					if (tcpt.body && tcpt.body.isActiveAndEnabled && tcpt.body.healthComponent && tcpt.body.mainHurtBox) {
						tcpt.body.AddTimedBuff(ClassicItemsPlugin.fearBuff, duration-lifeStopwatch);
						tcpt.body.healthComponent?.TakeDamage(new DamageInfo {
							attacker = owner,
							crit = owner?.GetComponent<CharacterBody>()?.RollCrit() ?? false,
							damage = this.damage*interval,
							damageColorIndex = DamageColorIndex.Item,
							damageType = DamageType.AOE,
							force = Vector3.zero,
							position = transform.position,
							procChainMask = default,
							procCoefficient = 1f
						});
					}
				}
			}
		}
	}
}