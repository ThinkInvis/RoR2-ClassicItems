using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems
{
    public class Brooch : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "Brooch";

        private ConfigEntry<float> cfgExtraTime;
        private ConfigEntry<int> cfgExtraStages;
        private ConfigEntry<bool> cfgSafeMode;

        public float extraTime {get;private set;}
        public int extraStages {get;private set;}
        public bool safeMode {get;private set;}

        private Xoroshiro128Plus BroochRNG;

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgExtraTime = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "ExtraTime"), 120f, new ConfigDescription(
                "Run time to add to the difficulty cost multiplier of chests spawned by Captain's Brooch.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));
            cfgExtraStages = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "ExtraStages"), 1, new ConfigDescription(
                "Passed stages to add to the difficulty cost multiplier of chests spawned by Captain's Brooch.",
                new AcceptableValueRange<int>(0, int.MaxValue)));
            cfgSafeMode = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "SafeMode"), false, new ConfigDescription(
                "If true, chests spawned by Captain's Brooch will immediately appear at the player's position instead of falling nearby."));

            extraTime = cfgExtraTime.Value;
            extraStages = cfgExtraStages.Value;
            safeMode = cfgSafeMode.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemIsEquipment = true;

            modelPathName = "broochcard.prefab";
            iconPathName = "captainsbrooch_icon.png";
            itemName = "Captain's Brooch";
            itemShortText = "One man's wreckage is another man's treasure.";
            itemLongText = "Call down a basic item chest, with an opening cost equivalent to one from " + extraStages.ToString("N0") + " level(s) and " + extraTime.ToString("N0") + " second(s) in the future.";
            itemLoreText = "A relic of times long past (ClassicItems mod)";
            itemEnigmable = true;
            itemCooldown = 135;
        }

        protected override void SetupBehaviorInner() {
            BroochRNG = new Xoroshiro128Plus(0UL);
            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }

        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(slot.characterBody && eqpid == regIndexEqp) {
                var trans = slot.characterBody.transform;

                var chestPrefab = Resources.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscChest1");
                chestPrefab.directorCreditCost = 0;
                var chestSpawnRes = chestPrefab.DoSpawn(trans.position, trans.rotation, new DirectorSpawnRequest(chestPrefab, null, BroochRNG));
                var chestSpawn = chestSpawnRes.spawnedInstance;

                var oldCost = chestSpawn.GetComponent<PurchaseInteraction>().cost;
                int newCost = (int)(oldCost * Mathf.Pow(Run.instance.difficultyCoefficient + MiscUtil.getDifficultyCoeffIncreaseAfter(extraTime, extraStages), 1.25f));
                chestSpawn.GetComponent<PurchaseInteraction>().cost = newCost;
                chestSpawn.GetComponent<PurchaseInteraction>().Networkcost = newCost;

                if(!safeMode)
                    chestSpawn.AddComponent<CaptainsBroochDroppod>();

                return true;
            }
            return orig(slot, eqpid);
        }

        public class CaptainsBroochDroppod:MonoBehaviour {
            Vector3 destination;
            Vector3 source;
            float droptimer = 2f;
            ShakeEmitter shkm;
            public void Awake() {
                shkm = this.transform.gameObject.AddComponent<ShakeEmitter>();
				shkm.wave = new Wave {
					amplitude = 0.25f,
					frequency = 180f,
					cycleOffset = 0f
				};
				shkm.duration = 0.45f;
				shkm.radius = 100f;
				shkm.amplitudeTimeDecay = false;

                var originalPos = this.gameObject.transform.position;

                this.gameObject.transform.position += Vector3.up * 2000f;
                source = this.gameObject.transform.position;
                Vector3 rndir = new Vector3(
                    UnityEngine.Random.Range(-0.005f,0.005f),
                    -1f,
                    UnityEngine.Random.Range(-0.005f,0.005f)
                    ).normalized;
                RaycastHit[] allHitInf = Physics.RaycastAll(this.gameObject.transform.position, rndir, 4000f);
                float closestHit = -1;
                foreach(RaycastHit h in allHitInf) {
                    if(h.collider?.gameObject?.layer == 11) {
                        var newdest = h.point + Vector3.down * 0.6f;
                        var hitDist = Math.Abs(newdest.y - originalPos.y);
                        if(closestHit == -1 || hitDist < closestHit) {
                            destination = newdest;
                            closestHit = hitDist;
                        }
                    }
                }
                if(destination == null) destination = originalPos;
            }
            
            public void Update() {
                droptimer -= Time.fixedDeltaTime;
                this.gameObject.transform.position = Vector3.Lerp(source, destination, 1f-Math.Max(droptimer/2f, 0f));
                if(droptimer <= 0f) {
					EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/PodGroundImpact"), new EffectData
					{
						origin = this.gameObject.transform.position,
						rotation = this.gameObject.transform.rotation,
                        scale = 0.25f
					}, true);
			        Util.PlaySound("Play_UI_podImpact", this.gameObject);
                    Destroy(shkm);
                    Destroy(this);
                }
            }
        }
    }
}
