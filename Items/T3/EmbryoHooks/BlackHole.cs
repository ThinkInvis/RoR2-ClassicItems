using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems.EmbryoHooks {
    public class BlackHole : Embryo.EmbryoHook {
        public override EquipmentDef targetEquipment => RoR2Content.Equipment.Blackhole;

        protected override void InstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole += IL_ESFireBlackhole;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole -= IL_ESFireBlackhole;
        }

        private void IL_ESFireBlackhole(MonoMod.Cil.ILContext il) {
            ILCursor c = new ILCursor(il);
            bool boost = false;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.instance.CheckEmbryoProc(slot.characterBody);
            });

            bool ILFound;

            ILLabel[] swarr = new ILLabel[] { };
            //Load switch case locations
            ILFound = c.TryGotoNext(
                x => x.MatchSwitch(out swarr));
            if(!ILFound) {
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch (Blackhole): couldn't find switch!");
                return;
            }

            if((int)RoR2Content.Equipment.Blackhole.equipmentIndex >= swarr.Length)
                ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Blackhole; not in switch");
            else if(subEnable[RoR2Content.Equipment.Blackhole]) {
                //Find: string "Prefabs/Projectiles/GravSphere", ldloc 15 (Vector3 position)
                c.GotoLabel(swarr[(int)RoR2Content.Equipment.Blackhole]);
                ILFound = c.TryGotoNext(MoveType.After,
                    x => x.MatchLdstr("Prefabs/Projectiles/GravSphere"),
                    x => x.MatchCallOrCallvirt<Resources>("Load"));

                if(ILFound) {
                    //Insert a custom function to check for Embryo proc (captures GravSphere projectile prefab)
                    //If proc happens, radius of prefab's RadialForce component (GravSphere pull range) is doubled
                    c.EmitDelegate<Func<GameObject, GameObject>>((obj) => {
                        var newobj = UnityEngine.Object.Instantiate(obj);
                        if(boost) newobj.GetComponent<RadialForce>().radius *= 2;
                        return newobj;
                    });
                } else {
                    ClassicItemsPlugin._logger.LogError("Failed to apply Beating Embryo IL patch: Blackhole; target instructions not found");
                }
            }
        }
    }
}
