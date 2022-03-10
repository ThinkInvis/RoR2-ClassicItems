using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using R2API;
using EntityStates.Merc;
using RoR2;

namespace ThinkInvisible.ClassicItems {
    public class MercEvis2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double duration. Kills reset duration.</color>";
        
        public override string targetBody => "MercBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/mercbody/MercBodyEvis");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPMERC_EVISNAME";
            newDescToken = "CLASSICITEMS_SCEPMERC_EVISDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Massacre";
            LanguageAPI.Add(nametoken, namestr);
            
            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/merc_evisicon.png");

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior() {
            GlobalEventManager.onCharacterDeathGlobal += Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate += On_EvisFixedUpdate;
        }

        internal override void UnloadBehavior() {
            GlobalEventManager.onCharacterDeathGlobal -= Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate -= On_EvisFixedUpdate;
        }
        private void Evt_GEMOnCharacterDeathGlobal(DamageReport rep) {
            var attackerState = rep.attackerBody?.GetComponent<EntityStateMachine>()?.state;
            if(attackerState is Evis asEvis && Scepter.instance.GetCount(rep.attackerBody) > 0
                && Vector3.Distance(rep.attackerBody.transform.position, rep.victim.transform.position) < Evis.maxRadius)
                asEvis.stopwatch = 0f;
        }

        private void On_EvisFixedUpdate(On.EntityStates.Merc.Evis.orig_FixedUpdate orig, Evis self) {
            var origDuration = Evis.duration;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) > 0) Evis.duration *= 2f;
            orig(self);
            Evis.duration = origDuration;
        }
    }
}