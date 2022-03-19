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

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
            On.RoR2.EquipmentSlot.FireBfg += On_ESFireBfg;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FixedUpdate -= EquipmentSlot_FixedUpdate;
            On.RoR2.EquipmentSlot.FireBfg -= On_ESFireBfg;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double impact damage.<style>");
        }

        protected internal override void AddComponents(CharacterBody body) {
            base.AddComponents(body);

            if(!NetworkServer.active) return;

            var cpt = body.gameObject.GetComponent<EmbryoBFGComponent>();
            if(!cpt)
                body.gameObject.AddComponent<EmbryoBFGComponent>();
        }

        private void EquipmentSlot_FixedUpdate(ILContext il) {
            ILCursor c = new ILCursor(il);

            EmbryoBFGComponent cpt = null;
            bool boost = false;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.instance.CheckEmbryoProc(slot.characterBody);
                cpt = slot.characterBody?.GetComponentInChildren<EmbryoBFGComponent>();
            });

            bool ilFound = c.TryGotoNext(
                    x => x.MatchLdstr("Prefabs/Projectiles/BeamSphere"))
                && c.TryGotoNext(
                    x => x.MatchCallvirt<CharacterBody>("get_damage"),
                    x => x.OpCode == OpCodes.Ldc_R4,
                    x => x.MatchMul());

            if(ilFound) {
                c.Index += 2;
                c.EmitDelegate<Func<float, float>>((origDamage) => {
                    if(cpt && cpt.boostedBFGs > 0) {
                        cpt.boostedBFGs--;
                        return origDamage * 2f;
                    }
                    return origDamage;
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: BFG (FixedUpdate)");
            }
        }

        private bool On_ESFireBfg(On.RoR2.EquipmentSlot.orig_FireBfg orig, EquipmentSlot self) {
            var retv = orig(self);
            bool boost = Embryo.instance.CheckEmbryoProc(self.inventory);
            var cpt = self.characterBody?.gameObject.GetComponent<EmbryoBFGComponent>();

            if(boost && cpt)
                cpt.boostedBFGs++;

            return retv;
        }
    }

    public class EmbryoBFGComponent : MonoBehaviour {
        public int boostedBFGs = 0;
    }
}
