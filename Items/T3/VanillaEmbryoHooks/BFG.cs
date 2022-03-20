using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class BFG : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/BFG");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_BFG";
        public override string configDisplayName => "PreonAccumulator";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FixedUpdate -= EquipmentSlot_FixedUpdate;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double impact damage.<style>");
        }

        private void EquipmentSlot_FixedUpdate(ILContext il) {
            ILCursor c = new ILCursor(il);

            var boost = Embryo.InjectLastProcCheckIL(c);

            bool ilFound = c.TryGotoNext(
                    x => x.MatchLdstr("Prefabs/Projectiles/BeamSphere"))
                && c.TryGotoNext(
                    x => x.MatchCallvirt<CharacterBody>("get_damage"),
                    x => x.OpCode == OpCodes.Ldc_R4,
                    x => x.MatchMul());

            if(ilFound) {
                c.Index += 2;
                c.EmitDelegate<Func<float, float>>((origDamage) => {
                    return origDamage * (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: BFG (FixedUpdate)");
            }
        }
    }
}
