using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class BlackHole : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/BlackHole");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_BLACKHOLE";
        public override string configDisplayName => "PrimordialCube";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole += EquipmentSlot_FireBlackhole;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole -= EquipmentSlot_FireBlackhole;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double pull radius.</style>");
        }

        private void EquipmentSlot_FireBlackhole(ILContext il) {
            ILCursor c = new ILCursor(il);

            int boost = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.CheckLastEmbryoProc(slot);
            });

            bool ilFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Prefabs/Projectiles/GravSphere"),
                x => x.MatchCallOrCallvirt("RoR2.LegacyResourcesAPI", "Load"));

            if(ilFound) {
                c.EmitDelegate<Func<GameObject, GameObject>>((obj) => {
                    var newobj = UnityEngine.Object.Instantiate(obj);
                    newobj.GetComponent<RadialForce>().radius *= boost + 1;
                    return newobj;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Blackhole; target instructions not found");
            }
        }
    }
}
