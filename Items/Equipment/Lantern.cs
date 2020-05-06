using BepInEx.Configuration;
using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using RoR2.Orbs;

namespace ThinkInvisible.ClassicItems {
    public class Lantern : ItemBoilerplate<Lantern> {
        public override string displayName {get;} = "Safeguard Lantern";

        [AutoItemCfg("Duration of the Safeguard Lantern effect.", default, 0f, float.MaxValue)]
        public float duration {get;private set;} = 10f;
		[AutoItemCfg("Base-player-damage/sec applied by Safeguard Lantern.", default, 0f, float.MaxValue)]
        public float damage {get;private set;} = 0.2f;
		[AutoItemCfg("Radius of the Safeguard Lantern aura.", default, 0f, float.MaxValue)]
        public float range {get;private set;} = 25f;

        private GameObject lanternWardPrefab;
        
        public override void SetupAttributesInner() {
            itemIsEquipment = true;

            eqpEnigmable = true;
			eqpIsLunar = true;
            eqpCooldown = 45;

            RegLang(
                "Drop a lantern that fears and damages enemies for 10 seconds.",
                "Sets a " + range.ToString("N0") + "-meter, " + duration.ToString("N0") + "-second AoE which <style=cIsUtility>fears enemies</style> and deals <style=cIsDamage>" + Pct(damage) + " damage per second</style>. <style=cIsUtility>Feared enemies will run out of melee</style>, <style=cDeath>but that won't stop them from shooting you.</style>" ,
                "A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
			var mshPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/WarbannerWard").transform.Find("Indicator");

			var lPrefabPrefab = new GameObject("LanternAuraPrefabPrefab");
			lPrefabPrefab.AddComponent<TeamFilter>();
			lPrefabPrefab.AddComponent<MeshFilter>().mesh = mshPrefab.GetComponentInChildren<MeshFilter>().mesh;
			lPrefabPrefab.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(mshPrefab.GetComponentInChildren<MeshRenderer>().material);
			lPrefabPrefab.GetComponent<MeshRenderer>().material.SetVector("_TintColor",new Vector4(0.3f,0.6f,1f,0.5f));
			var lw = lPrefabPrefab.AddComponent<LanternWard>();
			lw.rangeIndicator = lPrefabPrefab.GetComponent<MeshRenderer>().transform;
			lw.interval = 1f;
			lw.duration = duration;
			lw.radius = range;
			lw.damage = 0f;
			lanternWardPrefab = lPrefabPrefab.InstantiateClone("LanternAuraPrefab");
			UnityEngine.Object.Destroy(lPrefabPrefab);

            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }
        
        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
                if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
                var ctrlInst = UnityEngine.Object.Instantiate(lanternWardPrefab, slot.characterBody.corePosition, Quaternion.identity);
				var lw = ctrlInst.GetComponent<LanternWard>();
				lw.owner = slot.characterBody.gameObject;
				lw.GetComponent<TeamFilter>().teamIndex = slot.characterBody.teamComponent.teamIndex;
                NetworkServer.Spawn(ctrlInst);
				lw.damage = slot.characterBody.damage * damage;
				lw.duration = duration;
                if(Embryo.instance.CheckProc<Lantern>(slot.characterBody)) {
					lw.duration *= 2;
                }
                return true;
            } else return orig(slot, eqpid);
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
			var tind = TeamIndex.Monster | TeamIndex.Neutral | TeamIndex.Player;
			if(FriendlyFireManager.friendlyFireMode == FriendlyFireManager.FriendlyFireMode.Off) tind &= ~teamFilter.teamIndex;
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(tind);
			float sqrad = radius * radius;
			foreach(TeamComponent tcpt in teamMembers) {
				if ((tcpt.transform.position - transform.position).sqrMagnitude <= sqrad) {
					if (tcpt.body && tcpt.body.isActiveAndEnabled && tcpt.body.healthComponent && tcpt.body.mainHurtBox && owner != tcpt.body.gameObject) {
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