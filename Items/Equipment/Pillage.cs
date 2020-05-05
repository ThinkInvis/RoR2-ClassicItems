using BepInEx.Configuration;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems {
    public class Pillage : ItemBoilerplate<Pillage> {
        public override string itemCodeName {get;} = "Pillage";

        private ConfigEntry<float> cfgDuration;
        public float duration {get;private set;}

        public BuffIndex pillageBuff {get;private set;}

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgDuration = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Duration"), 14f, new ConfigDescription(
                "Duration of the buff applied by Pillaged Gold.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));

            duration = cfgDuration.Value;
        }
        
        protected override void SetupAttributesInner() {
            itemIsEquipment = true;

            modelPathName = "pillage_model.prefab";
            iconPathName = "pillage_icon.png";
            eqpEnigmable = true;
            eqpCooldown = 45;

            RegLang("Pillaged Gold",
                "For 14 seconds, hitting enemies cause them to drop gold.",
                "While active, every hit <style=cIsUtility>drops 1 gold</style> (scales with difficulty).",
                "A relic of times long past (ClassicItems mod)");
        }

        protected override void SetupBehaviorInner() {
            var pillageBuffDef = new R2API.CustomBuff(new BuffDef {
                buffColor = new Color(0.85f, 0.8f, 0.3f),
                canStack = true,
                isDebuff = false,
                name = "PillagedGold",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName
            });
            pillageBuff = R2API.BuffAPI.Add(pillageBuffDef);

            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
            On.RoR2.GlobalEventManager.OnHitEnemy += On_GEMOnHitEnemy;
        }
        
        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
                var sbdy = slot.characterBody;
                if(!sbdy) return false;
                sbdy.ClearTimedBuffs(pillageBuff);
                sbdy.AddTimedBuff(pillageBuff, duration);
                if(Embryo.instance.CheckProc<Pillage>(sbdy)) sbdy.AddTimedBuff(pillageBuff, duration);
                return true;
            } else return orig(slot, eqpid);
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