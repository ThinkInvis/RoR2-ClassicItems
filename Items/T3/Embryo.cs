using Mono.Cecil.Cil;
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
using R2API.Utils;
using System.Reflection;

namespace ThinkInvisible.ClassicItems {
    //[Obsolete("Unstable as of CI 5.0.0; currently undergoing rewrite.")]
    public class Embryo : Item<Embryo> {
        public abstract class EmbryoHook {
            public abstract EquipmentDef targetEquipment { get; }
            public abstract string configDisplayName { get; }
            public virtual string descriptionAppendToken { get; } = null;
            public bool isInstalled { get; private set; } = false;
            public bool isEnabled { get; private set; } = true;

            public EmbryoHook() { }

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
                if(equipmentDef != this.targetEquipment) return retv;
                var proc = CheckLastEmbryoProc(self);
                if(proc > 0) {
                    for(var i = 0; i < proc; i++)
                        retv |= orig(self, equipmentDef); //return true if any activation was successful
                }
                return retv;
            }
        }

        public override string displayName => "Beating Embryo";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.EquipmentRelated});

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance of triggering an equipment twice. Stacks additively.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 30f;

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("If true, proc chance past 100% can triple-, quadruple-, etc.-proc.", AutoConfigFlags.None)]
        public bool canMultiproc { get; private set; } = true;

        internal readonly List<EmbryoHook> allHooks = new List<EmbryoHook>();

        internal Dictionary<EmbryoHook, bool> hooksEnabled { get; } = new Dictionary<EmbryoHook, bool>();

        public Embryo() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/embryo_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Embryo.prefab");

            InitAllEmbryoHooksInMyAssembly();
        }

        public static void InitAllEmbryoHooksInMyAssembly()  {
            var callingAssembly = Assembly.GetCallingAssembly();
            foreach(Type type in callingAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(EmbryoHook)))) {
                var newModule = (EmbryoHook)Activator.CreateInstance(type, nonPublic: true);
                Embryo.instance.allHooks.Add(newModule);
                Embryo.instance.hooksEnabled.Add(newModule, true);
            }
        }

        public override void SetupConfig() {
            base.SetupConfig();
        }

        public override void SetupLate() {
            base.SetupLate();

            foreach(var hook in allHooks) {
                hook.SetupConfig();
            }

            Bind(typeof(Embryo).GetPropertyCached(nameof(hooksEnabled)), ClassicItemsPlugin.cfgFile, "ClassicItems", "Items.Embryo.SubEnable", new AutoConfigAttribute($"<AIC.DictKeyProp.{nameof(EmbryoHook.configDisplayName)}>", "If false, this equipment's Beating Embryo functionality will be disabled.", AutoConfigFlags.BindDict | AutoConfigFlags.PreventNetMismatch));

            ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(hooksEnabled)) {
                    var hook = (EmbryoHook)args.target.boundKey;
                    if((bool)args.newValue) {
                        hook.Enable();
                    } else {
                        hook.Disable();
                    }
                }
            };
        }

        public static int InjectLastProcCheckIL(ILCursor c) {
            int boost = 0;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = CheckLastEmbryoProc(slot);
            });
            return boost;
        }

        public static (int boost, TComponent cpt) InjectLastProcCheckIL<TComponent>(ILCursor c) where TComponent : MonoBehaviour {
            int boost = 0;
            TComponent cpt = null;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<EquipmentSlot>>((slot) => {
                boost = CheckLastEmbryoProc(slot);
                cpt = slot.characterBody?.GetComponentInChildren<TComponent>();
            });
            return (boost, cpt);
        }

        public static (int boost, TComponent cpt) InjectLastProcCheckDirect<TComponent>(EquipmentSlot slot) where TComponent : MonoBehaviour {
            int boost = CheckLastEmbryoProc(slot);
            TComponent cpt = null;
            if(slot && slot.characterBody)
                cpt = slot.characterBody.gameObject.GetComponent<TComponent>();
            return (boost, cpt);
        }

        public static int CheckLastEmbryoProc(CharacterBody body) {
            if(!body) return 0;
            var cpt = body.GetComponent<EmbryoTrackLastComponent>();
            if(!cpt) return 0;
            return cpt.lastBoost;
        }

        public static int CheckLastEmbryoProc(EquipmentSlot slot) {
            if(!slot || !slot.characterBody) return 0;
            var cpt = slot.characterBody.GetComponent<EmbryoTrackLastComponent>();
            if(!cpt) return 0;
            return cpt.lastBoost;
        }

        private int _CheckEmbryoProc(CharacterBody body) {
            if(!this.enabled) return 0;
            if(!canMultiproc)
                return Util.CheckRoll(GetCount(body) * procChance, body?.master) ? 1 : 0;
            var totalChance = GetCount(body) * procChance;
            return Mathf.FloorToInt(totalChance) + (Util.CheckRoll((totalChance % 100f) / 100, body?.master) ? 1 : 0);
        }

        public static int CheckEmbryoProc(CharacterBody body) {
            return Embryo.instance._CheckEmbryoProc(body);
        }

        private int _CheckEmbryoProc(Inventory inv) {
            if(!this.enabled) return 0;
            if(!canMultiproc)
                return Util.CheckRoll(GetCount(inv) * procChance, inv?.gameObject?.GetComponent<CharacterMaster>()) ? 1 : 0;
            var totalChance = GetCount(inv) * procChance;
            return Mathf.FloorToInt(totalChance) + (Util.CheckRoll((totalChance % 100f) / 100, inv?.gameObject?.GetComponent<CharacterMaster>()) ? 1 : 0);
        }

        public static int CheckEmbryoProc(Inventory inv) {
            return Embryo.instance._CheckEmbryoProc(inv);
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
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        public override void Uninstall() {
            base.Uninstall();

            foreach(var hook in allHooks) {
                hook.Uninstall();
            }

            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.EquipmentSlot.PerformEquipmentAction -= EquipmentSlot_PerformEquipmentAction;
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
            var cpt = self.gameObject.GetComponent<EmbryoTrackLastComponent>();
            if(!cpt)
                self.gameObject.AddComponent<EmbryoTrackLastComponent>();
            foreach(var hook in allHooks) {
                if(!hook.isEnabled) continue;
                hook.AddComponents(self);
            }
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef) {
            int boost = CheckEmbryoProc(self.characterBody);
            var cpt = self.characterBody?.gameObject.GetComponent<EmbryoTrackLastComponent>();
            if(cpt) cpt.lastBoost = boost;

            return orig(self, equipmentDef);
        }
    }

    public class EmbryoTrackLastComponent : MonoBehaviour {
        public int lastBoost = 0;
    }
}
