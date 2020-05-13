using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using EntityStates.Mage;
using RoR2;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public static class ArtificerFlyUp2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/magebody/MageBodyFlyUp");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPMAGE_FLYUPNAME";
            var desctoken = "CLASSICITEMS_SCEPMAGE_FLYUPDESC";
            var namestr = "Antimatter Surge";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Double damage, quadruple radius.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/mage_flyupicon.png");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Mage.FlyUpState.OnEnter += On_FlyUpStateEnter;
        }

        internal static void UnloadBehavior() {
            On.EntityStates.Mage.FlyUpState.OnEnter -= On_FlyUpStateEnter;
        }

        private static void On_FlyUpStateEnter(On.EntityStates.Mage.FlyUpState.orig_OnEnter orig, EntityStates.Mage.FlyUpState self) {
            var origRadius = FlyUpState.blastAttackRadius;
            var origDamage = FlyUpState.blastAttackDamageCoefficient;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
                FlyUpState.blastAttackRadius *= 4f;
                FlyUpState.blastAttackDamageCoefficient *= 2f;
            }
            orig(self);
            FlyUpState.blastAttackRadius = origRadius;
            FlyUpState.blastAttackDamageCoefficient = origDamage;
        }
    }
}
