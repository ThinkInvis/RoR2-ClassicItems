using RoR2;
using System.Collections.ObjectModel;
using TILER2;
using static TILER2.MiscUtil;
using static TILER2.SkillUtil;
using RoR2.Skills;
using System.Collections.Generic;
using R2API;
using System.Linq;
using System;
using UnityEngine;

namespace ThinkInvisible.ClassicItems {
    public abstract class ScepterSkill {
        public abstract SkillDef myDef {get; protected set;}
        public abstract string oldDescToken {get; protected set;}
        public abstract string newDescToken {get; protected set;}
        public abstract string overrideStr {get;}
        internal abstract void SetupAttributes();
        internal virtual void LoadBehavior() { }
        internal virtual void UnloadBehavior() { }
        public abstract string targetBody {get;}
        public abstract SkillSlot targetSlot {get;}
        public abstract int targetVariantIndex {get;}
    }

    public class Scepter : Item<Scepter> {
        public override string displayName => "Ancient Scepter";
		public override ItemTier itemTier => ItemTier.Tier3;
		public override ReadOnlyCollection<ItemTag> itemTags => new ReadOnlyCollection<ItemTag>(new[]{ItemTag.Any});
        public override bool itemIsAIBlacklisted {get; protected set;} = true;

        protected override string GetNameString(string langid = null) => displayName;
        protected override string GetPickupString(string langid = null) => "Upgrades one of your skills.";
        protected override string GetDescString(string langid = null) => "While held, one of your selected character's <style=cIsUtility>skills</style> <style=cStack>(unique per character)</style> becomes a <style=cIsUtility>more powerful version</style>."
            + $" <style=cStack>{(rerollExtras ? "Extra/unusable" : "Unusable (but NOT extra)")} pickups will reroll into other red items.</style>";
        protected override string GetLoreString(string langid = null) => "A relic of times long past (ClassicItems mod)";
        
        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, TR12-C Gauss Compact will recharge faster to match the additional stock.")]
        public bool engiTurretAdjustCooldown {get; private set;} = false;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, TR58-C Carbonizer Mini will recharge faster to match the additional stock.")]
        public bool engiWalkerAdjustCooldown {get; private set;} = false;
        
        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, any stacks picked up past the first will reroll to other red items. If false, this behavior will only be used for characters which cannot benefit from the item at all.")]
        public bool rerollExtras {get; private set;} = true;
        
        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, Dragon's Breath will use significantly lighter particle effects and no dynamic lighting.", AutoConfigFlags.DeferForever)]
        public bool artiFlamePerformanceMode {get; private set;} = false;

        [AutoConfigRoOCheckbox()]
        [AutoConfig("If true, other mods will be able to override default Ancient Scepter skills with their own.", AutoConfigFlags.DeferForever)]
        public bool allow3POverride { get; private set; } = true;

        //TODO: test w/ stage changes
        public enum StridesInteractionMode {
            StridesTakesPrecedence, ScepterTakesPrecedence, ScepterRerolls
        }
        [AutoConfigRoOChoice()]
        [AutoConfig("Changes what happens when a character whose Utility skill is affected by Ancient Scepter has both Ancient Scepter and Strides of Heresy at the same time.",
            AutoConfigFlags.DeferUntilNextStage | AutoConfigFlags.PreventNetMismatch)]
        public StridesInteractionMode stridesInteractionMode {get; private set;} = StridesInteractionMode.ScepterRerolls;

        internal List<ScepterSkill> skills = new List<ScepterSkill>();

        public Scepter() {
            iconResource = ClassicItemsPlugin.resources.LoadAsset<Sprite>("Assets/ClassicItems/Textures/ClassicIcons/scepter_icon.png");
            modelResource = ClassicItemsPlugin.resources.LoadAsset<GameObject>("Assets/ClassicItems/Prefabs/Scepter.prefab");

            skills.Add(new ArtificerFlamethrower2());
            skills.Add(new ArtificerFlyUp2());
            skills.Add(new CaptainAirstrike2());
            //skills.Add(new CaptainAirstrikeAlt2());
            skills.Add(new CommandoBarrage2());
            skills.Add(new CommandoGrenade2());
            skills.Add(new CrocoDisease2());
            skills.Add(new EngiTurret2());
            skills.Add(new EngiWalker2());
            skills.Add(new HuntressBallista2());
            skills.Add(new HuntressRain2());
            skills.Add(new LoaderChargeFist2());
            skills.Add(new LoaderChargeZapFist2());
            skills.Add(new MercEvis2());
            skills.Add(new MercEvisProjectile2());
            skills.Add(new ToolbotDash2());
            skills.Add(new TreebotFlower2_2());
        }

