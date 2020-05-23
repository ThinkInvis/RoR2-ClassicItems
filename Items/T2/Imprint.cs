using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;

namespace ThinkInvisible.ClassicItems {
    public class Imprint : Item<Imprint> {
        public override string displayName => "Filial Imprinting";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Any});

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Base cooldown between Filial Imprinting buffs, in seconds.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseCD {get;private set;} = 20f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Multiplicative cooldown decrease per additional stack of Filial Imprinting. Caps at a minimum of baseDuration.", AutoItemConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float stackCDreduc {get;private set;} = 0.1f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Duration of buffs applied by Filial Imprinting.", AutoItemConfigFlags.None, 0f, float.MaxValue)]
        public float baseDuration {get;private set;} = 5f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Extra health regen multiplier applied by Filial Imprinting.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float regenMod {get;private set;} = 1f;
        
        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Extra move speed multiplier applied by Filial Imprinting.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float speedMod {get;private set;} = 0.5f;

        [AutoUpdateEventInfo(AutoUpdateEventFlags.InvalidateDescToken)]
        [AutoItemConfig("Extra attack speed multiplier applied by Filial Imprinting.", AutoItemConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float attackMod {get;private set;} = 0.5f;

        protected override string NewLangName(string langid = null) => displayName;
        protected override string NewLangPickup(string langid = null) => "Hatch a strange creature who drops buffs periodically.";
        protected override string NewLangDesc(string langid = null) => "Every <style=cIsUtility>" + baseCD.ToString("N0") + " seconds</style> <style=cStack>(-" + Pct(stackCDreduc) + " per stack, min. " + baseDuration.ToString("N0") + " s)</style>, gain <style=cIsHealing>+" + Pct(regenMod) + " health regen</style> OR <style=cIsUtility>+" + Pct(speedMod) + " move speed</style> OR <style=cIsDamage>+" + Pct(attackMod) + " attack speed</style> for <style=cIsUtility>" + baseDuration.ToString("N0") + " seconds</style>.";
        protected override string NewLangLore(string langid = null) => "A relic of times long past (ClassicItems mod)";
        
        public BuffIndex attackBuff {get; private set;}
        public BuffIndex speedBuff {get; private set;}
        public BuffIndex healBuff {get; private set;}

        public Imprint() {
            onAttrib += (tokenIdent, namePrefix) => {
                var attackBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = Color.red,
                    canStack = false,
                    isDebuff = false,
                    name = namePrefix + "ImprintAttack",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/Imprint_icon.png"
                });
                attackBuff = R2API.BuffAPI.Add(attackBuffDef);
                var speedBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = Color.cyan,
                    canStack = false,
                    isDebuff = false,
                    name = namePrefix + "ImprintSpeed",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/Imprint_icon.png"
                });
                speedBuff = R2API.BuffAPI.Add(speedBuffDef);
                var healBuffDef = new R2API.CustomBuff(new BuffDef {
                    buffColor = Color.green,
                    canStack = false,
                    isDebuff = false,
                    name = namePrefix + "ImprintHeal",
                    iconPath = "@ClassicItems:Assets/ClassicItems/icons/Imprint_icon.png"
                });
                healBuff = R2API.BuffAPI.Add(healBuffDef);
            };
        }

        protected override void LoadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBInventoryChanged;
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }

        protected override void UnloadBehavior() {
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBInventoryChanged;
            GetStatCoefficients -= Evt_TILER2GetStatCoefficients;
        }

        private void Evt_TILER2GetStatCoefficients(CharacterBody sender, StatHookEventArgs args) {
            if(sender.HasBuff(healBuff)) args.regenMultAdd += regenMod;
            if(sender.HasBuff(attackBuff)) args.attackSpeedMultAdd += attackMod;
            if(sender.HasBuff(speedBuff)) args.moveSpeedMultAdd += speedMod;
        }
        
        private void On_CBInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            var cpt = self.GetComponent<ImprintComponent>();
            if(!cpt) cpt = self.gameObject.AddComponent<ImprintComponent>();
            cpt.count = GetCount(self);
            cpt.ownerBody = self;
        }
    }

    public class ImprintComponent : MonoBehaviour {
        public int count = 0;
        public CharacterBody ownerBody;
        private float stopwatch = 0f;

        private static readonly BuffIndex[] rndBuffs = {
            Imprint.instance.attackBuff,
            Imprint.instance.speedBuff,
            Imprint.instance.healBuff
        };

        private void FixedUpdate() {
            if(count <= 0) return;
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Mathf.Max(Imprint.instance.baseCD * Mathf.Pow(1f - Imprint.instance.stackCDreduc, count - 1), Imprint.instance.baseDuration);
                ownerBody.AddTimedBuff(Imprint.instance.itemRng.NextElementUniform(rndBuffs), Imprint.instance.baseDuration);
            }
        }
    }
}
