using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using RoR2;
using R2API;
using MonoMod.Cil;
using System;
using RoR2.Projectile;
using Mono.Cecil.Cil;

namespace ThinkInvisible.ClassicItems {
    public static class ArtificerFlamethrower2 {
        private static GameObject projCloud;
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/magebody/MageBodyFlamethrower");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPMAGE_FLAMETHROWERNAME";
            var desctoken = "CLASSICITEMS_SCEPMAGE_FLAMETHROWERDESC";
            var namestr = "Dragon's Breath";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Hits leave behind a lingering fire cloud.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/mage_flamethrowericon.png");

            LoadoutAPI.AddSkillDef(myDef);

            projCloud = Resources.Load<GameObject>("prefabs/projectiles/BeetleQueenAcid").InstantiateClone("CIScepMageFlamethrowerCloud");
            var pdz = projCloud.GetComponent<ProjectileDotZone>();
            pdz.lifetime = 10f;
            pdz.impactEffect = null;
            pdz.fireFrequency = 2f;
            var fxObj = projCloud.transform.Find("FX");
            fxObj.Find("Spittle").gameObject.SetActive(false);
            fxObj.Find("Decal").gameObject.SetActive(false);
            fxObj.Find("Gas").gameObject.SetActive(false);
            foreach(var x in fxObj.GetComponents<AnimateShaderAlpha>()) {x.enabled = false;}
            var fxcloud = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("prefabs/FireTrail").GetComponent<DamageTrail>().segmentPrefab, fxObj.transform);
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
            psemit.rateOverTime = 20f;
            var lightCpt = fxObj.Find("Point Light").gameObject.GetComponent<Light>();
            lightCpt.color = new Color(1f, 0.5f, 0.2f);
            lightCpt.intensity = 3.5f;
            lightCpt.range = 5f;

            ProjectileCatalog.getAdditionalEntries += (list) => list.Add(projCloud);
        }

        internal static void LoadBehavior() {
            IL.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet += IL_FlamethrowerFireGauntlet;
        }

        internal static void UnloadBehavior() {
            IL.EntityStates.Mage.Weapon.Flamethrower.FireGauntlet -= IL_FlamethrowerFireGauntlet;
        }

        private static void IL_FlamethrowerFireGauntlet(ILContext il) {
            ILCursor c = new ILCursor(il);

            bool ilFound = c.TryGotoNext(
                x => x.MatchCallvirt<BulletAttack>("Fire"));
            if(ilFound) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<BulletAttack,EntityStates.Mage.Weapon.Flamethrower,BulletAttack>>((origAttack,state) => {
                    if(Scepter.instance.GetCount(state.outer.commonComponents.characterBody) < 1) return origAttack;
                    origAttack.hitCallback = (ref BulletAttack.BulletHit h) => {
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
                        return origAttack.DefaultHitCallback(ref h);
                    };
                    return origAttack;
                });
            }
        }
    }
}
