using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Permafrost : Item_V2<Permafrost> {
        public override string displayName => "Permafrost";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        public override bool itemAIB {get; protected set;} = true;
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Percent chance of triggering Permafrost on hit. Affected by proc coefficient; stacks hyperbolically.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 6f;

        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Duration of freeze applied by Permafrost.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float freezeTime {get;private set;} = 1.5f;
        
        [AutoUpdateEventInfo_V2(AutoUpdateEventFlags_V2.InvalidateLanguage)]
        [AutoConfig("Duration of slow applied by Permafrost.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float slowTime {get;private set;} = 3.0f;

        [AutoConfig("If true, Permafrost will slow targets even if they can't be frozen.")]
        public bool slowUnfreezable {get;private set;} = true;
        
        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Chance to freeze enemies on hit.";
        protected override string NewLangDesc(string langid = null) => "<style=cIsUtility>" + Pct(procChance,1,1) + "</style> <style=cStack>(+" + Pct(procChance,1,1) + " per stack, hyperbolic)</style> chance to <style=cIsUtility>freeze and slow</style> an enemy (" + freezeTime.ToString("N1") + "s and " + slowTime.ToString("N1") + "s respectively). Affected by proc coefficient.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void SetupBehavior() {
            base.SetupBehavior();
            if(Compat_ItemStats.enabled) {
				Compat_ItemStats.CreateItemStatDef(regItem.ItemDef,
					((count,inv,master)=>{return Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance * count);},
					(value,inv,master)=>{return $"Freeze Chance: {Pct(value, 1, 1f)}";}));
			}
            if(Compat_BetterUI.enabled)
                Compat_BetterUI.AddEffect(regIndex, procChance, procChance, Compat_BetterUI.ChanceFormatter, Compat_BetterUI.HyperbolicStacking);
        }

        public override void Install() {
            base.Install();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer += On_SSOHOnTakeDamageServer;
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.SetStateOnHurt.OnTakeDamageServer -= On_SSOHOnTakeDamageServer;
        }

        private void On_SSOHOnTakeDamageServer(On.RoR2.SetStateOnHurt.orig_OnTakeDamageServer orig, SetStateOnHurt self, DamageReport damageReport) {
            orig(self, damageReport);
            int icnt = GetCount(damageReport.attackerMaster);
            if(icnt < 1) return;
            var ch = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance * icnt * damageReport.damageInfo.procCoefficient);
            if(!Util.CheckRoll(ch)) return;
            if(self.canBeFrozen) {
                self.SetFrozen(freezeTime);
                damageReport.victim?.body.AddTimedBuff(ClassicItemsPlugin.freezeBuff, freezeTime);
            }
            if((self.canBeFrozen || slowUnfreezable) && damageReport.victim) damageReport.victim.body.AddTimedBuff(BuffIndex.Slow60, slowTime);
        }
    }
}
