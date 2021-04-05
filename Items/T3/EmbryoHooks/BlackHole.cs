using RoR2;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class BlackHole : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.Blackhole;

        protected override void InstallHooks() {
        }

        protected override void UninstallHooks() {
        }
    }
}
