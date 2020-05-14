using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API;
using RoR2;
using R2API.Utils;

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
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Charges four times faster. Hold and fire up to four charges at once.</color>");
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/merc_evisprojectileicon.png");
            myDef.baseMaxStock *= 4;
            myDef.baseRechargeInterval /= 4f;

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Commando.CommandoWeapon.FireFMJ.OnEnter += On_FireFMJEnter;
        }

        internal static void UnloadBehavior() {
            On.EntityStates.Commando.CommandoWeapon.FireFMJ.OnEnter -= On_FireFMJEnter;
        }

        private static void On_FireFMJEnter(On.EntityStates.Commando.CommandoWeapon.FireFMJ.orig_OnEnter orig, EntityStates.Commando.CommandoWeapon.FireFMJ self) {
            orig(self);
            if(!(self is EntityStates.Commando.CommandoWeapon.ThrowEvisProjectile) || Scepter.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;
            if(!self.outer.commonComponents.skillLocator?.special) return;
            var fireCount = self.outer.commonComponents.skillLocator.special.stock;
            self.outer.commonComponents.skillLocator.special.stock = 0;
            for(var i = 0; i < fireCount; i++) {
                typeof(EntityStates.Commando.CommandoWeapon.FireFMJ).GetMethodCached("Fire").Invoke(self, new object[]{});
            }
        }
    }
}