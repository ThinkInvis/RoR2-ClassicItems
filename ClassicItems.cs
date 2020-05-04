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
// Change H3AD-5T V2 to a green item if removing the stomp effect?
// Add lots of missing items!
// Figure out skill modification/overwrites, for e.g. Ancient Scepter
// Watch for R2API.StatsAPI or similar, for use in some items like Bitter Root, Mysterious Vial, Rusty Jetpack
// Find out how to safely and instantaneously change money counter, for cases like Life Savings that shouldn't have the sound effects
// Engineer turrets spammed errors during FixedUpdate and/or RecalculateStats at one point?? Probably resolved now but keep an eye out for things like this

namespace ThinkInvisible.ClassicItems {
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.funkfrog_sipondo.sharesuite",BepInDependency.DependencyFlags.SoftDependency)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI), nameof(BuffAPI), nameof(CommandHelper))]
    public class ClassicItemsPlugin:BaseUnityPlugin {
        public const string ModVer =
            #if DEBUG
                "0." +
            #endif
            "3.0.1";
        public const string ModName = "ClassicItems";
        public const string ModGuid = "com.ThinkInvisible.ClassicItems";

        private static ConfigFile cfgFile;
        
        public static MiscUtil.FilingDictionary<ItemBoilerplate> masterItemList = new MiscUtil.FilingDictionary<ItemBoilerplate>();
        
        private static ConfigEntry<bool> gCfgHSV2NoStomp;
        private static ConfigEntry<bool> gCfgAllCards;
        private static ConfigEntry<bool> gCfgCoolYourJets;

        public static BuffIndex freezeBuff {get;private set;}

        public static bool gHSV2NoStomp {get;private set;}
        public static bool gAllCards {get;private set;}
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
            gCfgAllCards = cfgFile.Bind(new ConfigDefinition("Global.VanillaTweaks", "AllCards"), false, new ConfigDescription(
                "If true, replaces the pickup models for most vanilla items and equipments with trading cards."));            
            gCfgCoolYourJets = cfgFile.Bind(new ConfigDefinition("Global.Interaction", "CoolYourJets"), true, new ConfigDescription(
                "If true, disables the Rusty Jetpack gravity reduction while Photon Jetpack is active. If false, there shall be yeet."));

            gHSV2NoStomp = gCfgHSV2NoStomp.Value;
            gAllCards = gCfgAllCards.Value;
            gCoolYourJets = gCfgCoolYourJets.Value;

            Debug.Log("ClassicItems: instantiating item classes...");

            foreach(Type type in Assembly.GetAssembly(typeof(ItemBoilerplate)).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ItemBoilerplate)))) {
                masterItemList.Add((ItemBoilerplate)Activator.CreateInstance(type));
            }

            Debug.Log("ClassicItems: loading item configs...");

            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupConfig(cfgFile);
            }

            masterItemList.RemoveWhere(x=>x.itemEnabled==false);

            Debug.Log("ClassicItems: registering item attributes...");

            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupAttributes();
                Debug.Log("CI"+x.itemCodeName + ": " + (x.itemIsEquipment ? ("EQP"+((int)x.regIndexEqp).ToString()) : ((int)x.regIndex).ToString()));
            }
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
            
            CommandHelper.AddToConsoleWhenReady();
            nodeRefType = typeof(DirectorCore).GetNestedTypes(BindingFlags.NonPublic).First(t=>t.Name == "NodeReference");
            nodeRefTypeArr = nodeRefType.MakeArrayType();

            Debug.Log("ClassicItems: tweaking vanilla stuff...");

            //Remove the H3AD-5T V2 state transition from idle to stomp, as Headstompers has similar functionality
            if(gHSV2NoStomp) {
                IL.EntityStates.Headstompers.HeadstompersIdle.FixedUpdate += IL_ESHeadstompersIdleFixedUpdate;
            }
            if(gAllCards) {
                On.RoR2.PickupCatalog.Init += On_PickupCatalogInit;
            }

            Debug.Log("ClassicItems: registering shared buffs...");
            //used only for purposes of Death Mark; applied by Permafrost and Snowglobe
            var freezeBuffDef = new CustomBuff(new BuffDef {
                buffColor = Color.cyan,
                canStack = false,
                isDebuff = true,
                name = "CIFreeze",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/permafrost_icon.png"
            });
            freezeBuff = BuffAPI.Add(freezeBuffDef);

            Debug.Log("ClassicItems: registering item behaviors...");


            foreach(ItemBoilerplate x in masterItemList) {
                x.SetupBehavior();
            }

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
        public void On_PickupCatalogInit(On.RoR2.PickupCatalog.orig_Init orig) {
            orig();

            Debug.Log("ClassicItems: replacing pickup models...");

            var eqpCardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/EqpCard.prefab");
            var lunarCardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/LunarCard.prefab");
            var t1CardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/CommonCard.prefab");
            var t2CardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/UncommonCard.prefab");
            var t3CardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/RareCard.prefab");
            var bossCardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/BossCard.prefab");

            int replacedItems = 0;
            int replacedEqps = 0;

            foreach(var pickup in PickupCatalog.allPickups) {
                GameObject npfb;
                if(pickup.interactContextToken == "EQUIPMENT_PICKUP_CONTEXT") {
                    if(pickup.equipmentIndex >= EquipmentIndex.Count || pickup.equipmentIndex < 0) continue;
                    var eqp = EquipmentCatalog.GetEquipmentDef(pickup.equipmentIndex);
                    if(!eqp.canDrop) continue;
                    npfb = eqpCardPrefab;
                    replacedEqps ++;
                } else if(pickup.interactContextToken == "ITEM_PICKUP_CONTEXT") {
                    if(pickup.itemIndex >= ItemIndex.Count || pickup.itemIndex < 0) continue;
                    var item = ItemCatalog.GetItemDef(pickup.itemIndex);
                    switch(item.tier) {
                        case ItemTier.Tier1:
                            npfb = t1CardPrefab; break;
                        case ItemTier.Tier2:
                            npfb = t2CardPrefab; break;
                        case ItemTier.Tier3:
                            npfb = t3CardPrefab; break;
                        case ItemTier.Lunar:
                            npfb = lunarCardPrefab; break;
                        case ItemTier.Boss:
                            npfb = bossCardPrefab; break;
                        default:
                            continue;
                    }
                    replacedItems ++;
                } else continue;
                pickup.displayPrefab = npfb.InstantiateClone(pickup.nameToken + "CICardPrefab", false);
                pickup.displayPrefab.transform.Find("ovrsprite").GetComponent<MeshRenderer>().material.mainTexture = pickup.iconTexture;
            }

            Debug.Log("ClassicItems: replaced " + replacedItems + " item models and " + replacedEqps + " equipment models.");
        }
    }


}
