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

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance of freezing each individual enemy for every Snowglobe tick.", AutoConfigFlags.None, 0f, 100f)]
        public float procRate {get;private set;} = 30f;

        [AutoConfigRoOIntSlider("{0:N0}", 0, 100)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Number of 1-second ticks of Snowglobe duration.", AutoConfigFlags.PreventNetMismatch, 0, int.MaxValue)]
        public int duration {get;private set;} = 8;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfig("Duration of freeze applied by Snowglobe.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float freezeTime {get;private set;} = 1.5f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 10f)]
        [AutoConfig("Duration of slow applied by Snowglobe.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float slowTime {get;private set;} = 3.0f;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, Snowglobe will slow targets even if they can't be frozen.")]
        public bool slowUnfreezable {get;private set;} = true;

        private GameObject snowglobeControllerPrefab;
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => $"Randomly {(procRate > 0f ? "freeze" : "slow")} enemies for {duration:N0} seconds.";
        protected override string GetDescString(string langid = null) => "Summon a snowstorm that" +
            $" <style=cIsUtility>{(procRate > 0f ? "freezes" : "slows")}</style> monsters at a <style=cIsUtility>{Pct(procRate, 1, 1)}/second chance " +
            $"over {duration:N0} seconds</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Snowglobe() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/snowglobe_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Snowglobe.prefab");
        }

        public override void SetupConfig() {
            base.SetupConfig();
            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(duration)) {
                    snowglobeControllerPrefab.GetComponentInChildren<PostProcessDuration>().maxDuration = (int)args.newValue;
                }
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
            var ctrlPfb2 = new GameObject("snowglobeControllerPrefabPrefab");
            ctrlPfb2.AddComponent<NetworkIdentity>();
            ctrlPfb2.AddComponent<SnowglobeController>();

            var msTemp = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm");

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

            snowglobeControllerPrefab = ctrlPfb2.InstantiateClone("snowglobeControllerPrefab", true);
            UnityEngine.Object.Destroy(ctrlPfb2);

            LanguageAPI.Add("EMBRYO_DESC_APPEND_SNOWGLOBE", "\n<style=cStack>Beating Embryo: Double duration.</style>");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
            Embryo.RegisterHook(this.equipmentDef, "EMBRYO_DESC_APPEND_SNOWGLOBE", () => "Snowglobe");
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            if(!slot.characterBody || !slot.characterBody.teamComponent) return false;
            var ctrlInst = UnityEngine.Object.Instantiate(snowglobeControllerPrefab, slot.characterBody.corePosition, Quaternion.identity);
            ctrlInst.GetComponent<SnowglobeController>().myTeam = slot.characterBody.teamComponent.teamIndex;
            var boost = Embryo.CheckLastEmbryoProc(slot.characterBody, equipmentDef) + 1;
            ctrlInst.GetComponent<SnowglobeController>().remainingTicks *= boost;
            ctrlInst.GetComponentInChildren<PostProcessDuration>().maxDuration *= boost;
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
            var teamMembers = GatherEnemies(myTeam);
            foreach(TeamComponent tcpt in teamMembers) {
                if(!Util.CheckRoll(Snowglobe.instance.procRate)) continue;
                var ssoh = tcpt.gameObject.GetComponent<SetStateOnHurt>();
                var hcpt = tcpt.gameObject.GetComponent<HealthComponent>();
                if(ssoh?.canBeFrozen == true) {
                    hcpt.body.AddTimedBuff(ClassicItemsPlugin.freezeBuff, Snowglobe.instance.freezeTime);
                    ssoh.SetFrozen(Snowglobe.instance.freezeTime);
                }
                if((ssoh?.canBeFrozen == true || Snowglobe.instance.slowUnfreezable) && hcpt) {
                    hcpt.body.AddTimedBuff(RoR2Content.Buffs.Slow60, Snowglobe.instance.slowTime);
                }
			}
        }
    }
}