using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Witness : Item<Witness> {
        public override string displayName => "Burning Witness";
		public override ItemTier itemTier => ItemTier.Boss;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect});
        public override bool itemAIB {get; protected set;} = true;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Duration of on-kill buff applied by the first stack of Burning Witness.", AICFlags.None, 0f, float.MaxValue)]
        public float baseDuration {get; private set;} = 6f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Duration of on-kill buff applied per additional stack of Burning Witness.", AICFlags.None, 0f, float.MaxValue)]
        public float stackDuration {get; private set;} = 3f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Move speed bonus from the first stack of Burning Witness, while active.", AICFlags.None, 0f, float.MaxValue)]
        public float baseSpeed {get; private set;} = 0.05f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Move speed bonus per additional stack of Burning Witness, while active.", AICFlags.None, 0f, float.MaxValue)]
        public float stackSpeed {get; private set;} = 0.05f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Damage bonus applied by Burning Witness, while active.", AICFlags.None, 0f, float.MaxValue)]
        public float damage {get; private set;} = 1f;     
        
        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "The Worm's eye seems to still see.. watching.. rewarding..";        
        protected override string NewLangDesc(string langid = null) => "<style=cDeath>On kill</style>: Grants a <style=cIsDamage>firetrail</style>, <style=cIsUtility>" + Pct(baseSpeed) + " movement speed</style> <style=cStack>(+" + Pct(stackSpeed) + " per stack)</style>, and <style=cIsDamage>+" + damage.ToString("N1") + " damage</style> for " + baseDuration.ToString("N0") + " <style=cStack>(+" + stackDuration.ToString("N0") + " per stack)</style>.";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Witness() {}

        protected override void LoadBehavior() {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += On_SSOHOnTakeDamageServer;
        }

        protected override void UnloadBehavior() {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer -= On_SSOHOnTakeDamageServer;
        }

        private void On_SSOHOnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, RoR2.DamageReport damageReport) {
            orig(self, damageReport);
            int icnt = GetCount(damageReport.attackerMaster);
            if(icnt < 1) return;
        }
    }
}
