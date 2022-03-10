using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using RoR2;
using R2API;
using EntityStates.Loader;

namespace ThinkInvisible.ClassicItems {
    public class LoaderChargeFist2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double damage and lunge speed. Utterly ridiculous knockback.</color>";
        
        public override string targetBody => "LoaderBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/loaderbody/ChargeFist");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPLOADER_CHARGEFISTNAME";
            newDescToken = "CLASSICITEMS_SCEPLOADER_CHARGEFISTDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Megaton Punch";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/loader_chargefisticon.png");

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior() {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter += on_BaseSwingChargedFistEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnMeleeHitAuthority += BaseSwingChargedFist_OnMeleeHitAuthority;
        }
        internal override void UnloadBehavior() {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter -= on_BaseSwingChargedFistEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnMeleeHitAuthority -= BaseSwingChargedFist_OnMeleeHitAuthority;
        }

        private void on_BaseSwingChargedFistEnter(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnEnter orig, BaseSwingChargedFist self) {
            orig(self);
            if(!(self is SwingChargedFist)) return;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
                self.minPunchForce *= 7f;
                self.maxPunchForce *= 7f;
                self.damageCoefficient *= 2f;
                self.minLungeSpeed *= 2f;
                self.maxLungeSpeed *= 2f;
            }
        }

        private void BaseSwingChargedFist_OnMeleeHitAuthority(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnMeleeHitAuthority orig, BaseSwingChargedFist self) {
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;
			var mTsf = self.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(self.swingEffectMuzzleString);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXCommandoGrenade"),
                new EffectData {
                    origin = mTsf?.position ?? self.outer.commonComponents.transform.position,
                    scale = 5f
                }, true);
        }
    }
}
