using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Fruit : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Fruit");
        public override string configDisplayName => "ForeignFruit";
    }

    public class Lightning : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Lightning");
        public override string configDisplayName => "RoyalCapacitor";
    }

    public class DroneBackup : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/DroneBackup");
        public override string configDisplayName => "TheBackup";
    }

    public class PassiveHealing : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/PassiveHealing");
        public override string configDisplayName => "GnarledWoodsprite";
    }

    public class Saw : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/Saw");
        public override string configDisplayName => "Sawmerang";
    }

    public class DeathProjectile : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/DeathProjectile");
        public override string configDisplayName => "ForgiveMePlease";
    }
}
