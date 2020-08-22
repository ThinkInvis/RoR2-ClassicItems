using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using RoR2;
using R2API;
using EntityStates.Captain.Weapon;
using MonoMod.Cil;
using System;

namespace ThinkInvisible.ClassicItems {
    public static class CaptainAirstrike2 {
        public static SkillDef myDef {get; private set;}
        public static SkillDef myCallDef {get; private set;}
        public static GameObject airstrikePrefab {get; private set;}

        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/captainbody/PrepAirstrike");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPCAPTAIN_AIRSTRIKENAME";
            var desctoken = "CLASSICITEMS_SCEPCAPTAIN_AIRSTRIKEDESC";
            var namestr = "21-Probe Salute";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Hold to call down one continuous barrage for 21x500% damage.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/mage_flyupicon.png");

            LoadoutAPI.AddSkillDef(myDef);

            var oldCallDef = Resources.Load<SkillDef>("skilldefs/captainbody/CallAirstrike");
            myCallDef = CloneSkillDef(oldCallDef);
            myCallDef.baseMaxStock = 21;
            myCallDef.mustKeyPress = false;
            myCallDef.isBullets = true;
            myCallDef.shootDelay = 0.1f;
            myCallDef.baseRechargeInterval = 0.1f;

            LoadoutAPI.AddSkillDef(myCallDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter += On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit += On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter += On_CallAirstrikeBaseEnter;
            On.RoR2.GenericSkill.RestockSteplike += GenericSkill_RestockSteplike;
            IL.EntityStates.Captain.Weapon.CallAirstrikeEnter.OnEnter += IL_CallAirstrikeEnterEnter;
        }

        internal static void UnloadBehavior() {
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnEnter -= On_SetupAirstrikeStateEnter;
            On.EntityStates.Captain.Weapon.SetupAirstrike.OnExit -= On_SetupAirstrikeStateExit;
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter -= On_CallAirstrikeBaseEnter;
            On.RoR2.GenericSkill.RestockSteplike -= GenericSkill_RestockSteplike;
            IL.EntityStates.Captain.Weapon.CallAirstrikeEnter.OnEnter -= IL_CallAirstrikeEnterEnter;
        }
        
        private static void IL_CallAirstrikeEnterEnter(ILContext il) {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<GenericSkill>("get_stock"));
            c.EmitDelegate<Func<int, int>>((origStock) => {
                if(origStock == 0) return 0;
                return origStock % 2 + 1;
            });
        }

        private static void GenericSkill_RestockSteplike(On.RoR2.GenericSkill.orig_RestockSteplike orig, GenericSkill self) {
            if(self.skillDef == myCallDef) return;
            orig(self);
        }

        private static void On_CallAirstrikeBaseEnter(On.EntityStates.Captain.Weapon.CallAirstrikeBase.orig_OnEnter orig, CallAirstrikeBase self) {
            orig(self);
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
                self.damageCoefficient = 5f;
            }
        }

        private static void On_SetupAirstrikeStateEnter(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnEnter orig, EntityStates.Captain.Weapon.SetupAirstrike self) {
            var origOverride = SetupAirstrike.primarySkillDef;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) {
                SetupAirstrike.primarySkillDef = myCallDef;
            }
            orig(self);
            SetupAirstrike.primarySkillDef = origOverride;
        }

        private static void On_SetupAirstrikeStateExit(On.EntityStates.Captain.Weapon.SetupAirstrike.orig_OnExit orig, EntityStates.Captain.Weapon.SetupAirstrike self) {
            var pSS = self.GetFieldValue<GenericSkill>("primarySkillSlot");
            if(pSS)
                pSS.UnsetSkillOverride(self, myCallDef, GenericSkill.SkillOverridePriority.Contextual);
            orig(self);
        }
    }
}
