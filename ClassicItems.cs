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
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(ResourcesAPI), nameof(PlayerAPI), nameof(PrefabAPI), nameof(BuffAPI), nameof(LoadoutAPI))]
    public class ClassicItemsPlugin:BaseUnityPlugin {
        public const string ModVer =
            #if DEBUG
                "0." +
            #endif
            "4.5.5";
        public const string ModName = "ClassicItems";
        public const string ModGuid = "com.ThinkInvisible.ClassicItems";

        private static ConfigFile cfgFile;
        
        internal static FilingDictionary<ItemBoilerplate_V2> masterItemList = new FilingDictionary<ItemBoilerplate_V2>();
        
        public class GlobalConfig:AutoConfigContainer {
            [AutoConfig("If true, removes the hold-space-to-stomp functionality of H3AD-5T V2 (due to overlap in functionality with ClassicItems Headstompers). H3AD-5T V2 will still increase jump height and prevent fall damage.",
                AutoConfigFlags.PreventNetMismatch)]
            public bool hSV2NoStomp {get;private set;} = false;
            internal bool hSV2Bound = false;

            [AutoConfig("If true, replaces the pickup models for most vanilla items and equipments with trading cards.",
                AutoConfigFlags.DeferForever)]
            public bool allCards {get; private set;} = false;

            [AutoConfig("If true, hides the dynamic description text on trading card-style pickup models. Enabling this may slightly improve performance.",
                AutoConfigFlags.DeferForever)]
            public bool hideDesc {get; private set;} = false;
            
            [AutoConfig("If true, descriptions on trading card-style pickup models will be the (typically longer) description text of the item. If false, pickup text will be used instead.",
                AutoConfigFlags.DeferForever)]
            public bool longDesc {get; private set;} = true;

            [AutoConfig("If true, trading card-style pickup models will have customized spin behavior which makes descriptions more readable. Disabling this may slightly improve compatibility and performance.",
                AutoConfigFlags.DeferForever)]
            public bool spinMod {get; private set;} = true;
            
            [AutoConfig("If true, disables the Rusty Jetpack gravity reduction while Photon Jetpack is active. If false, there shall be yeet.",
                AutoConfigFlags.PreventNetMismatch)]
            public bool coolYourJets {get; private set;} = true;
        }

        public static readonly GlobalConfig globalConfig = new GlobalConfig();

        public static BuffIndex freezeBuff {get;private set;}
        public static BuffIndex fearBuff {get;private set;}

        private static readonly ReadOnlyDictionary<ItemTier, string> modelNameMap = new ReadOnlyDictionary<ItemTier,string>(new Dictionary<ItemTier, string>{
            {ItemTier.Boss, "BossCard"},
            {ItemTier.Lunar, "LunarCard"},
            {ItemTier.Tier1, "CommonCard"},
            {ItemTier.Tier2, "UncommonCard"},
            {ItemTier.Tier3, "RareCard"}
        });

        internal static BepInEx.Logging.ManualLogSource _logger;

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
                var bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider("@ClassicItems", bundle);
                ResourcesAPI.AddProvider(provider);
            }

            cfgFile = new ConfigFile(Path.Combine(Paths.ConfigPath, ModGuid + ".cfg"), true);
            
            Logger.LogDebug("Loading global configs...");

            globalConfig.BindAll(cfgFile, "ClassicItems", "Global");
            globalConfig.ConfigEntryChanged += (sender, args) => {
                if(args.target.boundProperty.Name == nameof(globalConfig.hSV2NoStomp)) {
                    var toBind = (bool)args.newValue == true;
                    if(toBind && !globalConfig.hSV2Bound) {
                        IL.EntityStates.Headstompers.HeadstompersIdle.FixedUpdate += IL_ESHeadstompersIdleFixedUpdate;
                    } else if(!toBind) {
                        IL.EntityStates.Headstompers.HeadstompersIdle.FixedUpdate -= IL_ESHeadstompersIdleFixedUpdate;
                    }
                }
            };

            Logger.LogDebug("Instantiating item classes...");
            masterItemList = T2Module.InitAll<ItemBoilerplate_V2>(new T2Module.ModInfo {
                displayName = "Classic Items",
                longIdentifier = "ClassicItems",
                shortIdentifier = "CI",
                mainConfigFile = cfgFile
            });

            Logger.LogDebug("Loading item configs...");
            foreach(ItemBoilerplate_V2 x in masterItemList) {
                x.SetupConfig();
                x.ConfigEntryChanged += (sender, args) => {
                    if((args.flags & AutoUpdateEventFlags_V2.InvalidateLanguage) == 0) return;
                    if(x.pickupDef != null) {
                        var ctsf = x.pickupDef.displayPrefab?.transform;
                        if(!ctsf) return;
                        var cfront = ctsf.Find("cardfront");
                        if(!cfront) return;

                        cfront.Find("carddesc").GetComponent<TextMeshPro>().text = Language.GetString(globalConfig.longDesc ? x.descToken : x.pickupToken);
                        cfront.Find("cardname").GetComponent<TextMeshPro>().text = Language.GetString(x.nameToken);
                    }
                    if(x.logbookEntry != null) {
                        x.logbookEntry.modelPrefab = x.pickupDef.displayPrefab;
                    }
                };
            }

            Logger.LogDebug("Registering item attributes...");
            
            int longestName = 0;
            foreach(ItemBoilerplate_V2 x in masterItemList) {
                string mpnOvr = null;
                if(x is Item_V2 item) mpnOvr = "@ClassicItems:Assets/ClassicItems/models/" + modelNameMap[item.itemTier] + ".prefab";
                else if(x is Equipment_V2 eqp) mpnOvr = "@ClassicItems:Assets/ClassicItems/models/" + (eqp.isLunar ? "LqpCard.prefab" : "EqpCard.prefab");
                var ipnOvr = "@ClassicItems:Assets/ClassicItems/icons/" + x.name + "_icon.png";

                if(mpnOvr != null) {
                    typeof(ItemBoilerplate_V2).GetProperty(nameof(ItemBoilerplate_V2.modelPathName)).SetValue(x, mpnOvr);
                    typeof(ItemBoilerplate_V2).GetProperty(nameof(ItemBoilerplate_V2.iconPathName)).SetValue(x, ipnOvr);
                }
                
                x.SetupAttributes();
                if(x.name.Length > longestName) longestName = x.name.Length;
            }
            
            Logger.LogMessage("Index dump follows (pairs of name / index):");
            foreach(ItemBoilerplate_V2 x in masterItemList) {
                if(x is Equipment_V2 eqp)
                    Logger.LogMessage("Equipment CI"+x.name.PadRight(longestName) + " / "+((int)eqp.regIndex).ToString());
                else if(x is Item_V2 item)
                    Logger.LogMessage("     Item CI"+x.name.PadRight(longestName) + " / "+((int)item.regIndex).ToString());
                else if(x is Artifact_V2 afct)
                    Logger.LogMessage(" Artifact CI"+x.name.PadRight(longestName) + " / "+((int)afct.regIndex).ToString());
                else
                    Logger.LogMessage("    Other CI"+x.name.PadRight(longestName) + " / N/A");
            }

            Logger.LogDebug("Tweaking vanilla stuff...");

            //Remove the H3AD-5T V2 state transition from idle to stomp, as Headstompers has similar functionality
            if(globalConfig.hSV2NoStomp && !globalConfig.hSV2Bound) {
                globalConfig.hSV2Bound = true;
                IL.EntityStates.Headstompers.HeadstompersIdle.FixedUpdate += IL_ESHeadstompersIdleFixedUpdate;
            }

            On.RoR2.PickupCatalog.Init += On_PickupCatalogInit;
            On.RoR2.UI.LogBook.LogBookController.BuildPickupEntries += On_LogbookBuildPickupEntries;

            if(globalConfig.spinMod)
                IL.RoR2.PickupDisplay.Update += IL_PickupDisplayUpdate;

            Logger.LogDebug("Registering shared buffs...");
            //used only for purposes of Death Mark; applied by Permafrost and Snowglobe
            var freezeBuffDef = new CustomBuff(new BuffDef {
                buffColor = Color.cyan,
                canStack = false,
                isDebuff = true,
                name = "CIFreeze",
                iconPath = "@ClassicItems:Assets/ClassicItems/icons/permafrost_icon.png"
            });
            freezeBuff = BuffAPI.Add(freezeBuffDef);

            var fearBuffDef = new CustomBuff(new BuffDef {
                buffColor = Color.red,
                canStack = false,
                isDebuff = true,
                name = "CIFear",
                iconPath = "textures/miscicons/texSprintIcon"
            });
            fearBuff = BuffAPI.Add(fearBuffDef);
            IL.EntityStates.AI.Walker.Combat.UpdateAI += IL_ESAIWalkerCombatUpdateAI;

            Logger.LogDebug("Registering item behaviors...");

            foreach(ItemBoilerplate_V2 x in masterItemList) {
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

        private void IL_PickupDisplayUpdate(ILContext il) {
            ILCursor c = new ILCursor(il);

            bool ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchLdfld<PickupDisplay>("modelObject"));
            GameObject puo = null;
            if(ILFound) {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate<Action<GameObject>>(x=>{
                    puo=x;
                });
            } else {
                Logger.LogError("Failed to apply vanilla IL patch (pickup model spin modifier)");
                return;
            }

            ILFound = c.TryGotoNext(MoveType.After,
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<PickupDisplay>("spinSpeed"),
                x=>x.MatchLdarg(0),
                x=>x.MatchLdfld<PickupDisplay>("localTime"),
                x=>x.MatchMul());
            if(ILFound) {
                c.EmitDelegate<Func<float,float>>((origAngle) => {
                    if(!puo || !puo.GetComponent<SpinModFlag>() || !NetworkClient.active || PlayerCharacterMasterController.instances.Count == 0) return origAngle;
                    var body = PlayerCharacterMasterController.instances[0].master.GetBody();
                    if(!body) return origAngle;
                    var btsf = body.coreTransform;
                    if(!btsf) btsf = body.transform;
                    return RoR2.Util.QuaternionSafeLookRotation(btsf.position - puo.transform.position).eulerAngles.y
                        + (float)Math.Tanh(((origAngle/100.0f) % 6.2832f - 3.1416f) * 2f) * 180f
                        + 180f
                        - (puo.transform.parent?.eulerAngles.y ?? 0f);
                });
            } else {
                Logger.LogError("Failed to apply vanilla IL patch (pickup model spin modifier)");
            }

        }
        private void IL_ESHeadstompersIdleFixedUpdate(ILContext il) {
            ILCursor c = new ILCursor(il);
            bool ILFound = c.TryGotoNext(
                x=>x.MatchLdarg(0),
                x=>x.OpCode == OpCodes.Ldfld,
                x=>x.MatchNewobj<EntityStates.Headstompers.HeadstompersCharge>(),
                x=>x.MatchCallOrCallvirt<EntityStateMachine>("SetNextState"));
            if(ILFound) {
                c.RemoveRange(4);
            } else {
                Logger.LogError("Failed to apply vanilla IL patch (HSV2NoStomp)");
            }
        }
        private void On_PickupCatalogInit(On.RoR2.PickupCatalog.orig_Init orig) {
            orig();

            Logger.LogDebug("Processing pickup models...");

            if(globalConfig.allCards) {
                var eqpCardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/EqpCard.prefab");
                var lunarCardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/LunarCard.prefab");
                var lunEqpCardPrefab = Resources.Load<GameObject>("@ClassicItems:Assets/ClassicItems/models/VOvr/LqpCard.prefab");
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
                        npfb = eqp.isLunar ? lunEqpCardPrefab : eqpCardPrefab;
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
                    pickup.displayPrefab = npfb;
                }

                Logger.LogDebug("Replaced " + replacedItems + " item models and " + replacedEqps + " equipment models.");
            }

            int replacedDescs = 0;

            var tmpfont = Resources.Load<TMP_FontAsset>("tmpfonts/misc/tmpRiskOfRainFont Bold OutlineSDF");
            var tmpmtl = Resources.Load<Material>("tmpfonts/misc/tmpRiskOfRainFont Bold OutlineSDF");

            foreach(var pickup in PickupCatalog.allPickups) {
                //pattern-match for CI card prefabs
                var ctsf = pickup.displayPrefab?.transform;
                if(!ctsf) continue;

                var cfront = ctsf.Find("cardfront");
                if(cfront == null) continue;
                var croot = cfront.Find("carddesc");
                var cnroot = cfront.Find("cardname");
                var csprite = ctsf.Find("ovrsprite");

                if(croot == null || cnroot == null || csprite == null) continue;
                
                //instantiate and update references
                pickup.displayPrefab = pickup.displayPrefab.InstantiateClone($"CIPickupCardPrefab{pickup.pickupIndex}", false);
                ctsf = pickup.displayPrefab.transform;
                cfront = ctsf.Find("cardfront");
                croot = cfront.Find("carddesc");
                cnroot = cfront.Find("cardname");
                csprite = ctsf.Find("ovrsprite");
                
                csprite.GetComponent<MeshRenderer>().material.mainTexture = pickup.iconTexture;

                if(globalConfig.spinMod)
                    pickup.displayPrefab.AddComponent<SpinModFlag>();

                string pname;
                string pdesc;
                Color prar = new Color(1f, 0f, 1f);
                if(pickup.interactContextToken == "EQUIPMENT_PICKUP_CONTEXT") {
                    var eqp = EquipmentCatalog.GetEquipmentDef(pickup.equipmentIndex);
                    if(eqp == null) continue;
                    pname = Language.GetString(eqp.nameToken);
                    pdesc = Language.GetString(globalConfig.longDesc ? eqp.descriptionToken : eqp.pickupToken);
                    prar = new Color(1f, 0.7f, 0.4f);
                } else if(pickup.interactContextToken == "ITEM_PICKUP_CONTEXT") {
                    var item = ItemCatalog.GetItemDef(pickup.itemIndex);
                    if(item == null) continue;
                    pname = Language.GetString(item.nameToken);
                    pdesc = Language.GetString(globalConfig.longDesc ? item.descriptionToken : item.pickupToken);
                    switch(item.tier) {
                        case ItemTier.Boss: prar = new Color(1f, 1f, 0f); break;
                        case ItemTier.Lunar: prar = new Color(0f, 0.6f, 1f); break;
                        case ItemTier.Tier1: prar = new Color(0.8f, 0.8f, 0.8f); break;
                        case ItemTier.Tier2: prar = new Color(0.2f, 1f, 0.2f); break;
                        case ItemTier.Tier3: prar = new Color(1f, 0.2f, 0.2f); break;
                    }
                } else continue;

                if(globalConfig.hideDesc) {
                    Destroy(croot.gameObject);
                    Destroy(cnroot.gameObject);
                } else {
                    var cdsc = croot.gameObject.AddComponent<TextMeshPro>();
                    cdsc.richText = true;
                    cdsc.enableWordWrapping = true;
                    cdsc.alignment = TextAlignmentOptions.Center;
                    cdsc.margin = new Vector4(4f, 1.874178f, 4f, 1.015695f);
                    cdsc.enableAutoSizing = true;
                    cdsc.overrideColorTags = false;
                    cdsc.fontSizeMin = 1;
                    cdsc.fontSizeMax = 8;
                    _ = cdsc.renderer;
                    cdsc.font = tmpfont;
                    cdsc.material = tmpmtl;
                    cdsc.color = Color.black;
                    cdsc.text = pdesc;

                    var cname = cnroot.gameObject.AddComponent<TextMeshPro>();
                    cname.richText = true;
                    cname.enableWordWrapping = false;
                    cname.alignment = TextAlignmentOptions.Center;
                    cname.margin = new Vector4(6.0f, 1.2f, 6.0f, 1.4f);
                    cname.enableAutoSizing = true;
                    cname.overrideColorTags = true;
                    cname.fontSizeMin = 1;
                    cname.fontSizeMax = 10;
                    _ = cname.renderer;
                    cname.font = tmpfont;
                    cname.material = tmpmtl;
                    cname.outlineColor = prar;
                    cname.outlineWidth = 0.15f;
                    cname.color = Color.black;
                    cname.fontStyle = FontStyles.Bold;
                    cname.text = pname;
                }
                replacedDescs ++;
            }
            Logger.LogDebug((globalConfig.hideDesc ? "Destroyed " : "Inserted ") + replacedDescs + " pickup model descriptions.");
        }

        private RoR2.UI.LogBook.Entry[] On_LogbookBuildPickupEntries(On.RoR2.UI.LogBook.LogBookController.orig_BuildPickupEntries orig) {
            var retv = orig();
            Logger.LogDebug("Processing logbook models...");
            int replacedModels = 0;
            foreach(RoR2.UI.LogBook.Entry e in retv) {
                if(!(e.extraData is PickupIndex)) continue;
                if(e.modelPrefab == null) continue;
                if(e.modelPrefab.transform.Find("cardfront")) {
                    e.modelPrefab = PickupCatalog.GetPickupDef((PickupIndex)e.extraData).displayPrefab;
                    replacedModels++;
                }
            }
            Logger.LogDebug("Modified " + replacedModels + " logbook models.");
            return retv;
        }
    }

    public class SpinModFlag : MonoBehaviour {}
}
