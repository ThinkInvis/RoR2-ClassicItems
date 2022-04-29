using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using R2API;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public class MercEvisProjectile2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Charges four times faster. Hold and fire up to four charges at once.</color>";
        
        public override string targetBody => "MercBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override SkillDef targetVariantDef => LegacyResourcesAPI.Load<SkillDef>("skilldefs/mercbody/MercBodyEvisProjectile");

        internal override void SetupAttributes() {
            myDef = CloneSkillDef(targetVariantDef);

            var nametoken = "CLASSICITEMS_SCEPMERC_EVISPROJNAME";
            newDescToken = "CLASSICITEMS_SCEPMERC_EVISPROJDESC";
            oldDescToken = targetVariantDef.skillDescriptionToken;
            var namestr = "Gale-Force";
            LanguageAPI.Add(nametoken, namestr);
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ScepterSkillIcons/merc_evisprojectileicon.png");
            myDef.baseMaxStock *= 4;
            myDef.baseRechargeInterval /= 4f;

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior() {
            On.EntityStates.GenericProjectileBaseState.OnEnter += On_FireFMJEnter;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.GenericProjectileBaseState.OnEnter -= On_FireFMJEnter;
        }

        private void On_FireFMJEnter(On.EntityStates.GenericProjectileBaseState.orig_OnEnter orig, EntityStates.GenericProjectileBaseState self) {
            orig(self);
            if(!(self is EntityStates.Merc.Weapon.ThrowEvisProjectile) || Scepter.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;
            if(!self.outer.commonComponents.skillLocator?.special) return;
            var fireCount = self.outer.commonComponents.skillLocator.special.stock;
            self.outer.commonComponents.skillLocator.special.stock = 0;
            for(var i = 0; i < fireCount; i++) {
                self.FireProjectile();
                self.DoFireEffects();
            }
        }
    }
}