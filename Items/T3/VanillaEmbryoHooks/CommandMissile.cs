using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class CommandMissile : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/CommandMissile");
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_COMMANDMISSILE";
        public override string configDisplayName => "DisposableMissileLauncher";

        protected override void InstallHooks() {
            On.RoR2.EquipmentSlot.FireCommandMissile += On_ESFireCommandMissile;
            On.RoR2.EquipmentSlot.FireMissile += On_ESFireMissile;
        }

        protected override void UninstallHooks() {
            On.RoR2.EquipmentSlot.FireCommandMissile -= On_ESFireCommandMissile;
            On.RoR2.EquipmentSlot.FireMissile -= On_ESFireMissile;
        }

        protected internal override void SetupAttributes() {
            base.SetupAttributes();
            LanguageAPI.Add(descriptionAppendToken, "\n<style=cStack>Beating Embryo: Fires twice as many missiles at double fire rate.<style>");
        }

        protected internal override void AddComponents(CharacterBody body) {
            base.AddComponents(body);

            if(!NetworkServer.active) return;

            var cpt = body.gameObject.GetComponent<EmbryoCommandMissileComponent>();
            if(!cpt)
                body.gameObject.AddComponent<EmbryoCommandMissileComponent>();
        }

        private bool On_ESFireCommandMissile(On.RoR2.EquipmentSlot.orig_FireCommandMissile orig, EquipmentSlot self) {
            var prevM = self.remainingMissiles;
            var retv = orig(self);
            var addedM = self.remainingMissiles - prevM;
            var (boost, cpt) = Embryo.InjectLastProcCheckDirect<EmbryoCommandMissileComponent>(self);

            if(boost > 0 && cpt) {
                var procM = addedM * boost;
                cpt.boostedMissiles += procM + addedM;
                self.remainingMissiles += procM;
            }

            return retv;
        }

        private void On_ESFireMissile(On.RoR2.EquipmentSlot.orig_FireMissile orig, EquipmentSlot self) {
            orig(self);
            var cpt = self.characterBody?.gameObject.GetComponent<EmbryoCommandMissileComponent>();
            if(cpt && cpt.boostedMissiles > 0) {
                self.missileTimer /= Mathf.Floor(cpt.boostedMissiles/12) + 2;
                cpt.boostedMissiles--;
            }
        }
    }

    public class EmbryoCommandMissileComponent : MonoBehaviour {
        public int boostedMissiles = 0;
    }
}
