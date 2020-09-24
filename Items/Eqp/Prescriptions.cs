using RoR2;
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
        protected override string NewLangPickup(string langid = null) {
            string desc = "Increase";
            if(dmgBoost > 0f) desc += $" damage";
            if(dmgBoost > 0f && aSpdBoost > 0f) desc += " and";
            if(aSpdBoost > 0f) desc += $" attack speed";
            if(dmgBoost <= 0f && aSpdBoost <= 0f) desc += $" <style=cIsDamage>NOTHING</style>";
            desc += $" for {duration:N0} seconds.";
            return desc;
        }
        protected override string NewLangDesc(string langid = null) {
            string desc = "While active, increases";
            if(dmgBoost > 0f) desc += $" <style=cIsDamage>base damage by {dmgBoost:N0} points</style>";
            if(dmgBoost > 0f && aSpdBoost > 0f) desc += " and";
            if(aSpdBoost > 0f) desc += $" <style=cIsDamage>attack speed by {Pct(aSpdBoost)}</style>";
            if(dmgBoost <= 0f && aSpdBoost <= 0f) desc += $" <style=cIsDamage>NOTHING</style>";
            desc += $". Lasts <style=cIsDamage>{duration:N0} seconds</style>.";
            return desc;
        }
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
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }
        protected override void UnloadBehavior() {
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }
        

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
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