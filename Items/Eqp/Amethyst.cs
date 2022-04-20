using BepInEx.Configuration;
using RoR2;
using TILER2;
using UnityEngine;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Amethyst : Equipment<Amethyst> {
        public override string displayName => "Gigantic Amethyst";

        public override float cooldown {get;protected set;} = 8f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Resets all your cooldowns.";
        protected override string GetDescString(string langid = null) => "Immediately <style=cIsUtility>restores 1 charge each</style> to <style=cIsUtility>all</style> of your <style=cIsUtility>skills</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        protected override bool PerformEquipmentAction(EquipmentSlot slot) {
            var sloc = slot.characterBody?.skillLocator;
            if(!sloc) return false;
            int count = 1 + Embryo.CheckLastEmbryoProc(slot, equipmentDef);
            for(var i = 0; i < count; i++)
                sloc.ApplyAmmoPack();
            return true;
        }

        public Amethyst() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/amethyst_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/GiganticAmethyst.prefab");
        }

        public override void RefreshPermanentLanguage() {
            permanentGenericLanguageTokens.Add("EMBRYO_DESC_APPEND_AMETHYST", "\n<style=cStack>Beating Embryo: Double duration.</style>");
            base.RefreshPermanentLanguage();
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
            Embryo.RegisterHook(this.equipmentDef, "EMBRYO_DESC_APPEND_AMETHYST", () => "CI.GiganticAmethyst");
        }
    }
}