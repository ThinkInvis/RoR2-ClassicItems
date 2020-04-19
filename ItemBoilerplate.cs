using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace ThinkInvisible.ClassicItems
{
    public abstract class ItemBoilerplate
    {
        public bool itemIsEquipment {get; protected set;}
        public bool itemEnabled {get; private set;}

        private ConfigEntry<bool> cfgEnable;

        public string modelPathName {get; protected set;}
        public string iconPathName {get; protected set;}
        public string itemName {get; protected set;}
        public string itemShortText {get; protected set;}
        public string itemLongText {get; protected set;}
        public string itemLoreText {get; protected set;}

        protected ItemTag[] _itemTags;
        public ReadOnlyCollection<ItemTag> itemTags {get; private set;}
        public ItemTier itemTier {get; protected set;}

        public int itemCooldown {get; protected set;}
        public bool itemEnigmable {get; protected set;}
            
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

        public void SetupConfig(ConfigFile cfl) {
            if(configDone) {
                Debug.LogError("ClassicItems: something tried to setup config for an item twice");
                return;
            }
            configDone = true;
                
            cfgEnable = cfl.Bind(new ConfigDefinition("Items." + itemCodeName, "Enable"), true, new ConfigDescription(
            "If false, the item will not appear ingame, nor will any relevant IL patches or hooks be added."));
            itemEnabled = cfgEnable.Value;
            SetupConfigInner(cfl);
        }
        
        protected abstract void SetupAttributesInner();
            
        public void SetupAttributes() {
            if(attributesDone) {
                Debug.LogError("ClassicItems: something tried to setup attributes for an item twice");
                return;
            }
            attributesDone = true;

            SetupAttributesInner();

            if(itemIsEquipment) {
                regDefEqp = new EquipmentDef {
                    pickupModelPath = "@ClassicItems:Assets/ClassicItems/models/" + modelPathName,
                    pickupIconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName,
                    nameToken = itemName,
                    pickupToken = itemShortText,
                    descriptionToken = itemLongText,
                    loreToken = itemLoreText,
                    cooldown = itemCooldown,
                    enigmaCompatible = itemEnigmable,
                    canDrop = true
                };
                regEqp = new CustomEquipment(regDefEqp, new ItemDisplayRule[0]);
                regIndexEqp = ItemAPI.Add(regEqp);
            } else {
                regDef = new ItemDef {
                    tier = itemTier,
                    pickupModelPath = "@ClassicItems:Assets/ClassicItems/models/" + modelPathName,
                    pickupIconPath = "@ClassicItems:Assets/ClassicItems/icons/" + iconPathName,
                    nameToken = itemName,
                    pickupToken = itemShortText,
                    descriptionToken = itemLongText,
                    loreToken = itemLoreText,
                    tags = _itemTags
                };
                itemTags = Array.AsReadOnly(_itemTags);
                regItem = new CustomItem(regDef, new ItemDisplayRule[0]);
                regIndex = ItemAPI.Add(regItem);
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
        public int GetCount(CharacterBody body) {
            if(itemIsEquipment) {
                Debug.LogError("ClassicItems: something tried to call GetCount for an equipment item");
                return 0;
            }
            return body?.inventory?.GetItemCount(regIndex) ?? 0;
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
