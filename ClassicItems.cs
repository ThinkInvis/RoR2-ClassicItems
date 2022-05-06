using BepInEx;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using System;
using TMPro;
using UnityEngine.Networking;
using Path = System.IO.Path;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using System.Runtime.Serialization;
using System.Linq;
using RoR2.ExpansionManagement;

namespace ThinkInvisible.ClassicItems {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI), nameof(LoadoutAPI), nameof(RecalculateStatsAPI))]
    public class ClassicItemsPlugin:BaseUnityPlugin {
        public const string ModVer = "7.0.0";
        public const string ModName = "ClassicItems";
        public const string ModGuid = "com.ThinkInvisible.ClassicItems";

        internal static ConfigFile cfgFile;
        
        internal static FilingDictionary<CatalogBoilerplate> masterItemList = new FilingDictionary<CatalogBoilerplate>();
        
        public class GlobalConfig:AutoConfigContainer {
            [AutoConfig("If true, disables the Rusty Jetpack gravity reduction while Photon Jetpack is active. If false, there shall be yeet.",
                AutoConfigFlags.PreventNetMismatch)]
            public bool coolYourJets {get; private set;} = true;
        }

        public static readonly GlobalConfig globalConfig = new GlobalConfig();

        public static BuffDef freezeBuff {get;private set;}
        public static BuffDef fearBuff {get;private set;}

        internal static BepInEx.Logging.ManualLogSource _logger;

        public static AssetBundle resources { get; private set; }

        private void Awake() {
            _logger = Logger;

            Logger.LogDebug("Performing plugin setup:");

            Logger.LogDebug("Loading assets...");
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ClassicItems.classicitems_assets")) {
                resources = AssetBundle.LoadFromStream(stream);
            }

            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);
            
            Logger.LogDebug("Loading global configs...");

            globalConfig.BindAll(cfgFile, "ClassicItems", "Global");

            Logger.LogDebug("Instantiating item classes...");
            masterItemList = T2Module.InitAll<CatalogBoilerplate>(new T2Module.ModInfo {
                displayName = "Classic Items",
                longIdentifier = "ClassicItems",
                shortIdentifier = "CI",
                mainConfigFile = cfgFile
            });

            Logger.LogDebug("Loading item configs...");
            foreach(CatalogBoilerplate x in masterItemList) {
                x.SetupConfig();
            }

            Logger.LogDebug("Registering item attributes...");
            foreach(CatalogBoilerplate x in masterItemList) {
                x.SetupAttributes();
            }

            Logger.LogDebug("Registering shared buffs...");
            //used only for purposes of Death Mark; applied by Permafrost and Snowglobe
            freezeBuff = ScriptableObject.CreateInstance<BuffDef>();
            freezeBuff.buffColor = Color.cyan;
            freezeBuff.canStack = false;
            freezeBuff.isDebuff = true;
            freezeBuff.name = "CIFreeze";
            freezeBuff.iconSprite = resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/permafrost_icon.png");
            ContentAddition.AddBuffDef(freezeBuff);

            fearBuff = ScriptableObject.CreateInstance<BuffDef>();
            fearBuff.buffColor = Color.red;
            fearBuff.canStack = false;
            fearBuff.isDebuff = true;
            fearBuff.name = "CIFear";
            fearBuff.iconSprite = LegacyResourcesAPI.Load<Sprite>("textures/miscicons/texSprintIcon");

            ContentAddition.AddBuffDef(fearBuff);
            IL.EntityStates.AI.Walker.Combat.UpdateAI += IL_ESAIWalkerCombatUpdateAI;

            Logger.LogDebug("Registering item behaviors...");

            foreach(CatalogBoilerplate x in masterItemList) {
                x.SetupBehavior();
            }

            Logger.LogDebug("Initial setup done!");
        }

        private void Start() {
            Logger.LogDebug("Performing late setup:");

            Logger.LogDebug("Late setup for individual items...");
            T2Module.SetupAll_PluginStart(masterItemList);

            Logger.LogDebug("Late setup done!");
        }

        private void IL_ESAIWalkerCombatUpdateAI(ILContext il) {
            ILCursor c = new ILCursor(il);

            int locMoveState = 0;
            bool ILFound = c.TryGotoNext(
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<EntityStates.AI.Walker.Combat>("dominantSkillDriver"),
                x=>x.MatchLdfld<RoR2.CharacterAI.AISkillDriver>("movementType"),
                x=>x.MatchStloc(out locMoveState))
                && c.TryGotoNext(MoveType.After,
                x=>x.MatchCallOrCallvirt<EntityStates.AI.BaseAIState>("get_body"));
            if(ILFound) {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldloc_S, (byte)locMoveState);
                c.EmitDelegate<Func<CharacterBody,RoR2.CharacterAI.AISkillDriver.MovementType,RoR2.CharacterAI.AISkillDriver.MovementType>>((body,origMove) => {
                    if(!body || !body.HasBuff(fearBuff)) return origMove;
                    else return RoR2.CharacterAI.AISkillDriver.MovementType.FleeMoveTarget;
                });
                c.Emit(OpCodes.Stloc_S, (byte)locMoveState);
            } else {
                Logger.LogError("Failed to apply shared buff IL patch (CIFear)");
            }
        }
    }
}
