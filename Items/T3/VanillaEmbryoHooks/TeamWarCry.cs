using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class TeamWarCry : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/TeamWarCry");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_TEAMWARCRY";

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireTeamWarCry += EquipmentSlot_FireTeamWarCry;
        }
        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireTeamWarCry -= EquipmentSlot_FireTeamWarCry;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Double buff duration.<style>");
        }

        private void EquipmentSlot_FireTeamWarCry(ILContext il) {
            ILCursor c = new ILCursor(il);

            var boost = Embryo.InjectLastProcCheckIL(c);

            bool ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchCallOrCallvirt<CharacterBody>("AddTimedBuff"));

            if(ILFound) {
                c.Index--;
                c.EmitDelegate<Func<float, float>>((origBuffTime) => {
                    return origBuffTime * (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: TeamWarCry; target instructions not found (first buff time replacement)");
            }

            ILFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdcI4(out _),
                x => x.MatchLdcR4(out _),
                x => x.MatchCallOrCallvirt<CharacterBody>("AddTimedBuff"));

            if(ILFound) {
                c.Index--;
                c.EmitDelegate<Func<float, float>>((origBuffTime) => {
                    return origBuffTime * (boost + 1);
                });
            } else {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: TeamWarCry; target instructions not found (second buff time replacement)");
            }
        }
    }
}
