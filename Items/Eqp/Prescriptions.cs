using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Prescriptions : Equipment<Prescriptions> {
        public override string displayName => "Prescriptions";

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidatePickupToken)]
        [AutoItemConfig("Duration of the buff applied by Prescriptions.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float duration {get;private set;} = 11f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Attack speed added while Prescriptions is active.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float aSpdBoost {get;private set;} = 0.4f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Base damage added while Prescriptions is active.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float dmgBoost {get;private set;} = 10f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Set to false to change Prescriptions' effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Prescriptions interacts with other effects.",
            AutoItemConfigFlags.PreventNetMismatch)]
        public bool useIL {get; private set;}

        private bool ilFailed = false;
        public BuffIndex prescriptionsBuff {get;private set;}        
        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Increase damage and attack speed for " + duration.ToString("N0") + " seconds.";        
        protected override string NewLangDesc(string langid = null) => "While active, increases <style=cIsDamage>base damage by " + dmgBoost.ToString("N0") + " points</style> and <style=cIsDamage>attack speed by " + Pct(aSpdBoost) + "</style>. Lasts <style=cIsDamage>" + duration.ToString("N0") + " seconds</style>.";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public Prescriptions() {
            onAttrib += (tokenIdent, namePrefix) => {
                
                var prescriptionsBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = Color.red,
                    canStack = true,
                    isDebuff = false,
                    name = namePrefix + "Prescriptions",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/Prescriptions_icon.png"
                });
                prescriptionsBuff = R2API.BuffAPI.Add(prescriptionsBuffDef);
            };
        }

        protected override void LoadBehavior() {
            if(useIL) {
                ilFailed = false;
                IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
                if(ilFailed) {
                    IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
                    On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
                }
            } else
                On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
        }
        protected override void UnloadBehavior() {
            IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
            On.RoR2.CharacterBody.RecalculateStats -= On_CBRecalcStats;
        }
        
        protected override bool OnEquipUseInner(EquipmentSlot slot) {
            var sbdy = slot.characterBody;
            if(!sbdy) return false;
            sbdy.ClearTimedBuffs(prescriptionsBuff);
            sbdy.AddTimedBuff(prescriptionsBuff, duration);
            if(instance.CheckEmbryoProc(sbdy)) sbdy.AddTimedBuff(prescriptionsBuff, duration);
            return true;
        }
        private void IL_CBRecalcStats(ILContext il) {
            var c = new ILCursor(il);

            bool ILFound;

            ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("baseDamage"),
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("levelDamage"),
                x=>x.MatchLdloc(out _),
                x=>x.MatchMul(),
                x=>x.MatchAdd());
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float,CharacterBody,float>>((origDmg, cb) => {
                    return origDmg + (cb.HasBuff(prescriptionsBuff) ? dmgBoost : 0f);
                });
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Prescriptions IL patch (damage modifier), falling back to event hook");
                return;
            }

            ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("baseAttackSpeed"),
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("levelAttackSpeed"),
                x=>x.MatchLdloc(out _),
                x=>x.MatchMul(),
                x=>x.MatchAdd());
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float,CharacterBody,float>>((origASpd, cb) => {
                    return origASpd * (1f + cb.GetBuffCount(prescriptionsBuff) * aSpdBoost);
                });
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Prescriptions IL patch (attack speed modifier), falling back to event hook");
                return;
            }
        }
        private void On_CBRecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);
            
            if(self.GetBuffCount(prescriptionsBuff) == 0) return;
            Reflection.SetPropertyValue(self, "damage", self.damage + dmgBoost);
            Reflection.SetPropertyValue(self, "attackSpeed", self.attackSpeed + aSpdBoost * self.GetBuffCount(prescriptionsBuff));
        }
    }
}