using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using EntityStates.Mage;
using RoR2;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class ArtificerFlyUp2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}
        
        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double damage, quadruple radius.</color>";
        
        public override string targetBody => "MageBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes() {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("skilldefs/magebody/MageBodyFlyUp");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPMAGE_FLYUPNAME";
            newDescToken = "CLASSICITEMS_SCEPMAGE_FLYUPDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Antimatter Surge";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/mage_flyupicon.png");

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior() {
            On.EntityStates.Mage.FlyUpState.OnEnter += On_FlyUpStateEnter;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.Mage.FlyUpState.OnEnter -= On_FlyUpStateEnter;
        }

        private void On_FlyUpStateEnter(On.EntityStates.Mage.FlyUpState.orig_OnEnter orig, EntityStates.Mage.FlyUpState self) {
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
