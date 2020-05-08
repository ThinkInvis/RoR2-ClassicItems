using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class LifeSavings : Item<LifeSavings> {
        public override string displayName => "Life Savings";
        public override bool itemAIB {get; protected set;} = true;
        public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AICAUEventInfo(AICAUEventFlags.InvalidateDescToken)]
        [AutoItemCfg("Money to add to players per second per Life Savings stack (without taking into account InvertCount).", AICFlags.None, 0f, float.MaxValue)]
        public float gainPerSec {get;private set;} = 1f;

        [AICAUEventInfo(AICAUEventFlags.InvalidateDescToken)]
        [AutoItemCfg("With <InvertCount stacks, number of stacks affects time per interval instead of multiplying money gained.", AICFlags.None, 0, int.MaxValue)]
        public int invertCount {get;private set;} = 3;

        [AutoItemCfg("If true, Life Savings stacks on deployables (e.g. Engineer turrets) will send money to their master.")]
        public bool inclDeploys {get;private set;} = false;

        [AutoItemCfg("If true, Life Savings will continue to work in areas where the run timer is paused (e.g. bazaar).")]
        public bool ignoreTimestop {get;private set;} = false;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Earn gold over time.";
        protected override string NewLangDesc(string langid = null) => "Generates <style=cIsUtility>$" + gainPerSec.ToString("N0") + "</style> <style=cStack>(+$" + gainPerSec.ToString("N0") + " per stack)</style> every second. <style=cStack>Generates less below " + invertCount.ToString("N0") + " stacks.</style>";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public LifeSavings() {}

        protected override void LoadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.SceneExitController.Begin += On_SECBegin;
            On.EntityStates.SpawnTeleporterState.OnExit += On_EntSTSOnExit;
            On.RoR2.CharacterMaster.AddDeployable += On_CMAddDeployable;
            On.RoR2.CharacterMaster.RemoveDeployable += On_CMRemoveDeployable;
        }
        protected override void UnloadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.SceneExitController.Begin -= On_SECBegin;
            On.EntityStates.SpawnTeleporterState.OnExit -= On_EntSTSOnExit;
            On.RoR2.CharacterMaster.AddDeployable -= On_CMAddDeployable;
            On.RoR2.CharacterMaster.RemoveDeployable -= On_CMRemoveDeployable;
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
