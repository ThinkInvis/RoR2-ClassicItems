using R2API.Utils;
using RoR2;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class RustyJetpack : Item<RustyJetpack> {
        public override string displayName => "Rusty Jetpack";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Multiplier for gravity reduction (0.0 = no effect, 1.0 = full anti-grav).", AutoItemConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float gravMod {get;private set;} = 0.5f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken | AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Amount added to jump power per stack.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float jumpMult {get;private set;} = 0.1f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateStats)]
        [AutoItemConfig("Set to false to change Rusty Jetpack's effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Rusty Jetpack interacts with other effects.",
            AutoItemConfigFlags.PreventNetMismatch)]
        public bool useIL {get;private set;} = true;

        private bool ilFailed = false;
        protected override string NewLangName(string langid = null) => displayName;        
        protected override string NewLangPickup(string langid = null) => "Increase jump height and reduce gravity.";        
        protected override string NewLangDesc(string langid = null) => "<style=cIsUtility>Reduces gravity</style> by <style=cIsUtility>" + Pct(gravMod) + "</style> while <style=cIsUtility>holding jump</style>. Increases <style=cIsUtility>jump power</style> by <style=cIsUtility>" + Pct(jumpMult) + "</style> <style=cStack>(+" + Pct(jumpMult)  + " per stack, linear)</style>.";        
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public RustyJetpack() {}

        protected override void LoadBehavior() {
            On.RoR2.CharacterBody.FixedUpdate += On_CBFixedUpdate;

            if(useIL) {
                IL.RoR2.CharacterBody.RecalculateStats += IL_CBRecalcStats;
                if(ilFailed) {
                    IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
                    On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
                }
            } else
                On.RoR2.CharacterBody.RecalculateStats += On_CBRecalcStats;
        }

        protected override void UnloadBehavior() {
            On.RoR2.CharacterBody.FixedUpdate -= On_CBFixedUpdate;
            On.RoR2.CharacterBody.RecalculateStats -= On_CBRecalcStats;
            IL.RoR2.CharacterBody.RecalculateStats -= IL_CBRecalcStats;
        }

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            orig(self);
            if(!self.characterMotor) return;
            if(GetCount(self) > 0 && self.inputBank.jump.down && (
                    !PhotonJetpack.instance.enabled
                    || !ClassicItemsPlugin.gCoolYourJets
                    || (self.GetComponent<PhotonJetpackComponent>()?.fuel ?? 0f) <= 0f))
                self.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * gravMod;
        }

        private void On_CBRecalcStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self) {
            orig(self);

            float JumpIncrement = 1.0f + jumpMult * GetCount(self);
            Reflection.SetPropertyValue(self, "jumpPower", self.jumpPower * JumpIncrement);
            Reflection.SetPropertyValue(self, "maxJumpHeight", Trajectory.CalculateApex(self.jumpPower));
        }

        private void IL_CBRecalcStats(ILContext il) {
            var c = new ILCursor(il);

            //Add another local variable to store Rusty Jetpack itemcount
            c.IL.Body.Variables.Add(new VariableDefinition(c.IL.Body.Method.Module.TypeSystem.Int32));
            int locItemCount = c.IL.Body.Variables.Count-1;
            c.Emit(OpCodes.Ldc_I4_0);
            c.Emit(OpCodes.Stloc, locItemCount);

            bool ILFound;
                    
            ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchCallOrCallvirt<CharacterBody>("get_inventory"),
                x=>x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit"),
                x=>x.OpCode==OpCodes.Brfalse);
                    
            if(ILFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Call,typeof(CharacterBody).GetMethod("get_inventory"));
                c.Emit(OpCodes.Ldc_I4, (int)regIndex);
                c.Emit(OpCodes.Callvirt,typeof(Inventory).GetMethod("GetItemCount"));
                c.Emit(OpCodes.Stloc, locItemCount);
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Rusty Jetpack IL patch (inventory load), falling back to event hook");
                return;
            }

            //Find (parts of): float jumpPower = this.baseJumpPower + this.levelJumpPower * num32;
            ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("baseJumpPower"),
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<CharacterBody>("levelJumpPower"),
                x=>x.OpCode == OpCodes.Ldloc_S,
                x=>x.MatchMul(),
                x=>x.MatchAdd());

            if(ILFound) {
                c.Emit(OpCodes.Ldloc, locItemCount);
                c.EmitDelegate<Func<float,int,float>>((orig, icnt) => {
                    return orig * (1 + icnt * jumpMult);
                });
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Rusty Jetpack IL patch (jump power modifier), falling back to event hook");
                return;
            }
        }
    }
}
