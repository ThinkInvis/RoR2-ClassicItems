using R2API.Utils;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.MiscUtil;
using static ThinkInvisible.ClassicItems.ClassicItemsPlugin.MasterItemList;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace ThinkInvisible.ClassicItems
{
    public class GoldenGun : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "GoldenGun";

        private ConfigEntry<float> cfgDamageBoost;
        private ConfigEntry<int> cfgGoldAmt;
        private ConfigEntry<float> cfgGoldReduc;

        public float damageBoost {get;private set;}
        public int goldAmt {get;private set;}
        public float goldReduc {get;private set;}

        private bool ilFailed = false;

        protected override void SetupConfigInner(ConfigFile cfl) {
            itemAIBDefault = true;

            cfgDamageBoost = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "DamageBoost"), 0.4f, new ConfigDescription(
                "Maximum multiplier to add to player damage.",
                new AcceptableValueRange<float>(0f,float.MaxValue)));
            cfgGoldAmt = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "GoldAmt"), 700, new ConfigDescription(
                "Gold required for maximum damage. Scales with difficulty level.",
                new AcceptableValueRange<int>(0,int.MaxValue)));
            cfgGoldReduc = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "GoldReduc"), 0.5f, new ConfigDescription(
                "Inverse-exponential multiplier for reduced GoldAmt per stack.",
                new AcceptableValueRange<float>(0f,0.999f)));

            damageBoost = cfgDamageBoost.Value;
            goldAmt = cfgGoldAmt.Value;
            goldReduc = cfgGoldReduc.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "goldenguncard.prefab";
            iconPathName = "goldengun_icon.png";
            RegLang("Golden Gun",
            	"More gold, more damage.",
            	"Deal <style=cIsDamage>bonus damage</style> based on your <style=cIsUtility>money</style>, up to <style=cIsDamage>40% damage</style> at <style=cIsUtility>$700</style> <style=cStack>(cost increases with difficulty, -50% per stack)</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new List<ItemTag>{ItemTag.Utility};
            itemTier = ItemTier.Tier2;
        }

        protected override void SetupBehaviorInner() {
            IL.RoR2.HealthComponent.TakeDamage += IL_CBTakeDamage;
            if(ilFailed) IL.RoR2.HealthComponent.TakeDamage -= IL_CBTakeDamage;
        }

        private void IL_CBTakeDamage(ILContext il) {
            var c = new ILCursor(il);

            bool ILFound;

            int locDmg = -1;
            ILFound = c.TryGotoNext(
                x=>x.MatchLdarg(1),
                x=>x.MatchLdfld<DamageInfo>("damage"),
                x=>x.MatchStloc(out locDmg));
            
            if(!ILFound) {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Golden Gun IL patch (damage var read), item will not work; target instructions not found");
                return;
            }

            int locChrm = -1;
            ILFound = c.TryGotoNext(
                x=>x.MatchLdloc(out locChrm),
                x=>x.MatchCallOrCallvirt<CharacterMaster>("get_inventory"),
                x=>x.MatchLdcI4((int)ItemIndex.Crowbar),
                x=>x.MatchCallOrCallvirt<Inventory>("GetItemCount"),
                x=>x.OpCode == OpCodes.Stloc_S)
            && c.TryGotoPrev(MoveType.After,
                x=>x.OpCode == OpCodes.Brfalse);

            if(ILFound) {
                c.Emit(OpCodes.Ldloc, locChrm);
                c.Emit(OpCodes.Ldloc, locDmg);
                c.EmitDelegate<Func<CharacterMaster,float,float>>((chrm, origdmg) => {
                    var icnt = GetCount(chrm.inventory);
                    var moneyCoef = chrm.money / (Run.instance.GetDifficultyScaledCost(goldAmt) * Mathf.Pow(goldReduc, icnt - 1));
                    if(icnt == 0) return origdmg;
                    return origdmg * (1 + Mathf.Lerp(0,damageBoost,moneyCoef));
                });
                c.Emit(OpCodes.Stloc, locDmg);
            } else {
                ilFailed = true;
                Debug.LogError("ClassicItems: failed to apply Golden Gun IL patch (damage var write), item will not work; target instructions not found");
                return;
            }
        }
    }
}
