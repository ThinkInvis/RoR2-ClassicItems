using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using EntityStates.Huntress;
using RoR2;
using R2API;
using EntityStates.Huntress.Weapon;

namespace ThinkInvisible.ClassicItems {
    public class HuntressBallista2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}
        public static SkillDef myCtxDef {get; private set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Fires quick bursts of five extra projectiles for 2.5x TOTAL damage.</color>";
        
        public override string targetBody => "HuntressBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/huntressbody/AimArrowSnipe");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPHUNTRESS_BALLISTANAME";
            newDescToken = "CLASSICITEMS_SCEPHUNTRESS_BALLISTADESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Rabauld";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/huntress_ballistaicon.png");

            ContentAddition.AddSkillDef(myDef);
            
            var oldCtxDef = Resources.Load<SkillDef>("skilldefs/huntressbody/FireArrowSnipe");
            myCtxDef = CloneSkillDef(oldCtxDef);

            myCtxDef.skillName = namestr;
            myCtxDef.skillNameToken = nametoken;
            myCtxDef.skillDescriptionToken = newDescToken;
            myCtxDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/huntress_ballistaicon.png");

            ContentAddition.AddSkillDef(myCtxDef);
        }

        internal override void LoadBehavior() {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter += On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.AimArrowSnipe.OnExit += On_AimArrowSnipeExit;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.FireBullet += On_FireArrowSnipeFire;
        }
        internal override void UnloadBehavior() {
            On.EntityStates.Huntress.AimArrowSnipe.OnEnter -= On_AimArrowSnipeEnter;
            On.EntityStates.Huntress.AimArrowSnipe.OnExit -= On_AimArrowSnipeExit;
            On.EntityStates.Huntress.Weapon.FireArrowSnipe.FireBullet -= On_FireArrowSnipeFire;
        }

        private void On_FireArrowSnipeFire(On.EntityStates.Huntress.Weapon.FireArrowSnipe.orig_FireBullet orig, FireArrowSnipe self, Ray aimRay) {
            orig(self, aimRay);
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;

            for(var i = 1; i < 6; i++) {
                var sprRay = new Ray(aimRay.origin, aimRay.direction);
                sprRay.direction = Util.ApplySpread(sprRay.direction, 0.1f+i/24f, 0.3f+i/8f, 1f, 1f, 0f, 0f);
				var pew = self.GenerateBulletAttack(sprRay);
                self.ModifyBullet(pew);
                pew.damage /= 10f/3f;
                pew.force /= 20f/3f;
                RoR2Application.fixedTimeTimers.CreateTimer(i*0.06f, ()=>{
                    pew.Fire();
			        Util.PlaySound(self.fireSoundString, self.outer.gameObject);
                });
            }
        }

        private void On_AimArrowSnipeEnter(On.EntityStates.Huntress.AimArrowSnipe.orig_OnEnter orig, AimArrowSnipe self) {
            orig(self);
            var sloc = self.outer.commonComponents.skillLocator;
            if(!sloc || !sloc.primary) return;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
				sloc.primary.UnsetSkillOverride(self, AimArrowSnipe.primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
                sloc.primary.SetSkillOverride(self, myCtxDef, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void On_AimArrowSnipeExit(On.EntityStates.Huntress.AimArrowSnipe.orig_OnExit orig, AimArrowSnipe self) {
            orig(self);
            var sloc = self.outer.commonComponents.skillLocator;
            if(!sloc || !sloc.primary) return;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0)
                sloc.primary.UnsetSkillOverride(self, myCtxDef, GenericSkill.SkillOverridePriority.Contextual);
        }
    }
}
