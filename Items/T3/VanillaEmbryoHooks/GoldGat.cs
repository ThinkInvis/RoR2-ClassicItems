using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class GoldGat : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/GoldGat");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_GOLDGAT";
        public override string configDisplayName => "TheCrowdfunder";

        protected override void InstallHooks() {
            IL.EntityStates.GoldGat.GoldGatFire.FireBullet += GoldGatFire_FireBullet;
        }

        protected override void UninstallHooks() {
            IL.EntityStates.GoldGat.GoldGatFire.FireBullet -= GoldGatFire_FireBullet;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double fire rate. Procs per individual shot.<style>");
        }

        private void GoldGatFire_FireBullet(ILContext il) {
            ILCursor c = new ILCursor(il);

            int boost = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EntityStates.GoldGat.GoldGatFire>>((ggf) => {
                boost = Embryo.CheckEmbryoProc(ggf.networkedBodyAttachment?.attachedBodyObject?.GetComponent<CharacterBody>());
            });

            bool ILFound;

            ILFound = c.TryGotoNext(
                x => x.MatchStfld<EntityStates.GoldGat.GoldGatFire>("fireFrequency"));

            if(ILFound) {
                c.EmitDelegate<Func<float, float>>((origFreq) => {
                    return origFreq * (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: GoldGat (FireBullet); target instructions not found");
            }
        }
    }
}
