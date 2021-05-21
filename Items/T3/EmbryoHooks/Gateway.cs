using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Gateway : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.Gateway;
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_GATEWAY";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireGateway += EquipmentSlot_FireGateway;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireGateway -= EquipmentSlot_FireGateway;
        }

        GameObject boostedGatewayPrefab;

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double transit speed.<style>");

            boostedGatewayPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/Zipline").InstantiateClone("EmbryoBoostedGatewayPrefab");
            var ziplineCtrl = boostedGatewayPrefab.GetComponent<ZiplineController>();
            ziplineCtrl.ziplineVehiclePrefab = ziplineCtrl.ziplineVehiclePrefab.InstantiateClone("EmbryoBoostedGatewayVehiclePrefab");
            var zvh = ziplineCtrl.ziplineVehiclePrefab.GetComponent<ZiplineVehicle>();
            zvh.maxSpeed *= 2f;
            zvh.acceleration *= 2f;
        }

        private void EquipmentSlot_FireGateway(ILContext il) {
            ILCursor c = new ILCursor(il);

            bool boost = Embryo.ILInjectProcCheck(c);

            bool ilFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Prefabs/NetworkedObjects/Zipline"),
                x => x.MatchCallOrCallvirt<Resources>("Load"));

            if(ilFound) {
                c.EmitDelegate<Func<GameObject, GameObject>>((obj) => {
                    return boost ? boostedGatewayPrefab : obj;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Gateway; target instructions not found");
            }
        }
    }
}
