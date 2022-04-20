using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using RoR2.Orbs;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class TeleSight : Item<TeleSight> {
        public override string displayName => "Telescopic Sight";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        [AutoConfigRoOSlider("{0:N1}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base percent chance of triggering Telescopic Sight on hit. Affected by proc coefficient.",AutoConfigFlags.None,0f,100f)]
        public float procChance {get;private set;} = 1f;

        [AutoConfigRoOSlider("{0:N1}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Added to ProcChance per extra stack of Telescopic Sight.",AutoConfigFlags.None,0f,100f)]
        public float stackChance {get;private set;} = 0.5f;

        [AutoConfigRoOSlider("{0:N1}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Maximum allowed ProcChance for Telescopic Sight.",AutoConfigFlags.None,0f,100f)]
        public float capChance {get;private set;} = 3f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, Telescopic Sight will not trigger on bosses.")]
        public bool bossImmunity {get;private set;} = false;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to instantly kill an enemy.";
        protected override string GetDescString(string langid = null) {
            string desc = "<style=cIsDamage>" + Pct(procChance, 1, 1) + "</style>";
            if(stackChance > 0f) desc += " <style=cStack>(+" + Pct(stackChance, 1, 1) + " per stack, up to " + Pct(capChance, 1, 1) + ")</style>";
            desc += " chance to <style=cIsDamage>instantly kill</style> an enemy. Affected by proc coefficient.";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public TeleSight() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/telesight_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/TeleSight.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            On.RoR2.GlobalEventManager.OnHitEnemy += On_GEMOnHitEnemy;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.GlobalEventManager.OnHitEnemy -= On_GEMOnHitEnemy;
        }

        private void On_GEMOnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            orig(self, damageInfo, victim);
			if(!NetworkServer.active || !victim || !damageInfo.attacker || damageInfo.procCoefficient <= 0f) return;
            
            var vicb = victim.GetComponent<CharacterBody>();

            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            if(!body || !vicb || !vicb.healthComponent || !vicb.mainHurtBox || (bossImmunity && vicb.isBoss)) return;

            CharacterMaster chrm = body.master;
            if(!chrm) return;

            int icnt = GetCount(body);
            if(icnt == 0) return;

            icnt--;
            float tProc = procChance;
            if(icnt > 0) tProc += stackChance * icnt;
            if(tProc > capChance) tProc = capChance;
            if(!Util.CheckRoll(tProc * damageInfo.procCoefficient, chrm)) return;

            OrbManager.instance.AddOrb(new TeleSightOrb {
                attacker = damageInfo.attacker,
                damageColorIndex = DamageColorIndex.WeakPoint,
                damageValue = vicb.healthComponent.fullCombinedHealth + vicb.healthComponent.fullBarrier,
                isCrit = true,
                origin = body.corePosition,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 1f,
                target = vicb.mainHurtBox,
                teamIndex = body.GetComponent<TeamComponent>()?.teamIndex ?? TeamIndex.Neutral
            });
        }
    }
    
	public class TeleSightOrb : Orb {
		public override void Begin() {
			base.duration = 0.5f;
			EffectData effectData = new EffectData {
				origin = this.origin,
				genericFloat = base.duration
			};
			effectData.SetHurtBoxReference(this.target);
			GameObject effectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/BeamSphereOrbEffect");
			EffectManager.SpawnEffect(effectPrefab, effectData, true);
		}

		public override void OnArrival() {
			if (this.target) {
				HealthComponent healthComponent = this.target.healthComponent;
				if (healthComponent) {
                    DamageInfo damageInfo = new DamageInfo {
                        damage = this.damageValue,
                        attacker = this.attacker,
                        inflictor = null,
                        force = Vector3.zero,
                        crit = this.isCrit,
                        procChainMask = this.procChainMask,
                        procCoefficient = this.procCoefficient,
                        position = this.target.transform.position,
                        damageColorIndex = this.damageColorIndex
                    };
                    healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
				}
			}
		}

		public float damageValue;
		public GameObject attacker;
		public TeamIndex teamIndex;
		public bool isCrit;
		public ProcChainMask procChainMask;
		public float procCoefficient = 0f;
		public DamageColorIndex damageColorIndex;
	}
}
