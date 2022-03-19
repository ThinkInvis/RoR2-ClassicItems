using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Jetpack : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Jetpack");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_JETPACK";

        protected override void InstallHooks() {
            On.RoR2.EquipmentSlot.FireJetpack += EquipmentSlot_FireJetpack;
            IL.RoR2.JetpackController.FixedUpdate += JetpackController_FixedUpdate;
        }

        protected override void UninstallHooks() {
            On.RoR2.EquipmentSlot.FireJetpack -= EquipmentSlot_FireJetpack;
        }

        protected internal override void AddComponents(CharacterBody body) {
            base.AddComponents(body);

            if(!NetworkServer.active) return;

            var cpt = body.gameObject.GetComponent<EmbryoJetpackComponent>();
            if(!cpt)
                body.gameObject.AddComponent<EmbryoJetpackComponent>();
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Jetpack: Double flight duration.<style>");
        }

        private bool EquipmentSlot_FireJetpack(On.RoR2.EquipmentSlot.orig_FireJetpack orig, EquipmentSlot self) {
            var (boost, cpt) = Embryo.InjectLastProcCheckDirect<EmbryoJetpackComponent>(self);
            if(cpt) cpt.boostedFlightTime += boost * 15f;

            return orig(self);
        }

        private void JetpackController_FixedUpdate(ILContext il) {
            ILCursor c = new ILCursor(il);

            var (boost, cpt) = Embryo.InjectLastProcCheckIL<EmbryoJetpackComponent>(c);

            bool ILFound = c.TryGotoNext(
                x => x.MatchCallOrCallvirt<UnityEngine.Time>("get_fixedDeltaTime"),
                x => x.MatchAdd(),
                x => x.MatchStfld<JetpackController>("stopwatch"));

            if(ILFound) {
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, JetpackController, float>>((origDecr, jpc) => {
                    if(!cpt || cpt.boostedFlightTime <= 0) return origDecr;
                    cpt.boostedFlightTime -= origDecr;
                    return 0f;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Jetpack (FixedUpdate); target instructions not found");
            }
        }
    }

    public class EmbryoJetpackComponent : MonoBehaviour {
        public float boostedFlightTime = 0f;
    }
}
