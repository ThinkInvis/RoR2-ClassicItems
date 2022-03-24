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

//TODO:
// Add missing documentation in... a whole lotta places... whoops.
// Change H3AD-5T V2 to a green item if removing the stomp effect?
// Add lots of missing items!
// Watch for R2API.StatsAPI or similar, for use in some items like Bitter Root, Mysterious Vial, Rusty Jetpack
// Find out how to safely and instantaneously change money counter, for cases like Life Savings that shouldn't have the sound effects
// Engineer turrets spammed errors during FixedUpdate and/or RecalculateStats at one point?? Probably resolved now but keep an eye out for things like this

namespace ThinkInvisible.ClassicItems {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(TILER2Plugin.ModGuid, TILER2Plugin.ModVer)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(PrefabAPI), nameof(LoadoutAPI), nameof(RecalculateStatsAPI))]
    public class ClassicItemsPlugin:BaseUnityPlugin {
        public const string ModVer =
            #if DEBUG
                "0." +
            #endif
            "6.1.0";
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

#if DEBUG
        public void Update() {
            var i3 = Input.GetKeyDown(KeyCode.F3);
            var i4 = Input.GetKeyDown(KeyCode.F4);
            var i5 = Input.GetKeyDown(KeyCode.F5);
            var i6 = Input.GetKeyDown(KeyCode.F6);
            var i7 = Input.GetKeyDown(KeyCode.F7);
            if (i3 || i4 || i5 || i6 || i7) {
                var trans = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                List<PickupIndex> spawnList;
                if(i3) spawnList = Run.instance.availableTier1DropList;
                else if(i4) spawnList = Run.instance.availableTier2DropList;
                else if(i5) spawnList = Run.instance.availableTier3DropList;
                else if(i6) spawnList = Run.instance.availableEquipmentDropList;
                else spawnList = Run.instance.availableLunarDropList;

                PickupDropletController.CreatePickupDroplet(spawnList[Run.instance.spawnRng.RangeInt(0,spawnList.Count)], trans.position, new Vector3(0f, -5f, 0f));
            }
        }
#endif

        private void Awake() {
            _logger = Logger;

            Logger.LogDebug("Performing plugin setup:");

            #if DEBUG
            Logger.LogWarning("Running test build with debug enabled! If you're seeing this after downloading the mod from Thunderstore, please panic.");
            #endif

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
