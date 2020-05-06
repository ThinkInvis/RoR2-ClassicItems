using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace ThinkInvisible.ClassicItems {
    public interface IAutoItemCfg {
        //REMINDER: use nameof(property) while accessing, to avoid hardcoded strings. may still cause problems with type changes
        Dictionary<string, ConfigEntryBase> autoItemCfgs {get;}

        //string UpdateLangName();
        //string UpdateLangPickup();
        //string UpdateLangDesc();
        //string UpdateLangLore();
    }

    public static class AutoItemCfg {
        public static void BindAll(this IAutoItemCfg self, ConfigFile cfl, string categoryName) {
            var autoCfgFields = self.GetType().GetProperties().Where(x => x.IsDefined(typeof(AutoItemCfgAttribute), false));
            foreach(var prop in autoCfgFields) {
                var attrib = (AutoItemCfgAttribute)prop.GetCustomAttributes(typeof(AutoItemCfgAttribute), false)[0];

                var genm = typeof(ConfigFile).GetMethods().First(x=>x.Name == "Bind" && x.GetParameters().Length > 0 && x.GetParameters()[0].ParameterType == typeof(ConfigDefinition));

                string cfgName = attrib.name ?? prop.Name;

                if(attrib.avb != null && attrib.avbType != prop.PropertyType)
                    throw new ArgumentException("AutoItemCfg entry " + categoryName + "/" + cfgName + ": property and AcceptableValue types must match (received " + attrib.avbType.Name + " and " + prop.PropertyType.Name + ").");

                var cfe = (ConfigEntryBase)genm.MakeGenericMethod(prop.PropertyType).Invoke(cfl,
                    new[] {new ConfigDefinition(categoryName, cfgName), prop.GetValue(self), new ConfigDescription(attrib.desc,attrib.avb)});

                self.autoItemCfgs.Add(prop.Name, cfe);

                if((attrib.flags & AICFlags.EnableAutoUpdate) == AICFlags.EnableAutoUpdate) {
                    var evh = typeof(ConfigEntry<>).MakeGenericType(prop.PropertyType).GetEvent("SettingChanged");
                    Action<object,EventArgs> lam = (object obj,EventArgs evtArgs) => {
                        Debug.Log("SettingChanged event fired for " + categoryName + "/" + cfgName);
                        Debug.Log("Obj type: " + obj.GetType());
                        Debug.Log("Args type: " + evtArgs.GetType());
                        Debug.Log("New BoxedValue: " + cfe.BoxedValue);
                        if((attrib.flags & AICFlags.DeferUntilEndGame) == AICFlags.DeferUntilEndGame) {
                            throw new NotImplementedException("AICFlags.AUDeferUntilEndGame");
                        } else if((attrib.flags & AICFlags.DeferUntilNextStage) == AICFlags.DeferUntilNextStage) {
                            throw new NotImplementedException("AICFlags.AUDeferUntilNextStage");
                        } else {
                            prop.SetValue(self, cfe.BoxedValue);
                        
                            if((attrib.flags & AICFlags.AUInvalidateNameToken) == AICFlags.AUInvalidateNameToken) throw new NotImplementedException("AICFlags.AUInvalidateNameToken");
                            if((attrib.flags & AICFlags.AUInvalidatePickupToken) == AICFlags.AUInvalidatePickupToken) throw new NotImplementedException("AICFlags.AUInvalidatePickupToken");
                            if((attrib.flags & AICFlags.AUInvalidateDescToken) == AICFlags.AUInvalidateDescToken) throw new NotImplementedException("AICFlags.AUInvalidateDescToken");
                            if((attrib.flags & AICFlags.AUInvalidateLoreToken) == AICFlags.AUInvalidateLoreToken) throw new NotImplementedException("AICFlags.AUInvalidateLoreToken");
                            if((attrib.flags & AICFlags.AUInvalidatePickupModel) == AICFlags.AUInvalidatePickupModel) throw new NotImplementedException("AICFlags.AUInvalidatePickupModel");
                        }
                    };
                    evh.AddEventHandler(cfe, Delegate.CreateDelegate(evh.EventHandlerType, lam.Method));
                }

                if((attrib.flags & AICFlags.ExposeAsConVar) == AICFlags.ExposeAsConVar) {
                    throw new NotImplementedException("AICFlags.ExposeAsConVar");
                }

                if((attrib.flags & AICFlags.NoInitialRead) != AICFlags.NoInitialRead)
                    prop.SetValue(self, cfe.BoxedValue);
            }
        }
    }

    [Flags]
    public enum AICFlags {
        None = 0,
        ///<summary>If UNSET (default): expects acceptableValues to contain 0 or 2 values, which will be added to an AcceptableValueRange. If SET: an AcceptableValueList will be used instead.</summary>
        AVIsList = 1,
        ///<summary>(TODO: WIP/UNTESTED) If SET: when ConfigEntry.SettingChanged fires, the attached property's value will be automatically updated to match config. If UNSET (default): the property will not automatically react to config changes.</summary>
        EnableAutoUpdate = 2,
        ///<summary>(TODO: NYI) If SET: when EnableAutoUpdate triggers an update, the attached item's language registrations for NameToken will be updated.</summary>
        AUInvalidateNameToken = 4,
        ///<summary>(TODO: NYI) If SET: when EnableAutoUpdate triggers an update, the attached item's language registrations for PickupToken will be updated.</summary>
        AUInvalidatePickupToken = 8,
        ///<summary>(TODO: NYI) If SET: when EnableAutoUpdate triggers an update, the attached item's language registrations for DescToken will be updated.</summary>
        AUInvalidateDescToken = 16,
        ///<summary>(TODO: NYI) If SET: when EnableAutoUpdate triggers an update, the attached item's language registrations for LoreToken will be updated.</summary>
        AUInvalidateLoreToken = 32,
        ///<summary>(TODO: NYI) If SET: when EnableAutoUpdate triggers an update, the attached item's pickup and logbook models will be updated and re-postprocessed.</summary>
        AUInvalidatePickupModel = 64,
        ///<summary>(TODO: NYI) If SET: will cache config changes and prevent them from applying to the attached property until the next stage transition.</summary>
        DeferUntilNextStage = 128,
        ///<summary>(TODO: NYI) If SET: will cache config changes and prevent them from applying to the attached property while there is an active run.</summary>
        DeferUntilEndGame = 256,
        ///<summary>(TODO: NYI) If SET: will add a ConVar linked to the attached property and config entry.</summary>
        ExposeAsConVar = 512,
        ///<summary>If SET: will stop the property value from being changed by the initial config read during BindAll.</summary>
        NoInitialRead = 1024,
        ///<summary>(TODO: NYI) If SET: invalidates all supported aspects of the attached item.</summary>
        AUInvalidateAll = AUInvalidateNameToken | AUInvalidatePickupToken | AUInvalidateDescToken | AUInvalidateLoreToken | AUInvalidatePickupModel
    }

    //TODO: AutoItemCfgCollectionAttribute, for e.g. dictionaries

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class AutoItemCfgAttribute : Attribute {

        public string name {get; private set;}
        public string desc {get; private set;} = "";
        public AcceptableValueBase avb {get; private set;} = null;
        public Type avbType {get; private set;} = null;
        public AICFlags flags {get; private set;}
        public AutoItemCfgAttribute(Tuple<string, string> nameAndDesc, AICFlags flags = AICFlags.None, params object[] acceptableValues) : this(nameAndDesc.Item2, flags, acceptableValues) {
            this.name = nameAndDesc.Item1;
        }

        public AutoItemCfgAttribute(string desc, AICFlags flags = AICFlags.None, params object[] acceptableValues) {
            if(acceptableValues.Length > 0) {
                var avList = (flags & AICFlags.AVIsList) == AICFlags.AVIsList;
                if(!avList && acceptableValues.Length != 2) throw new ArgumentException("Range mode for acceptableValues (flag AVIsList not set) requires either 0 or 2 params; received " + acceptableValues.Length + ".\nThe description provided was: \"" + desc + "\".");
                var iType = acceptableValues[0].GetType();
                for(var i = 1; i < acceptableValues.Length; i++) {
                    if(iType != acceptableValues[i].GetType()) throw new ArgumentException("Types of all acceptableValues must match");
                }
                var avbVariety = avList ? typeof(AcceptableValueList<>).MakeGenericType(iType) : typeof(AcceptableValueRange<>).MakeGenericType(iType);
                this.avb = (AcceptableValueBase)Activator.CreateInstance(avbVariety, acceptableValues);
                this.avbType = iType;
            }
            this.desc = desc;
            this.flags = flags;
        }
    }
}
