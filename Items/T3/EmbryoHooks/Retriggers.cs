using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Fruit : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Fruit");
    }

    public class Lightning : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Lightning");
    }

    public class DroneBackup : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/DroneBackup");
    }

    public class PassiveHealing : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/PassiveHealing");
    }

    public class Saw : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Saw");
    }

    public class DeathProjectile : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/DeathProjectile");
    }
}
