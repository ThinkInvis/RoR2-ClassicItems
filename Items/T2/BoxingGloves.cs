﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class BoxingGloves : Item<BoxingGloves> {
        public override string displayName => "Boxing Gloves";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for Boxing Gloves to proc; stacks multiplicatively.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 6f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 1000f)]
        [AutoConfig("Multiplier for knockback force vs. grounded targets.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float procForceGrounded {get;private set;} = 90f;

        [AutoConfigRoOSlider("{0:N1}", 0f, 1000f)]
        [AutoConfig("Multiplier for knockback force vs. flying targets.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float procForceFlying {get;private set;} = 30f;
        
        [AutoConfigRoOCheckbox()]
        [AutoConfig("If false, Boxing Gloves will not proc on bosses.", AutoConfigFlags.None)]
        public bool affectBosses {get;private set;} = false;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Hitting enemies have a " + Pct(procChance,0,1) + " chance to knock them back.";
        protected override string GetDescString(string langid = null) => "<style=cIsUtility>" + Pct(procChance,0,1) + "</style> <style=cStack>(+"+Pct(procChance,0,1)+" per stack, multiplicative)</style> chance to <style=cIsUtility>knock back</style> an enemy <style=cIsDamage>based on fraction of health removed</style>.";
        protected override string GetLoreString(string langid = null) => "Order: Boxing Gloves\n\nTracking Number: 362***********\nEstimated Delivery: 7/7/2056\nShipping Method: Standard\nShipping Address: O.B.-GYM, Slam Station, Venus\nShipping Details:\n\nThese should work fine for the kids you're training. A bit musty, though. It'll make your trainees hit like a pro, ha!";

        public BoxingGloves() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/boxinggloves_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/BoxingGloves.prefab");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
        }

        public override void Install() {
            base.Install();
            On.RoR2.HealthComponent.TakeDamage += On_HCTakeDamage;
        }
        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.HealthComponent.TakeDamage -= On_HCTakeDamage;
        }

        private void On_HCTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo di) {
            if(di?.attacker && (affectBosses || (!self.body?.isBoss ?? false))) {
                var cb = di.attacker.GetComponent<CharacterBody>();
                if(cb) {
                    var pChance = (1f-Mathf.Pow(1-procChance/100f,GetCount(cb)))*100f;
                    var proc = cb.master ? Util.CheckRoll(pChance,cb.master) : Util.CheckRoll(pChance);
                    if(proc) {
                        var mass = self.body.characterMotor?.mass ?? (self.body.rigidbody?.mass ?? 1f);
                        //var prcf = di.damage * procForce;
                        var prcf = (di.damage / self.fullCombinedHealth) * (self.body.isFlying ? procForceFlying : procForceGrounded);
                        if(di.force == Vector3.zero)
                            di.force += Vector3.Normalize(di.position - cb.corePosition) * prcf * mass;
                        else di.force += Vector3.Normalize(di.force) * prcf * mass;
                    }
                }
            }

            orig(self,di);
        }
    }
}
