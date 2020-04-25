using BepInEx;
using MonoMod.Cil;
using R2API;
using R2API.AssetPlus;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using System;

//TODO:
// Add missing documentation in... a whole lotta places... whoops.
// Give the IL patches in Beating Embryo another pass -- those were made before I realized that there are more options in GotoNext than just full matches
// Change H3AD-5T V2 to a green item if removing the stomp effect?
// Add lots of missing items!
// Find a better way to store custom data on character bodies, seems like there should be an ingame solution rather than the current one (CPD lib)... custom-built GameObjects?
// Move CPD setup into item files
// Figure out skill modification/overwrites, for e.g. Ancient Scepter
// Watch for R2API.StatsAPI or similar, for use in some items like Bitter Root, Mysterious Vial, Rusty Jetpack
// Rusty Jetpack: add an IL patch option for jump height modification
// Find out how to safely and instantaneously change money counter, for cases like Life Savings that shouldn't have the sound effects
// Multiplayer testing and solutions
// Actually model the items instead of cheating with sprites (or at least get a better card sprite)
// Engineer turrets spammed errors during FixedUpdate and/or RecalculateStats at one point?? Probably resolved now but keep an eye out for things like this

namespace ThinkInvisible.ClassicItems {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.funkfrog_sipondo.sharesuite",BepInDependency.DependencyFlags.SoftDependency)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI))]
    public class ClassicItemsPlugin:BaseUnityPlugin {
        public const string ModVer =
            #if DEBUG
                "0." +
            #endif
            "2.2.0";
        public const string ModName = "ClassicItems";
        public const string ModGuid = "com.ThinkInvisible.ClassicItems";

        private static ConfigFile cfgFile;
        
        public static class MasterItemList
        {
            public static readonly BoxingGloves boxingGloves = new BoxingGloves();
            public static readonly Brooch brooch = new Brooch();
            public static readonly BitterRoot bitterRoot = new BitterRoot();
            public static readonly Clover clover = new Clover();
            public static readonly Embryo embryo = new Embryo();
            public static readonly GoldenGun goldenGun = new GoldenGun();
            public static readonly Headstompers headstompers = new Headstompers();
            public static readonly LifeSavings lifeSavings = new LifeSavings();
            public static readonly PhotonJetpack photonJetpack = new PhotonJetpack();
            public static readonly RustyJetpack rustyJetpack = new RustyJetpack();
            public static readonly SkeletonKey skeletonKey = new SkeletonKey();
            public static readonly SmartShopper smartShopper = new SmartShopper();
            public static readonly SnakeEyes snakeEyes = new SnakeEyes();
            public static readonly Vial vial = new Vial();
        }

        private readonly List<ItemBoilerplate> MILL = new List<ItemBoilerplate>();
        
        private static ConfigEntry<bool> gCfgHSV2NoStomp;
        private static ConfigEntry<bool> gCfgCoolYourJets;

        public static bool gHSV2NoStomp {get;private set;}
        public static bool gCoolYourJets {get;private set;}

        public ClassicItemsPlugin() {
            #if DEBUG
            Debug.LogWarning("ClassicItems: running test build with debug enabled! If you're seeing this after downloading the mod from Thunderstore, please panic.");
            #endif
            Debug.Log("ClassicItems: loading assets...");
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ClassicItems.classicitems_assets")) {
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@ClassicItems", bundle);
                ResourcesAPI.AddProvider(provider);
            }
            cfgFile = new ConfigFile(Paths.ConfigPath + "\\" + ModGuid + ".cfg", true);
            

            Debug.Log("ClassicItems: loading global configs...");

            gCfgHSV2NoStomp = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "NoHeadStompV2"), true, new ConfigDescription(
                "If true, removes the hold-space-to-stomp functionality of H3AD-5T V2 (due to overlap in functionality with ClassicItems Headstompers). H3AD-5T V2 will still increase jump height and prevent fall damage."));            
            gCfgCoolYourJets = cfgFile.Bind(new ConfigDefinition("Global.Interaction", "CoolYourJets"), true, new ConfigDescription(
                "If true, disables the Rusty Jetpack gravity reduction while Photon Jetpack is active. If false, there shall be yeet."));

            gHSV2NoStomp = gCfgHSV2NoStomp.Value;
            gCoolYourJets = gCfgCoolYourJets.Value;

            Debug.Log("ClassicItems: loading item configs...");

            var mFields = typeof(MasterItemList).GetFields().ToList();
            mFields.ForEach(x=>{
                MILL.Add((ItemBoilerplate)x.GetValue(null));
            });

            MILL.ForEach(x=>{
                x.SetupConfig(cfgFile);
            });

            MILL.RemoveAll(x=>x.itemEnabled==false);
            
            Debug.Log("ClassicItems: registering item attributes...");
            MILL.ForEach(x=>{
                x.SetupAttributes();
                Debug.Log(x.itemCodeName + ": " + (x.itemIsEquipment ? (int)x.regIndexEqp : (int)x.regIndex));
            });
        }

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
        
        internal static Type nodeRefType;
        internal static Type nodeRefTypeArr;

        public void Awake() {
            Debug.Log("ClassicItems: performing plugin setup...");

            nodeRefType = typeof(DirectorCore).GetNestedTypes(System.Reflection.BindingFlags.NonPublic).First(t=>t.Name == "NodeReference");
            nodeRefTypeArr = nodeRefType.MakeArrayType();

            Debug.Log("ClassicItems: tweaking vanilla stuff...");

            //Remove the H3AD-5T V2 state transition from idle to stomp, as Headstompers has similar functionality
            if(gHSV2NoStomp) {
                IL.EntityStates.Headstompers.HeadstompersIdle.FixedUpdate += IL_ESHeadstompersIdleFixedUpdate;
            }

            Debug.Log("ClassicItems: registering item behaviors...");

            MILL.ForEach(x=>{
                x.SetupBehavior();
            });

            Debug.Log("ClassicItems: done!");
        }
        public void IL_ESHeadstompersIdleFixedUpdate(ILContext il) {            
            ILCursor c = new ILCursor(il);
            bool ILFound = c.TryGotoNext(
                x=>x.MatchLdarg(0),
                x=>x.OpCode == OpCodes.Ldfld,
                x=>x.MatchNewobj<EntityStates.Headstompers.HeadstompersCharge>(),
                x=>x.MatchCallOrCallvirt<EntityStateMachine>("SetNextState"));
            if(ILFound) {
                c.RemoveRange(4);
            } else {
                Debug.LogError("ClassicItems: failed to apply vanilla IL patch (HSV2NoStomp)");
            }
        }
    }


}
