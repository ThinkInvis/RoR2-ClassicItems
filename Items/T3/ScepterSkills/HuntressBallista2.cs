using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using EntityStates.Huntress;
using RoR2;
using R2API;
using RoR2.Projectile;

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
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Quadruple shot count and fire rate, half damage. Every shot has intense bonus knockback.</color>");

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
            myCtxDef.baseMaxStock *= 4;
            myCtxDef.shootDelay /= 4;

            LoadoutAPI.AddSkillDef(myCtxDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter += On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.ModifyBullet += On_FireArrowSnipeModify;
        }

        private static void On_FireArrowSnipeModify(On.EntityStates.Huntress.Weapon.FireArrowSnipe.orig_ModifyBullet orig, EntityStates.Huntress.Weapon.FireArrowSnipe self, BulletAttack bulletAttack) {
            orig(self, bulletAttack);
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
                bulletAttack.force *= 4f;
                bulletAttack.damage /= 2f;
            }
        }

        internal static void UnloadBehavior() {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter -= On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.ModifyBullet -= On_FireArrowSnipeModify;
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
    }
}
