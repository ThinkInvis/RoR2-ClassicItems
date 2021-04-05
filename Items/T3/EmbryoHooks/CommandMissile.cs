using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class CommandMissile : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.CommandMissile;

        protected override void InstallHooks() {
            On.RoR2.EquipmentSlot.FireCommandMissile += On_ESFireCommandMissile;
        }

        protected override void UninstallHooks() {
            On.RoR2.EquipmentSlot.FireCommandMissile -= On_ESFireCommandMissile;
        }

        private GameObject componentPrefab;

        protected internal override void SetupAttributes() {
            base.SetupAttributes();

            var eCptPrefab2 = new GameObject("embryoCptMissilePrefabPrefab");
            eCptPrefab2.AddComponent<NetworkIdentity>();
            eCptPrefab2.AddComponent<CommandMissileComponent>();
            eCptPrefab2.AddComponent<NetworkedBodyAttachment>().forceHostAuthority = true;
            componentPrefab = eCptPrefab2.InstantiateClone("embryoCptMissilePrefab");
            GameObject.Destroy(eCptPrefab2);
        }

        protected internal override void AddComponents(CharacterBody body) {
            base.AddComponents(body);

            var cpt = body.GetComponentInChildren<CommandMissileComponent>();
            if(!cpt) {
                var cptInst = GameObject.Instantiate(componentPrefab, body.transform);
                cptInst.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject);
            }
        }

        private bool On_ESFireCommandMissile(On.RoR2.EquipmentSlot.orig_FireCommandMissile orig, EquipmentSlot self) {
            var prevM = self.remainingMissiles;
            var retv = orig(self);
            var addedM = self.remainingMissiles - prevM;
            bool boost = Util.CheckRoll(Embryo.instance.GetCount(self.characterBody) * Embryo.instance.procChance);
            CommandMissileComponent cpt = self.characterBody?.GetComponentInChildren<CommandMissileComponent>();

            if(boost && cpt) {
                cpt.boostedMissiles += addedM * 2;
                self.remainingMissiles += addedM;
            }

            return retv;
        }
    }

    public class CommandMissileComponent : NetworkBehaviour {
        public int boostedMissiles = 0;
    }
}
