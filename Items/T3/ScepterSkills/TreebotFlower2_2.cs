using UnityEngine;
using RoR2.Skills;
using static TILER2.MiscUtil;
using R2API.Utils;
using RoR2;
using R2API;
using System.Collections.Generic;
using EntityStates.Treebot.TreebotFlower;

namespace ThinkInvisible.ClassicItems {
    public static class TreebotFlower2_2 {
        public static SkillDef myDef {get; private set;}
        internal static void SetupAttributes() {
            var oldDef = Resources.Load<SkillDef>("skilldefs/treebotbody/TreebotBodyFireFlower2");
            myDef = CloneSkillDef(oldDef);

            var nametoken = "CLASSICITEMS_SCEPTREEBOT_FLOWER2NAME";
            var desctoken = "CLASSICITEMS_SCEPTREEBOT_FLOWER2DESC";
            var namestr = "Chaotic Growth";
            LanguageAPI.Add(nametoken, namestr);
            LanguageAPI.Add(desctoken, Language.GetString(oldDef.skillDescriptionToken) + "\n<color=#d299ff>SCEPTER: Double radius. Pulses random debuffs.</color>");

            myDef.skillName = namestr;
            myDef.skillNameToken = nametoken;
            myDef.skillDescriptionToken = desctoken;
            myDef.icon = Resources.Load<Sprite>("@ClassicItems:Assets/ClassicItems/icons/scepter/treebot_entangleicon.png");

            LoadoutAPI.AddSkillDef(myDef);
        }

        internal static void LoadBehavior() {
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.RootPulse += On_TreebotFlower2RootPulse;
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.OnEnter += On_TreebotFlower2Enter;
        }

        internal static void UnloadBehavior() {
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.RootPulse -= On_TreebotFlower2RootPulse;
            On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.OnEnter -= On_TreebotFlower2Enter;
        }
        
        private static void On_TreebotFlower2Enter(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_OnEnter orig, TreebotFlower2Projectile self) {
            var origRadius = TreebotFlower2Projectile.radius;
            if(Scepter.instance.GetCount(self.GetFieldValue<GameObject>("owner")?.GetComponent<CharacterBody>()) > 0) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
        }

        private static Xoroshiro128Plus treebotFlowerRNG = new Xoroshiro128Plus(0);

        private static void On_TreebotFlower2RootPulse(On.EntityStates.Treebot.TreebotFlower.TreebotFlower2Projectile.orig_RootPulse orig, TreebotFlower2Projectile self) {
            var owner = self.GetFieldValue<GameObject>("owner");
            var isBoosted = Scepter.instance.GetCount(owner?.GetComponent<CharacterBody>()) > 0;
            var origRadius = TreebotFlower2Projectile.radius;
            if(isBoosted) TreebotFlower2Projectile.radius *= 2f;
            orig(self);
            TreebotFlower2Projectile.radius = origRadius;
            if(!isBoosted) return;
            var rb = self.GetFieldValue<List<CharacterBody>>("rootedBodies");
            rb.ForEach(cb => {
                var nbi = treebotFlowerRNG.NextElementUniform(new[] {
                    BuffIndex.Bleeding,
                    BuffIndex.ClayGoo,
                    BuffIndex.Cripple,
                    BuffIndex.HealingDisabled,
                    BuffIndex.OnFire,
                    BuffIndex.Weak,
                    BuffIndex.Pulverized,
                    ClassicItemsPlugin.freezeBuff
                });
                if(nbi == ClassicItemsPlugin.freezeBuff) {
                    var ssoh = cb.gameObject.GetComponent<SetStateOnHurt>();
                    if(ssoh && ssoh.canBeFrozen)
                        ssoh.SetFrozen(1.5f);
                    else return;
                }
                if(nbi == BuffIndex.OnFire) DotController.InflictDot(cb.gameObject, owner, DotController.DotIndex.Burn, 1.5f, 1f);
                if(nbi == BuffIndex.Bleeding) DotController.InflictDot(cb.gameObject, owner, DotController.DotIndex.Bleed, 1.5f, 1f);
                cb.AddTimedBuff(nbi, 1.5f);
            });
        }
    }
}
