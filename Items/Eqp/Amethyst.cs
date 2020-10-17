using BepInEx.Configuration;
using RoR2;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Amethyst : Equipment_V2<Amethyst> {
        public override string displayName => "Gigantic Amethyst";

        public override float cooldown {get;protected set;} = 8f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Resets all your cooldowns.";
        protected override string GetDescString(string langid = null) => "Immediately <style=cIsUtility>restores 1 charge each</style> to <style=cIsUtility>all</style> of your <style=cIsUtility>skills</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var sloc = slot.characterBody?.skillLocator;
            if(!sloc) return false;
            sloc.ApplyAmmoPack();
            if(instance.CheckEmbryoProc(slot.characterBody)) sloc.ApplyAmmoPack();
            return true;
        }
    }
}