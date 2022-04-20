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
        public override bool itemIsAIBlacklisted {get; protected set;} = true;
        public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AutoConfigRoOSlider("${0:N1}", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Money to add to players per second per Life Savings stack (without taking into account InvertCount).", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float gainPerSec {get;private set;} = 1f;

        [AutoConfigRoOIntSlider("${0:N0}", 0, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("With less than InvertCount stacks, number of stacks affects time per interval instead of multiplying money gained.", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int invertCount {get;private set;} = 3;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, Life Savings stacks on deployables (e.g. Engineer turrets) will send money to their master.",
            AutoConfigFlags.PreventNetMismatch)]
        public bool inclDeploys {get;private set;} = false;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, Life Savings will continue to work in areas where the run timer is paused (e.g. bazaar).",
            AutoConfigFlags.PreventNetMismatch)]
        public bool ignoreTimestop {get;private set;} = false;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Earn gold over time.";
        protected override string GetDescString(string langid = null) => "Generates <style=cIsUtility>$" + gainPerSec.ToString("N0") + "</style> <style=cStack>(+$" + gainPerSec.ToString("N0") + " per stack)</style> every second. <style=cStack>Generates less below " + invertCount.ToString("N0") + " stacks.</style>";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public LifeSavings() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/lifesavings_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/LifeSavings.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.AddDeployable += On_CMAddDeployable;
            On.RoR2.CharacterMaster.RemoveDeployable += On_CMRemoveDeployable;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.AddDeployable -= On_CMAddDeployable;
            On.RoR2.CharacterMaster.RemoveDeployable -= On_CMRemoveDeployable;
        }

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            if(!self.master || !NetworkServer.active) return;
            var cpt = self.master.gameObject.GetComponent<LifeSavingsComponent>();
            if(!cpt) cpt = self.master.gameObject.AddComponent<LifeSavingsComponent>();
            if(NetworkServer.active) cpt.ServerUpdateIcnt();
        }

        private void On_CMAddDeployable(On.RoR2.CharacterMaster.orig_AddDeployable orig, CharacterMaster self, Deployable dpl, DeployableSlot dpls) {
            orig(self, dpl, dpls);
            if(inclDeploys && NetworkServer.active && self.TryGetComponent<LifeSavingsComponent>(out var cpt)) {
                cpt.ServerUpdateIcnt();
            }
        }
        private void On_CMRemoveDeployable(On.RoR2.CharacterMaster.orig_RemoveDeployable orig, CharacterMaster self, Deployable dpl) {
            orig(self, dpl);
            if(inclDeploys && NetworkServer.active && self.TryGetComponent<LifeSavingsComponent>(out var cpt)) {
                cpt.ServerUpdateIcnt();
            }
        }
    }
        
    [RequireComponent(typeof(CharacterMaster))]
    public class LifeSavingsComponent : NetworkBehaviour {
        private float moneyBuffer = 0f;
        public int icnt = 0;

        CharacterMaster master;

        public void Awake() {
            master = GetComponent<CharacterMaster>();
        }

        public void ServerUpdateIcnt() {
            if(master.hasBody) {
                var body = master.GetBody();
                icnt = LifeSavings.instance.GetCount(body);
            }
            if(LifeSavings.instance.inclDeploys)
                icnt += LifeSavings.instance.GetCountOnDeployables(master);
        }

        internal static float CalculateMoneyIncrease(int count) {
            return LifeSavings.instance.gainPerSec * ((count < LifeSavings.instance.invertCount)?(1f/(float)(LifeSavings.instance.invertCount-count+1)):(count-LifeSavings.instance.invertCount+1));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void FixedUpdate() {
            if(!NetworkServer.active) return;
            if(icnt > 0)
                moneyBuffer += Time.fixedDeltaTime * CalculateMoneyIncrease(icnt);
            //Disable during pre-teleport money drain so it doesn't softlock
            //Accumulator is emptied into actual money variable whenever a tick passes and it has enough for a change in integer value
            if(moneyBuffer >= 1.0f && !SceneExitController.isRunning && (LifeSavings.instance.ignoreTimestop || !Run.instance.isRunStopwatchPaused)){
                if(Compat_ShareSuite.enabled && Compat_ShareSuite.MoneySharing())
                    Compat_ShareSuite.GiveMoney((uint)Math.Floor(moneyBuffer));
                else
                    master.GiveMoney((uint)Math.Floor(moneyBuffer));
                moneyBuffer %= 1.0f;
            }
        }
    }
}
