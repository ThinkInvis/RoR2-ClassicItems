#if DEBUG
using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Witness : Item_V2<Witness> {
        public override string displayName => "Burning Witness";
		public override ItemTier itemTier => ItemTier.Boss;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Duration of on-kill buff applied by the first stack of Burning Witness.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDuration {get; private set;} = 6f;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Duration of on-kill buff applied per additional stack of Burning Witness.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float stackDuration {get; private set;} = 3f;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Move speed bonus from the first stack of Burning Witness, while active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseSpeed {get; private set;} = 0.05f;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Move speed bonus per additional stack of Burning Witness, while active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float stackSpeed {get; private set;} = 0.05f;
        
        [AutoConfigUpdateEventInfo(AutoConfigUpdateEventFlags.InvalidateLanguage)]
        [AutoConfig("Damage bonus applied by Burning Witness, while active.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float damage {get; private set;} = 1f;     
        
        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => "The Worm's eye seems to still see.. watching.. rewarding..";        
        protected override string GetDescString(string langid = null) => "<style=cDeath>On kill</style>: Grants a <style=cIsDamage>firetrail</style>, <style=cIsUtility>" + Pct(baseSpeed) + " movement speed</style> <style=cStack>(+" + Pct(stackSpeed) + " per stack)</style>, and <style=cIsDamage>+" + damage.ToString("N1") + " damage</style> for " + baseDuration.ToString("N0") + " <style=cStack>(+" + stackDuration.ToString("N0") + " per stack)</style>.";        
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Witness() {}

        public override void Install() {
            base.Install();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += On_SSOHOnTakeDamageServer;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer -= On_SSOHOnTakeDamageServer;
        }

        private void On_SSOHOnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, RoR2.DamageReport damageReport) {
            orig(self, damageReport);
            int icnt = GetCount(damageReport.attackerMaster);
            if(icnt < 1) return;
        }
    }
}
#endif