        public override void SetupConfig() {
            base.SetupConfig();

            ConfigEntryChanged += (sender, args) => {
                switch(args.target.boundProperty.Name) {
                    case nameof(engiTurretAdjustCooldown):
                        var engiSkill = skills.First(x => x is EngiTurret2);
                        engiSkill.myDef.baseRechargeInterval = EngiTurret2.oldDef.baseRechargeInterval * (((bool)args.newValue) ? 2f / 3f : 1f);
                        GlobalUpdateSkillDef(engiSkill.myDef);
                        break;
                    case nameof(engiWalkerAdjustCooldown):
                        var engiSkill2 = skills.First(x => x is EngiWalker2);
                        engiSkill2.myDef.baseRechargeInterval = EngiWalker2.oldDef.baseRechargeInterval / (((bool)args.newValue) ? 2f : 1f);
                        GlobalUpdateSkillDef(engiSkill2.myDef);
                        break;
                    default:
                        break;
                }
            };
        }

        public override void SetupAttributes() {
            base.SetupAttributes();

            foreach(var skill in skills) {
                skill.SetupAttributes();
                RegisterScepterSkill(skill.myDef, skill.targetBody, skill.targetSlot, skill.targetVariantIndex);
            }
        }

        public override void SetupBehavior() {
            base.SetupBehavior();

            FakeInventory.blacklist.Add(itemDef);
        }

        public override void Install() {
            base.Install();
            On.RoR2.CharacterBody.OnInventoryChanged += On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += On_CMGetDeployableSameSlotLimit;
            On.RoR2.GenericSkill.SetSkillOverride += On_GSSetSkillOverride;

            foreach(var skill in skills) {
                skill.LoadBehavior();
            }

            foreach(var cm in AliveList()) {
                if(!cm.hasBody) continue;
                var body = cm.GetBody();
                HandleScepterSkill(body);
            }
        }

        public override void Uninstall() {
            base.Uninstall();
            On.RoR2.CharacterBody.OnInventoryChanged -= On_CBOnInventoryChanged;
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit -= On_CMGetDeployableSameSlotLimit;
            On.RoR2.GenericSkill.SetSkillOverride -= On_GSSetSkillOverride;
            
            foreach(var cm in AliveList()) {
                if(!cm.hasBody) continue;
                var body = cm.GetBody();
                HandleScepterSkill(body, true);
            }
            
            foreach(var skill in skills) {
                skill.UnloadBehavior();
            }
        }

        public override void InstallLanguage() {
            base.InstallLanguage();
            foreach(var skill in skills) {
                if(skill.oldDescToken == null) {
                    ClassicItemsPlugin._logger.LogError(skill.GetType().Name + " oldDescToken is null!");
                    continue;
                }
                languageOverlays.Add(LanguageAPI.AddOverlay(skill.newDescToken, Language.GetString(skill.oldDescToken) + skill.overrideStr, Language.currentLanguageName));
            }
        }

        bool handlingOverride = false;
        private void On_GSSetSkillOverride(On.RoR2.GenericSkill.orig_SetSkillOverride orig, GenericSkill self, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority) {
            if(stridesInteractionMode != StridesInteractionMode.ScepterTakesPrecedence
                || skillDef.skillIndex != CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef.skillIndex
                || !(source is CharacterBody body)
                || body.inventory.GetItemCount(catalogIndex) < 1
                || handlingOverride)
                orig(self, source, skillDef, priority);
            else {
                handlingOverride = true;
                HandleScepterSkill(body);
                handlingOverride = false;
            }
        }

        private int On_CMGetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot) {
            var retv = orig(self, slot);
            if(slot != DeployableSlot.EngiTurret) return retv;
            var sp = self.GetBody()?.skillLocator?.special;
            if(!sp) return retv;
            if(sp.skillDef == skills.First(x => x is EngiTurret2).myDef)
                return retv + 1;
            if(sp.skillDef == skills.First(x => x is EngiWalker2).myDef)
                return retv + 2;
            return retv;
        }

        private class ScepterReplacer {
            public string bodyName;
            public SkillSlot slotIndex;
            public int variantIndex;
            public SkillDef replDef;
        }

