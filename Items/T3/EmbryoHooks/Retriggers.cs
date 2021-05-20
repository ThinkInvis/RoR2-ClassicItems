using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class Fruit : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.Fruit;
    }

    public class Lightning : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.Lightning;
    }

    public class DroneBackup : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.DroneBackup;
    }

    public class PassiveHealing : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.PassiveHealing;
    }

    public class Saw : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.Saw;
    }

    public class DeathProjectile : Embryo.SimpleRetriggerEmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.DeathProjectile;
    }
}
