using RoR2;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using UnityEngine.Networking;
using System.Collections.ObjectModel;
using R2API;
using RoR2.Orbs;

namespace ThinkInvisible.ClassicItems {
    public class BarbedWire : Item<BarbedWire> {
        public override string displayName => "Barbed Wire";
		
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});

		[AutoItemCfg("AoE radius for the first stack of Barbed Wire.", default, 0f, float.MaxValue)]
        public float baseRadius {get; private set;} = 5f;
		[AutoItemCfg("AoE radius to add per additional stack of Barbed Wire.", default, 0f, float.MaxValue)]
        public float stackRadius {get; private set;} = 1f;
		[AutoItemCfg("AoE damage/sec (as fraction of owner base damage) for the first stack of Barbed Wire.", default, 0f, float.MaxValue)]
        public float baseDmg {get; private set;} = 0.5f;
		[AutoItemCfg("AoE damage/sec (as fraction of owner base damage) per additional stack of Barbed Wire.", default, 0f, float.MaxValue)]
        public float stackDmg {get; private set;} = 0.15f;
		[AutoItemCfg("If true, Barbed Wire only affects one target at most. If false, Barbed Wire affects every target in range.")]
		public bool oneOnly {get; private set;} = true;
		[AutoItemCfg("If true, deployables (e.g. Engineer turrets) with Barbed Wire will benefit from their master's damage. Deployables usually have 0 damage stat by default, and will not otherwise be able to use Barbed Wire.")]
        public bool inclDeploys {get;private set;} = true;

		internal static GameObject barbedWardPrefab;
        
        public override void SetupAttributesInner() {
            RegLang(
            	"Hurt nearby enemies.",
            	"Deal <style=cIsDamage>" + Pct(baseDmg) + "</style> <style=cStack>(+" + Pct(stackDmg) + " per stack)</style> <style=cIsDamage>damage/sec</style> to enemies within <style=cIsDamage>" + baseRadius.ToString("N1") + " m</style> <style=cStack>(+ " + stackRadius.ToString("N2") + " per stack)</style>",
            	"A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
			var mshPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/MushroomWard");

			var bwPrefabPrefab = new GameObject("BarbedWardAuraPrefabPrefab");
			bwPrefabPrefab.AddComponent<TeamFilter>();
			bwPrefabPrefab.AddComponent<MeshFilter>().mesh = mshPrefab.GetComponentInChildren<MeshFilter>().mesh;
			bwPrefabPrefab.AddComponent<MeshRenderer>().material = UnityEngine.Object.Instantiate(mshPrefab.GetComponentInChildren<MeshRenderer>().material);
			bwPrefabPrefab.GetComponent<MeshRenderer>().material.SetVector("_TintColor",new Vector4(1f,0f,0f,0.5f));
			bwPrefabPrefab.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;
			var bw = bwPrefabPrefab.AddComponent<BarbedWard>();
			bw.rangeIndicator = bwPrefabPrefab.GetComponent<MeshRenderer>().transform;
			bw.interval = 1f;
			barbedWardPrefab = bwPrefabPrefab.InstantiateClone("BarbedWardAuraPrefab");
			UnityEngine.Object.Destroy(bwPrefabPrefab);
			
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
			if(inclDeploys)
				On.RoR2.CharacterMaster.AddDeployable += On_CMAddDeployable;
        }

		//AddDeployable fires after OnInventoryChanged while creating a turret, so Deployable.ownerMaster won't be set in OIC
		private void On_CMAddDeployable(On.RoR2.CharacterMaster.orig_AddDeployable orig, CharacterMaster self, Deployable depl, DeployableSlot slot) {
			orig(self, depl, slot);

			var body = depl.GetComponent<CharacterMaster>()?.GetBody();
			if(body) updateBarbedWard(body);
		}

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
			orig(self);

			if(!NetworkServer.active || (!inclDeploys && self.master?.GetComponent<Deployable>())) return;
			updateBarbedWard(self);
        }

		private void updateBarbedWard(CharacterBody body) {
            var cpt = body.GetComponentInChildren<BarbedWard>()?.gameObject;

			var icnt = GetCount(body);
			var idmg = body.damage;
			if(inclDeploys) idmg += body.master?.GetComponent<Deployable>()?.ownerMaster?.GetBody()?.damage ?? 0;
			if(icnt == 0 || idmg == 0) {
				if(cpt) UnityEngine.Object.Destroy(cpt);
			} else {
				if(!cpt) {
					cpt = UnityEngine.Object.Instantiate(barbedWardPrefab);
					cpt.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
					cpt.GetComponent<BarbedWard>().owner = body.gameObject;
					cpt.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
				}
				cpt.GetComponent<BarbedWard>().netRadius = baseRadius + (icnt-1) * stackRadius;
				cpt.GetComponent<BarbedWard>().netDamage = (baseDmg + (icnt-1) * stackDmg) * idmg;
			}
		}
    }

	[RequireComponent(typeof(TeamFilter))]
	public class BarbedWard : NetworkBehaviour {
		[SyncVar]
		float radius;
		public float netRadius {
			get {return radius;}
			set {base.SetSyncVar<float>(value, ref radius, 1u);}
		}

		[SyncVar]
		float damage;
		public float netDamage {
			get {return damage;}
			set {base.SetSyncVar<float>(value, ref damage, 1u);}
		}

		public float interval;
		public Transform rangeIndicator;

		public GameObject owner;
		
		private TeamFilter teamFilter;
		private float rangeIndicatorScaleVelocity;

		private float stopwatch;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void Awake() {
			teamFilter = base.GetComponent<TeamFilter>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void Update() {
			float num = Mathf.SmoothDamp(rangeIndicator.localScale.x, radius*2f, ref rangeIndicatorScaleVelocity, 0.2f);
			rangeIndicator.localScale = new Vector3(num, num, num);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
		private void FixedUpdate() {
			stopwatch -= Time.fixedDeltaTime;
			if (stopwatch <= 0f) {
				if(NetworkServer.active) {
					stopwatch = interval;
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
			tind &= ~teamFilter.teamIndex;
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(tind);
			float sqrad = radius * radius;
			foreach(TeamComponent tcpt in teamMembers) {
				if ((tcpt.transform.position - transform.position).sqrMagnitude <= sqrad) {
					if (tcpt.body && tcpt.body.mainHurtBox && tcpt.body.isActiveAndEnabled && damage > 0f) {
						OrbManager.instance.AddOrb(new LightningOrb {
							attacker = owner,
							bouncesRemaining = 0,
							damageColorIndex = DamageColorIndex.Bleed,
							damageType = DamageType.AOE,
							damageValue = damage,
							isCrit = false,
							lightningType = LightningOrb.LightningType.RazorWire,
							origin = transform.position,
							procChainMask = default,
							procCoefficient = 1f,
							target = tcpt.body.mainHurtBox,
							teamIndex = teamFilter.teamIndex,
						});
						if(BarbedWire.instance.oneOnly) break;
					}
				}
			}
		}
	}
}
