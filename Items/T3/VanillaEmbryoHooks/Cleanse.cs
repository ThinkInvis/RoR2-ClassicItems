using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Cleanse : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Cleanse");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_CLEANSE";

        protected override void InstallHooks() {
            IL.RoR2.Util.CleanseBody += IL_UtilCleanseBody;
        }

        protected override void UninstallHooks() {
            IL.RoR2.Util.CleanseBody -= IL_UtilCleanseBody;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double cleanse radius.<style>");
        }

        private void IL_UtilCleanseBody(ILContext il) {
            ILCursor c = new ILCursor(il);

            var boost = Embryo.InjectLastProcCheckIL(c);

            bool ILFound = c.TryGotoNext(
                x => x.MatchCall<Vector3>("get_sqrMagnitude"),
                x => x.MatchLdloc(out _),
                x => x.MatchBgeUn(out _));

            if(ILFound) {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, CharacterBody, float>>((origValue, body) => {
                    return origValue * (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Blast Shower (Util.CleanseBody); target instructions not found");
            }
        }
    }
}