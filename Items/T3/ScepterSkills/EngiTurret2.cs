using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public static class EngiTurret2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceTurret");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPENGI_TURRETNAME";
            var desctoken = "CLASSICITEMS_SCEPENGI_TURRETDESC";
            var namestr = "TR12-C Gauss Compact";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Hold and place one more turret.</color>");
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/engi_turreticon.png");
            myDef.baseMaxStock += 1;

            LoadoutAPI.AddSkillDef(myDef);
        }
    }
}