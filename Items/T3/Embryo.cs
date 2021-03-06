﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using R2API;
using TILER2;
using static TILER2.MiscUtil;

namespace ThinkInvisible.ClassicItems {
    [Obsolete("Unstable as of CI 5.0.0; currently undergoing rewrite.")]
    public class Embryo : Item<Embryo> {
        public abstract class EmbryoHook {
            public abstract EquipmentDef targetEquipment { get; }
            public virtual string descriptionAppendToken { get; } = null;
            public bool isInstalled { get; private set; } = false;
            public bool isEnabled { get; private set; } = true;
            //TODO: reimplement config-based enable/disable

            public EmbryoHook() {
                Embryo.instance.allHooks.Add(this);
            }

            internal void Install() {
                if(this.isInstalled) return;
                InstallHooks();
                this.isInstalled = true;
            }
            internal void Uninstall() {
                if(!this.isInstalled) return;
                UninstallHooks();
                this.isInstalled = false;
            }

            public void Enable() {
                isEnabled = true;
                if(Embryo.instance.enabled)
                    Install();
                Embryo.instance.hooksEnabled[this] = true;
            }
            public void Disable() {
                isEnabled = false;
                Uninstall();
                Embryo.instance.hooksEnabled[this] = false;
            }

            protected internal virtual void SetupConfig() { }
            protected internal virtual void SetupAttributes() { }

            protected internal virtual void AddComponents(CharacterBody body) { }

            protected abstract void InstallHooks();
            protected abstract void UninstallHooks();
        }

        public abstract class SimpleRetriggerEmbryoHook : EmbryoHook {
            public override string descriptionAppendToken => $"EMBRYO_DESC_APPEND_RETRIGGER";

            protected override void InstallHooks() {
                On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            }
            protected override void UninstallHooks() {
                On.RoR2.EquipmentSlot.PerformEquipmentAction -= EquipmentSlot_PerformEquipmentAction;
            }

            private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef) {
                var retv = orig(self, equipmentDef);
                if(equipmentDef == this.targetEquipment)
                    retv = retv | orig(self, equipmentDef); //return true if either activation was successful
                return retv;
            }
        }

        public override string displayName => "Beating Embryo";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.EquipmentRelated});

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance of triggering an equipment twice. Stacks additively.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 30f;

        internal readonly List<EmbryoHook> allHooks = new List<EmbryoHook>();

        [AutoConfig("Controls which equipments work with Beating Embryo.", AutoConfigFlags.BindDict | AutoConfigFlags.PreventNetMismatch)]
        internal Dictionary<EmbryoHook, bool> hooksEnabled { get; } = new Dictionary<EmbryoHook, bool>();

        public Embryo() {
            new EmbryoHooks.CommandMissile();
        }

        public override void SetupConfig() {
            base.SetupConfig();

            ConfigEntryChanged += (sender,args) => {
                var hook = (EmbryoHook)args.target.boundKey;
                if((bool)args.newValue) {
                    hook.Enable();
                } else {
                    hook.Disable();
                }
            };

            foreach(var hook in allHooks)
                hook.SetupConfig();

            this.enabled = false;
        }

        public static bool ILInjectProcCheck(ILCursor c) {
            bool boost = false;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = Embryo.instance.CheckEmbryoProc(slot.characterBody);
            });
            return boost;
        }

        public bool CheckEmbryoProc(CharacterBody body) {
            if(!this.enabled) return false;
            return Util.CheckRoll(Embryo.instance.GetCount(body) * procChance, body?.master);
        }

        public bool CheckEmbryoProc(Inventory inv) {
            ClassicItemsPlugin._logger.LogWarning($"Embryo.CheckProc: enab {this.enabled}, inv {inv}");
            ClassicItemsPlugin._logger.LogWarning($"Count {GetCount(inv)} --> chance {GetCount(inv) * procChance}");
            if(!this.enabled) return false;
            return Util.CheckRoll(GetCount(inv) * procChance, inv?.gameObject?.GetComponent<CharacterMaster>());
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            foreach(var hook in allHooks)
                hook.SetupAttributes();

            LanguageAPI.Add("EMBRYO_DESC_APPEND_RETRIGGER", "\n<style=cStack>Beating Embryo: Activates twice simultaneously.<style>");
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            if(Compat_ItemStats.enabled) {
                Compat_ItemStats.CreateItemStatDef(itemDef,
                    ((count, inv, master) => { return procChance * count; },
                    (value, inv, master) => { return $"Proc Chance: {Pct(value, 1, 1)}"; }
                ));
            }
        }

        protected override string GetNameString(string langid = null) => displayName;        
        protected override string GetPickupString(string langid = null) => $"Equipment has a {Pct(procChance, 0, 1)} chance to deal double the effect.";        
        protected override string GetDescString(string langid = null) => "Upon activating an equipment, adds a <style=cIsUtility>" + Pct(procChance, 0, 1) + "</style> <style=cStack>(+" + Pct(procChance, 0, 1) + " per stack)</style> chance to <style=cIsUtility>double its effects somehow</style>.";        
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";

        public override void Install() {
            base.Install();

            foreach(var hook in allHooks) {
                if(hook.isEnabled)
                    hook.Install();
            }

            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
        }
        public override void Uninstall() {
            base.Uninstall();

            foreach(var hook in allHooks) {
                hook.Uninstall();
            }

            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
        }

        public override void InstallLanguage() {
            base.InstallLanguage();
            foreach(var hook in allHooks) {
                if(hook.descriptionAppendToken == null) continue;
                var oldDescToken = hook.targetEquipment.descriptionToken;
                languageOverlays.Add(LanguageAPI.AddOverlay(oldDescToken, Language.GetString(oldDescToken) + Language.GetString(hook.descriptionAppendToken), Language.currentLanguageName));
            }
        }

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            if(!NetworkServer.active || GetCount(self) < 1) return;
            foreach(var hook in allHooks) {
                if(!hook.isEnabled) continue;
                hook.AddComponents(self);
            }
        }
    }
}
