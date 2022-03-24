using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using RoR2;
using R2API;
using EntityStates.Captain.Weapon;
using MonoMod.Cil;
using System;
using RoR2.Projectile;

namespace ThinkInvisible.ClassicItems {
    public class CaptainAirstrikeAlt2 : ScepterSkill {
        private static GameObject projReplacer;
        private static GameObject ghostReplacer;
        public override SkillDef myDef {get; protected set;}
        public static SkillDef myCallDef {get; private set;}
        public static GameObject airstrikePrefab {get; private set;}
        
        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Covers a massive radius.</color>";

        public override string targetBody => "CaptainBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes() {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("skilldefs/captainbody/PrepAirstrike");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPCAPTAIN_AIRSTRIKEALTNAME";
            newDescToken = "CLASSICITEMS_SCEPCAPTAIN_AIRSTRIKEALTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Colony Drop";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/captain_airstrikealticon.png");

            ContentAddition.AddSkillDef(myDef);

            var oldCallDef = LegacyResourcesAPI.Load<SkillDef>("skilldefs/captainbody/CallAirstrikeAlt");
            myCallDef = CloneSkillDef(oldCallDef);
            myCallDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/captain_airstrikealticon.png");

            ContentAddition.AddSkillDef(myCallDef);

            projReplacer = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/CaptainAirstrikeAltProjectile").InstantiateClone("CIScepCaptainAirstrikeAlt", true);
            projReplacer.GetComponent<ProjectileImpactExplosion>().blastRadius *= 8f;
            var pc = projReplacer.GetComponent<ProjectileController>();

            ghostReplacer = pc.ghostPrefab.InstantiateClone("CIScepCaptainAirstrikeAltGhost");
            ghostReplacer.transform.Find("Indicator").localScale.Scale(new Vector3(8f, 8f, 2f));
            ghostReplacer.transform.Find("AreaIndicatorCenter").localScale.Scale(new Vector3(8f, 8f, 2f));
            ghostReplacer.transform.Find("AirstrikeOrientation").Find("FallingProjectile").localScale.Scale(new Vector3(4f, 4f, 4f));

            pc.ghostPrefab = ghostReplacer;

            ContentAddition.AddProjectile(projReplacer);
        }

        internal override void LoadBehavior() {
            On.EntityStates.AimThrowableBase.FireProjectile += AimThrowableBase_FireProjectile;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.AimThrowableBase.FireProjectile -= AimThrowableBase_FireProjectile;
        }

        private void AimThrowableBase_FireProjectile(On.EntityStates.AimThrowableBase.orig_FireProjectile orig, EntityStates.AimThrowableBase self) {
            bool isScep = Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
            var origPrefab = self.projectilePrefab;
            if(isScep)
                self.projectilePrefab = projReplacer;
            orig(self);
            if(isScep)
                self.projectilePrefab = origPrefab;
        }
    }
}
