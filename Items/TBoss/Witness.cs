using RoR2;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.ObjectModel;

namespace ThinkInvisible.ClassicItems {
    public class Witness : Item<Witness> {
        public override string displayName => "Burning Witness";
		public override ItemTier itemTier => ItemTier.Boss;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage, ItemTag.Utility, ItemTag.OnKillEffect});
        public override bool itemAIBDefault => true;

        [AutoItemCfg("Duration of on-kill buff applied by the first stack of Burning Witness.", default, 0f, float.MaxValue)]
        public float baseDuration {get; private set;} = 6f;
        [AutoItemCfg("Duration of on-kill buff applied per additional stack of Burning Witness.", default, 0f, float.MaxValue)]
        public float stackDuration {get; private set;} = 3f;
        [AutoItemCfg("Move speed bonus from the first stack of Burning Witness, while active.", default, 0f, float.MaxValue)]
        public float baseSpeed {get; private set;} = 0.05f;
        [AutoItemCfg("Move speed bonus per additional stack of Burning Witness, while active.", default, 0f, float.MaxValue)]
        public float stackSpeed {get; private set;} = 0.05f;
        [AutoItemCfg("Damage bonus applied by Burning Witness, while active.", default, 0f, float.MaxValue)]
        public float damage {get; private set;} = 1f;
        
        public override void SetupAttributesInner() {
            RegLang(
            	"The Worm's eye seems to still see.. watching.. rewarding..",
            	"<style=cDeath>On kill</style>: Grants a <style=cIsDamage>firetrail</style>, <style=cIsUtility>" + Pct(baseSpeed) + " movement speed</style> <style=cStack>(+" + Pct(stackSpeed) + " per stack)</style>, and <style=cIsDamage>+" + damage.ToString("N1") + " damage</style> for " + baseDuration.ToString("N0") + " <style=cStack>(+" + stackDuration.ToString("N0") + " per stack)</style>.",
            	"A relic of times long past (ClassicItems mod)");
        }

        public override void SetupBehaviorInner() {
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += On_SSOHOnTakeDamageServer;
        }

        private void On_SSOHOnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, RoR2.DamageReport damageReport) {
            orig(self, damageReport);
            int icnt = GetCount(damageReport.attackerMaster);
            if(icnt < 1) return;
        }
    }
}
