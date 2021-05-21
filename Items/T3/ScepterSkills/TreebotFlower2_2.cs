using UnityEngine;
using RoR2.Skills;
using static TILER2.SkillUtil;
using RoR2;
using R2API;
using EntityStates.Treebot.TreebotFlower;
using RoR2.Projectile;

namespace ThinkInvisible.ClassicItems {
    public class TreebotFlower2_2 : ScepterSkill {
        public override SkillDef myDef {get; protected set;}

        public override string oldDescToken {get; protected set;}
        public override string newDescToken {get; protected set;}
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double radius. Pulses random debuffs.</color>";
        
        public override string targetBody => "TreebotBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/treebotbody/TreebotBodyFireFlower2");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPTREEBOT_FLOWER2NAME";
            newDescToken = "CLASSICITEMS_SCEPTREEBOT_FLOWER2DESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Chaotic Growth";
            LanguageAPI.Add(nametoken, namestr);

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = newDescToken;
            myDef.icon = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/scepter/treebot_entangleicon.png");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal override void LoadBehavior() {
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.RootPulse += On_TreebotFlower2RootPulse;
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.OnEnter += On_TreebotFlower2Enter;
        }

        internal override void UnloadBehavior() {
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.RootPulse -= On_TreebotFlower2RootPulse;
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.OnEnter -= On_TreebotFlower2Enter;
        }
        
        private void On_TreebotFlower2Enter(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_OnEnter orig, TreebotFlower2Projectile self) {
			var owner = self.outer.GetComponent<ProjectileController>()?.owner;
            var origRadius = TreebotFlower2Projectile.radius;
            if(Scepter.instance.GetCount(owner.GetComponent<CharacterBody>()) > 0) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
        }

        private void On_TreebotFlower2RootPulse(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_RootPulse orig, TreebotFlower2Projectile self) {
            var isBoosted = Scepter.instance.GetCount(self.owner?.GetComponent<CharacterBody>()) > 0;
            var origRadius = TreebotFlower2Projectile.radius;
            if(isBoosted) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
            if(!isBoosted) return;
            self.rootedBodies.ForEach(cb => {
                var nbi = Scepter.instance.rng.NextElementUniform(new[] {
                    RoR2Content.Buffs.Bleeding,
                    RoR2Content.Buffs.ClayGoo,
                    RoR2Content.Buffs.Cripple,
                    RoR2Content.Buffs.HealingDisabled,
                    RoR2Content.Buffs.OnFire,
                    RoR2Content.Buffs.Weak,
                    RoR2Content.Buffs.Pulverized,
                    ClassicItemsPlugin.freezeBuff
                });
                if(nbi == ClassicItemsPlugin.freezeBuff) {
                    var ssoh = cb.gameObject.GetComponent<SetStateOnHurt>();
                    if(ssoh && ssoh.canBeFrozen)
                        ssoh.SetFrozen(1.5f);
                    else return;
                }
                if(nbi == RoR2Content.Buffs.OnFire) DotController.InflictDot(cb.gameObject, self.owner, DotController.DotIndex.Burn, 1.5f, 1f);
                if(nbi == RoR2Content.Buffs.Bleeding) DotController.InflictDot(cb.gameObject, self.owner, DotController.DotIndex.Bleed, 1.5f, 1f);
                cb.AddTimedBuff(nbi, 1.5f);
            });
        }
    }
}
