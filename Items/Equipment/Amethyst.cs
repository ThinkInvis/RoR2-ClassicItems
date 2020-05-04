using BepInEx.Configuration;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public class Amethyst : ItemBoilerplate<Amethyst> {
        public override string itemCodeName {get;} = "Amethyst";

        protected override void SetupConfigInner(ConfigFile cfl) {
        }
        
        protected override void SetupAttributesInner() {
            itemIsEquipment = true;

            modelPathName = "amethyst_model.prefab";
            iconPathName = "amethyst_icon.png";
            eqpEnigmable = true;
            eqpCooldown = 8;

            RegLang("Gigantic Amethyst",
                "Resets all your cooldowns.",
                "Immediately <style=cIsUtility>restores 1 charge each</style> to <style=cIsUtility>all</style> of your <style=cIsUtility>skills</style>.",
                "A relic of times long past (ClassicItems mod)");
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;
        }
        
        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex eqpid) {
            if(eqpid == regIndexEqp) {
                var sloc = slot.characterBody?.skillLocator;
                if(!sloc) return false;
                sloc.ApplyAmmoPack();
                if(Embryo.instance.CheckProc<Amethyst>(slot.characterBody)) sloc.ApplyAmmoPack();
                return true;
            } else return orig(slot, eqpid);
        }
    }
}