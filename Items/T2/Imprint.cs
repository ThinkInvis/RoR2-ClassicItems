﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static R2API.RecalculateStatsAPI;
using UnityEngine.Networking;
using R2API;

namespace ThinkInvisible.ClassicItems {
    public class Imprint : Item<Imprint> {
        public override string displayName => "Filial Imprinting";
		public override ItemTier itemTier => ItemTier.Tier2;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Any});

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Base cooldown between Filial Imprinting buffs, in seconds.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float baseCD {get;private set;} = 20f;

        [AutoConfigRoOSlider("x{0:N1}", 0f, 0.999f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Multiplicative cooldown decrease per additional stack of Filial Imprinting. Caps at a minimum of baseDuration.", AutoConfigFlags.PreventNetMismatch, 0f, 0.999f)]
        public float stackCDreduc {get;private set;} = 0.1f;

        [AutoConfigRoOSlider("{0:N1} s", 0f, 100f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Duration of buffs applied by Filial Imprinting.", AutoConfigFlags.None, 0f, float.MaxValue)]
        public float baseDuration {get;private set;} = 5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra health regen multiplier applied by Filial Imprinting.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float regenMod {get;private set;} = 1f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra move speed multiplier applied by Filial Imprinting.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float speedMod {get;private set;} = 0.5f;

        [AutoConfigRoOSlider("{0:P0}", 0f, 10f)]
        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Extra attack speed multiplier applied by Filial Imprinting.", AutoConfigFlags.PreventNetMismatch, 0f, float.MaxValue)]
        public float attackMod {get;private set;} = 0.5f;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Hatch a strange creature who drops buffs periodically.";
        protected override string GetDescString(string langid = null) => "Every <style=cIsUtility>" + baseCD.ToString("N0") + " seconds</style> <style=cStack>(-" + Pct(stackCDreduc) + " per stack, minimum of " + baseDuration.ToString("N0") + " seconds)</style>, gain <style=cIsHealing>+" + Pct(regenMod) + " health regen</style> OR <style=cIsUtility>+" + Pct(speedMod) + " move speed</style> OR <style=cIsDamage>+" + Pct(attackMod) + " attack speed</style> for <style=cIsUtility>" + baseDuration.ToString("N0") + " seconds</style>.";
        protected override string GetLoreString(string langid = null) => "Order: Filial Imprinting\n\nTracking Number: 940***********\nEstimated Delivery: 4/14/2056\nShipping Method: Priority\nShipping Address: Row E, Mutation Hold, Earth\nShipping Details:\n\nYou didn't tell me the roe was FERTILIZED. Good lord! Anyways, one of the suckers actually hatched, and have been nothing but friendly to me. Filial imprinting, perhaps.\n\nI quite like the little guy. He's almost dog-like in how affectionate he is to me. I.. have begun to care a lot about the thing. I've been feeding him nutapples and he seems to enjoy it, but I doubt that's very nutritious. Suggestions?";
        
        public BuffDef attackBuff {get; private set;}
        public BuffDef speedBuff {get; private set;}
        public BuffDef healBuff {get; private set;}

        public Imprint() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/imprint_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Imprint.prefab");
        }

        public override void SetupAttributes() {
            base.SetupAttributes();
            attackBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackBuff.buffColor = Color.red;
            attackBuff.canStack = false;
            attackBuff.isDebuff = false;
            attackBuff.name = modInfo.shortIdentifier + "ImprintAttack";
            attackBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/Imprint_icon.png");
            ContentAddition.AddBuffDef(attackBuff);
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff.buffColor = Color.cyan;
            speedBuff.canStack = false;
            speedBuff.isDebuff = false;
            speedBuff.name = modInfo.shortIdentifier + "ImprintSpeed";
            speedBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/Imprint_icon.png");
            ContentAddition.AddBuffDef(speedBuff);
            healBuff = ScriptableObject.CreateInstance<BuffDef>();
            healBuff.buffColor = Color.green;
            healBuff.canStack = false;
            healBuff.isDebuff = false;
            healBuff.name = modInfo.shortIdentifier + "ImprintHeal";
            healBuff.iconSprite = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/Imprint_icon.png");
            ContentAddition.AddBuffDef(healBuff);
        }

        public override void SetupBehavior() {
            base.SetupBehavior();
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by UnityEngine")]
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
