using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using RoR2;
using R2API;
using EntityStates.Loader;

namespace ThinkInvisible.ClassicItems {
    public static class LoaderChargeFist2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/loaderbody/ChargeFist");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPLOADER_CHARGEFISTNAME";
            var desctoken = "CLASSICITEMS_SCEPLOADER_CHARGEFISTDESC";
            var namestr = "Megaton Punch";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Double damage and lunge speed. Utterly ridiculous knockback.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/loader_chargefisticon.png");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter += on_BaseSwingChargedFistEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnMeleeHitAuthority += BaseSwingChargedFist_OnMeleeHitAuthority;
        }
        internal static void UnloadBehavior() {
            On.EntityStates.Loader.BaseSwingChargedFist.OnEnter -= on_BaseSwingChargedFistEnter;
            On.EntityStates.Loader.BaseSwingChargedFist.OnMeleeHitAuthority -= BaseSwingChargedFist_OnMeleeHitAuthority;
        }

        private static void on_BaseSwingChargedFistEnter(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnEnter orig, BaseSwingChargedFist self) {
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

        private static void BaseSwingChargedFist_OnMeleeHitAuthority(On.EntityStates.Loader.BaseSwingChargedFist.orig_OnMeleeHitAuthority orig, BaseSwingChargedFist self) {
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) == 0) return;
			var mTsf = self.outer.commonComponents.modelLocator?.modelTransform?.GetComponent<ChildLocator>()?.FindChild(self.swingEffectMuzzleString);
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXCommandoGrenade"),
                new EffectData {
                    origin = mTsf?.position ?? self.outer.commonComponents.transform.position,
                    scale = 5f
                }, true);
        }
    }
}
