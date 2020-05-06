using BepInEx.Configuration;
using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;

namespace ThinkInvisible.ClassicItems {
    public class Snowglobe : ItemBoilerplate<Snowglobe> {
        public override string displayName {get;} = "Snowglobe";

        [AutoItemCfg("Percent chance of freezing each individual enemy for every Snowglobe tick.", default, 0f, 100f)]
        public float procRate {get;private set;} = 30f;
        [AutoItemCfg("Number of 1-second ticks of Snowglobe duration.", default, 0, int.MaxValue)]
        public int duration {get;private set;} = 8;
        [AutoItemCfg("Duration of freeze applied by Snowglobe.", default, 0f, float.MaxValue)]
        public float freezeTime {get;private set;} = 1.5f;
        [AutoItemCfg("Duration of slow applied by Snowglobe.", default, 0f, float.MaxValue)]
        public float slowTime {get;private set;} = 3.0f;
        [AutoItemCfg("If true, Snowglobe will slow targets even if they can't be frozen.")]
        public bool slowUnfreezable {get;private set;} = true;

        private GameObject snowglobeControllerPrefab;

        public override void SetupAttributesInner() {
            itemIsEquipment = true;

            eqpEnigmable = true;
            eqpCooldown = 45;

            RegLang(
                "Randomly freeze enemies for 8 seconds.",
                "Summon a snowstorm that <style=cIsUtility>freezes</style> monsters at a <style=cIsUtility>" + Pct(procRate,1,1) + " chance over " + duration.ToString("N0") + " seconds</style>.",
                "A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
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

            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }
        
        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
                if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
                var ctrlInst = UnityEngine.Object.Instantiate(snowglobeControllerPrefab, slot.characterBody.corePosition, Quaternion.identity);
                ctrlInst.GetComponent<SnowglobeController>().myTeam = slot.characterBody.teamComponent.teamIndex;
                if(Embryo.instance.CheckProc<Snowglobe>(slot.characterBody)) {
                    ctrlInst.GetComponent<SnowglobeController>().remainingTicks *= 2;
                    ctrlInst.GetComponentInChildren<PostProcessDuration>().maxDuration *= 2;
                }
                NetworkServer.Spawn(ctrlInst);
                return true;
            } else return orig(slot, eqpid);
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
            var tind = TeamIndex.Monster | TeamIndex.Neutral | TeamIndex.Player;
			tind &= ~myTeam;
			ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(tind);
			foreach(TeamComponent tcpt in teamMembers) {
                if(!Util.CheckRoll(Snowglobe.instance.procRate)) continue;
                var ssoh = tcpt.gameObject.GetComponent<SetStateOnHurt>();
                var hcpt = tcpt.gameObject.GetComponent<HealthComponent>();
                if(ssoh.canBeFrozen && ssoh) {
                    hcpt.body.AddTimedBuff(ClassicItemsPlugin.freezeBuff, Snowglobe.instance.freezeTime);
                    ssoh.SetFrozen(Snowglobe.instance.freezeTime);
                }
                if(((ssoh?.canBeFrozen ?? false) || Snowglobe.instance.slowUnfreezable) && hcpt) {
                    hcpt.body.AddTimedBuff(BuffIndex.Slow60, Snowglobe.instance.slowTime);
                }
			}
        }
	}
}