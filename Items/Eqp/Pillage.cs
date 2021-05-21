using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Pillage : Equipment<Pillage> {
        public override string displayName => "Pillaged Gold";

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the buff applied by Pillaged Gold.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 14f;

        public BuffDef pillageBuff {get;private set;}
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "For " + duration.ToString("N0") + " seconds, hitting enemies cause them to drop gold.";
        protected override string GetDescString(string langid = null) => "While active, every hit <style=cIsUtility>drops 1 gold</style> (scales with difficulty). Lasts <style=cIsUtility>" + duration.ToString("N0") + " seconds</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupAttributes() {
            base.SetupAttributes();

            pillageBuff = ScriptableObject.CreateInstance<BuffDef>();
            pillageBuff.buffColor = new Color(0.85f, 0.8f, 0.3f);
            pillageBuff.canStack = true;
            pillageBuff.isDebuff = false;
            pillageBuff.name = modInfo.shortIdentifier + "PillagedGold";
            pillageBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/pillage_icon.png");
            R2API.BuffAPI.Add(new R2API.CustomBuff(pillageBuff));
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
            sbdy.AddTimedBuff(pillageBuff, duration);
            if(Embryo.instance.CheckEmbryoProc(sbdy)) sbdy.AddTimedBuff(pillageBuff, duration);
            return true;
        }

        private void On_GEMOnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim) {
            orig(self, damageInfo, victim);
			if(!NetworkServer.active || !damageInfo.attacker) return;

            CharacterBody body = damageInfo.attacker.GetComponent<CharacterBody>();
            if(!body || !body.HasBuff(pillageBuff)) return;

            CharacterMaster chrm = body.master;
            if(!chrm) return;

            int mamt = Run.instance.GetDifficultyScaledCost(body.GetBuffCount(pillageBuff));

            if(Compat_ShareSuite.enabled && Compat_ShareSuite.MoneySharing())
                Compat_ShareSuite.GiveMoney((uint)mamt);
            else
                chrm.GiveMoney((uint)mamt);
        }
    }
}