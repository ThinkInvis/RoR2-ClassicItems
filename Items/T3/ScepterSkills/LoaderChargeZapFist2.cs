using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using RoR2;
using R2API;
using MonoMod.Cil;
using RoR2.Projectile;
using Mono.Cecil.Cil;
using System;
using EntityStates.Loader;

namespace ThinkInvisible.ClassicItems {
    public class LoaderChargeZapFist2 : ScepterSkill {
        private static GameObject projReplacer;
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Triple omnidirectional lightning bolts.</color>";
        
        public override string targetBody => "LoaderBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/loaderbody/ChargeZapFist");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPLOADER_CHARGEZAPFISTNAME";
            newDescToken = "CLASSICITEMS_SCEPLOADER_CHARGEZAPFISTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Thundercrash";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/loader_chargezapfisticon.png");

            LoadoutAPI.AddSkillDef(myDef);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/LoaderZapCone").InstantiateClone("CIScepLoaderThundercrash");
            var proxb = projReplacer.GetComponent<ProjectileProximityBeamController>();
            proxb.attackFireCount *= 3;
            proxb.maxAngleFilter = 180f;
            projReplacer.transform.Find("Effect").localScale *= 3f;

            ProjectileCatalog.getAdditionalEntries += (list) => list.Add(projReplacer);
        }

        internal override void LoadBehavior() {
            IL.EntityStates.Loader.SwingZapFist.OnMeleeHitAuthority += IL_SwingZapFistMeleeHit;
            On.EntityStates.Loader.BaseChargeFist.OnEnter += On_BaseChargeFistEnter;
        }

        internal override void UnloadBehavior() {
            IL.EntityStates.Loader.SwingZapFist.OnMeleeHitAuthority -= IL_SwingZapFistMeleeHit;
        }
        
        private void On_BaseChargeFistEnter(On.EntityStates.Loader.BaseChargeFist.orig_OnEnter orig, BaseChargeFist self) {
            orig(self);
            if(!(self is ChargeZapFist) || Scepter.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;
			var mTsf = self.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(BaseChargeFist.chargeVfxChildLocatorName);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/MageLightningBombExplosion"),
                new EffectData {
                    origin = mTsf?.position ?? self.outer.commonComponents.transform.position,
                    scale = 3f
                }, true);
        }

        private void IL_SwingZapFistMeleeHit(ILContext il) {
            ILCursor c = new ILCursor(il);
            bool ilFound = c.TryGotoNext(
                x => x.MatchStfld<FireProjectileInfo>(nameof(FireProjectileInfo.projectilePrefab)));
            if(ilFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<GameObject, SwingZapFist, GameObject>>((origProj, state) => {
                    if(Scepter.instance.GetCount(state.outer.commonComponents.characterBody) < 1) return origProj;
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
