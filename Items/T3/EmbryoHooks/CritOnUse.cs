using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class CritOnUse : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.CritOnUse;

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

            var cpt = body.gameObject.GetComponent<CritOnUseComponent>();
            if(!cpt)
                body.gameObject.AddComponent<CritOnUseComponent>();
        }

        private void EquipmentSlot_FireCritOnUse(ILContext il) {
            ILCursor c = new ILCursor(il);

            CritOnUseComponent cpt = null;
            bool boost = false;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.instance.CheckEmbryoProc(slot.characterBody);
                cpt = slot.characterBody?.GetComponentInChildren<CritOnUseComponent>();
            });

            bool ilFound = c.TryGotoNext(
                x => x.OpCode == OpCodes.Ldc_R4,
                x => x.MatchCallOrCallvirt<CharacterBody>("AddTimedBuff"));

            if(ilFound) {
                //Advance cursor to the found ldcR4 (time argument of AddTimedBuff)
                c.Index += 1;
                //Replace original buff time with a custom function to check for Embryo proc
                //If proc happens, doubles the buff time; otherwise returns original
                c.EmitDelegate<Func<float, float>>((origBuffTime) => {
                    if(cpt) cpt.lastUseWasBoosted = boost;
                    return boost ? origBuffTime * 2 : origBuffTime;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Blackhole; target instructions not found");
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
                    return (slot.characterBody?.GetComponentInChildren<CritOnUseComponent>()?.lastUseWasBoosted == true) ? origValue * 2f : origValue;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: CritOnUse VFX (RpcOnEquipmentActivationReceived); target instructions not found");
            }
        }

        public class CritOnUseComponent : NetworkBehaviour {
            public bool lastUseWasBoosted = false;
        }
    }
}
