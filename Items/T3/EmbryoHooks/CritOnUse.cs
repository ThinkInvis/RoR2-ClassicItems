using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class CritOnUse : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.CritOnUse;
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_CRITONUSE";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireCritOnUse += EquipmentSlot_FireCritOnUse;
            IL.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += IL_ESRpcOnEquipmentActivationReceived;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireCritOnUse -= EquipmentSlot_FireCritOnUse;
            IL.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved -= IL_ESRpcOnEquipmentActivationReceived;
        }

        protected internal override void AddComponents(CharacterBody body) {
            base.AddComponents(body);

            var cpt = body.gameObject.GetComponent<EmbryoCritOnUseComponent>();
            if(!cpt)
                body.gameObject.AddComponent<EmbryoCritOnUseComponent>();
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double duration.<style>");
        }

        private void EquipmentSlot_FireCritOnUse(ILContext il) {
            ILCursor c = new ILCursor(il);

            EmbryoCritOnUseComponent cpt = null;
            bool boost = false;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.instance.CheckEmbryoProc(slot.characterBody);
                cpt = slot.characterBody?.GetComponentInChildren<EmbryoCritOnUseComponent>();
            });

            bool ilFound = c.TryGotoNext(
                x => x.OpCode == OpCodes.Ldc_R4,
                x => x.MatchCallOrCallvirt<CharacterBody>("AddTimedBuff"));

            if(ilFound) {
                c.Index += 1;
                c.EmitDelegate<Func<float, float>>((origBuffTime) => {
                    if(cpt) cpt.lastUseWasBoosted = boost;
                    return boost ? origBuffTime * 2 : origBuffTime;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: CritOnUse; target instructions not found");
            }
        }

        private void IL_ESRpcOnEquipmentActivationReceived(ILContext il) {
            ILCursor c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("activeDuration"),
                x => x.MatchLdcR4(out _));

            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EquipmentSlot, float>>((origValue, slot) => {
                    return (slot.characterBody?.GetComponentInChildren<EmbryoCritOnUseComponent>()?.lastUseWasBoosted == true) ? origValue * 2f : origValue;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: CritOnUse VFX (RpcOnEquipmentActivationReceived); target instructions not found");
            }
        }

        public class EmbryoCritOnUseComponent : NetworkBehaviour {
            public bool lastUseWasBoosted = false;
        }
    }
}
