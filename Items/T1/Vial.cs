using R2API.Utils;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;
using System;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class Vial : Item<Vial> {
        public override string displayName => "Mysterious Vial";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Healing});
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Direct additive to natural health regen per stack of Mysterious Vial.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float addRegen {get;private set;} = 1.4f;

        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Increased health regeneration.";        
        protected override string NewLangDesc(string langid = null) => "Increases <style=cIsHealing>health regen by +" + addRegen.ToString("N1") + "/sec</style> <style=cStack>(+" + addRegen.ToString("N1") + "/sec per stack)</style>.";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Vial() {}

        protected override void LoadBehavior() {
            OnPreRecalcStats += Evt_TILER2OnPreRecalcStats;
        }
        protected override void UnloadBehavior() {
            OnPreRecalcStats -= Evt_TILER2OnPreRecalcStats;
        }

        private void Evt_TILER2OnPreRecalcStats(CharacterBody sender, StatHookEventArgs args) {
            args.regenMultAdd += GetCount(sender) * addRegen;
        }
    }
}
