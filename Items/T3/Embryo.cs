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

namespace ThinkInvisible.ClassicItems {
    public static class EmbryoExtensions {
        public static bool CheckEmbryoProc(this Equipment eqp, CharacterBody body) {
            return Embryo.instance.CheckEmbryoProc(eqp.equipmentDef, body);
        }
    }

    public class Embryo : Item<Embryo> {
        public abstract class EmbryoHook {
            public abstract EquipmentDef targetEquipment { get; }
            public bool isInstalled { get; private set; } = false;
            //TODO: reimplement config-based enable/disable

            public EmbryoHook() {
                Embryo.instance.allGlobalHooks.Add(this);
            }

            public void Install() {
                if(this.isInstalled) return;
                if(Embryo.instance.hookedEquipmentDefs.ContainsKey(targetEquipment))
                    throw new InvalidOperationException("Target EquipmentDef already has an EmbryoHook");
                Embryo.instance._hookedEquipmentDefs.Add(targetEquipment, this);
                InstallHooks();
                this.isInstalled = true;
            }
            public void Uninstall() {
                if(!this.isInstalled) return;
                Embryo.instance._hookedEquipmentDefs.Remove(targetEquipment);
                UninstallHooks();
                this.isInstalled = false;
            }

            public bool CheckProc(CharacterBody body) {
                return Embryo.instance.enabled
                    && this.isInstalled
                    && Util.CheckRoll(Embryo.instance.GetCount(body) * Embryo.instance.procChance, body.master);
            }

            protected internal virtual void SetupConfig() { }
            protected internal virtual void SetupAttributes() { }

            protected internal virtual void AddComponents(CharacterBody body) { }
            protected internal virtual void RemoveComponents(CharacterBody body) { }

            protected abstract void InstallHooks();
            protected abstract void UninstallHooks();
        }

        public override string displayName => "Beating Embryo";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.EquipmentRelated});

        [AutoConfigUpdateActions(AutoConfigUpdateActionTypes.InvalidateLanguage)]
        [AutoConfig("Percent chance of triggering an equipment twice. Stacks additively.", AutoConfigFlags.None, 0f, 100f)]
        public float procChance {get;private set;} = 30f;

        private Dictionary<EquipmentDef, EmbryoHook> _hookedEquipmentDefs = new Dictionary<EquipmentDef, EmbryoHook>();
        public ReadOnlyDictionary<EquipmentDef, EmbryoHook> hookedEquipmentDefs { get; private set; }

        internal List<EmbryoHook> allInternalHooks = new List<EmbryoHook>();
        internal List<EmbryoHook> allGlobalHooks = new List<EmbryoHook>();

        public Embryo() {
            allInternalHooks.Add(new EmbryoHooks.CommandMissile());
            hookedEquipmentDefs = new ReadOnlyDictionary<EquipmentDef, EmbryoHook>(_hookedEquipmentDefs);
        }

        public override void SetupConfig() {
            base.SetupConfig();

            foreach(var hook in allGlobalHooks)
                hook.SetupConfig();
        }

        public bool CheckEmbryoProc(EquipmentDef eqp, CharacterBody body) {
            Embryo.instance.hookedEquipmentDefs.TryGetValue(eqp, out Embryo.EmbryoHook hook);
            return hook?.CheckProc(body) ?? false;
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            foreach(var hook in allGlobalHooks)
                hook.SetupAttributes();
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

            foreach(var hook in allInternalHooks) {
                hook.Install();
            }

            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
        }
        public override void Uninstall() {
            base.Uninstall();

            foreach(var hook in allInternalHooks) {
                hook.Uninstall();
            }

            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
        }

        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            if(!NetworkServer.active || GetCount(self) < 1) return;
            foreach(var hook in allGlobalHooks) {
                hook.AddComponents(self);
            }
        }
    }
}
