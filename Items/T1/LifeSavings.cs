using RoR2;
using System;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine.Networking;
using R2API.Utils;

namespace ThinkInvisible.ClassicItems {
    public class LifeSavings : ItemBoilerplate<LifeSavings> {
        public override string itemCodeName {get;} = "LifeSavings";

        private ConfigEntry<float> cfgGainPerSec;
        private ConfigEntry<int> cfgInvertCount;
        private ConfigEntry<bool> cfgInclDeploys;
        private ConfigEntry<bool> cfgIgnoreTimestop;

        public float gainPerSec {get;private set;}
        public int invertCount {get;private set;}
        public bool inclDeploys {get;private set;}
        public bool ignoreTimestop {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            itemAIBDefault = true;

            cfgGainPerSec = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "GainPerSec"), 1f, new ConfigDescription(
                "Money to add to players per second per Life Savings stack (without taking into account InvertCount).",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgInvertCount = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "InvertCount"), 3, new ConfigDescription(
                "With <InvertCount stacks, number of stacks affects time per interval instead of multiplying money gained.",
                new AcceptableValueRange<int>(0,int.MaxValue)));
            cfgInclDeploys = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "InclDeploys"), false, new ConfigDescription(
                "If true, Life Savings stacks on deployables (e.g. Engineer turrets) will send money to their master."));
            cfgIgnoreTimestop = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "IgnoreTimestop"), false, new ConfigDescription(
                "If true, Life Savings will continue to work in areas where the run timer is paused (e.g. bazaar)."));

            gainPerSec = cfgGainPerSec.Value;
            invertCount = cfgInvertCount.Value;
            inclDeploys = cfgInclDeploys.Value;
            ignoreTimestop = cfgIgnoreTimestop.Value;
        }

        protected override void SetupAttributesInner() {
            modelPathName = "lifesavings_model.prefab";
            iconPathName = "lifesavings_icon.png";
            RegLang("Life Savings",
            	"Earn gold over time.",
            	"Generates <style=cIsUtility>$" + gainPerSec + "</style> <style=cStack>(+$" + gainPerSec + " per stack)</style> every second.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier1;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.SceneExitController.Begin += On_SECBegin;
            On.EntityStates.SpawnTeleporterState.OnExit += On_EntSTSOnExit;
            On.RoR2.CharacterMaster.AddDeployable += On_CMAddDeployable;
            On.RoR2.CharacterMaster.RemoveDeployable += On_CMRemoveDeployable;
        }
        private void On_EntSTSOnExit(On.EntityStates.SpawnTeleporterState.orig_OnExit orig, EntityStates.SpawnTeleporterState self) {
            orig(self);
            if(!NetworkServer.active) return;
            var cpt = self.outer.commonComponents.characterBody.GetComponent<LifeSavingsComponent>();
            if(cpt) cpt.holdIt = false;
        }

        private void On_SECBegin(On.RoR2.SceneExitController.orig_Begin orig, SceneExitController self) {
            orig(self);
            if(!NetworkServer.active) return;
            foreach(NetworkUser networkUser in NetworkUser.readOnlyInstancesList) {
				if(networkUser.master.hasBody) {
                    var cpt = networkUser.master.GetBody().GetComponent<LifeSavingsComponent>();
                    if(cpt) cpt.holdIt = true;
				}
            }
        }

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            var cpt = self.GetComponent<LifeSavingsComponent>();
            if(!cpt) cpt = self.gameObject.AddComponent<LifeSavingsComponent>();
            if(NetworkServer.active) cpt.ServerUpdateIcnt();
        }

        private void On_CMAddDeployable(On.RoR2.CharacterMaster.orig_AddDeployable orig, CharacterMaster self, Deployable dpl, DeployableSlot dpls) {
            orig(self, dpl, dpls);
            if(inclDeploys && self.hasBody) {
                self.GetBody().GetComponent<LifeSavingsComponent>()?.ServerUpdateIcnt();
            }
        }
        private void On_CMRemoveDeployable(On.RoR2.CharacterMaster.orig_RemoveDeployable orig, CharacterMaster self, Deployable dpl) {
            orig(self, dpl);
            if(inclDeploys && self.hasBody) {
                self.GetBody().GetComponent<LifeSavingsComponent>()?.ServerUpdateIcnt();
            }
        }
    }
        
    public class LifeSavingsComponent : NetworkBehaviour {
        private float moneyBuffer = 0f;
        [SyncVar]
        public bool holdIt = true; //https://www.youtube.com/watch?v=vDMwDT6BhhE
        [SyncVar]
        public int icnt = 0;

        [Server]
        public void ServerUpdateIcnt() {
            var body = this.gameObject.GetComponent<CharacterBody>();
            icnt = LifeSavings.instance.GetCount(body);
            if(LifeSavings.instance.inclDeploys && body.master) icnt += LifeSavings.instance.GetCountOnDeploys(body.master);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void FixedUpdate() {
            var body = this.gameObject.GetComponent<CharacterBody>();
            if(body.inventory && body.master) {
                if(icnt > 0)
                    moneyBuffer += Time.fixedDeltaTime * LifeSavings.instance.gainPerSec * ((icnt < LifeSavings.instance.invertCount)?(1f/(float)(LifeSavings.instance.invertCount-icnt+1)):(icnt-LifeSavings.instance.invertCount+1));
                //Disable during pre-teleport money drain so it doesn't softlock
                //Accumulator is emptied into actual money variable whenever a tick passes and it has enough for a change in integer value
                if(moneyBuffer >= 1.0f && !holdIt && (LifeSavings.instance.ignoreTimestop || !Run.instance.isRunStopwatchPaused)){
                    if(Compat_ShareSuite.enabled && Compat_ShareSuite.MoneySharing())
                        Compat_ShareSuite.GiveMoney((uint)Math.Floor(moneyBuffer));
                    else
                        body.master.GiveMoney((uint)Math.Floor(moneyBuffer));
                    moneyBuffer %= 1.0f;
                }
            }
        }
    }
}
