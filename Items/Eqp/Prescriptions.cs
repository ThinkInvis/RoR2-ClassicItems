using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class Prescriptions : Equipment<Prescriptions> {
        public override string displayName => "Prescriptions";

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidatePickupToken)]
        [AutoItemConfig("Duration of the buff applied by Prescriptions.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 11f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Attack speed added while Prescriptions is active.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float aSpdBoost {get;private set;} = 0.4f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Base damage added while Prescriptions is active.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float dmgBoost {get;private set;} = 10f;

        public BuffIndex prescriptionsBuff {get;private set;}
        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Increase damage and attack speed for " + duration.ToString("N0") + " seconds.";        
        protected override string NewLangDesc(string langid = null) => "While active, increases <style=cIsDamage>base damage by " + dmgBoost.ToString("N0") + " points</style> and <style=cIsDamage>attack speed by " + Pct(aSpdBoost) + "</style>. Lasts <style=cIsDamage>" + duration.ToString("N0") + " seconds</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Prescriptions() {
            onAttrib += (tokenIdent, namePrefix) => {
                
                var prescriptionsBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = Color.red,
                    canStack = true,
                    isDebuff = false,
                    name = namePrefix + "Prescriptions",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/Prescriptions_icon.png"
                });
                prescriptionsBuff = R2API.BuffAPI.Add(prescriptionsBuffDef);
            };
        }

        protected override void LoadBehavior() {
            OnPreRecalcStats += Evt_TILER2OnPreRecalcStats;
        }
        protected override void UnloadBehavior() {
            OnPreRecalcStats -= Evt_TILER2OnPreRecalcStats;
        }
        

        private void Evt_TILER2OnPreRecalcStats(CharacterBody sender, StatHookEventArgs args) {
            if(sender.HasBuff(prescriptionsBuff)) {
                args.baseDamageAdd += dmgBoost;
                args.attackSpeedMultAdd += sender.GetBuffCount(prescriptionsBuff) * aSpdBoost;
            }
        }

        protected override bool OnEquipUseInner(EquipmentSlot slot) {
            var sbdy = slot.characterBody;
            if(!sbdy) return false;
            sbdy.ClearTimedBuffs(prescriptionsBuff);
            sbdy.AddTimedBuff(prescriptionsBuff, duration);
            if(instance.CheckEmbryoProc(sbdy)) sbdy.AddTimedBuff(prescriptionsBuff, duration);
            return true;
        }
    }
}