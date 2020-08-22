using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public class EngiWalker2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}
        internal static SkillDef oldDef {get; private set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Hold and place two more turrets.</color>";
        
        public override string targetBody => "EngiBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes() {
            oldDef = Resources.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceWalkerTurret");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPENGI_WALKERNAME";
            newDescToken = "CLASSICITEMS_SCEPENGI_WALKERDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "TR58-C Carbonizer Mini";
            LanguageAPI.Add(nametoken, namestr);
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/engi_walkericon.png");
            myDef.baseMaxStock += 2;

            LoadoutAPI.AddSkillDef(myDef);
        }
    }
}