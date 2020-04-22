using BepInEx.Configuration;
using RoR2;
using R2API;
using System;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems
{
    public class Brooch : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "Brooch";

        private ConfigEntry<float> cfgExtraCost;
        private ConfigEntry<bool> cfgSafeMode;
        private ConfigEntry<bool> cfgFallbackSpawn;

        public float extraCost {get;private set;}
        public bool safeMode {get;private set;}
        public bool doFallbackSpawn {get;private set;}

        private Xoroshiro128Plus BroochRNG;

        internal static InteractableSpawnCard broochPrefab;

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgExtraCost = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "ExtraCost"), 0.5f, new ConfigDescription(
                "Multiplier for additional cost of chests spawned by Captain's Brooch.",
                new AcceptableValueRange<float>(0f, float.MaxValue)));
            cfgSafeMode = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "SafeMode"), false, new ConfigDescription(
                "If true, chests spawned by Captain's Brooch will immediately appear at the target position instead of falling nearby, and will not be destroyed after purchase."));
            cfgFallbackSpawn = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "FallbackSpawn"), false, new ConfigDescription(
                "If true, Captain's Brooch will spawn chests directly at the player's position if it can't find a suitable spot nearby. If false, it will fail to spawn the chest and refrain from using an equipment charge."));

            extraCost = cfgExtraCost.Value;
            safeMode = cfgSafeMode.Value;
            doFallbackSpawn = cfgFallbackSpawn.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemIsEquipment = true;

            modelPathName = "broochcard.prefab";
            iconPathName = "captainsbrooch_icon.png";
            itemCooldown = 135;

            RegLang("Captain's Brooch",
                "One man's wreckage is another man's treasure.",
                "Calls down a <style=cIsUtility>low-tier item chest</style> which <style=cIsUtility>costs " + pct(extraCost) + " more than usual</style>.",
                "A relic of times long past (ClassicItems mod)");
        }


        private bool ILFailed = false;

        protected override void SetupBehaviorInner() {
            IL.RoR2.DirectorCore.TrySpawnObject += IL_DCTrySpawnObject;
            if(ILFailed) safeMode = true;

            BroochRNG = new Xoroshiro128Plus(0UL);

            broochPrefab = UnityEngine.Object.Instantiate(Resources.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscChest1"));
            broochPrefab.directorCreditCost = 0;
            broochPrefab.sendOverNetwork = true;
            broochPrefab.skipSpawnWhenSacrificeArtifactEnabled = false;
            broochPrefab.prefab = PrefabAPI.InstantiateClone(broochPrefab.prefab,"chestBrooch");

            if(!safeMode) broochPrefab.prefab.AddComponent<CaptainsBroochDroppod>();

            var pInt = broochPrefab.prefab.GetComponent<PurchaseInteraction>();

            pInt.cost = Mathf.CeilToInt(pInt.cost * (1f + extraCost));
            pInt.automaticallyScaleCostWithDifficulty = true;
            
            if(!safeMode) On.RoR2.ChestBehavior.Open += On_CBOpen;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }

        private void IL_DCTrySpawnObject(ILContext il) {
            ILCursor c = new ILCursor(il);

            
            ILLabel[] swarr = new ILLabel[]{};
            int instind = -1;

            bool ILFound = c.TryGotoNext(
                x=>x.MatchSwitch(out swarr))
            && c.TryGotoNext(
                x=>x.MatchLdfld<SpawnCard.SpawnResult>("spawnedInstance")
                && x.Offset > swarr[(int)DirectorPlacementRule.PlacementMode.Approximate].Target.Offset
                && x.Offset < swarr[(int)DirectorPlacementRule.PlacementMode.Approximate+1].Target.Offset,
                x=>x.MatchStloc(out instind)
                )
            && c.TryGotoNext(
                x=>x.MatchCallOrCallvirt<DirectorCore>("AddOccupiedNode")
                && x.Offset < swarr[(int)DirectorPlacementRule.PlacementMode.Approximate+1].Target.Offset);
            if(ILFound) {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldloc, instind);
                c.EmitDelegate<Action<RoR2.Navigation.NodeGraph.NodeIndex, GameObject>>((ind,res)=>{
                    var cpt = res.GetComponent<CaptainsBroochDroppod>();
                    if(cpt) cpt.mapNode = ind;
                });
            } else {
                Debug.LogError("ClassicItems: failed to apply Captain's Brooch IL patch. SafeMode will be enabled (no animations, no opened chest cleanup).");
                ILFailed = true;
            }
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

        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
                if(!slot.characterBody) return false;
                var trans = slot.characterBody.transform;

                var dsr = new DirectorSpawnRequest(broochPrefab, new DirectorPlacementRule {
                    maxDistance = 25f,
                    minDistance = 5f,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = trans.position,
                    preventOverhead = true
                }, BroochRNG);

                dsr.onSpawnedServer += Evt_BroochChestSpawnServer;

                var spawnobj = DirectorCore.instance.TrySpawnObject(dsr);
                //broochPrefab.DoSpawn(trans.position, trans.rotation, dsr);
                if(spawnobj == null) {
                    if(doFallbackSpawn) {
                        Debug.LogWarning("Captain's Brooch: spawn failed, using fallback position. This may be caused by too many objects nearby/no suitable ground.");
                        var dsrFallback = new DirectorSpawnRequest(broochPrefab, new DirectorPlacementRule {
                            placementMode = DirectorPlacementRule.PlacementMode.Direct,
                            position = trans.position
                        }, BroochRNG);
                        dsrFallback.onSpawnedServer += Evt_BroochChestSpawnServer;
                        broochPrefab.DoSpawn(trans.position, trans.rotation, dsrFallback);
                        return true;
                    } else {
                        Debug.LogWarning("Captain's Brooch: spawn failed, not triggering equipment. This may be caused by too many objects nearby/no suitable ground.");
                        return false;
                    }
                } else return true;
            } else return orig(slot, eqpid);
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

        public RoR2.Navigation.NodeGraph.NodeIndex mapNode;

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
            EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/PodGroundImpact"), new EffectData {
				origin = this.gameObject.transform.position,
				rotation = this.gameObject.transform.rotation,
                scale = 0.25f
			}, true);
            Util.PlaySound("Play_UI_podImpact", this.gameObject);
            shkm.enabled = false;
        }

        [ClientRpc]
        public void RpcUnlanded() {
            EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/PodGroundImpact"), new EffectData {
				origin = this.gameObject.transform.position,
				rotation = this.gameObject.transform.rotation,
                scale = 0.25f
			}, true);
            Util.PlaySound("Play_UI_podImpact", this.gameObject);
            shkm.enabled = true;
        }

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
                        DirectorCore.instance.RemoveOccupiedNode(
                            SceneInfo.instance.GetNodeGraph(Brooch.broochPrefab.nodeGraphType),
                            mapNode);
                    Destroy(this.gameObject);
                }
            }
        }
    }
}
