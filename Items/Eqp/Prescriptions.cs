using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class Prescriptions : Equipment_V2<Prescriptions> {
        public override string displayName => "Prescriptions";

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Duration of the buff applied by Prescriptions.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 11f;

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage | AutoUpdateEventFlags_V2.InvalidateStats)]
        [AutoConfig("Attack speed added while Prescriptions is active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float aSpdBoost {get;private set;} = 0.4f;

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage | AutoUpdateEventFlags_V2.InvalidateStats)]
        [AutoConfig("Base damage added while Prescriptions is active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float dmgBoost {get;private set;} = 10f;

        public BuffIndex prescriptionsBuff {get;private set;}
        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) {
            string desc = "Increase";
            if(dmgBoost > 0f) desc += $" damage";
            if(dmgBoost > 0f && aSpdBoost > 0f) desc += " and";
            if(aSpdBoost > 0f) desc += $" attack speed";
            if(dmgBoost <= 0f && aSpdBoost <= 0f) desc += $" <style=cIsDamage>NOTHING</style>";
            desc += $" for {duration:N0} seconds.";
            return desc;
        }
        protected override string GetDescString(string langid = null) {
            string desc = "While active, increases";
            if(dmgBoost > 0f) desc += $" <style=cIsDamage>base damage by {dmgBoost:N0} points</style>";
            if(dmgBoost > 0f && aSpdBoost > 0f) desc += " and";
            if(aSpdBoost > 0f) desc += $" <style=cIsDamage>attack speed by {Pct(aSpdBoost)}</style>";
            if(dmgBoost <= 0f && aSpdBoost <= 0f) desc += $" <style=cIsDamage>NOTHING</style>";
            desc += $". Lasts <style=cIsDamage>{duration:N0} seconds</style>.";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupAttributes() {
            base.SetupAttributes();

            var prescriptionsBuffDef = new R2API.CustomBuff(new BuffDef {
                buffColor = Color.red,
                canStack = true,
                isDebuff = false,
                name = $"{modInfo.shortIdentifier}Prescriptions",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/Prescriptions_icon.png"
            });
            prescriptionsBuff = R2API.BuffAPI.Add(prescriptionsBuffDef);
        }

        public override void Install() {
            base.Install();
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }
        public override void Uninstall() {
            base.Uninstall();
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }
        

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(sender.HasBuff(prescriptionsBuff)) {
                args.baseDamageAdd += dmgBoost;
                args.attackSpeedMultAdd += sender.GetBuffCount(prescriptionsBuff) * aSpdBoost;
            }
        }

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var sbdy = slot.characterBody;
            if(!sbdy) return false;
            sbdy.ClearTimedBuffs(prescriptionsBuff);
            sbdy.AddTimedBuff(prescriptionsBuff, duration);
            if(instance.CheckEmbryoProc(sbdy)) sbdy.AddTimedBuff(prescriptionsBuff, duration);
            return true;
        }
    }
}