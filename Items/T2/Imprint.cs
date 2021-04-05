using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.StatHooks;
using UnityEngine.Networking;

namespace ThinkInvisible.ClassicItems {
    public class Imprint : Item<Imprint> {
        public override string displayName => "Filial Imprinting";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Any});

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base cooldown between Filial Imprinting buffs, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseCD {get;private set;} = 20f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplicative cooldown decrease per additional stack of Filial Imprinting. Caps at a minimum of baseDuration.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float stackCDreduc {get;private set;} = 0.1f;
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of buffs applied by Filial Imprinting.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDuration {get;private set;} = 5f;
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra health regen multiplier applied by Filial Imprinting.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float regenMod {get;private set;} = 1f;
        
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra move speed multiplier applied by Filial Imprinting.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float speedMod {get;private set;} = 0.5f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra attack speed multiplier applied by Filial Imprinting.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float attackMod {get;private set;} = 0.5f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Hatch a strange creature who drops buffs periodically.";
        protected override string GetDescString(string langid = null) => "Every <style=cIsUtility>" + baseCD.ToString("N0") + " seconds</style> <style=cStack>(-" + Pct(stackCDreduc) + " per stack, minimum of " + baseDuration.ToString("N0") + " seconds)</style>, gain <style=cIsHealing>+" + Pct(regenMod) + " health regen</style> OR <style=cIsUtility>+" + Pct(speedMod) + " move speed</style> OR <style=cIsDamage>+" + Pct(attackMod) + " attack speed</style> for <style=cIsUtility>" + baseDuration.ToString("N0") + " seconds</style>.";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";
        
        public BuffDef attackBuff {get; private set;}
        public BuffDef speedBuff {get; private set;}
        public BuffDef healBuff {get; private set;}

        public override void SetupAttributes() {
            base.SetupAttributes();
            attackBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackBuff.buffColor = Color.red;
            attackBuff.canStack = false;
            attackBuff.isDebuff = false;
            attackBuff.name = modInfo.shortIdentifier + "ImprintAttack";
            attackBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/Imprint_icon.png");
            R2API.BuffAPI.Add(new R2API.CustomBuff(attackBuff));
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff.buffColor = Color.cyan;
            speedBuff.canStack = false;
            speedBuff.isDebuff = false;
            speedBuff.name = modInfo.shortIdentifier + "ImprintSpeed";
            speedBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/Imprint_icon.png");
            R2API.BuffAPI.Add(new R2API.CustomBuff(speedBuff));
            healBuff = ScriptableObject.CreateInstance<BuffDef>();
            healBuff.buffColor = Color.green;
            healBuff.canStack = false;
            healBuff.isDebuff = false;
            healBuff.name = modInfo.shortIdentifier + "ImprintHeal";
            healBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/icons/Imprint_icon.png");
            R2API.BuffAPI.Add(new R2API.CustomBuff(healBuff));
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
            if(Compat_ItemStats.enabled) {
                Compat_ItemStats.CreateItemStatDef(itemDef,
                    ((count, inv, master) => { return Mathf.Max(baseCD * Mathf.Pow(1f - stackCDreduc, count - 1), baseDuration); },
                    (value, inv, master) => { return $"Buff Interval: {value.ToString("N1")} s"; }
                ));
            }
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBInventoryChanged;
            GetStatCoefficients += Evt_TILER2GetStatCoefficients;
        }

        public override void Uninstall() {
            base.Uninstall();
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
            if(!NetworkServer.active) return;
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

        private static readonly BuffDef[] rndBuffs = {
            Imprint.instance.attackBuff,
            Imprint.instance.speedBuff,
            Imprint.instance.healBuff
        };

        private void FixedUpdate() {
            if(count <= 0) return;
            stopwatch -= Time.fixedDeltaTime;
            if(stopwatch <= 0f) {
                stopwatch = Mathf.Max(Imprint.instance.baseCD * Mathf.Pow(1f - Imprint.instance.stackCDreduc, count - 1), Imprint.instance.baseDuration);
                ownerBody.AddTimedBuff(Imprint.instance.rng.NextElementUniform(rndBuffs), Imprint.instance.baseDuration);
            }
        }
    }
}