        private readonly List<ScepterReplacer> scepterReplacers = new List<ScepterReplacer>();
        private readonly Dictionary<string, SkillSlot> scepterSlots = new Dictionary<string, SkillSlot>();

        public bool RegisterScepterSkill(SkillDef replacingDef, string targetBodyName, SkillSlot targetSlot, int targetVariant) {
            if(replacingDef == null) {
                ClassicItemsPlugin._logger.LogError("Can't register a null scepter skill");
                return false;
            }
            if(targetVariant < 0) {
                ClassicItemsPlugin._logger.LogError($"Can't register scepter skill {replacingDef.skillNameToken} to negative variant index");
                return false;
            }
            var existing = scepterReplacers.Find(x => x.bodyName == targetBodyName && (x.slotIndex != targetSlot || x.variantIndex == targetVariant));
            if(existing != null) {
                if(allow3POverride) {
                    ClassicItemsPlugin._logger.LogDebug($"{targetBodyName}/{targetSlot}/{targetVariant}: overriding existing replacer {existing.replDef.skillNameToken} with {replacingDef.skillNameToken}");
                    scepterReplacers.Remove(existing);
                } else {
                    ClassicItemsPlugin._logger.LogError($"A scepter skill already exists for {targetBodyName}/{targetSlot}/{targetVariant}; can't add multiple for different slots nor for the same variant (attemping to add {replacingDef.skillNameToken})");
                    return false;
                }
            }
            scepterReplacers.Add(new ScepterReplacer {bodyName = targetBodyName, slotIndex = targetSlot, variantIndex = targetVariant, replDef = replacingDef});
            scepterSlots[targetBodyName] = targetSlot;
            return true;
        }

        bool handlingInventory = false;
        private void On_CBOnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self) {
            orig(self);
            if(handlingInventory) return;
            handlingInventory = true;
            if(!HandleScepterSkill(self)) {
                if(GetCount(self) > 0) {
                    Reroll(self, GetCount(self));
                }
            } else if(GetCount(self) > 1 && rerollExtras) {
                 Reroll(self, GetCount(self) - 1);
            }
            handlingInventory = false;
        }

        private void Reroll(CharacterBody self, int count) {
            if(count <= 0 || self.master?.GetComponent<Deployable>()) return;
            var list = Run.instance.availableTier3DropList.Except(new[] {pickupIndex}).ToList();
            for(var i = 0; i < count; i++) {
                self.inventory.RemoveItem(catalogIndex, 1);
                self.inventory.GiveItem(PickupCatalog.GetPickupDef(list[UnityEngine.Random.Range(0, list.Count)]).itemIndex);
            }
        }
        
        private bool HandleScepterSkill(CharacterBody self, bool forceOff = false) {
            bool hasStrides = self.inventory.GetItemCount(RoR2Content.Items.LunarUtilityReplacement) > 0;
            if(self.skillLocator && self.master?.loadout != null) {
                var bodyName = BodyCatalog.GetBodyName(self.bodyIndex);

                var repl = scepterReplacers.FindAll(x => x.bodyName == bodyName);
                if(repl.Count > 0) {
                    SkillSlot targetSlot = scepterSlots[bodyName];
                    if(targetSlot == SkillSlot.Utility && stridesInteractionMode == StridesInteractionMode.ScepterRerolls && hasStrides) return false;
                    var targetSkill = self.skillLocator.GetSkill(targetSlot);
                    if(!targetSkill) return false;
                    var targetSlotIndex = self.skillLocator.GetSkillSlotIndex(targetSkill);
                    var targetVariant = self.master.loadout.bodyLoadoutManager.GetSkillVariant(self.bodyIndex, targetSlotIndex);
                    var replVar = repl.Find(x => x.variantIndex == targetVariant);
                    if(replVar == null) return false;
                    if(!forceOff && GetCount(self) > 0) {
                        if(stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasStrides) {
                            self.skillLocator.utility.UnsetSkillOverride(self, CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                        }
                        targetSkill.SetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                    } else {
                        targetSkill.UnsetSkillOverride(self, replVar.replDef, GenericSkill.SkillOverridePriority.Upgrade);
                        if(stridesInteractionMode == StridesInteractionMode.ScepterTakesPrecedence && hasStrides) {
                            self.skillLocator.utility.SetSkillOverride(self, CharacterBody.CommonAssets.lunarUtilityReplacementSkillDef, GenericSkill.SkillOverridePriority.Replacement);
                        }
                    }

                    return true;
                }
            }
            return false;
        }
    }
}
