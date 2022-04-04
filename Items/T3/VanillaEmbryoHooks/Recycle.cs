using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Recycle : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Recycle");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_RECYCLE";
        public override string configDisplayName => "Recycler";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireRecycle += EquipmentSlot_FireRecycle;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireRecycle -= EquipmentSlot_FireRecycle;
        }


        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Recycle twice. Cannot multiproc.</style>"); //todo: recycle into an option with 1 item per proc
        }

        private void EquipmentSlot_FireRecycle(ILContext il) {
            ILCursor c = new ILCursor(il);

            int boost = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.CheckLastEmbryoProc(slot);
            });

            bool ILFound = c.TryGotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(1),
                x => x.MatchCallOrCallvirt<GenericPickupController>("set_NetworkRecycled"));

            if(ILFound) {
                c.Index++;
                c.Emit(OpCodes.Dup);
                c.Index++;
                c.EmitDelegate<Func<GenericPickupController, bool, bool>>((pctrl, origRecyc) => {
                    Debug.Log($"setting recyc flag {boost} {pctrl} {pctrl?.GetComponent<EmbryoRecycleFlag>()?.ToString() ?? "NULL"}");
                    if(boost > 0 && pctrl && pctrl.GetComponent<EmbryoRecycleFlag>() == null) {
                        pctrl.gameObject.AddComponent<EmbryoRecycleFlag>();
                        return false;
                    }
                    return true;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Recycle; target instructions not found");
            }
        }
    }

    public class EmbryoRecycleFlag : MonoBehaviour {

    }
}
