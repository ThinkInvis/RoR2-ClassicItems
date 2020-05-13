using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public static class MercEvisProjectile2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/mercbody/MercBodyEvisProjectile");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPMERC_EVISPROJNAME";
            var desctoken = "CLASSICITEMS_SCEPMERC_EVISPROJDESC";
            var namestr = "Gale-Force";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Charges faster. Fires up to 4 projectiles at once.</color>");
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/merc_evisprojectileicon.png");
            myDef.baseMaxStock *= 4;
            myDef.baseRechargeInterval /= 4f;

            LoadoutAPI.AddSkillDef(myDef);
        }
    }
}