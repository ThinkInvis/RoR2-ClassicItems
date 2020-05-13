using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using RoR2;
using R2API;
using MonoMod.Cil;
using RoR2.Projectile;
using Mono.Cecil.Cil;
using System;
using EntityStates.Loader;

namespace ThinkInvisible.ClassicItems {
    public static class LoaderChargeZapFist2 {
        private static GameObject projReplacer;
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/loaderbody/ChargeZapFist");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPLOADER_CHARGEZAPFISTNAME";
            var desctoken = "CLASSICITEMS_SCEPLOADER_CHARGEZAPFISTDESC";
            var namestr = "Thundercrash";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Triple radius, triple lightning bolts. AoE is omnidirectional.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/loader_chargezapfisticon.png");

            LoadoutAPI.AddSkillDef(myDef);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/LoaderZapCone").InstantiateClone("CIScepLoaderThundercrash");
            var proxb = projReplacer.GetComponent<ProjectileProximityBeamController>();
            proxb.attackFireCount *= 3;
            proxb.maxAngleFilter = 180f;
            proxb.attackRange *= 3f;
            projReplacer.transform.Find("Effect").localScale *= 3f;

            ProjectileCatalog.getAdditionalEntries += (list) => list.Add(projReplacer);
        }

        internal static void LoadBehavior() {
            IL.EntityStates.Loader.SwingZapFist.OnMeleeHitAuthority += IL_SwingZapFistMeleeHit;
            On.EntityStates.Loader.BaseChargeFist.OnEnter += On_BaseChargeFistEnter;
        }

        internal static void UnloadBehavior() {
            IL.EntityStates.Loader.SwingZapFist.OnMeleeHitAuthority -= IL_SwingZapFistMeleeHit;
        }
        
        private static void On_BaseChargeFistEnter(On.EntityStates.Loader.BaseChargeFist.orig_OnEnter orig, BaseChargeFist self) {
            orig(self);
            if(!(self is ChargeZapFist) || Scepter.instance.GetCount(self.outer.commonComponents.characterBody) == 0) return;
			var mTsf = self.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(BaseChargeFist.chargeVfxChildLocatorName);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/MageLightningBombExplosion"),
                new EffectData {
                    origin = mTsf?.position ?? self.outer.commonComponents.transform.position,
                    scale = 3f
                }, true);
        }

        private static void IL_SwingZapFistMeleeHit(ILContext il) {
            ILCursor c = new ILCursor(il);
            bool ilFound = c.TryGotoNext(
                x => x.MatchStfld<FireProjectileInfo>(nameof(FireProjectileInfo.projectilePrefab)));
            if(ilFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<GameObject, SwingZapFist, GameObject>>((origProj, state) => {
                    if(Scepter.instance.GetCount(state.outer.commonComponents.characterBody) == 0) return origProj;
			        var mTsf = state.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(state.swingEffectMuzzleString);
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/ImpactEffects/LightningStrikeImpact"),
                        new EffectData {
                            origin = mTsf?.position ?? state.outer.commonComponents.transform.position,
                            scale = 1f
                        }, true);
                    return projReplacer;
                });
            }
        }
    }
}
