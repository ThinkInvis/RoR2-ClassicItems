using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public static class EngiWalker2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceWalkerTurret");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPENGI_WALKERNAME";
            var desctoken = "CLASSICITEMS_SCEPENGI_WALKERDESC";
            var namestr = "TR58-C Carbonizer Mini";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Hold and place two more turrets.</color>");
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/engi_walkericon.png");
            myDef.baseMaxStock += 2;

            LoadoutAPI.AddSkillDef(myDef);
        }
    }
}