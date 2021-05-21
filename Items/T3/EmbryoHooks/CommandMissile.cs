using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class CommandMissile : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.CommandMissile;
        public override string descriptionAppendToken => "EMBRYO_DESC_APPEND_COMMANDMISSILE";

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

            var cpt = body.gameObject.GetComponent<CommandMissileComponent>();
            if(!cpt)
                body.gameObject.AddComponent<CommandMissileComponent>();
        }

        private bool On_ESFireCommandMissile(On.RoR2.EquipmentSlot.orig_FireCommandMissile orig, EquipmentSlot self) {
            var prevM = self.remainingMissiles;
            var retv = orig(self);
            var addedM = self.remainingMissiles - prevM;
            bool boost = Embryo.instance.CheckEmbryoProc(self.inventory);
            var cpt = self.characterBody?.gameObject.GetComponent<CommandMissileComponent>();

            if(boost && cpt) {
                cpt.boostedMissiles += addedM * 2;
                self.remainingMissiles += addedM;
            }

            return retv;
        }

        private void On_ESFireMissile(On.RoR2.EquipmentSlot.orig_FireMissile orig, EquipmentSlot self) {
            orig(self);
            var cpt = self.characterBody?.gameObject.GetComponent<CommandMissileComponent>();
            if(cpt && cpt.boostedMissiles > 0)
                self.missileTimer /= 2f;
        }
    }

    public class CommandMissileComponent : MonoBehaviour {
        public int boostedMissiles = 0;
    }
}
