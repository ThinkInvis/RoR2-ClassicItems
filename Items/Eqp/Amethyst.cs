using BepInEx.Configuration;
using RoR2;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Amethyst : Equipment<Amethyst> {
        public override string displayName => "Gigantic Amethyst";

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Resets all your cooldowns.";
        protected override string NewLangDesc(string langid = null) => "Immediately <style=cIsUtility>restores 1 charge each</style> to <style=cIsUtility>all</style> of your <style=cIsUtility>skills</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Amethyst() { }

        protected override bool OnEquipUseInner(EquipmentSlot slot) {
            var sloc = slot.characterBody?.skillLocator;
            if(!sloc) return false;
            sloc.ApplyAmmoPack();
            if(instance.CheckEmbryoProc(slot.characterBody)) sloc.ApplyAmmoPack();
            return true;
        }
    }
}