﻿using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using R2API;
using RoR2.Projectile;
using RoR2;
using EntityStates.Commando.CommandoWeapon;

namespace ThinkInvisible.ClassicItems {
    public class CommandoGrenade2 : ScepterSkill {
        private static GameObject projReplacer;
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Half damage and knockback; throw eight at once.</color>";
        
        public override string targetBody => "CommandoBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override SkillDef targetVariantDef => LegacyResourcesAPI.Load<SkillDef>("skilldefs/commandobody/ThrowGrenade");

        internal override void SetupAttributes() {
            myDef = CloneSkillDef(targetVariantDef);

            var nametoken = "CLASSICITEMS_SCEPCOMMANDO_GRENADENAME";
            newDescToken = "CLASSICITEMS_SCEPCOMMANDO_GRENADEDESC";
            oldDescToken = targetVariantDef.skillDescriptionToken;
            var namestr = "Carpet Bomb";
            LanguageAPI.Add(nametoken, namestr);
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/commando_grenadeicon.png");

            ContentAddition.AddSkillDef(myDef);

            projReplacer = LegacyResourcesAPI.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile").InstantiateClone("CIScepCommandoGrenade", true);
            var pie = projReplacer.GetComponent<ProjectileImpactExplosion>();
            pie.blastDamageCoefficient *= 0.5f;
            pie.bonusBlastForce *= 0.5f;

            ContentAddition.AddProjectile(projReplacer);
        }
        
        internal override void LoadBehavior() {
            On.EntityStates.GenericProjectileBaseState.FireProjectile += On_FireFMJFire;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.GenericProjectileBaseState.FireProjectile -= On_FireFMJFire;
        }

        private void On_FireFMJFire(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self) {
            var cc = self.outer.commonComponents;
            bool isBoosted = self.GetType() == typeof(ThrowGrenade)
                && Util.HasEffectiveAuthority(self.outer.networkIdentity)
                && Scepter.instance.GetCount(cc.characterBody) > 0;
            if(isBoosted) self.projectilePrefab = projReplacer;
            orig(self);
            if(isBoosted) {
                for(var i = 0; i < 7; i++) {
                    Ray r;
			        if (cc.inputBank) r = new Ray(cc.inputBank.aimOrigin, cc.inputBank.aimDirection);
                    else r = new Ray(cc.transform.position, cc.transform.forward);
				    r.direction = Util.ApplySpread(r.direction, self.minSpread + 7f, self.maxSpread + 15f, 1f, 1f, 0f, self.projectilePitchBonus);
				    ProjectileManager.instance.FireProjectile(
                        self.projectilePrefab,
                        r.origin, Util.QuaternionSafeLookRotation(r.direction),
                        self.outer.gameObject,
                        self.damageStat * self.damageCoefficient,
                        self.force,
                        Util.CheckRoll(self.critStat, cc.characterBody.master),
                        DamageColorIndex.Default, null, -1f);
                }
            }
        }
    }
}