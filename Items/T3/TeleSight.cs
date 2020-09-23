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
        public override bool itemAIB {get; protected set;} = true;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Base percent chance of triggering Telescopic Sight on hit. Affected by proc coefficient.",AutoItemConfigFlags.None,0f,100f)]
        public float procChance {get;private set;} = 1f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Added to ProcChance per extra stack of Telescopic Sight.",AutoItemConfigFlags.None,0f,100f)]
        public float stackChance {get;private set;} = 0.5f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Maximum allowed ProcChance for Telescopic Sight.",AutoItemConfigFlags.None,0f,100f)]
        public float capChance {get;private set;} = 3f;

        [AutoItemConfig("If true, Telescopic Sight will not trigger on bosses.")]
        public bool bossImmunity {get;private set;} = false;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Chance to instantly kill an enemy.";
        protected override string NewLangDesc(string langid = null) => "<style=cIsDamage>" + Pct(procChance,1,1) + "</style> <style=cStack>(+" + Pct(stackChance,1,1) + " per stack, up to " + Pct(capChance,1,1) + ")</style> chance to <style=cIsDamage>instantly kill</style> an enemy. Affected by proc coefficient.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public TeleSight() {
            onBehav += () => {
			    if(Compat_ItemStats.enabled) {
				    Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
					    ((count,inv,master)=>{return Mathf.Min(procChance+stackChance*(count-1), capChance);},
					    (value,inv,master)=>{return $"Instakill Chance: {Pct(value, 1, 1f)}";}));
			    }
                if(Compat_BetterUI.enabled)
                    Compat_BetterUI.AddEffect(regIndex, procChance, stackChance, Compat_BetterUI.ChanceFormatter, Compat_BetterUI.LinearStacking,
                        (value, extraStackValue, procCoefficient) => {
                            return Mathf.CeilToInt((capChance - value*procCoefficient)/(extraStackValue*procCoefficient)) + 1;
                        });
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.GlobalEventManager.OnHitEnemy += On_GEMOnHitEnemy;
        }

        protected override void UnloadBehavior() {
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
			GameObject effectPrefab = Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/BeamSphereOrbEffect");
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
