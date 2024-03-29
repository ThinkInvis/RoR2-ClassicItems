﻿using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using RoR2;
using R2API;
using MonoMod.Cil;
using System;
using RoR2.Projectile;
using Mono.Cecil.Cil;

namespace ThinkInvisible.ClassicItems {
    public class ArtificerFlamethrower2 : ScepterSkill {
        private GameObject projCloud;
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hits leave behind a lingering fire cloud.</color>";

        public override string targetBody => "MageBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override SkillDef targetVariantDef => LegacyResourcesAPI.Load<SkillDef>("skilldefs/magebody/MageBodyFlamethrower");

        internal override void SetupAttributes() {
            myDef = CloneSkillDef(targetVariantDef);

            var nametoken = "CLASSICITEMS_SCEPMAGE_FLAMETHROWERNAME";
            newDescToken = "CLASSICITEMS_SCEPMAGE_FLAMETHROWERDESC";
            oldDescToken = targetVariantDef.skillDescriptionToken;
            var namestr = "Dragon's Breath";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/mage_flamethrowericon.png");

            ContentAddition.AddSkillDef(myDef);

            projCloud = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/BeetleQueenAcid").InstantiateClone("CIScepMageFlamethrowerCloud", true);
            var pdz = projCloud.GetComponent<ProjectileDotZone>();
            pdz.lifetime = 10f;
            pdz.impactEffect = null;
            pdz.fireFrequency = 2f;
            var fxObj = projCloud.transform.Find("FX");
            fxObj.Find("Spittle").gameObject.SetActive(false);
            fxObj.Find("Decal").gameObject.SetActive(false);
            fxObj.Find("Gas").gameObject.SetActive(false);
            foreach(var x in fxObj.GetComponents<AnimateShaderAlpha>()) {x.enabled = false;}
            var fxcloud = UnityEngine.Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("prefabs/FireTrail").GetComponent<DamageTrail>().segmentPrefab, fxObj.transform);
            var psmain = fxcloud.GetComponent<ParticleSystem>().main;
            psmain.duration = 10f;
            psmain.gravityModifier = -0.05f;
            var pstartx = psmain.startSizeX;
            pstartx.constantMin *= 0.75f;
            pstartx.constantMax *= 0.75f;
            var pstarty = psmain.startSizeY;
            pstarty.constantMin *= 0.75f;
            pstarty.constantMax *= 0.75f;
            var pstartz = psmain.startSizeZ;
            pstartz.constantMin *= 0.75f;
            pstartz.constantMax *= 0.75f;
            var pslife = psmain.startLifetime;
            pslife.constantMin = 0.75f;
            pslife.constantMax = 1.5f;
            fxcloud.GetComponent<DestroyOnTimer>().enabled = false;
            fxcloud.transform.localPosition = Vector3.zero;
            fxcloud.transform.localScale = Vector3.one;
            var psshape = fxcloud.GetComponent<ParticleSystem>().shape;
            psshape.shapeType = ParticleSystemShapeType.Sphere;
            psshape.scale = Vector3.one * 1.5f;
            var psemit = fxcloud.GetComponent<ParticleSystem>().emission;
            psemit.rateOverTime = Scepter.instance.artiFlamePerformanceMode ? 4f : 20f;
            var lightObj = fxObj.Find("Point Light").gameObject;
            if(Scepter.instance.artiFlamePerformanceMode) {
                UnityEngine.Object.Destroy(lightObj);
            } else {
                var lightCpt = lightObj.GetComponent<Light>();
                lightCpt.color = new Color(1f, 0.5f, 0.2f);
                lightCpt.intensity = 3.5f;
                lightCpt.range = 5f;
            }

            ContentAddition.AddProjectile(projCloud);
        }

        internal override void LoadBehavior() {
            IL.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += IL_FlamethrowerFireGauntlet;
        }

        internal override void UnloadBehavior() {
            IL.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet -= IL_FlamethrowerFireGauntlet;
        }

        private void IL_FlamethrowerFireGauntlet(ILContext il) {
            ILCursor c = new ILCursor(il);

            bool ilFound = c.TryGotoNext(
                x => x.MatchCallvirt<BulletAttack>("Fire"));
            if(ilFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<BulletAttack,EntityStates.Mage.Weapon.Flamethrower,BulletAttack>>((origAttack,state) => {
                    if(Scepter.instance.GetCount(state.outer.commonComponents.characterBody) < 1) return origAttack;
                    origAttack.hitCallback = (BulletAttack self, ref BulletAttack.BulletHit h) => {
                        ProjectileManager.instance.FireProjectile(new FireProjectileInfo {
                            crit = false,
                            damage = origAttack.damage,
                            damageColorIndex = default,
                            damageTypeOverride = DamageType.PercentIgniteOnHit,
                            force = 0f,
                            owner = origAttack.owner,
                            position = h.point,
                            procChainMask = origAttack.procChainMask,
                            projectilePrefab = projCloud,
                            target = null
                        });
                        return BulletAttack.defaultHitCallback(origAttack, ref h);
                    };
                    return origAttack;
                });
            }
        }
    }
}
