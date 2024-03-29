﻿using RoR2;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class Prescriptions : Equipment<Prescriptions> {
        public override string displayName => "Prescriptions";

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of the buff applied by Prescriptions.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 11f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Attack speed added while Prescriptions is active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float aSpdBoost {get;private set;} = 0.4f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 1000f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage | AutoConfigUpdateActionTypes.InvalidateStats)]
        [AutoConfig("Base damage added while Prescriptions is active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float dmgBoost {get;private set;} = 10f;

        public BuffDef prescriptionsBuff {get;private set;}
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
        protected override string GetLoreString(string langid = null) => "Order: Prescriptions\n\nTracking Number: 817***********\nEstimated Delivery: 1/18/2056\nShipping Method: Volatile\nShipping Address: JusticeCenter, Science Dept., Venus\nShipping Details:\n\nAre these pills legal? No. Are these pills helpful? Yes. Do pigs fly? No.\nVan-Ex increases strength and muscle mass in only 24 hours! Proven by science!\nMake sure to take only 2 every 8 hours, max! Otherwise you may find yourself dead!";

        public Prescriptions() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/prescriptions_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Prescriptions.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            prescriptionsBuff = ScriptableObject.CreateInstance<BuffDef>();
            prescriptionsBuff.buffColor = Color.red;
            prescriptionsBuff.canStack = true;
            prescriptionsBuff.isDebuff = false;
            prescriptionsBuff.name = $"{modInfo.shortIdentifier}Prescriptions";
            prescriptionsBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/Prescriptions_icon.png");
            ContentAddition.AddBuffDef(prescriptionsBuff);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            Embryo.RegisterHook(this.equipmentDef, "EMBRYO_DESC_APPEND_RETRIGGER", () => "CI.Prescriptions");
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
            var count = Embryo.CheckLastEmbryoProc(slot, equipmentDef) + 1;
            for(var i = 0; i < count; i++)
                sbdy.AddTimedBuff(prescriptionsBuff, duration);
            return true;
        }
    }
}