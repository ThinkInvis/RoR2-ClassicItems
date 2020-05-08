using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Pillage : Equipment<Pillage> {
        public override string displayName => "Pillaged Gold";

        [AICAUEventInfo(AICAUEventFlags.InvalidateDescToken | AICAUEventFlags.InvalidatePickupToken)]
        [AutoItemCfg("Duration of the buff applied by Pillaged Gold.", AICFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 14f;

        public BuffIndex pillageBuff {get;private set;}
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "For " + duration.ToString("N0") + " seconds, hitting enemies cause them to drop gold.";
        protected override string NewLangDesc(string langid = null) => "While active, every hit <style=cIsUtility>drops 1 gold</style> (scales with difficulty). Lasts <style=cIsUtility>" + duration.ToString("N0") + " seconds</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Pillage() {
            onAttrib += (tokenIdent, namePrefix) => {
                var pillageBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = new Color(0.85f, 0.8f, 0.3f),
                    canStack = true,
                    isDebuff = false,
                    name = namePrefix + "PillagedGold",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/pillage_icon.png"
                });
                pillageBuff = R2API.BuffAPI.Add(pillageBuffDef);
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.GlobalEventManager.OnHitEnemy += On_GEMOnHitEnemy;
        }

        protected override void UnloadBehavior() {
            On.RoR2.GlobalEventManager.OnHitEnemy -= On_GEMOnHitEnemy;
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot) {
            var sbdy = slot.characterBody;
            if(!sbdy) return false;
            sbdy.ClearTimedBuffs(pillageBuff);
            sbdy.AddTimedBuff(pillageBuff, duration);
            if(instance.CheckEmbryoProc(sbdy)) sbdy.AddTimedBuff(pillageBuff, duration);
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