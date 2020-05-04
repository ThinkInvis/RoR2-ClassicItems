using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ThinkInvisible.ClassicItems {
    public abstract class ItemBoilerplate<T>:ItemBoilerplate where T : ItemBoilerplate<T> {
        public static T instance {get;private set;}

        public ItemBoilerplate() {
            if(instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBoilerplate was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class ItemBoilerplate {
        public bool itemIsEquipment {get; protected set;} = false;
        public bool itemAIBDefault {get;protected set;} = false;

        public bool itemEnabled {get; private set;}
        public bool itemAIB {get;private set;}

        private ConfigEntry<bool> cfgEnable;
        private ConfigEntry<bool> cfgAIB;

        public string modelPathName {get; protected set;}
        public string iconPathName {get; protected set;}

        protected List<ItemTag> _itemTags;
        public ReadOnlyCollection<ItemTag> itemTags {get; private set;}
        public ItemTier itemTier {get; protected set;}

        public int eqpCooldown {get; protected set;}
        public bool eqpEnigmable {get; protected set;} = true;
        public bool eqpIsLunar {get; protected set;} = false;
            
        public bool configDone {get; private set;} = false;
        public bool attributesDone {get; private set;} = false;
        public bool behaviorDone {get; private set;} = false;

        public abstract string itemCodeName {get;}

        public ItemIndex regIndex {get; private set;}
        public ItemDef regDef {get; private set;}
        public CustomItem regItem {get; private set;}

        public EquipmentIndex regIndexEqp {get; private set;}
        public EquipmentDef regDefEqp {get; private set;}
        public CustomEquipment regEqp {get; private set;}

        protected abstract void SetupConfigInner(ConfigFile cfl);

        public ItemBoilerplate() {
            if(itemIsEquipment) _itemTags = new List<ItemTag>();
        }

        public void SetupConfig(ConfigFile cfl) {
            if(configDone) {
                Debug.LogError("ClassicItems: something tried to setup config for an item twice");
                return;
            }
            configDone = true;

            SetupConfigInner(cfl);
                
            cfgEnable = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Enable"), true, new ConfigDescription(
            "If false, the item will not appear ingame, nor will any relevant IL patches or hooks be added."));
            itemEnabled = cfgEnable.Value;

            if(!itemIsEquipment) {
                cfgAIB = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "AIBlacklist"), itemAIBDefault, new ConfigDescription(
                "If true, the item will not be given to enemies by Evolution nor in the arena map, and it will not be found by Scavengers."));
                itemAIB = cfgAIB.Value;
            }

        }
        
        protected abstract void SetupAttributesInner();
            
        private static Dictionary<ItemTier, string> modelNameMap = new Dictionary<ItemTier, string>{
            {ItemTier.Boss, "BossCard"},
            {ItemTier.Lunar, "LunarCard"},
            {ItemTier.Tier1, "CommonCard"},
            {ItemTier.Tier2, "UncommonCard"},
            {ItemTier.Tier3, "RareCard"}
        };

        public void SetupAttributes() {
            if(attributesDone) {
                Debug.LogError("ClassicItems: something tried to setup attributes for an item twice");
                return;
            }
            attributesDone = true;

            var gNameToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_NAME";
            var gPickupToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_PICKUP";
            var gDescriptionToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_DESC";
            var gLoreToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_LORE";

            SetupAttributesInner();

            if(itemIsEquipment) {
                regDefEqp = new EquipmentDef {
                    name = "CI"+itemCodeName,
                    pickupModelPath = "@ClassicItems:Assets/ClassicItems/models/EqpCard.prefab" ,
                    pickupIconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName,
                    nameToken = gNameToken,
                    pickupToken = gPickupToken,
                    descriptionToken = gDescriptionToken,
                    loreToken = gLoreToken,
                    cooldown = eqpCooldown,
                    enigmaCompatible = eqpEnigmable,
                    isLunar = eqpIsLunar,
                    canDrop = true
                };
                regEqp = new CustomEquipment(regDefEqp, new ItemDisplayRuleDict(null));
                regIndexEqp = ItemAPI.Add(regEqp);
            } else {
                if(itemAIB) _itemTags.Add(ItemTag.AIBlacklist);
                var iarr = _itemTags.ToArray();
                regDef = new ItemDef {
                    name = "CI"+itemCodeName,
                    tier = itemTier,
                    pickupModelPath = "@ClassicItems:Assets/ClassicItems/models/" + modelNameMap[itemTier] + ".prefab",
                    pickupIconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName,
                    nameToken = gNameToken,
                    pickupToken = gPickupToken,
                    descriptionToken = gDescriptionToken,
                    loreToken = gLoreToken,
                    tags = iarr
                };

                itemTags = Array.AsReadOnly(iarr);
                regItem = new CustomItem(regDef, new ItemDisplayRuleDict(null));
                regIndex = ItemAPI.Add(regItem);
            }
        }

        public void RegLang(string name, string pickup, string desc, string lore, string langid = null) {
            var gNameToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_NAME";
            var gPickupToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_PICKUP";
            var gDescriptionToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_DESC";
            var gLoreToken = "CLASSICITEMS_" + itemCodeName.ToUpper() + "_LORE";

            if(langid == null) {
                LanguageAPI.Add(gNameToken, name);
                LanguageAPI.Add(gPickupToken, pickup);
                LanguageAPI.Add(gDescriptionToken, desc);
                LanguageAPI.Add(gLoreToken, lore);
            } else {
                LanguageAPI.Add(gNameToken, name, langid);
                LanguageAPI.Add(gPickupToken, pickup, langid);
                LanguageAPI.Add(gDescriptionToken, desc, langid);
                LanguageAPI.Add(gLoreToken, lore, langid);
            }
        }

        public void SetupBehavior() {
            if(behaviorDone) {
                Debug.LogError("ClassicItems: something tried to setup behavior for an item twice");
                return;
            }
            behaviorDone = true;

            SetupBehaviorInner();
        }

        protected abstract void SetupBehaviorInner();
        
        public int GetCount(Inventory inv) {
            if(itemIsEquipment) {
                Debug.LogError("ClassicItems: something tried to call GetCount for an equipment item");
                return 0;
            }
            return inv?.GetItemCount(regIndex) ?? 0;
        }
        public int GetCount(CharacterMaster chrm) {
            if(itemIsEquipment) {
                Debug.LogError("ClassicItems: something tried to call GetCount for an equipment item");
                return 0;
            }
            return chrm?.inventory?.GetItemCount(regIndex) ?? 0;
        }
        public int GetCount(CharacterBody body) {
            if(itemIsEquipment) {
                Debug.LogError("ClassicItems: something tried to call GetCount for an equipment item");
                return 0;
            }
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

        public bool HasEqp(Inventory inv, bool inMain = true, bool inAlt = false) {
            if(!itemIsEquipment) {
                Debug.LogError("ClassicItems: something tried to call HasEqp for a non-equipment item");
                return false;
            }
            return (inMain && (inv?.currentEquipmentIndex ?? EquipmentIndex.None) == regIndexEqp) || (inAlt && (inv?.alternateEquipmentIndex ?? EquipmentIndex.None) == regIndexEqp);
        }
        public bool HasEqp(CharacterBody body) {
            if(!itemIsEquipment) {
                Debug.LogError("ClassicItems: something tried to call HasEqp for a non-equipment item");
                return false;
            }
            return (body?.equipmentSlot?.equipmentIndex ?? EquipmentIndex.None) == regIndexEqp;
        }
    }
}
