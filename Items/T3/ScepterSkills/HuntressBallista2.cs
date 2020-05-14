using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using EntityStates.Huntress;
using RoR2;
using R2API;
using RoR2.Projectile;
using EntityStates.Huntress.Weapon;

namespace ThinkInvisible.ClassicItems {
    public static class HuntressBallista2 {
        public static SkillDef myDef {get; private set;}
        public static SkillDef myCtxDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/huntressbody/AimArrowSnipe");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPHUNTRESS_BALLISTANAME";
            var desctoken = "CLASSICITEMS_SCEPHUNTRESS_BALLISTADESC";
            var namestr = "Rabauld";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Fires quick bursts of five extra projectiles for 2.5x TOTAL damage.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/huntress_ballistaicon.png");

            LoadoutAPI.AddSkillDef(myDef);
            
            var oldCtxDef = Resources.Load<SkillDef>("skilldefs/huntressbody/FireArrowSnipe");
            myCtxDef = CloneSkillDef(oldCtxDef);

            myCtxDef.skillName = namestr;
            myCtxDef.skillNameToken = nametoken;
            myCtxDef.skillDescriptionToken = desctoken;
            myCtxDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/huntress_ballistaicon.png");

            LoadoutAPI.AddSkillDef(myCtxDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter += On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.AimArrowSnipe.OnExit += On_AimArrowSnipeExit;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.FireBullet += On_FireArrowSnipeFire;
        }
        internal static void UnloadBehavior() {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter -= On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.AimArrowSnipe.OnExit -= On_AimArrowSnipeExit;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.FireBullet -= On_FireArrowSnipeFire;
        }

        private static void On_FireArrowSnipeFire(On.EntityStates.Huntress.Weapon.FireArrowSnipe.orig_FireBullet orig, FireArrowSnipe self, Ray aimRay) {
            orig(self, aimRay);
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) == 0) return;

            for(var i = 1; i < 6; i++) {
                var sprRay = new Ray(aimRay.origin, aimRay.direction);
                sprRay.direction = Util.ApplySpread(sprRay.direction, 0.1f+i/24f, 0.3f+i/8f, 1f, 1f, 0f, 0f);
				var pew = (BulletAttack)typeof(EntityStates.GenericBulletBaseState).GetMethodCached("GenerateBulletAttack").Invoke(self, new object[]{sprRay});
				typeof(FireArrowSnipe).GetMethodCached("ModifyBullet").Invoke(self, new object[]{pew});
                pew.damage /= 10f/3f;
                pew.force /= 20f/3f;
                RoR2Application.fixedTimeTimers.CreateTimer(i*0.06f, ()=>{
                    pew.Fire();
			        Util.PlaySound(self.fireSoundString, self.outer.gameObject);
                });
            }
        }

        private static void On_AimArrowSnipeEnter(On.EntityStates.Huntress.AimArrowSnipe.orig_OnEnter orig, AimArrowSnipe self) {
            orig(self);
            var sloc = self.outer.commonComponents.skillLocator;
            if(!sloc || !sloc.primary) return;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
				sloc.primary.UnsetSkillOverride(self, AimArrowSnipe.primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
                sloc.primary.SetSkillOverride(self, myCtxDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private static void On_AimArrowSnipeExit(On.EntityStates.Huntress.AimArrowSnipe.orig_OnExit orig, AimArrowSnipe self) {
            orig(self);
            var sloc = self.outer.commonComponents.skillLocator;
            if(!sloc || !sloc.primary) return;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
                sloc.primary.UnsetSkillOverride(self, myCtxDef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}
