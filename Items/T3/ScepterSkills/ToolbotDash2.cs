using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using RoR2;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class ToolbotDash2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Halves incoming damage (stacks with armor), double duration. After stopping: retaliate and stun for 200% of unmodified damage taken with a huge explosion.</color>";
        
        public override string targetBody => "ToolbotBody";
        public override SkillSlot targetSlot => SkillSlot.Utility;
        public override int targetVariantIndex => 0;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/toolbotbody/ToolbotBodyToolbotDash");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPTOOLBOT_DASHNAME";
            newDescToken = "CLASSICITEMS_SCEPTOOLBOT_DASHDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Breach Mode";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/toolbot_dashicon.png");

            ContentAddition.AddSkillDef(myDef);
        }

        internal override void LoadBehavior() {
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += On_ToolbotDashEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnExit += On_ToolbotDashExit;
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.Toolbot.ToolbotDash.OnEnter -= On_ToolbotDashEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnExit -= On_ToolbotDashExit;
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }
        
        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
            if(!self.body) {orig(self, damageInfo);return;}
            var cpt = self.body.GetComponent<ScepterToolbotDashTracker>();
            if(!cpt || !cpt.enabled) {orig(self, damageInfo);return;}
            cpt.trackedDamageTaken += damageInfo.damage;
            damageInfo.damage /= 2f;
            orig(self, damageInfo);
        }

        private void On_ToolbotDashEnter(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDash self) {
            orig(self);
            if(!self.outer.commonComponents.characterBody) return;
            if(Scepter.instance.GetCount(self.outer.commonComponents.characterBody) < 1) return;
            var cpt = self.outer.commonComponents.characterBody.GetComponent<ScepterToolbotDashTracker>();
            if(!cpt) cpt = self.outer.commonComponents.characterBody.gameObject.AddComponent<ScepterToolbotDashTracker>();
            cpt.enabled = true;
            cpt.trackedDamageTaken = 0f;
            self.baseDuration *= 2f;
        }

        private void On_ToolbotDashExit(On.EntityStates.Toolbot.ToolbotDash.orig_OnExit orig, EntityStates.Toolbot.ToolbotDash self) {
            orig(self);
            var cpt = self.outer.commonComponents.characterBody.GetComponent<ScepterToolbotDashTracker>();
            if(!cpt || !cpt.enabled) return;
            new BlastAttack {
                attacker = self.outer.commonComponents.characterBody.gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = cpt.trackedDamageTaken * 2f,
                baseForce = 1000f,
                bonusForce = Vector3.up * 500f,
                crit = self.outer.commonComponents.characterBody.RollCrit(),
                damageColorIndex = default,
                damageType = DamageType.Stun1s,
                falloffModel = BlastAttack.FalloffModel.Linear,
                losType = BlastAttack.LoSType.None,
                position = self.outer.commonComponents.transform.position,
                procCoefficient = 1f,
                radius = 20f,
                teamIndex = self.outer.commonComponents.teamComponent?.teamIndex ?? default
            }.Fire();
            EffectManager.SpawnEffect(Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFX"),
                new EffectData {
                    origin = self.outer.commonComponents.transform.position,
                    scale = 20f
                }, true);

            cpt.enabled = false;
        }

        public class ScepterToolbotDashTracker : MonoBehaviour {
            public float trackedDamageTaken;
        }
    }
}
