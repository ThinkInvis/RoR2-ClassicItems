using Mono.Cecil.Cil;
using R2API;
using RoR2;
using MonoMod.Cil;
using System;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class LifestealOnHit : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/LifestealOnHit");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_LIFESTEALONHIT";
        public override string configDisplayName => "SuperMassiveLeech";

        protected override void InstallHooks() {
            IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        protected override void UninstallHooks() {
            IL.RoR2.GlobalEventManager.OnHitEnemy -= GlobalEventManager_OnHitEnemy;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double lifesteal amount.</style>");
        }

        private void GlobalEventManager_OnHitEnemy(ILContext il) {
            ILCursor c = new ILCursor(il);

            int bodyIndex = 0;
            bool ilFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out bodyIndex),
                x => x.MatchLdsfld("RoR2.RoR2Content/Buffs", "LifeSteal"),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
                x => x.MatchLdcR4(out _));

            if(ilFound) {
                c.Emit(OpCodes.Ldloc, bodyIndex);
                c.EmitDelegate<Func<float, CharacterBody, float>>((origAmt, body) => {
                    var boost = Embryo.CheckLastEmbryoProc(body);
                    return origAmt * (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: LifestealOnHit; target instructions not found");
            }
        }
    }
}
