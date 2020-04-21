using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;
using R2API.Utils;
using BepInEx.Configuration;
using static ThinkInvisible.ClassicItems.ClassicItemsPlugin.MasterItemList;
using static ThinkInvisible.ClassicItems.MiscUtil;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ThinkInvisible.ClassicItems
{
    public class Embryo : ItemBoilerplate
    {
        public override string itemCodeName{get;} = "Embryo";

        private ConfigEntry<float> cfgProcChance;

        private Dictionary<EquipmentIndex,ConfigEntry<bool>> cfgSubEnable;

        private ConfigEntry<bool> cfgSubEnableModded;

        private ConfigEntry<bool> cfgSubEnableBrooch;

        public float procChance {get;private set;}

        public ReadOnlyDictionary<EquipmentIndex,bool> subEnable {get;private set;}

        public bool subEnableModded {get;private set;}
        public bool subEnableBrooch {get;private set;}


        protected override void SetupConfigInner(ConfigFile cfl) {
            cfgProcChance = cfl.Bind<float>(new ConfigDefinition("Items." + itemCodeName, "ProcChance"), 30f, new ConfigDescription(
                "Percent chance of triggering an equipment twice. Stacks additively. Must be in the range [0, 100].",
                new AcceptableValueRange<float>(0f,100f)));

            cfgSubEnable = new Dictionary<EquipmentIndex,ConfigEntry<bool>>();
            Dictionary<EquipmentIndex,bool> _subEnable = new Dictionary<EquipmentIndex, bool>();
            
            foreach(EquipmentIndex ind in (EquipmentIndex[]) Enum.GetValues(typeof(EquipmentIndex))) {
                if(ind == EquipmentIndex.AffixBlue || ind == EquipmentIndex.AffixGold || ind == EquipmentIndex.AffixHaunted || ind == EquipmentIndex.AffixPoison || ind == EquipmentIndex.AffixRed || ind == EquipmentIndex.AffixWhite || ind == EquipmentIndex.AffixYellow
                    || ind == EquipmentIndex.BurnNearby || ind == EquipmentIndex.CrippleWard || ind == EquipmentIndex.LunarPotion || ind == EquipmentIndex.SoulCorruptor || ind == EquipmentIndex.Tonic
                    || ind == EquipmentIndex.GhostGun || ind == EquipmentIndex.OrbitalLaser || ind == EquipmentIndex.SoulJar
                    || ind == EquipmentIndex.GoldGat || ind == EquipmentIndex.Gateway || ind == EquipmentIndex.Recycle || ind == EquipmentIndex.Scanner
                    || ind == EquipmentIndex.Count || ind == EquipmentIndex.Enigma || ind == EquipmentIndex.None || ind == EquipmentIndex.QuestVolatileBattery
                    #if !DEBUG
                    || ind == EquipmentIndex.Jetpack
                    #endif
                    )
                    continue;
                cfgSubEnable.Add(ind, cfl.Bind<bool>(new ConfigDefinition("Items." + itemCodeName, "SubEnable" + ind.ToString()), true, new ConfigDescription(
                "If false, Beating Embryo will not affect " + ind.ToString() + ".")));
                _subEnable.Add(ind, cfgSubEnable[ind].Value);
            }
            subEnable = new ReadOnlyDictionary<EquipmentIndex,bool>(_subEnable);

            cfgSubEnableModded = cfl.Bind<bool>(new ConfigDefinition("Items." + itemCodeName, "SubEnableModded"), false, new ConfigDescription(
                "If false, Beating Embryo will not affect equipment added by other mods. If true, these items will be triggered twice when Beating Embryo procs, which may not work with some items."));
            subEnableModded = cfgSubEnableModded.Value;

            cfgSubEnableBrooch = cfl.Bind<bool>(new ConfigDefinition("Items." + itemCodeName, "SubEnableSaw"), true, new ConfigDescription(
                "If false, Beating Embryo will not affect Captain's Brooch (added by CustomItems)."));
            subEnableBrooch = cfgSubEnableBrooch.Value;
        }
        
        protected override void SetupAttributesInner() {
            modelPathName = "embryocard.prefab";
            iconPathName = "embryo_icon.png";
            RegLang("Beating Embryo",
            	"Equipment has a 30% chance to deal double the effect.",
            	"Upon activating an equipment, adds a <style=cIsUtility>" + pct(cfgProcChance.Value, 0, 1) + "</style> <style=cStack>(+" + pct(cfgProcChance.Value, 0, 1) + " per stack)</style> chance to <style=cIsUtility>double its effects somehow</style>.",
            	"A relic of times long past (ClassicItems mod)");
            _itemTags = new[]{ItemTag.EquipmentRelated};
            itemTier = ItemTier.Tier3;
        }

        protected override void SetupBehaviorInner() {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += On_ESPerformEquipmentAction;

            IL.RoR2.EquipmentSlot.PerformEquipmentAction += IL_ESPerformEquipmentAction;
            IL.RoR2.EquipmentSlot.FixedUpdate += IL_ESFixedUpdate;
        }

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {            
            orig(self);

            var cpt = self.GetComponent<EmbryoComponent>();
            if(!cpt) {
                cpt = self.gameObject.AddComponent<EmbryoComponent>();
            }
        }

        private bool On_ESPerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot slot, EquipmentIndex ind) {
            var retv = orig(slot, ind);
            if(retv && slot.characterBody && Util.CheckRoll(GetCount(slot.characterBody)*cfgProcChance.Value)) {
                switch(ind) {
                    case EquipmentIndex.Fruit:
                        if(subEnable[EquipmentIndex.Fruit]) 
                            orig(slot, ind);
                        break;
                    case EquipmentIndex.Lightning:
                        if(subEnable[EquipmentIndex.Lightning]) 
                            orig(slot, ind);
                        break;
                    case EquipmentIndex.DroneBackup:
                        if(subEnable[EquipmentIndex.DroneBackup]) 
                            orig(slot, ind);
                        break;
                    case EquipmentIndex.PassiveHealing:
                        if(subEnable[EquipmentIndex.PassiveHealing]) 
                            orig(slot, ind);
                        break;
                    case EquipmentIndex.Saw:
                        if(subEnable[EquipmentIndex.Saw])
                            orig(slot, ind);
                        break;
                    case EquipmentIndex.BFG:
                    case EquipmentIndex.Blackhole:
                    case EquipmentIndex.BurnNearby:
                    case EquipmentIndex.Cleanse:
                    case EquipmentIndex.CommandMissile:
                    case EquipmentIndex.CrippleWard:
                    case EquipmentIndex.CritOnUse:
                    case EquipmentIndex.Enigma:
                    case EquipmentIndex.FireBallDash:
                    case EquipmentIndex.GainArmor:
                    case EquipmentIndex.Gateway:
                    case EquipmentIndex.GhostGun:
                    case EquipmentIndex.GoldGat:
                    case EquipmentIndex.Jetpack:
                    case EquipmentIndex.LunarPotion:
                    case EquipmentIndex.Meteor:
                    case EquipmentIndex.None:
                    case EquipmentIndex.OrbitalLaser:
                    case EquipmentIndex.QuestVolatileBattery:
                    case EquipmentIndex.Recycle:
                    case EquipmentIndex.Scanner:
                    case EquipmentIndex.SoulCorruptor:
                    case EquipmentIndex.SoulJar:
                    case EquipmentIndex.Tonic:
                        break;
                    default:
                        if(subEnableModded)
                            orig(slot, ind);
                        break;
                }
                if(subEnableBrooch && brooch.itemEnabled && ind == brooch.regIndexEqp) orig(slot, ind);
            }
            return retv;
        }

        private void IL_ESPerformEquipmentAction(ILContext il) {            
            ILCursor c = new ILCursor(il);

            //for some reason, this breaks the entire IL patch despite not throwing an error
            /*c.GotoNext(x=>x.MatchRet(),
                x=>x.MatchLdarg(1));
            c.Index++;*/

            //Insert a check for Embryo procs at the top of the function
            bool boost = false;
            EmbryoComponent cpt = null;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot)=>{
                boost = Util.CheckRoll(GetCount(slot.characterBody)*cfgProcChance.Value);
                cpt = slot.characterBody?.GetComponent<EmbryoComponent>();
            });
            
            bool ILFound;

            ILLabel[] swarr = new ILLabel[]{};
            //Load switch case locations
            ILFound = c.TryGotoNext(
                x=>x.MatchSwitch(out swarr));
            if(!ILFound) {
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch (ALL EQUIPMENTS): couldn't find switch!");
                return;
            }

            //CommandMissile: double number of missiles fired in the same timespan
            if((int)EquipmentIndex.CommandMissile >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: CommandMissile (PerformEquipmentAction); not in switch");
            else if(subEnable[EquipmentIndex.CommandMissile]) {
                //Find: default missile increment (+= (int)12)
                c.GotoLabel(swarr[(int)EquipmentIndex.CommandMissile]);
                ILFound = c.TryGotoNext(
                    x=>x.MatchLdfld<EquipmentSlot>("remainingMissiles"),
                    x=>x.OpCode == OpCodes.Ldc_I4_S,
                    x=>x.MatchAdd());

                if(ILFound) {
                    c.Index+=2;
                    //Replace original increment number with a custom function to check for Embryo proc
                    //If proc happens, doubles number of missiles added and marks the total number of missiles added as boosted; otherwise returns original
                    c.EmitDelegate<Func<int,int>>((int origMissiles)=>{
                        if(boost && cpt) cpt.boostedMissiles += origMissiles*2;
                        return (sbyte)(boost ? origMissiles*2 : origMissiles);
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: CommandMissile (PerformEquipmentAction); target instructions not found");
                }
            }

            //Blackhole: double yoink radius
            if((int)EquipmentIndex.CommandMissile >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: Blackhole; not in switch");
            else if(subEnable[EquipmentIndex.Blackhole]) {
                //Find: string "Prefabs/Projectiles/GravSphere", ldloc 15 (Vector3 position)
                c.GotoLabel(swarr[(int)EquipmentIndex.Blackhole]);
                ILFound = c.TryGotoNext(MoveType.After,
                    x=>x.MatchLdstr("Prefabs/Projectiles/GravSphere"),
                    x=>x.MatchCallOrCallvirt<Resources>("Load"));

                if(ILFound) {
                    //Insert a custom function to check for Embryo proc (captures GravSphere projectile prefab)
                    //If proc happens, radius of prefab's RadialForce component (GravSphere pull range) is doubled
                    c.EmitDelegate<Func<GameObject,GameObject>>((obj)=>{
                        var newobj = UnityEngine.Object.Instantiate(obj);
                        if(boost) newobj.GetComponent<RadialForce>().radius *= 2;
                        return newobj;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: Blackhole; target instructions not found");
                }
            }

            //CritOnUse: double duration
            if((int)EquipmentIndex.CritOnUse >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: CritOnUse; not in switch");
            else if(subEnable[EquipmentIndex.CritOnUse]) {
                //Find: AddTimedBuff(BuffIndex.FullCrit, 8f)
                c.GotoLabel(swarr[(int)EquipmentIndex.CritOnUse]);
                ILFound = c.TryGotoNext(
                    x=>x.MatchLdcI4((int)BuffIndex.FullCrit),
                    x=>x.OpCode == OpCodes.Ldc_R4,
                    x=>x.MatchCallOrCallvirt<CharacterBody>("AddTimedBuff"));
                    
                if(ILFound) {
                    //Advance cursor to the found ldcR4 (time argument of AddTimedBuff)
                    c.Index+=2;
                    //Replace original buff time with a custom function to check for Embryo proc
                    //If proc happens, doubles the buff time; otherwise returns original
                    c.EmitDelegate<Func<float,float>>((origBuffTime) => {
                        return boost ? origBuffTime*2 : origBuffTime;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: CritOnUse; target instructions not found");
                }
            }

            //BFG: double impact damage
            if((int)EquipmentIndex.BFG >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: BFG; not in switch");
            else if(subEnable[EquipmentIndex.BFG]) {
                //Find: loading of (int)2 into EquipmentSlot.bfgChargeTimer
                c.GotoLabel(swarr[(int)EquipmentIndex.BFG]);
                ILFound = c.TryGotoNext(MoveType.After,
                    x=>x.OpCode == OpCodes.Ldc_R4,
                    x=>x.MatchStfld<EquipmentSlot>("bfgChargeTimer"));

                if(ILFound) {
                    //Insert a custom function to check boost
                    //If proc happens, increments the player's boosted BFG shot counter; this will be spent once the BFG actually fires
                    c.EmitDelegate<Action>(()=>{
                        if(boost && cpt) cpt.boostedBFGs++;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: BFG (PerformEquipmentAction); target instructions not found");
                }
            }

            //Jetpack: double duration
            #if DEBUG
            if(subEnable[EquipmentIndex.Jetpack]) {
                //Find: JetpackController.ResetTimer (incl. retrieval of JetpackController)
                ILFound = c.TryGotoNext(
                    x=>x.OpCode == OpCodes.Ldloc_S,
                    x=>x.MatchCallvirt(typeof(JetpackController),"ResetTimer"));

                if(ILFound) {
                    VariableDefinition jpcVar = (VariableDefinition)c.Next.Operand;
                    //Advance cursor to just after the found callvirt
                    c.Index+=2;
                        
                    //Insert a custom function to check boost (needs insertion of local var containing the JetpackController)
                    //If proc happens, sets the JetpackController's stopwatch to negative duration (effectively 2x duration)
                    c.Emit(OpCodes.Ldloc, jpcVar.Index);
                    c.EmitDelegate<Action<JetpackController>>((jpc)=>{
                        if(boost) {
                            jpc.SetFieldValue("stopwatch", -jpc.duration); //this doesn't work... may have something to do with RPC
                        }
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: Jetpack");
                }
            }
            #endif

            //FireBallDash: double speed and damage
            if((int)EquipmentIndex.FireBallDash >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: FireBallDash; not in switch");
            else if(subEnable[EquipmentIndex.FireBallDash]) {
                //Find: string "Prefabs/NetworkedObjects/FireballVehicle"
                //Then find: instantiation of the prefab
                c.GotoLabel(swarr[(int)EquipmentIndex.FireBallDash]);
                ILFound = c.TryGotoNext(
                    x=>x.MatchLdstr("Prefabs/NetworkedObjects/FireballVehicle"))
                && c.TryGotoNext(
                    x=>x.MatchCallOrCallvirt<UnityEngine.Object>("Instantiate"));

                if(ILFound) {
                    c.Index++;

                    //Insert a custom function to check boost (captures a dup of the instantiated FireballVehicle)
                    //If proc happens, doubles the instanced vehicle's target speed, acceleration, and blast and collision damage
                    c.Emit(OpCodes.Dup);
                    c.EmitDelegate<Action<GameObject>>((go)=>{
                        if(boost) {
                            go.GetComponent<FireballVehicle>().targetSpeed *= 2f;
                            go.GetComponent<FireballVehicle>().acceleration *= 2f;
                            go.GetComponent<FireballVehicle>().blastDamageCoefficient *= 2f;
                            go.GetComponent<FireballVehicle>().overlapDamageCoefficient *= 2f;
                        }
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: FireBallDash; target instructions not found");
                }
            }

            //GainArmor: double duration
            if((int)EquipmentIndex.GainArmor >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: GainArmor; not in switch");
            else if(subEnable[EquipmentIndex.GainArmor]) {
                //Find: AddTimedBuff(BuffIndex.ElephantArmorBoost, 5f)
                c.GotoLabel(swarr[(int)EquipmentIndex.GainArmor]);
                ILFound = c.TryGotoNext(
                    x=>x.MatchLdcI4((int)BuffIndex.ElephantArmorBoost),
                    x=>x.OpCode == OpCodes.Ldc_R4,
                    x=>x.MatchCallvirt<CharacterBody>("AddTimedBuff"));

                if(ILFound) {
                    //Advance cursor to the found ldcR4 (time argument of AddTimedBuff)
                    c.Index+=2;

                    //Replace original buff time (5f) with a custom function to check for Embryo proc
                    //If proc happens, doubles the buff time to 10f; otherwise returns original
                    c.EmitDelegate<Func<float,float>>((origBuffTime)=>{
                        return boost?2*origBuffTime:origBuffTime;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: GainArmor; target instructions not found");
                }
            }

            //Cleanse: double projectile delete radius
            if((int)EquipmentIndex.Cleanse >= swarr.Length)
                Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: Cleanse; not in switch");
            else if(subEnable[EquipmentIndex.Cleanse]) {
                //Find: num3 = 6f; num4 = num3 * num3;
                float origRadius = 6f;
                c.GotoLabel(swarr[(int)EquipmentIndex.Cleanse]);
                ILFound = c.TryGotoNext(
                    x=>x.MatchCallOrCallvirt<SetStateOnHurt>("Cleanse"),
                    x=>x.MatchLdcR4(out origRadius),
                    x=>x.MatchDup(),
                    x=>x.MatchMul(),
                    x=>x.OpCode == OpCodes.Stloc_S);

                if(ILFound) {
                    c.Index+=2;
                    c.EmitDelegate<Func<float,float>>((ofl)=>{
                        return boost?2*ofl:ofl;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: Cleanse; target instructions not found");
                }
            }
        }

        private void IL_ESFixedUpdate(ILContext il) {
            ILCursor c = new ILCursor(il);

            EmbryoComponent cpt = null;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot)=>{
                cpt = slot.characterBody?.GetComponent<EmbryoComponent>();
            });
                
            bool ILFound;

            if(subEnable[EquipmentIndex.CommandMissile]) {
                //Find: loading of 0.125f into EquipmentSlot.missileTimer
                ILFound = c.TryGotoNext(
                    x=>x.OpCode == OpCodes.Ldc_R4,
                    x=>x.MatchStfld<EquipmentSlot>("missileTimer"));

                if(ILFound) {
                    //Replace original missile cooldown (0.125f) with a custom function to check for Embryo-boosted missiles
                    //If boosts exist, halves the missile cooldown to 0.0625f and deducts a boost from CPD; otherwise returns original
                    c.Index++;
                    c.EmitDelegate<Func<float,float>>((origCooldown) => {
                        if(cpt && cpt.boostedMissiles > 0) {
                            cpt.boostedMissiles --;
                            return origCooldown/2;
                        }
                        return origCooldown;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: CommandMissile (FixedUpdate)");
                }
            }

            if(subEnable[EquipmentIndex.BFG]) {
                //Find: string "Prefabs/Projectiles/BeamSphere"
                //Then find: CharacterBody.get_damage * 2f (damage argument of FireProjectile)
                ILFound = c.TryGotoNext(
                    x=>x.MatchLdstr("Prefabs/Projectiles/BeamSphere"))
                && c.TryGotoNext(
                    x=>x.MatchCallvirt<CharacterBody>("get_damage"),
                    x=>x.OpCode == OpCodes.Ldc_R4,
                    x=>x.MatchMul());

                if(ILFound) {
                    //Advance cursor to found ldcR4
                    c.Index+=2;
                        
                    //Replace original FireProjectile damage coefficient (2f) with a custom function to check for Embryo-boosted BFG shots
                    //If boosts exist, doubles the damage coefficient to 4f and deducts a boost from CPD; otherwise returns original
                    c.EmitDelegate<Func<float,float>>((origDamage)=>{
                        if(cpt && cpt.boostedBFGs > 0) {
                            cpt.boostedBFGs --;
                            return origDamage*2f;
                        }
                        return origDamage;
                    });
                } else {
                    Debug.LogError("ClassicItems: failed to apply Beating Embryo IL patch: BFG (FixedUpdate)");
                }
            }
        }
    }
    
    public class EmbryoComponent : MonoBehaviour
    {
        public float boostedMissiles = 0f;
        public float boostedBFGs = 0f;
    }
}
