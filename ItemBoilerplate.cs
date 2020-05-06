using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ThinkInvisible.ClassicItems.MiscUtil;

namespace ThinkInvisible.ClassicItems {

    public abstract class Item<T>:Item where T : Item<T> {
        public static T instance {get;private set;}

        public Item() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Item was instantiated twice");
            this.itemCodeName = typeof(T).Name;
            instance = this as T;
        }
    }
    public abstract class Equipment<T>:Equipment where T : Equipment<T> {
        public static T instance {get;private set;}

        public Equipment() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate/Equipment was instantiated twice");
            this.itemCodeName = typeof(T).Name;
            instance = this as T;
        }
    }

    public abstract class Item : ItemBoilerplate {
        public ItemIndex regIndex {get; private set;}
        public ItemDef regDef {get; private set;}
        public CustomItem regItem {get; private set;}

        public abstract ItemTier itemTier {get;}

        public virtual bool itemAIBDefault => false;
        private ConfigEntry<bool> cfgAIB;
        public bool itemAIB {get;private set;}

        public virtual ReadOnlyCollection<ItemTag> itemTags {get; private set;}
        

        public override void SetupConfig(ConfigFile cfl) {
            if(configDone) {
                Debug.LogError("ClassicItems: something tried to setup config for an item twice");
                return;
            }
            configDone = true;

            this.BindAll(cfl, "Items." + itemCodeName);

            SetupConfigInner(cfl);

            cfgEnable = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Enable"), true, new ConfigDescription(
            "If false, " + displayName + " will not appear ingame, nor will any relevant IL patches or hooks be added."));
            enabled = cfgEnable.Value;

            cfgAIB = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "AIBlacklist"), itemAIBDefault, new ConfigDescription(
            "If true, " + displayName + " will not be given to enemies by Evolution nor in the arena map, and it will not be found by Scavengers."));
            itemAIB = cfgAIB.Value;
        }
        public override void SetupAttributes(string modTokenIdent, string modCNamePrefix = "") {
            if(attributesDone) {
                Debug.LogError("ClassicItems: something tried to setup attributes for an item twice");
                return;
            }
            attributesDone = true;

            nameToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_NAME";
            pickupToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_PICKUP";
            descToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_DESC";
            loreToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_LORE";

            SetupAttributesInner();
            
            var _itemTags = new List<ItemTag>(itemTags);
            if(itemAIB) _itemTags.Add(ItemTag.AIBlacklist);
            var iarr = _itemTags.ToArray();
            regDef = new ItemDef {
                name = modCNamePrefix+itemCodeName,
                tier = itemTier,
                pickupModelPath = modelPathName,
                pickupIconPath = iconPathName,
                nameToken = this.nameToken,
                pickupToken = this.pickupToken,
                descriptionToken = this.descToken,
                loreToken = this.loreToken,
                tags = iarr
            };

            itemTags = Array.AsReadOnly(iarr);
            regItem = new CustomItem(regDef, new ItemDisplayRuleDict(null));
            regIndex = ItemAPI.Add(regItem);
        }

        public int GetCount(Inventory inv) {
            return inv?.GetItemCount(regIndex) ?? 0;
        }
        public int GetCount(CharacterMaster chrm) {
            return chrm?.inventory?.GetItemCount(regIndex) ?? 0;
        }
        public int GetCount(CharacterBody body) {
            return body?.inventory?.GetItemCount(regIndex) ?? 0;
        }
        public int GetCountOnDeploys(CharacterMaster master) {
            if(master == null) return 0;
            var dplist = master.GetFieldValue<List<DeployableInfo>>("deployablesList");
            if(dplist == null) return 0;
            int count = 0;
            foreach(DeployableInfo d in dplist) {
                Debug.Log(d.deployable.name);
                count += GetCount(d.deployable.gameObject.GetComponent<Inventory>());
            }
            return count;
        }
    }
    public abstract class Equipment : ItemBoilerplate {
        public EquipmentIndex regIndex {get; private set;}
        public EquipmentDef regDef {get; private set;}
        public CustomEquipment regEqp {get; private set;}

        public virtual float eqpCooldown => 45f; //TODO: allow this to change (other things too but this first), use config?, add a getter function to update ingame cooldown properly if in use
        public virtual bool eqpEnigmable => true;
        public virtual bool eqpIsLunar => false;

        public override void SetupConfig(ConfigFile cfl) {
            if(configDone) {
                Debug.LogError("ClassicItems: something tried to setup config for an equipment twice");
                return;
            }
            configDone = true;

            this.BindAll(cfl, "Items." + itemCodeName);

            SetupConfigInner(cfl);

            cfgEnable = cfl.Bind(new ConfigDefinition("Equipments." + itemCodeName, "Enable"), true, new ConfigDescription(
            "If false, " + displayName + " will not appear ingame, nor will any relevant IL patches or hooks be added."));
            enabled = cfgEnable.Value;
        }
        public override void SetupAttributes(string modTokenIdent, string modCNamePrefix = "") {
            if(attributesDone) {
                Debug.LogError("ClassicItems: something tried to setup attributes for an equipment twice");
                return;
            }
            attributesDone = true;

            nameToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_NAME";
            pickupToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_PICKUP";
            descToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_DESC";
            loreToken = modTokenIdent + "_" + itemCodeName.ToUpper() + "_LORE";

            SetupAttributesInner();

            regDef = new EquipmentDef {
                name = modCNamePrefix+itemCodeName,
                pickupModelPath = modelPathName,
                pickupIconPath = iconPathName,
                nameToken = this.nameToken,
                pickupToken = this.pickupToken,
                descriptionToken = this.descToken,
                loreToken = this.loreToken,
                cooldown = eqpCooldown,
                enigmaCompatible = eqpEnigmable,
                isLunar = eqpIsLunar,
                canDrop = true
            };
            regEqp = new CustomEquipment(regDef, new ItemDisplayRuleDict(null));
            regIndex = ItemAPI.Add(regEqp);
        }
        
        public bool HasEqp(Inventory inv, bool inMain = true, bool inAlt = false) {
            return (inMain && (inv?.currentEquipmentIndex ?? EquipmentIndex.None) == regIndex) || (inAlt && (inv?.alternateEquipmentIndex ?? EquipmentIndex.None) == regIndex);
        }
        public bool HasEqp(CharacterBody body) {
            return (body?.equipmentSlot?.equipmentIndex ?? EquipmentIndex.None) == regIndex;
        }
    }

    public abstract class ItemBoilerplate : IAutoItemCfg {
        public string nameToken {get; private protected set;}
        public string pickupToken {get; private protected set;}
        public string descToken {get; private protected set;}
        public string loreToken {get; private protected set;}

        public bool enabled {get; protected set;}

        protected ConfigEntry<bool> cfgEnable;

        public string modelPathName {get; protected set;}
        public string iconPathName {get; protected set;}
            
        public bool configDone {get; private protected set;} = false;
        public bool attributesDone {get; private protected set;} = false;
        public bool behaviorDone {get; private protected set;} = false;

        public string itemCodeName {get; private protected set;}
        /// <summary>The item's display name in the mod's default language. Will be used in config files; will also be used in RegLang if called with no language parameter.</summary>
        public abstract string displayName {get;} 


        public virtual void SetupConfigInner(ConfigFile cfl) {}
        
        public Dictionary<string, ConfigEntryBase> autoItemCfgs {get;} = new Dictionary<string, ConfigEntryBase>();

        public abstract void SetupConfig(ConfigFile cfl);
        
        public abstract void SetupAttributesInner();


        public abstract void SetupAttributes(string modTokenIdent, string modCNamePrefix = "");

        public void RegLang(string name, string pickup, string desc, string lore, string langid) {
            if(langid == null) {
                LanguageAPI.Add(nameToken, name);
                LanguageAPI.Add(pickupToken, pickup);
                LanguageAPI.Add(descToken, desc);
                LanguageAPI.Add(loreToken, lore);
            } else {
                LanguageAPI.Add(nameToken, name, langid);
                LanguageAPI.Add(pickupToken, pickup, langid);
                LanguageAPI.Add(descToken, desc, langid);
                LanguageAPI.Add(loreToken, lore, langid);
            }
        }

        public void RegLang(string pickup, string desc, string lore) {
            RegLang(displayName, pickup, desc, lore, null);
        }

        public void SetupBehavior() {
            if(behaviorDone) {
                Debug.LogError("ClassicItems: something tried to setup behavior for an item twice");
                return;
            }
            behaviorDone = true;

            SetupBehaviorInner();
        }

        public abstract void SetupBehaviorInner();

        public static FilingDictionary<ItemBoilerplate> InitAll() {
            FilingDictionary<ItemBoilerplate> f = new FilingDictionary<ItemBoilerplate>();
            foreach(Type type in Assembly.GetCallingAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ItemBoilerplate)))) {
                f.Add((ItemBoilerplate)Activator.CreateInstance(type));
            }
            return f; //:regional_indicator_f:
        }
    }
}
