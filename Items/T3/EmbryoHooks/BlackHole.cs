using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class BlackHole : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/BlackHole");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_BLACKHOLE";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole += EquipmentSlot_FireBlackhole;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole -= EquipmentSlot_FireBlackhole;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double pull radius.<style>");
        }

        private void EquipmentSlot_FireBlackhole(MonoMod.Cil.ILContext il) {
            ILCursor c = new ILCursor(il);

            bool boost = Embryo.ILInjectProcCheck(c);

            bool ilFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Prefabs/Projectiles/GravSphere"),
                x => x.MatchCallOrCallvirt<Resources>("Load"));

            if(ilFound) {
                c.EmitDelegate<Func<GameObject, GameObject>>((obj) => {
                    var newobj = UnityEngine.Object.Instantiate(obj);
                    if(boost) newobj.GetComponent<RadialForce>().radius *= 2;
                    return newobj;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Blackhole; target instructions not found");
            }
        }
    }
}
