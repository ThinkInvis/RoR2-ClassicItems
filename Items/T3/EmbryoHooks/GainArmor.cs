using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class GainArmor : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/GainArmor");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_GAINARMOR";

        protected override void InstallHooks() {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        protected override void UninstallHooks() {
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double armor.<style>");
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args) {
            if(!sender.HasBuff(RoR2Content.Buffs.ElephantArmorBoost)) return;
            var boost = Embryo.CheckLastEmbryoProc(sender);
            args.armorAdd += boost * 500f;
        }
    }
}
