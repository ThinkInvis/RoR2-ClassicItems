﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    public class Taser : Item<Taser> {
        public override string displayName => "Taser";
		public override ItemTier itemTier => ItemTier.Tier1;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Utility});

        [AutoConfigRoOSlider("{0:N0}%", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance for Taser to proc.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 7f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of root applied by first Taser stack.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float procTime {get;private set;} = 1.5f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of root applied per additional Taser stack.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float stackTime {get;private set;} = 0.5f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Chance to snare on hit.";
        protected override string GetDescString(string langid = null) {
            string desc = "<style=cIsUtility>" + Pct(procChance, 0, 1) + "</style> chance to <style=cIsUtility>entangle</style> an enemy for <style=cIsUtility>" + procTime.ToString("N1") + " seconds</style>";
            if(stackTime > 0f) desc += $" <style=cStack>(+{stackTime:N1} per stack)</style>";
            desc += ".";
            return desc;
        }
        protected override string GetLoreString(string langid = null) => "Order: Taser\n\nTracking Number: 717***********\nEstimated Delivery: 11/14/2056\nShipping Method: Standard\nShipping Address: 94123 Bldg. 201, Fort Mason\nShipping Details:\n\nYou say you can fix 'em? These tasers are very very faulty; got a few of my officers hurt they did. They fire, but the don't do nothin' like, 99% of the time! My department is running low on money, so I expect a good deal for these!";

        public Taser() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/taser_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Taser.prefab");
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
            orig(self,di);

            if(di == null || di.rejected || !di.attacker || di.attacker == self.gameObject) return;

            var cb = di.attacker.GetComponent<CharacterBody>();
            if(cb) {
                var icnt = GetCount(cb);
                if(icnt < 1) return;
                var proc = cb.master ? Util.CheckRoll(procChance,cb.master) : Util.CheckRoll(procChance);
                if(proc) {
                    self.body.AddTimedBuff(RoR2Content.Buffs.Entangle, procTime + (icnt-1) * stackTime);
                }
            }
        }
    }
}
