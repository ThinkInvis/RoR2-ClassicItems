using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Taser : Item_V2<Taser> {
        public override string displayName => "Taser";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Percent chance for Taser to proc.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 7f;
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Duration of root applied by first Taser stack.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float procTime {get;private set;} = 1.5f;
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Duration of root applied per additional Taser stack.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float stackTime {get;private set;} = 0.5f;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Chance to snare on hit.";
        protected override string NewLangDesc(string langid = null) {
            string desc = "<style=cIsUtility>" + Pct(procChance, 0, 1) + "</style> chance to <style=cIsUtility>entangle</style> an enemy for <style=cIsUtility>" + procTime.ToString("N1") + " seconds</style>";
            if(stackTime > 0f) desc += $" <style=cStack>(+{stackTime:N1} per stack)</style>";
            desc += ".";
            return desc;
        }
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();

            if(Compat_ItemStats.enabled) {
                Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
                    ((count, inv, master) => { return procTime + (count - 1) * stackTime; },
                    (value, inv, master) => { return $"Duration: {value.ToString("N1")} s"; }
                ));
            }
            if(Compat_BetterUI.enabled)
                Compat_BetterUI.AddEffect(regIndex, procChance, null, Compat_BetterUI.ChanceFormatter, Compat_BetterUI.NoStacking);
        }

        public override void Install() {
            base.Install();
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
            orig(self,di);

            if(di == null || di.rejected || !di.attacker || di.attacker == self.gameObject) return;

            var cb = di.attacker.GetComponent<CharacterBody>();
            if(cb) {
                var icnt = GetCount(cb);
                if(icnt < 1) return;
                var proc = cb.master ? Util.CheckRoll(procChance,cb.master) : Util.CheckRoll(procChance);
                if(proc) {
                    self.body.AddTimedBuff(BuffIndex.Entangle, procTime + (icnt-1) * stackTime);
                }
            }
        }
    }
}
