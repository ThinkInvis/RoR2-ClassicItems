using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Pillage : Equipment<Pillage> {
        public override string displayName => "Pillaged Gold";

        [AutoConfigRoOSlider("{0:N0} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the buff applied by Pillaged Gold.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 14f;

        [AutoConfigRoOSlider("${0:N1}", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Money per hit provided during Pillaged Gold effect.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float amount { get; private set; } = 1f;

        public BuffDef pillageBuff {get;private set;}
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => $"For {duration:N0} seconds, hitting enemies cause them to drop gold.";
        protected override string GetDescString(string langid = null) => $"While active, every hit <style=cIsUtility>drops {amount:N1} gold</style> (scales with difficulty). Lasts <style=cIsUtility>{duration:N0} seconds</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Pillage() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/pillage_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Pillage.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            pillageBuff = ScriptableObject.CreateInstance<BuffDef>();
            pillageBuff.buffColor = new Color(0.85f, 0.8f, 0.3f);
            pillageBuff.canStack = true;
            pillageBuff.isDebuff = false;
            pillageBuff.name = modInfo.shortIdentifier + "PillagedGold";
            pillageBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/pillage_icon.png");
            ContentAddition.AddBuffDef(pillageBuff);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            Embryo.RegisterHook(this.equipmentDef, "EMBRYO_DESC_APPEND_RETRIGGER", () => "PillagedGold");
        }

        public override void Install() {
            base.Install();
            On.RoR2.GlobalEventManager.OnHitEnemy += On_GEMOnHitEnemy;
            //ConfigEntryChanged += Evt_ConfigEntryChanged;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.GlobalEventManager.OnHitEnemy -= On_GEMOnHitEnemy;
            //ConfigEntryChanged -= Evt_ConfigEntryChanged;
        }

        /*private void Evt_ConfigEntryChanged(object sender, AutoUpdateEventArgs args) {
            if(args.changedProperty.Name == nameof(duration)) {
            }
        }*/ //TODO: update buff timers. will require a lot of reflection

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var sbdy = slot.characterBody;
            if(!sbdy) return false;
            sbdy.ClearTimedBuffs(pillageBuff);
            var count = Embryo.CheckLastEmbryoProc(slot, equipmentDef) + 1;
            for(var i = 0; i < count; i++)
                sbdy.AddTimedBuff(pillageBuff, duration);
            return true;
        }

        private void On_GEMOnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            orig(self, damageInfo, victim);
			if(!NetworkServer.active || !damageInfo.attacker) return;

            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            if(!body || !body.HasBuff(pillageBuff)) return;

            CharacterMaster chrm = body.master;
            if(!chrm) return;

            float mamt = (float)Run.instance.GetDifficultyScaledCost(body.GetBuffCount(pillageBuff)) * amount;
            if(!body.gameObject.TryGetComponent<PillageMoneyBuffer>(out var pmb))
                pmb = body.gameObject.AddComponent<PillageMoneyBuffer>();
            var bufferAmt = pmb.AddMoneyAndEmptyBuffer(Mathf.Max(mamt, 0f));

            if(Compat_ShareSuite.enabled && Compat_ShareSuite.MoneySharing())
                Compat_ShareSuite.GiveMoney(bufferAmt);
            else
                chrm.GiveMoney(bufferAmt);
        }
    }
    public class PillageMoneyBuffer : MonoBehaviour {
        float _buffer = 0f;
        public uint AddMoneyAndEmptyBuffer(float amount) {
            _buffer += amount;
            var totalToAdd = Mathf.FloorToInt(_buffer);
            _buffer -= totalToAdd;
            return (uint)Mathf.Max(totalToAdd, 0);
        }
    }
}