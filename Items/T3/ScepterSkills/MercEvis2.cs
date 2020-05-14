using EntityStates;
using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API;
using EntityStates.Merc;
using RoR2;
using R2API.Utils;

namespace ThinkInvisible.ClassicItems {
    public static class MercEvis2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/mercbody/MercBodyEvis");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPMERC_EVISNAME";
            var desctoken = "CLASSICITEMS_SCEPMERC_EVISDESC";
            var namestr = "Massacre";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Double duration. Kills reset duration.</color>");
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/merc_evisicon.png");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal static void LoadBehavior() {
            GlobalEventManager.onCharacterDeathGlobal += Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate += On_EvisFixedUpdate;
        }

        internal static void UnloadBehavior() {
            GlobalEventManager.onCharacterDeathGlobal -= Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate -= On_EvisFixedUpdate;
        }
        private static void Evt_GEMOnCharacterDeathGlobal(DamageReport rep) {
            var attackerState = rep.attackerBody?.GetComponent<EntityStateMachine>()?.state;
            if(attackerState is Evis && Scepter.instance.GetCount(rep.attackerBody) > 0)
                typeof(Evis).GetFieldCached("stopwatch").SetValue(attackerState, 0f);
        }

        private static void On_EvisFixedUpdate(On.EntityStates.Merc.Evis.orig_FixedUpdate orig, Evis self) {
            var origDuration = Evis.duration;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) Evis.duration *= 2f;
            orig(self);
            Evis.duration = origDuration;
        }
    }
}