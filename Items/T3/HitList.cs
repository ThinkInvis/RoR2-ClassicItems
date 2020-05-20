using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using RoR2.Orbs;
using TILER2;
using static TILER2.MiscUtil;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.ClassicItems {
    public class HitList : Item<HitList> {
        public override string displayName => "The Hit List";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Damage});
        public override bool itemAIB {get; protected set;} = true;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Time per batch of marked enemies.",AutoItemConfigFlags.None,0f,float.MaxValue)]
        public float cooldown {get;private set;} = 10f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Additive bonus to base damage per marked enemy killed.",AutoItemConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float procDamage {get;private set;} = 0.5f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Maximum damage bonus from The Hit List.",AutoItemConfigFlags.PreventNetMismatch,0f,float.MaxValue)]
        public float maxDamage {get;private set;} = 20f;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Killing marked enemies permanently increases damage.";
        protected override string NewLangDesc(string langid = null) => "Every <style=cIsUtility>" + cooldown.ToString("N0") + " seconds</style>, <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> random enemy will be <style=cIsUtility>marked</style> for the same duration. Killing <style=cIsUtility>marked</style> enemies gives you <style=cIsDamage>+" + procDamage.ToString("N1") + " permanent base damage</style> <style=cStack>(max. " + maxDamage.ToString("N1") + ")</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public ItemIndex hitListTally {get; private set;}
        public BuffIndex markDebuff {get; private set;}

        public HitList() {
            onAttrib += (tokenIdent, namePrefix) => {
                var markDebuffDef = new CustomBuff(new BuffDef {
                    buffColor = new Color(0.85f, 0.8f, 0.3f),
                    canStack = true,
                    isDebuff = false,
                    name = "CIHitListDebuff",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/hitlist_debuff_icon.png"
                });
                markDebuff = BuffAPI.Add(markDebuffDef);

                var hitListTallyDef = new CustomItem(new ItemDef {
                    hidden = true,
                    name = "CIINTERNALTally",
                    tier = ItemTier.NoTier,
                    canRemove = false
                }, new ItemDisplayRuleDict(null));
                hitListTally = ItemAPI.Add(hitListTallyDef);
            };
        }

        protected override void LoadBehavior() {
            GlobalEventManager.onCharacterDeathGlobal += Evt_GEMOnCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate += On_RunFixedUpdate;
            IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
        }

        protected override void UnloadBehavior() {
            GlobalEventManager.onCharacterDeathGlobal -= Evt_GEMOnCharacterDeathGlobal;
            On.RoR2.Run.FixedUpdate -= On_RunFixedUpdate;
            IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
        }

        private float stopwatch = 0f;
        private void On_RunFixedUpdate(On.RoR2.Run.orig_FixedUpdate orig, Run self) {
            orig(self);
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch > 0f) return;

            stopwatch = cooldown;
            var alive = AliveList();
            int totalCount = 0;
            foreach(var cm in alive) {
                totalCount += GetCount(cm);
            }
            if(totalCount <= 0) return;
            for(var i = 0; i < totalCount; i++) {
                if(alive.Count <= 0) break;
                var next = itemRng.NextElementUniform(alive);
                if(next.hasBody)
                    next.GetBody().AddTimedBuff(markDebuff, cooldown);
                else
                    i--;
                alive.Remove(next);
            }
        }
        
        private void Evt_GEMOnCharacterDeathGlobal(DamageReport rep) {
            if((rep.victimBody?.HasBuff(markDebuff) ?? false) && GetCount(rep.attackerBody) > 0)
                rep.attackerBody.inventory.GiveItem(hitListTally);
        }

        private void IL_CBRecalcStats(ILContext il) {
            var c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("baseDamage"));

            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float,CharacterBody,float>>((baseDamage, cb) => {
                    var ret = baseDamage;
                    ret += Math.Min(procDamage * (cb.inventory?.GetItemCount(hitListTally) ?? 0), maxDamage);
                    return ret;
                });
            } else {
                Debug.LogError("ClassicItems: failed to apply The Hit List IL patch (base damage modifier)");
            }
        }
    }
}
