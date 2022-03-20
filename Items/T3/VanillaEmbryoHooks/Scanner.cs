using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Scanner : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Scanner");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_SCANNER";
        public override string configDisplayName => "RadarScanner";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireScanner += EquipmentSlot_FireScanner;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireScanner -= EquipmentSlot_FireScanner;
        }

        GameObject boostedScannerPrefab;

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double duration. Cannot multiproc.<style>");

            boostedScannerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/ChestScanner").InstantiateClone("EmbryoBoostedScannerPrefab", true);
            boostedScannerPrefab.GetComponent<ChestRevealer>().revealDuration *= 2f;
        }

        private void EquipmentSlot_FireScanner(ILContext il) {
            ILCursor c = new ILCursor(il);

            var boost = Embryo.InjectLastProcCheckIL(c);

            bool ilFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Prefabs/NetworkedObjects/ChestScanner"),
                x => x.MatchCallOrCallvirt("RoR2.LegacyResourcesAPI", "Load"));

            if(ilFound) {
                c.EmitDelegate<Func<GameObject, GameObject>>((obj) => {
                    return (boost > 0) ? boostedScannerPrefab : obj;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Scanner; target instructions not found");
            }
        }
    }
}
