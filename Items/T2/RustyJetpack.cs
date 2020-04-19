﻿using R2API.Utils;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using static ThinkInvisible.ClassicItems.ClassicItemsPlugin.MasterItemList;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ThinkInvisible.ClassicItems
{
    public class RustyJetpack : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "RustyJetpack";

        private ConfigEntry<float> cfgGravMod;
        private ConfigEntry<float> cfgJumpMult;
        private ConfigEntry<bool> cfgUseIL;

        public float gravMod {get;private set;}
        public float jumpMult {get;private set;}
        public bool useIL {get;private set;}

        private bool ilFailed = false;

        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgGravMod = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "GravMod"), 0.5f, new ConfigDescription(
                "Multiplier for gravity reduction (0.0 = no effect, 1.0 = full anti-grav).",
                new AcceptableValueRange<float>(0f,0.99f)));
            cfgJumpMult = cfl.Bind<float>(new ConfigDefinition("Items." + itemCodeName, "JumpMult"), 0.1f, new ConfigDescription(
                "Amount added to jump power per stack.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgUseIL = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "UseIL"), true, new ConfigDescription(
                "Set to false to change Rusty Jetpack's effect from an IL patch to an event hook, which may help if experiencing compatibility issues with another mod. This will change how Rusty Jetpack interacts with other effects."));

            gravMod = cfgGravMod.Value;
            jumpMult = cfgJumpMult.Value;
            useIL = cfgUseIL.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "rustyjetpackcard.prefab";
            iconPathName = "rustyjetpack_icon.png";
            itemName = "Rusty Jetpack";
            itemShortText = "Increase jump height and reduce gravity.";
            itemLongText = "<style=cIsUtility>Reduces gravity</style> by <style=cIsUtility>" + pct(gravMod) + "</style> while <style=cIsUtility>holding jump</style>. Increases <style=cIsUtility>jump power</style> by <style=cIsUtility>" + pct(jumpMult) + "</style> <style=cStack>(+" + pct(jumpMult)  + " per stack, linear)</style>.";
            itemLoreText = "A relic of times long past (ClassicItems mod)";
            _itemTags = new[]{ItemTag.Utility};
            itemTier = ItemTier.Tier2;
        }

        protected override void SetupBehaviorInner() {
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

        private void On_CBFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self) {
            if(!self.characterMotor) {orig(self);return;}
            if(GetCount(self) > 0 && self.inputBank.jump.down && (
                    !photonJetpack.itemEnabled
                    || !ClassicItemsPlugin.gCoolYourJets
                    || (self.GetComponent<PhotonJetpackComponent>()?.fuel ?? 0f) <= 0f
                ))
                self.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * gravMod;

            orig(self);
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
