﻿using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
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
        public override SkillDef targetVariantDef => LegacyResourcesAPI.Load<SkillDef>("skilldefs/engibody/EngiBodyPlaceWalkerTurret");

        internal override void SetupAttributes() {
            myDef = CloneSkillDef(targetVariantDef);

            var nametoken = "CLASSICITEMS_SCEPENGI_WALKERNAME";
            newDescToken = "CLASSICITEMS_SCEPENGI_WALKERDESC";
            oldDescToken = targetVariantDef.skillDescriptionToken;
            var namestr = "TR58-C Carbonizer Mini";
            LanguageAPI.Add(nametoken, namestr);
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/engi_walkericon.png");
            myDef.baseMaxStock += 2;

            ContentAddition.AddSkillDef(myDef);
        }
    }
}