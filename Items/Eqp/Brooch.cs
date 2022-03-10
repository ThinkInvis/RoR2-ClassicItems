using RoR2;
using R2API;
using System;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Brooch : Equipment<Brooch> {
        public override string displayName => "Captain's Brooch";

        float baseCost = 25f;

        public override float cooldown {get;protected set;} = 135f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplier for additional cost of chests spawned by Captain's Brooch.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float extraCost {get;private set;} = 0.5f;

        [AutoConfig("If true, chests spawned by Captain's Brooch will immediately appear at the target position instead of falling nearby, and will not be destroyed after purchase.",
            AutoConfigFlags.PreventNetMismatch)]
        public bool safeMode {get;private set;} = false;

        [AutoConfig("If true, Captain's Brooch will spawn chests directly at the player's position if it can't find a suitable spot nearby. If false, it will fail to spawn the chest and refrain from using an equipment charge.")]
        public bool doFallbackSpawn {get;private set;} = false;

        internal static InteractableSpawnCard broochPrefab;        
        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "One man's wreckage is another man's treasure.";        
        protected override string GetDescString(string langid = null) => "Calls down a <style=cIsUtility>low-tier item chest</style> which <style=cIsUtility>costs " + Pct(extraCost) + " more than usual</style>.";        
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        private bool ILFailed = false;

        public override void SetupConfig() {
            base.SetupConfig();

            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(safeMode))
                    broochPrefab.prefab.GetComponent<CaptainsBroochDroppod>().enabled = !(bool)args.newValue;
                else if(args.target.boundProperty.Name == nameof(extraCost)) {
                    broochPrefab.prefab.GetComponent<PurchaseInteraction>().cost = Mathf.CeilToInt(baseCost * (1f + extraCost));
                }
            };
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
            broochPrefab = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscChest1"));
            broochPrefab.directorCreditCost = 0;
            broochPrefab.sendOverNetwork = true;
            broochPrefab.skipSpawnWhenSacrificeArtifactEnabled = false;
            broochPrefab.prefab = PrefabAPI.InstantiateClone(broochPrefab.prefab, "chestBrooch", true);

            broochPrefab.prefab.AddComponent<CaptainsBroochDroppod>().enabled = !safeMode;

            var pInt = broochPrefab.prefab.GetComponent<PurchaseInteraction>();

            baseCost = pInt.cost;

            pInt.cost = Mathf.CeilToInt(baseCost * (1f + extraCost));
            pInt.automaticallyScaleCostWithDifficulty = true;
        }

        public override void Install() {
            base.Install();
            ILFailed = false;
            if(ILFailed) safeMode = true;

            if(!safeMode) On.RoR2.ChestBehavior.Open += On_CBOpen;
        }
        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.ChestBehavior.Open -= On_CBOpen;
        }

        private void On_CBOpen(On.RoR2.ChestBehavior.orig_Open orig, ChestBehavior self) {
            orig(self);
            var dot = self.GetComponentInParent<CaptainsBroochDroppod>();
            if(dot) dot.ServerUnlaunch();
        }

        private void Evt_BroochChestSpawnServer(SpawnCard.SpawnResult spawnres) {
            if(!safeMode && spawnres.success)
                spawnres.spawnedInstance.GetComponent<CaptainsBroochDroppod>().ServerLaunch();
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody) return false;
            if(SceneCatalog.mostRecentSceneDef.baseSceneName == "bazaar") return false;
            bool s1 = TrySpawnChest(slot.characterBody.transform);
            bool s2 = false;
            if(Embryo.instance.CheckEmbryoProc(slot.characterBody)) s2 = TrySpawnChest(slot.characterBody.transform);
            return s1 || s2;
        }

        private bool TrySpawnChest(Transform trans) {
            var dsr = new DirectorSpawnRequest(broochPrefab, new DirectorPlacementRule {
                maxDistance = 25f,
                minDistance = 5f,
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = trans.position,
                preventOverhead = true
            }, rng);

            dsr.onSpawnedServer += Evt_BroochChestSpawnServer;

            var spawnobj = DirectorCore.instance.TrySpawnObject(dsr);
            //broochPrefab.DoSpawn(trans.position, trans.rotation, dsr);
            if(spawnobj == null) {
                if(doFallbackSpawn) {
                    ClassicItemsPlugin._logger.LogWarning("Captain's Brooch: spawn failed, using fallback position. This may be caused by too many objects nearby/no suitable ground.");
                    var dsrFallback = new DirectorSpawnRequest(broochPrefab, new DirectorPlacementRule {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        position = trans.position
                    }, rng);
                    dsrFallback.onSpawnedServer += Evt_BroochChestSpawnServer;
                    broochPrefab.DoSpawn(trans.position, trans.rotation, dsrFallback);
                    return true;
                } else {
                    ClassicItemsPlugin._logger.LogWarning("Captain's Brooch: spawn failed, not triggering equipment. This may be caused by too many objects nearby/no suitable ground.");
                    return false;
                }
            } else return true;
        }
    }
    internal class CaptainsBroochDroppod:NetworkBehaviour {
        ShakeEmitter shkm;

        [SyncVar]
        int launchState = 0;
        [SyncVar]
        float droptimer = 2f;
        [SyncVar]
        Vector3 destination;
        [SyncVar]
        Vector3 source;

        [ClientRpc]
        private void RpcLaunch() {
            shkm = this.gameObject.AddComponent<ShakeEmitter>();
			shkm.wave = new Wave {
				amplitude = 0.25f,
				frequency = 180f,
				cycleOffset = 0f
			};
			shkm.duration = 0.45f;
			shkm.radius = 100f;
			shkm.amplitudeTimeDecay = false;

            launchState = 1;
        }

        [Server]
        public void ServerLaunch() {
            RpcLaunch();

            var originalPos = this.gameObject.transform.position;

            this.gameObject.transform.position += Vector3.up * 2000f;
            source = this.gameObject.transform.position;
            var rth = UnityEngine.Random.Range(0,Mathf.PI*2);
            var rmag = UnityEngine.Random.Range(0,150f);
            source += new Vector3(
                Mathf.Cos(rth)*rmag,
                0,
                Mathf.Sin(rth)*rmag);

            destination = originalPos;
            launchState = 1;
        }
        
        [ClientRpc]
        private void RpcUnlaunch() {
            launchState = 3;
        }

        [Server]
        public void ServerUnlaunch() {
            RpcUnlaunch();
            launchState = 3;
        }

        [ClientRpc]
        public void RpcLanded() {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/PodGroundImpact"), new EffectData {
				origin = this.gameObject.transform.position,
				rotation = this.gameObject.transform.rotation,
                scale = 0.25f
			}, true);
            Util.PlaySound("Play_UI_podImpact", this.gameObject);
            shkm.enabled = false;
        }

        [ClientRpc]
        public void RpcUnlanded() {
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/PodGroundImpact"), new EffectData {
				origin = this.gameObject.transform.position,
				rotation = this.gameObject.transform.rotation,
                scale = 0.25f
			}, true);
            Util.PlaySound("Play_UI_podImpact", this.gameObject);
            shkm.enabled = true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void FixedUpdate() {
            if(launchState == 1) {
                droptimer -= Time.fixedDeltaTime;
                this.gameObject.transform.position = Vector3.Lerp(source, destination, 1f-Math.Max(droptimer/2f, 0f));
                if(droptimer <= 0f) {
                    if(NetworkServer.active)
                        RpcLanded();
                    launchState = 2;
                    droptimer = 0f;
                }
            } else if(launchState == 3) {
                droptimer += Time.fixedDeltaTime;
                if(droptimer >= 5f) {
                    if(NetworkServer.active)
                        RpcUnlanded();
                    droptimer = 2f;
                    launchState = 4;
                }
            } else if(launchState == 4) {
                droptimer -= Time.fixedDeltaTime;
                this.gameObject.transform.position = Vector3.Lerp(destination, source, 1f-Math.Max(droptimer/2f, 0f));
                if(droptimer <= 0f) {
                    if(NetworkServer.active)
                        DirectorCore.instance.RemoveAllOccupiedNodes(this.gameObject);
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
