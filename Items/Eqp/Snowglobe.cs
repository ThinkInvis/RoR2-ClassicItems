using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using TILER2;
using static TILER2.MiscUtil;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems {
    public class Snowglobe : Equipment<Snowglobe> {
        public override string displayName => "Snowglobe";

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Percent chance of freezing each individual enemy for every Snowglobe tick.", AutoItemConfigFlags.None, 0f, 100f)]
        public float procRate {get;private set;} = 30f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidatePickupToken)]
        [AutoItemConfig("Number of 1-second ticks of Snowglobe duration.", AutoItemConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int duration {get;private set;} = 8;

        [AutoItemConfig("Duration of freeze applied by Snowglobe.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float freezeTime {get;private set;} = 1.5f;

        [AutoItemConfig("Duration of slow applied by Snowglobe.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float slowTime {get;private set;} = 3.0f;

        [AutoItemConfig("If true, Snowglobe will slow targets even if they can't be frozen.")]
        public bool slowUnfreezable {get;private set;} = true;

        private GameObject snowglobeControllerPrefab;
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) {
            string desc = "";
            if(procRate > 0f) desc += $"Randomly freeze enemies";
            else desc += $"Slow enemies";
            desc += " for {duration:N0} seconds.";
            return desc;
        }
        protected override string NewLangDesc(string langid = null) {
            string desc = "Summon a snowstorm that";
            desc += $"<style=cIsUtility>{(procRate>0f ? "freezes" : "slows")}</style> monsters at a <style=cIsUtility>{Pct(procRate, 1, 1)}/second chance ";
            desc += $"over {duration:N0} seconds</style>.";
            return desc;
        }
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Snowglobe() {
            onAttrib += (tokenIdent, namePrefix) => {
                var ctrlPfb2 = new GameObject("snowglobeControllerPrefabPrefab");
                ctrlPfb2.AddComponent<NetworkIdentity>();
                ctrlPfb2.AddComponent<SnowglobeController>();

                var msTemp = Resources.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm");

                var ppvOrig = msTemp.transform.GetChild(0).gameObject.GetComponent<PostProcessVolume>();
                var ppvIn = UnityEngine.Object.Instantiate(msTemp.transform.GetChild(0).gameObject);
                ppvIn.transform.parent = ctrlPfb2.transform;
                var ppv = ppvIn.GetComponent<PostProcessVolume>();
                ppv.sharedProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
			    foreach(PostProcessEffectSettings ppesOrig in ppvOrig.sharedProfile.settings) {
				    PostProcessEffectSettings ppesNew = UnityEngine.Object.Instantiate(ppesOrig);
				    ppv.sharedProfile.settings.Add(ppesNew);
			    }
                ppv.sharedProfile.GetSetting<Vignette>().color.Override(Color.grey);
                ppv.sharedProfile.GetSetting<Vignette>().intensity.Override(0.2f);
                ppv.sharedProfile.GetSetting<Bloom>().color.Override(Color.grey);
                ppv.sharedProfile.GetSetting<Bloom>().intensity.Override(0.5f);
                ppv.sharedProfile.GetSetting<ColorGrading>().mixerRedOutRedIn.Override(150.0f);
                ppv.sharedProfile.GetSetting<ColorGrading>().mixerGreenOutGreenIn.Override(150.0f);
                ppv.sharedProfile.GetSetting<ColorGrading>().mixerBlueOutBlueIn.Override(150.0f);
                ppv.sharedProfile.GetSetting<ColorGrading>().contrast.Override(-10f);
                ppv.sharedProfile.GetSetting<ColorGrading>().postExposure.Override(0.5f);

                var ppdIn = ppvIn.GetComponent<PostProcessDuration>();
                ppdIn.maxDuration = duration;
                ppdIn.ppWeightCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 1f), new Keyframe(0.9f, 1f), new Keyframe(1f, 0f));
                ppdIn.destroyOnEnd = true;

                snowglobeControllerPrefab = ctrlPfb2.InstantiateClone("snowglobeControllerPrefab");
                UnityEngine.Object.Destroy(ctrlPfb2);
            };

            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(duration)) {
                    snowglobeControllerPrefab.GetComponentInChildren<PostProcessDuration>().maxDuration = (int)args.newValue;
                }
            };
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot) {
            if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var ctrlInst = UnityEngine.Object.Instantiate(snowglobeControllerPrefab, slot.characterBody.corePosition, Quaternion.identity);
            ctrlInst.GetComponent<SnowglobeController>().myTeam = slot.characterBody.teamComponent.teamIndex;
            if(instance.CheckEmbryoProc(slot.characterBody)) {
                ctrlInst.GetComponent<SnowglobeController>().remainingTicks *= 2;
                ctrlInst.GetComponentInChildren<PostProcessDuration>().maxDuration *= 2;
            }
            NetworkServer.Spawn(ctrlInst);
            return true;
        }
    }

	public class SnowglobeController : NetworkBehaviour {
        internal int remainingTicks = Snowglobe.instance.duration;
        float stopwatch = 0f;
        public TeamIndex myTeam;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
        private void FixedUpdate() {
            if(!NetworkServer.active) return;
            stopwatch += Time.fixedDeltaTime;
            if(stopwatch >= 1f) {
                stopwatch -= 1f;
                remainingTicks --;
                if(remainingTicks < 0) Destroy(this);
                else DoFreeze();
            }
        }

        private void DoFreeze() {
			List<TeamComponent> teamMembers = new List<TeamComponent>();
			bool isFF = FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off;
			if(isFF || myTeam != TeamIndex.Monster) teamMembers.AddRange(TeamComponent.GetTeamMembers(TeamIndex.Monster));
			if(isFF || myTeam != TeamIndex.Neutral) teamMembers.AddRange(TeamComponent.GetTeamMembers(TeamIndex.Neutral));
			if(isFF || myTeam != TeamIndex.Player) teamMembers.AddRange(TeamComponent.GetTeamMembers(TeamIndex.Player));
			foreach(TeamComponent tcpt in teamMembers) {
                if(!Util.CheckRoll(Snowglobe.instance.procRate)) continue;
                var ssoh = tcpt.gameObject.GetComponent<SetStateOnHurt>();
                var hcpt = tcpt.gameObject.GetComponent<HealthComponent>();
                if(ssoh?.canBeFrozen == true) {
                    hcpt.body.AddTimedBuff(ClassicItemsPlugin.freezeBuff, Snowglobe.instance.freezeTime);
                    ssoh.SetFrozen(Snowglobe.instance.freezeTime);
                }
                if((ssoh?.canBeFrozen == true || Snowglobe.instance.slowUnfreezable) && hcpt) {
                    hcpt.body.AddTimedBuff(BuffIndex.Slow60, Snowglobe.instance.slowTime);
                }
			}
        }
	}
}