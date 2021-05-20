﻿using Mono.Cecil.Cil;
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
            IL.RoR2.EquipmentSlot.FireBlackhole += EquipmentSlot_FireBlackhole;
        }

        protected override void UninstallHooks() {
            IL.RoR2.EquipmentSlot.FireBlackhole -= EquipmentSlot_FireBlackhole;
        }

        private void EquipmentSlot_FireBlackhole(MonoMod.Cil.ILContext il) {
            ILCursor c = new ILCursor(il);

            bool boost = Embryo.ILInjectProcCheck(c);

            bool ilFound = c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Prefabs/Projectiles/GravSphere"),
                x => x.MatchCallOrCallvirt<Resources>("Load"));

            if(ilFound) {
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
