using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class FireBallDash : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/FireBallDash");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_FIREBALLDASH";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireFireBallDash += EquipmentSlot_FireFireBallDash;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireFireBallDash -= EquipmentSlot_FireFireBallDash;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double speed and damage.<style>");
        }

        private void EquipmentSlot_FireFireBallDash(ILContext il) {
            var c = new ILCursor(il);

            var boost = Embryo.InjectLastProcCheckIL(il);

            bool ILFound = c.TryGotoNext(
                x => x.MatchLdstr("Prefabs/NetworkedObjects/FireballVehicle"))
            && c.TryGotoNext(
                x => x.MatchCallOrCallvirt<UnityEngine.Object>("Instantiate"));

            if(ILFound) {
                c.Index++;

                c.Emit(OpCodes.Dup);
                c.EmitDelegate<Action<GameObject>>((go) => {
                    go.GetComponent<FireballVehicle>().targetSpeed *= (boost + 1);
                    go.GetComponent<FireballVehicle>().acceleration *= (boost + 1);
                    go.GetComponent<FireballVehicle>().blastDamageCoefficient *= (boost + 1);
                    go.GetComponent<FireballVehicle>().overlapDamageCoefficient *= (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: FireBallDash; target instructions not found");
            }
        }
    }
}
