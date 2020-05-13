using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public static class CommandoBarrage2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/commandobody/CommandoBodyBarrage");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPCOMMANDO_BARRAGENAME";
            var desctoken = "CLASSICITEMS_SCEPCOMMANDO_BARRAGEDESC";
            var namestr = "Death Blossom";
            LanguageAPI.Add(nametoken, namestr);
            //TODO: fire auto-aim bullets at every enemy in range
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Fires twice as many bullets, twice as fast, with twice the accuracy.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/commando_barrageicon.png");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter += On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet += On_FireBarrage_FireBullet;
        }

        internal static void UnloadBehavior() {
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.OnEnter -= On_FireBarrage_Enter;
            On.EntityStates.Commando.CommandoWeapon.FireBarrage.FireBullet -= On_FireBarrage_FireBullet;
        }

        private static void On_FireBarrage_Enter(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_OnEnter orig, FireBarrage self) {
            orig(self);
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
                self.SetFieldValue("durationBetweenShots", self.GetFieldValue<float>("durationBetweenShots") / 2f);
                self.SetFieldValue("bulletCount", self.GetFieldValue<int>("bulletCount") * 2);
            }
        }

        private static void On_FireBarrage_FireBullet(On.EntityStates.Commando.CommandoWeapon.FireBarrage.orig_FireBullet orig, FireBarrage self) {
            bool hasScep = Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0;
            var origAmp = FireBarrage.recoilAmplitude;
            if(hasScep) FireBarrage.recoilAmplitude /= 2;
            orig(self);
            FireBarrage.recoilAmplitude = origAmp;
        }
    }
}
